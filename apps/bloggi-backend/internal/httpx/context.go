package httpx

type contextKey string

const (
	BodyKey       contextKey = "validated_body"
	UserClaimsKey contextKey = "user_claims"
)
