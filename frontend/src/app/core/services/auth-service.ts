import { computed, inject, Service, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LoginRequest, RegisterRequest, User } from '../models/auth.model';
import { catchError, Observable, tap, of } from 'rxjs';

@Service()
export class AuthService {
	private readonly http = inject(HttpClient);
	private readonly apiUrl = '/api/auth';

	private readonly currentUser = signal<User | null>(null);
	readonly user = this.currentUser.asReadonly();
	readonly isLoggedIn = computed(() => this.user() !== null);

	login(credentials: LoginRequest): Observable<User> {
		return this.http.post<User>(
			`${this.apiUrl}/login`,
			credentials
		).pipe(
			tap(user => this.currentUser.set(user))
		);
	}
	register(data: RegisterRequest): Observable<User | null> {
		return this.http.post<User>(
			`${this.apiUrl}/register`,
			data
		).pipe(
			tap(user => this.currentUser.set(user))
		);
	}
	getCurrentUser(): Observable<User | null> {
		return this.http.get<User>(
			`${this.apiUrl}/me`
		).pipe(
			tap(user => this.currentUser.set(user)),
			catchError(() => {
				this.currentUser.set(null);
				return of(null);
			})
		);
	}
	logout(): Observable<void> {
		return this.http.post<void>(
			`${this.apiUrl}/logout`,
			{}
		).pipe(
			tap(() => this.currentUser.set(null))
		);
	}
}
