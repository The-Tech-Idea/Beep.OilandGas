using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.SeedData;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Generic CSV seeder for PPDM39 tables
    /// Uses metadata to map CSV files to tables and entity types
    /// Uses PPDMGenericRepository to insert data
    /// </summary>
    public class PPDMCSVSeeder : IPPDMSeeder
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;

        public PPDMCSVSeeder(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
        }

        /// <summary>
        /// Seeds data from CSV file
        /// </summary>
        public async Task<int> SeedAsync(string csvFilePath, string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(csvFilePath))
                throw new ArgumentException("CSV file path cannot be null or empty", nameof(csvFilePath));

            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException($"CSV file not found: {csvFilePath}");

            var fileName = Path.GetFileNameWithoutExtension(csvFilePath);
            
            // Determine table name and entity type from CSV filename using metadata
            var (tableName, entityType) = await GetTableAndEntityTypeFromFileNameAsync(fileName);
            
            if (entityType == null)
            {
                throw new InvalidOperationException($"Could not determine entity type for CSV file: {fileName}. Table name: {tableName}");
            }

            // Read CSV file (already cleaned - no logo, disclaimer, copyright rows)
            // Row 1 (index 0) is the header row
            // Row 2+ (index 1+) are data rows
            var csvLines = File.ReadAllLines(csvFilePath);
            if (csvLines.Length < 2)
            {
                throw new InvalidOperationException($"CSV file has less than 2 rows: {csvFilePath}");
            }

            // Get header row (row 1, index 0)
            var headerRow = ParseCSVLine(csvLines[0]);
            
            // Find column indices
            var columnIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headerRow.Count; i++)
            {
                var colName = headerRow[i].Trim('"').Trim();
                if (!string.IsNullOrWhiteSpace(colName))
                {
                    columnIndices[colName] = i;
                }
            }

            // Get table metadata to understand column mappings
            var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
            if (tableMetadata == null)
            {
                throw new InvalidOperationException($"Table metadata not found for: {tableName}");
            }

            // Create repository for this entity type
            var repository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, tableName);

            // Process data rows (starting from row 2, index 1)
            int seededCount = 0;
            for (int rowIndex = 1; rowIndex < csvLines.Length; rowIndex++)
            {
                var row = csvLines[rowIndex];
                
                // Skip empty rows (rows with only commas, whitespace, or no data)
                if (IsEmptyRow(row))
                    continue;

                try
                {
                    var values = ParseCSVLine(row);
                    
                    // Create entity instance
                    var entity = Activator.CreateInstance(entityType);
                    
                    // Map CSV columns to entity properties
                    MapCSVToEntity(entity, values, columnIndices, tableMetadata);
                    
                    // Set common columns (PrepareForInsert will be called by repository.InsertAsync)
                    
                    // Insert using repository
                    var result = await repository.InsertAsync(entity, userId);
                    
                    if (result != null)
                    {
                        seededCount++;
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with next row
                    Console.WriteLine($"Error seeding row {rowIndex + 1} in {fileName}: {ex.Message}");
                }
            }

            return seededCount;
        }

        /// <summary>
        /// Determines table name and entity type from CSV filename using metadata
        /// CSV filename pattern: tablename_PPDM_2023number.csv
        /// </summary>
        private async Task<(string tableName, Type entityType)> GetTableAndEntityTypeFromFileNameAsync(string fileName)
        {
            // Extract table name from CSV filename pattern: tablename_PPDM_2023number.csv
            // Example: "Life_Cycle_PPDM_20230203.csv" -> "Life_Cycle"
            var tableNameMatch = System.Text.RegularExpressions.Regex.Match(fileName, @"^(.+?)_PPDM_\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (!tableNameMatch.Success)
            {
                throw new InvalidOperationException($"CSV filename does not match expected pattern 'tablename_PPDM_YYYYMMDD.csv': {fileName}");
            }
            
            var extractedTableName = tableNameMatch.Groups[1].Value;
            
            // Strategy 1: For well status facets, map to R_WELL_STATUS
            var wellStatusFacets = new[] { "Life_Cycle", "Role", "Business_Interest", "Business_Intention", 
                "Outcome", "Lahee_Class", "Play_Type", "Well_Structure", "Trajectory_Type", 
                "Fluid_Direction", "Well_Reporting_Class", "Fluid_Type", "Wellbore_Status", 
                "Well_Status", "Operatorship" };
            
            if (wellStatusFacets.Any(f => extractedTableName.Equals(f, StringComparison.OrdinalIgnoreCase)))
            {
                return await GetEntityTypeForTableAsync("R_WELL_STATUS");
            }
            
            // Strategy 2: Try direct table name match (e.g., "Stratigraphic_Unit_Type" -> "R_STRAT_UNIT_TYPE")
            var directTableName = extractedTableName.ToUpper();
            var metadata = await _metadata.GetTableMetadataAsync(directTableName);
            if (metadata != null)
            {
                return await GetEntityTypeForTableAsync(directTableName);
            }
            
            // Strategy 3: Try with R_ prefix (reference tables)
            var refTableName = "R_" + directTableName;
            metadata = await _metadata.GetTableMetadataAsync(refTableName);
            if (metadata != null)
            {
                return await GetEntityTypeForTableAsync(refTableName);
            }
            
            // Strategy 4: Try to find by entity type name (remove underscores, convert to uppercase)
            var entityTypeName = extractedTableName.Replace("_", "").ToUpper();
            var entityType = GetEntityTypeByName(entityTypeName);
            if (entityType != null)
            {
                // Try to find table name from metadata by entity type name
                // We need to search through metadata - let's try common patterns
                var possibleTableNames = new[]
                {
                    directTableName,
                    refTableName,
                    "R_" + extractedTableName.Replace("_", "_").ToUpper(),
                    extractedTableName.Replace("_", "").ToUpper()
                };
                
                foreach (var possibleTableName in possibleTableNames)
                {
                    var tableMeta = await _metadata.GetTableMetadataAsync(possibleTableName);
                    if (tableMeta != null && tableMeta.EntityTypeName.Equals(entityTypeName, StringComparison.OrdinalIgnoreCase))
                    {
                        return (possibleTableName, entityType);
                    }
                }
            }
            
            throw new InvalidOperationException($"Could not determine table for CSV file: {fileName}. Extracted table name: {extractedTableName}");
        }

        /// <summary>
        /// Gets entity type for a table name
        /// </summary>
        private async Task<(string tableName, Type entityType)> GetEntityTypeForTableAsync(string tableName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
            {
                throw new InvalidOperationException($"Table metadata not found: {tableName}");
            }
            
            var entityType = GetEntityTypeByName(metadata.EntityTypeName);
            if (entityType == null)
            {
                throw new InvalidOperationException($"Entity type not found: {metadata.EntityTypeName}");
            }
            
            return (tableName, entityType);
        }

        /// <summary>
        /// Gets entity type by name from PPDM39.Models namespace
        /// </summary>
        private Type GetEntityTypeByName(string entityTypeName)
        {
            // Search in Beep.OilandGas.PPDM39.Models namespace
            var assembly = typeof(Beep.OilandGas.PPDM39.Models.WELL).Assembly;
            var entityType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name.Equals(entityTypeName, StringComparison.OrdinalIgnoreCase) &&
                                     t.Namespace == "Beep.OilandGas.PPDM39.Models");
            
            return entityType;
        }

        /// <summary>
        /// Parses a CSV line handling quoted values
        /// </summary>
        private List<string> ParseCSVLine(string line)
        {
            var values = new List<string>();
            var currentValue = "";
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
                        currentValue += '"';
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (currentChar == ',' && !inQuotes)
                {
                    values.Add(currentValue.Trim('"'));
                    currentValue = "";
                }
                else
                {
                    currentValue += currentChar;
                }
            }
            values.Add(currentValue.Trim('"')); // Add last value
            
            return values;
        }

        /// <summary>
        /// Checks if a CSV row is empty (only commas, whitespace, or no data)
        /// </summary>
        private bool IsEmptyRow(string row)
        {
            if (string.IsNullOrWhiteSpace(row))
                return true;
            
            // Remove all commas and whitespace, check if anything remains
            var cleaned = row.Replace(",", "").Trim();
            return string.IsNullOrWhiteSpace(cleaned);
        }

        /// <summary>
        /// Maps CSV values to entity properties
        /// </summary>
        private void MapCSVToEntity(object entity, List<string> csvValues, Dictionary<string, int> columnIndices, PPDMTableMetadata tableMetadata)
        {
            var entityType = entity.GetType();
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // Map CSV columns to entity properties
            foreach (var prop in properties)
            {
                // Try to find matching CSV column
                var csvColumnName = FindMatchingCSVColumn(prop.Name, columnIndices.Keys);
                
                if (csvColumnName != null && columnIndices.ContainsKey(csvColumnName))
                {
                    var columnIndex = columnIndices[csvColumnName];
                    if (columnIndex < csvValues.Count)
                    {
                        var csvValue = csvValues[columnIndex].Trim();
                        
                        if (!string.IsNullOrWhiteSpace(csvValue))
                        {
                            SetPropertyValue(entity, prop, csvValue);
                        }
                    }
                }
            }
            
            // Note: R_WELL_STATUS uses composite key (STATUS_TYPE, STATUS), not STATUS_ID
            // STATUS_ID is only used in WELL_STATUS table, not R_WELL_STATUS reference table
        }

        /// <summary>
        /// Finds matching CSV column name for entity property
        /// </summary>
        private string FindMatchingCSVColumn(string propertyName, IEnumerable<string> csvColumns)
        {
            // Direct match
            var directMatch = csvColumns.FirstOrDefault(c => 
                c.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (directMatch != null)
                return directMatch;
            
            // Common mappings
            var mappings = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "NAME1", new[] { "STATUS", "LONG_NAME", "NAME" } },
                { "DEFINITION", new[] { "DESCRIPTION", "REMARK" } },
                { "VALUE_STATUS", new[] { "STATUS_GROUP" } },
                { "SOURCE", new[] { "SOURCE" } },
                { "RESOURCE", new[] { "RESOURCE", "URL" } }
            };
            
            foreach (var mapping in mappings)
            {
                if (mapping.Value.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
                {
                    var match = csvColumns.FirstOrDefault(c => 
                        c.Equals(mapping.Key, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                        return match;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Sets property value on entity, handling type conversions
        /// </summary>
        private void SetPropertyValue(object entity, PropertyInfo property, string csvValue)
        {
            try
            {
                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(entity, csvValue);
                }
                else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                {
                    if (int.TryParse(csvValue, out int intValue))
                    {
                        property.SetValue(entity, intValue);
                    }
                }
                else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                {
                    if (decimal.TryParse(csvValue, out decimal decValue))
                    {
                        property.SetValue(entity, decValue);
                    }
                }
                else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                {
                    if (DateTime.TryParse(csvValue, out DateTime dateValue))
                    {
                        property.SetValue(entity, dateValue);
                    }
                    else
                    {
                        // Try Excel date serial number
                        if (double.TryParse(csvValue, out double excelDate))
                        {
                            var date = DateTime.FromOADate(excelDate);
                            property.SetValue(entity, date);
                        }
                    }
                }
                else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                {
                    var boolValue = csvValue.Equals("Y", StringComparison.OrdinalIgnoreCase) ||
                                   csvValue.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                                   csvValue.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                   csvValue.Equals("True", StringComparison.OrdinalIgnoreCase);
                    property.SetValue(entity, boolValue);
                }
            }
            catch
            {
                // Ignore conversion errors
            }
        }
    }
}

