DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'exporter') THEN
    CREATE ROLE exporter LOGIN PASSWORD :'db_exporter_P@ssw0rd';
    END IF;
END
$$;

GRANT pg_monitor TO exporter;