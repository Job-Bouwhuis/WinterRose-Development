using Microsoft.Xna.Framework;
using WinterRose.Monogame;

namespace TopDownGame
{
    /// <summary>
    /// Thinker away with this.
    /// </summary>
    internal class TinkerComponent : ObjectBehavior
    {
        protected override void Awake()
        {
            float deltaTime = Time.deltaTime;
            float allTime = Time.SinceStartup;
            float worldtime = Time.SinceWorldLoad;
        }

        protected override void Start()
        {
            transform.position = new Vector2(100, 100);
            transform.rotation = 360; // rotation in degrees
            transform.scale = new Vector2(1, 1);
        }

        protected override void Update()
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

        protected override void Close()
        {
            // called when the object is destroyed
        }

        protected override void ResetClone(in ObjectComponent newComponent)
        {
            // when the component is being cloned, this method is called
        }
    }
}
