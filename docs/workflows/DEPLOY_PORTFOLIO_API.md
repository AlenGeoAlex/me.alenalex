# Portfolio API Deployment Workflow

This workflow (`deploy-portfolio-api.yml`) automates the deployment of the Hono-based backend to Cloudflare Workers.

## Details

- **Trigger**:
  - Manual trigger via `workflow_dispatch`.
  - Pushes to `main` or `master` branches that modify `apps/portfolio-backend-hono/`, the root `package.json`, or the workflow itself.
- **Key Steps**:
  - **Environment Preparation**: Runs `pnpm run deploy:prepare-env-backend-hono` and `pnpm run generate:about-me`.
  - **Configuration Update**: Injects GitHub Secrets into the `wrangler.jsonc` file using the `update-wrangler-config.mjs` script.
  - **Cloudflare Deployment**: Uses the `cloudflare/wrangler-action` to deploy the worker.
