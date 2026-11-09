using System.ComponentModel.DataAnnotations;

namespace Lune.Auth.Options;


public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required] public string Secret { get; set; } = string.Empty;
    [Required] public string Issuer { get; set; } = "lune";
    /// <summary>
    /// Set to 24 hours expiration for JWT. 
    /// </summary>
    [Required] public int ExpirationMinutes { get; set; } = 1440;   
    public string CookieName { get; set; } = "Lune_auth";
}
