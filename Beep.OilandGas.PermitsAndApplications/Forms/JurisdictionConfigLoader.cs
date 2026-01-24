using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public static class JurisdictionConfigLoader
    {
        public static JurisdictionConfig LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<JurisdictionConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return config ?? new JurisdictionConfig();
        }

        public static IReadOnlyList<JurisdictionConfig> LoadFromDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Directory path is required.", nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                return Array.Empty<JurisdictionConfig>();

            var configs = new List<JurisdictionConfig>();
            foreach (var filePath in Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly))
            {
                configs.Add(LoadFromFile(filePath));
            }

            return configs;
        }
    }
}
