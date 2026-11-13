import { Component, inject, input, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { GamePriceComparison } from '../../core/models/game-price.model';
import { GamePriceService } from '../../core/services/game-price-service';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { catchError, distinctUntilChanged, of, switchMap, tap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  imports: [CurrencyPipe],
  selector: 'app-price-comparison',
  styleUrl: './price-comparison.scss',
  templateUrl: './price-comparison.html',
})
export class PriceComparison {
  private readonly priceService = inject(GamePriceService);

  gameId = input.required<number>();

  prices = signal<GamePriceComparison | null>(null);
  loading = signal(true);
  error = signal('');

  constructor() {
    toObservable(this.gameId).pipe(
      distinctUntilChanged(),
      tap(() => {
        this.loading.set(true);
        this.error.set('');
        this.prices.set(null);
      }),
      switchMap(id =>
        this.priceService.fetchPrices(id).pipe(
          catchError((err: HttpErrorResponse) => {
            this.error.set(
              err.status === 404
                ? 'No pricing information available.'
                : 'Unable to load prices.'
            );
            return of(null);
          })
        )
      ),
      takeUntilDestroyed()
    ).subscribe(result => {
      this.prices.set(result);
      this.loading.set(false);
    });
  }

  validStoreUrl(url: string | null): string | null {
    if (!url) return null;

    return /^https?:\/\//i.test(url) ? url : null;
  }
}
