import {Component, input} from '@angular/core';

export interface IErrorResponse {
  message: string;
}

@Component({
  selector: 'terminal-body-error-response-context',
  imports: [],
  template: `
    <div class="text-red-400" id="terminal-body-item-{{ id() }}">
     X {{ error().message }}
    </div>
  `,
  styles: ``,
})
export class ErrorResponseContext {
  public error = input.required<IErrorResponse>();
  public id = input.required<string | number>();

}
