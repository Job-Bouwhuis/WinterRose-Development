using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.HealthChecks.BuildIn
{
    internal class UniverseHealthChecks() : HealthCheck("Universe Health Check")
    {
        public override HealthStatus Check()
        {
            if(Universe.imGuiRenderer is null)
            {
                Message = "ImGui renderer faulted.";
                return HealthStatus.Unhealthy;
            }
            return HealthStatus.Healthy;
        }
    }
}
