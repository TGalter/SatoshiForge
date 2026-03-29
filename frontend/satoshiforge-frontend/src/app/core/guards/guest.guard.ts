import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { AuthSessionService } from '../../features/auth/services/auth-session.service';

export const guestGuard: CanActivateFn = () => {
  const authSessionService = inject(AuthSessionService);
  const router = inject(Router);

  if (authSessionService.isRestoringSession()) {
    return false;
  }

  if (authSessionService.isAuthenticated()) {
    return router.createUrlTree(['/me']);
  }

  return true;
};