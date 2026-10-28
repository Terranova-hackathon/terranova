using System.ComponentModel.DataAnnotations;
using MockApi.Shared;

namespace MockApi.Auth;

public static class AuthEndpoints
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);
    private static readonly EmailAddressAttribute EmailValidator = new();

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapGet("/me", (HttpContext http, SessionStore sessions, UserStore users) =>
        {
            var userId = sessions.GetUserId(http);
            var user = userId is null ? null : users.FindById(userId);
            return user is null ? Problems.Unauthorized() : Results.Ok(user.ToDto());
        });

        group.MapPost("/register", (RegisterRequest? body, HttpContext http, SessionStore sessions, UserStore users) =>
        {
            var errors = new Dictionary<string, string[]>();
            var email = body?.Email?.Trim();
            var password = body?.Password;

            if (string.IsNullOrEmpty(email))
                errors["Email"] = ["The Email field is required."];
            else if (!EmailValidator.IsValid(email))
                errors["Email"] = ["The Email field is not a valid e-mail address."];

            if (string.IsNullOrEmpty(password))
                errors["Password"] = ["The Password field is required."];
            else if (password.Length < 8)
                errors["Password"] = ["The field Password must be a string or array type with a minimum length of '8'."];

            if (errors.Count > 0) return Problems.Validation(errors);

            var identityErrors = PasswordRuleErrors(password!);
            if (identityErrors.Count > 0) return Problems.Validation(identityErrors);

            var displayName = string.IsNullOrWhiteSpace(body!.DisplayName) ? null : body.DisplayName.Trim();
            if (!users.TryCreate(email!, password!, displayName, out var user))
            {
                return Problems.Validation(new Dictionary<string, string[]>
                {
                    ["DuplicateUserName"] = [$"Username '{email}' is already taken."],
                    ["DuplicateEmail"] = [$"Email '{email}' is already taken."]
                });
            }

            sessions.SignIn(http, user.Id);
            return Results.Created("/api/auth/me", user.ToDto());
        });

        group.MapPost("/login", (LoginRequest? body, HttpContext http, SessionStore sessions, UserStore users) =>
        {
            var errors = new Dictionary<string, string[]>();
            var email = body?.Email?.Trim();

            if (string.IsNullOrEmpty(email))
                errors["Email"] = ["The Email field is required."];
            else if (!EmailValidator.IsValid(email))
                errors["Email"] = ["The Email field is not a valid e-mail address."];

            if (string.IsNullOrEmpty(body?.Password))
                errors["Password"] = ["The Password field is required."];

            if (errors.Count > 0) return Problems.Validation(errors);

            var invalid = Problems.Problem(401, "Invalid email or password.");
            var user = users.FindByEmail(email!);
            if (user is null) return invalid;
            if (user.LockoutEnd > DateTime.UtcNow) return invalid;

            if (user.PasswordHash != body!.Password)
            {
                if (++user.AccessFailedCount >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                    user.AccessFailedCount = 0;
                }
                return invalid;
            }

            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            sessions.SignIn(http, user.Id);
            return Results.Ok(user.ToDto());
        });

        group.MapPost("/logout", (HttpContext http, SessionStore sessions) =>
        {
            sessions.SignOut(http);
            return Results.NoContent();
        });
    }

    private static Dictionary<string, string[]> PasswordRuleErrors(string password)
    {
        var errors = new Dictionary<string, string[]>();
        if (!password.Any(char.IsDigit))
            errors["PasswordRequiresDigit"] = ["Passwords must have at least one digit ('0'-'9')."];
        if (!password.Any(char.IsLower))
            errors["PasswordRequiresLower"] = ["Passwords must have at least one lowercase ('a'-'z')."];
        if (!password.Any(char.IsUpper))
            errors["PasswordRequiresUpper"] = ["Passwords must have at least one uppercase ('A'-'Z')."];
        if (password.All(char.IsLetterOrDigit))
            errors["PasswordRequiresNonAlphanumeric"] = ["Passwords must have at least one non alphanumeric character."];
        return errors;
    }
}
