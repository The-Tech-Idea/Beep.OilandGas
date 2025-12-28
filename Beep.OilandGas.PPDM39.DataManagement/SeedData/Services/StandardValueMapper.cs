using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Maps values from different standards (PPDM, IHS, API, ISO) to PPDM structure
    /// Handles value conflicts and cross-references between standards
    /// </summary>
    public class StandardValueMapper
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;

        // Mapping dictionaries for cross-reference
        private readonly Dictionary<string, Dictionary<string, string>> _ihsToPPDMMappings = new Dictionary<string, Dictionary<string, string>>();
        private readonly Dictionary<string, Dictionary<string, string>> _apiToPPDMMappings = new Dictionary<string, Dictionary<string, string>>();
        private readonly Dictionary<string, Dictionary<string, string>> _isoToPPDMMappings = new Dictionary<string, Dictionary<string, string>>();

        public StandardValueMapper(
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
            InitializeMappings();
        }

        /// <summary>
        /// Maps IHS code to PPDM table
        /// </summary>
        public async Task<ImportResult> MapIHSToPPDMAsync(string ihsCode, string ihsName, string ppdmTableName, bool skipExisting = true, string userId = "SYSTEM")
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
                // Check if mapping exists
                if (_ihsToPPDMMappings.ContainsKey(ppdmTableName) && 
                    _ihsToPPDMMappings[ppdmTableName].ContainsKey(ihsCode))
                {
                    var ppdmCode = _ihsToPPDMMappings[ppdmTableName][ihsCode];
                    // Use mapped PPDM code
                    return await InsertToPPDMTableAsync(ppdmTableName, ppdmCode, ihsName, skipExisting, userId);
                }

                // Try direct mapping (IHS code = PPDM code)
                return await InsertToPPDMTableAsync(ppdmTableName, ihsCode, ihsName, skipExisting, userId);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Mapping error: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Maps API code to PPDM table
        /// </summary>
        public async Task<ImportResult> MapAPIToPPDMAsync(string apiCode, string apiName, string ppdmTableName, bool skipExisting = true, string userId = "SYSTEM")
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
                // Check if mapping exists
                if (_apiToPPDMMappings.ContainsKey(ppdmTableName) && 
                    _apiToPPDMMappings[ppdmTableName].ContainsKey(apiCode))
                {
                    var ppdmCode = _apiToPPDMMappings[ppdmTableName][apiCode];
                    return await InsertToPPDMTableAsync(ppdmTableName, ppdmCode, apiName, skipExisting, userId);
                }

                return await InsertToPPDMTableAsync(ppdmTableName, apiCode, apiName, skipExisting, userId);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Mapping error: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Maps ISO code to PPDM table
        /// </summary>
        public async Task<ImportResult> MapISOToPPDMAsync(string isoCode, string isoName, string ppdmTableName, bool skipExisting = true, string userId = "SYSTEM")
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
                // Check if mapping exists
                if (_isoToPPDMMappings.ContainsKey(ppdmTableName) && 
                    _isoToPPDMMappings[ppdmTableName].ContainsKey(isoCode))
                {
                    var ppdmCode = _isoToPPDMMappings[ppdmTableName][isoCode];
                    return await InsertToPPDMTableAsync(ppdmTableName, ppdmCode, isoName, skipExisting, userId);
                }

                return await InsertToPPDMTableAsync(ppdmTableName, isoCode, isoName, skipExisting, userId);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Mapping error: {ex.Message}");
                return result;
            }
        }

        private async Task<ImportResult> InsertToPPDMTableAsync(string tableName, string code, string name, bool skipExisting, string userId)
        {
            var result = new ImportResult
            {
                Success = true,
                RecordsProcessed = 1,
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

                // Check if exists
                if (skipExisting)
                {
                    var primaryKeyColumn = metadata.PrimaryKeyColumn;
                    if (!string.IsNullOrEmpty(primaryKeyColumn))
                    {
                        var existing = await repository.GetByIdAsync(code);
                        if (existing != null)
                        {
                            result.RecordsSkipped = 1;
                            return result;
                        }
                    }
                }

                // Create entity
                var entity = Activator.CreateInstance(entityType);
                var primaryKeyProp = entityType.GetProperty(metadata.PrimaryKeyColumn, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (primaryKeyProp != null && primaryKeyProp.CanWrite)
                {
                    primaryKeyProp.SetValue(entity, code);
                }

                // Set name property (usually ends with _NAME or _DESC)
                var nameProp = entityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .FirstOrDefault(p => p.Name.EndsWith("_NAME", StringComparison.OrdinalIgnoreCase) || 
                                        p.Name.EndsWith("_DESC", StringComparison.OrdinalIgnoreCase));
                if (nameProp != null && nameProp.CanWrite)
                {
                    nameProp.SetValue(entity, name);
                }

                // Set common columns
                if (entity is IPPDMEntity ppdmEntity)
                    _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);

                // Insert
                var inserted = await repository.InsertAsync(entity, userId);
                if (inserted != null)
                {
                    result.RecordsInserted = 1;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error inserting to {tableName}: {ex.Message}");
            }

            return result;
        }

        private void InitializeMappings()
        {
            // Initialize IHS to PPDM mappings
            // Example: Well Status mappings
            _ihsToPPDMMappings["RA_WELL_STATUS"] = new Dictionary<string, string>
            {
                // Add IHS to PPDM mappings here
                // Example: { "IHS_CODE", "PPDM_CODE" }
            };

            // Initialize API to PPDM mappings
            _apiToPPDMMappings["RA_WELL_STATUS"] = new Dictionary<string, string>
            {
                // Add API to PPDM mappings here
            };

            // Initialize ISO to PPDM mappings
            _isoToPPDMMappings["RA_WELL_STATUS"] = new Dictionary<string, string>
            {
                // Add ISO to PPDM mappings here
            };
        }
    }
}

