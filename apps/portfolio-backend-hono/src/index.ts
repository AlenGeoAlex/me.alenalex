import { fromHono } from "chanfana";
import { Hono } from "hono";
import {InitMe} from "./endpoints/me";
import { cors } from "hono/cors";

const app = new Hono<{ Bindings: Env }>();
app.use(async (c, next) => {
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
		console.log(origin);
		return allowedHosts.includes(origin)
	},
	allowMethods: ['GET', 'POST', 'OPTIONS']
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

// Register OpenAPI endpoints
// openapi.get("/api/tasks", TaskList);
// openapi.post("/api/tasks", TaskCreate);
// openapi.get("/api/tasks/:taskSlug", TaskFetch);
// openapi.delete("/api/tasks/:taskSlug", TaskDelete);

//me
openapi.get('/api/me', InitMe)

// Export the Hono app
export default app;
