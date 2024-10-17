using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyRecipeBook.Domain.Interfaces;

namespace MyRecipeBook.Infrastructure.Tokens;

public class JsonWebTokenCreate : ITokenGenerator
{
    private readonly int _expirationTimeInMinutes;
    private readonly string _signingKey;

    public JsonWebTokenCreate(int expirationTimeInMinutes, string signingKey)
    {
        _expirationTimeInMinutes = expirationTimeInMinutes;
        _signingKey = signingKey;
    }
    
    public string Generate(Guid specificGuid)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new List<Claim> { new Claim(ClaimTypes.Sid, specificGuid.ToString()) };
        var securityKey = new SymmetricSecurityKey(new UTF8Encoding().GetBytes(_signingKey));

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddMinutes(_expirationTimeInMinutes)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}