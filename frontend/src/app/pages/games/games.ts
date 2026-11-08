import { afterNextRender, Component, DestroyRef, effect, inject, Injector, OnInit, signal } from '@angular/core';
import { GameCard } from '../../shared/game-card/game-card';
import { GameModel } from '../../core/models/game.model';
import { GameService } from '../../core/services/game-service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { FavoriteService } from '../../core/services/favorite-service';
import { FormsModule } from '@angular/forms';
import { catchError, combineLatest, distinctUntilChanged, EMPTY, map, startWith, Subject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth-service';
import { GamesNavigationService } from '../../core/services/games-navigation-service';

@Component({
  imports: [GameCard, FormsModule],
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
    const params$ = this.route.queryParamMap.pipe(
      map(params => {
        const search = params.get('search')?.trim() ?? '';
        const rawPage = params.get('page') ?? '1';
        const parsedPage = Number(rawPage);
        const page = Number.isSafeInteger(parsedPage)
          && parsedPage >= 1 ? parsedPage : 1;
        return {search, page };
      }),

      distinctUntilChanged((previous, current) =>
        previous.search === current.search &&
        previous.page === current.page
      ),
    )
    // Fetch when the URL params change OR when retry is clicked.
    // startWith makes retry emit once at the start, so combineLatest fires
    // right away instead of waiting for a first retry click.
    // map then drops the retry value, so only the params continue.
    combineLatest([params$,
      this.retryRequest.pipe(startWith(undefined))
    ]).pipe(
      map(([params]) => params),
      tap(({search, page}) => {
        this.searchInput = search;
        this.searchTerm.set(search);
        this.currentPage.set(page);
        const gamesUrl = this.router.serializeUrl(
          this.router.createUrlTree(['/games'], {
            queryParams: {
              search: search || null,
              page: page === 1 ? null : page
            }
          })
        );
        this.gamesNavigation.memorize(gamesUrl);
        this.loading.set(true);
        this.error.set('');
      }),
      // switchMap cancels any request still in flight when a new one starts, so an old
      // response can't overwrite newer results
      switchMap(({ search, page }) =>
        this.gameService.fetchGames(page,
          this.pageSize,
          search
        ).pipe(
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
      const returnUrl = this.router.serializeUrl(
        this.router.createUrlTree(['/games'], {
          queryParams: {
            search: this.searchTerm() || null,
            page: this.currentPage() === 1
            ? null : this.currentPage()
          },
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

    void this.router.navigate(['/games'], {
      queryParams: {
        search : search || null,
        page: null
      },
      queryParamsHandling: 'merge'
    });
  }
// Re-run the last fetch. This is needed because navigating to the same URL
// doesn't make queryParamMap emit again.
  retryGames(): void {
    this.retryRequest.next();
  }
}
