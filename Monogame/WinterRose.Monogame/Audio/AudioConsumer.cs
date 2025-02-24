using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Audio;

/// <summary>
/// An audio listener for directional audio. Only one can exist at a time.
/// </summary>
public class AudioConsumer : ObjectBehavior
{
    /// <summary>
    /// The current audio listener
    /// </summary>
    public static AudioConsumer Listener
    {
        get
        {
            if(instance is null)
                throw new InvalidOperationException("No AudioListener component in your scene. Please add one to any object of your choosing");
            return instance;
        }
    }

    /// <summary>
    /// Whether an audio listener exists in the world
    /// </summary>
    public static bool Exists => instance is not null;

    private static AudioConsumer? instance;

    private readonly AudioListener listener = new();

    public AudioConsumer()
    {
        if(instance is not null)
            throw new Exception("Only one AudioListener can exist at a time");
        instance = this;
    }

    protected override void Update()
    {
        listener.Up = transform.up.Vector3();
        listener.Position = transform.position.Vector3();
    }

    internal static void Reset() => instance = null;

    public static implicit operator AudioListener(AudioConsumer listener) => listener.listener;

    protected override void Close()
    {
        instance = null;
    }
}
