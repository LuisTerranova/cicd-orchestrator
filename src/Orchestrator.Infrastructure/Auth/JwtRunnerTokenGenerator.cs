using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Auth;

public sealed class JwtRunnerTokenGenerator : IRunnerTokenGenerator
{
    private readonly AuthOptions _options;

    public JwtRunnerTokenGenerator(IOptions<AuthOptions> options)
    {
        _options = options.Value;
    }

    private string GetSecretKey()
    {
        return string.IsNullOrWhiteSpace(_options.SecretKey) || _options.SecretKey.Length < 32
            ? "dev-secret-key-must-be-at-least-32-chars-long!"
            : _options.SecretKey;
    }

    public string GenerateToken(Guid runnerId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSecretKey()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "orchestrator",
            audience: "runner",
            claims: [new Claim("runnerId", runnerId.ToString())],
            expires: DateTime.UtcNow.AddDays(_options.TokenExpirationDays),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token, out Guid runnerId)
    {
        runnerId = Guid.Empty;

        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSecretKey()));
            var handler = new JwtSecurityTokenHandler();
            var result = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = "orchestrator",
                ValidateAudience = true,
                ValidAudience = "runner",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            }, out _);

            var claim = result.FindFirst("runnerId")?.Value;
            return Guid.TryParse(claim, out runnerId);
        }
        catch
        {
            return false;
        }
    }
}
