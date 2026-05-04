import {
  ApplicationConfig,
  importProvidersFrom, provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { providePrimeNG } from 'primeng/config';
import { BlogTheme } from './bloggi-theme';
import { ApiModule, Configuration, ConfigurationParameters } from '@services/api/generated-sdk';
import { provideHotToastConfig } from '@ngxpert/hot-toast';
import {appAuthStateInitializer} from '@angular/initializer';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import {httpAccessInterceptorInterceptor} from '@angular/interceptor/http-access-interceptor-interceptor';
import {DialogService} from 'primeng/dynamicdialog';
import {ConfirmationService} from 'primeng/api';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([
      httpAccessInterceptorInterceptor
    ])),
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
    provideHotToastConfig({
      position: 'bottom-right',
    }),
    provideAppInitializer(appAuthStateInitializer),
    DialogService,
    ConfirmationService
  ],
};

function apiConfigFactory(): Configuration {
  const params: ConfigurationParameters = {
    basePath: 'api',
  };
  return new Configuration(params);
}
