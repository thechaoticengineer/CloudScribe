using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CloudScribe.Notes.API.Tests.Helpers;

public class TestAuthHandler
{
    public const string Issuer = "integration-tests";
    public const string TestSecretKey = "super-secret-key-for-integration-tests-only!!";
    
    public static string GenerateJwtToken(string userId = TestConst.TestUserId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId), 
            new Claim(JwtRegisteredClaimNames.Name, "Integration Tester"),
            new Claim("preferred_username", "tester") 
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: "account",
            claims: claims,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}