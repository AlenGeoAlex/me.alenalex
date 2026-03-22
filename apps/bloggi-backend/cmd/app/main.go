package main

import (
	"bloggi-backend/config"
	bloggidb "bloggi-backend/database/go/bloggi"
	"bloggi-backend/internal/handlers"
	"bloggi-backend/internal/httpx"
	"bloggi-backend/internal/services"
	"context"
	"log/slog"
	"net/http"
	"os"
	"strconv"

	"github.com/jackc/pgx/v5/pgxpool"
)

func main() {
	config := config.NewConfig()
	logger := slog.New(
		slog.NewJSONHandler(os.Stdout, nil),
	)
	connectionString := "postgres://" + config.Database.Username + ":" + config.Database.Password + "@" + config.Database.Host + ":" + strconv.Itoa(config.Database.Port) + "/" + config.Database.Database + "?sslmode=disable"
	pool, err := pgxpool.New(context.Background(), connectionString)
	if err != nil {
		logger.Error("Failed to connect to database", "error", err)
		os.Exit(1)
		return
	}
	defer pool.Close()

	queries := bloggidb.New(pool)

	//Middlewares
	authMiddleware := httpx.NewAuthMiddleware(config.Jwt)

	// Services
	authorService := services.NewAuthorService(queries, logger)
	postService := services.NewPostService(logger, queries)

	// Handlers
	authorHandler := handlers.NewAuthorHandler(logger, authorService)
	postHandler := handlers.NewPostHandler(logger, authorService, postService)
	postRevisionHandler := handlers.NewPostRevisionHandler(logger, postService)

	mux := http.NewServeMux()
	mux.HandleFunc("GET /author/{authorId}", authorHandler.GetAuthor)

	mux.HandleFunc("GET /post/", postHandler.ListPosts)
	mux.HandleFunc("GET /post/{postId}", postHandler.GetPost)
	mux.Handle("POST /post/", authMiddleware.RequireAuth()(
		httpx.ValidateBody[handlers.CreatePostRequest](
			http.HandlerFunc(postRevisionHandler.CreatePostRevision),
		),
	),
	)
	mux.HandleFunc("PUT /post/{postId}", postHandler.UpdatePost)

	mux.HandleFunc("POST /post/{postId}/revision", postRevisionHandler.CreatePostRevision)
	mux.HandleFunc("GET /post/{postId}/revision/{revisionId}", postRevisionHandler.GetPostRevision)
	mux.HandleFunc("GET /post/{postId}/revision", postRevisionHandler.ListPostRevisions)
	mux.HandleFunc("PUT /post/{postId}/revision/{revisionId}", postRevisionHandler.UpdatePostRevision)

	server := &http.Server{
		Addr:    ":8080",
		Handler: mux,
	}

	logger.Info("Server started", "port", 8080)

	err = server.ListenAndServe()
	if err != nil {
		logger.Error("Server failed", "error", err)
	}
}
