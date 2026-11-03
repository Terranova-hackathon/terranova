namespace Lune.Favorites.Data;

/// <summary>Single source for the favorites limits, shared by the database mapping and the service.</summary>
public static class FavoriteLimits
{
    public const int MaxPerUser = 1000;
    public const int NameLength = 300;
    public const int ImageUrlLength = 500;
    public const int ReleasedLength = 10;
}
