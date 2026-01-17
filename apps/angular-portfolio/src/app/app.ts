import { Component, inject } from '@angular/core';
import { DialogService } from '@ngneat/dialog';
import { NavbarComponent } from './components/navbar-component/navbar-component';
import { HomePage } from './pages/home-page/home-page';
import { AboutPage } from './pages/about-page/about-page';
import {ContactMeComponent} from './components/contact-me-component/contact-me-component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavbarComponent, HomePage, AboutPage],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly dialog = inject(DialogService);

  protected onContactMeRequest() {
    this.dialog.open(ContactMeComponent, {
      backdrop: true,
      draggable: false,
      size: 'lg',
      closeButton: false,
      windowClass: 'bg-gray-900 rounded-2xl shadow-xl p-6 text-white',
    });
  }
}
