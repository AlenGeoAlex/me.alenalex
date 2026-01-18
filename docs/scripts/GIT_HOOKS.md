# Git Hook Scripts

These scripts are used to maintain repository integrity and security.

- **`scripts/git/block-sensitive-files.mjs`**: A git hook script to prevent accidental commitment of sensitive files (like `.env`).
- **`scripts/git/revert-me-json.js`**: Reverts changes to `me.json` to prevent accidental commits of local modifications.
