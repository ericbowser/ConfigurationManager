using ConfigurationManager.Models;
using Npgsql;
using System.Text.Json;

namespace ConfigurationManager.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<EnvConfiguration>> GetAllConfigurationsAsync()
        {
            var configurations = new List<EnvConfiguration>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT id, project, url, config FROM config.env ORDER BY id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                try
                {
                    var config = new EnvConfiguration
                    {
                        Id = reader.GetInt32(0),
                        Project = reader.GetString(1),
                        Url = reader.IsDBNull(2) ? null : reader.GetString(2)
                    };

                    if (!reader.IsDBNull(3))
                    {
                        var jsonString = reader.GetString(3);
                        config.Config = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                    }

                    configurations.Add(config);
                }
                catch (Exception ex)
                {
                    // Log the error but continue processing other rows
                    System.Diagnostics.Debug.WriteLine($"Error reading row: {ex.Message}");
                }
            }

            return configurations;
        }

        public async Task<int> AddConfigurationAsync(string project, string? url, Dictionary<string, string>? config)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "INSERT INTO config.env (project, url, config) VALUES (@project, @url, @config::jsonb) RETURNING id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            
            cmd.Parameters.AddWithValue("project", project);
            cmd.Parameters.AddWithValue("url", (object?)url ?? DBNull.Value);
            
            var configJson = config != null ? JsonSerializer.Serialize(config) : "{}";
            cmd.Parameters.AddWithValue("config", configJson);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
