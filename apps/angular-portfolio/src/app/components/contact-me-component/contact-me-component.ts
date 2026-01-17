import {Component, ComponentRef, ElementRef, viewChild} from '@angular/core';
import {environment} from '@env/environment';

@Component({
  selector: 'app-contact-me-component',
  imports: [],
  templateUrl: './contact-me-component.html',
  styleUrl: './contact-me-component.scss',
})
export class ContactMeComponent {

  private readonly turnstile = viewChild.required<ElementRef<HTMLDivElement>>('turnstileElement');

  constructor() {

  }

  ngAfterViewInit(): void {
    const widgetId = turnstile.render(this.turnstile().nativeElement, {
      sitekey: environment.turnstileApiKey,
      callback: function (token) {
        console.log("Success:", token);
        const response = turnstile.getResponse(widgetId);
        console.log(response);
      },
    });
  }

  private get turnstileObject() : any {
    return turnstile as any;
  }


}
