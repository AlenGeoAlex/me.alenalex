import {computed, effect, inject, Injectable} from '@angular/core';
import {BehaviorSubject, delay, tap} from 'rxjs';
import {toSignal} from '@angular/core/rxjs-interop';
import {Router} from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthStateService {

  private readonly _authState : BehaviorSubject<AuthState | null> = new BehaviorSubject<AuthState | null>(null);
  private readonly _authStateSignal = toSignal(this._authState, {initialValue: null});
  private readonly _isAuthenticatedSignal = computed(() => this._authStateSignal() !== null);
  private readonly router = inject(Router);
  constructor() {
    this.reloadState();
  }

  public get authState$() {
    return this._authState.asObservable();
  }

  public get authStateSignal() {
    return this._authStateSignal;
  }

  public async reloadState(options? : {
    redirect?: boolean,
  }) {
    console.log('Reloading state');
    const cookie = await window.cookieStore.get('bloggi-user-info')
    if(!cookie || !cookie.value) {
      this._authState.next(null);
      if(options?.redirect) {
        this.router.navigate(['/'])
          .catch(console.error)
      }
      return;
    }

    const userInfo = JSON.parse(decodeURIComponent(cookie.value));
    this._authState.next({
      email: userInfo.email,
      displayName: userInfo.displayName,
      avatarUrl: userInfo.avatarUrl,
    });
  }

  public get state() {
    return this._authState.value;
  }

  public get isAuthenticated() {
    return this.state !== null;
  }

  public get isAuthenticatedSignal() {
    return this._isAuthenticatedSignal;
  }

}

export interface AuthState {
  email: string;
  displayName: string;
  avatarUrl: string;
}
