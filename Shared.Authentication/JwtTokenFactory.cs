using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shared.Authentication;

/// Shared low-level token builder. Each service keeps its own IJwtService
/// with its own claim shape (patient vs employee) and calls into this
/// for the actual signing/writing, so Jwt:Key/Issuer/Audience handling
/// lives in exactly one place.
public class JwtTokenFactory
{
    private readonly IConfiguration _config;

    public JwtTokenFactory(IConfiguration config) => _config = config;

    public string CreateToken(IEnumerable<Claim> claims, TimeSpan expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}