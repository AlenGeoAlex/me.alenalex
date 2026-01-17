import path from "node:path";
import fs from "node:fs/promises";

const FAVICON_PATH = "assets/favicon/";
const FAVICON_REPLACEMENT_DESTINATION =
    process.env.FAVICON_REPLACEMENT_DESTINATION;

if (!FAVICON_REPLACEMENT_DESTINATION) {
    throw new Error("Missing env var FAVICON_REPLACEMENT_DESTINATION");
}

const faviconSourcePath = path.join(process.cwd(), FAVICON_PATH);
const faviconDestinationPath = path.join(
    process.cwd(),
    FAVICON_REPLACEMENT_DESTINATION
);

await fs.cp(faviconSourcePath, faviconDestinationPath, {
    recursive: true,
    force: true,
});

console.log('Favicon updated at '+faviconDestinationPath);
process.exit(0);
