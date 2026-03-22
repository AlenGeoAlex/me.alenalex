package httpx

import "net/http"

func GetBody[T any](r *http.Request) T {

	body, ok := r.Context().Value(BodyKey).(T)
	if !ok {
		panic("validated body not found in context")
	}

	return body
}
