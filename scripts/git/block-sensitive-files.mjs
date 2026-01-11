import { execSync } from 'node:child_process';

const BLOCKED_REGEXES = [
    // .env
    /(^|\/)\.env$/,

    // .env.* except .env.example
    /(^|\/)\.env\.(?!example$)[^/]+$/,
];

const stagedFiles = execSync('git diff --cached --name-only', {
    encoding: 'utf8',
})
    .split('\n')
    .filter(Boolean);

const blocked = stagedFiles.filter(file =>
    BLOCKED_REGEXES.some(rx => rx.test(file))
);

if (blocked.length > 0) {
    console.error('\n[X] Commit blocked: sensitive files detected:\n');

    blocked.forEach(f => console.error(`  • ${f}`));

    console.error('\nUnstage them before committing:');
    blocked.forEach(f =>
        console.error(`  git restore --staged ${f}`)
    );

    process.exit(1);
}
