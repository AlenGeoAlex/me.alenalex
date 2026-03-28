import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { providePrimeNG } from 'primeng/config';
import {BlogTheme} from './bloggi-theme';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    providePrimeNG({
      ripple: true,
      theme: {
        preset: BlogTheme,
        options: {
          prefix:           'p',
          darkModeSelector: '.dark',
          cssLayer:         false,
        },
      },
    }),
  ]
};
