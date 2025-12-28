using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData.Services
{
    /// <summary>
    /// Generic CSV importer for LOV data
    /// Supports mapping to LIST_OF_VALUE or RA_* tables
    /// </summary>
    public class CSVLOVImporter
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;

        public CSVLOVImporter(
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
        /// Imports LOV data from CSV file to LIST_OF_VALUE table
        /// </summary>
        public async Task<ImportResult> ImportToListOfValueAsync(string csvFilePath, Dictionary<string, string>? columnMapping = null, bool skipExisting = true, string userId = "SYSTEM", string? connectionName = null)
        {
            var result = new ImportResult
            {
                Success = true,
                RecordsProcessed = 0,
                RecordsInserted = 0,
                RecordsSkipped = 0,
                Errors = new List<string>()
            };

            try
            {
                if (!File.Exists(csvFilePath))
                {
                    result.Success = false;
                    result.Errors.Add($"CSV file not found: {csvFilePath}");
                    return result;
                }

                var lines = await File.ReadAllLinesAsync(csvFilePath);
                if (lines.Length < 2)
                {
                    result.Success = false;
                    result.Errors.Add("CSV file must have at least a header row and one data row");
                    return result;
                }

                // Parse header
                var headers = ParseCSVLine(lines[0]);
                var mapping = columnMapping ?? CreateDefaultMapping(headers);

                // Validate required columns
                var requiredColumns = new[] { "VALUE_TYPE", "VALUE_CODE" };
                foreach (var required in requiredColumns)
                {
                    if (!mapping.ContainsKey(required) || !headers.Contains(mapping[required], StringComparer.OrdinalIgnoreCase))
                    {
                        result.Success = false;
                        result.Errors.Add($"Required column mapping not found: {required}");
                        return result;
                    }
                }

                var resolvedConnectionName = connectionName ?? _connectionName;
                var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(LIST_OF_VALUE), resolvedConnectionName, "LIST_OF_VALUE");

                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var values = ParseCSVLine(lines[i]);
                        if (values.Length != headers.Length)
                        {
                            result.Errors.Add($"Row {i + 1}: Column count mismatch");
                            continue;
                        }

                        var lov = new LIST_OF_VALUE();
                        var dataDict = new Dictionary<string, string>();
                        for (int j = 0; j < headers.Length; j++)
                        {
                            dataDict[headers[j]] = values[j];
                        }

                        // Map CSV columns to entity properties
                        foreach (var kvp in mapping)
                        {
                            var csvColumn = kvp.Value;
                            if (dataDict.ContainsKey(csvColumn))
                            {
                                var value = dataDict[csvColumn];
                                SetPropertyValue(lov, kvp.Key, value);
                            }
                        }

                        // Set defaults if not provided
                        if (string.IsNullOrEmpty(lov.LIST_OF_VALUE_ID))
                        {
                            lov.LIST_OF_VALUE_ID = Guid.NewGuid().ToString();
                        }
                        if (string.IsNullOrEmpty(lov.ACTIVE_IND))
                        {
                            lov.ACTIVE_IND = "Y";
                        }

                        // Check if exists
                        if (skipExisting)
                        {
                            var existingFilters = new List<TheTechIdea.Beep.Report.AppFilter>
                            {
                                new TheTechIdea.Beep.Report.AppFilter { FieldName = "VALUE_TYPE", Operator = "=", FilterValue = lov.VALUE_TYPE },
                                new TheTechIdea.Beep.Report.AppFilter { FieldName = "VALUE_CODE", Operator = "=", FilterValue = lov.VALUE_CODE }
                            };
                            var existing = await repository.GetAsync(existingFilters);
                            if (existing.Any())
                            {
                                result.RecordsSkipped++;
                                result.RecordsProcessed++;
                                continue;
                            }
                        }

                        // Insert
                        if (lov is IPPDMEntity lovEntity)
                            _commonColumnHandler.PrepareForInsert(lovEntity, userId);
                        var inserted = await repository.InsertAsync(lov, userId);
                        if (inserted != null)
                        {
                            result.RecordsInserted++;
                        }
                        result.RecordsProcessed++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {i + 1}: {ex.Message}");
                    }
                }

                if (result.Errors.Any())
                {
                    result.Success = false;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Import error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Imports LOV data from CSV to a specific RA_* table
        /// </summary>
        public async Task<ImportResult> ImportToRATableAsync(string csvFilePath, string tableName, Dictionary<string, string>? columnMapping = null, bool skipExisting = true, string userId = "SYSTEM", string? connectionName = null)
        {
            var result = new ImportResult
            {
                Success = true,
                RecordsProcessed = 0,
                RecordsInserted = 0,
                RecordsSkipped = 0,
                Errors = new List<string>()
            };

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync(tableName);
                if (metadata == null)
                {
                    result.Success = false;
                    result.Errors.Add($"Table metadata not found: {tableName}");
                    return result;
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ??
                                Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

                if (entityType == null)
                {
                    result.Success = false;
                    result.Errors.Add($"Entity type not found for table: {tableName}");
                    return result;
                }

                if (!File.Exists(csvFilePath))
                {
                    result.Success = false;
                    result.Errors.Add($"CSV file not found: {csvFilePath}");
                    return result;
                }

                var lines = await File.ReadAllLinesAsync(csvFilePath);
                if (lines.Length < 2)
                {
                    result.Success = false;
                    result.Errors.Add("CSV file must have at least a header row and one data row");
                    return result;
                }

                var resolvedConnectionName = connectionName ?? _connectionName;
                var headers = ParseCSVLine(lines[0]);
                var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, resolvedConnectionName, tableName);

                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var values = ParseCSVLine(lines[i]);
                        if (values.Length != headers.Length)
                        {
                            result.Errors.Add($"Row {i + 1}: Column count mismatch");
                            continue;
                        }

                        var entity = Activator.CreateInstance(entityType);
                        var dataDict = new Dictionary<string, string>();
                        for (int j = 0; j < headers.Length; j++)
                        {
                            dataDict[headers[j]] = values[j];
                        }

                        // Map CSV columns to entity properties
                        var mapping = columnMapping ?? CreateDefaultMappingForTable(headers, entityType);
                        foreach (var kvp in mapping)
                        {
                            var csvColumn = kvp.Value;
                            if (dataDict.ContainsKey(csvColumn))
                            {
                                var value = dataDict[csvColumn];
                                SetPropertyValue(entity, kvp.Key, value);
                            }
                        }

                        // Check if exists
                        if (skipExisting)
                        {
                            var primaryKeyColumn = metadata.PrimaryKeyColumn;
                            if (!string.IsNullOrEmpty(primaryKeyColumn))
                            {
                                var pkValue = GetPropertyValue(entity, primaryKeyColumn);
                                if (pkValue != null)
                                {
                                    var existing = await repository.GetByIdAsync(pkValue.ToString());
                                    if (existing != null)
                                    {
                                        result.RecordsSkipped++;
                                        result.RecordsProcessed++;
                                        continue;
                                    }
                                }
                            }
                        }

                        // Insert
                        if (entity is IPPDMEntity ppdmEntity)
                            _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
                        var inserted = await repository.InsertAsync(entity, userId);
                        if (inserted != null)
                        {
                            result.RecordsInserted++;
                        }
                        result.RecordsProcessed++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {i + 1}: {ex.Message}");
                    }
                }

                if (result.Errors.Any())
                {
                    result.Success = false;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Import error: {ex.Message}");
            }

            return result;
        }

        private Dictionary<string, string> CreateDefaultMapping(string[] headers)
        {
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers)
            {
                mapping[header] = header; // Default: CSV column name = entity property name
            }
            return mapping;
        }

        private Dictionary<string, string> CreateDefaultMappingForTable(string[] headers, Type entityType)
        {
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var properties = entityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            foreach (var header in headers)
            {
                var prop = properties.FirstOrDefault(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
                if (prop != null)
                {
                    mapping[prop.Name] = header;
                }
            }
            return mapping;
        }

        private string[] ParseCSVLine(string line)
        {
            var values = new List<string>();
            var current = "";
            var inQuotes = false;

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            values.Add(current);
            return values.ToArray();
        }

        private void SetPropertyValue(object entity, string propertyName, string value)
        {
            var prop = entity.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop != null && prop.CanWrite)
            {
                var convertedValue = ConvertValue(value, prop.PropertyType);
                prop.SetValue(entity, convertedValue);
            }
        }

        private object? GetPropertyValue(object entity, string propertyName)
        {
            var prop = entity.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            return prop?.GetValue(entity);
        }

        private object? ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(string))
                return value;

            if (underlyingType == typeof(int) || underlyingType == typeof(Int32))
            {
                if (int.TryParse(value, out var result))
                    return result;
            }

            if (underlyingType == typeof(long) || underlyingType == typeof(Int64))
            {
                if (long.TryParse(value, out var result))
                    return result;
            }

            if (underlyingType == typeof(decimal) || underlyingType == typeof(Decimal))
            {
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    return result;
            }

            if (underlyingType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, out var result))
                    return result;
            }

            if (underlyingType == typeof(bool) || underlyingType == typeof(Boolean))
            {
                if (bool.TryParse(value, out var result))
                    return result;
                if (value.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    return true;
                if (value.Equals("N", StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return Convert.ChangeType(value, underlyingType);
        }
    }

    /// <summary>
    /// Result of CSV import operation
    /// </summary>
    public class ImportResult
    {
        public bool Success { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsInserted { get; set; }
        public int RecordsSkipped { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}

