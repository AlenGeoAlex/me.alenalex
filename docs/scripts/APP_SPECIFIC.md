# App-Specific Scripts

These scripts are located within specific application directories.

## Portfolio Backend (`apps/portfolio-backend-hono/scripts/`)

- **`generate-env-example.mjs`**: Creates a `.env.example` file based on the keys in your current `.env` file (stripping the values).
- **`update-wrangler-config.mjs`**: Used during CI/CD to update `wrangler.jsonc` with environment-specific variables from GitHub Secrets.
