-- Configuration Manager Database Setup Script
-- Run this script in your PostgreSQL database to create the necessary schema and table

-- Create the config schema
CREATE SCHEMA IF NOT EXISTS config;

-- Create the env table with IDENTITY column
CREATE TABLE IF NOT EXISTS config.env (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    project TEXT NOT NULL,
    url TEXT,
    config JSONB
);

-- Create an index on the project column for faster searches
CREATE INDEX IF NOT EXISTS idx_env_project ON config.env(project);

-- Add some sample data (optional - remove if you don't want test data)
INSERT INTO config.env (project, url, config) VALUES 
(
    'sample-project', 
    'https://example.com',
    '{
        "API_KEY": "sample-key-123",
        "DATABASE_URL": "postgres://localhost:5432/sample",
        "DEBUG": "true",
        "MAX_CONNECTIONS": "10"
    }'::jsonb
),
(
    'my-web-app',
    'https://mywebapp.com',
    '{
        "NODE_ENV": "production",
        "PORT": "3000",
        "SESSION_SECRET": "mysecret123"
    }'::jsonb
);

-- Verify the setup
SELECT * FROM config.env;
