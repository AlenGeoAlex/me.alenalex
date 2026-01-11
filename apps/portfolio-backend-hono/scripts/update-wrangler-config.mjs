import {existsSync, readFileSync, writeFileSync} from "node:fs";
import {join} from "path";
import set from 'lodash-es/set.js';

const rootDirectory = process.cwd();
if (rootDirectory.includes("apps")) {
    throw new Error(
        "Execute this script from the root directory of the project, not inside apps"
    );
}

const wranglerConfigPath = join(
    rootDirectory,
    "apps",
    "portfolio-backend-hono",
    "wrangler.jsonc"
);
if (!existsSync(wranglerConfigPath)) {
    throw new Error("Wrangler config not found at " + wranglerConfigPath);
}

function getWranglerEnvs() {
    const envs = Object.keys(process.env).filter((key) =>
        key.startsWith("WRANGLER-CFG-")
    );
    if (envs.length === 0) {
        console.warn("No wrangler envs found");
        return {};
    }

    const envsObj = {};

    envs.forEach((env) => {
        const configValue = env.replace("WRANGLER-CFG-", "");
        envsObj[configValue] = process.env[env];
    });

    return envsObj;
}


const fileContent = readFileSync(wranglerConfigPath, "utf-8");
const config = JSON.parse(fileContent);

const wranglerEnvs = getWranglerEnvs();
Object.keys(wranglerEnvs).forEach((key) => {
    set(config, key, wranglerEnvs[key]);
    console.log(`Updated wrangler config: ${key}`);
})

const newFileContent = JSON.stringify(config, null, 2);
console.log("Writing wrangler config...");
writeFileSync(wranglerConfigPath, newFileContent, 'utf-8')
console.log("Wrangler config updated successfully!");