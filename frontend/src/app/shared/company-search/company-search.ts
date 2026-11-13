import { Component, computed, inject, input, output, signal } from '@angular/core';
import { GameService } from '../../core/services/game-service';
import { CompanyOption, MAX_VALUES } from '../../core/models/game-filter.model';
import { catchError, debounceTime, distinctUntilChanged, map, of, Subject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  imports: [],
  selector: 'app-company-search',
  styleUrl: './company-search.scss',
  templateUrl: './company-search.html',
})
export class CompanySearch {
  private readonly gameService = inject(GameService);

  // INPUTS: set by GameFilters
  kind = input.required<'developer' | 'publisher'>();
  selected = input<string[]>([]);

  // OUTPUT: "the user clicked this slug". The parent decides add or remove.
  toggled = output<string>();

  term = signal('');
  results = signal<CompanyOption[]>([]);
  loading = signal(false);
  error = signal('');
  isFull = computed(() => this.selected().length >= MAX_VALUES);

  // Every keystroke goes in here
  private readonly typed = new Subject<string>();
  // slug -> display name, remembered from search results
  private readonly names = signal(new Map<string, string>());

  constructor() {
    this.typed.pipe(
      map(value => value.trim()),
      debounceTime(300),
      distinctUntilChanged(),
      tap(term => {
        this.term.set(term);
        this.error.set('');
        this.loading.set(term.length >= 2);
      }),
      switchMap(term =>
        term.length < 2
          ? of([])
          : this.gameService.fetchCompanies(this.kind(), term).pipe(
              catchError(() => {
                this.error.set('Could not search. Try again.');
                return of([]);
              })
            )
      ),
      takeUntilDestroyed()
    ).subscribe(results => {
      this.results.set(results);
      this.loading.set(false);
    });
  }

  onType(value: string): void {
    this.typed.next(value);
  }

  pick(company: CompanyOption): void {
    this.names.update(m => new Map(m).set(company.slug, company.name));
    this.toggled.emit(company.slug);
  }

  isSelected(slug: string): boolean {
    return this.selected().includes(slug);
  }

  // Falls back to the slug made readable when the page was loaded from a URL
  displayName(slug: string): string {
    return this.names().get(slug) ?? slug.replace(/-/g, ' ');
  }
}
