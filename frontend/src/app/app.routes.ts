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
		path: 'games',
		component: Games,
		canActivate: [authGuard]
	},
	{
		path: 'favorites',
		component: Favorites
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
