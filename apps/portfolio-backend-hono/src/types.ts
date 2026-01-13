import { DateTime, Str } from "chanfana";
import type { Context } from "hono";
import { z } from "zod";

export type AppContext = Context<{ Bindings: Env }>;

export const Task = z.object({
	name: Str({ example: "lorem" }),
	slug: Str(),
	description: Str({ required: false }),
	completed: z.boolean().default(false),
	due_date: DateTime(),
});

export type ContentType =
	| "text/plain"
	| "text/html"
	| "application/pdf"
	| "image/png"
	| "image/jpeg"
	| "video/mp4"
	| "audio/mpeg"
	| "text/markdown";

export interface FileNode {
	type: "file";
	name: string;
	contentType: ContentType;
	content: string;
}

export interface DirectoryNode {
	type: "directory";
	name: string;
	children: FSNode[];
}

export type FSNode = FileNode | DirectoryNode;

