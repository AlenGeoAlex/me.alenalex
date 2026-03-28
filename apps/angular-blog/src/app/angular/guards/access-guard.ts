import {CanActivateChildFn, Router} from '@angular/router';
import {inject} from '@angular/core';
import {AuthStateService} from '@services/auth-state.service';

export const accessGuard: CanActivateChildFn = (childRoute, state) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  if(!authState.isAuthenticated){
    router.navigate(['/'])
      .catch(console.error);
    return false;
  }


  return true;
};
