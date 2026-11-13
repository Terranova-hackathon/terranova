import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FavoriteService } from '../../core/services/favorite-service';
import { FavoriteModel } from '../../core/models/favorite.model';
import { GameCard } from '../../shared/game-card/game-card';
import { catchError, combineLatest, distinctUntilChanged, EMPTY, map, startWith, Subject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PagedResult } from '../../core/models/paged-result.model';
import { ScrollControls } from '../../shared/scroll-controls/scroll-controls';

@Component({
  imports: [RouterLink, GameCard, ScrollControls],
  selector: 'app-favorites',
  styleUrl: './favorites.scss',
  templateUrl: './favorites.html',
})
export class Favorites implements OnInit {
  private readonly favoriteService = inject(FavoriteService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly retryRequest = new Subject<void>();

  readonly pageSize = 20;

  favorites = signal<FavoriteModel[]>([]);
  totalCount = signal(0);
  currentPage = signal(1);
  totalPages = signal(0);
  loading = signal(false);
  error = signal('');
  removingIds = signal<Set<number>>(new Set());

  ngOnInit(): void {
    const page$ = this.route.queryParamMap.pipe(
      map(params => readPage(params.get('page'))),
      distinctUntilChanged()
    );

    combineLatest([
      page$,
      this.retryRequest.pipe(startWith(undefined))
    ]).pipe(
      map(([page]) => page),
      tap(page => {
        this.currentPage.set(page);
        this.loading.set(true);
        this.error.set('');
      }),
      switchMap(page =>
        this.favoriteService.getFavorites(page, this.pageSize).pipe(
          catchError(() => {
            this.loading.set(false);
            this.error.set('Unable to load favorites.');
            return EMPTY;
          })
        )
      ),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(result => this.applyResult(result));
  }

  loadFavorites(): void {
    this.retryRequest.next();
  }

  removeFavorite(id: number): void {
    if (this.removingIds().has(id)) return;

    this.removingIds.update(ids => new Set([...ids, id]));
    this.error.set('');

    this.favoriteService.removeFavorite(id).subscribe({
      next: () => {
        this.favorites.update(favorites => 
          favorites.filter(f => f.id !== id)
        );
        this.totalCount.update(count => count - 1);
        this.clearRemoving(id);
        this.refreshCurrentPage();
      },
      error: () => {
        this.clearRemoving(id);
        this.error.set('Unable to remove the game.');
      }
    });
  }
  private refreshCurrentPage(): void {
    const page = this.currentPage();
    this.favoriteService.getFavorites(page, this.pageSize).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: result => {
        if (page !== this.currentPage()) return;
        this.applyResult(result);
      },
      error: () => {}
    });
  }
  private clearRemoving(id: number): void {
    this.removingIds.update(ids => {
      const updated = new Set(ids);
      updated.delete(id);
      return updated;
    });
  }

  private applyResult(result: PagedResult<FavoriteModel>): void {
    if (result.items.length === 0 && result.page > 1) {
      this.navigateToPage(Math.max(result.totalPages, 1), true);
      return;
    }
    this.favorites.set(result.items);
    this.totalCount.set(result.totalCount);
    this.totalPages.set(result.totalPages);
    this.loading.set(false);
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages()
      || page === this.currentPage()) return;
    this.navigateToPage(page);
  }

  private navigateToPage(page: number, replaceUrl = false): void {
    void this.router.navigate(['/favorites'], {
      queryParams: { page: page === 1 ? null : page },
      queryParamsHandling: 'merge',
      replaceUrl
    });
  }
}

function readPage(raw: string | null): number {
  const page = Number(raw ?? '1');
  return Number.isSafeInteger(page) && page >= 1 ? page : 1;
}
