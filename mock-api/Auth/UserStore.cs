using System.Collections.Concurrent;

namespace MockApi.Auth;

public class UserStore
{
    public const string DemoEmail = "demo@example.com";
    public const string DemoPassword = "Passw0rd!";

    private readonly ConcurrentDictionary<string, UserRow> _byId = new();

    public UserStore()
    {
        Add(new UserRow
        {
            Id = "7c1d2f0e-6b1a-4a39-9f1e-2c3f6a5b8d10",
            Email = DemoEmail,
            DisplayName = "Demo User",
            PasswordHash = DemoPassword
        });
    }

    public UserRow? FindById(string id) => _byId.GetValueOrDefault(id);

    public UserRow? FindByEmail(string email) =>
        _byId.Values.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

    public bool TryCreate(string email, string password, string? displayName, out UserRow user)
    {
        lock (_byId)
        {
            if (FindByEmail(email) is not null)
            {
                user = null!;
                return false;
            }

            user = new UserRow { Id = Guid.NewGuid().ToString(), Email = email, DisplayName = displayName, PasswordHash = password };
            Add(user);
            return true;
        }
    }

    private void Add(UserRow user) => _byId[user.Id] = user;
}
