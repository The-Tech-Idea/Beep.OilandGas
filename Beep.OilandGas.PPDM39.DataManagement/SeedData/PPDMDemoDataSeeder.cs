using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Unified service for seeding comprehensive demo/test data
    /// Consolidates demo data seeding, CSV orchestration, catalog, validation, and template generation
    /// </summary>
    public class PPDMDemoDataSeeder
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMReferenceDataSeeder _referenceDataSeeder;
        private readonly PPDMCSVSeeder _csvSeeder;
        private readonly ILogger<PPDMDemoDataSeeder>? _logger;
        private readonly string _connectionName;

        public PPDMDemoDataSeeder(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMReferenceDataSeeder referenceDataSeeder,
            PPDMCSVSeeder csvSeeder,
            string connectionName,
            ILogger<PPDMDemoDataSeeder>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _referenceDataSeeder = referenceDataSeeder ?? throw new ArgumentNullException(nameof(referenceDataSeeder));
            _csvSeeder = csvSeeder ?? throw new ArgumentNullException(nameof(csvSeeder));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        #region Demo Data Seeding

        /// <summary>
        /// Seed full demo dataset
        /// </summary>
        public async Task<SeedDataResult> SeedFullDemoDatasetAsync(string userId = "SYSTEM")
        {
            var result = new SeedDataResult
            {
                Success = true,
                Message = "Demo dataset seeding completed",
                TablesSeeded = 0,
                RecordsInserted = 0
            };

            try
            {
                _logger?.LogInformation("Starting full demo dataset seeding for connection {ConnectionName}", _connectionName);

                // Step 1: Seed reference data
                _logger?.LogInformation("Seeding reference data...");
                var referenceResult = await _referenceDataSeeder.SeedPPDMReferenceTablesAsync(
                    _connectionName, null, true, userId);
                
                if (!referenceResult.Success)
                {
                    _logger?.LogWarning("Reference data seeding had issues: {Message}", referenceResult.Message);
                }
                else
                {
                    result.TablesSeeded += referenceResult.TablesSeeded;
                    result.RecordsInserted += referenceResult.RecordsInserted;
                }

                // Step 2: Seed sample fields
                _logger?.LogInformation("Seeding sample fields...");
                await SeedSampleFieldsAsync(userId);

                // Step 3: Seed sample wells
                _logger?.LogInformation("Seeding sample wells...");
                await SeedSampleWellsAsync(userId);

                // Step 4: Seed sample facilities
                _logger?.LogInformation("Seeding sample facilities...");
                await SeedSampleFacilitiesAsync(userId);

                // Step 5: Seed sample production data
                _logger?.LogInformation("Seeding sample production data...");
                await SeedSampleProductionDataAsync(userId);

                _logger?.LogInformation("Full demo dataset seeding completed successfully");
                result.Message = $"Demo dataset seeded: {result.TablesSeeded} tables, {result.RecordsInserted} records";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error seeding full demo dataset");
                result.Success = false;
                result.Message = $"Failed to seed demo dataset: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Seed sample fields
        /// </summary>
        private async Task SeedSampleFieldsAsync(string userId)
        {
            try
            {
                var fieldRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FIELD), _connectionName, "FIELD");

                var sampleFields = new List<FIELD>
                {
                    new FIELD
                    {
                        FIELD_ID = "DEMO_FIELD_001",
                        FIELD_NAME = "Demo Field Alpha",
                        REMARK = "Sample field for demonstration purposes"
                    },
                    new FIELD
                    {
                        FIELD_ID = "DEMO_FIELD_002",
                        FIELD_NAME = "Demo Field Beta",
                        REMARK = "Another sample field for testing"
                    }
                };

                foreach (var field in sampleFields)
                {
                    if (field is IPPDMEntity ppdmField)
                        _commonColumnHandler.PrepareForInsert(ppdmField, userId);
                    await fieldRepo.InsertAsync(field, userId);
                }

                _logger?.LogInformation("Seeded {Count} sample fields", sampleFields.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error seeding sample fields");
                throw;
            }
        }

        /// <summary>
        /// Seed sample wells
        /// </summary>
        private async Task SeedSampleWellsAsync(string userId)
        {
            try
            {
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), _connectionName, "WELL");

                var sampleWells = new List<WELL>
                {
                    new WELL
                    {
                        UWI = "DEMO_WELL_001",
                        WELL_NAME = "Demo Well Alpha-1",
                        ASSIGNED_FIELD = "DEMO_FIELD_001",
                        REMARK = "Sample production well"
                    },
                    new WELL
                    {
                        UWI = "DEMO_WELL_002",
                        WELL_NAME = "Demo Well Beta-1",
                        ASSIGNED_FIELD = "DEMO_FIELD_002",
                        REMARK = "Sample exploration well"
                    }
                };

                foreach (var well in sampleWells)
                {
                    if (well is IPPDMEntity ppdmWell)
                        _commonColumnHandler.PrepareForInsert(ppdmWell, userId);
                    await wellRepo.InsertAsync(well, userId);
                }

                _logger?.LogInformation("Seeded {Count} sample wells", sampleWells.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error seeding sample wells");
                throw;
            }
        }

        /// <summary>
        /// Seed sample facilities
        /// </summary>
        private async Task SeedSampleFacilitiesAsync(string userId)
        {
            try
            {
                var facilityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY), _connectionName, "FACILITY");

                var sampleFacilities = new List<FACILITY>
                {
                    new FACILITY
                    {
                        FACILITY_ID = "DEMO_FACILITY_001",
                        DESCRIPTION = "Demo Processing Facility Alpha",
                        PRIMARY_FIELD_ID = "DEMO_FIELD_001",
                        REMARK = "Sample processing facility"
                    }
                };

                foreach (var facility in sampleFacilities)
                {
                    if (facility is IPPDMEntity ppdmFacility)
                        _commonColumnHandler.PrepareForInsert(ppdmFacility, userId);
                    await facilityRepo.InsertAsync(facility, userId);
                }

                _logger?.LogInformation("Seeded {Count} sample facilities", sampleFacilities.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error seeding sample facilities");
                throw;
            }
        }

        /// <summary>
        /// Seed sample production data
        /// </summary>
        private async Task SeedSampleProductionDataAsync(string userId)
        {
            try
            {
                // Seed sample production data would go here
                // This is a placeholder for actual production data seeding
                _logger?.LogInformation("Sample production data seeding - implementation needed");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error seeding sample production data");
                throw;
            }
        }

        #endregion

        #region CSV Seeding

        /// <summary>
        /// Seeds all CSV files from a folder
        /// </summary>
        public async Task<SeedingSummary> SeedAllFromCSVAsync(string csvFolder, string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(csvFolder))
                throw new ArgumentException("CSV folder cannot be null or empty", nameof(csvFolder));

            if (!Directory.Exists(csvFolder))
                throw new DirectoryNotFoundException($"CSV folder not found: {csvFolder}");

            var csvFiles = Directory.GetFiles(csvFolder, "*.csv", SearchOption.TopDirectoryOnly);
            
            var summary = new SeedingSummary
            {
                TotalFiles = csvFiles.Length,
                ProcessedFiles = 0,
                SkippedFiles = 0,
                TotalRecordsSeeded = 0,
                Errors = new List<string>()
            };

            _logger?.LogInformation("Found {Count} CSV files to process", csvFiles.Length);

            foreach (var csvFile in csvFiles)
            {
                var fileName = Path.GetFileName(csvFile);
                _logger?.LogInformation("Processing: {FileName}", fileName);

                try
                {
                    var recordsSeeded = await _csvSeeder.SeedAsync(csvFile, userId);
                    summary.TotalRecordsSeeded += recordsSeeded;
                    summary.ProcessedFiles++;
                    _logger?.LogInformation("Seeded {Count} records from {FileName}", recordsSeeded, fileName);
                }
                catch (Exception ex)
                {
                    summary.SkippedFiles++;
                    var errorMsg = $"Error processing {fileName}: {ex.Message}";
                    summary.Errors.Add(errorMsg);
                    _logger?.LogError(ex, "Error processing CSV file {FileName}", fileName);
                }
            }

            return summary;
        }

        /// <summary>
        /// Seeds a specific CSV file
        /// </summary>
        public async Task<int> SeedFileFromCSVAsync(string csvFilePath, string userId = "SYSTEM")
        {
            return await _csvSeeder.SeedAsync(csvFilePath, userId);
        }

        /// <summary>
        /// Seeds well status facets (R_WELL_STATUS) from multiple CSV files
        /// Maps CSV files to STATUS_TYPE values
        /// </summary>
        public async Task<int> SeedWellStatusFacetsFromCSVAsync(string csvFolder, string userId = "SYSTEM")
        {
            var statusTypeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Life_Cycle", "Business Life Cycle Phase" },
                { "Role", "Role" },
                { "Business_Interest", "Business Interest" },
                { "Business_Intention", "Business Intention" },
                { "Outcome", "Outcome" },
                { "Lahee_Class", "Lahee Class" },
                { "Play_Type", "Play Type" },
                { "Well_Structure", "Well Structure" },
                { "Trajectory_Type", "Trajectory Type" },
                { "Fluid_Direction", "Fluid Direction" },
                { "Well_Reporting_Class", "Well Reporting Class" },
                { "Fluid_Type", "Fluid Type" },
                { "Wellbore_Status", "Wellbore Status" },
                { "Well_Status", "Well Status" },
                { "Operatorship", "Operatorship" }
            };

            var csvFiles = Directory.GetFiles(csvFolder, "*.csv", SearchOption.TopDirectoryOnly);
            int totalSeeded = 0;

            foreach (var csvFile in csvFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(csvFile);
                
                // Check if this is a well status facet file
                var statusType = statusTypeMapping.FirstOrDefault(kvp => 
                    fileName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)).Value;
                
                if (statusType != null)
                {
                    _logger?.LogInformation("Seeding well status facet: {StatusType} from {FileName}", statusType, Path.GetFileName(csvFile));
                    
                    try
                    {
                        var recordsSeeded = await SeedWellStatusFromCSVAsync(csvFile, statusType, userId);
                        totalSeeded += recordsSeeded;
                        _logger?.LogInformation("Seeded {Count} records for {StatusType}", recordsSeeded, statusType);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error seeding well status facet {StatusType}", statusType);
                    }
                }
            }

            return totalSeeded;
        }

        /// <summary>
        /// Seeds R_WELL_STATUS from CSV with specific STATUS_TYPE
        /// </summary>
        private async Task<int> SeedWellStatusFromCSVAsync(string csvFilePath, string statusType, string userId)
        {
            var csvLines = File.ReadAllLines(csvFilePath);
            if (csvLines.Length < 2)
                return 0;

            var headerRow = ParseCSVLine(csvLines[0]);
            var name1Index = headerRow.FindIndex(h => h.Trim('"').Equals("NAME1", StringComparison.OrdinalIgnoreCase));
            var definitionIndex = headerRow.FindIndex(h => h.Trim('"').Equals("DEFINITION", StringComparison.OrdinalIgnoreCase));
            var valueStatusIndex = headerRow.FindIndex(h => h.Trim('"').Equals("VALUE_STATUS", StringComparison.OrdinalIgnoreCase));
            var sourceIndex = headerRow.FindIndex(h => h.Trim('"').Equals("SOURCE", StringComparison.OrdinalIgnoreCase));
            var resourceIndex = headerRow.FindIndex(h => h.Trim('"').Equals("RESOURCE", StringComparison.OrdinalIgnoreCase));

            if (name1Index == -1)
                return 0;

            var entityType = typeof(Beep.OilandGas.PPDM39.Models.R_WELL_STATUS);
            var repository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "R_WELL_STATUS");

            int seededCount = 0;

            for (int rowIndex = 1; rowIndex < csvLines.Length; rowIndex++)
            {
                var row = csvLines[rowIndex];
                
                if (IsEmptyRow(row))
                    continue;

                try
                {
                    var values = ParseCSVLine(row);
                    
                    if (values.All(v => string.IsNullOrWhiteSpace(v)) || values.Count <= name1Index)
                        continue;

                    var status = values[name1Index].Trim();
                    if (string.IsNullOrWhiteSpace(status))
                        continue;

                    var entity = Activator.CreateInstance(entityType);
                    var entityTypeInfo = entityType;

                    entityTypeInfo.GetProperty("STATUS_TYPE")?.SetValue(entity, statusType);
                    entityTypeInfo.GetProperty("STATUS")?.SetValue(entity, status);
                    
                    var statusIdProp = entityTypeInfo.GetProperty("STATUS_ID");
                    if (statusIdProp != null)
                    {
                        statusIdProp.SetValue(entity, $"{statusType},{status}");
                    }
                    
                    entityTypeInfo.GetProperty("LONG_NAME")?.SetValue(entity, status);
                    var shortName = status.Length > 20 ? status.Substring(0, 20) : status;
                    entityTypeInfo.GetProperty("SHORT_NAME")?.SetValue(entity, shortName);
                    
                    if (definitionIndex >= 0 && definitionIndex < values.Count)
                    {
                        entityTypeInfo.GetProperty("DESCRIPTION")?.SetValue(entity, values[definitionIndex].Trim());
                    }
                    
                    if (valueStatusIndex >= 0 && valueStatusIndex < values.Count)
                    {
                        var valueStatus = values[valueStatusIndex].Trim();
                        if (!string.IsNullOrWhiteSpace(valueStatus))
                        {
                            entityTypeInfo.GetProperty("VALUE_STATUS")?.SetValue(entity, valueStatus);
                        }
                    }
                    
                    if (sourceIndex >= 0 && sourceIndex < values.Count)
                    {
                        var source = values[sourceIndex].Trim();
                        if (!string.IsNullOrWhiteSpace(source))
                        {
                            entityTypeInfo.GetProperty("SOURCE")?.SetValue(entity, source);
                        }
                    }
                    
                    if (resourceIndex >= 0 && resourceIndex < values.Count)
                    {
                        var resource = values[resourceIndex].Trim();
                        if (!string.IsNullOrWhiteSpace(resource))
                        {
                            entityTypeInfo.GetProperty("RESOURCE")?.SetValue(entity, resource);
                        }
                    }
                    
                    entityTypeInfo.GetProperty("ACTIVE_IND")?.SetValue(entity, "Y");
                    entityTypeInfo.GetProperty("PPDM_GUID")?.SetValue(entity, Guid.NewGuid().ToString().ToUpper());
                    entityTypeInfo.GetProperty("EFFECTIVE_DATE")?.SetValue(entity, DateTime.Now);
                    entityTypeInfo.GetProperty("EXPIRY_DATE")?.SetValue(entity, DateTime.MinValue);
                    
                    var result = await repository.InsertAsync(entity, userId);
                    
                    if (result != null)
                    {
                        seededCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error seeding row {RowIndex} in {FileName}", rowIndex + 1, Path.GetFileName(csvFilePath));
                }
            }

            return seededCount;
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
                    if (i + 1 < chars.Length && chars[i + 1] == '"')
                    {
                        currentValue += '"';
                        i++;
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
            values.Add(currentValue.Trim('"'));
            
            return values;
        }

        /// <summary>
        /// Checks if a CSV row is empty (only commas, whitespace, or no data)
        /// </summary>
        private static bool IsEmptyRow(string row)
        {
            if (string.IsNullOrWhiteSpace(row))
                return true;
            
            var cleaned = row.Replace(",", "").Trim();
            return string.IsNullOrWhiteSpace(cleaned);
        }

        #endregion

        #region Catalog and Metadata

        /// <summary>
        /// Gets all available seed data categories
        /// </summary>
        public List<SeedDataCategory> GetSeedDataCategories()
        {
            var categories = new List<SeedDataCategory>();

            // PPDM Reference Tables
            categories.Add(new SeedDataCategory
            {
                CategoryName = "PPDM",
                Description = "PPDM standard reference tables (R_* tables)",
                TableNames = GetPPDMReferenceTables(),
                EstimatedRecords = 500
            });

            // Accounting
            var accountingTables = GetAccountingWorkflowRequirements()
                .SelectMany(r => r.RequiredTables)
                .Distinct()
                .ToList();
            categories.Add(new SeedDataCategory
            {
                CategoryName = "Accounting",
                Description = "Seed data for ProductionAccounting workflows",
                TableNames = accountingTables,
                EstimatedRecords = 200
            });

            // LifeCycle
            var lifecycleTables = GetLifeCycleWorkflowRequirements()
                .SelectMany(r => r.RequiredTables)
                .Distinct()
                .ToList();
            categories.Add(new SeedDataCategory
            {
                CategoryName = "LifeCycle",
                Description = "Seed data for LifeCycle workflows",
                TableNames = lifecycleTables,
                EstimatedRecords = 150
            });

            // Analysis
            var analysisTables = GetAnalysisWorkflowRequirements()
                .SelectMany(r => r.RequiredTables)
                .Distinct()
                .ToList();
            categories.Add(new SeedDataCategory
            {
                CategoryName = "Analysis",
                Description = "Seed data for analysis modules",
                TableNames = analysisTables,
                EstimatedRecords = 50
            });

            return categories;
        }

        /// <summary>
        /// Gets seed data requirements for a specific category
        /// </summary>
        public SeedDataCategory? GetSeedDataForCategory(string category)
        {
            return GetSeedDataCategories()
                .FirstOrDefault(c => c.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets seed data requirements for a specific workflow
        /// </summary>
        public WorkflowSeedDataRequirement? GetSeedDataForWorkflow(string workflowName)
        {
            var allRequirements = new List<WorkflowSeedDataRequirement>();
            allRequirements.AddRange(GetAccountingWorkflowRequirements());
            allRequirements.AddRange(GetLifeCycleWorkflowRequirements());
            allRequirements.AddRange(GetAnalysisWorkflowRequirements());

            return allRequirements.FirstOrDefault(r => 
                r.WorkflowName.Equals(workflowName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all workflows that require a specific table
        /// </summary>
        public List<WorkflowSeedDataRequirement> GetWorkflowsRequiringTable(string tableName)
        {
            var allRequirements = new List<WorkflowSeedDataRequirement>();
            allRequirements.AddRange(GetAccountingWorkflowRequirements());
            allRequirements.AddRange(GetLifeCycleWorkflowRequirements());
            allRequirements.AddRange(GetAnalysisWorkflowRequirements());

            return allRequirements
                .Where(r => r.RequiredTables.Any(t => t.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        /// <summary>
        /// Gets standard PPDM reference tables (R_* tables)
        /// </summary>
        private List<string> GetPPDMReferenceTables()
        {
            return new List<string>
            {
                "R_WELL_STATUS",
                "R_FIELD_STATUS",
                "R_FACILITY_STATUS",
                "R_PIPELINE_STATUS",
                "R_PROPERTY_STATUS",
                "R_COST_TYPE",
                "R_ACCOUNTING_METHOD",
                "R_UNIT_OF_MEASURE",
                "R_ROW_QUALITY",
                "R_SOURCE",
                "R_WORK_ORDER_TYPE",
                "R_WORK_ORDER_STATUS",
                "R_MAINTENANCE_TYPE",
                "R_INSPECTION_TYPE",
                "R_INVOICE_TYPE",
                "R_PAYMENT_METHOD",
                "R_GL_ACCOUNT_TYPE",
                "R_PRODUCT_TYPE",
                "R_LEASE_STATUS",
                "R_LEASE_TYPE",
                "R_OWNERSHIP_TYPE",
                "R_REVENUE_TYPE",
                "R_TAX_TYPE",
                "R_ROYALTY_TYPE"
            };
        }

        /// <summary>
        /// Gets required seed data for ProductionAccounting workflows
        /// </summary>
        public List<WorkflowSeedDataRequirement> GetAccountingWorkflowRequirements()
        {
            return new List<WorkflowSeedDataRequirement>
            {
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "AFE Management",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "AFE_STATUS", "COST_TYPE", "COST_CATEGORY", "COST_CENTER" },
                    Description = "Authorization for Expenditure management requires status, cost types, categories, and centers"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Cost Transactions",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "COST_TYPE", "COST_CATEGORY", "COST_CENTER", "COST_BASIS" },
                    Description = "Cost transaction processing requires cost classifications"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Revenue Allocation",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "REVENUE_TYPE", "ALLOCATION_METHOD", "PRODUCT_TYPE" },
                    Description = "Revenue allocation requires revenue types, allocation methods, and product types"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Journal Entries",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "GL_ACCOUNT", "ENTRY_TYPE", "ENTRY_STATUS" },
                    Description = "Journal entry processing requires GL accounts, entry types, and statuses"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "General Ledger",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "GL_ACCOUNT", "ACCOUNT_TYPE", "ACCOUNT_CATEGORY" },
                    Description = "General ledger requires chart of accounts with account types and categories"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Invoice Management",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "INVOICE_TYPE", "PAYMENT_TERMS", "TAX_RATE", "INVOICE_STATUS" },
                    Description = "Invoice management requires invoice types, payment terms, tax rates, and statuses"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Purchase Orders",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "PO_STATUS", "VENDOR_STATUS", "PO_TYPE" },
                    Description = "Purchase order management requires PO statuses, vendor statuses, and PO types"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Accounts Payable",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "PAYMENT_METHOD", "PAYMENT_STATUS", "VENDOR_TYPE" },
                    Description = "Accounts payable requires payment methods, payment statuses, and vendor types"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Accounts Receivable",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "PAYMENT_METHOD", "INVOICE_STATUS", "CUSTOMER_TYPE" },
                    Description = "Accounts receivable requires payment methods, invoice statuses, and customer types"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Royalty Management",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "ROYALTY_TYPE", "ROYALTY_CALCULATION_METHOD", "ROYALTY_STATUS" },
                    Description = "Royalty management requires royalty types, calculation methods, and statuses"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Inventory",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "INVENTORY_TYPE", "UNIT_OF_MEASURE", "INVENTORY_STATUS" },
                    Description = "Inventory management requires inventory types, units of measure, and statuses"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Tax Management",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "TAX_TYPE", "TAX_RATE", "TAX_JURISDICTION" },
                    Description = "Tax management requires tax types, rates, and jurisdictions"
                }
            };
        }

        /// <summary>
        /// Gets required seed data for LifeCycle workflows
        /// </summary>
        public List<WorkflowSeedDataRequirement> GetLifeCycleWorkflowRequirements()
        {
            return new List<WorkflowSeedDataRequirement>
            {
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Work Orders",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "WORK_ORDER_TYPE", "WORK_ORDER_STATUS", "PRIORITY", "WORK_ORDER_CATEGORY" },
                    Description = "Work order management requires types, statuses, priorities, and categories"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Maintenance",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "MAINTENANCE_TYPE", "MAINTENANCE_STATUS", "EQUIPMENT_TYPE", "MAINTENANCE_PRIORITY" },
                    Description = "Maintenance management requires types, statuses, equipment types, and priorities"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Inspections",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "INSPECTION_TYPE", "INSPECTION_STATUS", "INSPECTION_RESULT", "INSPECTION_FREQUENCY" },
                    Description = "Inspection management requires types, statuses, results, and frequencies"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Operations",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "OPERATION_TYPE", "SHIFT_TYPE", "INCIDENT_TYPE", "OPERATION_STATUS" },
                    Description = "Operations management requires types, shift types, incident types, and statuses"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Field Management",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "FIELD_STATUS", "FIELD_TYPE", "FIELD_CLASSIFICATION" },
                    Description = "Field management requires statuses, types, and classifications"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Well Management",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "WELL_STATUS", "WELL_TYPE", "COMPLETION_TYPE", "WELL_CLASSIFICATION" },
                    Description = "Well management requires statuses, types, completion types, and classifications"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Pipeline Management",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "PIPELINE_STATUS", "PIPELINE_TYPE", "MATERIAL_TYPE", "PIPELINE_CLASSIFICATION" },
                    Description = "Pipeline management requires statuses, types, material types, and classifications"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Facility Management",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "FACILITY_STATUS", "FACILITY_TYPE", "FACILITY_CLASSIFICATION" },
                    Description = "Facility management requires statuses, types, and classifications"
                }
            };
        }

        /// <summary>
        /// Gets required seed data for analysis workflows
        /// </summary>
        public List<WorkflowSeedDataRequirement> GetAnalysisWorkflowRequirements()
        {
            return new List<WorkflowSeedDataRequirement>
            {
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Analysis Results",
                    WorkflowCategory = "Analysis",
                    RequiredTables = new List<string> { "ANALYSIS_STATUS", "ANALYSIS_TYPE", "EQUIPMENT_TYPE" },
                    Description = "Analysis result management requires statuses, types, and equipment types"
                }
            };
        }

        /// <summary>
        /// Gets all unique table names required across all workflows
        /// </summary>
        public List<string> GetAllRequiredTables()
        {
            var allRequirements = new List<WorkflowSeedDataRequirement>();
            allRequirements.AddRange(GetAccountingWorkflowRequirements());
            allRequirements.AddRange(GetLifeCycleWorkflowRequirements());
            allRequirements.AddRange(GetAnalysisWorkflowRequirements());

            return allRequirements
                .SelectMany(r => r.RequiredTables)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates seed data before insertion
        /// </summary>
        public async Task<SeedDataValidationResult> ValidateSeedDataAsync(string tableName, List<Dictionary<string, object>> seedData)
        {
            var result = new SeedDataValidationResult
            {
                TableName = tableName,
                IsValid = true,
                Errors = new List<string>()
            };

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync(tableName);
                if (metadata == null)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Table metadata not found for: {tableName}");
                    return result;
                }

                // Validate each row
                for (int i = 0; i < seedData.Count; i++)
                {
                    var row = seedData[i];
                    var rowErrors = await ValidateRowAsync(metadata, row, i);
                    result.Errors.AddRange(rowErrors);
                }

                result.IsValid = result.Errors.Count == 0;
                result.ValidRows = seedData.Count - result.Errors.Count;
                result.InvalidRows = result.Errors.Count;

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Error validating seed data: {ex.Message}");
                return result;
            }
        }

        private async Task<List<string>> ValidateRowAsync(PPDMTableMetadata metadata, Dictionary<string, object> row, int rowIndex)
        {
            var errors = new List<string>();

            // Check required columns (primary key is always required)
            if (!string.IsNullOrEmpty(metadata.PrimaryKeyColumn) && !row.ContainsKey(metadata.PrimaryKeyColumn))
            {
                errors.Add($"Row {rowIndex + 1}: Required primary key column '{metadata.PrimaryKeyColumn}' is missing");
            }

            return errors;
        }

        #endregion

        #region Template Generation

        /// <summary>
        /// Generates seed data template for a table
        /// </summary>
        public async Task<List<Dictionary<string, object>>> GenerateSeedDataTemplateAsync(string tableName, int numberOfRows = 1)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
            {
                throw new InvalidOperationException($"Table metadata not found for: {tableName}");
            }

            var template = new List<Dictionary<string, object>>();

            for (int i = 0; i < numberOfRows; i++)
            {
                var row = new Dictionary<string, object>();
                
                // Generate default values for common columns
                row["ACTIVE_IND"] = "Y";
                row["PPDM_GUID"] = Guid.NewGuid().ToString().ToUpper();

                template.Add(row);
            }

            return template;
        }

        /// <summary>
        /// Generates seed data template from entity type
        /// </summary>
        public async Task<List<Dictionary<string, object>>> GenerateSeedDataTemplateFromEntityAsync(Type entityType, int numberOfRows = 1)
        {
            var tableName = entityType.Name;
            return await GenerateSeedDataTemplateAsync(tableName, numberOfRows);
        }

        #endregion
    }

    /// <summary>
    /// Seed data result
    /// </summary>
    public class SeedDataResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TablesSeeded { get; set; }
        public int RecordsInserted { get; set; }
    }

    /// <summary>
    /// Summary of seeding operation
    /// </summary>
    public class SeedingSummary
    {
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
        public int SkippedFiles { get; set; }
        public int TotalRecordsSeeded { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Validation result
    /// </summary>
    public class SeedDataValidationResult
    {
        public string TableName { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
    }
}
