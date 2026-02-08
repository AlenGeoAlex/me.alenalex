package services

import (
	bloggidb "bloggi-backend/database/go/bloggi"
	"context"
	"log/slog"
)

type AuthorService struct {
	db  *bloggidb.Queries
	log *slog.Logger
}

func (service *AuthorService) GetAuthor(id string) (bloggidb.Author, error) {
	service.log.Info("Trying to get author with id", "id", id)
	author, err := service.db.GetAuthor(context.Background(), id)
	if err != nil {
		return bloggidb.Author{}, err
	}

	service.log.Info("Author found", "authorId", author.ID)
	return author, nil
}

func NewAuthorService(db *bloggidb.Queries, logger *slog.Logger) *AuthorService {
	return &AuthorService{db: db, log: logger}
}
