using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
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
    /// Service for seeding comprehensive demo data for testing
    /// Includes reference data, sample entities, production data, and relationships
    /// </summary>
    public class DemoDataSeeder
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMReferenceDataSeeder _referenceDataSeeder;
        private readonly ILogger<DemoDataSeeder>? _logger;
        private readonly string _connectionName;

        public DemoDataSeeder(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMReferenceDataSeeder referenceDataSeeder,
            string connectionName,
            ILogger<DemoDataSeeder>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _referenceDataSeeder = referenceDataSeeder ?? throw new ArgumentNullException(nameof(referenceDataSeeder));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

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
}

