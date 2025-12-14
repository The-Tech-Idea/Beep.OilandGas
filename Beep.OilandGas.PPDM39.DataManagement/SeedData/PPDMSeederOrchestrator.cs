using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.SeedData;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Orchestrator for seeding all PPDM39 tables from CSV files
    /// Processes all CSV files in a folder and seeds the corresponding tables
    /// </summary>
    public class PPDMSeederOrchestrator
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly PPDMCSVSeeder _csvSeeder;

        public PPDMSeederOrchestrator(
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
            
            _csvSeeder = new PPDMCSVSeeder(editor, commonColumnHandler, defaults, metadata, connectionName);
        }

        /// <summary>
        /// Seeds all CSV files from a folder
        /// </summary>
        /// <param name="csvFolder">Folder containing CSV files</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <returns>Summary of seeding results</returns>
        public async Task<SeedingSummary> SeedAllAsync(string csvFolder, string userId = "SYSTEM")
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

            Console.WriteLine($"Found {csvFiles.Length} CSV files to process");
            Console.WriteLine("");

            foreach (var csvFile in csvFiles)
            {
                var fileName = Path.GetFileName(csvFile);
                Console.WriteLine($"Processing: {fileName}");

                try
                {
                    var recordsSeeded = await _csvSeeder.SeedAsync(csvFile, userId);
                    summary.TotalRecordsSeeded += recordsSeeded;
                    summary.ProcessedFiles++;
                    Console.WriteLine($"  ✓ Seeded {recordsSeeded} records");
                }
                catch (Exception ex)
                {
                    summary.SkippedFiles++;
                    var errorMsg = $"Error processing {fileName}: {ex.Message}";
                    summary.Errors.Add(errorMsg);
                    Console.WriteLine($"  ✗ {errorMsg}");
                }

                Console.WriteLine("");
            }

            return summary;
        }

        /// <summary>
        /// Seeds a specific CSV file
        /// </summary>
        public async Task<int> SeedFileAsync(string csvFilePath, string userId = "SYSTEM")
        {
            return await _csvSeeder.SeedAsync(csvFilePath, userId);
        }

        /// <summary>
        /// Seeds well status facets (R_WELL_STATUS) from multiple CSV files
        /// Maps CSV files to STATUS_TYPE values
        /// </summary>
        public async Task<int> SeedWellStatusFacetsAsync(string csvFolder, string userId = "SYSTEM")
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
                    Console.WriteLine($"Seeding well status facet: {statusType} from {Path.GetFileName(csvFile)}");
                    
                    try
                    {
                        // For well status, we need to set STATUS_TYPE before seeding
                        var recordsSeeded = await SeedWellStatusFromCSVAsync(csvFile, statusType, userId);
                        totalSeeded += recordsSeeded;
                        Console.WriteLine($"  ✓ Seeded {recordsSeeded} records");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ✗ Error: {ex.Message}");
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
            // Read CSV file (already cleaned - no logo, disclaimer, copyright rows)
            // Row 1 (index 0) is the header row
            // Row 2+ (index 1+) are data rows
            var csvLines = File.ReadAllLines(csvFilePath);
            if (csvLines.Length < 2)
                return 0;

            // Get header row (row 1, index 0)
            var headerRow = ParseCSVLine(csvLines[0]);
            var name1Index = headerRow.FindIndex(h => h.Trim('"').Equals("NAME1", StringComparison.OrdinalIgnoreCase));
            var definitionIndex = headerRow.FindIndex(h => h.Trim('"').Equals("DEFINITION", StringComparison.OrdinalIgnoreCase));
            var valueStatusIndex = headerRow.FindIndex(h => h.Trim('"').Equals("VALUE_STATUS", StringComparison.OrdinalIgnoreCase));
            var sourceIndex = headerRow.FindIndex(h => h.Trim('"').Equals("SOURCE", StringComparison.OrdinalIgnoreCase));
            var resourceIndex = headerRow.FindIndex(h => h.Trim('"').Equals("RESOURCE", StringComparison.OrdinalIgnoreCase));

            if (name1Index == -1)
                return 0;

            // Get R_WELL_STATUS entity type
            var entityType = typeof(Beep.OilandGas.PPDM39.Models.R_WELL_STATUS);
            var repository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "R_WELL_STATUS");

            int seededCount = 0;

            // Process data rows (starting from row 2, index 1)
            for (int rowIndex = 1; rowIndex < csvLines.Length; rowIndex++)
            {
                var row = csvLines[rowIndex];
                
                // Skip empty rows (rows with only commas, whitespace, or no data)
                if (IsEmptyRow(row))
                    continue;

                try
                {
                    var values = ParseCSVLine(row);
                    
                    // Skip if all values are empty
                    if (values.All(v => string.IsNullOrWhiteSpace(v)))
                        continue;
                    
                    if (values.Count <= name1Index)
                        continue;

                    var status = values[name1Index].Trim();
                    if (string.IsNullOrWhiteSpace(status))
                        continue;

                    // Create R_WELL_STATUS entity
                    var entity = Activator.CreateInstance(entityType);
                    var entityTypeInfo = entityType;

                    // Set STATUS_TYPE
                    entityTypeInfo.GetProperty("STATUS_TYPE")?.SetValue(entity, statusType);
                    
                    // Set STATUS
                    entityTypeInfo.GetProperty("STATUS")?.SetValue(entity, status);
                    
                    // Note: R_WELL_STATUS may use composite key (STATUS_TYPE, STATUS) instead of STATUS_ID
                    // Check if STATUS_ID property exists, if not, the table uses composite key
                    var statusIdProp = entityTypeInfo.GetProperty("STATUS_ID");
                    if (statusIdProp != null)
                    {
                        statusIdProp.SetValue(entity, $"{statusType},{status}");
                    }
                    
                    // Set LONG_NAME
                    entityTypeInfo.GetProperty("LONG_NAME")?.SetValue(entity, status);
                    
                    // Set SHORT_NAME (truncate if needed)
                    var shortName = status.Length > 20 ? status.Substring(0, 20) : status;
                    entityTypeInfo.GetProperty("SHORT_NAME")?.SetValue(entity, shortName);
                    
                    // Set DESCRIPTION
                    if (definitionIndex >= 0 && definitionIndex < values.Count)
                    {
                        entityTypeInfo.GetProperty("DESCRIPTION")?.SetValue(entity, values[definitionIndex].Trim());
                    }
                    
                    // Set VALUE_STATUS
                    if (valueStatusIndex >= 0 && valueStatusIndex < values.Count)
                    {
                        var valueStatus = values[valueStatusIndex].Trim();
                        if (!string.IsNullOrWhiteSpace(valueStatus))
                        {
                            entityTypeInfo.GetProperty("VALUE_STATUS")?.SetValue(entity, valueStatus);
                        }
                    }
                    
                    // Set SOURCE
                    if (sourceIndex >= 0 && sourceIndex < values.Count)
                    {
                        var source = values[sourceIndex].Trim();
                        if (!string.IsNullOrWhiteSpace(source))
                        {
                            entityTypeInfo.GetProperty("SOURCE")?.SetValue(entity, source);
                        }
                    }
                    
                    // Set RESOURCE
                    if (resourceIndex >= 0 && resourceIndex < values.Count)
                    {
                        var resource = values[resourceIndex].Trim();
                        if (!string.IsNullOrWhiteSpace(resource))
                        {
                            entityTypeInfo.GetProperty("RESOURCE")?.SetValue(entity, resource);
                        }
                    }
                    
                    // Set ACTIVE_IND
                    entityTypeInfo.GetProperty("ACTIVE_IND")?.SetValue(entity, "Y");
                    
                    // Set PPDM_GUID
                    entityTypeInfo.GetProperty("PPDM_GUID")?.SetValue(entity, Guid.NewGuid().ToString().ToUpper());
                    
                    // Set dates
                    entityTypeInfo.GetProperty("EFFECTIVE_DATE")?.SetValue(entity, DateTime.Now);
                    entityTypeInfo.GetProperty("EXPIRY_DATE")?.SetValue(entity, DateTime.MinValue);
                    
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
                    Console.WriteLine($"    Error seeding row {rowIndex + 1}: {ex.Message}");
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
            
            // Remove all commas and whitespace, check if anything remains
            var cleaned = row.Replace(",", "").Trim();
            return string.IsNullOrWhiteSpace(cleaned);
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
    }
}

