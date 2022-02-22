using Nickvision.Avalonia.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace NickvisionTagger.Models
{
    public class Configuration
    {
        private static readonly string ConfigDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}Nickvision{Path.DirectorySeparatorChar}NickvisionTagger";
        private static readonly string ConfigPath = $"{ConfigDir}{Path.DirectorySeparatorChar}config.json";

        public Theme Theme { get; set; }
        public AccentColor AccentColor { get; set; }

        public Configuration()
        {
            Theme = Theme.Dark;
            AccentColor = AccentColor.Blue;
        }

        public static Configuration Load()
        {
            if (!Directory.Exists(ConfigDir))
            {
                Directory.CreateDirectory(ConfigDir);
            }
            try
            {
                var json = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<Configuration>(json);
                return config ?? new Configuration();
            }
            catch
            {
                return new Configuration();
            }
        }

        public static async Task<Configuration> LoadAsync()
        {
            if (!Directory.Exists(ConfigDir))
            {
                Directory.CreateDirectory(ConfigDir);
            }
            try
            {
                var json = await File.ReadAllTextAsync(ConfigPath);
                var config = JsonSerializer.Deserialize<Configuration>(json);
                return config ?? new Configuration();
            }
            catch
            {
                return new Configuration();
            }
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(ConfigPath, json);
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(this);
            await File.WriteAllTextAsync(ConfigPath, json);
        }
    }
}