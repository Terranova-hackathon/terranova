import { FilterOption } from "./game-filter.model";
import { PagedResult } from "./paged-result.model";

export interface GameModel {
	id: number;
	name: string;
	backgroundImage: string | null;
	released: string | null;
	rating: number;
	metacritic: number | null;
	genres: FilterOption[];
	platforms: FilterOption[];
}

export interface GameDetailsModel extends GameModel {
	description: string | null;
	website: string | null;
	developers: FilterOption[];
	publishers: FilterOption[];
	cover?: string | null;
	storyLine?: string | null;
	screenshots: string[];
	artworks?: string[];
	videos?: string[];
	relatedGames: PagedResult<GameModel>;
}
