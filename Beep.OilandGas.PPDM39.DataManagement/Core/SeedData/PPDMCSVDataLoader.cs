using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.SeedData
{
    /// <summary>
    /// Loads CSV seed data from embedded JSON resource
    /// Similar to PPDMMetadataLoader but for CSV seed data
    /// </summary>
    public class PPDMCSVDataLoader
    {
        private const string EmbeddedResourceName = "Beep.OilandGas.PPDM39.DataManagement.Core.SeedData.PPDMCSVData.json";

        /// <summary>
        /// Loads CSV data from embedded JSON resource
        /// </summary>
        public static Dictionary<string, PPDMCSVData> LoadFromEmbeddedResource()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = EmbeddedResourceName;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException($"Embedded resource not found: {resourceName}. Make sure the JSON file is set as 'Embedded Resource' in project properties.");
                }

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var json = reader.ReadToEnd();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    return JsonSerializer.Deserialize<Dictionary<string, PPDMCSVData>>(json, options);
                }
            }
        }

        /// <summary>
        /// Loads CSV data from JSON file
        /// </summary>
        public static Dictionary<string, PPDMCSVData> LoadFromJsonFile(string jsonPath)
        {
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException($"JSON file not found: {jsonPath}");

            var json = File.ReadAllText(jsonPath, Encoding.UTF8);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<Dictionary<string, PPDMCSVData>>(json, options);
        }
    }

    /// <summary>
    /// Represents CSV data structure
    /// </summary>
    public class PPDMCSVData
    {
        public string FileName { get; set; }
        public string TableName { get; set; }
        public List<string> Headers { get; set; } = new List<string>();
        public List<List<string>> Rows { get; set; } = new List<List<string>>();
    }
}

