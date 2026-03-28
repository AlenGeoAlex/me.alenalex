import {Component, DestroyRef, inject, output, signal} from '@angular/core';
import {RouterLink, RouterLinkActive} from '@angular/router';
import {ButtonModule} from 'primeng/button';
import { IconFieldModule } from 'primeng/iconfield';
import {InputIcon, InputIconModule} from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import {ThemeService} from '@services/theme-service';
import {toSignal} from '@angular/core/rxjs-interop';

@Component({
  selector: 'bloggi-navbar',
  imports: [
    RouterLink,
    ButtonModule,
    InputTextModule,
    IconFieldModule,
    InputIcon,
  ],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {
  readonly searchClicked = output<void>();
  protected readonly mobileOpen = signal(false);

  private readonly destroyRef = inject(DestroyRef);
  private readonly themeService = inject(ThemeService);
  protected readonly GITHUB_LINK = "https://github.com/AlenGeoAlex/me.alenalex"
  protected readonly ABOUT_LINK = "https://alenalex.me"
  protected readonly darkModeSignal = toSignal(this.themeService.darkMode$, {initialValue: false});

  constructor() {

  }

  onSearchClick(): void {
    this.mobileOpen.set(false);
    this.searchClicked.emit();
  }

  toggleMobile(): void {
    this.mobileOpen.update(v => !v);
  }

  closeMobile(): void {
    this.mobileOpen.set(false);
  }

  protected toggleDarkMode() {
    this.themeService.toggleDarkMode();
  }
}
