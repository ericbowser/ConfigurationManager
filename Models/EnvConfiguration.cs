using System.Text.Json;

namespace ConfigurationManager.Models
{
    public class EnvConfiguration
    {
        public int Id { get; set; }
        public string Project { get; set; } = string.Empty;
        public string? Url { get; set; }
        public Dictionary<string, string>? Config { get; set; }

        public string ConfigDisplay => Config != null ? 
            JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true }) : 
            "{}";
    }
}
