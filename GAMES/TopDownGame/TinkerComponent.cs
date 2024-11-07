using Microsoft.Xna.Framework;
using WinterRose.Monogame;

namespace TopDownGame
{
    /// <summary>
    /// Thinker away with this.
    /// </summary>
    internal class TinkerComponent : ObjectBehavior
    {
        private void Awake()
        {
            float deltaTime = Time.SinceLastFrame;
            float allTime = Time.SinceStartup;
            float worldtime = Time.SinceWorldLoad;
        }

        private void Start()
        {
            transform.position = new Vector2(100, 100);
            transform.rotation = 360; // rotation in degrees
            transform.scale = new Vector2(1, 1);
        }

        private void Update()
        {
            // called each frame.

            if(Input.GetKey(Microsoft.Xna.Framework.Input.Keys.I))
            {

            }
            // the above can be written as
            if(Input.I)
            {

            }
            // and keydown can be
            if(Input.IDown)
            {

            }

            // this is implemented for all entries in the Keys enum
            //.... yes... i dont have life
        }

        private void Close()
        {
            // called when the object is destroyed
        }

        public override void ResetClone(in ObjectComponent newComponent)
        {
            // when the component is being cloned, this method is called
        }
    }
}
