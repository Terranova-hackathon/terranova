namespace MockApi.Favorites;

public class FavoriteStore
{
    public const int MaxPerUser = 1000;

    private readonly List<FavoriteRow> _rows = [];
    private readonly object _lock = new();

    public List<FavoriteRow> List(string userId)
    {
        lock (_lock)
            return _rows.Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedAtUtc).ToList();
    }

    public enum AddResult { Added, AlreadyExists, LimitReached }

    public AddResult Add(FavoriteRow row)
    {
        lock (_lock)
        {
            var mine = _rows.Where(r => r.UserId == row.UserId).ToList();
            if (mine.Any(r => r.RawgId == row.RawgId)) return AddResult.AlreadyExists;
            if (mine.Count >= MaxPerUser) return AddResult.LimitReached;
            _rows.Add(row);
            return AddResult.Added;
        }
    }

    public void Remove(string userId, int rawgId)
    {
        lock (_lock)
            _rows.RemoveAll(r => r.UserId == userId && r.RawgId == rawgId);
    }
}
