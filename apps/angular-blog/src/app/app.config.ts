import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import Aura from '@primeuix/themes/aura';
import { routes } from './app.routes';
import { provideHotToastConfig } from '@ngxpert/hot-toast';
import { providePrimeNG } from "primeng/config";

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHotToastConfig(),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: '.blg-alex-dark',
          cssLayer: {
            name: 'primeng',
            order: 'theme, base, primeng'
          }
        }
      }
    })
  ]
};
