using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Particles;

namespace WinterRose.Monogame;

public sealed class ParticleEmitter : ActiveRenderer
{
    private readonly Random random = new();
    private List<Particle> particles;

    private Sprite sprite;

    public float time;

    /// <summary>
    /// The number of particles to emit per second.
    /// </summary>
    public float EmissionRate { get; set; }
    /// <summary>
    /// The range of the particles' lifetime.
    /// </summary>
    public RangeF ParticleLifeTime { get; set; }
    /// <summary>
    /// The range of the particles' speed.
    /// </summary>
    public ValueRange ParticleSpeed { get; set; }
    /// <summary>
    /// The color of the partices over their lifetime.
    /// </summary>
    public ColorRange ParticleColor { get; set; }
    /// <summary>
    /// The range of the particles' size.
    /// </summary>
    public ValueRange ParticleSize { get; set; }
    /// <summary>
    /// If true, the particle emitter will remove the oldest particles when it reaches the max particle count.<br></br>
    /// If false, the particle emitter will stop emitting particles when it reaches the max particle count. and resume when particles reached the end of their lifetime.
    /// </summary>
    public bool ForceRemoveOnMaxParticles { get; set; } = false;
    /// <summary>
    /// The maximum number of particles that can be alive at once.
    /// </summary>
    public int MaxParticles { get; set; }
    /// <summary>
    /// The min-max range of the particles in the X direction.
    /// </summary>
    public Vector2 DirectionXBounds { get; set; } = new Vector2(-1, 1);
    /// <summary>
    /// The min-max range of the particles in the Y direction.
    /// </summary>
    public Vector2 DirectionYBounds { get; set; } = new Vector2(-1, 1);

    /// <summary>
    /// The sprite of the particle emitter.
    /// </summary>
    public Sprite Sprite
    {
        get => sprite;
        set => sprite = value;
    }
    /// <summary>
    /// The bounds of the particle emitter.
    /// </summary>
    [Experimental("WRM_NotImplemented")]
    public override RectangleF Bounds => RectangleF.Zero;

    /// <summary>
    /// Automatically emit particle every frame.
    /// </summary>
    public bool AutoEmit { get; set; } = true;

    public override TimeSpan DrawTime { get; protected set; } = new();

    /// <summary>
    /// Creates a new particle emitter with the given sprite and default values.
    /// </summary>
    /// <param name="sprite"></param>
    public ParticleEmitter(Sprite sprite)
    {
        this.sprite = sprite;
        particles = new();
        EmissionRate = 100;
        ParticleLifeTime = (1, 2);
        ParticleSpeed = new([new(0, 1), new(1, 1)]);
        // default color spectrum is white to transparent.
        ParticleColor = new([(Color.White, 0), (Color.Transparent, 1)]);
        // default size spectrum is 0, to 1, to 0. 
        ParticleSize = new ValueRange([(0, 0.5f), (0.5f, 1), (1, 0)]);
        MaxParticles = 10_000;
    }
    /// <summary>
    /// Creates a new empty particle emitter with default values.
    /// </summary>
    public ParticleEmitter()
    {
        particles = new();
        EmissionRate = 100;
        ParticleLifeTime = (1, 2);
        ParticleSpeed = new([new(0, 1), new(1, 1)]);
        ParticleColor = new([new(Color.White, 0), new(Color.Transparent, 1)]);
        ParticleSize = new ValueRange([(0, 0.5f), (0.5f, 1), (1, 0)]);
        MaxParticles = 10_000;
    }
    /// <summary>
    /// Creates a new particle emitter with the given sprite and values.
    /// </summary>
    public ParticleEmitter(Sprite sprite, float emissionRate, RangeF particleLifeTime, ValueRange particleSpeed, ValueRange particleSize, ColorRange particleColor, int maxparticles = 10_000)
    {
        this.sprite = sprite;
        particles = new();
        EmissionRate = emissionRate;
        ParticleLifeTime = particleLifeTime;
        ParticleSpeed = particleSpeed;
        ParticleColor = particleColor;
        ParticleSize = particleSize;
        MaxParticles = maxparticles;
    }
    /// <summary>
    /// Creates a new particle emitter with the given sprite and values.
    /// </summary>
    public ParticleEmitter(Sprite sprite, float emissionRate, (float, float) particleLifeTime, ValueRange particleSpeed, ValueRange particleSize, ColorRange particleColor, int maxparticles = 10_000)
    {
        this.sprite = sprite;
        particles = new();
        EmissionRate = emissionRate;
        ParticleLifeTime = particleLifeTime;
        ParticleSpeed = particleSpeed;
        ParticleColor = particleColor;
        ParticleSize = particleSize;
        MaxParticles = maxparticles;
    }

    protected override void Update()
    {
        Debug.Log($"Total particles: {particles.Count}");
        if (AutoEmit)
            EmitParticles();
        RemoveDeadParticles();

        for (int i = 0; i < particles.Count; i++)
        {
            particles[i].Update(Time.deltaTime);
        }
    }
    private void RemoveDeadParticles()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            if (!particles[i].IsAlive)
            {
                particles.RemoveAt(i);
                i--;
            }
        }
    }
    private void EmitParticles()
    {
        time += Time.deltaTime;

        float timeBetweenParticles = 1f / EmissionRate;

        while (time > timeBetweenParticles)
        {
            EmitOne();
            time -= timeBetweenParticles;
        }
    }

    public override void Render(SpriteBatch batch)
    {
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < particles.Count; i++)
            particles[i].Render(batch, sprite);

        sw.Stop();
        DrawTime = sw.Elapsed;
    }

    /// <summary>
    /// Emits the given amount of particles
    /// </summary>
    /// <param name="particleAmountToEmit"></param>
    public void Emit(int particleAmountToEmit)
    {
        int emitted = 0;
        while (emitted < particleAmountToEmit)
        {
            EmitOne();
            emitted++;
        }
    }

    private void EmitOne()
    {
        if (particles.Count >= MaxParticles)
            if (ForceRemoveOnMaxParticles)
                particles.RemoveAt(0);
            else
                return;

        var norm = Vector2.Normalize(random.NextVector2(DirectionXBounds, DirectionYBounds));
        //var abs = Direction.Abs();

        Particle p = new()
        {
            direction = norm,
            speed = ParticleSpeed,
            color = ParticleColor,
            scale = ParticleSize,
            rotation = random.NextFloat(0, MathF.PI * 2),
            angularVelocity = random.NextFloat(-MathF.PI, MathF.PI),
            position = transform.position,
            lifeTime = random.NextFloat(ParticleLifeTime.Start, ParticleLifeTime.End)
        };

        Debug.Log(p.angularVelocity);
        particles.Add(p);
    }
}
