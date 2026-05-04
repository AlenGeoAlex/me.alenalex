import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {Navbar} from '@components/navbar/navbar';
import {ConfirmPopup} from 'primeng/confirmpopup';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, ConfirmPopup],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
}
