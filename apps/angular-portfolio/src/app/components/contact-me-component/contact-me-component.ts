import {Component, ElementRef, inject, signal, viewChild} from '@angular/core';
import {environment} from '@env/environment';
import {email, Field, form, minLength, required} from '@angular/forms/signals';
import {MeService, PostContactMeRequest} from '@api/generated-sdk';
import {HotToastService} from '@ngxpert/hot-toast';
import {DialogRef} from '@ngneat/dialog';

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
    this.meService.postContactMe(this.formSignal())
      .pipe(
        this.toastService.observe({
          success: 'Thank you for your message! We will get back to you soon.',
          error: 'An error occurred while sending your message. Please try again later.',
          loading: 'Please wait while we send your message.'
        })
      )
      .subscribe({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false),
        complete: () => this.ref.close()
      });
  }
}
