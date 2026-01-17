import {Component, ComponentRef, effect, ElementRef, signal, viewChild} from '@angular/core';
import {environment} from '@env/environment';
import {email, Field, form, minLength, required} from '@angular/forms/signals';

@Component({
  selector: 'app-contact-me-component',
  imports: [
    Field
  ],
  templateUrl: './contact-me-component.html',
  styleUrl: './contact-me-component.scss',
})
export class ContactMeComponent {

  private readonly turnstile = viewChild.required<ElementRef<HTMLDivElement>>('turnstileElement');
  protected readonly turnstileError = signal<string | undefined>('');
  protected readonly isLoading = signal(false);
  protected readonly formSignal = signal({
    email: '',
    message: '',
    captchaKey: ''
  });

  protected readonly formField = form(this.formSignal, (schemaPath) => {
    required(schemaPath.email);
    email(schemaPath.email);
    required(schemaPath.message);
    required(schemaPath.captchaKey);
    minLength(schemaPath.message, 20);
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



}
