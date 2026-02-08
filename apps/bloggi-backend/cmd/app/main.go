package main

import (
	"bloggi-backend/config"
	bloggidb "bloggi-backend/database/go/bloggi"
	"bloggi-backend/internal/handlers"
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

	// Services
	authorService := services.NewAuthorService(queries, logger)

	// Handlers
	authorHandler := handlers.NewAuthorHandler(logger, authorService)

	mux := http.NewServeMux()
	mux.HandleFunc("GET /author/{authorId}", authorHandler.GetAuthor)

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
