-- ===============================
-- BLOGGI INITIAL SCHEMA (UP)
-- ===============================

-- -------------------------------
-- Extensions
-- -------------------------------

CREATE EXTENSION IF NOT EXISTS btree_gin WITH SCHEMA public;

-- -------------------------------
-- Enum Types
-- -------------------------------

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_type
        WHERE typname = 'post_block_type'
    ) THEN
CREATE TYPE public.post_block_type AS ENUM (
            'text',
            'markdown',
            'media',
            'code',
            'html',
            'card_content',
            'embed'
        );
END IF;
END$$;

-- -------------------------------
-- Tables
-- -------------------------------

CREATE TABLE IF NOT EXISTS public.author (
                                             id          VARCHAR(36) PRIMARY KEY,
    name        VARCHAR(50)  NOT NULL,
    description VARCHAR(100),
    github      VARCHAR(50),
    website     VARCHAR(100),
    linkedin    VARCHAR(50)
    );

CREATE TABLE IF NOT EXISTS public.post (
                                           id            VARCHAR(36) PRIMARY KEY,
    title         VARCHAR(200) NOT NULL,
    authored_date TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    author_id     VARCHAR(36) NOT NULL,
    published     BOOLEAN NOT NULL DEFAULT FALSE,

    vector_text tsvector
    GENERATED ALWAYS AS (
                            to_tsvector(
                            'english',
                            COALESCE(title, '')::text
    )
    ) STORED
    );

CREATE TABLE IF NOT EXISTS public.post_revision (
                                                    id            VARCHAR(26) PRIMARY KEY,
    post_id       VARCHAR(36) NOT NULL,
    published     BOOLEAN NOT NULL DEFAULT FALSE,
    change_log    TEXT,
    revision_date TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    public_id     INTEGER
    );

CREATE TABLE IF NOT EXISTS public.post_block (
                                                 id            VARCHAR(26) PRIMARY KEY,
    block_ordinal INTEGER NOT NULL,
    block_type    public.post_block_type NOT NULL,
    post_id       VARCHAR(36) NOT NULL,
    content_data  JSONB NOT NULL DEFAULT '{}',
    revision_id   VARCHAR(26) NOT NULL,
    vector_text   tsvector
    );

-- -------------------------------
-- Unique Constraints
-- -------------------------------

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'uq_post_block_post_id_revision_id_ordinal'
    ) THEN
ALTER TABLE public.post_block
    ADD CONSTRAINT uq_post_block_post_id_revision_id_ordinal
        UNIQUE (post_id, revision_id, block_ordinal);
END IF;
END$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'uq_post_revision_public_id_post_id'
    ) THEN
ALTER TABLE public.post_revision
    ADD CONSTRAINT uq_post_revision_public_id_post_id
        UNIQUE (post_id, public_id);
END IF;
END$$;

-- -------------------------------
-- Foreign Keys
-- -------------------------------

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_post_author_id'
    ) THEN
ALTER TABLE public.post
    ADD CONSTRAINT fk_post_author_id
        FOREIGN KEY (author_id)
            REFERENCES public.author(id);
END IF;
END$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_post_block_post_id'
    ) THEN
ALTER TABLE public.post_block
    ADD CONSTRAINT fk_post_block_post_id
        FOREIGN KEY (post_id)
            REFERENCES public.post(id);
END IF;
END$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_post_block_revision_id'
    ) THEN
ALTER TABLE public.post_block
    ADD CONSTRAINT fk_post_block_revision_id
        FOREIGN KEY (revision_id)
            REFERENCES public.post_revision(id);
END IF;
END$$;

-- -------------------------------
-- Indexes
-- -------------------------------

CREATE UNIQUE INDEX IF NOT EXISTS ix_author_name
    ON public.author (name);

CREATE INDEX IF NOT EXISTS gin_idx_post
    ON public.post
    USING GIN (vector_text);

CREATE INDEX IF NOT EXISTS gin_idx_post_block
    ON public.post_block
    USING GIN (vector_text);

-- -------------------------------
-- Functions
-- -------------------------------

CREATE OR REPLACE FUNCTION public.post_revision_public_id_trigger()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN

    IF NEW.public_id IS NULL THEN

SELECT COALESCE(MAX(public_id), 0) + 1
INTO NEW.public_id
FROM public.post_revision
WHERE post_id = NEW.post_id;

END IF;

RETURN NEW;

END;
$$;

-- -------------------------------
-- Triggers
-- -------------------------------

DROP TRIGGER IF EXISTS trg_post_revision_public_id
ON public.post_revision;

CREATE TRIGGER trg_post_revision_public_id
    BEFORE INSERT
    ON public.post_revision
    FOR EACH ROW
    EXECUTE FUNCTION public.post_revision_public_id_trigger();
