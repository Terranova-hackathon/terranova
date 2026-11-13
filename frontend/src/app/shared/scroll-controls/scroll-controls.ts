import { Component, DOCUMENT, inject } from '@angular/core';

@Component({
  imports: [],
  selector: 'app-scroll-controls',
  styleUrl: './scroll-controls.scss',
  templateUrl: './scroll-controls.html',
})
export class ScrollControls {
  private readonly document = inject(DOCUMENT);

  scrollTo(position: 'top' | 'bottom'): void {
    const window = this.document.defaultView;
    const page = this.document.scrollingElement;

    if (!window || !page) return;

    const reducedMotion = window.matchMedia(
      '(prefers-reduced-motion: reduce)'
    ).matches;

    window.scrollTo({
      top: position === 'top' ? 0 : page.scrollHeight,
      behavior: reducedMotion ? 'instant' : 'smooth',
    });
  }
}
