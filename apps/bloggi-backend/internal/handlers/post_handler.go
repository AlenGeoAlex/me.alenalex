package handlers

import (
	"bloggi-backend/internal/httpx"
	"bloggi-backend/internal/services"
	"log/slog"
	"net/http"
)

type PostHandler struct {
	logger        *slog.Logger
	authorService *services.AuthorService
	postService   *services.PostService
}

type CreatePostRequest struct {
	Title     string `json:"title" validate:"required,min=5,max=200"`
	Published bool   `json:"published"`
}

func NewPostHandler(logger *slog.Logger, authorService *services.AuthorService, postService *services.PostService) *PostHandler {
	return &PostHandler{logger: logger, authorService: authorService, postService: postService}
}

func (h *PostHandler) GetPost(w http.ResponseWriter, r *http.Request) {

}

func (h *PostHandler) CreatePost(w http.ResponseWriter, r *http.Request) {
	req := httpx.GetBody[CreatePostRequest](r)
}

func (h *PostHandler) UpdatePost(w http.ResponseWriter, r *http.Request) {

}

func (h *PostHandler) ListPosts(w http.ResponseWriter, r *http.Request) {

}
