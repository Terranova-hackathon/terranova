import { afterNextRender, Component, computed, DestroyRef, effect, ElementRef, inject, Injector, OnInit, signal, viewChild } from '@angular/core';
import { GameCard } from '../../shared/game-card/game-card';
import { GameModel } from '../../core/models/game.model';
import { GameService } from '../../core/services/game-service';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { FavoriteService } from '../../core/services/favorite-service';
import { FormsModule } from '@angular/forms';
import { catchError, combineLatest, debounceTime, distinctUntilChanged, EMPTY, map, startWith, Subject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth-service';
import { GamesNavigationService } from '../../core/services/games-navigation-service';
import { emptyFilterState, FilterState, toFilterParams, toFilterState } from '../../core/models/game-filter.model';
import { GameFilters } from '../../shared/game-filters/game-filters';
import { readGameQuery } from '../../core/utils/game-query';
import { ScrollControls } from '../../shared/scroll-controls/scroll-controls';

@Component({
  imports: [GameCard, FormsModule, GameFilters, ScrollControls],
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
  private readonly loginDialog = viewChild<ElementRef<HTMLDialogElement>>('loginDialog');

  readonly pageSize = 12;
  readonly favoriteIds = this.favoriteService.favoriteIds;
  readonly isLoggedIn = this.auth.isLoggedIn;

  games = signal<GameModel[]>([]);
  loginPrompt = signal<GameModel | null>(null);
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
            this.completePendingFavorite();
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
        queryParams: this.searchParams(search),
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
      this.openLoginPrompt(id);
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

  private openLoginPrompt(id: number): void {
    const game = this.games().find(g => g.id === id) ?? null;
    this.loginPrompt.set(game);
    this.loginDialog()?.nativeElement.showModal();
  }

  goToLogin(): void {
    const game = this.loginPrompt();
    if (!game) return;

    this.favoriteService.setPendingFavorite(game.id);
    const returnUrl = this.router.serializeUrl(
      this.router.createUrlTree(['/games'], {
        queryParams: this.route.snapshot.queryParams,
        fragment: `game-${game.id}`
      })
    );
    void this.router.navigate(['/login'], { queryParams: { returnUrl } });
  }

  private completePendingFavorite(): void {
    const id = this.favoriteService.takePendingFavorite();
    if (id === null) return;
    if (this.route.snapshot.fragment !== `game-${id}`) return;
    if (this.favoriteIds().has(id)) return;
    this.toggleFavorite(id);
  }

  private clearFavoriteBusy(id: number): void {
    this.favoriteBusyIds.update(ids => {
      const updated = new Set(ids);
      updated.delete(id);
      return updated;
    });
  }

  private searchParams(search: string) {
    const wasSearching = this.searchTerm() !== '';

    if (search && !wasSearching) {
      const page = this.currentPage();
      return { search, page: null, fromPage: page > 1 ? page : null };
    }
    if (!search && wasSearching) {
      return {
        search: null,
        page: readFromPage(this.route.snapshot.queryParamMap),
        fromPage: null
      };
    }
    return { search: search || null, page: null };
  }

  submitSearch(): void {
    const search = this.searchInput.trim();
    const draft = this.filtersPanel()?.filterState ?? this.filterState();

    void this.router.navigate(['/games'], {
      queryParams: {
        ...this.searchParams(search),
        ...toFilterParams(draft)
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
        page: null,
        fromPage: null
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

function readFromPage(params: ParamMap): number | null {
  const page = Number(params.get('fromPage'));
  return Number.isSafeInteger(page) && page > 1 ? page : null;
}
