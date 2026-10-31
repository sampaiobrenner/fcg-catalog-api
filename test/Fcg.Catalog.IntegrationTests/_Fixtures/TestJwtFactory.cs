using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Fcg.Contracts.Security;
using Microsoft.IdentityModel.Tokens;

namespace Fcg.Catalog.IntegrationTests._Fixtures;

/// <summary>Gera tokens com a mesma chave/issuer/audience da UsersAPI, sem depender dela.</summary>
public static class TestJwtFactory
{
    public const string SigningKey = "fcg-dev-only-signing-key-change-me-0123456789";
    public const string Issuer = "fcg-users-api";
    public const string Audience = "fcg";

    public static string Create(Guid userId, string role = FcgRoles.User, string name = "Teste", string email = "teste@fcg.dev")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var token = new JwtSecurityToken(
            issuer: Issuer, audience: Audience,
            claims: [new(FcgClaimTypes.UserId, userId.ToString()), new(FcgClaimTypes.Email, email),
                     new(FcgClaimTypes.Name, name), new(FcgClaimTypes.Role, role)],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
