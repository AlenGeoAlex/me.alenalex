import { fromHono } from "chanfana";
import { Hono } from "hono";
import {CvGet, InitMe} from "./endpoints/me";
import { cors } from "hono/cors";
import {FsList} from "./endpoints/fs";
import {FsOpen} from "./endpoints/fs/fsOpen";
import {ContactMe} from "./endpoints/me/contactMe";
import {rateLimiter} from "hono-rate-limiter";
import {IPUtils} from "./utils/ip-utils";

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

app.use(
	rateLimiter<{ Bindings: Env }>({
		binding: (c) => c.env.CONTACT_ME_LIMIT,
		keyGenerator: (c) => IPUtils.getClientIP(c)
	})
);

const openapi = fromHono(app, {
	docs_url: "/",
});

//me
openapi.get('/api/me', InitMe)
openapi.get('/api/me/cv', CvGet)
openapi.post('/api/me/contact', ContactMe)

// fs
openapi.get('/api/path/:path', FsList)
openapi.get('/api/open/:fileName', FsOpen)

// Export the Hono app
export default app;
