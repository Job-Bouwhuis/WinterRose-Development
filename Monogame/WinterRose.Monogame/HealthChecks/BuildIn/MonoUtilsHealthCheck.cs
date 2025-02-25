using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.HealthChecks.BuildIn
{
    internal class MonoUtilsHealthCheck() : HealthCheck("MonoUtils Health Check")
    {
        public override HealthStatus Check()
        {
            if(MonoUtils.MainGame is null)
                return HealthStatus.Unhealthy;

            if(MonoUtils.Graphics is null)
                return HealthStatus.Unhealthy;

            if(MonoUtils.Content is null)
                return HealthStatus.Damaged;

            if (MonoUtils.SpriteBatch is null)
                return HealthStatus.Unhealthy;

            return HealthStatus.Healthy;
        }
    }
}
