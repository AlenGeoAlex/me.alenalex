import {Component, inject, signal} from '@angular/core';
import { ProgressSpinner } from 'primeng/progressspinner';
import {HotToastService} from '@ngxpert/hot-toast';
import {AuthService} from '@services/api/generated-sdk';
import {ActivatedRoute, Router} from '@angular/router';
import {asProblemDetailsAsync} from '@utils/http-utils';
import {AuthStateService} from '@services/auth-state.service';

@Component({
  selector: 'bloggi-google-callback',
  imports: [
    ProgressSpinner
  ],
  templateUrl: './google-callback.html',
  styleUrl: './google-callback.scss',
})
export class GoogleCallback {

  private readonly authService = inject(AuthService);
  private readonly toastService = inject(HotToastService);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly authState = inject(AuthStateService);
  protected readonly status = signal<'success' | 'loading' | string>('loading');
  private readonly router = inject(Router);

  constructor() {
    const code = this.activatedRoute.snapshot.queryParamMap.get('code');
    const state = this.activatedRoute.snapshot.queryParamMap.get('state');
    if (!code || !state) {
      this.toastService.error('Invalid login attempt.');
      return;
    }

    this.status.set('loading')
    this.authService.login({code, state})
      .subscribe({
        next: (response) => {
          this.status.set('success')
          this.authState.reloadState();
          setTimeout(() => {
            this.router.navigate(['/', 'admin'])
              .catch(console.error)
          }, 2500)
        },
        error: (error) => {
          asProblemDetailsAsync(error)
            .then((problemDetails) => {
              if(problemDetails.code === 'Auth.RegistrationDisabled'){
                this.status.set('Sorry, Blog Management is limited to the author. Your PI  are not preserved after this request.')
              } else if (problemDetails.code === 'Auth.InSufficientPermissions'){
                this.status.set('Sorry, you are not allowed access to blog management page yet. If this is your first time login, then please wait for access')
              } else {
                this.status.set(problemDetails.detail)
              }
              this.toastService.error(problemDetails.detail);
            })
        }
      })
  }
}
