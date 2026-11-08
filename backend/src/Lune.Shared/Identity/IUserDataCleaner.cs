namespace Lune.Shared.Identity;

/// <summary>
/// Implemented by any module that stores per-user data (favorites, wishlist, ...).
/// Register each implementation with <c>services.AddScoped&lt;IUserDataCleaner, MyCleaner&gt;()</c>;
/// the account deletion endpoint runs every registered cleaner, so Auth never references other modules.
/// </summary>
public interface IUserDataCleaner
{
    Task DeleteUserDataAsync(string userId, CancellationToken cancellationToken = default);
}
