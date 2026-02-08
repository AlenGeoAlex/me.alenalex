package config

import (
	"github.com/caarlos0/env/v11"
	"github.com/joho/godotenv"
)

type Config struct {
	Database DBConfig
}

type DBConfig struct {
	Host     string `env:"DB_HOST,required"`
	Port     int    `env:"DB_PORT,required"`
	Username string `env:"DB_USERNAME,required"`
	Password string `env:"DB_PASSWORD,required"`
	Database string `env:"DB_DATABASE,required"`
	Debug    bool   `env:"DB_DEBUG"`
}

func NewConfig() *Config {
	_ = godotenv.Load()
	var config Config
	err := env.Parse(&config)
	if err != nil {
		panic(err)
	}

	return &config
}

func NewDBConfig() DBConfig {
	_ = godotenv.Load()
	var config DBConfig
	err := env.Parse(&config)
	if err != nil {
		panic(err)
	}

	return config
}
