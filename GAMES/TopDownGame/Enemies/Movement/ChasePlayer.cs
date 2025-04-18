using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;

namespace TopDownGame.Enemies.Movement
{
    public class ChasePlayer() : AIMovement("chase")
    {
        public override void Move()
        {
            var dir = Controller.Target.transform.position - Controller.transform.position;
            Controller.transform.position += dir.Normalized() * Controller.MovementSpeed * Time.deltaTime;

            var dist = Vector2.Distance(Controller.Target.transform.position,
                Controller.transform.position);

            if (dist > Controller.VisionRange)
            {
                Controller.SetMovement("idle");
            }
            else if (dist < Controller.EvadeDistance)
            {
                Controller.SetMovement("evade");
            }
        }

        public override void TransitionIn(AIMovement current)
        {
        }

        public override void TransitionOut(AIMovement next)
        {
        }
    }
}
