-- name: GetAuthor :one
SELECT * FROM author
WHERE id = $1;