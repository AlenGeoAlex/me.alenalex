import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {NavbarComponent} from './components/navbar-component/navbar-component';
import {HomePage} from './pages/home-page/home-page';
import {AboutPage} from './pages/about-page/about-page';

@Component({
  selector: 'app-root',
  imports: [NavbarComponent, HomePage, AboutPage],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {



}
