# Generator Scripts

These scripts automate the generation of code and data.

- **`scripts/generator/generate-api-sdk.js`**: Automates the generation of the Angular SDK from the backend's OpenAPI specification. It downloads the generated code from the OpenAPI Generator API and unzips it into the correct directory.
  - Usage: `pnpm run gen:sdk`
- **`scripts/generator/generate-about-me.js`**: Generates the `me.json` data used by the portfolio backend.
  - Usage: `pnpm run generate:about-me`
