namespace MockApi.Auth;

public record UserDto(string Id, string Email, string? DisplayName);

public record RegisterRequest(string? Email, string? Password, string? DisplayName);

public record LoginRequest(string? Email, string? Password);

public class UserRow
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
    public required string PasswordHash { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public int AccessFailedCount { get; set; }
    public DateTime? LockoutEnd { get; set; }

    public UserDto ToDto() => new(Id, Email, DisplayName);
}
