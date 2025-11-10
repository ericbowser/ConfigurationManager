# ConfigurationManager

A self-hosted configuration manager for environment variables and project configurations, built as an alternative after the popular `.env` service moved to a paid model. This WPF application stores configurations as JSONB in PostgreSQL and provides seamless export to `.env` file format.

## Problem Statement

When the popular `.env` management service transitioned to a paid subscription model, many developers needed a free, self-hosted alternative for managing environment variables and project configurations. ConfigurationManager solves this by:

- **Self-hosted**: Full control over your configuration data with no external dependencies
- **PostgreSQL-backed**: Uses JSONB for flexible, efficient storage
- **Export to .env**: Convert stored configurations back to standard `.env` format for easy integration with existing workflows
- **Simple GUI**: WPF interface for easy management without command-line complexity

## Features

- ✅ **Store configurations as JSONB** in PostgreSQL for flexible, structured storage
- ✅ **Export to .env format** - Convert stored configurations back to standard `.env` files
- ✅ **Project organization** - Group configurations by project name
- ✅ **Optional URLs** - Associate project URLs with configurations
- ✅ **Visual management** - WPF GUI for creating, viewing, and managing configurations
- ✅ **Local-first** - Complete data ownership and privacy

## Why Export to .env?

While storing configurations as JSONB in PostgreSQL provides flexibility and querying capabilities, exporting back to `.env` format offers several benefits:

- **Standard format**: `.env` files are the de-facto standard for environment variable storage
- **Tool compatibility**: Works seamlessly with frameworks and tools that expect `.env` files
- **Version control**: Easy to track configuration changes in git
- **Deployment**: Simple file-based deployment workflows
- **Legacy support**: Integrate with existing systems that require `.env` files

## Requirements

- .NET 9.0 or later
- PostgreSQL database (any recent version)
- Windows OS (WPF application)

## Quick Start

### 1. Setup Database

Create the database schema and table:

```bash
psql -U postgres -d your_database -f database/setup.sql
```

### 2. Build and Run

```bash
dotnet restore
dotnet build
dotnet run
```

See [QUICKSTART.md](QUICKSTART.md) for detailed setup and usage instructions.

## Usage

### Connect to Database

Enter your PostgreSQL connection string:
```
Host=localhost;Database=your_database;Username=postgres;Password=your_password;Port=5432
```

### Manage Configurations

- **Add**: Create new configurations with project name, optional URL, and JSON config
- **View**: Browse all stored configurations
- **Update**: Modify existing configurations
- **Delete**: Remove configurations
- **Export**: Export configurations to `.env` file format

### Configuration Format

Configurations are stored as JSON objects where all values are strings:

```json
{
  "API_KEY": "abc123xyz",
  "DATABASE_HOST": "db.example.com",
  "CACHE_TTL": "3600",
  "DEBUG": "true"
}
```

## Architecture

- **Models**: `EnvConfiguration` - Data model for configurations
- **Services**: `DatabaseService` - Handles all database operations including export to `.env`
- **UI**: WPF interface for configuration management
- **Storage**: PostgreSQL with JSONB column for flexible configuration storage

## Future Improvements

- [ ] Import from existing `.env` files
- [ ] Bulk export operations
- [ ] Configuration versioning
- [ ] Search and filtering capabilities
- [ ] Environment-specific configurations (dev, staging, prod)
- [ ] Encryption for sensitive values
- [ ] Multi-user support with authentication

## License

See [LICENSE](LICENSE) file for details.
