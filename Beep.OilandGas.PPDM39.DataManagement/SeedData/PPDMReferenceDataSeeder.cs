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
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Seeds reference/lookup tables with standard PPDM data
    /// </summary>
    public class PPDMReferenceDataSeeder
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly LOVManagementService _lovService;
        private readonly string _connectionName;

        public PPDMReferenceDataSeeder(
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
                        
                        foreach (var dataRow in seedData)
                        {
                            try
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
                            catch (Exception ex)
                            {
                                // Log error but continue with next row
                                Console.WriteLine($"Error preparing LOV row: {ex.Message}");
                            }
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
                        
                        foreach (var dataRow in seedData)
                        {
                            try
                            {
                                var entity = Activator.CreateInstance(entityType);
                                if (entity == null)
                                {
                                    continue;
                                }

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
                            catch (Exception ex)
                            {
                                // Log error but continue with next row
                                Console.WriteLine($"Error preparing {tableName} row: {ex.Message}");
                            }
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

                foreach (var dataRow in seedData)
                {
                    try
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
                    catch (Exception ex)
                    {
                        // Log error but continue with next row
                        Console.WriteLine($"Error seeding row in {tableName}: {ex.Message}");
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
        public async Task<SeedDataResponse> SeedPPDMReferenceTablesAsync(string? connectionName = null, List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var templatePath = GetTemplatePath("PPDMReferenceData.json");
            return await SeedFromTemplateAsync(templatePath, connectionName, tableNames, skipExisting, userId);
        }

        /// <summary>
        /// Seeds accounting-specific reference data from JSON template
        /// </summary>
        public async Task<SeedDataResponse> SeedAccountingReferenceDataAsync(string? connectionName = null, List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var templatePath = GetTemplatePath("AccountingSeedData.json");
            return await SeedFromTemplateAsync(templatePath, connectionName, tableNames, skipExisting, userId);
        }

        /// <summary>
        /// Seeds lifecycle-specific reference data from JSON template
        /// </summary>
        public async Task<SeedDataResponse> SeedLifeCycleReferenceDataAsync(string? connectionName = null, List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var templatePath = GetTemplatePath("LifeCycleSeedData.json");
            return await SeedFromTemplateAsync(templatePath, connectionName, tableNames, skipExisting, userId);
        }

        /// <summary>
        /// Seeds analysis result reference data from JSON template
        /// </summary>
        public async Task<SeedDataResponse> SeedAnalysisReferenceDataAsync(string? connectionName = null, List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var templatePath = GetTemplatePath("AnalysisSeedData.json");
            return await SeedFromTemplateAsync(templatePath, connectionName, tableNames, skipExisting, userId);
        }

        /// <summary>
        /// Seeds data by category
        /// </summary>
        public async Task<SeedDataResponse> SeedByCategoryAsync(string category, string? connectionName = null, List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
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
        public async Task<SeedDataResponse> SeedListOfValueTableAsync(string? connectionName = null, List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var templatePath = GetTemplatePath("CustomLOVSeedData.json");
            return await SeedFromTemplateAsync(templatePath, connectionName, tableNames, skipExisting, userId);
        }

        /// <summary>
        /// Seeds IHS reference data from IHSReferenceData.json
        /// </summary>
        public async Task<SeedDataResponse> SeedIHSReferenceDataAsync(string? connectionName = null, List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var response = new SeedDataResponse
            {
                Success = true,
                Message = "IHS reference data seeding completed",
                TableResults = new List<TableSeedResult>()
            };

            try
            {
                var templatePath = GetTemplatePath("IHSReferenceData.json");
                if (!File.Exists(templatePath))
                {
                    response.Success = false;
                    response.Message = $"Template file not found: {templatePath}";
                    return response;
                }

                var jsonContent = await File.ReadAllTextAsync(templatePath);
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("ihsData", out var ihsData))
                {
                    // Use IHSStandardValueImporter to import IHS data
                    // This would require injecting the importer, but for now we'll use a simplified approach
                    // The importer can be called separately via API
                    response.Message = "IHS data import requires IHSStandardValueImporter. Use import endpoint instead.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error seeding IHS reference data: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Seeds industry standards data from IndustryStandardsReferenceData.json
        /// </summary>
        public async Task<SeedDataResponse> SeedIndustryStandardsDataAsync(string? connectionName = null, List<string>? tableNames = null, bool skipExisting = true, string userId = "SYSTEM")
        {
            connectionName ??= _connectionName;
            var response = new SeedDataResponse
            {
                Success = true,
                Message = "Industry standards data seeding completed",
                TableResults = new List<TableSeedResult>()
            };

            try
            {
                var templatePath = GetTemplatePath("IndustryStandardsReferenceData.json");
                if (!File.Exists(templatePath))
                {
                    response.Success = false;
                    response.Message = $"Template file not found: {templatePath}";
                    return response;
                }

                var jsonContent = await File.ReadAllTextAsync(templatePath);
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("standards", out var standards))
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
                                try
                                {
                                    var code = item.GetProperty("code").GetString();
                                    var name = item.GetProperty("name").GetString();
                                    var description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null;
                                    var valueType = item.TryGetProperty("valueType", out var vt) ? vt.GetString() : standardName;
                                    var storeInLOV = item.TryGetProperty("storeInLOV", out var sil) && sil.GetBoolean();

                                    if (storeInLOV)
                                    {
                                        var lov = new LIST_OF_VALUE
                                        {
                                            LIST_OF_VALUE_ID = Guid.NewGuid().ToString(),
                                            VALUE_TYPE = valueType ?? standardName,
                                            VALUE_CODE = code ?? string.Empty,
                                            VALUE_NAME = name ?? string.Empty,
                                            DESCRIPTION = description,
                                            CATEGORY = "IndustryStandards",
                                            SOURCE = standardName,
                                            ACTIVE_IND = "Y"
                                        };

                                        lovsToImport.Add(lov);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    response.Errors.Add($"Error importing {standardName} item: {ex.Message}");
                                }
                            }
                        }
                    }

                    // Bulk import using LOVManagementService
                    if (lovsToImport.Any())
                    {
                        var bulkResult = await _lovService.BulkAddLOVsAsync(lovsToImport, userId, skipExisting, connectionName);
                        response.RecordsInserted += bulkResult.TotalInserted;
                        response.RecordsSkipped += bulkResult.TotalSkipped;
                        response.Errors.AddRange(bulkResult.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error seeding industry standards data: {ex.Message}";
            }

            return response;
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

            try
            {
                if (!File.Exists(templatePath))
                {
                    response.Success = false;
                    response.Message = $"Template file not found: {templatePath}";
                    return response;
                }

                var jsonContent = await File.ReadAllTextAsync(templatePath);
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;
                
                var template = new SeedDataTemplate
                {
                    Category = root.GetProperty("category").GetString() ?? string.Empty,
                    Version = root.GetProperty("version").GetString() ?? string.Empty,
                    Description = root.GetProperty("description").GetString() ?? string.Empty,
                    Tables = new List<TableTemplate>()
                };

                if (root.TryGetProperty("tables", out var tablesElement))
                {
                    foreach (var tableElement in tablesElement.EnumerateArray())
                    {
                        var tableTemplate = new TableTemplate
                        {
                            TableName = tableElement.GetProperty("tableName").GetString() ?? string.Empty,
                            Description = tableElement.GetProperty("description").GetString() ?? string.Empty,
                            Data = new List<Dictionary<string, object>>()
                        };

                        if (tableElement.TryGetProperty("data", out var dataElement))
                        {
                            foreach (var dataRow in dataElement.EnumerateArray())
                            {
                                var rowDict = new Dictionary<string, object>();
                                foreach (var prop in dataRow.EnumerateObject())
                                {
                                    rowDict[prop.Name] = prop.Value.ValueKind switch
                                    {
                                        JsonValueKind.String => prop.Value.GetString() ?? string.Empty,
                                        JsonValueKind.Number => prop.Value.GetDecimal(),
                                        JsonValueKind.True => true,
                                        JsonValueKind.False => false,
                                        JsonValueKind.Null => null!,
                                        _ => prop.Value.GetRawText()
                                    };
                                }
                                tableTemplate.Data.Add(rowDict);
                            }
                        }

                        template.Tables.Add(tableTemplate);
                    }
                }

                if (template == null || template.Tables == null)
                {
                    response.Success = false;
                    response.Message = "Invalid template format";
                    return response;
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
                    catch (Exception ex)
                    {
                        tableResult.Success = false;
                        tableResult.ErrorMessage = ex.Message;
                    }

                    response.TableResults.Add(tableResult);
                }

                response.Message = $"Seeded {response.TablesSeeded} table(s), inserted {response.RecordsInserted} record(s), skipped {response.RecordsSkipped} record(s)";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error seeding from template: {ex.Message}";
            }

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

