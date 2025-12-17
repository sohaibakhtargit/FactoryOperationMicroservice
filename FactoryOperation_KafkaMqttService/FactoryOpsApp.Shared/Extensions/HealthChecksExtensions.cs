using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Linq;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Extensions
{
    public static class HealthChecksExtensions
    {
        public static IHealthChecksBuilder AddHealthChecksIfNeeded(this IServiceCollection services)
        {
            // HealthCheckService is added the first time AddHealthChecks() is called.
            var alreadyAdded = services.Any(d => d.ServiceType == typeof(HealthCheckService));
            return alreadyAdded ? services.GetHealthChecksBuilder() : services.AddHealthChecks();
        }

        private static IHealthChecksBuilder GetHealthChecksBuilder(this IServiceCollection services)
        {
            // Recreate a builder targeting existing service collection
            return new HealthChecksBuilder(services);
        }

        private sealed class HealthChecksBuilder : IHealthChecksBuilder
        {
            public IServiceCollection Services { get; }
            public HealthChecksBuilder(IServiceCollection services) => Services = services;

            public IHealthChecksBuilder Add(HealthCheckRegistration registration)
            {
                Services.AddSingleton(registration);
                return this;
            }
        }
    }
}