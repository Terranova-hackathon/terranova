
using Lune.Auth.Options;
using Lune.Auth.Contracts;
using Microsoft.Extensions.Options;

namespace Lune.Auth.Services;

internal sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public string CreateToken(AppUser user)
    {
        //TODO: Implement JWT token creation logic here
        return string.Empty; // Placeholder until JWT token creation is implemented
    }
}