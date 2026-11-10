import { inject, Service } from '@angular/core';
import { GameDetails, GameModel } from '../models/game.model';
import { Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { PagedResult } from '../models/paged-result.model';
import { CompanyOption, GameFilter, GameQuery } from '../models/game-filter.model';

@Service()
export class GameService {

	private readonly http = inject(HttpClient);
	private readonly apiUrl = '/api/games';

	fetchGames(query: GameQuery = {}
	): Observable<PagedResult<GameModel>> {
		let params = new HttpParams()
			.set('page', query.page ?? 1)
			.set('pageSize', query.pageSize ?? 12);
		const otherFilters = {
			search: query.search,
			genres: query.genres,
			platforms: query.platforms,
			developers: query.developers,
			publishers: query.publishers,
			ordering: query.ordering,
			metacriticMin: query.metacriticMin,
			metacriticMax: query.metacriticMax
		};

		for (const [key, value] of Object.entries(otherFilters)) {
			if (value !== undefined && value !== null && value !== '') {
				params = params.set(key, String(value));
			}
		}

		return this.http.get<PagedResult<GameModel>>(
			this.apiUrl,
			{ params }
		);
	}
	fetchCompanies(
		kind: 'developer' | 'publisher',
		search: string
	): Observable<CompanyOption[]> {
		const params = new HttpParams()
			.set('kind', kind)
			.set('search', search.trim());
		return this.http.get<CompanyOption[]>(
			`${this.apiUrl}/companies`, { params }
		);
	}

	fetchFeatured(): Observable<GameDetails[]> {
		return this.http.get<GameDetails[]>(
			`${this.apiUrl}/featured`
		);
	}

	fetchFilterOptions(): Observable<GameFilter> {
		return this.http.get<GameFilter>(
			`${this.apiUrl}/filters`
		);
	}
}
