import { ParamMap } from '@angular/router';
import { cleanList, cleanOrdering, GameQuery } from '../models/game-filter.model';

// Makes sure a metacritic score is a whole number from 0 to 100
function readScore(value: string | null, fallback: number): number {
  if (value === null || value.trim() === '') return fallback;
  const score = Number(value);
  if (!Number.isInteger(score) || score < 0 || score > 100) return fallback;
  return score;
}

// Turns a URL query string into a clean GameQuery object
export function readGameQuery(params: ParamMap): GameQuery {
  const rawPage = Number(params.get('page') ?? '1');
  const page = Number.isSafeInteger(rawPage) && rawPage >= 1 ? rawPage : 1;
  const min = params.get('metacriticMin');
  const max = params.get('metacriticMax');
  let metacriticMin = min === null ? undefined : readScore(min, 0);
  let metacriticMax = max === null ? undefined : readScore(max, 100);
  // A reversed range (80-50) is treated as 50-80 instead of an error
  if (metacriticMin !== undefined && metacriticMax !== undefined
      && metacriticMin > metacriticMax) {
    [metacriticMin, metacriticMax] = [metacriticMax, metacriticMin];
  }
  // 0 and 100 aren't restrictions, so games with no metacritic aren't excluded
  if (metacriticMin === 0) metacriticMin = undefined;
  if (metacriticMax === 100) metacriticMax = undefined;

  return {
    search: params.get('search')?.trim() ?? '', page,
    genres: cleanList(params.get('genres'), 'slug'),
    platforms: cleanList(params.get('platforms'), 'id'),
    developers: cleanList(params.get('developers'), 'slug'),
    publishers: cleanList(params.get('publishers'), 'slug'),
    ordering: cleanOrdering(params.get('ordering')),
    metacriticMin,
    metacriticMax
  };
}
