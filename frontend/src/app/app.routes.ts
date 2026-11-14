import { Routes } from '@angular/router';
import { Home } from './pages/home/home'
import { Games } from './pages/games/games';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { authGuard } from './core/guards/auth-guard';
import { Favorites } from './pages/favorites/favorites';
import { GameDetails } from './pages/game-details/game-details';
import { Privacy } from './pages/privacy/privacy';
import { UserAccount } from './pages/user-account/user-account';

import { About } from './pages/about/about';

export const routes: Routes = [
	{
		path: '',
		component: Home
	},
	{
// removed guard to allow guest users to navigate
		path: 'games',
		component: Games
	},
	{
		path: 'favorites',
		component: Favorites,
		canActivate: [authGuard]
	},
	{
		path: 'login',
		component: Login
	},
	{
		path: 'register',
		component: Register
	},
	{
		path: 'games/:id',
		component: GameDetails
	},
  {
    path: 'account',
		component: UserAccount,
		canActivate: [authGuard],
	},
  {
    path: 'privacy',
    component: Privacy
  },
	{
		path: 'about',
		component: About
	},
	{
		path: '**',
		redirectTo: ''
	}
];
