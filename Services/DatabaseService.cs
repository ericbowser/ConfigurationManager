using ConfigurationManager.Models;
using Npgsql;
using NpgsqlTypes;
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
            
            if (config != null)
            {
                var configJson = JsonSerializer.Serialize(config);
                cmd.Parameters.AddWithValue("config", NpgsqlDbType.Jsonb, configJson);
            }
            else
            {
                cmd.Parameters.AddWithValue("config", NpgsqlDbType.Jsonb, "{}");
            }

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task UpdateConfigurationAsync(int id, string project, string? url, Dictionary<string, string>? config)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "UPDATE config.env SET project = @project, url = @url, config = @config::jsonb WHERE id = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("project", project);
            cmd.Parameters.AddWithValue("url", (object?)url ?? DBNull.Value);
            
            if (config != null)
            {
                var configJson = JsonSerializer.Serialize(config);
                cmd.Parameters.AddWithValue("config", NpgsqlDbType.Jsonb, configJson);
            }
            else
            {
                cmd.Parameters.AddWithValue("config", NpgsqlDbType.Jsonb, "{}");
            }

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteConfigurationAsync(int id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM config.env WHERE id = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            await cmd.ExecuteNonQueryAsync();
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

        /// <summary>
        /// Exports a configuration to .env file format
        /// </summary>
        public async Task<string> ExportToEnvFormatAsync(int configId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "SELECT project, config FROM config.env WHERE id = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", configId);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var project = reader.GetString(0);
                var envContent = new System.Text.StringBuilder();
                
                envContent.AppendLine($"# Configuration for {project}");
                envContent.AppendLine($"# Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                envContent.AppendLine();

                if (!reader.IsDBNull(1))
                {
                    var jsonString = reader.GetString(1);
                    var config = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                    
                    if (config != null)
                    {
                        foreach (var kvp in config.OrderBy(x => x.Key))
                        {
                            // Escape values that contain special characters
                            var value = kvp.Value;
                            if (value.Contains(' ') || value.Contains('#') || value.Contains('='))
                            {
                                value = $"\"{value}\"";
                            }
                            envContent.AppendLine($"{kvp.Key}={value}");
                        }
                    }
                }

                return envContent.ToString();
            }

            throw new Exception($"Configuration with ID {configId} not found.");
        }
    }
}
