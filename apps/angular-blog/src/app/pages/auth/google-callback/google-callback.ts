import {Component, inject} from '@angular/core';
import { ProgressSpinner } from 'primeng/progressspinner';
import {HotToastService} from '@ngxpert/hot-toast';
import {AuthService} from '@services/api/generated-sdk';
import {ActivatedRoute} from '@angular/router';
import {asProblemDetailsAsync} from '@utils/http-utils';

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

  constructor() {
    const code = this.activatedRoute.snapshot.queryParamMap.get('code');
    const state = this.activatedRoute.snapshot.queryParamMap.get('state');
    if (!code || !state) {
      this.toastService.error('Invalid login attempt.');
      return;
    }

    this.authService.login({code, state})
      .subscribe({
        next: (response) => {
          console.log(response);
        },
        error: (error) => {
          asProblemDetailsAsync(error)
            .then((problemDetails) => {
              this.toastService.error(problemDetails.detail);
            })
        }
      })
  }
}
