namespace ZTR.Framework.Service
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthCheckup;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    /// <summary>
    /// SystemMemoryHealthCheck
    /// </summary>
    /// <seealso cref="IHealthCheck" />
    public class SystemMemoryHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Runs the health check, returning the status of the component being checked.
        /// </summary>
        /// <param name="context">A context object associated with the current execution.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> that can be used to cancel the health check.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task`1" /> that completes when the health check has finished, yielding the status of the component being checked.
        /// </returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var client = new MemoryMetricsClient();
            var metrics = client.GetMetrics();
            var percentUsed = 100 * metrics.Used / metrics.Total;

            var status = HealthStatus.Healthy;
            if (percentUsed > 80)
            {
                status = HealthStatus.Degraded;
            }

            if (percentUsed > 90)
            {
                status = HealthStatus.Unhealthy;
            }

            var data = new Dictionary<string, object>
            {
                {"Total", metrics.Total}, {"Used", metrics.Used}, {"Free", metrics.Free}
            };

            var result = new HealthCheckResult(status, null, null, data);

            return await Task.FromResult(result);
        }
    }
}
