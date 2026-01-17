import { fromHono } from "chanfana";
import { Hono } from "hono";
import {InitMe} from "./endpoints/me";
import { cors } from "hono/cors";
import {FsList} from "./endpoints/fs";
import {FsOpen} from "./endpoints/fs/fsOpen";

const app = new Hono<{ Bindings: Env }>();
app.use(async (c, next) => {
	const url = new URL(c.req.url);
	if (url.pathname === '/openapi.json') {
		return await next();
	}

	const hostHeader = c.req.header('Origin');
	const allowedHosts = c.env.ALLOWED_HOST?.split(',');
	if(!allowedHosts.includes(hostHeader)) {
		return c.text(undefined, 401)
	}

	await next();
})

app.use(cors({
	origin: (origin, c) => {
		const allowedHosts = c.env.ALLOWED_HOST?.split(',');
		return allowedHosts.includes(origin) ? origin : undefined;
	},
}))

app.use(async (c, next) => {
	const start = performance.now()
	await next()
	const end = performance.now()
	c.res.headers.set('X-Response-Time', `${end - start}`)
});

const openapi = fromHono(app, {
	docs_url: "/",
});

//me
openapi.get('/api/me', InitMe)
openapi.get('/api/me/cv', InitMe)

// fs
openapi.get('/api/path/:path', FsList)
openapi.get('/api/open/:fileName', FsOpen)

// Export the Hono app
export default app;
