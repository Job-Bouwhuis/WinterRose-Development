using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.HealthChecks.BuildIn
{
    internal class GameHealthChecks() : HealthCheck("Game Health Checks")
    {
        public override HealthStatus Check()
        {
            if (MonoUtils.MainGame is null)
            {
                Message = "Game instance was null";
                return HealthStatus.Unhealthy;
            }

            if(MonoUtils.MainGame.Components.Count != 0)
            {
                Message = "Game class has components. Please use the components from WinterRose which you can attach to your WorldObjects (ObjectComponent/ObjectBehavior)";
                return HealthStatus.Damaged;
            }

            return HealthStatus.Healthy;
        }
    }
}
