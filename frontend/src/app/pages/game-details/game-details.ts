import { Component, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { GameService } from '../../core/services/game-service';
import { catchError, combineLatest, EMPTY, map, startWith, Subject, switchMap, tap } from 'rxjs';
import { GamesNavigationService } from '../../core/services/games-navigation-service';
import { GameDetailsModel, GameModel } from '../../core/models/game.model';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DecimalPipe } from '@angular/common';
import { PagedResult } from '../../core/models/paged-result.model';
import { AuthService } from '../../core/services/auth-service';
import { FavoriteService } from '../../core/services/favorite-service';

@Component({
  imports: [RouterLink, DecimalPipe],
  selector: 'app-game-details',
  styleUrl: './game-details.scss',
  templateUrl: './game-details.html',
  host: { '(document:keydown.escape)': 'goBack()' },
})
export class GameDetails implements OnInit {
  private readonly gameService = inject(GameService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly retryRequest = new Subject<void>();
  private readonly relatedPageRequests = new Subject<number>();
  private readonly auth = inject(AuthService);
  private readonly favoriteService = inject(FavoriteService);

  readonly gamesNavigation = inject(GamesNavigationService);
  private pressStartedOnBackdrop = false;
  readonly relatedPageSize = 6;
  readonly isLoggedIn = this.auth.isLoggedIn;
  readonly favoriteIds = this.favoriteService.favoriteIds;

  game = signal<GameDetailsModel | null>(null);
  loading = signal(true);
  error = signal('');
  selectedShot = signal<string | null>(null);
  relatedGames = signal<PagedResult<GameModel> | null>(null);
  relatedLoading = signal(false);
  relatedError = signal('');
  favoritesReady = signal(false);
  favoriteBusyIds = signal<Set<number>>(new Set());
  favoriteError = signal('');

  constructor() {
    effect(onCleanup => {
      if (!this.isLoggedIn()) {
        this.favoritesReady.set(false);
        this.favoriteError.set('');
        this.favoriteService.clear();
        return;
      }

      this.favoritesReady.set(false);
      this.favoriteError.set('');

      const subscription = this.favoriteService
        .loadFavoriteIds()
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

      onCleanup(() => subscription.unsubscribe());
    });
  }

  ngOnInit(): void {
    const id$ = this.route.paramMap.pipe(
      map(params => Number(params.get('id')))
    );
    combineLatest([id$, this.retryRequest.pipe(startWith(undefined))]).pipe(
      map(([id]) => id),
      tap(() => {
        this.loading.set(true);
        this.error.set('');
        this.selectedShot.set(null);
        this.game.set(null);
        this.relatedGames.set(null);
        this.relatedLoading.set(false);
        this.relatedError.set('');
      }),
      switchMap(id => 
        this.gameService.fetchGameDetails(id).pipe(
          catchError((err: HttpErrorResponse) => {
            this.loading.set(false);
            this.game.set(null);
            this.error.set(err.status === 404 ? 'Game not found.'
              : 'Unable to load this game. Please try again.');
            return EMPTY;
          })
        )
      ),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(game => {
      this.game.set(game);
      this.relatedGames.set(game.relatedGames);
      this.loading.set(false);
      this.completePendingFavorite();
    });

    this.relatedPageRequests.pipe(
      switchMap(page => {
        const currentGame = this.game();

        if (!currentGame) {
          return EMPTY;
        }

        this.relatedLoading.set(true);
        this.relatedError.set('');

        return this.gameService.fetchGameDetails(
          currentGame.id,
          page,
          this.relatedPageSize
        ).pipe(
          map(response => ({
            gameId: currentGame.id,
            related: response.relatedGames
          })),

          catchError(() => {
            if (this.game()?.id === currentGame.id) {
              this.relatedLoading.set(false);
              this.relatedError.set(
                'Could not load related games. Please try again.'
              );
            }

            return EMPTY;
          })
        );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(({ gameId, related }) => {
      if (this.game()?.id !== gameId) {
        return;
      }
      this.relatedGames.set(related);
      this.relatedLoading.set(false);
    });

  }
  retry(): void {
    this.retryRequest.next();
  }
  metacriticClass(score: number): string {
	  return score >= 75 ? 'good' : score >= 50 ? 'mixed' : 'bad';
  }
  onBackdropPointerDown(event: PointerEvent): void {
  this.pressStartedOnBackdrop = this.isBackdrop(event);
}

  onBackdropClick(event: MouseEvent): void {
    if (this.pressStartedOnBackdrop && this.isBackdrop(event)) {
      this.goBack();
    }
    this.pressStartedOnBackdrop = false;
  }

  goBack(): void {
    this.router.navigateByUrl(this.gamesNavigation.gamesUrl());
  }

  private isBackdrop(event: Event): boolean {
    const target = event.target as HTMLElement;
    return target === event.currentTarget
      || target.classList.contains('game-details__container');
  }

  changeRelatedPage(page: number): void {
    const related = this.relatedGames();

    if (!this.game() || !related || this.relatedLoading()) {
      return;
    }

    if (
      !Number.isSafeInteger(page) ||
      page < 1 ||
      page > related.totalPages ||
      page === related.page
    ) {
      return;
    }

    this.relatedPageRequests.next(page);
  }

  toggleFavorite(id: number): void {
    if (!this.isLoggedIn()) {
      this.favoriteService.setPendingFavorite(id);

      const returnUrl = this.router.serializeUrl(
        this.router.createUrlTree(['/games', id])
      );

      void this.router.navigate(['/login'], {
        queryParams: { returnUrl }
      });

      return;
    }

    if (!this.favoritesReady()) {
      return;
    }

    if (this.favoriteBusyIds().has(id)) {
      return;
    }

    this.favoriteError.set('');

    this.favoriteBusyIds.update(ids =>
      new Set([...ids, id])
    );

    const request = this.favoriteIds().has(id)
      ? this.favoriteService.removeFavorite(id)
      : this.favoriteService.addFavorite(id);

    request.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.clearFavoriteBusy(id);
      },

      error: (err: HttpErrorResponse) => {
        this.clearFavoriteBusy(id);

        // Avoid displaying an old game's error
        // after navigating to another game.
        if (this.game()?.id !== id) {
          return;
        }

        if (err.status === 409) {
          this.favoriteError.set(
            'You have reached your Favorites limit.'
          );
        } else if (err.status === 401) {
          this.favoriteError.set(
            'Your session has expired. Please log in again.'
          );
        } else {
          this.favoriteError.set(
            'Could not update Favorites. Please try again.'
          );
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

  private completePendingFavorite(): void {
    const currentGame = this.game();

    if (!this.isLoggedIn() || !this.favoritesReady()) {
      return;
    }

    if (!currentGame) {
      return;
    }

    const pendingId =
      this.favoriteService.takePendingFavorite();

    if (pendingId !== currentGame.id) {
      return;
    }

    if (this.favoriteIds().has(pendingId)) {
      return;
    }

    this.toggleFavorite(pendingId);
  }

}
