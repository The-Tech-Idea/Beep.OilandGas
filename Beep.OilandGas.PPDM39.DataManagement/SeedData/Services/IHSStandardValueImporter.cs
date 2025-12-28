using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.SeedData.Services;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData.Services
{
    /// <summary>
    /// Imports IHS reference codes and maps to appropriate tables (RA_* or LIST_OF_VALUE)
    /// </summary>
    public class IHSStandardValueImporter
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly CSVLOVImporter _csvImporter;
        private readonly StandardValueMapper _valueMapper;
        private readonly string _connectionName;

        public IHSStandardValueImporter(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            CSVLOVImporter csvImporter,
            StandardValueMapper valueMapper,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _csvImporter = csvImporter ?? throw new ArgumentNullException(nameof(csvImporter));
            _valueMapper = valueMapper ?? throw new ArgumentNullException(nameof(valueMapper));
            _connectionName = connectionName;
        }

        /// <summary>
        /// Imports IHS reference data from JSON file
        /// </summary>
        public async Task<ImportResult> ImportFromJSONAsync(string jsonFilePath, bool mapToPPDM = true, bool skipExisting = true, string userId = "SYSTEM")
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
                if (!File.Exists(jsonFilePath))
                {
                    result.Success = false;
                    result.Errors.Add($"JSON file not found: {jsonFilePath}");
                    return result;
                }

                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("ihsData", out var ihsData))
                {
                    foreach (var category in ihsData.EnumerateObject())
                    {
                        var categoryName = category.Name;
                        var categoryData = category.Value;

                        if (categoryData.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in categoryData.EnumerateArray())
                            {
                                try
                                {
                                    var ihsCode = item.GetProperty("code").GetString();
                                    var ihsName = item.GetProperty("name").GetString();
                                    var description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null;
                                    var targetTable = item.TryGetProperty("targetTable", out var table) ? table.GetString() : null;

                                    // Map to PPDM if requested
                                    if (mapToPPDM && !string.IsNullOrEmpty(targetTable))
                                    {
                                        var mappedResult = await _valueMapper.MapIHSToPPDMAsync(ihsCode, ihsName, targetTable, skipExisting, userId);
                                        result.RecordsProcessed += mappedResult.RecordsProcessed;
                                        result.RecordsInserted += mappedResult.RecordsInserted;
                                        result.RecordsSkipped += mappedResult.RecordsSkipped;
                                        result.Errors.AddRange(mappedResult.Errors);
                                    }
                                    else
                                    {
                                        // Store in LIST_OF_VALUE
                                        var lov = new LIST_OF_VALUE
                                        {
                                            LIST_OF_VALUE_ID = Guid.NewGuid().ToString(),
                                            VALUE_TYPE = categoryName,
                                            VALUE_CODE = ihsCode ?? string.Empty,
                                            VALUE_NAME = ihsName ?? string.Empty,
                                            DESCRIPTION = description,
                                            CATEGORY = "IHS",
                                            SOURCE = "IHS",
                                            ACTIVE_IND = "Y"
                                        };

                                        var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                                            typeof(LIST_OF_VALUE), _connectionName, "LIST_OF_VALUE");

                                        if (skipExisting)
                                        {
                                            var filters = new List<TheTechIdea.Beep.Report.AppFilter>
                                            {
                                                new TheTechIdea.Beep.Report.AppFilter { FieldName = "VALUE_TYPE", Operator = "=", FilterValue = categoryName },
                                                new TheTechIdea.Beep.Report.AppFilter { FieldName = "VALUE_CODE", Operator = "=", FilterValue = ihsCode }
                                            };
                                            var existing = await repository.GetAsync(filters);
                                            if (existing.Any())
                                            {
                                                result.RecordsSkipped++;
                                                result.RecordsProcessed++;
                                                continue;
                                            }
                                        }

                                        if (lov is IPPDMEntity lovEntity)
                                            _commonColumnHandler.PrepareForInsert(lovEntity, userId);
                                        var inserted = await repository.InsertAsync(lov, userId);
                                        if (inserted != null)
                                        {
                                            result.RecordsInserted++;
                                        }
                                        result.RecordsProcessed++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    result.Errors.Add($"Error importing IHS item: {ex.Message}");
                                }
                            }
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
    }
}

