using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace MyRecipeBook.Infrastructure.Tokens;

public class JsonWebTokenRepository : ITokenRepository
{
    private readonly int _expirationTimeInMinutes;
    private readonly string _signingKey;

    private SymmetricSecurityKey PrivateSecurityKey =>
        new SymmetricSecurityKey(new UTF8Encoding().GetBytes(_signingKey));
private  TokenValidationParameters ValidationParameters => 
    new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = PrivateSecurityKey,
    };
    public JsonWebTokenRepository(int expirationTimeInMinutes, string signingKey)
    {
        _expirationTimeInMinutes = expirationTimeInMinutes;
        _signingKey = signingKey;
    }
    
    public string Generate(Guid specificGuid)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new List<Claim> { new Claim(ClaimTypes.Sid, specificGuid.ToString()) };

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(PrivateSecurityKey, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddMinutes(_expirationTimeInMinutes)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    public Guid ValidateAndGetUserIdentifier(string token)
    {
        var principal = new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters, out _);

        var userIdentifier = principal.Claims.First(c => c.Type == ClaimTypes.Sid).Value;

        return Guid.Parse(userIdentifier);
    }
}