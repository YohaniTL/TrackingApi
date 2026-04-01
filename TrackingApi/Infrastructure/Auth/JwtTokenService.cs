using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace TrackingApi.Infrastructure.Auth;

public sealed class JwtTokenService
{
    private readonly IOptions<JwtOptions> _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }

    public TokenResult CreateToken(string email, string displayName)
    {
        var options = _jwtOptions.Value;
        var expiresAtLocal = DateTime.Now.AddDays(options.TokenDays <= 0 ? 7 : options.TokenDays);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, displayName)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: string.IsNullOrWhiteSpace(options.Issuer) ? null : options.Issuer,
            audience: string.IsNullOrWhiteSpace(options.Audience) ? null : options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtLocal.ToUniversalTime(),
            signingCredentials: credentials);

        return new TokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtLocal);
    }
}
