using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Animations;

namespace WinterRose.Monogame
{
    /// <summary>
    /// A component that plays <see cref="Animation"/>s
    /// </summary>
    public class Animator : ObjectBehavior
    {
        /// <summary>
        /// The <see cref="AnimationController"/> that this <see cref="Animator"/> will play
        /// </summary>
        [IncludeInTemplateCreation]
        public AnimationController AnimationController { get; set; }

        /// <summary>
        /// Whether or not this <see cref="Animator"/> will play automatically on <see cref="Awake"/>
        /// </summary>
        [IncludeInTemplateCreation]
        public bool AutoPlay { get; set; } = true;
        /// <summary>
        /// Whether or not this <see cref="Animator"/> is currently playing
        /// </summary>
        public bool Playing { get; set; } = false;
        public Animator() { }
        public Animator(AnimationController animation)
        {
            AnimationController = animation;
        }

        private void Awake()
        {
            if(AnimationController is null)
            {
                Debug.LogWarning("No animation selected " + owner.Name);
                return;
            }
            AnimationController.Setup(owner);

            if (AutoPlay)
                Playing = true;
        }

        private void Update()
        {
            if (!Playing) return;
            if (AnimationController is null)
            {
                Debug.LogWarning("No animation selected " + owner.Name);
                return;
            }
            AnimationController.AnimationStep();
        }
    }
}
