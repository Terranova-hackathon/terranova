import { GameModel } from "./game.model";
import { FavoriteModel } from "./favorite.model";

export function favoriteToGame(favorite: FavoriteModel): GameModel {
	return {
		id: favorite.id,
		name: favorite.name,
		backgroundImage: favorite.imageUrl,
		released: favorite.released,
		rating: favorite.rating,
		metacritic: favorite.metacritic,
		genres: favorite.genres,
		platforms: favorite.platforms
	};
}

// This mapper uses a function to convert Favorite Model into the existing game type expected by GameCard