import {Component, inject} from '@angular/core';
import {AuthService} from '@services/api/generated-sdk';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import {HotToastService} from '@ngxpert/hot-toast';
import {HttpErrorResponse} from '@angular/common/http';
import {asProblemDetailsAsync} from '@utils/http-utils';
import {Router} from '@angular/router';

@Component({
  selector: 'bloggi-google-oauth',
  imports: [ProgressSpinnerModule],
  templateUrl: './google-oauth.html',
  styleUrl: './google-oauth.scss',
})
export class GoogleOauth {

  private readonly authService = inject(AuthService);
  private readonly toastService = inject(HotToastService);
  private readonly router = inject(Router);

  constructor() {
    this.authService.getLogin()
      .subscribe({
        next: (response) => {
          if (!response.googleLoginUrl) {
            this.toastService.error('Failed to fetch Google login URL.');
            return;
          }
          window.location.href = response.googleLoginUrl;
        },
        error: (error : HttpErrorResponse) => {
          asProblemDetailsAsync(error)
            .then(problemDetails => {
              this.toastService.error(problemDetails.detail)
              setTimeout(() => {
                this.router.navigate(['/'])
                  .catch(console.error)
              }, 2500)
            })
        }
      })
  }

}
