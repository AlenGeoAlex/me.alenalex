import {
  ApplicationConfig,
  importProvidersFrom,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { providePrimeNG } from 'primeng/config';
import { BlogTheme } from './bloggi-theme';
import { ApiModule, Configuration, ConfigurationParameters } from '@services/api/generated-sdk';
import { provideHotToastConfig } from '@ngxpert/hot-toast';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    importProvidersFrom([ApiModule.forRoot(apiConfigFactory)]),
    providePrimeNG({
      ripple: true,
      theme: {
        preset: BlogTheme,
        options: {
          prefix: 'p',
          darkModeSelector: '.dark',
          cssLayer: false,
        },
      },
    }),
    provideHotToastConfig(),
  ],
};

function apiConfigFactory(): Configuration {
  const params: ConfigurationParameters = {
    basePath: 'api',
  };
  return new Configuration(params);
}
