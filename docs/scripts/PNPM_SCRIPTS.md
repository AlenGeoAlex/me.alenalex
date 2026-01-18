# Useful PNPM Scripts

Available from the root `package.json`:

| Script | Command | Description |
|--------|---------|-------------|
| `dev:portfolio` | `pnpm --filter angular-portfolio dev` | Start Angular portfolio in dev mode. |
| `dev:api` | `pnpm --filter portfolio-backend-hono dev` | Start Hono API in dev mode. |
| `gen:sdk` | `node scripts/generator/generate-api-sdk.js` | Generate the Angular SDK. |
| `generate:about-me` | `node scripts/generator/generate-about-me.js` | Update personal info data. |
