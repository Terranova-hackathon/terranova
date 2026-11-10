import { afterNextRender, Component, computed, DestroyRef, effect, inject, Injector, OnInit, signal, viewChild } from '@angular/core';
import { GameCard } from '../../shared/game-card/game-card';
import { GameModel } from '../../core/models/game.model';
import { GameService } from '../../core/services/game-service';
import { ActivatedRoute, ParamMap, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { FavoriteService } from '../../core/services/favorite-service';
import { FormsModule } from '@angular/forms';
import { catchError, combineLatest, debounceTime, distinctUntilChanged, EMPTY, map, startWith, Subject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth-service';
import { GamesNavigationService } from '../../core/services/games-navigation-service';
import { cleanList, cleanOrdering, emptyFilterState, FilterState, GameFilter, GameQuery, ListFilterKey, MAX_VALUES, toFilterParams, toFilterState } from '../../core/models/game-filter.model';
import { GameFilters } from '../../shared/game-filters/game-filters';

@Component({
  imports: [GameCard, FormsModule, GameFilters],
  selector: 'app-games',
  styleUrl: './games.scss',
  templateUrl: './games.html',
})
// Games page: shows a paginated, searchable list of games
// allows user to add/remove favorites
// Search and page live in the URL (?search=...&page=...), so a view can be
//  bookmarked, shared and reached with back/forward arrows.
export class Games implements OnInit {

  private readonly gameService = inject(GameService);
  private readonly favoriteService = inject(FavoriteService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly auth = inject(AuthService);
  // Emits when the user clicks "retry", which re-runs the fetch with the same params
  private readonly retryRequest = new Subject<void>();
  // Needed to call afterNextRender outside the constructor (in the subscribe callback)
  private readonly injector = inject(Injector);
  private readonly gamesNavigation = inject(GamesNavigationService);
// return an instance of app-game-filters from wthich we can read the filterstate and monitor changes
  private readonly filtersPanel = viewChild(GameFilters);
// Emits on every keystroke in the searchbox (even the x button)
  private readonly searchTyped = new Subject<string>();

  readonly pageSize = 12;
  readonly favoriteIds = this.favoriteService.favoriteIds;
  readonly isLoggedIn = this.auth.isLoggedIn;

  games = signal<GameModel[]>([]);
  currentPage = signal(1);
  totalPages = signal(0);
  totalGames = signal(0);
  loading = signal(false);
  error = signal('');
  // favoritesReady: blocks toggling until we know which games are already favorites
  // favoriteBusyIds: games with a favorite request still running, so double-clicks are ignored
  favoritesReady = signal(false);
  favoriteBusyIds = signal<Set<number>>(new Set());
  favoriteError = signal('');
  // searchInput: bound to the input box with ngModel, changes on every keystroke
  // searchTerm: the search actually applied, taken from the URL
  searchInput = '';
  searchTerm = signal('');


  filterState = signal<FilterState>(emptyFilterState(), {
    equal: (a, b) => JSON.stringify(a) === JSON.stringify(b)
  });
  hasActiveFilters = computed(() =>
  Object.values(toFilterParams(this.filterState())).some(v => v !== null)
);

  // Keep favorites in sync with login state. This runs again whenever isLoggedIn changes:
  // - logged out: clear favorites and turn toggling off
  // - logged in: fetch favorite IDs, then turn toggling on
  // onCleanUp cancels an unfinished fetch if login state changes again first.
  constructor() {
    effect((onCleanUp) => {
      if (!this.isLoggedIn()) {
        this.favoritesReady.set(false);
        this.favoriteError.set('');
        this.favoriteService.clear();
        return;
      }
      this.favoritesReady.set(false);
      this.favoriteError.set('');
      const subscription = this.favoriteService.loadFavoriteIds()
        .subscribe({
          next: () => {
            this.favoritesReady.set(true);
          },
          error: () => {
            this.favoriteError.set(
              'Could not load your favorite games.'
          );
        }
      });
      onCleanUp(() => subscription.unsubscribe());
    });
  }

  // Read search and page from the URL. An invalid or missing page falls back to 1.
  // distinctUntilChanged skips emissions where neither value changed
  // (for example, when only the fragment changed).
  ngOnInit(): void {
    this.searchTyped.pipe(
      map(value => value.trim()),
      debounceTime(300),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(search => {
      if (search === this.searchTerm()) return;
      void this.router.navigate(['/games'], {
        queryParams: { search: search || null, page: null },
        queryParamsHandling: 'merge',
      });
    });
    const params$ = this.route.queryParamMap.pipe(
      map(params => readGameQuery(params)),

      distinctUntilChanged((previous, current) =>
        JSON.stringify(previous) === JSON.stringify(current)
      )
    );
    // Fetch when the URL params change OR when retry is clicked.
    // startWith makes retry emit once at the start, so combineLatest fires
    // right away instead of waiting for a first retry click.
    // map then drops the retry value, so only the params continue.
    combineLatest([params$,
      this.retryRequest.pipe(startWith(undefined))
    ]).pipe(
      map(([params]) => params),
      tap(query => {
        const urlSearch = query.search ?? '';
        if (this.searchInput.trim() !== urlSearch) {
          this.searchInput = urlSearch;
        }
        this.searchTerm.set(query.search ?? '');
        this.currentPage.set(query.page ?? 1);
        this.gamesNavigation.memorize(
          this.router.url.split('#')[0]
        );
        this.loading.set(true);
        this.error.set('');
        this.filterState.set(toFilterState(query));
      }),
      // switchMap cancels any request still in flight when a new one starts, so an old
      // response can't overwrite newer results
      switchMap(query =>
        this.gameService.fetchGames({...query,
          pageSize: this.pageSize
        }).pipe(
          catchError((err: HttpErrorResponse) => {
            this.loading.set(false);
            this.games.set([]);
            this.totalPages.set(0);
            this.totalGames.set(0);
            this.error.set(
              err.status === 401
               ? 'Request failed. Please try again.'
               : 'Unable to load games. Please try again.'
            );
            return EMPTY;
          })
        )
      ),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(response => {
      this.games.set(response.items);
      this.currentPage.set(response.page);
      this.totalPages.set(response.totalPages);
      this.totalGames.set(response.totalCount);
      this.loading.set(false);
      const fragment = this.route.snapshot.fragment;
      // After returning from login, the URL has #game-<id>. Once the list has
      // rendered, scroll that card into view so the user lands where they left off.
      // The regex makes sure only our own fragment format is used to find an element.
      if (fragment && /^game-\d+$/.test(fragment)) {
        afterNextRender({
          mixedReadWrite: () => {
            document.getElementById(fragment)
              ?.scrollIntoView({
                behavior: 'smooth',
                block: 'center'
              });
          }
        }, { injector: this.injector });
      }
    });
  }
  // updates page and let URL stream reload
  changePage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    if (page === this.currentPage()) return;

    void this.router.navigate(['/games'], {
      queryParams: {
        page: page === 1 ? null : page
      },
      queryParamsHandling: 'merge'
    });
  }
  // Guests are sent to login with a returnUrl that brings them back to this
  // search, page and card (through the #game-<id> fragment)
  toggleFavorite(id: number): void {
    if (!this.isLoggedIn()) {
// avoid losing filters if loggin from favorites
      const returnUrl = this.router.serializeUrl(
        this.router.createUrlTree(['/games'], {
          queryParams:
            this.route.snapshot.queryParams,
            fragment: `game-${id}`
        })
      );
      void this.router.navigate(['/login'], {
        queryParams: { returnUrl }
      });
      return;
    }
    if (!this.favoritesReady()) return;
    if (this.favoriteBusyIds().has(id)) return;

    this.favoriteError.set('');
    this.favoriteBusyIds.update(ids => new Set([...ids, id]));

    const request = this.favoriteIds().has(id) ? this.favoriteService.removeFavorite(id)
      : this.favoriteService.addFavorite(id);
    request.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.clearFavoriteBusy(id);
      },
      error: (err: HttpErrorResponse) => {
        this.clearFavoriteBusy(id);

        if (err.status === 409) {
          this.favoriteError.set('You have reached Favorites limit.');
        } else if (err.status === 401) {
          this.favoriteError.set('Session expired or no logged in.');
        } else {
          this.favoriteError.set('Could not update favorites. Try again.');
        }
      }
    });
  }
  private clearFavoriteBusy(id: number): void {
    this.favoriteBusyIds.update(ids => {
      const updated = new Set(ids);
      updated.delete(id);
      return updated;
    });
  }

  submitSearch(): void {
    const search = this.searchInput.trim();

    const draft = this.filtersPanel()?.filterState ?? this.filterState();
    void this.router.navigate(['/games'], {
      queryParams: {
        search : search || null,
        ...toFilterParams(draft),
        page: null
      },
      queryParamsHandling: 'merge'
    });
  }
  onSearchInput(value: string): void {
    this.searchTyped.next(value);
  }

// Re-run the last fetch. This is needed because navigating to the same URL
// doesn't make queryParamMap emit again.
  retryGames(): void {
    this.retryRequest.next();
  }

  submitFilters(state: FilterState): void {
    void this.router.navigate(['/games'], {
      queryParams: {
        ...toFilterParams(state),
        page: null
      },
      queryParamsHandling: 'merge'
    });
  }
  resetFilters(): void {
    void this.router.navigate(['/games'], {
      queryParams: { search: this.searchTerm() || null }
    });
  }
}

// readscore makes sure metacritic score is a whole number
function readScore(
  value: string | null,
  fallback: number
): number {
  if (value === null || value.trim() === '') {
    return fallback;
  }
  const score = Number(value);

  if (!Number.isInteger(score) || score < 0 || score > 100) {
    return fallback;
  }
  return score;
}
// turn URL query string into clean GameQuery object
function readGameQuery(params: ParamMap): GameQuery {
  const rawPage = Number(params.get('page') ?? '1');
  const page = 
    Number.isSafeInteger(rawPage) && rawPage >= 1
      ? rawPage : 1;
  const min = params.get('metacriticMin');
  const max = params.get('metacriticMax');
  let metacriticMin = min === null ? undefined : readScore(min, 0);
  let metacriticMax = max === null ? undefined : readScore(max, 100);
  // swaps reversed range (protects case where user inserts 80 - 50,
  // instead of error assume they meant 50 - 80)
  if (metacriticMin !== undefined  && metacriticMax !== undefined
      && metacriticMin > metacriticMax) {
    [metacriticMin, metacriticMax] = [metacriticMax, metacriticMin];
  }
// 0 and 100 are not restrictions 
// so not setting doesn't exlude games with no metacritic
  if (metacriticMin === 0) metacriticMin = undefined;
  if (metacriticMax === 100) metacriticMax = undefined;

  return {
    search: params.get('search')?.trim() ?? '', page,
    genres: cleanList(params.get('genres'), 'slug'),
    platforms: cleanList(params.get('platforms'), 'id'),
    developers: cleanList(params.get('developers'), 'slug'),
    publishers: cleanList(params.get('publishers'), 'slug'),
    ordering: cleanOrdering(params.get('ordering')),
    metacriticMin,
    metacriticMax
  };
}
