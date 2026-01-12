import {ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import {provideHttpClient} from '@angular/common/http';
import {environment} from '../environments/environment';
import {ApiModule, Configuration, ConfigurationParameters} from '@api/generated-sdk';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    importProvidersFrom([
      ApiModule.forRoot(() => {
        const params: ConfigurationParameters = {
          basePath: environment.apiBasePath
        }

        return new Configuration(params)
      })
    ])
  ]
};
