import { Component, inject, OnInit, signal } from '@angular/core';
import { GameCard } from '../../shared/game-card/game-card';
import { GameModel } from '../../core/models/game.model';
import { GameService } from '../../core/services/game-service';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { FavoriteService } from '../../core/services/favorite-service';

@Component({
  imports: [GameCard, RouterLink],
  selector: 'app-games',
  styleUrl: './games.scss',
  templateUrl: './games.html',
})
export class Games implements OnInit {

  private readonly gameService = inject(GameService);
  private readonly favoriteService = inject(FavoriteService);

  readonly pageSize = 12;
  readonly favoriteIds = this.favoriteService.favoriteIds;

  games = signal<GameModel[]>([]);
  currentPage = signal(1);
  totalPages = signal(0);
  totalGames = signal(0);
  loading = signal(false);
  error = signal('');
  favoriteBusyIds = signal<Set<number>>(new Set());
  favoriteError = signal('');

  ngOnInit(): void {
    this.loadGames();
    this.loadFavorites();
  }

  loadGames(): void {
    if (this.loading()) return;

    this.loading.set(true);
    this.error.set('');

    this.gameService.fetchGames(this.currentPage(), this.pageSize).subscribe({
      next: (response) => {
        this.games.set(response.items);
        this.currentPage.set(response.page);
        this.totalPages.set(response.totalPages);
        this.totalGames.set(response.totalCount);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(
          err.status === 401
            ? 'No session found. Please log in'
            : 'Unable to load Games, Please try again.'
        );
      }
    });
  }

  changePage(page: number): void {
    if (this.loading()) return;
    if (page < 1 || page > this.totalPages()) return;
    if (page === this.currentPage()) return;

    this.currentPage.set(page);
    this.loadGames();
  }

  loadFavorites(): void {
    this.favoriteService.loadFavoriteIds().subscribe({
      error: () => {
        this.favoriteError.set(
          'Could not load your favorite games.'
        );
      }
    });
  }

  toggleFavorite(id: number): void {
    if (this.favoriteBusyIds().has(id)) return;

    this.favoriteError.set('');
    this.favoriteBusyIds.update(ids => new Set([...ids, id]));

    const request = this.favoriteIds().has(id) ? this.favoriteService.removeFavorite(id)
      : this.favoriteService.addFavorite(id);
    request.subscribe({
      next: () => {
        this.clearFavoriteBusy(id);
      },
      error: (err: HttpErrorResponse) => {
        this.clearFavoriteBusy(id);

        if (err.status === 409) {
          this.favoriteError.set('You have reached Favorites limit.');
        } else if (err.status === 401) {
          this.favoriteError.set('Please log in to browse your favorite games.');
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
}
