import { Component, inject, OnInit, signal } from '@angular/core';
import { GameCard } from '../../shared/game-card/game-card';
import { GameModel } from '../../core/models/game.model';
import { GameService } from '../../core/services/game-service';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  imports: [GameCard, RouterLink],
  selector: 'app-games',
  styleUrl: './games.scss',
  templateUrl: './games.html',
})
export class Games implements OnInit {
  private readonly gameService = inject(GameService);
  readonly pageSize = 12;

  games = signal<GameModel[]>([]);
  currentPage = signal(1);
  totalPages = signal(0);
  totalGames = signal(0);
  loading = signal(false);
  error = signal('');

  ngOnInit(): void {
    this.loadGames();
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
}
