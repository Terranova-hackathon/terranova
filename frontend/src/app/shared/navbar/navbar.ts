import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth-service';
import { ThemeService } from '../../core/services/theme-service';
import { GamesNavigationService } from '../../core/services/games-navigation-service';

@Component({
  imports: [RouterLink, RouterLinkActive],
  selector: 'app-navbar',
  styleUrl: './navbar.scss',
  templateUrl: './navbar.html',
})
export class Navbar {
  private readonly router = inject(Router);

  readonly auth = inject(AuthService);
  readonly themeService = inject(ThemeService);
  readonly loggingOut = signal(false);
  readonly gamesNavigation = inject(GamesNavigationService);

  logout(): void {
    if (this.loggingOut()) return;
    this.loggingOut.set(true);
    this.auth.logout().subscribe({
      next: () => {
        this.loggingOut.set(false);
        this.router.navigateByUrl('/login');
      },
      error: () => {
        this.loggingOut.set(false);
      }
    });
  }
}
