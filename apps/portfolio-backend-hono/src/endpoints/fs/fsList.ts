import {Bool, OpenAPIRoute, Str} from "chanfana";
import {z} from "zod";
import {AppContext, DirectoryNode, FSNode} from "../../types";
import content from '../../data/me.json'

const DirectorySchema = z.object({
    type: z.literal("directory"),
    name: z.string(),
    children: z.array(z.object({
        type: z.enum(["file", "directory"]),
        name: z.string()
    }))
});

const ErrorSchema = z.object({
    error: z.string()
});

export class FsList extends OpenAPIRoute {
    schema = {
        tags: ["Path"],
        summary: "List all paths",
        request: {
            params: z.object({
                path: Str({
                    description: "Dynamic path to directory (e.g., 'folder1/folder2')",
                    required: false,
                    default: ""
                })
            }),
            query: z.object({
                head: Bool({
                    description: "Just check if directory exists (returns 200 if exists, 404 if not, 400 if file, no body)",
                    required: false,
                    default: false
                })
            })
        },
        responses: {
            "200": {
                description: "Success - directory exists and listing returned (when head=false) or directory exists (when head=true, no body)",
                content: {
                    "application/json": {
                        schema: DirectorySchema
                    }
                }
            },
            "400": {
                description: "Bad Request - Path is a file, not a directory",
                content: {
                    "application/json": {
                        schema: ErrorSchema
                    }
                }
            },
            "404": {
                description: "Not Found - Path does not exist",
                content: {
                    "application/json": {
                        schema: ErrorSchema
                    }
                }
            }
        }
    }

    async handle(c: AppContext)  {
        const data = await this.getValidatedData<typeof this.schema>();
        const url = new URL(c.req.url);
        console.log(url.pathname)
        let pathSplit = url.pathname.split('/');

        // First, it starts with / so check if a path is empty
        if(pathSplit[0] === '')
            pathSplit = pathSplit.slice(1)

        if(pathSplit.length <= 2)
            //This should never happen, but just in case
            return c.json({
                error: 'Invalid path'
            }, 500);

        if(pathSplit[0] !== 'api' || pathSplit[1] !== 'path')
            return c.json({error: 'Forbidden path'}, 500)

        pathSplit = pathSplit.slice(2)
        let currentChildren = content.children as FSNode[];
        let currentPathStr = '~'
        for (let eachPath of pathSplit) {
            let next = currentChildren.find(x => x.name === eachPath);
            currentPathStr += `/${eachPath}`
            if(!next)
                return c.json({
                    error: 'No directory found at ' + currentPathStr
                }, 404)

            if(next.type === 'file')
                return c.json({
                    error: 'Path is a file, not a directory'
                }, 400)

            currentChildren = next.children as FSNode[];
        }

        if (!data.query.head) {
            return c.json({
                type: 'directory',
                name: currentPathStr,
                children: currentChildren.map(x => ({
                    type: x.type,
                    name: x.name
                }))
            })
        }

        return c.body(undefined, 200);
    }
}