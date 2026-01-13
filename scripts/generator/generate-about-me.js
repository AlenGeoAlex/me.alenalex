import fs from 'fs';
import path from 'path';

const ABOUT_ME_JSON_ROOT_PATH = path.join(process.cwd(), 'packages/about-me');
const REPLACEMENT_PATH = 'apps/portfolio-backend-hono/src/data/me.json';
const INITIAL_JSON = {
    "type": "directory",
    "name": "~",
    "children": []
};

const VALID_CONTENT_TYPES = [
    "text/plain",
    "text/html",
    "application/pdf",
    "image/png",
    "image/jpeg",
    "video/mp4",
    "audio/mpeg",
    "text/markdown"
];

function isValidContentType(contentType) {
    return VALID_CONTENT_TYPES.includes(contentType);
}

function validateFileNode(obj) {
    return (
        obj &&
        obj.type === "file" &&
        typeof obj.name === "string" &&
        obj.name.length > 0 &&
        typeof obj.contentType === "string" &&
        isValidContentType(obj.contentType) &&
        typeof obj.content === "string"
    );
}

function validateDirectoryNode(obj) {
    return (
        obj &&
        obj.type === "directory" &&
        typeof obj.name === "string" &&
        obj.name.length > 0 &&
        Array.isArray(obj.children) &&
        obj.children.every(child => validateFSNode(child))
    );
}

function validateFSNode(obj) {
    return validateFileNode(obj) || validateDirectoryNode(obj);
}

function buildFileSystemTree(dirPath, basePath = dirPath) {
    const stats = fs.statSync(dirPath);
    const relativeName = path.relative(basePath, dirPath) || path.basename(dirPath);

    if (!stats.isDirectory()) {
        throw new Error(`${dirPath} is not a directory`);
    }

    const children = [];
    const entries = fs.readdirSync(dirPath);

    for (const entry of entries) {
        const fullPath = path.join(dirPath, entry);
        const entryStat = fs.statSync(fullPath);

        if (entryStat.isDirectory()) {
            children.push(buildFileSystemTree(fullPath, basePath));
        } else if (entryStat.isFile() && entry.endsWith('.json')) {
            const fileContent = fs.readFileSync(fullPath, 'utf-8');
            try {
                const parsedContent = JSON.parse(fileContent);

                if (validateFSNode(parsedContent)) {
                    children.push(parsedContent);
                } else {
                    console.error(`Invalid FSNode in file: ${fullPath}`);
                    throw new Error(`Validation failed for ${fullPath}`);
                }
            } catch (error) {
                console.error(`Error parsing JSON file ${fullPath}:`, error);
                throw error;
            }
        }
    }

    return {
        type: "directory",
        name: relativeName === '' ? path.basename(dirPath) : relativeName,
        children
    };
}

// Main execution
if (!fs.existsSync(ABOUT_ME_JSON_ROOT_PATH)) {
    throw new Error(`Source directory not found: ${ABOUT_ME_JSON_ROOT_PATH}`);
}

try {
    console.log(`Reading JSON files from: ${ABOUT_ME_JSON_ROOT_PATH}`);
    const tree = buildFileSystemTree(ABOUT_ME_JSON_ROOT_PATH);
    INITIAL_JSON.children = tree.children;

    if (!validateDirectoryNode(INITIAL_JSON)) {
        throw new Error('Final JSON structure validation failed');
    }

    const fullPath = path.join(process.cwd(), REPLACEMENT_PATH);
    const outputDir = path.dirname(fullPath)
    if (!fs.existsSync(outputDir)) {
        fs.mkdirSync(outputDir, { recursive: true });
    }

    console.log(`Writing file system tree to: ${fullPath}`);
    fs.writeFileSync(fullPath, JSON.stringify(INITIAL_JSON, null, 2), 'utf-8');

    console.log(`Successfully wrote file system tree to: ${REPLACEMENT_PATH}`);
    console.log(`Total children: ${INITIAL_JSON.children.length}`);

} catch (error) {
    console.error('Error building file system tree:', error);
    process.exit(1);
}