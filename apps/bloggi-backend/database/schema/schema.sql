--
-- PostgreSQL database dump
--


-- Dumped from database version 17.7 (Debian 17.7-3.pgdg12+1)
-- Dumped by pg_dump version 17.7 (Debian 17.7-3.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: btree_gin; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS btree_gin WITH SCHEMA public;


--
-- Name: post_block_type; Type: TYPE; Schema: public; Owner: -
--

CREATE TYPE public.post_block_type AS ENUM (
    'text',
    'markdown',
    'media',
    'code',
    'html',
    'card_content',
    'embed'
);


--
-- Name: post_revision_public_id_trigger(); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.post_revision_public_id_trigger() RETURNS trigger
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


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: author; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.author (
    id character varying(36) NOT NULL,
    name character varying(50) NOT NULL,
    description character varying(100),
    github character varying(50),
    website character varying(100),
    linkedin character varying(50)
);


--
-- Name: post; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.post (
    id character varying(36) NOT NULL,
    title character varying(200) NOT NULL,
    authored_date timestamp with time zone DEFAULT (now() AT TIME ZONE 'UTC'::text) NOT NULL,
    author_id character varying(36) NOT NULL,
    published boolean DEFAULT false NOT NULL,
    vector_text tsvector GENERATED ALWAYS AS (to_tsvector('english'::regconfig, (COALESCE(title, ''::character varying))::text)) STORED
);


--
-- Name: post_block; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.post_block (
    id character varying(26) NOT NULL,
    block_ordinal integer NOT NULL,
    block_type public.post_block_type NOT NULL,
    post_id character varying(36) NOT NULL,
    content_data jsonb DEFAULT '{}'::jsonb NOT NULL,
    revision_id character varying(26) NOT NULL,
    vector_text tsvector
);


--
-- Name: post_revision; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.post_revision (
    id character varying(26) NOT NULL,
    post_id character varying(36) NOT NULL,
    published boolean DEFAULT false NOT NULL,
    change_log text,
    revision_date timestamp with time zone DEFAULT (now() AT TIME ZONE 'UTC'::text) NOT NULL,
    public_id integer
);


--
-- Name: schema_migrations; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.schema_migrations (
    version bigint NOT NULL,
    dirty boolean NOT NULL
);


--
-- Name: author author_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.author
    ADD CONSTRAINT author_pkey PRIMARY KEY (id);


--
-- Name: post_block post_block_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.post_block
    ADD CONSTRAINT post_block_pkey PRIMARY KEY (id);


--
-- Name: post post_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.post
    ADD CONSTRAINT post_pkey PRIMARY KEY (id);


--
-- Name: post_revision post_revision_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.post_revision
    ADD CONSTRAINT post_revision_pkey PRIMARY KEY (id);


--
-- Name: schema_migrations schema_migrations_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.schema_migrations
    ADD CONSTRAINT schema_migrations_pkey PRIMARY KEY (version);


--
-- Name: post_block uq_post_block_post_id_revision_id_ordinal; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.post_block
    ADD CONSTRAINT uq_post_block_post_id_revision_id_ordinal UNIQUE (post_id, revision_id, block_ordinal);


--
-- Name: post_revision uq_post_revision_public_id_post_id; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.post_revision
    ADD CONSTRAINT uq_post_revision_public_id_post_id UNIQUE (post_id, public_id);


--
-- Name: gin_idx_post; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX gin_idx_post ON public.post USING gin (vector_text);


--
-- Name: gin_idx_post_block; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX gin_idx_post_block ON public.post_block USING gin (vector_text);


--
-- Name: ix_author_name; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_author_name ON public.author USING btree (name);


--
-- Name: post_revision trg_post_revision_public_id; Type: TRIGGER; Schema: public; Owner: -
--

CREATE TRIGGER trg_post_revision_public_id BEFORE INSERT ON public.post_revision FOR EACH ROW EXECUTE FUNCTION public.post_revision_public_id_trigger();


--
-- Name: post fk_post_author_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.post
    ADD CONSTRAINT fk_post_author_id FOREIGN KEY (author_id) REFERENCES public.author(id);


--
-- Name: post_block fk_post_block_post_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.post_block
    ADD CONSTRAINT fk_post_block_post_id FOREIGN KEY (post_id) REFERENCES public.post(id);


--
-- Name: post_block fk_post_block_revision_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.post_block
    ADD CONSTRAINT fk_post_block_revision_id FOREIGN KEY (revision_id) REFERENCES public.post_revision(id);


--
-- PostgreSQL database dump complete
--


