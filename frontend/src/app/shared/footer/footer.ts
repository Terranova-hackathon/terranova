import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ThemeService } from '../../core/services/theme-service';

@Component({
  imports: [RouterLink, RouterLinkActive],
  selector: 'app-footer',
  styleUrl: './footer.scss',
  templateUrl: './footer.html',
})
export class Footer {
  private readonly router = inject(Router);
  readonly themeService = inject(ThemeService);
  readonly loggingOut = signal(false);

}
