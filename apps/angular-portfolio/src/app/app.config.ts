import {ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import {provideHttpClient} from '@angular/common/http';
import {environment} from '../environments/environment';
import {ApiModule, Configuration, ConfigurationParameters} from '@api/generated-sdk';
import { provideHotToastConfig } from '@ngxpert/hot-toast';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    provideHotToastConfig(),
    importProvidersFrom([
      ApiModule.forRoot(() => {
        const params: ConfigurationParameters = {
          basePath: environment.apiBasePath
        }

        return new Configuration(params)
      })
    ]), provideHotToastConfig()
  ]
};
