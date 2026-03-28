import {Component, computed, DestroyRef, effect, inject, output, signal} from '@angular/core';
import {ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive} from '@angular/router';
import {ButtonModule} from 'primeng/button';
import { IconFieldModule } from 'primeng/iconfield';
import {InputIcon, InputIconModule} from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import {ThemeService} from '@services/theme-service';
import {toSignal} from '@angular/core/rxjs-interop';
import {AuthStateService} from '@services/auth-state.service';
import {AvatarModule} from 'primeng/avatar';
import {TooltipModule} from 'primeng/tooltip';
import {AuthService} from '@services/api/generated-sdk';
import {filter, map} from 'rxjs';

@Component({
  selector: 'bloggi-navbar',
  imports: [
    RouterLink,
    ButtonModule,
    InputTextModule,
    IconFieldModule,
    InputIcon,
    AvatarModule,
    TooltipModule
  ],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {
  readonly searchClicked = output<void>();
  protected readonly mobileOpen = signal(false);

  private readonly authStateService = inject(AuthStateService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly themeService = inject(ThemeService);
  protected readonly GITHUB_LINK = "https://github.com/AlenGeoAlex/me.alenalex"
  protected readonly ABOUT_LINK = "https://alenalex.me"
  protected readonly darkModeSignal = toSignal(this.themeService.darkMode$, {initialValue: false});
  protected readonly isAuthenticatedSignal = this.authStateService.isAuthenticatedSignal;
  protected readonly authenticationStatus = this.authStateService.authStateSignal;
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      map(e => (e as NavigationEnd).urlAfterRedirects)
    ),
    { initialValue: this.router.url }
  );

  protected readonly routeBasedTitle = computed(() => {
    const url = this.currentUrl();

    if (url.startsWith('/admin')) return "Blog Management";

    return "Alen's Blog";
  });

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

  protected logout() {
    this.authService.logout()
      .subscribe(() => {
        this.authStateService.reloadState({
          redirect: true,
        });
      });
  }
}
