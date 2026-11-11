import { inject, Service, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { FavoriteModel } from '../models/favorite.model';
import { PagedResult } from '../models/paged-result.model';

@Service()
export class FavoriteService {
	private readonly http = inject(HttpClient);
	private readonly apiUrl = '/api/favorites';

	readonly favoriteIds = signal<Set<number>>(new Set());
	private pendingFavoriteId: number | null = null;

	setPendingFavorite(id: number): void {
		this.pendingFavoriteId = id;
	}

	takePendingFavorite(): number | null {
		const id = this.pendingFavoriteId;
		this.pendingFavoriteId = null;
		return id;
	}

	getFavorites(page = 1, pageSize = 20): Observable<PagedResult<FavoriteModel>> {
		return this.http.get<PagedResult<FavoriteModel>>(this.apiUrl, {
			params: { page, pageSize }
		});
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
