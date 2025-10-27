using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FabricLibrary.Backend.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _cfg;

    public JwtService(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    /// <summary>
    /// Create an application JWT for the specified user.
    /// </summary>
    /// <param name="userId">User identifier (GUID) to include in the token <c>sub</c> claim.</param>
    /// <param name="email">User email to include as a claim.</param>
    /// <returns>Signed JWT string.</returns>
    public string CreateToken(Guid userId, string email)
    {
        var secret = _cfg["JWT_SECRET"] ?? "dev-secret-change-me";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
