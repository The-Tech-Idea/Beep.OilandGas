using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Report;
using WELL = Beep.OilandGas.PPDM39.Models.WELL;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL
{
    /// <summary>
    /// Repository for Well entities and related business logic
    /// Handles well structures (using WELL_XREF), well status, and child entities
    /// Uses PPDMGenericRepository directly for all table operations
    /// Uses AppFilter for all queries (no SQL)
    /// </summary>
    public partial class WellServices
    {
        #region Fields and Dependencies

        protected readonly PPDMGenericRepository _wellRepository;
        protected readonly PPDMGenericRepository _wellXrefRepository;
        protected readonly PPDMGenericRepository _wellStatusRepository;
        protected readonly PPDMGenericRepository _wellStatusReferenceRepository;
        protected readonly PPDMGenericRepository _wellStatusTypeRepository;
        protected readonly PPDMGenericRepository _wellStatusQualRepository;
        protected readonly PPDMGenericRepository _wellStatusQualValueRepository;
        protected readonly IPPDM39DefaultsRepository _defaults;
        protected readonly IPPDMMetadataRepository _metadata;
        protected readonly IDMEEditor _editor;
        protected readonly string _connectionName;

        // Well Status XREF UnitOfWork (lazy initialization)
        private IUnitOfWorkWrapper _wellStatusXrefUnitOfWork;
        private readonly object _unitOfWorkLock = new object();
        private const string WELL_STATUS_XREF_TABLE = "R_WELL_STATUS_XREF";

        // ── PPDM Well Status & Classification v3 (R-3, June 2020) ───────────────────────────────────
        // Reference: PPDM Association Well Status & Classification v3 standard.
        // v3 changes vs v2:
        //   • "Business Life Cycle Phase" → "Life Cycle" (Planning/Constructing/Operating/Closing/Closed)
        //   • "Trajectory Type"           → "Profile Type" (Vertical/Inclined/Horizontal)
        //   • "Fluid Type"                → "Product Type" (Oil/Gas/Water/Geothermal/Mineral/Non-hydrocarbon Gas/Steam)
        //   • "Well Status" / "Wellbore Status" → "Condition" (Active / Inactive[Shut In, Idle, Abandoned])
        //   • "Operatorship"              → absorbed into Business Interest qualifiers (Financial-Operated etc.)
        //   • REMOVED:  "Lahee Class"     (classification now lives in Well Reporting Class or regulator data)
        //   • REMOVED:  "Well Reporting Class" (eliminated in v3)
        //   • NEW:      "Condition"       (operational state relative to Role)
        //   • NEW:      "Product Significance" (Primary/Secondary/Tertiary/Show — always paired with Product Type)
        //   • NEW:      "Regulatory Approval"  (Reg Submission → Reg Review → Reg Outcome → Reg Monitoring → Reg Closed)
        // ────────────────────────────────────────────────────────────────────────────────────────────

        /// <summary>All PPDM WSC v3 STATUS_TYPE values across Well, Wellbore and Wellhead Stream scope.</summary>
        public static readonly List<string> DEFAULT_WELL_STATUS_TYPES = new List<string>
        {
            // ── Life cycle & business context (Well scope) ──────────────────────────────
            "Life Cycle",              // Planning → Constructing → Operating → Closing → Closed
            "Business Interest",       // Yes (Financial-Operated/Non-op/Obligatory/Technical) | No  — can change
            "Business Intention",      // Explore | Appraise | Extend | Develop  — set at drilling start
            "Outcome",                 // Achieved | Unachieved  — evaluated once Business Intention is known
            // ── Geological / technical (Well / Wellbore scope) ─────────────────────────
            "Play Type",               // Conventional | Shale | Oil Sands | CBM | Gas Hydrate | Tight Sand | Sub-salt | Nonhydrocarbon | CCS
            "Role",                    // Produce | Inject | Produce/Inject | Service | Research | No Role
            "Condition",               // Active | Inactive (Shut In / Idle / Abandoned)  — operational state
            "Profile Type",            // Vertical | Inclined (Slant Hole, S-type, Deep Inclined) | Horizontal
            "Well Structure",          // Simple | Simplex | Compound | Complex | Network
            // ── Product / fluid (Well / Wellbore / Wellhead Stream scope) ───────────────
            "Product Type",            // Oil | Gas | Geothermal | Mineral | Non-hydrocarbon Gas | Steam | Water
            "Product Significance",    // Primary | Secondary | Tertiary | Show  — always paired with Product Type
            "Fluid Direction",         // Inflow | Outflow | Static | Dual Flow  — Wellhead Stream facet
            // ── Regulatory (Well / Component scope) ────────────────────────────────────
            "Regulatory Approval"      // Reg Submission → Reg Review → Reg Outcome → Reg Monitoring → Reg Closed
        };

        /// <summary>
        /// WSC v3 STATUS_TYPEs initialised at the Well level.
        /// Does NOT include Product Significance (paired at wellbore/completion level) or Profile Type (wellbore).
        /// </summary>
        public static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELL = new List<string>
        {
            "Life Cycle",              // Well — simplified five-phase model
            "Business Interest",       // Well — yes/no with hierarchical qualifiers
            "Business Intention",      // Well — general approval purpose
            "Outcome",                 // Well — result of achieving Business Intention
            "Play Type",               // Well — may change if Role or target formation changes
            "Role",                    // Well — highest-significance Role among components
            "Condition",               // Well — highest-significance Condition among components
            "Well Structure",          // Well — may change as new wellbores are added
            "Product Type",            // Well — primary product; paired with Product Significance
            "Product Significance",    // Well — Primary/Secondary/Tertiary/Show
            "Regulatory Approval"      // Well — tracked per regulated activity
        };

        /// <summary>WSC v3 STATUS_TYPEs relevant to the Wellbore level.</summary>
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELLBORE = new List<string>
        {
            "Life Cycle",              // Wellbore — may differ from parent Well life cycle
            "Business Interest",       // Wellbore — may differ from Well
            "Role",                    // Wellbore — subject to review/change over life cycle
            "Condition",               // Wellbore — Active / Inactive / Abandoned
            "Profile Type",            // Wellbore — Vertical / Inclined / Horizontal; confirmed after construction
            "Product Type",            // Wellbore — may change if completion or Role changes
            "Product Significance"     // Wellbore — paired with Product Type
        };

        /// <summary>WSC v3 STATUS_TYPEs relevant to the Wellhead Stream (FACILITY_TYPE = WELLHEAD STREAM).</summary>
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELLHEAD_STREAM = new List<string>
        {
            "Fluid Direction"          // Inflow | Outflow | Static | Dual Flow — can change over well life
        };

        #endregion

        #region Constructor

        public WellServices(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));

            // Create generic repositories for each table
            _wellRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(Beep.OilandGas.PPDM39.Models.WELL), connectionName, "WELL");

            _wellXrefRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(WELL_XREF), connectionName, "WELL_XREF");

            _wellStatusRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(WELL_STATUS), connectionName, "WELL_STATUS");

            _wellStatusReferenceRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(R_WELL_STATUS), connectionName, "R_WELL_STATUS");

            _wellStatusTypeRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(R_WELL_STATUS_TYPE), connectionName, "R_WELL_STATUS_TYPE");

            _wellStatusQualRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(R_WELL_STATUS_QUAL), connectionName, "R_WELL_STATUS_QUAL");

            _wellStatusQualValueRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(R_WELL_STATUS_QUAL_VALUE), connectionName, "R_WELL_STATUS_QUAL_VALUE");
        }

        #endregion

        #region Core Well Operations

        /// <summary>
        /// Gets well by UWI (Unique Well Identifier)
        /// </summary>
        public async Task<Beep.OilandGas.PPDM39.Models.WELL> GetByUwiAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = uwi, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var result = await _wellRepository.GetAsync(filters);
            return result.Cast<Beep.OilandGas.PPDM39.Models.WELL>().FirstOrDefault();
        }

        /// <summary>
        /// Creates a new well
        /// Optionally initializes default status types for the well
        /// </summary>
        /// <param name="well">Well entity to create</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="initializeDefaultStatuses">If true, creates default status records for all standard STATUS_TYPEs</param>
        /// <returns>Created well entity</returns>
        public async Task<Beep.OilandGas.PPDM39.Models.WELL> CreateWellAsync(
            Beep.OilandGas.PPDM39.Models.WELL well, 
            string userId,
            bool initializeDefaultStatuses = true)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var result = await _wellRepository.InsertAsync(well, userId);
            var createdWell = result as Beep.OilandGas.PPDM39.Models.WELL;

            // Initialize default status types for the well
            if (initializeDefaultStatuses && createdWell != null && !string.IsNullOrWhiteSpace(createdWell.UWI))
            {
                await InitializeDefaultWellStatusesAsync(createdWell.UWI, userId);
            }

            return createdWell;
        }

        /// <summary>
        /// Updates an existing well
        /// </summary>
        public async Task<Beep.OilandGas.PPDM39.Models.WELL> UpdateWellAsync(
            Beep.OilandGas.PPDM39.Models.WELL well, 
            string userId)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var result = await _wellRepository.UpdateAsync(well, userId);
            return result as Beep.OilandGas.PPDM39.Models.WELL;
        }

        /// <summary>
        /// Soft deletes a well (sets ACTIVE_IND = 'N')
        /// </summary>
        public async Task<bool> SoftDeleteWellAsync(string uwi, string userId)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            return await _wellRepository.SoftDeleteAsync(uwi, userId);
        }

        /// <summary>
        /// Hard deletes a well from the database
        /// </summary>
        public async Task<bool> DeleteWellAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            return await _wellRepository.DeleteAsync(uwi);
        }

        #endregion

        #region Additional Query Methods

        /// <summary>
        /// Canonical alias for <see cref="CreateWellAsync"/> — matches the pattern documented in CLAUDE.md.
        /// <code>await wellServices.CreateAsync(well, userId, initializeDefaultStatuses: true);</code>
        /// </summary>
        public Task<Beep.OilandGas.PPDM39.Models.WELL> CreateAsync(Beep.OilandGas.PPDM39.Models.WELL well, string userId, bool initializeDefaultStatuses = true)
            => CreateWellAsync(well, userId, initializeDefaultStatuses);

        /// <summary>
        /// Returns all active wells assigned to the given field.
        /// Uses <c>ASSIGNED_FIELD</c> — the PPDM 3.9 field-link column on <c>WELL</c>.
        /// </summary>
        public async Task<List<Beep.OilandGas.PPDM39.Models.WELL>> GetWellsByFieldAsync(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ASSIGNED_FIELD", FilterValue = fieldId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var result = await _wellRepository.GetAsync(filters);
            return result.OfType<Beep.OilandGas.PPDM39.Models.WELL>().ToList();
        }

        /// <summary>
        /// Returns the count of active wells in the given field.
        /// </summary>
        public async Task<int> GetWellCountByFieldAsync(string fieldId)
        {
            var wells = await GetWellsByFieldAsync(fieldId);
            return wells.Count;
        }

        /// <summary>
        /// Searches wells by UWI prefix or well name (case-insensitive LIKE search).
        /// Optionally scoped to a specific field via <paramref name="fieldId"/>.
        /// </summary>
        /// <param name="searchTerm">UWI prefix or partial well name to search for.</param>
        /// <param name="fieldId">Optional field ID to restrict the search scope.</param>
        public async Task<List<Beep.OilandGas.PPDM39.Models.WELL>> SearchWellsAsync(string searchTerm, string fieldId = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "ASSIGNED_FIELD", FilterValue = fieldId, Operator = "=" });

            // Load candidate wells then filter in-memory to avoid DB-specific LIKE syntax issues.
            var result = await _wellRepository.GetAsync(filters);
            var allWells = result.OfType<Beep.OilandGas.PPDM39.Models.WELL>().ToList();

            return allWells
                .Where(w =>
                    (w.UWI != null && w.UWI.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (w.WELL_NAME != null && w.WELL_NAME.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();
        }

        /// <summary>
        /// Returns a compact <see cref="WellSummaryDto"/> that combines the core WELL row
        /// with all current status facets pre-resolved to human-readable names.
        /// </summary>
        public async Task<WellSummaryDto> GetWellSummaryAsync(string uwi)
        {
            var well = await GetByUwiAsync(uwi);
            if (well == null)
                return null;

            // GetWellStatusSummaryAsync populates CurrentStatuses with WellStatusInfo per facet.
            var summary = await GetWellStatusSummaryAsync(uwi);
            summary.UWI = well.UWI;
            summary.WellName = well.WELL_NAME;
            summary.AssignedField = well.ASSIGNED_FIELD;
            summary.Operator = well.OPERATOR;
            summary.FinalTD = well.FINAL_TD;
            summary.ProfileType = well.PROFILE_TYPE;
            summary.SpudDate = well.SPUD_DATE;
            summary.CompletionDate = well.COMPLETION_DATE;
            summary.AbandonmentDate = well.ABANDONMENT_DATE;
            return summary;
        }

        #endregion
    }
}
