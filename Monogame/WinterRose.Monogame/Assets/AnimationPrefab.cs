using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.Monogame.Animations
{
    /// <summary>
    /// A prefab for an <see cref="Animation"/>.
    /// </summary>
    public class AnimationPrefab : Prefab
    {
        private Animation? result;

        /// <summary>
        /// Creates a new <see cref="AnimationPrefab"/> with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public AnimationPrefab(string name) : base(name)
        {
        }
        /// <summary>
        /// Creates a new <see cref="AnimationPrefab"/> with the specified <see cref="Animation"/>.
        /// </summary>
        /// <param name="result"></param>
        public AnimationPrefab(Animation result) : this(result.Name)
        {
            this.result = result;
        }

        /// <summary>
        /// Loads the <see cref="Animation"/> from the file.
        /// </summary>
        public override void Load()
        {
            result = WinterForge.DeserializeFromFile<Animation>(File.File.FullName);

            foreach(var key in result.Keys)
            {
                key.Animation = result;
            }
        }

        /// <summary>
        /// Saves the <see cref="Animation"/> to the file.
        /// </summary>
        public override void Save()
        {
            WinterForge.SerializeToFile(result, File.File.FullName);
        }

        /// <summary>
        /// Unloads the <see cref="AnimationPrefab"/>. The animation instance is not affected.
        /// </summary>
        public override void Unload()
        {
            result = null;
        }

        public static implicit operator Animation(AnimationPrefab prefab) => prefab.result!;
        public static implicit operator AnimationPrefab(Animation anim) => new(anim);
    }
}
