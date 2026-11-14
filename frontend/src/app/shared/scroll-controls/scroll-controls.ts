import { Component, DestroyRef, DOCUMENT, inject, signal } from '@angular/core';

@Component({
  imports: [],
  selector: 'app-scroll-controls',
  styleUrl: './scroll-controls.scss',
  templateUrl: './scroll-controls.html',
  host: {
    '(window:scroll)': 'onScroll()',
    '(window:resize)': 'hideControls()',
  },
})
export class ScrollControls {
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);
  private hideTimer?: ReturnType<typeof setTimeout>;

  readonly visible = signal(false);

  constructor() {
    this.destroyRef.onDestroy(() => {
      clearTimeout(this.hideTimer);
    });
  }

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

  onScroll(): void {
    const window = this.document.defaultView;
    const page = this.document.scrollingElement;

    if (
      !window ||
      !page ||
      window.innerWidth < 1200 ||
      page.scrollHeight <= page.clientHeight + 1
    ) {
      this.hideControls();
      return;
    }

    this.visible.set(true);

    clearTimeout(this.hideTimer);
    this.hideTimer = setTimeout(() => {
      this.visible.set(false);
    }, 1200);
  }

  hideControls(): void {
    clearTimeout(this.hideTimer);
    this.visible.set(false);
  }
}
