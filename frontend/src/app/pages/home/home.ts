import { Component, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { toFilterParams } from '../../core/models/game-filter.model';
import { GameFilters } from '../../shared/game-filters/game-filters';
import { FeaturedGame } from '../../shared/featured-game/featured-game';
import { GameService } from '../../core/services/game-service';
import { GameDetails, GameModel } from '../../core/models/game.model';
import { catchError, debounce, debounceTime, distinctUntilChanged, map, of, Subject, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  imports: [FormsModule, GameFilters, FeaturedGame],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {
  private readonly router = inject(Router);
  private readonly filtersPanel = viewChild(GameFilters);
  private readonly gameService = inject(GameService);
  private readonly searchTyped = new Subject<string>();

  suggestions = signal<GameModel[]>([]);
  featuredGames = signal<GameDetails[]>([]);
  featuredLoading = signal(true);
  featuredError = signal(false);
  searchInput = '';

  constructor() {
    this.loadFeatured();

    this.searchTyped.pipe(
      map(value => value.trim()),
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(search =>
        search.length < 2 ? of([])
          : this.gameService.fetchGames({ search, pageSize: 5 }).pipe(
            map(res => res.items),
            catchError(() => of([]))
          )
      ),
      takeUntilDestroyed()
    ).subscribe(games => this.suggestions.set(games));
  }

  loadFeatured(): void {
    this.featuredLoading.set(true);
    this.featuredError.set(false);

    this.gameService.fetchFeatured().subscribe({
      next: (games) => {
        this.featuredGames.set(games);
        this.featuredLoading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.featuredError.set(err.status !== 404);
        this.featuredLoading.set(false);
      }
    });
  }

  onSearchInput(value: string): void {
    this.searchTyped.next(value);
  }

  pickSuggestion(game: GameModel): void {
    this.searchInput = game.name;
    this.submitSearch();
  }

  submitSearch(): void {
    const draft = this.filtersPanel()?.filterState;
    void this.router.navigate(['/games'], {
      queryParams: {
        search: this.searchInput.trim() || null,
        ...(draft ? toFilterParams(draft) : {})
      }
    });
  }
}
