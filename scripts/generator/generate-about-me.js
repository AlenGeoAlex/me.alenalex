import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const ABOUT_ME_JSON_ROOT_PATH = path.join(process.cwd(), 'packages/about-me');
const REPLACEMENT_PATH = 'apps/portfolio-backend-hono/src/data/me.json';
const INITIAL_JSON = {
    "type": "directory",
    "name": "~",
    "children": []
};

const EXTENSION_TO_CONTENT_TYPE = {
    '.txt': 'text/plain',
    '.html': 'text/html',
    '.pdf': 'application/pdf',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.jpeg': 'image/jpeg',
    '.mp4': 'video/mp4',
    '.mp3': 'audio/mpeg',
    '.md': 'text/markdown',
    '.json': null
};

function getContentTypeFromExtension(filename) {
    const ext = path.extname(filename).toLowerCase();
    return EXTENSION_TO_CONTENT_TYPE[ext] || null;
}

function readFileAsBase64(filePath) {
    const buffer = fs.readFileSync(filePath);
    return buffer.toString('base64');
}

function readFileAsText(filePath) {
    return fs.readFileSync(filePath, 'utf-8');
}

function processJsonMetadata(filePath) {
    try {
        const fileContent = fs.readFileSync(filePath, 'utf-8');
        const metadata = JSON.parse(fileContent);

        if (!metadata.contentType || !metadata.content) {
            throw new Error(`JSON file ${filePath} must have contentType and content properties`);
        }

        return {
            contentType: metadata.contentType,
            content: metadata.content,
            name: metadata.name
        };
    } catch (error) {
        console.error(`Error parsing metadata JSON file ${filePath}:`, error);
        throw error;
    }
}

function buildFileNode(filePath, basePath) {
    const filename = path.basename(filePath);
    const ext = path.extname(filename).toLowerCase();

    let contentType, content, name;

    if (ext === '.json') {
        const metadata = processJsonMetadata(filePath);
        contentType = metadata.contentType;
        content = metadata.content;
        name = metadata.name || filename;
    } else {
        contentType = getContentTypeFromExtension(filename);

        if (!contentType) {
            console.warn(`Unsupported file type: ${filename}, skipping...`);
            return null;
        }

        if (contentType === 'text/plain' || contentType === 'text/html' || contentType === 'text/markdown') {
            content = readFileAsText(filePath);
        } else {
            content = readFileAsBase64(filePath);
        }

        name = filename;
    }

    return {
        type: "file",
        name: name,
        contentType: contentType,
        content: content
    };
}

function buildFileSystemTree(dirPath, basePath = dirPath) {
    const stats = fs.statSync(dirPath);
    let relativeName = path.relative(basePath, dirPath) || path.basename(dirPath);
    const fileSplits = relativeName.split('/');
    if(fileSplits.length > 1){
        relativeName = fileSplits.pop();
    }

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
        } else if (entryStat.isFile()) {
            const fileNode = buildFileNode(fullPath, basePath);
            if (fileNode) {
                children.push(fileNode);
            }
        }
    }

    return {
        type: "directory",
        name: relativeName === '' ? path.basename(dirPath) : relativeName,
        children
    };
}

if (!fs.existsSync(ABOUT_ME_JSON_ROOT_PATH)) {
    throw new Error(`Source directory not found: ${ABOUT_ME_JSON_ROOT_PATH}`);
}

try {
    console.log(`Reading files from: ${ABOUT_ME_JSON_ROOT_PATH}`);
    const tree = buildFileSystemTree(ABOUT_ME_JSON_ROOT_PATH);
    INITIAL_JSON.children = tree.children;

    const fullPath = path.join(process.cwd(), REPLACEMENT_PATH);
    const outputDir = path.dirname(fullPath);

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