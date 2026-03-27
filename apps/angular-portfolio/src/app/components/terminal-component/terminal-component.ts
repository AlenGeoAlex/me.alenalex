import {
  Component,
  effect,
  ElementRef,
  inject,
  input,
  linkedSignal,
  output,
  signal,
  untracked,
  viewChild
} from '@angular/core';
import {Header} from './header/header';
import {Body} from './body/body';
import {IResponseContextInput} from './body/response-context/response-context';
import {ITerminalCommandOutput} from './body/current-context/current-context';
import {IErrorResponse} from './body/error-response-context/error-response-context';
import {HelpBar} from './help-bar/help-bar';
import {animate, stagger} from 'motion';
import {GetFsList200Response, MeService, PathService} from '@api/generated-sdk';
import {rxResource} from '@angular/core/rxjs-interop';
import {HttpErrorResponse} from '@angular/common/http';

export type TTerminalContent = { kind: 'command', data: ITerminalCommandOutput, }
  | {kind: 'response', data: IResponseContextInput}
  | {kind: 'error', data: IErrorResponse};

@Component({
  selector: 'app-terminal-component',
  imports: [
    Header,
    Body,
    HelpBar
  ],
  templateUrl: './terminal-component.html',
  styleUrl: './terminal-component.css',
})
export class TerminalComponent {
  public readonly title = input<string>('connected to me.alenalex:about');
  public readonly  close = output();
  public readonly error = output<Error | string>();
  protected readonly path = signal<string[]>(['home']);
  protected readonly  content = signal<TTerminalContent[]>([]);
  protected readonly isLoading = signal(false);
  private readonly helpBar = viewChild.required<HelpBar>('helpBar');
  private readonly tBody = viewChild.required<Body>('tBody');
  private readonly terminalRef = viewChild.required<ElementRef<HTMLElement>>('terminalRef');
  private readonly meService = inject(MeService);
  private readonly pathService = inject(PathService);
  private readonly initialized = signal(false);
  private readonly commandHistory = signal<string[]>([]);

  private historyCounter = 0;

  constructor() {
    effect(() => {
      const status = this.initialResource.status();
      if(status === 'loading' || status === 'reloading'){
        this.isLoading.set(true);
      }else {
        this.isLoading.set(false);
      }

      let error: Error | undefined;
      let data: string | undefined;
      try {
        error = this.initialResource.error();
      } catch (e) {}

      try {
        data = this.initialResource.value()
      }catch (e){}

      if(status === 'error'){
        console.error(error)
        this.error.emit('An unknown error occurred');
        return;
      }
      const initialized = this.initialized();
      if(initialized) return;
      if(!this.initialResource.hasValue()) return;

      untracked(() => {
        this.initialized.set(true);
      })
      this.content.update(x => {
        x.push({kind: 'response', data : {
            contentType: 'text/html',
            content: data!
          }})
        return x;
      })
      this.pushNewCommand();
    })
  }

  protected readonly initialResource = rxResource({
    params: () => ({}),
    stream: (params) => this.meService.getInitMe()
  })

  protected onCloseEmit(){
    // Emit to the parent component
    this.close.emit();
  }

  protected onCommand($event: ITerminalCommandOutput) {
    this.historyCounter = 0;
    if($event.command.trim().startsWith("rm -rf")){
      this.runEffect();
      return;
    }

    // Just update the array with the last command
    // This is due to my bad design choice :)
    this.content.update(x => {
      const last = x[x.length - 1];
      if(last.kind !== 'command')
        return x;

      last.data = $event;
      return x;
    })

    this.commandHistory.update(x => {
      x.push($event.command);
      return x;
    })

    const userCommand = $event.command.trim().split(' ');
    // Technically current context won't emit empty strings,
    // but for my peace of mind, I can check for empty

    if(userCommand.length === 0)
      return;


    const commandToken = userCommand[0];
    switch (commandToken.toLowerCase()) {
      case 'switch':
      case 'cd':
        this.onCdCommand(userCommand.slice(1));
        break;
      case 'list':
      case 'ls':
        this.onLsCommand();
        break;
      case 'back':
        this.onBackCommand();
        break;
      case 'clear':
        this.onClearCommand();
        break;
      case 'help':
        this.onHelpCommand();
        break;
      case 'about':
        this.pushResponse({
          contentType: 'text/plain',
          content: 'This is an interactive terminal for geeks like me who like to dwell a bit into its working and use keyboard rather than mouse. It is a very basic implementation and supports only handful of commands. PLEASE DON\'T BREAK IT :)'
        })
        this.pushNewCommand();
        break;
      case 'open':
        this.onOpenCommand(userCommand.slice(1));
        break;
      case 'download':
        this.onOpenCommand(userCommand.slice(1), false);
        break;
      default:
        this.pushError(`Unknown command: ${commandToken}. Type help for a list of commands?
        `);
        break;
    }
  }

  private pushNewCommand(){
    this.content.update(x => {
      x.push({kind: 'command', data: {path: this.path(), command: ''}})
      return x;
    })
  }

  private pushError(message: string, addNextCommand = true){
    this.content.update(x => {
      x.push({kind: 'error', data: {message}})
      if(addNextCommand){
        x.push({kind: 'command', data: {path: this.path(), command: ''}})
      }
      return x;
    })
  }

  private pushResponse(response: IResponseContextInput){
    this.content.update(x => {
      x.push({kind: 'response', data: response})
      return x;
    })
  }

  private onCdCommand(newPath: string[]){
    if(newPath.length === 0){
      this.pushError('Path cannot be empty');
      return;
    }

    const directory = newPath[0];
    switch (directory.toLowerCase()) {
      case '~':
        this.path.set(['']);
        this.pushNewCommand();
        break;
      case '/':
        this.path.set(['home']);
        this.pushNewCommand();
        break;
      default:
        const constructedPath = this.constructPath(directory);
        this.isLoading.set(true);
        this.onPathChange(constructedPath);
        break;
    }
  }

  private onPathChange(newPath: string){
    this.pathService.getFsList(
      newPath,
      true
    ).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.path.set(newPath.split('/'));
        this.pushNewCommand();
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);
        if(err.status === 404){
          this.pushError(`Directory ${newPath} does not exist`);
          return;
        }

        if(err.status === 400){
          this.pushError(err.message || err.error || 'An unknown error occurred. Please try again later.');
          return;
        }

        this.pushError(err.message || err.error || 'An unknown error occurred. Please try again later.');
      }
    })
  }

  private constructPath(newPath: string) : string {
    if(newPath.startsWith('/')){
      return newPath;
    }

    return this.path().join('/') + '/' + newPath;
  }

  private onBackCommand(){
    const strings = this.path();
    if(strings.length <= 1){
      this.pushError('Cannot go back any further');
      return;
    }

    strings.pop();
    this.path.set(strings);
    this.onPathChange(this.path().join('/'));
    this.pushNewCommand();
  }


  private onHelpCommand(){
    const helpMessage = `
    ###Commands

    - \`cd <dir>\`
      Change the current directory

    - \`ls\`
      List files in the current directory

    - \`list\`
      Alias for \`ls\`

    - \`back\`
      Go back to the previous directory

    - \`open <name>\`
      Open a file or directory

    - \`download <file>\`
      Download a file

    - \`about\`
      Print the about file


    ###Keybinds

    - **Enter**
      Execute the current command

    - **Tab**
      ❌ No autocompletion support :)

    - **Up Arrow**
      ⚠️ Buggy history support :)
    `;
    this.content.update(x => {
      x.push({kind: 'response', data: {
        contentType: 'text/markdown',
        content: helpMessage
        }})

      return x;
    })

    queueMicrotask(() => {
      this.pushNewCommand();
    })
  }

  runEffect(){
    setTimeout(() => {
      this.runEffectV1();
    }, 200)
  }

  private async runEffectV1(){
    // this.scrollTerminalToTop();

    function getAllTerminalLines(): HTMLElement[] {
      const lines: HTMLElement[] = [];
      let i = 0;

      while (true) {
        const el = document.getElementById(`terminal-body-item-${i}`);
        if (!el) break;
        lines.push(el);
        i++;
      }

      return lines;
    }

    const lines = getAllTerminalLines();
    if (!lines.length) return;

    if(this.content().length <= 2){
      animate(
        lines,
        {
          transform: lines.map(() => `translate(0px,0px) rotate(0deg)`)
            .concat(lines.map(() => `translate(${(Math.random() - 0.5) * 10}px, 80px) rotate(-2deg)`)),
          opacity: [1, 0],
        },
        {
          delay: stagger(0.06),
          duration: 1.2,
          ease: 'easeIn',
        }
      );
    } else{


      lines.forEach(line => {
        line.style.transform = `translateY(-100px)`;
        line.style.opacity = '0';
        line.style.transform = `translateY(-150px)`;
      });

      await new Promise(r => setTimeout(r, 200));

      animate(
        lines,
        {
          transform: lines.map(() => 'translateY(-150px) rotate(-5deg)').concat(lines.map(() => 'translateY(0px) rotate(0deg)')),
          opacity: [0, 1],
        },
        {
          delay: stagger(0.08),
          duration: 0.8,
          ease: 'easeOut',
        }
      );
    }
  }

  scrollTerminalToTop() {
    const body = document.getElementById('terminal-body');
    if (!body) return;

    body.scrollTo({
      top: 0,
      behavior: 'smooth',
    });
  }


  private onClearCommand() {
    this.content.set([])
    this.pushNewCommand();
  }

  private onLsCommand() {
    this.isLoading.set(true);
    const currentPath = this.path().join('/');
    this.pathService.getFsList(
      currentPath,
      false
    ).subscribe({
      next: (data: GetFsList200Response) => {
        this.isLoading.set(false);
        this.pushResponse({
          contentType: 'text/html',
          content: `
          <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 10px 20px; padding: 10px;">
            ${data.children.map(item => {
                    const color = item.type === 'directory' ? '#4CAF50' : '#E0E0E0';
                    const fontWeight = item.type === 'directory' ? 'bold' : 'normal';
                    return `<div style="color: ${color}; font-weight: ${fontWeight};">${item.name}</div>`;
                  }).join('')}
          </div>
           `
        })
        this.pushNewCommand();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.pushError(err.message || err.error || 'An unknown error occurred. Please try again later.');
      },
      complete: () => {
      }
    })
  }

  private onOpenCommand(strings: string[], download = false) {
    if(strings.length === 0){
      this.pushError('No file specified');
      return;
    }



    const fileName = this.path().join('/') + '/' + strings[0];

    this.isLoading.set(true);
    this.pathService.getFsOpen(fileName, download)
      .subscribe({
        next: (data) => {
          this.isLoading.set(false);
          this.pushResponse({
            contentType: data.contentType as IResponseContextInput['contentType'],
            content: data.content
          })
          this.pushNewCommand();
        },
        error: (err) => {
          this.isLoading.set(false);
          this.pushError(err.message || err.error || 'An unknown error occurred. Please try again later.');
        }
      })
  }

  protected onTab($event: any) {
    $event.preventDefault();
  }
}
