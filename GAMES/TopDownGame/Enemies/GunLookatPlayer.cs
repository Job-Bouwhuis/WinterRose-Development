using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;

namespace TopDownGame.Enemies
{
    class GunLookatPlayer : ObjectBehavior
    {
        private Transform target;

        protected override void Awake() => target = world.FindObjectWithFlag("Player")?.transform
                ?? throw new Exception("No player in the scene. expected object tag: \"Player\"");

        protected override void Update() => transform.LookAt(target.position);
    }
}
