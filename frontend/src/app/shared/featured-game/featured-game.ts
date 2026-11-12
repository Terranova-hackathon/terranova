import { afterRenderEffect, Component, ElementRef, input, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GameDetailsModel } from '../../core/models/game.model';

@Component({
  imports: [RouterLink],
  selector: 'app-featured-game',
  styleUrl: './featured-game.scss',
  templateUrl: './featured-game.html',
})
export class FeaturedGame {
  games = input.required<GameDetailsModel[]>();

  private readonly track = viewChild<ElementRef<HTMLElement>>('track');

  activeIndex = signal(0);

  constructor() {
    afterRenderEffect(() => {
      this.games();
      this.updateScrollState();
    });
  }

  scrollToIndex(index: number): void {
    const el = this.track()?.nativeElement;
    if (!el) return;

    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    el.scrollTo({
      left: index * el.clientWidth,
      behavior: reduceMotion ? 'auto' : 'smooth',
    });
  }

  updateScrollState(): void {
    const el = this.track()?.nativeElement;
    if (!el || el.clientWidth === 0) return;

    this.activeIndex.set(Math.round(el.scrollLeft / el.clientWidth));
  }

  releaseYear(released: string | null): string {
    return released?.slice(0, 4) ?? '';
  }
}
