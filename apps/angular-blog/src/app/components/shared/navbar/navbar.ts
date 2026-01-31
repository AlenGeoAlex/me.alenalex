import {Component, HostListener, inject, signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {Button} from 'primeng/button';
import {InputText} from 'primeng/inputtext';
import {IconField} from 'primeng/iconfield';
import {InputIcon} from 'primeng/inputicon';
import {Appearance} from '@services/appearance.service';
import {NgClass} from '@angular/common';
import {toSignal} from '@angular/core/rxjs-interop';
import {Tooltip} from 'primeng/tooltip';
import {RouterLink} from '@angular/router';
import {AutoComplete} from 'primeng/autocomplete';

@Component({
  selector: 'shared-navbar',
  imports: [
    FormsModule,
    Button,
    InputText,
    IconField,
    InputIcon,
    NgClass,
    Tooltip,
    RouterLink,
    AutoComplete
  ],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {

  private readonly appearanceService = inject(Appearance);

  protected toggleDarkMode() {
    this.appearanceService.toggleDarkMode();
  }

  protected isDarkTheme = toSignal(this.appearanceService.darkMode$);

  protected login() {

  }
}
