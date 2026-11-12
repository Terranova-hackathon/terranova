import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { GameService } from '../../core/services/game-service';
import { catchError, combineLatest, EMPTY, map, startWith, Subject, switchMap, tap } from 'rxjs';
import { GamesNavigationService } from '../../core/services/games-navigation-service';
import { GameDetailsModel } from '../../core/models/game.model';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DecimalPipe } from '@angular/common';

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

  readonly gamesNavigation = inject(GamesNavigationService);
  private pressStartedOnBackdrop = false;

  game = signal<GameDetailsModel | null>(null);
  loading = signal(true);
  error = signal('');
  selectedShot = signal<string | null>(null);

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
      this.loading.set(false);
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
}
