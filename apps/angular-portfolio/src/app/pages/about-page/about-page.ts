import {Component, inject, signal} from '@angular/core';
import {TerminalComponent} from '../../components/terminal-component/terminal-component';
import {HotToastService} from '@ngxpert/hot-toast';

@Component({
  selector: 'app-about-page',
  templateUrl: './about-page.html',
  imports: [
    TerminalComponent
  ]
})
export class AboutPage {

  protected readonly pageType = signal<'interactive' | 'static'>('interactive');
  protected readonly toastService = inject(HotToastService);

  protected onInteractiveError(error: Error | string){
    let errorMessage = error instanceof Error ? error.message : error;
    this.toastService.error(errorMessage, {
      dismissible: true,
      position: 'bottom-right'
    })

    this.pageType.set('static');
  }

  protected onInteractiveClose() {
    if(this.pageType() === 'interactive')
      this.pageType.set('static');
    else this.pageType.set('interactive');
  }
}
