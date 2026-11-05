export interface FavoriteModel {
	id: number;
	name: string;
	imageUrl: string | null;
	released: string | null;
	rating: number;
	metacritic: number | null;
	genres: string[];
	platforms: string[];
	addedAtUtc: string;
}
