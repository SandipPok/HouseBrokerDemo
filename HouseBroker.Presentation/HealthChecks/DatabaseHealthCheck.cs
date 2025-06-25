using Dapper;
using HouseBroker.Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HouseBroker.Presentation.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DatabaseHealthCheck(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                await connection.QueryAsync("SELECT 1");
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database connection failed", ex);
            }
        }
    }
}