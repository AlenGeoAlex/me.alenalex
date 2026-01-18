# Portfolio API Environment Variables

The `portfolio-backend-hono` application requires several environment variables for its operation on Cloudflare Workers. These are configured via the `.env` file for local development and GitHub Secrets for deployment.

## Environment Variables

| Variable | Description |
|----------|-------------|
| `ALLOWED_HOST` | A comma-separated list of allowed origins for CORS (e.g., `http://localhost:4200,https://alenalex.me`). |
| `R2_ACCOUNT_ID` | Cloudflare Account ID for R2 storage. |
| `R2_ACCESS_KEY` | Access key for R2 storage (S3-compatible). |
| `R2_SECRET_KEY` | Secret key for R2 storage (S3-compatible). |
| `R2_CV_PATH` | The path/key in the R2 bucket where the CV file is stored. |
| `R2_BUCKET` | The name of the R2 bucket. |
| `R2_PUBLIC_URL` | The public URL prefix for R2 objects. |
| `TURNSTILE_SECRET_KEY` | The secret key for Cloudflare Turnstile verification. |
| `DESTINATION_ADDRESS` | The email address where contact form messages will be sent. |

## Local Development

For local development, create a `.env` file in `apps/portfolio-backend-hono/`. You can use the `generate-env-example.mjs` script to maintain an `.env.example` file.

```bash
# Example .env in apps/portfolio-backend-hono/
ALLOWED_HOST=http://localhost:4200
R2_ACCOUNT_ID=your_account_id
R2_ACCESS_KEY=your_access_key
R2_SECRET_KEY=your_secret_key
R2_CV_PATH=cv/my-cv.pdf
R2_BUCKET=my-bucket
R2_PUBLIC_URL=https://pub-xxx.r2.dev
TURNSTILE_SECRET_KEY=1x0000000000000000000000000000000AA
DESTINATION_ADDRESS=your-email@example.com
```

## Production (GitHub Actions)

When deploying via GitHub Actions, these variables must be added as Secrets to the repository. The deployment workflow (`deploy-portfolio-api.yml`) uses them to update the `wrangler.jsonc` configuration before deploying to Cloudflare.

The secrets should be prefixed with `BACKEND_ENV_`, for example:
- `BACKEND_ENV_ALLOWED_HOST`
- `BACKEND_ENV_R2_ACCOUNT_ID`
- ...and so on.
