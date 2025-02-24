using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

namespace WinterRose.Monogame.Audio;

/// <summary>
/// A component that plays audio. Requires an <see cref="AudioConsumer"/> in the world  to work. if there is no <see cref="AudioConsumer"/> in the world, the sound will not play
/// </summary>
public class AudioSource : ObjectBehavior
{
    [Show]
    private AudioEmitter emitter;
    private SoundEffectInstance soundInstance;

    /// <summary>
    /// The maximum distance the sound can be heard from
    /// </summary>
    public float MaxRadius { get; private set; } = 1000;
    /// <summary>
    /// The minimum distance the sound can be heard from at max volume. the volume will gradually decrease from this point to <see cref="MaxRadius"/> until the sound is no longer audible
    /// </summary>
    public float MinRadius { get; private set; } = 100;

    public AudioSource(SoundEffect soundInstance)
    {
        this.soundInstance = soundInstance.CreateInstance();

        emitter = new AudioEmitter();
        Debug.Log(emitter.DopplerScale, true);
    }

    protected override void Update()
    {
        emitter.Position = transform.position.Vector3();
        emitter.Up = transform.up.Vector3();
    }

    /// <summary>
    /// Plays the sound
    /// </summary>
    public void Play()
    {
        if (!AudioConsumer.Exists)
        {
            Debug.LogWarning("No AudioListener in the scene. Please add one to any object you wish");
            return;
        }

        soundInstance.Apply3D(AudioConsumer.Listener, emitter);
        ApplyDistancing();
        soundInstance.Play();
    }

    private void ApplyDistancing()
    {
        var listener = AudioConsumer.Listener;
        float distance = Vector2.Distance(transform.position, listener.transform.position);
        float pan = 0f;
        float volume = 1f;

        if (distance <= MaxRadius)
        {
            if (distance > MinRadius)
            {
                float ratio = (distance / MaxRadius);
                volume = 1 - ratio;

                var differenceX = (transform.position - listener.transform.position).X;

                if (differenceX != 0)
                {
                    var differenceAbs = MathF.Abs(differenceX);

                    if (differenceAbs >= MinRadius / 2)
                        pan = ratio * (differenceX / differenceAbs);
                }

            }
        }
        else volume = 0f;

        soundInstance.Volume = volume;
        soundInstance.Pan = pan;

        Console.WriteLine($"Volume: {soundInstance.Volume}, Pan: {soundInstance.Pan}");
    }
}
