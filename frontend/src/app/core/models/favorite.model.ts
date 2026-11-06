import { GameModel } from "./game.model";


export interface FavoriteModel extends GameModel {
	addedAtUtc: string;
}
