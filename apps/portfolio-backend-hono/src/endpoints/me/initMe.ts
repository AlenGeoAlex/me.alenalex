import {OpenAPIRoute} from "chanfana";
import {z} from "zod";
import type {AppContext} from "../../types";
import initData from '../../data/initialize.json'

function renderTemplate(template: string, data: Record<string, string>) {
    return template.replace(/\{\{(\w+)\}\}/g, (_, key) => data[key] ?? '');
}

export class InitMe extends OpenAPIRoute {
    schema = {
        tags: ["Me"],
        summary: "Initialize the current user",
        responses: {
            "200": {
                description: "Returns the initialization status and message",
                content: {
                    "text/plain": { schema: z.string() },
                    "text/html": { schema: z.string() },
                    "text/markdown": { schema: z.string() }
                }
            }
        }
    }

    async handle(c: AppContext) {
        const file = initData;
        if (!file) return c.text('File not found', 404);

        const replacements: Record<string, string> = {
            platform: "GNU/Developer Linux",
            kernel: "6.∞.dev",
            arch: "human64",
            portfolioUrl: "https://www.alenalex.me",
            githubUrl: "https://github.com/AlenGeoAlex",
            contactEmail: "contact@alenalex.me",
            currentDate: new Date().toUTCString(),
            systemLoad: "99.9%",
            activeProjects: "-",
            memoryUsage: "42%",
            coffeeLevel: "LOW",
            diskUsage: "12.4%",
            bugsAlive: "∞",
            ipv4: "127.0.0.1",
            visitors: "1",
            lastLogin: "Fri Jan 9 11:56:18 2026"
        };

        const content = renderTemplate(file.content, replacements);

        return c.body(content, 200, { "Content-Type": file.contentType });
    }
}
