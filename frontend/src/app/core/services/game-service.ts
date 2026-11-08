import { inject, Service } from '@angular/core';
import { GameModel } from '../models/game.model';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { PagedResult } from '../models/paged-result.model';

@Service()
export class GameService {

	private readonly http = inject(HttpClient);
	private readonly apiUrl = '/api/games';

	fetchGames(
		page = 1, pageSize = 12, search = ''
	): Observable<PagedResult<GameModel>> {
		return this.http.get<PagedResult<GameModel>>( this.apiUrl, { 
			params: { 
				page,
				pageSize,
				search
			}
	  	});
	}
}
