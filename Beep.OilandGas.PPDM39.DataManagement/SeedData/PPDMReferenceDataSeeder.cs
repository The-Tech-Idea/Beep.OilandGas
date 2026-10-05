using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Beep.Editor;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Central reference-data/template import infrastructure for PPDM setup.
    /// This service stays in PPDM39.DataManagement because it encapsulates generic metadata,
    /// LOV, and repository-based reference-data import behavior reused across modules.
    /// </summary>
    public class PPDMReferenceDataSeeder
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly LOVManagementService _lovService;
        private readonly IFailureReporter _failures;
        private readonly string _connectionName;

        public PPDMReferenceDataSeeder(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            LOVManagementService lovService,
            IFailureReporter failures,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _lovService = lovService ?? throw new ArgumentNullException(nameof(lovService));
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
            _connectionName = connectionName;
        }

        /// <summary>
        /// Seeds reference data for a specific table
        /// </summary>
        public async Task<int> SeedReferenceTableAsync(string tableName, List<Dictionary<string, object>> seedData, string userId = "SYSTEM")
        {
            try
            {
                var tableType = LOVManagementService.GetTableType(tableName);
                
                // Use LOVManagementService for LIST_OF_VALUE, R_*, and RA_* tables
                if (tableType == Beep.OilandGas.PPDM39.DataManagement.Services.ReferenceTableType.ListOfValue || 
                    tableType == Beep.OilandGas.PPDM39.DataManagement.Services.ReferenceTableType.RTable || 
                    tableType == Beep.OilandGas.PPDM39.DataManagement.Services.ReferenceTableType.RATable)
                {
                    // Get entity type
                    var entityType = await _lovService.GetEntityTypeAsync(tableName);
                    if (entityType == null)
                    {
                        throw new InvalidOperationException($"Entity type not found for table: {tableName}");
                    }

                    // Check if it's LIST_OF_VALUE for direct handling
                    if (tableType == Beep.OilandGas.PPDM39.DataManagement.Services.ReferenceTableType.ListOfValue && entityType == typeof(LIST_OF_VALUE))
                    {
                        var lovsToImport = new List<LIST_OF_VALUE>();

                        // OILGAS-CATCH-01: a row the template gets wrong fails the table's seeding, which the caller
                        // reports and records against the table. It was written to the console and left out, and the
                        // table counted as seeded without it.
                        foreach (var dataRow in seedData)
                        {
                            var lov = new LIST_OF_VALUE();

                            // Set properties from seed data
                            foreach (var kvp in dataRow)
                            {
                                var prop = typeof(LIST_OF_VALUE).GetProperty(kvp.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                                if (prop != null && prop.CanWrite)
                                {
                                    var value = ConvertValue(kvp.Value, prop.PropertyType);
                                    prop.SetValue(lov, value);
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

                            lovsToImport.Add(lov);
                        }

                        // Bulk import using LOVManagementService
                        if (lovsToImport.Any())
                        {
                            var bulkResult = await _lovService.BulkAddLOVsAsync(lovsToImport, userId, true, _connectionName);
                            return bulkResult.TotalInserted;
                        }

                        return 0;
                    }
                    else
                    {
                        // Use generic method for R_* and RA_* tables
                        var entitiesToImport = new List<object>();
                        
                        // As above: a row the template gets wrong fails the table rather than being left out.
                        foreach (var dataRow in seedData)
                        {
                            var entity = Activator.CreateInstance(entityType)
                                ?? throw new InvalidOperationException($"Cannot create an instance of {entityType.Name} to seed {tableName}.");

                            // Set properties from seed data
                            foreach (var kvp in dataRow)
                            {
                                var prop = entityType.GetProperty(kvp.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                                if (prop != null && prop.CanWrite)
                                {
                                    var value = ConvertValue(kvp.Value, prop.PropertyType);
                                    prop.SetValue(entity, value);
                                }
                            }

                            // Set defaults if needed
                            if (entity is IPPDMEntity ppdmEntity)
                            {
                                var activeIndProp = entityType.GetProperty("ACTIVE_IND", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                                if (activeIndProp != null && activeIndProp.GetValue(entity) == null)
                                {
                                    activeIndProp.SetValue(entity, "Y");
                                }
                            }

                            entitiesToImport.Add(entity);
                        }

                        // Use reflection to call generic BulkAddReferenceValuesAsync
                        if (entitiesToImport.Any())
                        {
                            var method = typeof(LOVManagementService).GetMethod("BulkAddReferenceValuesAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (method != null)
                            {
                                var genericMethod = method.MakeGenericMethod(entityType);
                                var task = genericMethod.Invoke(_lovService, new object[] { entitiesToImport, userId, true, _connectionName }) as Task<BulkReferenceResult>;
                                if (task != null)
                                {
                                    var bulkResult = await task;
                                    return bulkResult.TotalInserted;
                                }
                            }
                        }

                        return 0;
                    }
                }

                // Fallback: Generic handling for other tables (non-reference tables)
                var metadata = await _metadata.GetTableMetadataAsync(tableName);
                if (metadata == null)
                {
                    throw new InvalidOperationException($"Table metadata not found for: {tableName}");
                }

                var fallbackEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ??
                                Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

                if (fallbackEntityType == null)
                {
                    throw new InvalidOperationException($"Entity type not found for table: {tableName}");
                }

                var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    fallbackEntityType, _connectionName, tableName);

                int seeded = 0;

                // A row that cannot be prepared or written fails the table (OILGAS-CATCH-01): it was written to the
                // console and skipped, and the count returned read as the table's whole seeding.
                foreach (var dataRow in seedData)
                {
                    var entity = Activator.CreateInstance(fallbackEntityType);
                    var entityTypeInfo = fallbackEntityType;

                    // Set properties from seed data
                    foreach (var kvp in dataRow)
                    {
                        var prop = entityTypeInfo.GetProperty(kvp.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                        if (prop != null && prop.CanWrite)
                        {
                            var value = ConvertValue(kvp.Value, prop.PropertyType);
                            prop.SetValue(entity, value);
                        }
                    }

                    // Set common columns
                    if (entity is IPPDMEntity ppdmEntity)
                        _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);

                    // Insert entity
                    var result = await repository.InsertAsync(entity, userId);
                    if (result != null)
                    {
                        seeded++;
                    }
                }

                return seeded;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error seeding reference table {tableName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Seeds standard accounting method reference data
        /// </summary>
        public async Task<int> SeedAccountingMethodsAsync(string userId = "SYSTEM")
        {
            var seedData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["ACCOUNTING_METHOD_ID"] = "SE",
                    ["ACCOUNTING_METHOD_NAME"] = "Successful Efforts",
                    ["DESCRIPTION"] = "Successful Efforts accounting method",
                    ["ACTIVE_IND"] = "Y"
                },
                new Dictionary<string, object>
                {
                    ["ACCOUNTING_METHOD_ID"] = "FC",
                    ["ACCOUNTING_METHOD_NAME"] = "Full Cost",
                    ["DESCRIPTION"] = "Full Cost accounting method",
                    ["ACTIVE_IND"] = "Y"
                }
            };

            return await SeedReferenceTableAsync("ACCOUNTING_METHOD", seedData, userId);
        }

        /// <summary>
        /// Seeds standard cost type reference data
        /// </summary>
        public async Task<int> SeedCostTypesAsync(string userId = "SYSTEM")
        {
            var seedData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["COST_TYPE_ID"] = "EXPLORATION",
                    ["COST_TYPE_NAME"] = "Exploration",
                    ["DESCRIPTION"] = "Exploration costs",
                    ["ACTIVE_IND"] = "Y"
                },
                new Dictionary<string, object>
                {
                    ["COST_TYPE_ID"] = "DEVELOPMENT",
                    ["COST_TYPE_NAME"] = "Development",
                    ["DESCRIPTION"] = "Development costs",
                    ["ACTIVE_IND"] = "Y"
                },
                new Dictionary<string, object>
                {
                    ["COST_TYPE_ID"] = "PRODUCTION",
                    ["COST_TYPE_NAME"] = "Production",
                    ["DESCRIPTION"] = "Production costs",
                    ["ACTIVE_IND"] = "Y"
                }
            };

            // Note: This assumes a COST_TYPE reference table exists
            // If not, this would need to be adapted to the actual table structure
            return 0; // Placeholder - implement when COST_TYPE table is defined
        }

        /// <summary>
        /// Seeds all PPDM reference tables (R_* tables) from JSON template
        /// </summary>
        public async Task<SeedDataResponse> SeedPPDMReferenceTablesAsync(string connectionName = "PPDM39", List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var templatePath = GetTemplatePath("PPDMReferenceData.json");
            return await SeedFromTemplateAsync(templatePath, connectionName, tableNames, skipExisting, userId);
        }

        /// <summary>
        /// Seeds accounting-specific reference data from JSON template
        /// </summary>
        public async Task<SeedDataResponse> SeedAccountingReferenceDataAsync(string connectionName = "PPDM39", List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var templatePath = GetTemplatePath("AccountingSeedData.json");
            return await SeedFromTemplateAsync(templatePath, connectionName, tableNames, skipExisting, userId);
        }

        /// <summary>
        /// Seeds lifecycle-specific reference data from JSON template
        /// </summary>
        public async Task<SeedDataResponse> SeedLifeCycleReferenceDataAsync(string connectionName = "PPDM39", List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var templatePath = GetTemplatePath("LifeCycleSeedData.json");
            return await SeedFromTemplateAsync(templatePath, connectionName, tableNames, skipExisting, userId);
        }

        /// <summary>
        /// Seeds analysis result reference data from JSON template
        /// </summary>
        public async Task<SeedDataResponse> SeedAnalysisReferenceDataAsync(string connectionName = "PPDM39", List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var templatePath = GetTemplatePath("AnalysisSeedData.json");
            return await SeedFromTemplateAsync(templatePath, connectionName, tableNames, skipExisting, userId);
        }

        /// <summary>
        /// Seeds data by category
        /// </summary>
        public async Task<SeedDataResponse> SeedByCategoryAsync(string category, string connectionName = "PPDM39", List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            return category.ToUpperInvariant() switch
            {
                "PPDM" => await SeedPPDMReferenceTablesAsync(connectionName, tableNames, skipExisting, userId),
                "ACCOUNTING" => await SeedAccountingReferenceDataAsync(connectionName, tableNames, skipExisting, userId),
                "LIFECYCLE" => await SeedLifeCycleReferenceDataAsync(connectionName, tableNames, skipExisting, userId),
                "ANALYSIS" => await SeedAnalysisReferenceDataAsync(connectionName, tableNames, skipExisting, userId),
                "CUSTOM" => await SeedListOfValueTableAsync(connectionName, tableNames, skipExisting, userId),
                "IHS" => await SeedIHSReferenceDataAsync(connectionName, tableNames, skipExisting, userId),
                "INDUSTRYSTANDARDS" => await SeedIndustryStandardsDataAsync(connectionName, tableNames, skipExisting, userId),
                _ => new SeedDataResponse
                {
                    Success = false,
                    Message = $"Unknown category: {category}",
                    TablesSeeded = 0,
                    RecordsInserted = 0
                }
            };
        }

        /// <summary>
        /// Seeds LIST_OF_VALUE table from CustomLOVSeedData.json
        /// </summary>
        public async Task<SeedDataResponse> SeedListOfValueTableAsync(string connectionName = "PPDM39", List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var templatePath = GetTemplatePath("CustomLOVSeedData.json");
            return await SeedFromTemplateAsync(templatePath, connectionName, tableNames, skipExisting, userId);
        }

        /// <summary>
        /// Seeds IHS reference data from IHSReferenceData.json
        /// </summary>
        public async Task<SeedDataResponse> SeedIHSReferenceDataAsync(string connectionName = "PPDM39", List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;

            // This route seeds nothing: the IHS template is imported by IHSStandardValueImporter through the import
            // endpoint. It answered "seeding completed" with Success = true after reading the template and doing nothing,
            // and a template it could not parse was caught and answered in the parser's words (OILGAS-CATCH-01). It now
            // says what it does, so a caller choosing the IHS category is not told data was seeded.
            var templatePath = GetTemplatePath("IHSReferenceData.json");
            return await Task.FromResult(new SeedDataResponse
            {
                Success = false,
                Message = File.Exists(templatePath)
                    ? "IHS reference data is not seeded by this route. Import it through the IHS import endpoint."
                    : $"Template file not found: {Path.GetFileName(templatePath)}",
                TableResults = new List<TableSeedResult>()
            });
        }

        /// <summary>
        /// Seeds industry standards data from IndustryStandardsReferenceData.json
        /// </summary>
        public async Task<SeedDataResponse> SeedIndustryStandardsDataAsync(string connectionName = "PPDM39", List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var response = new SeedDataResponse
            {
                Success = true,
                Message = "Industry standards data seeding completed",
                TableResults = new List<TableSeedResult>()
            };

            var templatePath = GetTemplatePath("IndustryStandardsReferenceData.json");
            if (!File.Exists(templatePath))
            {
                response.Success = false;
                response.Message = $"Template file not found: {Path.GetFileName(templatePath)}";
                return response;
            }

            var jsonContent = await File.ReadAllTextAsync(templatePath);
            if (!TryParseTemplate(jsonContent, templatePath, out var jsonDoc, out var unreadable))
            {
                response.Success = false;
                response.Message = unreadable;
                return response;
            }

            using (jsonDoc)
            {
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("standards", out var standards) && standards.ValueKind == JsonValueKind.Object)
                {
                    // Import API, ISO, and Regulatory standards to LIST_OF_VALUE using LOVManagementService
                    var lovsToImport = new List<LIST_OF_VALUE>();

                    foreach (var standardType in standards.EnumerateObject())
                    {
                        var standardName = standardType.Name;
                        var standardData = standardType.Value;

                        if (standardData.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in standardData.EnumerateArray())
                            {
                                // Each value is asked for by kind (OILGAS-CATCH-01): an item without a code or name was
                                // thrown on, caught, and recorded in the reader's own words.
                                var code = SeedTemplateJson.StringProperty(item, "code");
                                var name = SeedTemplateJson.StringProperty(item, "name");
                                if (code == null || name == null)
                                {
                                    response.Errors.Add($"A {standardName} item in the industry standards template has no code or name and was not imported.");
                                    continue;
                                }

                                var description = SeedTemplateJson.StringProperty(item, "description");
                                var valueType = SeedTemplateJson.StringProperty(item, "valueType") ?? standardName;
                                var storeInLOV = item.ValueKind == JsonValueKind.Object &&
                                                 item.TryGetProperty("storeInLOV", out var sil) && sil.ValueKind == JsonValueKind.True;

                                if (storeInLOV)
                                {
                                    var lov = new LIST_OF_VALUE
                                    {
                                        LIST_OF_VALUE_ID = Guid.NewGuid().ToString(),
                                        VALUE_TYPE = valueType,
                                        VALUE_CODE = code,
                                        VALUE_NAME = name,
                                        DESCRIPTION = description,
                                        CATEGORY = "IndustryStandards",
                                        SOURCE = standardName,
                                        ACTIVE_IND = "Y"
                                    };

                                    lovsToImport.Add(lov);
                                }
                            }
                        }
                    }

                    // Bulk import using LOVManagementService. A failure to write reaches the caller: it was caught and
                    // answered in the provider's words.
                    if (lovsToImport.Any())
                    {
                        var bulkResult = await _lovService.BulkAddLOVsAsync(lovsToImport, userId, skipExisting, connectionName);
                        response.RecordsInserted += bulkResult.TotalInserted;
                        response.RecordsSkipped += bulkResult.TotalSkipped;
                        response.Errors.AddRange(bulkResult.Errors);
                    }
                }
            }

            if (response.Errors.Count > 0)
            {
                response.Success = false;
                response.Message = $"Industry standards data seeded with {response.Errors.Count} problem(s).";
            }

            return response;
        }

        /// <summary>
        /// Parses a shipped seed template. The JSON reader has no question to ask before parsing, so a template it refuses
        /// is caught here — narrowly — reported, and answered with a sentence carrying the reference.
        /// </summary>
        private bool TryParseTemplate(string jsonContent, string templatePath, out JsonDocument document, out string unreadable)
        {
            try
            {
                document = JsonDocument.Parse(jsonContent);
                unreadable = string.Empty;
                return true;
            }
            catch (JsonException ex)
            {
                document = null!;
                unreadable = ReportedFailure.Sentence(_failures, ex,
                    $"reading the seed template {Path.GetFileName(templatePath)}",
                    $"The seed template {Path.GetFileName(templatePath)} could not be read, so nothing was seeded from it.");
                return false;
            }
        }

        /// <summary>
        /// Seeds data from a JSON template file
        /// </summary>
        private async Task<SeedDataResponse> SeedFromTemplateAsync(string templatePath, string connectionName, List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            var response = new SeedDataResponse
            {
                Success = true,
                Message = "Seed operation completed",
                TableResults = new List<TableSeedResult>()
            };

            if (!File.Exists(templatePath))
            {
                response.Success = false;
                response.Message = $"Template file not found: {Path.GetFileName(templatePath)}";
                return response;
            }

            var jsonContent = await File.ReadAllTextAsync(templatePath);
            if (!TryParseTemplate(jsonContent, templatePath, out var jsonDoc, out var unreadable))
            {
                response.Success = false;
                response.Message = unreadable;
                return response;
            }

            // The template is read by asking each element for its kind (OILGAS-CATCH-01): a missing or mistyped property
            // was thrown on and caught with the whole template, and answered in the reader's own words.
            var template = new SeedDataTemplate { Tables = new List<TableTemplate>() };
            using (jsonDoc)
            {
                var root = jsonDoc.RootElement;
                template.Category = SeedTemplateJson.StringProperty(root, "category") ?? string.Empty;
                template.Version = SeedTemplateJson.StringProperty(root, "version") ?? string.Empty;
                template.Description = SeedTemplateJson.StringProperty(root, "description") ?? string.Empty;

                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("tables", out var tablesElement) &&
                    tablesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tableElement in tablesElement.EnumerateArray())
                    {
                        var tableName = SeedTemplateJson.StringProperty(tableElement, "tableName");
                        if (string.IsNullOrWhiteSpace(tableName))
                        {
                            response.Errors.Add($"A table in the template {Path.GetFileName(templatePath)} has no table name and was not seeded.");
                            continue;
                        }

                        var tableTemplate = new TableTemplate
                        {
                            TableName = tableName,
                            Description = SeedTemplateJson.StringProperty(tableElement, "description") ?? string.Empty,
                            Data = new List<Dictionary<string, object>>()
                        };

                        if (tableElement.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var dataRow in dataElement.EnumerateArray())
                            {
                                if (dataRow.ValueKind != JsonValueKind.Object)
                                    continue;
                                var rowDict = new Dictionary<string, object>();
                                foreach (var prop in dataRow.EnumerateObject())
                                    rowDict[prop.Name] = SeedTemplateJson.Value(prop.Value);
                                tableTemplate.Data.Add(rowDict);
                            }
                        }

                        template.Tables.Add(tableTemplate);
                    }
                }
            }

            var tablesToSeed = template.Tables;
            if (tableNames != null && tableNames.Any())
            {
                tablesToSeed = tablesToSeed.Where(t => tableNames.Contains(t.TableName, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            foreach (var tableTemplate in tablesToSeed)
            {
                var tableResult = new TableSeedResult
                {
                    TableName = tableTemplate.TableName,
                    Success = false
                };

                try
                {
                    if (skipExisting)
                    {
                        // Check if table already has data
                        var metadata = await _metadata.GetTableMetadataAsync(tableTemplate.TableName);
                        if (metadata != null)
                        {
                            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ??
                                            Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");
                            if (entityType != null)
                            {
                                var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                                    entityType, connectionName, tableTemplate.TableName);
                                var existing = await repository.GetAsync(new List<TheTechIdea.Beep.Report.AppFilter>());
                                if (existing.Any())
                                {
                                    tableResult.Success = true;
                                    tableResult.RecordsSkipped = tableTemplate.Data?.Count ?? 0;
                                    response.RecordsSkipped += tableResult.RecordsSkipped;
                                    response.TableResults.Add(tableResult);
                                    continue;
                                }
                            }
                        }
                    }

                    var seedData = tableTemplate.Data?.Select(d =>
                        d.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)).ToList() ?? new List<Dictionary<string, object>>();

                    var seeded = await SeedReferenceTableAsync(tableTemplate.TableName, seedData, userId);
                    tableResult.Success = true;
                    tableResult.RecordsInserted = seeded;
                    response.RecordsInserted += seeded;
                    response.TablesSeeded++;
                }
                // Broad: each table is seeded on its own and its result says how it went — the metadata, the repository
                // and the driver each throw their own types. A table that fails is reported and recorded with the
                // reference; the others are still seeded. It used to carry the exception's text, and the response stayed
                // "successful" whatever its tables did.
                catch (Exception ex) when (ex is not RefusalException)
                {
                    tableResult.Success = false;
                    tableResult.ErrorMessage = ReportedFailure.Sentence(_failures, ex,
                        $"seeding reference table {tableTemplate.TableName} from {Path.GetFileName(templatePath)}",
                        $"Table {tableTemplate.TableName} was not seeded.");
                    response.Errors.Add(tableResult.ErrorMessage);
                }

                response.TableResults.Add(tableResult);
            }

            var failedTables = response.TableResults.Count(table => !table.Success);
            if (failedTables > 0 || response.Errors.Count > 0)
                response.Success = false;
            response.Message = failedTables > 0
                ? $"Seeded {response.TablesSeeded} table(s), inserted {response.RecordsInserted} record(s), skipped {response.RecordsSkipped} record(s); {failedTables} table(s) failed"
                : $"Seeded {response.TablesSeeded} table(s), inserted {response.RecordsInserted} record(s), skipped {response.RecordsSkipped} record(s)";

            return response;
        }

        /// <summary>
        /// Gets the path to a seed data template file
        /// </summary>
        private string GetTemplatePath(string fileName)
        {
            var basePath = AppContext.BaseDirectory;
            var solutionRoot = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", ".."));
            
            var possiblePaths = new[]
            {
                Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "SeedData", "Templates", fileName),
                Path.Combine(basePath, "SeedData", "Templates", fileName),
                Path.Combine(basePath, "..", "..", "..", "SeedData", "Templates", fileName)
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // Return the most likely path
            return Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "SeedData", "Templates", fileName);
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

    /// <summary>
    /// Seed data template structure
    /// </summary>
    internal class SeedDataTemplate
    {
        public string Category { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<TableTemplate> Tables { get; set; } = new List<TableTemplate>();
    }

    /// <summary>
    /// Table template structure
    /// </summary>
    internal class TableTemplate
    {
        public string TableName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Dictionary<string, object>>? Data { get; set; }
    }
}

