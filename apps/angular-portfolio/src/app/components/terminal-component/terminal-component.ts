import {Component, effect, ElementRef, inject, input, output, signal, viewChild} from '@angular/core';
import {Header} from './header/header';
import {Body} from './body/body';
import {IResponseContextInput} from './body/response-context/response-context';
import {ITerminalCommandOutput} from './body/current-context/current-context';
import {IErrorResponse} from './body/error-response-context/error-response-context';
import {HelpBar} from './help-bar/help-bar';
import {animate, stagger} from 'motion';
import {MeService} from '@api/generated-sdk';
import {rxResource} from '@angular/core/rxjs-interop';

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
  public readonly title = input<string>('connect me.alenalex:about');
  public readonly  close = output();
  public readonly error = output<Error | string>();
  protected readonly  path = signal<string[]>(['home']);
  protected readonly  content = signal<TTerminalContent[]>([]);
  protected readonly isLoading = signal(false);
  private readonly helpBar = viewChild.required<HelpBar>('helpBar');
  private readonly tBody = viewChild.required<Body>('tBody');
  private readonly terminalRef = viewChild.required<ElementRef<HTMLElement>>('terminalRef');
  private readonly meService = inject(MeService);

  constructor() {
    effect(() => {
      const status = this.initialResource.status();
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

      if(!this.initialResource.hasValue()) return;

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

    const userCommand = $event.command.trim().split(' ');
    // Technically current context won't emit empty strings,
    // but for my peace of mind, I can check for empty

    if(userCommand.length === 0)
      return;


    const commandToken = userCommand[0];
    switch (commandToken.toLowerCase()) {
      case 'cd':
        this.onCdCommand(userCommand.slice(1));
        break;
      case 'back':
        break;
      case 'clear':
        this.onClearCommand();
        break;
      case 'help':
        this.onHelpCommand();
        break;
      case 'about':

        break;
      case 'open':

        break;
      case 'download':

        break;
      case 'skills':

        break;
      default:
        this.pushError(`Unknown command: ${commandToken}.
        Did you mean: type help for a list of commands?
        `);
        break;
    }

    // this.isLoading.set(true);
    // setTimeout(() => {
    //   const number = Math.random();
    //   this.isLoading.set(false);
    //   if(number <= 0.5){
    //     this.content.update(v => {
    //       v.push({
    //         kind: 'response',
    //         data: {
    //           contentType: 'text/plain',
    //           content: `
    //           Hello World
    //           `
    //         }
    //       })
    //
    //       v.push({kind: 'command', data: {path: [], command: ''}})
    //       return v;
    //     })
    //   }else {
    //     this.content.update(v => {
    //       v.push({kind: 'error', data: {message: 'Command not found'}})
    //
    //       v.push({kind: 'command', data: {path: [], command: ''}})
    //       return v;
    //     })
    //   }
    // }, 250)
  }

  private pushNewCommand(){
    this.content.update(x => {
      x.push({kind: 'command', data: {path: this.path(), command: ''}})
      return x;
    })
  }

  private pushError(message: string){
    this.content.update(x => {
      x.push({kind: 'error', data: {message}})
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
        this.path.set(['/home/']);
        break;
      case '/':
        this.path.set(['/']);
        break;
      default:
        this.pushError(`Directory ${directory} does not exist`);
        break;
    }
  }

  private onBackCommand(){

  }



  private onHelpCommand(){
    // this.helpBar().toggleHelp();
    // setTimeout(() => {
    //   this.tBody().focusInput();
    // }, 500)
    const helpMessage = `
      ### Command
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
    // lines.reverse();

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


  private motd = `<div class="terminal-motd">
  <span style="color:#E95420;">Welcome to AboutMeOs 1.0.0 LTS</span>
  (<span style="color:#8AE234;">GNU/Developer Linux</span>)
  <br>
  <span style="color:#888;">Kernel:</span> 6.∞.dev •
  <span style="color:#888;">Arch:</span> human64
  <br><br>

  <span style="color:#888;"> * Portfolio:</span>
  <a href="https://www.alenalex.me" style="color:#729FCF;">https://alenalex.me</a><br>
  <span style="color:#888;"> * Source:</span>
  <a href="https://github.com/AlenGeoAlex" style="color:#729FCF;">https://github.com/AlenGeoAlex</a><br>
  <span style="color:#888;"> * Contact:</span>
  <a href="mailto:contact@alenalex.me" style="color:#729FCF;">contact@alenalex.me</a>
  <br><br>

  <span style="color:#888;">
    System information as of
  </span>
  <span style="color:#C4A000;">
    Sat Jan 10 21:02:14 UTC 2026
  </span>
  <br><br>

  <pre style="margin:0; color:#DDD;">
  System load:        <span style="color:#8AE234;">0.01</span>               Active projects:     <span style="color:#8AE234;">7</span>
  Memory usage:      <span style="color:#8AE234;">42%</span>               Coffee level:        <span style="color:#EF2929;">LOW</span>
  Disk usage (/):    <span style="color:#8AE234;">12.4%</span>             Bugs currently alive: <span style="color:#EF2929;">∞</span>
  IPv4 address:      <span style="color:#729FCF;">127.0.0.1</span>          Visitors online:     <span style="color:#8AE234;">1</span>
  </pre>

  <br>

  <span style="color:#FCE94F;">
    Expanded Curiosity Maintenance is enabled.
  </span><br>
  <span style="color:#DDD;">
    New features are deployed frequently.
  </span>
  <br><br>

  <span style="color:#DDD;">
    Type
    <span style="color:#8AE234;">help</span>
    to see available commands.
  </span><br>
  <span style="color:#DDD;">
    Type
    <span style="color:#8AE234;">about</span>,
    <span style="color:#8AE234;">skills</span>,
    <span style="color:#8AE234;">projects</span>,
    or
    <span style="color:#8AE234;">hobbies</span>
    to explore.
  </span>
  <br><br>

  <span style="color:#888;">
    Last login: Fri Jan 9 11:56:18 2026 from you!
  </span>
</div>`;

  private onClearCommand() {
    this.content.set([])
    this.pushNewCommand();
  }
}
