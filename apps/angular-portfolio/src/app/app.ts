import {Component, signal, viewChild} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {NavbarComponent} from './components/navbar-component/navbar-component';
import {HomePage} from './pages/home-page/home-page';
import {AboutPage} from './pages/about-page/about-page';
import {ModalOptions, NgxCustomModalComponent} from 'ngx-custom-modal';
import {ContactMeComponent} from './components/contact-me-component/contact-me-component';

@Component({
  selector: 'app-root',
  imports: [NavbarComponent, HomePage, AboutPage, NgxCustomModalComponent, ContactMeComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {

  protected readonly contactMeModal = viewChild.required<NgxCustomModalComponent>('modalComponent');
  protected readonly modalOptions : ModalOptions = {

  }

  protected onContactMeRequest() {
    this.contactMeModal().open();
  }
}
