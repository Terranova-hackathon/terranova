import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth-service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
	imports: [ReactiveFormsModule, RouterLink],
	selector: 'app-login',
	styleUrl: './login.scss',
	templateUrl: './login.html',
})
export class Login {
	private readonly buildform = inject(FormBuilder);
	private readonly auth = inject(AuthService);
	private readonly router = inject(Router);
	private readonly route = inject(ActivatedRoute);

	readonly loading = signal(false);
	readonly error = signal<string | null>(null);

	readonly form = this.buildform.nonNullable.group({
		email: ['', [Validators.required, Validators.email]],
		password: ['', Validators.required]
	});

	onSubmit(): void {
		if (this.form.invalid || this.loading()) {
			this.form.markAllAsTouched();
			return;
		}
		this.error.set(null);
		this.loading.set(true);
// requestedUrl destination after login. The games page sets returnUrl when a guest tries
// to favorite a game, so we can bring them back to the same search/page/card.
// follows returnUrl if it's a /games URL (prevents open redirects); otherwise go to /games
// navigateByUrl because returnUrl is a full URL string that already includes
// its query params and fragment


		this.auth.login(this.form.getRawValue()).subscribe({
			next: () => {
				this.loading.set(false);
				const requestedUrl = 
					this.route.snapshot.queryParamMap.get('returnUrl');
				const returnUrl =
					requestedUrl && /^\/games(?:[?#]|$)/.test(requestedUrl)
						? requestedUrl : '/games';
				this.router.navigateByUrl(returnUrl);
			},
			error: (err: HttpErrorResponse) => {
				this.loading.set(false);
				if (err.status === 401) {
					this.error.set('Invalid email or password.');
				} else if (err.status === 0) {
					this.error.set('Cannot connect to the server.');
				} else if (err.status === 400) {
					this.error.set('Please check yout credentials.');
				} else {
					this.error.set('Login failed. Please try again.');
				}
			}
		});
	}
}
