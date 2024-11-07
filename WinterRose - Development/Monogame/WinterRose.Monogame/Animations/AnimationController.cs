using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Animations
{
    public sealed class AnimationController
    {
        public List<Animation> Animations { get; private set; } = new();
        
        public AnimationController(params Animation[] animations) => animations.Foreach(x => Animations.Add(x));
        public AnimationController(params string[] assetNames)
        {
            foreach (var name in assetNames) 
                Animations.Add(Prefab.Create<AnimationPrefab>(name));
        }

        public void AnimationStep()
        {
            foreach (var anim in Animations)
                anim.Step();
        }
        internal void Setup(WorldObject owner) => Animations.Foreach(x => x.Setup(owner));
    }
}
