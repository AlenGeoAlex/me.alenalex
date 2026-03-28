import {inject} from '@angular/core';
import {AuthStateService} from '@services/auth-state.service';

export async function appAuthStateInitializer() {
  const authStateService = inject(AuthStateService);
  await authStateService.reloadState();
}
