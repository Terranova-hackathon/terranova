import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { GamePriceComparison } from '../models/game-price.model';

@Service()
export class GamePriceService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/prices';

  fetchPrices(gameId: number): Observable<GamePriceComparison> {
    return this.http.get<GamePriceComparison>(
      `${this.apiUrl}/${gameId}`
    );
  }
}
