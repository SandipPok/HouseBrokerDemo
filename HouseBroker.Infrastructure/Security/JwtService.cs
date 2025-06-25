using HouseBroker.Application.Interfaces;
using HouseBroker.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HouseBroker.Infrastructure.Security
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
                     {
                         new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                         new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                         new(JwtRegisteredClaimNames.Email, user.Email),
                         new(ClaimTypes.Role, user.Role)
                     };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            double expiryHours = 3;
            var expiryHoursSection = _configuration.GetSection("JwtSettings:ExpiryHours");
            if (expiryHoursSection.Exists() && double.TryParse(expiryHoursSection.Value, out var parsedExpiryHours))
            {
                expiryHours = parsedExpiryHours;
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expiryHours),
                SigningCredentials = creds,
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.CreateEncodedJwt(tokenDescriptor);
        }
    }
}