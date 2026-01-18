# Generated API SDK

This directory contains the auto-generated Angular SDK for the Portfolio API. It is generated using the [OpenAPI Generator](https://openapi-generator.tech/).

## Generation

The SDK is generated from the OpenAPI specification provided by the `portfolio-backend-hono` application.

## Structure

- **`api/`**: Contains the Angular services for different API tags (e.g., `MeService`, `PathService`).
- **`model/`**: Contains the TypeScript interfaces for request and response bodies.
- **`api.module.ts`**: The Angular module that provides the services.
- **`variables.ts`**: Contains configuration variables like `BASE_PATH`.

## Usage in Angular

### 1. Import the Module

In your `app.config.ts` (for standalone) or `AppModule`:

```typescript
import { ApiModule, Configuration } from '@api/generated-sdk';

// ...
providers: [
  importProvidersFrom(
    ApiModule.forRoot(() => {
      return new Configuration({
        basePath: environment.apiUrl,
      });
    })
  ),
]
```

### 2. Inject and Use Services

You can inject the services directly into your components:

```typescript
import { MeService } from '@api/generated-sdk';

@Component({ ... })
export class MyComponent {
  private meService = inject(MeService);

  ngOnInit() {
    this.meService.getInitMe().subscribe(data => {
      console.log(data);
    });
  }
}
```

## Updating the SDK

To update the SDK when the API changes, run the generation script from the root of the project:

```bash
pnpm run generate:portfolio-angular:backend-api-sdk
```

*(Note: Ensure the backend is running or the `openapi.json` is available as configured in the generation script.)*
