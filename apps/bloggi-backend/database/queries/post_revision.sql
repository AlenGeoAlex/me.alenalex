-- name: CreateRevision :exec
INSERT INTO public.post_revision (id, post_id, published, change_log, revision_date)
VALUES ($1, $2, $3, $4, $5)
RETURNING *;