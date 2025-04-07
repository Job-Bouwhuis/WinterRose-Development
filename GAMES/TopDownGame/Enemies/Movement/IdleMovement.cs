using Microsoft.Xna.Framework;
using WinterRose.Monogame;

namespace TopDownGame.Enemies.Movement;

internal class IdleMovement() : AIMovement("idle")
{
    public override void Move()
    {
        if(Vector2.Distance(Controller.Target.transform.position,
            Controller.transform.position) < Controller.VisionRange)
        {
            Controller.SetMovement("chase");
        }
    }

    public override void TransitionIn(AIMovement current)
    {
        
    }

    public override void TransitionOut(AIMovement next)
    {
        
    }
}
