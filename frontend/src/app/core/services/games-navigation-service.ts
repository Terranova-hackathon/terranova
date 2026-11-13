import { computed, inject, Service, signal } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { FilterState, toFilterParams, toFilterState } from '../models/game-filter.model';
import { readGameQuery } from '../utils/game-query';

@Service()
export class GamesNavigationService {
	private readonly storageKey = 'games-last-url';
	private readonly router = inject(Router);

	readonly lastGamesUrl = signal('/games');
	readonly gamesUrl = computed<UrlTree>(() => 
		this.router.parseUrl(this.lastGamesUrl())
	);
	// The filters from the last Games page URL, as a FilterState
	readonly lastFilterState = computed<FilterState>(() =>
		toFilterState(readGameQuery(this.gamesUrl().queryParamMap))
	);

	readonly hasActiveFilters = computed(() =>
		Object.values(toFilterParams(this.lastFilterState())).some(v => v !== null)
	);

	memorize(url: string): void {
		if (!/^\/games(?:[?#]|$)/.test(url)) {
			return;
		}
		this.lastGamesUrl.set(url);
	}
	// Drops the filters but keeps the search text
	clearFilters(): void {
		const search = this.gamesUrl().queryParamMap.get('search');
		const tree = this.router.createUrlTree(['/games'], {
			queryParams: { search }
		});
		this.lastGamesUrl.set(this.router.serializeUrl(tree));
	}
}
