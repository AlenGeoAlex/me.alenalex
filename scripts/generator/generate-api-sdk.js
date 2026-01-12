import AdmZip from 'adm-zip';
import * as readline from "node:readline";
import path from "path";
import fs from "fs";
import {execSync} from "node:child_process";

// Helper Functions
const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

const askQuestion = (question) => {
    return new Promise((resolve) => {
        rl.question(question, (answer) => {
            resolve(answer);
        });
    });
};

const API_URL = "https://api.openapi-generator.tech/api/gen/clients/typescript-angular";
const API_OPTIONS = {
    openAPIUrl: '',
    ngVersion: '21',
    snapshot: false,
    modelSuffix: 'IApi',
    withInterfaces: true,
}
const OUTPUT_DIR = 'apps/angular-portfolio/src/api/generated-sdk/'
const BLACKLISTED_FILES = [
    'README.md',
    '__MACOSX',
    'git_push.sh',
    '.gitignore'
]

const main = async () => {
    let response = await askQuestion("Enter open-api spec url: (defaults: https://portfolio-backend-hono.alengeoalex123.workers.dev/openapi.json)");

    if(!response || response.trim().length === 0) {
        response = 'https://portfolio-backend-hono.alengeoalex123.workers.dev/openapi.json';
    }

    let responseUrl
    try{
        responseUrl = new URL(response);
    }catch (e){
        throw new Error("Invalid open-api spec URL provided");
    }

    if(!responseUrl)
        throw new Error("Failed to validate open-api spec URL provided");

    API_OPTIONS.openAPIUrl = response;
    const generatedClientResponse = await fetch(API_URL, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(API_OPTIONS)
    });

    if(!generatedClientResponse.ok)
    {
        const error = await generatedClientResponse.text();
        throw new Error("Failed to generate SDK: "+error);
    }

    const generatedClientResponseJson = await generatedClientResponse.json();
    const generatedClientDownloadLink = generatedClientResponseJson['link'];
    if(!generatedClientDownloadLink)
        throw new Error("Failed to generate SDK: No download link provided");

    const clientCodeResponse = await fetch(generatedClientDownloadLink);
    if(!clientCodeResponse.ok){
        const error = await clientCodeResponse.text();
        throw new Error("Failed to download generated SDK: "+error);
    }

    const zipBufferData = Buffer.from(await clientCodeResponse.arrayBuffer());
    const zip = new AdmZip(zipBufferData);

    for (let entry of zip.getEntries()) {
        const parts = entry.entryName.split(/\/|\\/);
        const fileName = parts[parts.length - 1];
        if(BLACKLISTED_FILES.includes(fileName)) {
            console.log("Skipping blacklisted file: "+fileName);
            continue;
        }

        if (parts.length > 1) {
            parts[0] = OUTPUT_DIR;
        } else {
            parts.unshift(OUTPUT_DIR);
        }

        const filePath = path.join(...parts);

        if (entry.isDirectory) {
            fs.mkdirSync(filePath, { recursive: true });
        } else {
            fs.mkdirSync(path.dirname(filePath), { recursive: true });
            fs.writeFileSync(filePath, entry.getData());
        }
    }

    // console.log('SDK unzipped successfully.')
    // console.log('Reinstalling dependencies...')
    // execSync('pnpm install', {cwd: process.cwd(), stdio: 'inherit'});
    // console.log('SDK installed successfully.')
    //
    // console.log('Building SDK for distribution...')
    // execSync('pnpm build:portfolio-sdk', {cwd: process.cwd(), stdio: 'inherit'})
    // console.log('SDK built successfully.')

    console.log('SDK generated successfully.')
    process.exit(0);
}

await main();