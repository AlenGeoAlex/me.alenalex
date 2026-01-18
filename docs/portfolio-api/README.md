# Portfolio API

This is the backend API for the portfolio website, built with [Hono](https://hono.dev/) and deployed on [Cloudflare Workers](https://workers.cloudflare.com/).

## Tech Stack

- **Framework**: [Hono](https://hono.dev/)
- **OpenAPI Documentation**: [chanfana](https://chanfana.pages.dev/)
- **Runtime**: [Cloudflare Workers](https://workers.cloudflare.com/)
- **Validation**: [Zod](https://zod.dev/)
- **Storage**: Cloudflare R2 / S3-compatible storage (for FS endpoints)
- **Security**: Cloudflare Turnstile (for contact form)

## Features

- **Personal Information**: Endpoints to retrieve CV and basic info.
- **Contact Form**: Secure contact form with Turnstile verification and email delivery.
- **Virtual File System**: Endpoints to list and open files for the terminal interface.
- **Rate Limiting**: Integrated rate limiting to prevent abuse.

## Endpoints

### Me Endpoints

- `GET /api/me`: Initialize/get basic info.
- `GET /api/me/cv`: Retrieve CV information.
- `POST /api/me/contact`: Send a message via the contact form.
  - Requires `email`, `message`, and `captchaKey` (Turnstile token).

### File System Endpoints

- `GET /api/path/:path`: List files and directories at the specified path.
- `GET /api/open/:fileName`: Get content or details of a specific file.

## Documentation

The API automatically generates OpenAPI documentation using `chanfana`. When running locally, you can access the interactive Swagger UI at the root path (`/`).

## Environment Variables

For details on the required environment variables, see [ENVIRONMENT.md](./ENVIRONMENT.md).

## Development

To start the development server:

```bash
pnpm run dev
```

From the root directory:
```bash
pnpm dev:api
```
