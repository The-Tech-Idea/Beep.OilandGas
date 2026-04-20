using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData.DummyData
{
    /// <summary>
    /// Generates realistic PPDM39-schema demo data into an already-configured database connection.
    /// Three scale options: minimal / standard / full.
    /// </summary>
    public partial class PPDM39DummyDataGenerator
    {
        // ── Dependencies ──────────────────────────────────────────────────────────
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly WellServices _wellServices;
        private readonly string _connectionName;
        private readonly ILogger<PPDM39DummyDataGenerator>? _logger;

        // ── Scale parameters resolved per option ─────────────────────────────────
        private int _fieldCount;
        private int _wellsPerField;
        private int _productionMonths;
        private bool _includeWellTests;
        private bool _includeFacilities;
        private bool _includeActivities;

        // ── ID tracking ───────────────────────────────────────────────────────────
        /// <summary>Field IDs seeded during this run (used by later partials).</summary>
        protected List<string> SeededFieldIds { get; } = new();
        /// <summary>UWIs seeded during this run (used by later partials).</summary>
        protected List<string> SeededUwis { get; } = new();

        // ─────────────────────────────────────────────────────────────────────────

        public PPDM39DummyDataGenerator(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            WellServices wellServices,
            string connectionName,
            ILogger<PPDM39DummyDataGenerator>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _wellServices = wellServices ?? throw new ArgumentNullException(nameof(wellServices));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        /// <summary>
        /// Entry point — generates demo data for the configured connection.
        /// </summary>
        /// <param name="seedOption">minimal | standard | full</param>
        /// <param name="userId">Audit user ID written to common columns.</param>
        public async Task<DummyDataResult> GenerateAsync(string seedOption, string userId = "SYSTEM")
        {
            ApplyScale(seedOption);

            var result = new DummyDataResult { SeedOption = seedOption };

            try
            {
                _logger?.LogInformation("[DummyDataGenerator] Starting {Option} seed for {Connection}", seedOption, _connectionName);

                // ── 1. Fields ────────────────────────────────────────────────────
                await GenerateFieldsAsync(userId);
                result.FieldsCreated = SeededFieldIds.Count;

                // ── 2. Wells ─────────────────────────────────────────────────────
                await GenerateWellsAsync(userId);
                result.WellsCreated = SeededUwis.Count;

                // ── 3. Production volumes (PDEN_VOL_SUMMARY) ──────────────────
                var prodRecords = await GenerateProductionAsync(userId);
                result.ProductionRecords = prodRecords;

                // ── 4. Facilities ────────────────────────────────────────────────
                if (_includeFacilities)
                {
                    result.FacilitiesCreated = await GenerateFacilitiesAsync(userId);
                }

                // ── 5. Well tests ────────────────────────────────────────────────
                if (_includeWellTests)
                {
                    result.WellTestsCreated = await GenerateWellTestsAsync(userId);
                }

                // ── 6. Activities ────────────────────────────────────────────────
                if (_includeActivities)
                {
                    result.ActivitiesCreated = await GenerateActivitiesAsync(userId);
                }

                result.Success = true;
                result.Message = $"Generated: {result.FieldsCreated} fields, {result.WellsCreated} wells, " +
                                 $"{result.ProductionRecords} production records" +
                                 (result.FacilitiesCreated > 0 ? $", {result.FacilitiesCreated} facilities" : "") +
                                 (result.WellTestsCreated > 0 ? $", {result.WellTestsCreated} well tests" : "") +
                                 (result.ActivitiesCreated > 0 ? $", {result.ActivitiesCreated} activities" : "");

                _logger?.LogInformation("[DummyDataGenerator] {Message}", result.Message);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Generation failed: {ex.Message}";
                _logger?.LogError(ex, "[DummyDataGenerator] Failed during {Option} seed", seedOption);
            }

            return result;
        }

        // ── Scale resolver ────────────────────────────────────────────────────────

        private void ApplyScale(string seedOption)
        {
            switch (seedOption?.ToLowerInvariant())
            {
                case "full":
                    _fieldCount = 5;
                    _wellsPerField = 10;
                    _productionMonths = 60;
                    _includeWellTests = true;
                    _includeFacilities = true;
                    _includeActivities = true;
                    break;

                case "standard":
                    _fieldCount = 3;
                    _wellsPerField = 8;
                    _productionMonths = 24;
                    _includeWellTests = true;
                    _includeFacilities = true;
                    _includeActivities = false;
                    break;

                default: // minimal
                    _fieldCount = 1;
                    _wellsPerField = 5;
                    _productionMonths = 12;
                    _includeWellTests = false;
                    _includeFacilities = false;
                    _includeActivities = false;
                    break;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        /// <summary>Creates a PPDMGenericRepository for the given PPDM entity type.</summary>
        protected PPDMGenericRepository MakeRepo<T>() where T : class =>
            new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), _connectionName, typeof(T).Name);
    }

    /// <summary>Result returned from a single generator run.</summary>
    public class DummyDataResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SeedOption { get; set; } = string.Empty;
        public int FieldsCreated { get; set; }
        public int WellsCreated { get; set; }
        public int ProductionRecords { get; set; }
        public int FacilitiesCreated { get; set; }
        public int WellTestsCreated { get; set; }
        public int ActivitiesCreated { get; set; }
    }
}
