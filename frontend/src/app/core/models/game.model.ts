export interface GameModel {
	id: number;
	name: string;
	backgroundImage: string | null;
	released: string | null;
	rating: number;
	metacritic: number | null;
	genres: string[];
	platforms: string[];
}

export interface GameDetails extends GameModel {
	description: string | null;
	website: string | null;
	developer: string[];
	publishers: string[];
}
