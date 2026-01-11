# Angular Portfolio

This is the personal portfolio application built with Angular. It features a unique terminal-like interface and a modern, responsive design.

## Features

- **Terminal Interface**: A functional terminal emulator that allows users to interact with the portfolio via commands.
- **Responsive Design**: Built using Tailwind CSS for a seamless experience across devices.
- **Modern Tech Stack**: Uses Angular 21, Motion (formerly Framer Motion for web), and Lucide icons.
- **Interactive Components**: Includes a custom terminal component with command history and context-aware responses.

## Tech Stack

- **Framework**: [Angular](https://angular.io/) (v21)
- **Styling**: [Tailwind CSS](https://tailwindcss.com/)
- **Animations**: [Motion](https://motion.dev/)
- **Icons**: [Lucide Angular](https://lucide.dev/), [Simple Icons](https://simpleicons.org/)
- **Parsing**: [Marked](https://marked.js.org/) (for Markdown support)

## Key Components

### Terminal Component

The heart of the portfolio is the `TerminalComponent`. It's located in `src/app/components/terminal-component/`.

- **Header**: Displays the window controls and title.
- **Body**: Handles the command input and output area.
- **Contexts**:
  - `current-context`: Manages the active command line.
  - `response-context`: Displays command results (supports HTML/Markdown).
  - `error-response-context`: Displays error messages.
- **Help Bar**: Provides quick access to available commands.

## Available Commands (in the Terminal)

- `help`: Lists all available commands.
- `clear`: Clears the terminal screen.
- `cd <directory>`: Changes the current directory context.
- `ls`: Lists files (simulated).
- `about`: Displays information about me.

## Development

To start the development server for this app:

```bash
pnpm run ng serve
```
or from the root:
```bash
pnpm dev:portfolio
```
