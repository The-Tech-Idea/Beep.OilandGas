using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Parses TCM.sql file to extract table descriptions and create JSON file
    /// </summary>
    public class ParseTCMDescriptions
    {
        /// <summary>
        /// Parses TCM.sql and creates ppdm38tabledescr.json
        /// </summary>
        public static void ParseAndCreateJson(string tcmSqlPath, string outputJsonPath)
        {
            if (!File.Exists(tcmSqlPath))
            {
                throw new FileNotFoundException($"TCM.sql file not found: {tcmSqlPath}");
            }

            var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            // Pattern to match: execute sp_addextendedproperty 'Description','[Description]','USER','dbo','TABLE','[TABLE_NAME]';
            var pattern = @"execute\s+sp_addextendedproperty\s+'Description',\s*'([^']*(?:''[^']*)*)',\s*'USER',\s*'dbo',\s*'TABLE',\s*'([^']+)';";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var lines = File.ReadAllLines(tcmSqlPath);
            var content = string.Join("\n", lines);

            var matches = regex.Matches(content);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    var description = match.Groups[1].Value;
                    var tableName = match.Groups[2].Value;

                    // Replace SQL escaped single quotes ('' with ')
                    description = description.Replace("''", "'");

                    // Use table name as key (case-insensitive dictionary handles duplicates)
                    if (!descriptions.ContainsKey(tableName))
                    {
                        descriptions[tableName] = description;
                    }
                    else
                    {
                        // If duplicate, append (shouldn't happen but handle it)
                        Console.WriteLine($"Warning: Duplicate table name found: {tableName}");
                    }
                }
            }

            // Create JSON with indentation
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(descriptions, options);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputJsonPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllText(outputJsonPath, json, Encoding.UTF8);

            Console.WriteLine($"Successfully parsed {descriptions.Count} table descriptions");
            Console.WriteLine($"Output written to: {outputJsonPath}");
        }
    }
}

