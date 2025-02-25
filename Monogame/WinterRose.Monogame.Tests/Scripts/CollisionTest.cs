using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Tests.Scripts
{
    internal class CollisionTest : ObjectComponent
    {
        SpriteRenderer renderer;
        private void Start()
        {
            Collider collider = FetchComponent<Collider>();
            renderer = FetchComponent<SpriteRenderer>();

            collider.OnCollisionEnter += Collider_OnCollisionEnter;
            collider.OnCollisionStay += Collider_OnCollisionStay;
            collider.OnCollisionExit += Collider_OnCollisionExit;
        }

        private void Collider_OnCollisionExit(CollisionInfo obj)
        {
            renderer.IsVisible = true;
        }

        private void Collider_OnCollisionStay(CollisionInfo obj)
        {
            Debug.Log("Im colliding");
        }

        private void Collider_OnCollisionEnter(CollisionInfo obj)
        {
            renderer.IsVisible = false;
        }
    }
}
