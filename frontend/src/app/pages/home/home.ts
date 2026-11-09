import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FilterState, toFilterParams } from '../../core/models/game-filter.model';
import { GameFilters } from '../../shared/game-filters/game-filters';

@Component({
  imports: [FormsModule, GameFilters],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {
  private readonly router = inject(Router);
  searchInput = '';

  submitSearch(): void {
    const search = this.searchInput.trim();
    void this.router.navigate(['/games'], {
      queryParams: { search: search || null }
    });
  }
  applyFilters(state: FilterState): void {
    void this.router.navigate(['/games'], {
      queryParams: {
        search: this.searchInput.trim() || null,
        ...toFilterParams(state)
      }
    });
  }
}
