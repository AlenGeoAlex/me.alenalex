# Angular Portfolio Deployment Workflow

This workflow (`azure-static-web-apps-*.yml`) automates the deployment of the Angular frontend to Azure Static Web Apps.

## Details

- **Trigger**:
  - Manual trigger via `workflow_dispatch`.
  - Pushes to `main` or `master` branches that modify `apps/angular-portfolio/`, the root `package.json`, or the workflow itself.
- **Key Steps**:
  - **Favicon Update**: Runs `update-favicon-pack.js` to ensure the correct assets are in place.
  - **Build**: Executes `pnpm run build:portfolio-app`.
  - **Azure Deployment**: Uses `Azure/static-web-apps-deploy` to upload the build artifacts to Azure.
