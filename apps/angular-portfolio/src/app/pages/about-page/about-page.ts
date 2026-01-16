import {Component, effect, inject, signal} from '@angular/core';
import {TerminalComponent} from '../../components/terminal-component/terminal-component';
import {HotToastService} from '@ngxpert/hot-toast';
import {StaticAboutMe} from "../../components/static-about-me/static-about-me";
export type TPageTypes = 'interactive' | 'static' | 'chat';
@Component({
  selector: 'app-about-page',
  templateUrl: './about-page.html',
  imports: [
    TerminalComponent,
    StaticAboutMe
  ]
})
export class AboutPage {

  protected readonly pageType = signal<TPageTypes>(this.loadPageType());
  protected readonly toastService = inject(HotToastService);
  protected readonly pageTypeStatus = signal<Record<TPageTypes, boolean>>({
    'interactive': true,
    'static': true,
    'chat': false
  })

  constructor() {
    effect(() => {
      const pageType = this.pageType();
      localStorage.setItem('pageType', pageType);
    })
  }

  private loadPageType() {
    return localStorage.getItem('pageType') as TPageTypes || 'static';
  }

  protected onInteractiveError(error: Error | string){
    let errorMessage = error instanceof Error ? error.message : error;
    this.toastService.error(errorMessage, {
      dismissible: true,
      position: 'bottom-right'
    })

    this.pageType.set('static');
    this.pageTypeStatus.update(x => {
      x.interactive = false
      return x
    })
  }

  protected onInteractiveClose() {
    this.pageType.set('static');
  }

  protected setPageType(mode: TPageTypes) {
    this.pageType.set(mode)
  }
}
