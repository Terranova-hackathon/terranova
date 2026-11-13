import { Component, DestroyRef, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { GameService } from '../../core/services/game-service';
import {
  emptyFilterState, FilterState, GameFilter, ListFilterKey, MAX_VALUES
} from '../../core/models/game-filter.model';
import { CompanySearch } from '../company-search/company-search';

@Component({
  selector: 'app-game-filters',
  imports: [FormsModule, CompanySearch],
  templateUrl: './game-filters.html',
  styleUrl: './game-filters.scss',
})
export class GameFilters {
  private readonly gameService = inject(GameService);
  private readonly destroyRef = inject(DestroyRef);

  // INPUTS: data the parent page passes in
  initialState = input<FilterState>(emptyFilterState());
  active = input(false);

  // OUTPUTS: events the panel sends back to the parent page
  applied = output<FilterState>();
  cleared = output<void>();

  readonly maxValues = MAX_VALUES;
  filterOptions = signal<GameFilter | null>(null);
  filtersLoading = signal(false);
  filtersError = signal('');
  filterState: FilterState = emptyFilterState();

  constructor() {
    // Whenever the parent passes a new initialState (e.g. the URL changed),
    // reset the draft to match it.
    effect(() => {
      this.filterState = { ...this.initialState() };
    });
    this.loadFilterOptions();
  }

  loadFilterOptions(): void {
    this.filtersLoading.set(true);
    this.filtersError.set('');
    this.gameService.fetchFilterOptions().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: options => {
        this.filterOptions.set(options);
        this.filtersLoading.set(false);
      },
      error: () => {
        this.filtersLoading.set(false);
        this.filtersError.set('Unable to load filter options.');
      }
    });
  }

  isSelected(key: ListFilterKey, value: string): boolean {
    return this.filterState[key].includes(value);
  }

  toggleFilter(key: ListFilterKey, value: string): void {
    const list = this.filterState[key];
    if (list.includes(value)) {
      this.filterState[key] = list.filter(v => v !== value);
    } else if (list.length < MAX_VALUES) {
      this.filterState[key] = [...list, value];
    }
  }

  submitFilters(): void {
    this.applied.emit(this.filterState);
  }

  resetFilters(): void {
    this.filterState = emptyFilterState();
    this.cleared.emit();
  }
// Moving min above max pushes max up with it
setMetacriticMin(value: number): void {
  this.filterState.metacriticMin = value;
  const max = this.filterState.metacriticMax;
  if (max !== null && value > max) {
    this.filterState.metacriticMax = value;
  }
}

// Moving max below min pushes min down with it
setMetacriticMax(value: number): void {
  this.filterState.metacriticMax = value;
  const min = this.filterState.metacriticMin;
  if (min !== null && value < min) {
    this.filterState.metacriticMin = value;
  }
}
}
