import { Routes } from '@angular/router';
import { Home } from './pages/home/home'
import { Games } from './pages/games/games';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { authGuard } from './core/guards/auth-guard';
import { Favorites } from './pages/favorites/favorites';


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
		path: '**',
		redirectTo: ''
	}
];
