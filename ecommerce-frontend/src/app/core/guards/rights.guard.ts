import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
export const rightsGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // First login chesara check
  if (!authService.isLoggedIn()) {
    router.navigate(['/auth/login']);
    return false;
  }

  // Route lo requiredRight set chesara check
  const requiredRight = route.data['requiredRight'];

  // requiredRight lekapothe — just login check chaalu
  if (!requiredRight) return true;

  // Right undo chuddam
  if (authService.hasRight(requiredRight)) {
    return true;
  }

  // Right ledu → unauthorized
  router.navigate(['/unauthorized']);
  return false;
};