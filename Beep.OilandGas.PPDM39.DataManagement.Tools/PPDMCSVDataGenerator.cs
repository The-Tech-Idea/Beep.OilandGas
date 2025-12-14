using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Generates JSON file from CSV files for embedding in library
    /// Similar to PPDMMetadataGenerator but for CSV seed data
    /// </summary>
    public class PPDMCSVDataGenerator
    {
        /// <summary>
        /// Generates JSON file from all CSV files in a folder
        /// </summary>
        public static void GenerateJsonFromCsvFiles(string csvFolder, string outputJsonPath)
        {
            if (string.IsNullOrWhiteSpace(csvFolder))
                throw new ArgumentException("CSV folder cannot be null or empty", nameof(csvFolder));

            if (string.IsNullOrWhiteSpace(outputJsonPath))
                throw new ArgumentException("Output JSON path cannot be null or empty", nameof(outputJsonPath));

            if (!Directory.Exists(csvFolder))
                throw new DirectoryNotFoundException($"CSV folder not found: {csvFolder}");

            // Load metadata to get actual table names
            var metadata = PPDM39Metadata.GetMetadata();
            var metadataRepo = new PPDMMetadataRepository(metadata);

            var csvFiles = Directory.GetFiles(csvFolder, "*.csv", SearchOption.TopDirectoryOnly);
            
            Console.WriteLine($"Found {csvFiles.Length} CSV files to process");
            Console.WriteLine("");

            var allCsvData = new Dictionary<string, PPDMCSVData>(StringComparer.OrdinalIgnoreCase);

            foreach (var csvFile in csvFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(csvFile);
                Console.WriteLine($"Processing: {fileName}");

                try
                {
                    var csvData = ParseCSVFile(csvFile, metadataRepo);
                    allCsvData[fileName] = csvData;
                    Console.WriteLine($"  ✓ Processed {csvData.Rows.Count} rows (Table: {csvData.TableName})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Error: {ex.Message}");
                }
            }

            // Serialize to JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(allCsvData, options);
            File.WriteAllText(outputJsonPath, json, Encoding.UTF8);

            Console.WriteLine("");
            Console.WriteLine($"========================================");
            Console.WriteLine($"Generation Complete!");
            Console.WriteLine($"========================================");
            Console.WriteLine($"Total files processed: {allCsvData.Count}");
            Console.WriteLine($"Output file: {outputJsonPath}");
            Console.WriteLine("");
        }

        /// <summary>
        /// Parses a CSV file and returns structured data
        /// CSV files are already cleaned (no logo, disclaimer, copyright rows)
        /// Row 1 (index 0) is the header row
        /// Row 2+ (index 1+) are data rows
        /// </summary>
        private static PPDMCSVData ParseCSVFile(string csvFilePath, PPDMMetadataRepository metadataRepo)
        {
            var csvLines = File.ReadAllLines(csvFilePath, Encoding.UTF8);
            
            if (csvLines.Length < 2)
            {
                throw new InvalidOperationException($"CSV file has less than 2 rows: {csvFilePath}");
            }

            // Get header row (row 1, index 0)
            var headerRow = ParseCSVLine(csvLines[0]);
            
            // Extract table name from filename: tablename_PPDM_2023number.csv
            var fileName = Path.GetFileNameWithoutExtension(csvFilePath);
            var tableNameMatch = Regex.Match(fileName, @"^(.+?)_PPDM_\d+$", RegexOptions.IgnoreCase);
            var extractedTableName = tableNameMatch.Success ? tableNameMatch.Groups[1].Value : fileName;

            // Get actual table name from metadata
            var actualTableName = GetActualTableNameFromMetadata(extractedTableName, metadataRepo);

            var csvData = new PPDMCSVData
            {
                FileName = fileName,
                TableName = actualTableName ?? extractedTableName, // Fallback to extracted name if not found in metadata
                Headers = headerRow,
                Rows = new List<List<string>>()
            };

            // Process data rows (starting from row 2, index 1)
            for (int rowIndex = 1; rowIndex < csvLines.Length; rowIndex++)
            {
                var row = csvLines[rowIndex];
                
                // Skip empty rows (rows with only commas, whitespace, or no data)
                if (IsEmptyRow(row))
                    continue;

                var values = ParseCSVLine(row);
                
                // Also check if parsed values are all empty
                if (values.All(v => string.IsNullOrWhiteSpace(v)))
                    continue;
                
                csvData.Rows.Add(values);
            }

            return csvData;
        }

        /// <summary>
        /// Parses a CSV line handling quoted values
        /// </summary>
        private static List<string> ParseCSVLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            var inQuotes = false;
            
            var chars = line.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                var currentChar = chars[i];
                
                if (currentChar == '"')
                {
                    // Check if it's an escaped quote (two quotes in a row)
                    if (i + 1 < chars.Length && chars[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (currentChar == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString().Trim('"'));
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(currentChar);
                }
            }
            values.Add(currentValue.ToString().Trim('"')); // Add last value
            
            return values;
        }

        /// <summary>
        /// Checks if a CSV row is empty (only commas, whitespace, or no data)
        /// </summary>
        private static bool IsEmptyRow(string row)
        {
            if (string.IsNullOrWhiteSpace(row))
                return true;
            
            // Remove all commas and whitespace, check if anything remains
            var cleaned = row.Replace(",", "").Trim();
            return string.IsNullOrWhiteSpace(cleaned);
        }

        /// <summary>
        /// Gets actual table name from metadata based on extracted table name
        /// </summary>
        private static string GetActualTableNameFromMetadata(string extractedTableName, PPDMMetadataRepository metadataRepo)
        {
            // Strategy 1: For well status facets, map to R_WELL_STATUS
            var wellStatusFacets = new[] { "Life_Cycle", "Role", "Business_Interest", "Business_Intention", 
                "Outcome", "Lahee_Class", "Play_Type", "Well_Structure", "Trajectory_Type", 
                "Fluid_Direction", "Well_Reporting_Class", "Fluid_Type", "Wellbore_Status", 
                "Well_Status", "Operatorship" };
            
            if (wellStatusFacets.Any(f => extractedTableName.Equals(f, StringComparison.OrdinalIgnoreCase)))
            {
                return "R_WELL_STATUS";
            }
            
            // Strategy 2: Try direct table name match (e.g., "Stratigraphic_Unit_Type" -> "R_STRAT_UNIT_TYPE")
            var directTableName = extractedTableName.ToUpper();
            var metadata = metadataRepo.GetTableMetadataAsync(directTableName).Result;
            if (metadata != null)
            {
                return directTableName;
            }
            
            // Strategy 3: Try with R_ prefix (reference tables)
            var refTableName = "R_" + directTableName;
            metadata = metadataRepo.GetTableMetadataAsync(refTableName).Result;
            if (metadata != null)
            {
                return refTableName;
            }
            
            // Strategy 4: Try to find by removing underscores and matching
            var searchPatterns = new[]
            {
                directTableName.Replace("_", ""),
                "R_" + directTableName.Replace("_", "_"),
                extractedTableName.Replace("_", "").ToUpper()
            };
            
            foreach (var pattern in searchPatterns)
            {
                metadata = metadataRepo.GetTableMetadataAsync(pattern).Result;
                if (metadata != null)
                {
                    return pattern;
                }
            }
            
            // Not found in metadata, return null to use extracted name as fallback
            return null;
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

