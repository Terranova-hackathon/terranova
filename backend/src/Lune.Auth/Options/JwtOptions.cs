using System.ComponentModel.DataAnnotations;


namespace Lune.Auth.Options;


public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required]
    public string Secret { get; set; } = string.Empty;
    [Required]
    public string Issuer { get; set; } = "lune";

    [Required]
    public int ExpirationMinutes { get; set; } = 60;
    //TODO: need to choose name for cookie, example: "game_explorer_auth" or similar    
    public string CookieName { get; set; } = "game_explorer_auth";
}
