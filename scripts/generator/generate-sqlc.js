import {execSync} from "node:child_process";
import fs from "node:fs";
import path from "node:path";


const DOCKER_VERSION_COMMAND = "docker version --format '{{.Server.Version}}'";
const DOCKER_CLI_COMMAND = "docker --version";
const DOCKER_SQLC_IMAGE = "sqlc/sqlc:latest";
const DOCKER_PULL_COMMAND = `docker pull ${DOCKER_SQLC_IMAGE}`
const DOCKER_DELETE_IMAGE_COMMAND = `docker rmi ${DOCKER_SQLC_IMAGE}`;

const BACKEND_ROOT_PATH = `${process.cwd()}/apps/bloggi-backend`;
const SQL_PATH = `${BACKEND_ROOT_PATH}/database`

const PG = {
    host: "host.docker.internal", // This will be run inside another container, so localhost won't work
    port: 5432,
    user: "local_test_bloggi",
    password: "local_test",
    database: "bloggi"
};

function generateSchema() {
    console.log("Generating schema.sql via pg_dump (Dockerized)...");

    const command = `
docker run --rm \
  -e PGPASSWORD=${PG.password} \
  -v ${SQL_PATH}/schema:/out \
  postgres:17 \
  pg_dump \
    --schema-only \
    --no-owner \
    --no-privileges \
    --no-comments \
    -h ${PG.host} \
    -p ${PG.port} \
    -U ${PG.user} \
    ${PG.database} \
    -f /out/schema.sql
`.trim();

    try {
        execSync(command, { stdio: 'inherit' });
        console.log("schema.sql generated successfully.");
    } catch (e) {
        console.error("Failed to generate schema.sql");
        process.exit(1);
    }
}


function ensureDockerExists(){
    try {
        execSync(DOCKER_CLI_COMMAND, { stdio: 'pipe' });
        const dockerVersion = execSync(DOCKER_VERSION_COMMAND, { stdio: 'pipe', encoding: 'utf-8' }).toString().trim();

        console.log(`Docker version: ${dockerVersion}`);
    }catch (e) {
        if (e.message.includes("ENOENT")) {
            console.error("Docker is not installed on this machine.");
        } else {
            console.error("Docker is installed, but the daemon (engine) is not running.");
            console.error("Please start Docker Desktop or the Docker service.");
        }
        process.exit(1);
    }
}

function ensureSqlCExists(){
    try {
        console.log("Pulling sqlc docker image...Please wait, this may take sometime");
        execSync(DOCKER_PULL_COMMAND, { stdio: 'inherit', encoding: 'utf-8' });
        console.log(`Pull complete...`)
    }catch (e) {
        console.error("Failed to pull sqlc docker image.");
        console.error(e);
        process.exit(1);
    }
}

function deleteSqlCImage(){
    try {
        console.log("Deleting sqlc docker image...");
        execSync(DOCKER_DELETE_IMAGE_COMMAND, { stdio: 'inherit' });
        console.log("sqlc docker image deleted successfully.");
    }catch (e){
        console.error("Failed to delete sqlc docker image.");
    }
}

function sanitizeSchemaFile() {
    const schemaPath = path.join(
        SQL_PATH,
        "schema",
        "schema.sql"
    );

    console.log("Sanitizing schema.sql (removing psql meta-commands)...");

    try {
        const content = fs.readFileSync(schemaPath, "utf8");

        const sanitized = content
            .split("\n")
            .filter(line => !line.startsWith("\\"))
            .join("\n");

        fs.writeFileSync(schemaPath, sanitized, "utf8");

        console.log("schema.sql sanitized successfully.");
    } catch (e) {
        console.error("Failed to sanitize schema.sql");
        console.error(e);
        process.exit(1);
    }
}


async function main() {
    ensureDockerExists();
    ensureSqlCExists();
    console.log('Backend path is been set to '+BACKEND_ROOT_PATH);

    console.log('Generating/Updating SQL Schema')
    generateSchema();
    console.log('Generating SQL Schema Complete');

    console.log('Sanitizing Schema Complete');
    sanitizeSchemaFile();
    console.log('Preparing to run sqlc')
    const dockerCommand = `docker run --rm -v ${BACKEND_ROOT_PATH}:/src -w /src sqlc/sqlc generate`;
    execSync(dockerCommand, { stdio: 'inherit' });
    deleteSqlCImage();
    process.exit(1);
}

await main()