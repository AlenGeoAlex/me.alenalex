package httpx

import (
	"bloggi-backend/config"
	"context"
	"fmt"
	"net/http"
	"strings"

	"github.com/golang-jwt/jwt/v5"
)

type UserClaims struct {
	UserID string
	Email  string
	Role   string
}

type AuthMiddleware struct {
	jwtConfig config.JwtConfig
}

func NewAuthMiddleware(jwtCfg config.JwtConfig) *AuthMiddleware {
	return &AuthMiddleware{
		jwtConfig: jwtCfg,
	}
}

func (a *AuthMiddleware) RequireAuth() func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			authHeader := r.Header.Get("Authorization")
			if authHeader == "" {
				http.Error(w, "missing authorization header", http.StatusUnauthorized)
				return
			}

			parts := strings.Split(authHeader, " ")
			if len(parts) != 2 || parts[0] != "Bearer" {
				http.Error(w, "invalid authorization header format", http.StatusUnauthorized)
				return
			}

			token := parts[1]

			claims, err := a.validateJWT(token)
			if err != nil {
				http.Error(w, "invalid or expired token", http.StatusUnauthorized)
				return
			}

			ctx := context.WithValue(r.Context(), UserClaimsKey, claims)
			next.ServeHTTP(w, r.WithContext(ctx))
		})
	}
}

func (a *AuthMiddleware) validateJWT(tokenString string) (*UserClaims, error) {
	token, err := jwt.Parse(tokenString, func(token *jwt.Token) (interface{}, error) {
		if _, ok := token.Method.(*jwt.SigningMethodEd25519); !ok {
			return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
		}
		return []byte(a.jwtConfig.Secret), nil
	})

	if err != nil {
		return nil, fmt.Errorf("failed to parse token: %w", err)
	}

	if !token.Valid {
		return nil, fmt.Errorf("invalid token")
	}

	// Extract claims
	claims, ok := token.Claims.(jwt.MapClaims)
	if !ok {
		return nil, fmt.Errorf("invalid claims format")
	}

	// Verify issuer
	if iss, ok := claims["iss"].(string); !ok || iss != a.jwtConfig.Issuer {
		return nil, fmt.Errorf("invalid issuer")
	}

	// Extract user claims
	userID, ok := claims["user_id"].(string)
	if !ok {
		return nil, fmt.Errorf("missing user_id claim")
	}

	email, ok := claims["email"].(string)
	if !ok {
		return nil, fmt.Errorf("missing email claim")
	}

	role, _ := claims["role"].(string)

	return &UserClaims{
		UserID: userID,
		Email:  email,
		Role:   role,
	}, nil
}

func GetUserClaims(r *http.Request) *UserClaims {
	claims, ok := r.Context().Value(UserClaimsKey).(*UserClaims)
	if !ok {
		return nil
	}
	return claims
}
