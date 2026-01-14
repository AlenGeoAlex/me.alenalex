import {Bool, OpenAPIRoute, Str} from "chanfana";
import {z} from "zod";
import {AppContext, FSNode} from "../../types";
import content from "../../data/me.json";

export class FsOpen extends OpenAPIRoute {

    schema = {
        tags: ["Path"],
        summary: "Open a file",
        request: {
            params: z.object({
                fileName: Str({
                    description: "Dynamic path to directory (e.g., 'folder1/folder2')",
                    required: false,
                    default: ""
                })
            }),
            query: z.object({
                download: Bool({
                    description: "Should download the data or not",
                    required: false,
                    default: false
                })
            })
        },
        responses: {
            "200": {
                description: "Success - file opened",
                content: {
                    "application/json": { schema: z.object({
                            contentType: z.string(),
                            content: z.string()
                        }) },

                }
            },
            "404": {
                description: "Not Found - Path does not exist",
                content: {
                    "application/json": { schema: z.object({error: z.string()}) }
                }
            }
        }
    }

    async handle(c: AppContext) {
        try {
            const data = await this.getValidatedData<typeof this.schema>();
            let pathSplit = data.params.fileName.split('/');
            let currentChildren = content.children as FSNode[];
            let currentPathStr = '~'
            const fileName = pathSplit.pop();


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

            const dataNode = currentChildren.find(x => x.name === fileName && x.type === 'file');
            if(!dataNode || dataNode.type !== 'file')
                return c.json({
                    error: 'No file found at ' + currentPathStr
                }, 404)

            return c.json({
                contentType: dataNode.contentType,
                content: dataNode.content
            }, 200)
        }catch (e){
            console.error(e)
        }
    }

}