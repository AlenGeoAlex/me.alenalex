package handlers

import (
	"bloggi-backend/internal/services"
	"log/slog"
	"net/http"
)

type PostRevisionHandler struct {
	logger      *slog.Logger
	postService *services.PostService
}

func NewPostRevisionHandler(logger *slog.Logger, postService *services.PostService) *PostRevisionHandler {
	return &PostRevisionHandler{logger: logger, postService: postService}
}

func (h *PostRevisionHandler) CreatePostRevision(w http.ResponseWriter, r *http.Request) {
}

func (h *PostRevisionHandler) GetPostRevision(w http.ResponseWriter, r *http.Request) {}

func (h *PostRevisionHandler) ListPostRevisions(w http.ResponseWriter, r *http.Request) {}

func (h *PostRevisionHandler) UpdatePostRevision(w http.ResponseWriter, r *http.Request) {}
