package handlers

import (
	"bloggi-backend/internal/services"
	"encoding/json"
	"log/slog"
	"net/http"
)

type AuthorHandler struct {
	logger        *slog.Logger
	authorService *services.AuthorService
}

func NewAuthorHandler(logger *slog.Logger, authorService *services.AuthorService) *AuthorHandler {
	return &AuthorHandler{
		logger:        logger,
		authorService: authorService,
	}
}

// GetAuthor Get Author
func (h *AuthorHandler) GetAuthor(w http.ResponseWriter, r *http.Request) {
	authorId := r.PathValue("authorId")

	if authorId == "" {
		h.respondError(w, http.StatusBadRequest, "authorId is required")
		return
	}

	author, err := h.authorService.GetAuthor(authorId)
	if err != nil {
		h.respondError(w, http.StatusInternalServerError, err.Error())
		return
	}

	h.respondJSON(w, http.StatusOK, author)
}

func (h *AuthorHandler) respondJSON(w http.ResponseWriter, status int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	json.NewEncoder(w).Encode(data)
}

func (h *AuthorHandler) respondError(w http.ResponseWriter, status int, message string) {
	h.respondJSON(w, status, map[string]string{"error": message})
}
