import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedin()) {
    return true;
  }
  return auth.getCurrentUser().pipe(
    map(user => user !== null ? true : 
      router.createUrlTree(['/login']))
  );
};
