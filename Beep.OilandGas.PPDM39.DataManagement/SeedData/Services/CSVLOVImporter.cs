using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
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
        private readonly LOVManagementService _lovService;
        private readonly string _connectionName;

        public CSVLOVImporter(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            LOVManagementService lovService,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _lovService = lovService ?? throw new ArgumentNullException(nameof(lovService));
            _connectionName = connectionName;
        }

        /// <summary>
        /// Imports LOV data from CSV file to LIST_OF_VALUE table
        /// </summary>
        public async Task<ImportResult> ImportToListOfValueAsync(string csvFilePath, Dictionary<string, string>? columnMapping = null, bool skipExisting = true, string userId = "SYSTEM", string connectionName = "PPDM39")
        {
            var result = new ImportResult
            {
                Success = true,
                RecordsProcessed = 0,
                RecordsInserted = 0,
                RecordsSkipped = 0,
                Errors = new List<string>()
            };

            // OILGAS-CATCH-01: what a row of the file gets wrong is recorded against that row in this importer's words, and
            // the import goes on. A failure — the file cannot be read, the database refuses the write — is not a row's
            // error: it reaches the caller. Both were caught and recorded in the exception's own words.
            if (!File.Exists(csvFilePath))
            {
                result.Success = false;
                result.Errors.Add($"CSV file not found: {Path.GetFileName(csvFilePath)}");
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
            var lovsToImport = new List<LIST_OF_VALUE>();

            // Process data rows and build LOV list
            for (int i = 1; i < lines.Length; i++)
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
                var problem = MapRow(lov, mapping, dataDict);
                if (problem != null)
                {
                    result.Errors.Add($"Row {i + 1}: {problem}");
                    continue;
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

                lovsToImport.Add(lov);
                result.RecordsProcessed++;
            }

            // Bulk import using LOVManagementService
            var bulkResult = await _lovService.BulkAddLOVsAsync(lovsToImport, userId, skipExisting, resolvedConnectionName);
            result.RecordsInserted = bulkResult.TotalInserted;
            result.RecordsSkipped = bulkResult.TotalSkipped;
            result.Errors.AddRange(bulkResult.Errors);

            if (result.Errors.Any())
            {
                result.Success = false;
            }

            return result;
        }

        /// <summary>
        /// Imports LOV data from CSV to a specific RA_* table
        /// </summary>
        public async Task<ImportResult> ImportToRATableAsync(string csvFilePath, string tableName, Dictionary<string, string>? columnMapping = null, bool skipExisting = true, string userId = "SYSTEM", string connectionName = "PPDM39")
        {
            var result = new ImportResult
            {
                Success = true,
                RecordsProcessed = 0,
                RecordsInserted = 0,
                RecordsSkipped = 0,
                Errors = new List<string>()
            };

            // As for LIST_OF_VALUE: a row's problems are recorded against it; a failure reaches the caller.
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
                result.Errors.Add($"CSV file not found: {Path.GetFileName(csvFilePath)}");
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
                var values = ParseCSVLine(lines[i]);
                if (values.Length != headers.Length)
                {
                    result.Errors.Add($"Row {i + 1}: Column count mismatch");
                    continue;
                }

                var entity = Activator.CreateInstance(entityType)
                    ?? throw new InvalidOperationException($"Cannot create an instance of {entityType.Name} to import {tableName}.");
                var dataDict = new Dictionary<string, string>();
                for (int j = 0; j < headers.Length; j++)
                {
                    dataDict[headers[j]] = values[j];
                }

                // Map CSV columns to entity properties
                var mapping = columnMapping ?? CreateDefaultMappingForTable(headers, entityType);
                var problem = MapRow(entity, mapping, dataDict);
                if (problem != null)
                {
                    result.Errors.Add($"Row {i + 1}: {problem}");
                    continue;
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

            if (result.Errors.Any())
            {
                result.Success = false;
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

        /// <summary>
        /// Sets the entity's mapped properties from a row; answers the first value that is not of its column's type, worded
        /// for the person who sent the file, or null when every value fitted.
        /// </summary>
        private static string? MapRow(object entity, Dictionary<string, string> mapping, Dictionary<string, string> row)
        {
            foreach (var kvp in mapping)
            {
                if (!row.TryGetValue(kvp.Value, out var value))
                    continue;
                var problem = SetPropertyValue(entity, kvp.Key, kvp.Value, value);
                if (problem != null)
                    return problem;
            }
            return null;
        }

        /// <summary>
        /// Sets one property from a cell; answers why the cell does not fit the property's type, or null. An empty cell
        /// leaves the property empty. The value was converted with Convert.ChangeType, whose refusal was caught with the
        /// row and recorded in the converter's words.
        /// </summary>
        private static string? SetPropertyValue(object entity, string propertyName, string column, string value)
        {
            var prop = entity.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop == null || !prop.CanWrite)
                return null;

            if (string.IsNullOrEmpty(value))
            {
                prop.SetValue(entity, null);
                return null;
            }

            if (!CsvValueConversion.TryConvert(value, prop.PropertyType, out var convertedValue))
                return CsvValueConversion.Refusal(column, value, prop.PropertyType);

            prop.SetValue(entity, convertedValue);
            return null;
        }

        private object? GetPropertyValue(object entity, string propertyName)
        {
            var prop = entity.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            return prop?.GetValue(entity);
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

