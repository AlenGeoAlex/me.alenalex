import {Component, effect, input, output, untracked, viewChild} from '@angular/core';
import {CurrentContext, ITerminalCommandOutput} from './current-context/current-context';
import {Spinner} from './spinner/spinner';
import {ResponseContext} from './response-context/response-context';
import {TTerminalContent} from '../terminal-component';
import {ErrorResponseContext} from './error-response-context/error-response-context';

@Component({
  selector: 'terminal-body',
  imports: [
    CurrentContext,
    Spinner,
    ResponseContext,
    ErrorResponseContext
  ],
  templateUrl: './body.html',
  styleUrl: './body.css',
})
export class Body {
  content = input.required<TTerminalContent[]>();
  public isLoading = input<boolean>(false);
  public readonly command = output<ITerminalCommandOutput>();
  private readonly activeContext = viewChild<CurrentContext>('activeContext');

  public focusInput() {
    this.activeContext()?.setFocus();
  }

  protected emitCommand($event: ITerminalCommandOutput) {
    this.command.emit($event);
  }

  constructor() {
    effect(() => {
      const contentLength = this.content().length;
      this.focusInput();
      untracked(() => {

      })
    });
  }
}
