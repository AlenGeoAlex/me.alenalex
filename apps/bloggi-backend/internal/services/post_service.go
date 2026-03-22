package services

import (
	bloggidb "bloggi-backend/database/go/bloggi"
	"log/slog"
)

type PostService struct {
	logger *slog.Logger
	db     *bloggidb.Queries
}

type CreatePostInput struct {
	Title        string
	AuthoredDate string
	AuthorID     string
	Published    bool
}

func NewPostService(logger *slog.Logger, db *bloggidb.Queries) *PostService {
	return &PostService{logger: logger, db: db}
}

func (service *PostService) CreatePost(postInput CreatePostInput) (bloggidb.Post, error) {

}
