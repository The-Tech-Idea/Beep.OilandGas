using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL
{
    /// <summary>
    /// Partial class for nested classes and DTOs used by <see cref="WellServices"/>.
    /// All DTOs live here to keep discovery consistent: Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL.WellServices.
    /// </summary>
    public partial class WellServices
    {
        #region DTOs

        /// <summary>
        /// Comprehensive information about a single well-status record, enriched with
        /// R_WELL_STATUS reference data and convenience computed properties.
        /// </summary>
        public class WellStatusInfo
        {
            /// <summary>The raw WELL_STATUS row from the database.</summary>
            public WELL_STATUS WellStatus { get; set; }

            /// <summary>Matching R_WELL_STATUS reference row (may be null if reference data not loaded).</summary>
            public R_WELL_STATUS StatusDescription { get; set; }

            /// <summary>
            /// Status facets from R_WELL_STATUS_XREF keyed by facet name.
            /// Populated by <see cref="GetWellStatusWithFacetsAsync"/>.
            /// </summary>
            public Dictionary<string, object> Facets { get; set; } = new();

            /// <summary>STATUS_TYPE column value from WELL_STATUS (e.g., "Business Life Cycle Phase").</summary>
            public string StatusType { get; set; }

            /// <summary>Human-readable status name from R_WELL_STATUS.LONG_NAME or STATUS.</summary>
            public string StatusName { get; set; }

            /// <summary>Short abbreviation from R_WELL_STATUS.ABBREVIATION.</summary>
            public string StatusAbbreviation { get; set; }

            /// <summary>Grouping from R_WELL_STATUS.STATUS_GROUP (e.g., "Business", "Operational").</summary>
            public string StatusGroup { get; set; }

            /// <summary>When this status became effective (WELL_STATUS.EFFECTIVE_DATE).</summary>
            public DateTime? EffectiveDate { get; set; }

            /// <summary>
            /// When this status expires (WELL_STATUS.EXPIRY_DATE).
            /// Null means the status is currently active with no defined end date.
            /// </summary>
            public DateTime? ExpiryDate { get; set; }

            /// <summary>True when ExpiryDate is null or in the future.</summary>
            public bool IsCurrentlyActive => ExpiryDate == null || ExpiryDate > DateTime.Now;

            /// <summary>True when ExpiryDate is set and already in the past.</summary>
            public bool IsHistorical => ExpiryDate.HasValue && ExpiryDate < DateTime.Now;

            /// <summary>Number of days the current status has been in effect (0 if not yet effective).</summary>
            public int DaysInCurrentStatus =>
                EffectiveDate.HasValue
                    ? (int)(DateTime.Now - EffectiveDate.Value).TotalDays
                    : 0;
        }

        /// <summary>
        /// Compact summary of a well combining core WELL entity fields with
        /// all current status facets resolved to human-readable names.
        /// </summary>
        public class WellSummaryDto
        {
            /// <summary>Unique Well Identifier (primary key on WELL).</summary>
            public string UWI { get; set; }

            /// <summary>WELL.WELL_NAME — common display name.</summary>
            public string WellName { get; set; }

            /// <summary>WELL.ASSIGNED_FIELD — links the well to a field.</summary>
            public string AssignedField { get; set; }

            /// <summary>WELL.OPERATOR — current operating company.</summary>
            public string Operator { get; set; }

            /// <summary>WELL.FINAL_TD — final total depth (driller's depth, metres or feet per OUOM).</summary>
            public decimal? FinalTD { get; set; }

            /// <summary>WELL.PROFILE_TYPE — e.g., Vertical, Directional, Horizontal.</summary>
            public string ProfileType { get; set; }

            /// <summary>WELL.SPUD_DATE — date drilling commenced.</summary>
            public DateTime? SpudDate { get; set; }

            /// <summary>WELL.COMPLETION_DATE — date the well was completed.</summary>
            public DateTime? CompletionDate { get; set; }

            /// <summary>WELL.ABANDONMENT_DATE — date the well was abandoned (if applicable).</summary>
            public DateTime? AbandonmentDate { get; set; }

            /// <summary>
            /// All current status facets keyed by STATUS_TYPE.
            /// Each value is a <see cref="WellStatusInfo"/> with full reference-data enrichment.
            /// </summary>
            public Dictionary<string, WellStatusInfo> CurrentStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);

            // ── Convenience accessors for the most-queried PPDM 3.9 facets ─────────────────

            // ── PPDM Well Status & Classification v3 convenience properties ──────────────────────────
            // Facet names match WSC v3 (R-3, June 2020). v2 "Lahee Class" and "Well Reporting Class"
            // are eliminated in v3. "Operatorship" is expressed via Business Interest qualifiers.

            /// <summary>
            /// Current <c>Life Cycle</c> phase (WSC v3).
            /// Values: Planning | Constructing | Operating | Closing | Closed.
            /// </summary>
            public string LifecyclePhase =>
                CurrentStatuses.GetValueOrDefault("Life Cycle")?.StatusName;

            /// <summary>
            /// Current <c>Condition</c> (WSC v3) — operational state relative to the Role.
            /// Values: Active | Inactive (Shut In, Idle) | Abandoned.
            /// </summary>
            public string Condition =>
                CurrentStatuses.GetValueOrDefault("Condition")?.StatusName;

            /// <summary>
            /// Current <c>Role</c> (WSC v3) — current purpose, whether planned or actual.
            /// Values: Produce | Inject | Produce/Inject | Service | Research | No Role.
            /// </summary>
            public string Role =>
                CurrentStatuses.GetValueOrDefault("Role")?.StatusName;

            /// <summary>
            /// Current <c>Business Intention</c> (WSC v3).
            /// Values: Explore | Appraise | Extend | Develop.
            /// </summary>
            public string BusinessIntention =>
                CurrentStatuses.GetValueOrDefault("Business Intention")?.StatusName;

            /// <summary>
            /// Current <c>Business Interest</c> (WSC v3).
            /// Values: Yes-Financial-Operated | Yes-Financial-Non-operated | Yes-Obligatory | Yes-Technical | No.
            /// </summary>
            public string BusinessInterest =>
                CurrentStatuses.GetValueOrDefault("Business Interest")?.StatusName;

            /// <summary>
            /// Current <c>Product Type</c> (WSC v3, replaces v2 "Fluid Type").
            /// Values: Oil | Gas | Geothermal | Mineral | Non-hydrocarbon Gas | Steam | Water.
            /// </summary>
            public string ProductType =>
                CurrentStatuses.GetValueOrDefault("Product Type")?.StatusName;

            /// <summary>
            /// Current <c>Product Significance</c> (WSC v3, new in v3).
            /// Values: Primary | Secondary | Tertiary | Show.
            /// Always paired with ProductType.
            /// </summary>
            public string ProductSignificance =>
                CurrentStatuses.GetValueOrDefault("Product Significance")?.StatusName;

            /// <summary>
            /// Current <c>Play Type</c> (WSC v3).
            /// Values: Conventional | Shale | Oil Sands | Coalbed Methane | Gas Hydrate | Tight Sand | Sub-salt | Nonhydrocarbon | CCS.
            /// </summary>
            public string PlayType =>
                CurrentStatuses.GetValueOrDefault("Play Type")?.StatusName;

            /// <summary>
            /// Current <c>Well Structure</c> (WSC v3).
            /// Values: Simple | Simplex | Compound | Complex | Network.
            /// </summary>
            public string WellStructure =>
                CurrentStatuses.GetValueOrDefault("Well Structure")?.StatusName;

            /// <summary>
            /// Current <c>Profile Type</c> (WSC v3, replaces v2 "Trajectory Type").
            /// Values: Vertical | Inclined | Horizontal.
            /// </summary>
            public string ProfileTypeFacet =>
                CurrentStatuses.GetValueOrDefault("Profile Type")?.StatusName;

            /// <summary>
            /// Current <c>Outcome</c> (WSC v3).
            /// Values: Achieved | Unachieved.
            /// Only meaningful when Business Intention is known.
            /// </summary>
            public string Outcome =>
                CurrentStatuses.GetValueOrDefault("Outcome")?.StatusName;

            /// <summary>
            /// Current <c>Fluid Direction</c> (WSC v3 — Wellhead Stream scope).
            /// Values: Inflow | Outflow | Static | Dual Flow.
            /// </summary>
            public string FluidDirection =>
                CurrentStatuses.GetValueOrDefault("Fluid Direction")?.StatusName;
        }

        /// <summary>
        /// Records a single status-type transition for audit and timeline display.
        /// </summary>
        public class WellStatusTransition
        {
            /// <summary>UWI of the well that changed status.</summary>
            public string UWI { get; set; }

            /// <summary>PPDM 3.9 STATUS_TYPE facet that changed (e.g., "Well Status").</summary>
            public string StatusType { get; set; }

            /// <summary>Previous STATUS value (null if this was the first assignment).</summary>
            public string PreviousStatus { get; set; }

            /// <summary>New STATUS value.</summary>
            public string NewStatus { get; set; }

            /// <summary>Date the new status became effective.</summary>
            public DateTime TransitionDate { get; set; }

            /// <summary>User ID who performed the transition.</summary>
            public string PerformedBy { get; set; }
        }

        // ── API transfer objects for the facet-classification page ──────────────────────────────────

        /// <summary>
        /// Request to set (insert or update) one STATUS_TYPE facet on a well.
        /// Maps to one WELL_STATUS row: STATUS_TYPE is the facet dimension,
        /// STATUS is the new value, STATUS_QUALIFIER / STATUS_QUALIFIER_VALUE are optional sub-qualifiers.
        /// </summary>
        public class SetFacetRequest
        {
            /// <summary>UWI of the well being classified.</summary>
            public string UWI { get; set; }

            /// <summary>PPDM STATUS_TYPE — identifies which facet is being set (e.g., "Condition").</summary>
            public string StatusType { get; set; }

            /// <summary>New STATUS value from R_WELL_STATUS (e.g., "Active", "Shut In").</summary>
            public string Status { get; set; }

            /// <summary>Optional STATUS_QUALIFIER (e.g., "Financial-Operated" for Business Interest = Yes).</summary>
            public string StatusQualifier { get; set; }

            /// <summary>Optional STATUS_QUALIFIER_VALUE for the selected qualifier.</summary>
            public string StatusQualifierValue { get; set; }

            /// <summary>Date the new status becomes effective. Defaults to today.</summary>
            public System.DateTime? EffectiveDate { get; set; }

            /// <summary>
            /// Date the new status expires. Null means "currently active, no defined end".
            /// Setting this will expire the current status record.
            /// </summary>
            public System.DateTime? ExpiryDate { get; set; }

            /// <summary>Free-text note for this status assignment (REMARK column).</summary>
            public string Remark { get; set; }

            /// <summary>WELL_STATUS.SOURCE — data provider or system of origin. Defaults to "USER".</summary>
            public string Source { get; set; } = "USER";

            /// <summary>WELL_STATUS.PERCENT_CAPABILITY — optional numeric indicator (0–100).</summary>
            public decimal? PercentCapability { get; set; }
        }

        /// <summary>
        /// One R_WELL_STATUS row — a single valid STATUS value for a given STATUS_TYPE.
        /// Returned from the reference-data API endpoints.
        /// </summary>
        public class FacetValueDto
        {
            /// <summary>STATUS_TYPE this value belongs to.</summary>
            public string StatusType { get; set; }

            /// <summary>STATUS column value (e.g., "Active", "Planning").</summary>
            public string Status { get; set; }

            /// <summary>Short abbreviation from R_WELL_STATUS.ABBREVIATION.</summary>
            public string Abbreviation { get; set; }

            /// <summary>Full human-readable description from R_WELL_STATUS.LONG_NAME.</summary>
            public string LongName { get; set; }
        }

        /// <summary>
        /// One R_WELL_STATUS_QUAL row — a STATUS_QUALIFIER allowed for a given STATUS_TYPE.
        /// </summary>
        public class FacetQualifierDto
        {
            /// <summary>STATUS_TYPE this qualifier applies to.</summary>
            public string StatusType { get; set; }

            /// <summary>STATUS value this qualifier is restricted to (null = applies to all).</summary>
            public string Status { get; set; }

            /// <summary>STATUS_QUALIFIER column value (e.g., "Financial-Operated").</summary>
            public string Qualifier { get; set; }

            /// <summary>Full description from R_WELL_STATUS_QUAL.LONG_NAME.</summary>
            public string LongName { get; set; }
        }

        /// <summary>
        /// One R_WELL_STATUS_QUAL_VALUE row — a valid QUALIFIER_VALUE for a
        /// given STATUS_TYPE + STATUS + STATUS_QUALIFIER combination.
        /// </summary>
        public class FacetQualifierValueDto
        {
            public string StatusType { get; set; }
            public string Status { get; set; }
            public string Qualifier { get; set; }
            public string QualifierValue { get; set; }
            public string LongName { get; set; }
        }

        /// <summary>
        /// Full reference definition for one STATUS_TYPE facet, combining
        /// R_WELL_STATUS_TYPE + R_WELL_STATUS + R_WELL_STATUS_QUAL* data
        /// with the current WELL_STATUS value for a specific well.
        ///
        /// This is the primary DTO returned to the classification page.
        /// </summary>
        public class FacetTypeDto
        {
            // ── Facet type definition (from R_WELL_STATUS_TYPE / FACET_CATALOG) ────────

            /// <summary>STATUS_TYPE column value (e.g., "Life Cycle").</summary>
            public string StatusType { get; set; }

            /// <summary>Full description of what this facet type means.</summary>
            public string LongName { get; set; }

            /// <summary>PPDM scope: Well | Wellbore | Wellhead Stream | Component.</summary>
            public string? Scope { get; set; }

            /// <summary>Valid STATUS values (from R_WELL_STATUS or FACET_CATALOG).</summary>
            public List<FacetValueDto> Values { get; set; } = new();

            /// <summary>
            /// STATUS_QUALIFIER options grouped by the STATUS they apply to.
            /// Key = STATUS code; value = list of qualifiers for that status.
            /// Key "*" means the qualifiers apply to all statuses.
            /// </summary>
            public Dictionary<string, List<FacetQualifierDto>> Qualifiers { get; set; } = new();

            /// <summary>
            /// Qualifier values grouped by "(STATUS, QUALIFIER)" composite key string "status|qualifier".
            /// </summary>
            public Dictionary<string, List<FacetQualifierValueDto>> QualifierValues { get; set; } = new();

            // ── Current well value (from WELL_STATUS for the requested UWI) ────────────

            /// <summary>
            /// Current WELL_STATUS row for this facet on the requested well.
            /// Null if no status has been set yet.
            /// </summary>
            public WellStatusInfo CurrentValue { get; set; }
        }

        #endregion

        #region Well Status Info Methods

        /// <summary>
        /// Builds a <see cref="WellStatusInfo"/> for an existing WELL_STATUS row,
        /// enriching it with R_WELL_STATUS reference data and any associated facets.
        /// </summary>
        public async Task<WellStatusInfo> GetWellStatusInfoAsync(WELL_STATUS wellStatus)
        {
            if (wellStatus == null)
                throw new ArgumentNullException(nameof(wellStatus));

            // Use STATUS_TYPE directly from the WELL_STATUS row (no separate lookup needed).
            R_WELL_STATUS statusDesc = null;
            if (!string.IsNullOrWhiteSpace(wellStatus.STATUS_TYPE) && !string.IsNullOrWhiteSpace(wellStatus.STATUS))
                statusDesc = await GetWellStatusDescriptionAsync(wellStatus.STATUS_TYPE, wellStatus.STATUS);
            else
                statusDesc = await GetWellStatusDescriptionByStatusIdAsync(wellStatus.STATUS_ID);

            var facets = await GetWellStatusFacetsAsync(wellStatus.STATUS_ID);

            return new WellStatusInfo
            {
                WellStatus = wellStatus,
                StatusDescription = statusDesc,
                Facets = facets,
                StatusType = wellStatus.STATUS_TYPE ?? statusDesc?.STATUS_TYPE,
                StatusName = statusDesc?.LONG_NAME ?? statusDesc?.STATUS ?? wellStatus.STATUS,
                StatusAbbreviation = statusDesc?.ABBREVIATION,
                StatusGroup = statusDesc?.STATUS_GROUP,
                EffectiveDate = wellStatus.EFFECTIVE_DATE,
                ExpiryDate = wellStatus.EXPIRY_DATE
            };
        }

        #endregion
    }
}
