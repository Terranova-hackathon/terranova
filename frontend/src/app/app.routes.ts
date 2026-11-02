import { Routes } from '@angular/router';
import { Home } from './pages/home/home'
import { Games } from './pages/games/games';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { authGuard } from './core/guards/auth-guard';


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
