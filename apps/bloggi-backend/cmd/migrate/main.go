package main

import (
	config2 "bloggi-backend/config"
	"errors"
	"flag"
	"log"
	"strconv"

	"github.com/golang-migrate/migrate/v4"
	_ "github.com/golang-migrate/migrate/v4/database/postgres"
	_ "github.com/golang-migrate/migrate/v4/source/file"
)

func main() {
	var direction string
	flag.StringVar(&direction, "direction", "up", "migration direction: up or down")
	flag.Parse()

	config := config2.NewDBConfig()
	var connectionUrl = "postgres://" + config.Username + ":" + config.Password + "@" + config.Host + ":" + strconv.Itoa(config.Port) + "/" + config.Database + "?sslmode=disable"

	m, err := migrate.New(
		"file://database/migrations/",
		connectionUrl,
	)
	if err != nil {
		log.Fatal(err)
		return
	}

	if direction == "up" {
		if err := m.Up(); err != nil && !errors.Is(err, migrate.ErrNoChange) {
			log.Fatal(err)
		}
	} else {
		if err := m.Down(); err != nil && !errors.Is(err, migrate.ErrNoChange) {
			log.Fatal(err)
		}
	}

	log.Println("Migration complete")
}
