import {execSync} from "node:child_process";

const hasMeJson = execSync('git diff --cached --name-only', {
    encoding: 'utf8',
})
    .split('\n')
    .filter(Boolean).some(file => file === 'apps/portfolio-backend-hono/src/data/me.json');

if(!hasMeJson){
    process.exit(0);
}

execSync('git reset HEAD apps/portfolio-backend-hono/src/data/me.json', {
    encoding: 'utf8',
    stdio: 'inherit',
});

execSync('git checkout HEAD -- apps/portfolio-backend-hono/src/data/me.json', {
    encoding: 'utf8',
    stdio: 'inherit',
});

console.log('Rolled back apps/portfolio-backend-hono/src/data/me.json to last committed version');
process.exit(0);