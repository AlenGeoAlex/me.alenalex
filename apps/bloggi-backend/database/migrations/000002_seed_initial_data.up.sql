INSERT INTO public.author (id, name, description, github, website, linkedin)
VALUES ('00000000-0000-0000-0000-000000000001', 'Alen Alex', 'A simple Human', 'https://github.com/AlenGeoAlex', 'https://alenalex.me', 'https://linkedin.com/in/alengeoalex')
ON CONFLICT DO NOTHING;