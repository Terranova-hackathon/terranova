import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { AuthService } from '../../core/services/auth-service';
import { FavoriteService } from '../../core/services/favorite-service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  imports: [FormsModule],
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

  editingName = signal(false);
  displayNameDraft = signal('');
  savingName = signal(false);
  nameError = signal('');
  nameSuccess = signal('');

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
  startEditingName(): void {
    this.displayNameDraft.set(this.user()?.displayName ?? '');
    this.nameError.set('');
    this.nameSuccess.set('');
    this.editingName.set(true);
  }

  cancelEditingName(): void {
    if (this.savingName()) return;

    this.editingName.set(false);
    this.nameError.set('');
  }

  saveDisplayName(): void {
    const name = this.displayNameDraft().trim();

    if (this.savingName() || name.length < 2 || name.length > 50) {
      return;
    }

    if (name === this.user()?.displayName) {
      this.editingName.set(false);
      return;
    }

    this.savingName.set(true);
    this.nameError.set('');
    this.nameSuccess.set('');

    this.authService.updateDisplayName(name).subscribe({
      next: () => {
        this.savingName.set(false);
        this.editingName.set(false);
        this.nameSuccess.set('Display name updated successfully.');
      },
      error: (err: HttpErrorResponse) => {
        this.savingName.set(false);

        if (err.status === 401) {
          this.nameError.set('Your session has expired.');
        } else if (err.status === 400) {
          this.nameError.set('Please enter a valid display name.');
        } else {
          this.nameError.set('Could not update your display name.');
        }
      }
    });
  }
}
