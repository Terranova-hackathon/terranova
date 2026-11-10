import { afterNextRender, Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth-service';
import { HttpErrorResponse } from '@angular/common/http';
import { safeReturnUrl } from '../../core/utils/return-url';

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  selector: 'app-register',
  styleUrl: './register.scss',
  templateUrl: './register.html',
})
export class Register {
  private readonly buildform = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly nameInput = viewChild.required<ElementRef<HTMLInputElement>>('nameInput');

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  constructor() {
		afterNextRender(() => this.nameInput().nativeElement.focus());
	}

  readonly form = this.buildform.nonNullable.group({
    displayName: [''],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [
      Validators.required, Validators.minLength(8),
      Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$/)
    ]],
    confirmPassword: ['', Validators.required],
    acceptedPrivacyPolicy: [false, Validators.requiredTrue]
  });

  onSubmit(): void {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }
    const values = this.form.getRawValue();

    if (values.password !== values.confirmPassword) {
      this.error.set('Passwords do not match.');
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    this.auth.register({
      email: values.email,
      password: values.password,
      displayName: values.displayName.trim() || null,
      acceptedPrivacyPolicy: values.acceptedPrivacyPolicy
    }).subscribe({
      next: () => {
        this.loading.set(false);
        const returnUrl = safeReturnUrl(
          this.route.snapshot.queryParamMap.get('returnUrl')
        );
        this.router.navigateByUrl(returnUrl);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 0) {
          this.error.set('Cannot connect to the server.');
        } else if (err.status === 400) {
          this.error.set('Check your credentials. Email may be already registered');
        } else {
          this.error.set('Registration failed, Please try again.');
        }
      }
    });
  }
}
