import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { AuthService } from '../../core/services/auth-service';
import { FavoriteService } from '../../core/services/favorite-service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  imports: [],
  selector: 'app-user-account',
  styleUrl: './user-account.scss',
  templateUrl: './user-account.html',
})
export class UserAccount {
  private readonly authService = inject(AuthService);
  private readonly favoriteService = inject(FavoriteService);
  private readonly router = inject(Router);

  private readonly deleteDialog =
    viewChild.required<ElementRef<HTMLDialogElement>>(
      'deleteDialog'
    );

  readonly user = this.authService.user;

  deleting = signal(false);
  error = signal('');

  openDeleteDialog(): void {
    this.error.set('');
    this.deleteDialog().nativeElement.showModal();
  }

  closeDeleteDialog(): void {
    if (this.deleting()) return;

    this.deleteDialog().nativeElement.close();
    this.error.set('');
  }

  onDialogCancel(event: Event): void {
    if (this.deleting()) {
      event.preventDefault();
    }
  }

  confirmDeletion(): void {
    if (this.deleting()) return;

    this.deleting.set(true);
    this.error.set('');

    this.authService.deleteAccount().subscribe({
      next: () => {
        this.favoriteService.clear();
        this.deleteDialog().nativeElement.close();
        this.deleting.set(false);

        void this.router.navigateByUrl('/');
      },
      error: (err: HttpErrorResponse) => {
        this.deleting.set(false);

        if (err.status === 401) {
          this.error.set(
            'Your session has expired. Please log in again.'
          );
        } else if (err.status === 0) {
          this.error.set(
            'Cannot connect to the server.'
          );
        } else {
          this.error.set(
            'Could not delete your account. Please try again.'
          );
        }
      }
    });
  }
}
