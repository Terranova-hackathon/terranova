using System.ComponentModel.DataAnnotations;
namespace Lune.Auth.Contracts;

/// <summary>
/// Represents the login and register request sent from the frontend.
/// </summary>
public sealed record RegisterRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8)] string Password,
    string? DisplayName,
    [property: Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the privacy policy.")]
    bool? AcceptedPrivacyPolicy);

public sealed record LoginRequest(
    [property: Required] string Email,
    [property: Required] string Password);

public sealed record UpdateProfileDisplayNameRequest(
    [property: Required] string DisplayName);

/// <summary>
/// Represents the response structure to the frontend.
/// </summary>
public sealed record UserResponse(
    [property: Required] string Id,
    [property: Required] string Email,
    [property: Required] string? DisplayName);
