export interface FilterOption {
	value: string;
	name: string;
}

export interface GameFilter {
	genres: FilterOption[];
	platforms: FilterOption[];
	orderings: FilterOption[];
}

export interface CompanyOption {
	slug: string;
	name: string;
	gamesCount: number;
}

export interface GameQuery {
	search?: string;
	page?: number;
	pageSize?: number;
	genres?: string;
	platforms?: string;
	developers?: string;
	publishers?: string;
	ordering?: string;
	metacriticMin?: number;
	metacriticMax?: number;
}

export interface FilterState {
	genres: string[];
	platforms: string[];
	developers: string[];
	publishers: string[];
	ordering: string;
	metacriticMin: number | null;
	metacriticMax: number | null;
}

export type ListFilterKey = 'genres' | 'platforms' | 'developers' | 'publishers';

export function emptyFilterState(): FilterState {
	return {
	genres: [], platforms: [], developers: [], publishers: [],
	ordering: '', metacriticMin: null, metacriticMax: null
	};
}

const splitList = (v?: string) => (v ? v.split(',').filter(Boolean) : []);
const joinList  = (v: string[]) => (v.length ? v.join(',') : null);

export function toFilterState(query: GameQuery): FilterState {
	return {
		genres: splitList(query.genres),
		platforms: splitList(query.platforms),
		developers: splitList(query.developers),
		publishers: splitList(query.publishers),
		ordering: query.ordering ?? '',
		metacriticMin: query.metacriticMin ?? null,
		metacriticMax: query.metacriticMax ?? null
	};
}

export function toFilterParams(state: FilterState) {
	return {
		genres: joinList(state.genres),
		platforms: joinList(state.platforms),
		developers: joinList(state.developers),
		publishers: joinList(state.publishers),
		ordering: state.ordering || null,
		metacriticMin: 
			state.metacriticMin === null || state.metacriticMin === 0
				? null : state.metacriticMin,
		metacriticMax: 
			state.metacriticMax === null || state.metacriticMax === 100
				? null : state.metacriticMax
	};
}

const SLUG = /^[a-z0-9][a-z0-9_-]{0,79}$/;
const ID = /^[1-9]\d*$/;
export const MAX_VALUES = 10;

// Cleans a comma-separated list from the URL so the backend won't reject it:
// trims, lowercases, drops invalid values, removes duplicates, caps at 10.
// "Action, rpg,,action,bad!" → "action,rpg"
export function cleanList(raw: string | null, kind: 'slug' | 'id'): string {
	const pattern = kind === 'slug' ? SLUG : ID;
	const values = (raw ?? '')
		.split(',')
		.map(v => v.trim().toLowerCase())
		.filter(v => pattern.test(v));
	return [...new Set(values)].slice(0, MAX_VALUES).join(',');
}

const ORDERINGS = [
	'-rating',
	'-metacritic',
	'-released',
	'released',
	'-added',
	'name'];

export function cleanOrdering(raw: string | null): string {
	return raw && ORDERINGS.includes(raw) ? raw : '';
}