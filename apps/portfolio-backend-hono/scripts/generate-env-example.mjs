import fs from 'node:fs';
import path from 'node:path';
import { execSync } from 'node:child_process';

const appDir = path.resolve(
    process.cwd(),
    'apps/portfolio-backend-hono'
);

const envFile = path.join(appDir, '.env');
const exampleFile = path.join(appDir, '.env.example');

if (!fs.existsSync(envFile)) {
    console.log('ℹ️  No .env found, skipping .env.example generation');
    process.exit(0);
}

const env = fs.readFileSync(envFile, 'utf8');

const lines = env
    .split('\n')
    .map(line => line.trim())
    .filter(line => line && !line.startsWith('#'))
    .map(line => {
        const eqIndex = line.indexOf('=');
        if (eqIndex === -1) return null;

        const key = line.slice(0, eqIndex).trim();
        const rest = line.slice(eqIndex + 1).trim();

        // Preserve inline comments
        const commentIndex = rest.indexOf('#');
        const comment =
            commentIndex !== -1
                ? ' ' + rest.slice(commentIndex)
                : '';

        return `${key}=${comment}`;
    })
    .filter(Boolean);

const content = [
    '# Auto-generated from .env',
    '# Do not commit secrets',
    '',
    ...lines,
    '',
].join('\n');

fs.writeFileSync(exampleFile, content);

execSync(`git add ${exampleFile}`);

console.log('.env.example updated/created.');
