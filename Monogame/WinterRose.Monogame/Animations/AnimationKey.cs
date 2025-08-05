using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Animations
{
    /// <summary>
    /// Abstract class for animation keys used in <see cref="Animation"/>.
    /// </summary>
    public abstract class AnimationKey
    {
        [WFInclude]
        public Animation Animation { get; internal set; }
        public object Target { get; set; }

        [WFInclude]
        public string Name { get; set; }

        public AnimationKey(string name) => Name = name;

        public abstract void KeyStep();
        public abstract void KeyEnd();
        public abstract void KeyCancel();
        public abstract bool EvaluateEnd();
        public abstract void Setup(WorldObject owner);
        public abstract void StartKey();
        public virtual bool ValidateTarget() => Target is not null;
    }
}
