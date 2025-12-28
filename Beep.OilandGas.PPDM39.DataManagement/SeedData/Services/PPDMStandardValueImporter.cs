using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.SeedData.Services;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData.Services
{
    /// <summary>
    /// Imports PPDM standard values from PPDM Reference Lists and PPDMCSVData.json
    /// </summary>
    public class PPDMStandardValueImporter
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly CSVLOVImporter _csvImporter;
        private readonly string _connectionName;

        public PPDMStandardValueImporter(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            CSVLOVImporter csvImporter,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _csvImporter = csvImporter ?? throw new ArgumentNullException(nameof(csvImporter));
            _connectionName = connectionName;
        }

        /// <summary>
        /// Imports PPDM standard values from PPDMCSVData.json
        /// </summary>
        public async Task<ImportResult> ImportFromPPDMCSVDataAsync(string? tableName = null, bool skipExisting = true, string userId = "SYSTEM")
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
                var csvDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Core", "SeedData", "PPDMCSVData.json");
                if (!File.Exists(csvDataPath))
                {
                    result.Success = false;
                    result.Errors.Add($"PPDMCSVData.json not found at: {csvDataPath}");
                    return result;
                }

                var jsonContent = await File.ReadAllTextAsync(csvDataPath);
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var tableProp in root.EnumerateObject())
                    {
                        var currentTableName = tableProp.Name;
                        if (!string.IsNullOrEmpty(tableName) && !currentTableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // Only process RA_* tables
                        if (!currentTableName.StartsWith("RA_", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        try
                        {
                            var tableData = tableProp.Value;
                            if (tableData.ValueKind == JsonValueKind.Array)
                            {
                                var records = new List<Dictionary<string, object>>();
                                foreach (var record in tableData.EnumerateArray())
                                {
                                    var recordDict = new Dictionary<string, object>();
                                    foreach (var prop in record.EnumerateObject())
                                    {
                                        recordDict[prop.Name] = prop.Value.ValueKind switch
                                        {
                                            JsonValueKind.String => prop.Value.GetString() ?? string.Empty,
                                            JsonValueKind.Number => prop.Value.GetDecimal(),
                                            JsonValueKind.True => true,
                                            JsonValueKind.False => false,
                                            JsonValueKind.Null => null!,
                                            _ => prop.Value.GetRawText()
                                        };
                                    }
                                    records.Add(recordDict);
                                }

                                // Import to RA_* table
                                var importResult = await ImportToRATableAsync(currentTableName, records, skipExisting, userId);
                                result.RecordsProcessed += importResult.RecordsProcessed;
                                result.RecordsInserted += importResult.RecordsInserted;
                                result.RecordsSkipped += importResult.RecordsSkipped;
                                result.Errors.AddRange(importResult.Errors);
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Error importing {currentTableName}: {ex.Message}");
                        }
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
        /// Imports data to a specific RA_* table
        /// </summary>
        private async Task<ImportResult> ImportToRATableAsync(string tableName, List<Dictionary<string, object>> records, bool skipExisting, string userId)
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

                var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, tableName);

                foreach (var record in records)
                {
                    try
                    {
                        var entity = Activator.CreateInstance(entityType);
                        var entityTypeInfo = entityType;

                        // Set properties from record
                        foreach (var kvp in record)
                        {
                            var prop = entityTypeInfo.GetProperty(kvp.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                            if (prop != null && prop.CanWrite)
                            {
                                var value = ConvertValue(kvp.Value, prop.PropertyType);
                                prop.SetValue(entity, value);
                            }
                        }

                        // Check if exists
                        if (skipExisting)
                        {
                            var primaryKeyColumn = metadata.PrimaryKeyColumn;
                            if (!string.IsNullOrEmpty(primaryKeyColumn) && record.ContainsKey(primaryKeyColumn))
                            {
                                var pkValue = record[primaryKeyColumn]?.ToString();
                                if (!string.IsNullOrEmpty(pkValue))
                                {
                                    var existing = await repository.GetByIdAsync(pkValue);
                                    if (existing != null)
                                    {
                                        result.RecordsSkipped++;
                                        result.RecordsProcessed++;
                                        continue;
                                    }
                                }
                            }
                        }

                        // Set common columns
                        if (entity is IPPDMEntity ppdmEntity)
                            _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);

                        // Insert
                        var inserted = await repository.InsertAsync(entity, userId);
                        if (inserted != null)
                        {
                            result.RecordsInserted++;
                        }
                        result.RecordsProcessed++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error importing record in {tableName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error importing {tableName}: {ex.Message}");
            }

            return result;
        }

        private object? ConvertValue(object? value, Type targetType)
        {
            if (value == null || value == DBNull.Value)
                return null;

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType.IsAssignableFrom(value.GetType()))
                return value;

            if (underlyingType == typeof(string))
                return value.ToString();

            if (underlyingType == typeof(DateTime))
            {
                if (value is DateTime dt)
                    return dt;
                if (DateTime.TryParse(value.ToString(), out var parsed))
                    return parsed;
            }

            if (underlyingType == typeof(decimal) || underlyingType == typeof(Decimal))
            {
                if (decimal.TryParse(value.ToString(), out var parsed))
                    return parsed;
            }

            if (underlyingType == typeof(int) || underlyingType == typeof(Int32))
            {
                if (int.TryParse(value.ToString(), out var parsed))
                    return parsed;
            }

            if (underlyingType == typeof(bool) || underlyingType == typeof(Boolean))
            {
                if (bool.TryParse(value.ToString(), out var parsed))
                    return parsed;
                if (value.ToString()?.Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
                    return true;
                if (value.ToString()?.Equals("N", StringComparison.OrdinalIgnoreCase) == true)
                    return false;
            }

            return Convert.ChangeType(value, underlyingType);
        }
    }
}

