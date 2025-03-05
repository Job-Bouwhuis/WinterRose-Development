using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.HealthChecks.BuildIn
{
    internal class TimeHealthChecks() : HealthCheck("Time Health Checks")
    {
        public override HealthStatus Check()
        {
            try
            {
                _ = Time.deltaTime;
                return HealthStatus.Healthy;
            }
            catch (Exception e)
            {
                return HealthStatus.Unhealthy;
            }
        }
    }
}
