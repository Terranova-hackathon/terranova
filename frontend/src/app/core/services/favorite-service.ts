import { inject, Service, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { FavoriteModel } from '../models/favorite.model';

@Service()
export class FavoriteService {
	private readonly http = inject(HttpClient);
	private readonly apiUrl = '/api/favorites';

	readonly favoriteIds = signal<Set<number>>(new Set());

	getFavorites(): Observable<FavoriteModel[]> {
		return this.http.get<FavoriteModel[]>(this.apiUrl);
	}

	loadFavoriteIds(): Observable<number[]> {
		return this.http.get<number[]>(
			`${this.apiUrl}/ids`
		).pipe(
			tap(ids => this.favoriteIds.set(new Set(ids)))
		);
	}

	isFavorite(id: number): boolean {
		return this.favoriteIds().has(id);
	}

	clear(): void {
		this.favoriteIds.set(new Set());
	}

	addFavorite(id: number): Observable<void> {
		return this.http.put<void>(
			`${this.apiUrl}/${id}`, {}
		).pipe(
			tap(() => {
				this.favoriteIds.update(ids => {
					const updated = new Set(ids);
					updated.add(id);
					return updated;
				});
			})
		);
	}

	removeFavorite(id: number): Observable<void> {
		return this.http.delete<void>(
			`${this.apiUrl}/${id}`
		).pipe(
			tap(() => {
				this.favoriteIds.update(ids => {
					const updated = new Set(ids);
					updated.delete(id);
					return updated;
				});
			})
		);
	}
}
