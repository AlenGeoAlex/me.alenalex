import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {Navbar} from '@components/navbar/navbar';
import {ConfirmationService, MessageService} from 'primeng/api';
import {ConfirmDialog} from 'primeng/confirmdialog';
import {ConfirmPopup} from 'primeng/confirmpopup';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, ConfirmDialog, ConfirmPopup],
  templateUrl: './app.html',
  styleUrl: './app.css',
  providers: [ConfirmationService, MessageService]
})
export class App {
}
