-- name: GetAuthor :one
SELECT * FROM public.author
WHERE id = $1 LIMIT 1;