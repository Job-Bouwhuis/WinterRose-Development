using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.Monogame.Animations
{
    /// <summary>
    /// Provides a way to animate a <see cref="WorldObject"/> using <see cref="AnimationKey"/>s.
    /// </summary>
    public sealed class Animation
    {
        /// <summary>
        /// The name of the animation.
        /// </summary>
        [IncludeWithSerialization]
        public string Name { get; set; }
        /// <summary>
        /// The length of the animation in seconds.
        /// </summary>
        [IncludeWithSerialization]
        public float AnimationLength { get; set; } = 1f;
        public float AnimationTime { get; private set; }
        /// <summary>
        /// Whether or not the animation is paused.
        /// </summary>
        [IncludeWithSerialization]
        public bool Paused { get; set; }
        /// <summary>
        /// The current key index.
        /// </summary>
        [IncludeWithSerialization]
        public int CurrentKeyIndex { get; set; } = 0;
        /// <summary>
        /// Whether or not the animation should loop when it reaches the end.
        /// </summary>
        [IncludeWithSerialization]
        public bool Loop { get; set; } = false;
        /// <summary>
        /// The keys that make up the animation.
        /// </summary>
        [IncludeWithSerialization]
        public List<AnimationKey> Keys { get; private set; } = new();
        bool newKey = true;

        /// <summary>
        /// Adds the specified key to the animation.
        /// </summary>
        /// <param name="key"></param>
        public void AddKey(AnimationKey key)
        {
            key.Animation = this;
            Keys.Add(key);
        }
        /// <summary>
        /// Removes the specified key from the animation.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveKey(AnimationKey key)
        {
            Keys.Remove(key);
            key.Animation = null;
        }
        /// <summary>
        /// Clears all keys from the animation.
        /// </summary>
        public void Clear() => Keys.Clear();

        /// <summary>
        /// Steps the animation forward one tick.
        /// </summary>
        /// <returns>if paused 2, if at the end and not looping 1, else 0</returns>
        public int Step()
        {
            if (Paused)
                return 2;

            AnimationTime += Time.deltaTime;

            if (CurrentKeyIndex >= Keys.Count)
                if (Loop)
                    CurrentKeyIndex = 0;
                else return 1;

            AnimationKey key = Keys[CurrentKeyIndex];

            if(newKey)
            {
                key.StartKey();
                newKey = false;
            }

            if (key.ValidateTarget())
            {
                key.KeyStep();
                if (key.EvaluateEnd())
                {
                    key.KeyEnd();
                    CurrentKeyIndex++;
                    AnimationTime = 0;
                    newKey = true;
                }
            }
            return 0;
        }

        /// <summary>
        /// Sets up the animation to use the specified <see cref="WorldObject"/>.
        /// </summary>
        /// <param name="owner"></param>
        public void Setup(WorldObject owner)
        {
            foreach (var key in Keys)
            {
                key.Setup(owner);
            }
        }
    }
}
