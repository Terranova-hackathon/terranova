import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth-service';
import { ThemeService } from '../../core/services/theme-service';
import { GamesNavigationService } from '../../core/services/games-navigation-service';

@Component({
  imports: [RouterLink, RouterLinkActive],
  selector: 'app-navbar',
  styleUrl: './navbar.scss',
  templateUrl: './navbar.html',
  host: {
    '(document:click)': 'onDocumentClick($event)',
    '(document:keydown.escape)': 'closeAccountMenu()'
  }
})
export class Navbar {
  private readonly router = inject(Router);

  readonly auth = inject(AuthService);
  readonly themeService = inject(ThemeService);
  readonly loggingOut = signal(false);
  readonly gamesNavigation = inject(GamesNavigationService);
  readonly accountOpen = signal(false);

  logout(): void {
    if (this.loggingOut()) return;
    this.loggingOut.set(true);
    this.auth.logout().subscribe({
      next: () => {
        this.closeAccountMenu();
        this.loggingOut.set(false);
        this.router.navigateByUrl('/login');
      },
      error: () => {
        this.loggingOut.set(false);
      }
    });
  }

  toggleAccountMenu(): void {
    this.accountOpen.update(open => !open);
  }

  closeAccountMenu(): void {
    this.accountOpen.set(false);
  }

  openAdvancedSettings(): void {
    this.closeAccountMenu();
    void this.router.navigate(['/account']);
  }
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as Node;

    if (!this.accountMenu()?.nativeElement.contains(target)) {
      this.closeAccountMenu();
    }
  }
  private readonly accountMenu =
    viewChild<ElementRef<HTMLElement>>('accountMenu');
}
