import {
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  input,
  model,
  output,
  signal,
  viewChild
} from '@angular/core';
import {FormsModule} from '@angular/forms';

export interface ITerminalCommandOutput {
  path: string[],
  command: string,
}

@Component({
  selector: 'terminal-body-current-context',
  imports: [
    FormsModule
  ],
  templateUrl: './current-context.html',
  styleUrl: './current-context.css',
})
export class CurrentContext {
  public readonly  user = input('you')
  public readonly  server = input('me.alenalex')
  public readonly  path = input<string[] | undefined>();
  protected readonly  pathComputed = computed(() => {
    const path = this.path();
    return path ? path.join('/') : '';
  })
  public readonly  disabled = model(false);
  readonly userInput = model<string>('');
  public readonly onUserCommandResponse = output<ITerminalCommandOutput>();
  private readonly destroyRef = inject(DestroyRef);
  private readonly terminalInput = viewChild.required<ElementRef<HTMLInputElement>>('terminalInput');

  constructor() {
  }

  protected submitUserCommand($event: any) {
    if (this.disabled()) {
      return;
    }

    if(this.userInput().trim().length === 0)
      return;

    this.onUserCommandResponse.emit({
      path: this.path() || [],
      command: this.userInput(),
    })
    this.disabled.set(true);
  }

  public setFocus(){
    setTimeout(() => {
      this.terminalInput()?.nativeElement?.focus();
    }, 0);
  }
}
