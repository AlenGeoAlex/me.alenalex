import {Component, ElementRef, inject, signal, viewChild} from '@angular/core';
import {environment} from '@env/environment';
import {email, Field, form, minLength, required} from '@angular/forms/signals';
import {MeService, PostContactMeRequest} from '@api/generated-sdk';
import {HotToastService} from '@ngxpert/hot-toast';
import {DialogRef} from '@ngneat/dialog';
import {HttpErrorResponse} from '@angular/common/http';

@Component({
  selector: 'app-contact-me-component',
  templateUrl: './contact-me-component.html',
  styleUrl: './contact-me-component.scss',
  imports: [
    Field
  ]
})
export class ContactMeComponent {

  private readonly ref = inject(DialogRef);
  private readonly turnstile = viewChild.required<ElementRef<HTMLDivElement>>('turnstileElement');
  protected readonly turnstileError = signal<string | undefined>('');
  protected readonly isLoading = signal(false);
  protected readonly formSignal = signal<PostContactMeRequest>({
    email: '',
    message: '',
    captchaKey: ''
  });
  private readonly meService = inject(MeService);
  private readonly toastService = inject(HotToastService);
  protected readonly formField = form(this.formSignal, (schemaPath) => {
    required(schemaPath.email);
    email(schemaPath.email, {
      message: 'Invalid email address'
    });
    required(schemaPath.message);
    required(schemaPath.captchaKey, {
      message: 'Please complete the captcha to submit the form'
    });
    minLength(schemaPath.message, 10, {
      message: 'Message must be at least 10 characters long'
    });
  })

  constructor() {

  }

  ngAfterViewInit(): void {
    const widgetId = turnstile.render(this.turnstile().nativeElement, {
      sitekey: environment.turnstileApiKey,
      callback: (token) => {
        this.formSignal.update(x => {
          x.captchaKey = token;
          return x;
        })

      },
      "error-callback":  (errorCode) => {
        this.turnstileError.set("An unexpected error occurred. Please try again later. Error code: " + errorCode + "");
      },
    });
  }


  protected onSubmit() {
    if(this.isLoading()) return;

    this.isLoading.set(true);
    const toast = this.toastService.loading('Sending message...');
    this.meService.postContactMe(this.formSignal())
      .subscribe({
        next: () => {
          this.isLoading.set(false);
          toast.updateMessage('Thank you for your message! We will get back to you soon.')
          toast.updateToast({
            dismissible: true,
            duration: 5000,
          })
        },
        error: (err: HttpErrorResponse) => {
          this.isLoading.set(false)
          if(err.status === 429){
             toast.updateMessage('You have sent too many messages recently. Please try again later.')
          } else if(err.status === 400){
            toast.updateMessage('An unknown error occurred while sending your message. Please try again later. If the problem persists, please contact me directly at discord')
          } else {
            toast.updateMessage('An error occurred while sending your message. Please try again later.')
          }


          toast.updateToast({
            dismissible: true,
            duration: 5000,
          })
        },
        complete: () => this.ref.close()
      });
  }
}
