import { Service, signal } from '@angular/core';

export type Theme = 'dark' | 'light'

@Service()
export class ThemeService {
	readonly theme = signal<Theme>('dark');

	constructor() {
		const savedTheme = localStorage.getItem('theme');

		if (savedTheme === 'dark' || savedTheme === 'light') {
			this.theme.set(savedTheme);
		}
		this.applyTheme();
	}

	toggleTheme(): void {
		const nextTheme = this.theme() === 'dark'
			? 'light'
			: 'dark';

		this.theme.set(nextTheme);
		localStorage.setItem('theme', nextTheme);
		this.applyTheme();
	}

	private applyTheme(): void {
		document.documentElement.setAttribute(
			'data-bs-theme',
			this.theme()
		);
	}
}
