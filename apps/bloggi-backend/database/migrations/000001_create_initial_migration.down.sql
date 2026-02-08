-- ===============================
-- BLOGGI INITIAL SCHEMA (DOWN)
-- ===============================

-- -------------------------------
-- Triggers
-- -------------------------------

DROP TRIGGER IF EXISTS trg_post_revision_public_id
ON public.post_revision;

-- -------------------------------
-- Functions
-- -------------------------------

DROP FUNCTION IF EXISTS public.post_revision_public_id_trigger();

-- -------------------------------
-- Indexes
-- -------------------------------

DROP INDEX IF EXISTS public.ix_author_name;
DROP INDEX IF EXISTS public.gin_idx_post;
DROP INDEX IF EXISTS public.gin_idx_post_block;

-- -------------------------------
-- Constraints
-- -------------------------------

ALTER TABLE IF EXISTS public.post_block
DROP CONSTRAINT IF EXISTS uq_post_block_post_id_revision_id_ordinal;

ALTER TABLE IF EXISTS public.post_revision
DROP CONSTRAINT IF EXISTS uq_post_revision_public_id_post_id;

ALTER TABLE IF EXISTS public.post
DROP CONSTRAINT IF EXISTS fk_post_author_id;

ALTER TABLE IF EXISTS public.post_block
DROP CONSTRAINT IF EXISTS fk_post_block_post_id;

ALTER TABLE IF EXISTS public.post_block
DROP CONSTRAINT IF EXISTS fk_post_block_revision_id;

-- -------------------------------
-- Tables
-- -------------------------------

DROP TABLE IF EXISTS public.post_block CASCADE;
DROP TABLE IF EXISTS public.post_revision CASCADE;
DROP TABLE IF EXISTS public.post CASCADE;
DROP TABLE IF EXISTS public.author CASCADE;

-- -------------------------------
-- Types
-- -------------------------------

DROP TYPE IF EXISTS public.post_block_type;

-- -------------------------------
-- Extensions
-- -------------------------------

DROP EXTENSION IF EXISTS btree_gin;
