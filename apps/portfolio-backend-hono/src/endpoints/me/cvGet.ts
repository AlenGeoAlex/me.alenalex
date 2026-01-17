import {OpenAPIRoute} from "chanfana";
import {AppContext} from "../../types";
import {z} from "zod";

export class CvGet extends OpenAPIRoute {
    schema = {
        tags: ["Me"],
        summary: "Get the cv response",
        request: {},
        responses: {
            "200": {
                description: "Returns the initialization status and message",
                content: {
                    "application/json": {
                        schema: z.object({
                            signedUrl: z.string()
                        }),
                    }
                }
            },
            "404": {
                description: "File not found",
                content: {
                    "application/json": {
                        schema: z.object({
                            error: z.string()
                        })
                    }
                }
            }
        }
    }

    async handle(c: AppContext) {
        const url = `${c.env.R2_PUBLIC_URL}/${c.env.R2_BUCKET}/${c.env.R2_CV_PATH}`;

        if(!url)
            return c.json({
                error: 'Failed to get the file'
            }, 404);

        return c.json({signedUrl: url});
    }
}