using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Enhances PPDM39Metadata.json with table descriptions from ppdm38tabledescr.json
    /// </summary>
    public class EnhanceMetadataWithDescriptions
    {
        /// <summary>
        /// Enhances metadata JSON with descriptions
        /// </summary>
        public static void EnhanceMetadata(string metadataPath, string descriptionsPath, string outputPath = null)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = metadataPath;
            }

            Console.WriteLine("Loading metadata...");
            var metadataJson = File.ReadAllText(metadataPath, Encoding.UTF8);
            var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(metadataJson);

            Console.WriteLine("Loading table descriptions...");
            var descriptionsJson = File.ReadAllText(descriptionsPath, Encoding.UTF8);
            var descriptions = JsonSerializer.Deserialize<Dictionary<string, string>>(descriptionsJson, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Console.WriteLine($"Found {descriptions.Count} table descriptions");
            Console.WriteLine($"Processing {metadata.Count} metadata entries...");

            int updatedCount = 0;
            int notFoundCount = 0;

            // Create a new dictionary to hold the enhanced metadata
            var enhancedMetadata = new Dictionary<string, object>();

            foreach (var kvp in metadata)
            {
                var tableName = kvp.Key;
                var tableElement = kvp.Value;

                // Convert JsonElement to dictionary for modification
                var tableDict = JsonSerializer.Deserialize<Dictionary<string, object>>(tableElement.GetRawText());

                // Add description if available
                if (descriptions.TryGetValue(tableName, out var description))
                {
                    tableDict["Description"] = description;
                    updatedCount++;
                }
                else
                {
                    notFoundCount++;
                }

                enhancedMetadata[tableName] = tableDict;
            }

            // Write enhanced metadata
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var outputJson = JsonSerializer.Serialize(enhancedMetadata, options);
            File.WriteAllText(outputPath, outputJson, Encoding.UTF8);

            Console.WriteLine($"Enhanced metadata written to: {outputPath}");
            Console.WriteLine($"Updated: {updatedCount} tables with descriptions");
            Console.WriteLine($"Not found: {notFoundCount} tables without descriptions");
        }
    }
}

