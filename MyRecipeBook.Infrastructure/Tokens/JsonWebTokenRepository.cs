using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace MyRecipeBook.Infrastructure.Tokens;

public class JsonWebTokenRepository(double expirationTimeInMinutes, string signingKey)
    : ITokenRepository
{
    private SymmetricSecurityKey PrivateSecurityKey =>
        new(new UTF8Encoding().GetBytes(signingKey));
    private  TokenValidationParameters ValidationParameters => 
        new()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            IssuerSigningKey = PrivateSecurityKey,
            ClockSkew = TimeSpan.Zero //for token expired test
        };

    public string Generate(Guid specificGuid)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new List<Claim> { new(ClaimTypes.Sid, specificGuid.ToString()) };

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(PrivateSecurityKey, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddMinutes(expirationTimeInMinutes)
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