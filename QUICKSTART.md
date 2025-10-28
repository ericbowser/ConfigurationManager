# Quick Start Guide

## Setup Database

1. Create the database schema and table:
```bash
psql -U postgres -d your_database -f database/setup.sql
```

## Build and Run

```bash
dotnet restore
dotnet build
dotnet run
```

## Usage

### Connect to Database
1. Enter connection string:
   ```
   Host=localhost;Database=your_database;Username=postgres;Password=your_password;Port=5432
   ```
2. Click "Connect"

### View Configurations
- Configurations load automatically after connecting
- Click "🔄 Refresh" to reload

### Add Configuration
1. Expand "➕ Add New Configuration"
2. Fill in:
   - **Project**: (required) e.g., `my-api-project`
   - **URL**: (optional) e.g., `https://api.myproject.com`
   - **Config JSON**: (optional) e.g.,
     ```json
     {
       "API_KEY": "abc123xyz",
       "DATABASE_HOST": "db.example.com",
       "CACHE_TTL": "3600"
     }
     ```
3. Click "Add Configuration"

## Troubleshooting

**Connection Failed**
- Verify PostgreSQL is running: `pg_isready`
- Check connection string format
- Verify database exists
- Test with: `psql -U postgres -d your_database`

**JSON Validation Error**
- Ensure valid JSON syntax
- Use double quotes for strings
- All values must be strings: `"true"`, `"123"`
