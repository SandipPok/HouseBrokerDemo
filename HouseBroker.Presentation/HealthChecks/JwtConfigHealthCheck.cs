using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HouseBroker.Presentation.HealthChecks
{
    public class JwtConfigHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;

        public JwtConfigHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(key) || key.Length < 32)
                return Task.FromResult(HealthCheckResult.Unhealthy("Invalid JWT key"));

            if (string.IsNullOrEmpty(issuer))
                return Task.FromResult(HealthCheckResult.Unhealthy("Missing JWT issuer"));

            if (string.IsNullOrEmpty(audience))
                return Task.FromResult(HealthCheckResult.Unhealthy("Missing JWT audience"));

            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}