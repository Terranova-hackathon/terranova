import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FavoriteService } from '../../core/services/favorite-service';
import { FavoriteModel } from '../../core/models/favorite.model';
import { GameCard } from '../../shared/game-card/game-card';
import { favoriteToGame } from '../../core/models/favorite.mapper';

@Component({
  imports: [RouterLink, GameCard],
  selector: 'app-favorites',
  styleUrl: './favorites.scss',
  templateUrl: './favorites.html',
})
export class Favorites implements OnInit {
  private readonly favoriteService = inject(FavoriteService);

  favorites = signal<FavoriteModel[]>([]);
  games = computed(() => this.favorites().map(favoriteToGame));
  loading = signal(false);
  error = signal('');
  removingIds = signal<Set<number>>(new Set());

  ngOnInit(): void {
    this.loadFavorites();
  }

  loadFavorites(): void {
    this.loading.set(true);
    this.error.set('');

    this.favoriteService.getFavorites().subscribe({
      next: (favorites) => {
        this.favorites.set(favorites);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Unable to load favorites.');
      }
    });
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
        this.clearRemoving(id);
      },
      error: () => {
        this.clearRemoving(id);
        this.error.set('Unable to remove the game.');
      }
    });
  }
  private clearRemoving(id: number): void {
    this.removingIds.update(ids => {
      const updated = new Set(ids);
      updated.delete(id);
      return updated;
    });
  }
}
