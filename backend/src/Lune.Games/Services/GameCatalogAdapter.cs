using Lune.Shared.Games;

namespace Lune.Games.Services;

/// <summary>Exposes the games service to other modules through the shared <see cref="IGameCatalog"/> contract.</summary>
internal sealed class GameCatalogAdapter(IGamesService games) : IGameCatalog
{
    public async Task<GameSnapshot?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        var game = await games.GetByIdAsync(id, page: 1, pageSize: 6, cancellationToken);
        return game is null
            ? null
            : new GameSnapshot(game.Id, game.Name, game.Released, game.BackgroundImage, game.Rating, game.Metacritic, game.Genres, game.Platforms);
    }
}
