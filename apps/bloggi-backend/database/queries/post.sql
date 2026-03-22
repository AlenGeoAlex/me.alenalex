-- name: CreatePost :exec
INSERT INTO public.post (id, title, authored_date, author_id, published)
VALUES ($1, $2, $3, $4, $5)
RETURNING *;