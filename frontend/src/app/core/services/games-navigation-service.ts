import { computed, inject, Service, signal } from '@angular/core';
import { Router, UrlTree } from '@angular/router';

@Service()
export class GamesNavigationService {
	private readonly storageKey = 'games-last-url';
	private readonly router = inject(Router);

	readonly lastGamesUrl = signal('/games');
	readonly gamesUrl = computed<UrlTree>(() => 
		this.router.parseUrl(this.lastGamesUrl())
	);

	memorize(url: string): void {
		if (!/^\/games(?:[?#]|$)/.test(url)) {
			return;
		}
		this.lastGamesUrl.set(url);
	}
}
