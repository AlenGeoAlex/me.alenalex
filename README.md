# me.alenalex

This repository is a monorepo containing various personal projects including a portfolio and a blog system.

## Project Structure

The project is organized into the following directories:

- `apps/`: Contains the main applications and APIs.
  - `angular-portfolio`: The main portfolio website built with Angular.
  - `angular-blog`: A blog frontend built with Angular (Work in Progress).
  - `blog-api`: Backend API for the blog system (Work in Progress).
  - `portfolio-backend-hono`: Backend API for the portfolio built with Hono.
- `docs/`: Detailed documentation for each component of the monorepo.

## Getting Started

### Prerequisites

- [pnpm](https://pnpm.io/) (v10.x recommended)
- Node.js

### Installation

```bash
pnpm install
```

### Development

To run the angular-portfolio:

```bash
pnpm run dev:portfolio-backend
```

## Documentation

For more detailed information about each application and the repository structure, please refer to the `docs/` folder:

- [Angular Portfolio Documentation](docs/angular-portfolio/README.md)
- [Angular Blog Documentation](docs/angular-blog/README.md)
- [Blog API Documentation](docs/blog-api/README.md)
- [Portfolio API Documentation](docs/portfolio-api/README.md)
- [GitHub Workflows](docs/WORKFLOWS.md)
- [Scripts Documentation](docs/SCRIPTS.md)
