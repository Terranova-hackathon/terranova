using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace Lune.Auth.Contracts;

/// <summary>
/// Represents the login and register request sent from the frontend.
/// </summary>
public sealed record LoginRequest(
    [property: Required] string Username,
    [property: Required] string Password,
    [property: Required] string? DisplayName);

public sealed record RegisterRequest(
    [property: Required] string Username,
    [property: Required] string Password);



/// <summary>
/// Represents the response structer to the frontend.
/// </summary>
public sealed record UserResponse(
    [property: Required] string Id,
    [property: Required] string Email,
    [property: Required] string? DisplayName);

