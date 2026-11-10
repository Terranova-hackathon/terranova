import { afterRenderEffect, Component, ElementRef, input, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GameDetails } from '../../core/models/game.model';

@Component({
  imports: [RouterLink],
  selector: 'app-featured-game',
  styleUrl: './featured-game.scss',
  templateUrl: './featured-game.html',
})
export class FeaturedGame {
  games = input.required<GameDetails[]>();

  private readonly track = viewChild<ElementRef<HTMLElement>>('track');

  canScrollPrev = signal(false);
  canScrollNext = signal(false);

  constructor() {
    // Re-check the buttons whenever the games (and so the slides) change.
    afterRenderEffect(() => {
      this.games();
      this.updateScrollState();
    });
  }

  scrollPrev(): void {
    this.scrollByPage(-1);
  }

  scrollNext(): void {
    this.scrollByPage(1);
  }

  updateScrollState(): void {
    const el = this.track()?.nativeElement;
    if (!el) return;

    this.canScrollPrev.set(el.scrollLeft > 1);
    this.canScrollNext.set(el.scrollLeft + el.clientWidth < el.scrollWidth - 1);
  }

  releaseYear(released: string | null): string {
    return released?.slice(0, 4) ?? '';
  }

  private scrollByPage(direction: -1 | 1): void {
    const el = this.track()?.nativeElement;
    if (!el) return;

    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    el.scrollBy({
      left: direction * el.clientWidth,
      behavior: reduceMotion ? 'auto' : 'smooth',
    });
  }
}
