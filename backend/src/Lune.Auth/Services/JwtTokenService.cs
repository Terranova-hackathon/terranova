using System.IdentityModel.Tokens.Jwt;

using Lune.Auth.Options;
using Lune.Auth.Data;
using Microsoft.Extensions.Options;

namespace Lune.Auth.Services;

internal sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public string CreateToken(AppUser user)
    {
        //TODO: Implement JWT token creation logic here
        //         var claims = new List<Claim>
        // {
        //     new(JwtRegisteredClaimNames.Sub, user.Id),
        //     new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        //     new(ClaimTypes.Name, user.DisplayName ?? user.Email ?? user.Id),
        // };

        // var credentials = new SigningCredentials(
        //     new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
        //     SecurityAlgorithms.HmacSha256);

        // var token = new JwtSecurityToken(
        //     issuer: _options.Issuer,
        //     audience: _options.Audience,
        //     claims: claims,
        //     expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
        //     signingCredentials: credentials);

        // return new JwtSecurityTokenHandler().WriteToken(token);
        return string.Empty; // Placeholder until JWT token creation is implemented
    }
}