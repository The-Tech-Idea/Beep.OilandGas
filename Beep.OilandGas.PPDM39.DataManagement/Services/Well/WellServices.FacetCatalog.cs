using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL
{
    /// <summary>
    /// Static WSC v3 (R-3, June 2020) facet catalog embedded in <see cref="WellServices"/>.
    ///
    /// Used as a fallback when the R_WELL_STATUS / R_WELL_STATUS_QUAL /
    /// R_WELL_STATUS_QUAL_VALUE reference tables have not yet been seeded.
    /// The database records always take precedence over this catalog.
    ///
    /// Schema → catalog correspondence:
    ///   R_WELL_STATUS_TYPE          → FacetTypeDef.StatusType / LongName / Scope
    ///   R_WELL_STATUS               → FacetTypeDef.Values  (FacetValueDef)
    ///   R_WELL_STATUS_QUAL          → FacetTypeDef.Qualifiers (FacetQualifierDef per STATUS_TYPE)
    ///   R_WELL_STATUS_QUAL_VALUE    → FacetTypeDef.QualifierValues (FacetQualifierValueDef per STATUS+QUALIFIER)
    /// </summary>
    public partial class WellServices
    {
        // ─────────────────────────────────────────────────────────────────────
        // Primitive record types used throughout the catalog
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// A valid STATUS value for a given STATUS_TYPE (maps to one R_WELL_STATUS row).
        /// </summary>
        public sealed record FacetValueDef(
            string Status,
            string Abbreviation,
            string LongName);

        /// <summary>
        /// A STATUS_QUALIFIER allowed for a given STATUS_TYPE (maps to one R_WELL_STATUS_QUAL row).
        /// </summary>
        public sealed record FacetQualifierDef(
            string Qualifier,
            string LongName);

        /// <summary>
        /// A QUALIFIER_VALUE for a given STATUS_TYPE + STATUS + STATUS_QUALIFIER combination
        /// (maps to one R_WELL_STATUS_QUAL_VALUE row).
        /// </summary>
        public sealed record FacetQualifierValueDef(
            string QualifierValue,
            string LongName);

        /// <summary>
        /// Full definition of one WSC v3 facet type — corresponds to R_WELL_STATUS_TYPE
        /// plus all related R_WELL_STATUS and R_WELL_STATUS_QUAL* rows.
        /// </summary>
        public sealed class FacetTypeDef
        {
            /// <summary>PPDM STATUS_TYPE column value (e.g., "Life Cycle").</summary>
            public string StatusType { get; init; }

            /// <summary>Human-readable description of what this facet type means (maps to R_WELL_STATUS_TYPE.LONG_NAME).</summary>
            public string LongName { get; init; }

            /// <summary>PPDM scope: Well | Wellbore | Wellhead Stream | Component.</summary>
            public string Scope { get; init; }

            /// <summary>All valid STATUS values for this facet type (maps to R_WELL_STATUS rows).</summary>
            public IReadOnlyList<FacetValueDef> Values { get; init; }

            /// <summary>
            /// Qualifiers applicable to this STATUS_TYPE (maps to R_WELL_STATUS_QUAL rows).
            /// Key = STATUS code, value = list of qualifiers valid for that status.
            /// If a qualifier applies to all statuses use key "*".
            /// </summary>
            public IReadOnlyDictionary<string, IReadOnlyList<FacetQualifierDef>> Qualifiers { get; init; }

            /// <summary>
            /// Valid qualifier values keyed by "(STATUS, QUALIFIER)" composite.
            /// Maps to R_WELL_STATUS_QUAL_VALUE rows.
            /// </summary>
            public IReadOnlyDictionary<(string Status, string Qualifier), IReadOnlyList<FacetQualifierValueDef>> QualifierValues { get; init; }
        }

        // ─────────────────────────────────────────────────────────────────────
        // WSC v3 Catalog — 13 STATUS_TYPE facets across all scopes
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// WSC v3 (June 2020) facet catalog keyed by STATUS_TYPE.
        /// Mirrors the pre-populated R_WELL_STATUS* reference tables.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, FacetTypeDef> FACET_CATALOG =
            new Dictionary<string, FacetTypeDef>(System.StringComparer.OrdinalIgnoreCase)
            {
                // ── 1. Life Cycle ──────────────────────────────────────────────────────
                ["Life Cycle"] = new FacetTypeDef
                {
                    StatusType = "Life Cycle",
                    LongName   = "The simplified five-phase life cycle of a well from planning through permanent closure.",
                    Scope      = "Well / Wellbore",
                    Values = new[]
                    {
                        new FacetValueDef("Planning",     "PLAN",  "Well approved; plans being formulated, permits pending"),
                        new FacetValueDef("Constructing", "CNST",  "Well is currently being drilled, cased, or completed"),
                        new FacetValueDef("Operating",    "OPER",  "Well is in service — producing, injecting, or monitoring"),
                        new FacetValueDef("Closing",      "CLOS",  "Decommissioning activities are in progress"),
                        new FacetValueDef("Closed",       "CLSD",  "Permanently abandoned; all regulatory obligations met"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 2. Role ────────────────────────────────────────────────────────────
                ["Role"] = new FacetTypeDef
                {
                    StatusType = "Role",
                    LongName   = "The current purpose of the well, whether planned or actual.  The Role at the Well level reflects the highest-significance Role among its components.",
                    Scope      = "Well / Wellbore",
                    Values = new[]
                    {
                        new FacetValueDef("Produce",         "PROD",  "Well is producing reservoir fluids"),
                        new FacetValueDef("Inject",          "INJT",  "Well is injecting fluids into the reservoir"),
                        new FacetValueDef("Produce/Inject",  "PRIJ",  "Well alternates between production and injection"),
                        new FacetValueDef("Service",         "SRVC",  "Well serves a support function (disposal, water supply, monitoring)"),
                        new FacetValueDef("Research",        "RSCH",  "Well used for scientific or experimental purposes"),
                        new FacetValueDef("No Role",         "NRLE",  "No current active role assigned to this well"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 3. Condition ───────────────────────────────────────────────────────
                ["Condition"] = new FacetTypeDef
                {
                    StatusType = "Condition",
                    LongName   = "The operational state of the well relative to its assigned Role.  Replaces the v2 'Well Status' and 'Wellbore Status' facets.",
                    Scope      = "Well / Wellbore",
                    Values = new[]
                    {
                        new FacetValueDef("Active",    "ACTV", "Well is operating as intended for its Role"),
                        new FacetValueDef("Shut In",   "SHTI", "Temporarily stopped; can be returned to service without significant remediation"),
                        new FacetValueDef("Idle",      "IDLE", "Not in service; future use is uncertain"),
                        new FacetValueDef("Abandoned", "ABND", "Permanently removed from service; no further production or injection expected"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 4. Business Interest ───────────────────────────────────────────────
                ["Business Interest"] = new FacetTypeDef
                {
                    StatusType = "Business Interest",
                    LongName   = "Whether the reporting company holds a business interest in this well. When the answer is Yes, a qualifier specifies the nature of that interest.",
                    Scope      = "Well",
                    Values = new[]
                    {
                        new FacetValueDef("Yes", "YES", "Company holds a current business interest in this well"),
                        new FacetValueDef("No",  "NO",  "Company has no current business interest in this well"),
                    },
                    Qualifiers = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>
                    {
                        // Qualifiers are only applicable when STATUS = "Yes"
                        ["Yes"] = new[]
                        {
                            new FacetQualifierDef("Financial-Operated",     "Company has working interest and is also the operator"),
                            new FacetQualifierDef("Financial-Non-operated", "Company has working interest; another company is the operator"),
                            new FacetQualifierDef("Obligatory",             "Company is obligated to participate (e.g., carried or non-consent interest)"),
                            new FacetQualifierDef("Technical",              "Technical interest only; company has no financial exposure"),
                        },
                    },
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 5. Business Intention ──────────────────────────────────────────────
                ["Business Intention"] = new FacetTypeDef
                {
                    StatusType = "Business Intention",
                    LongName   = "The general business purpose for which the well was approved.  Set at the time of drilling approval; may be updated if the well's purpose changes.",
                    Scope      = "Well",
                    Values = new[]
                    {
                        new FacetValueDef("Explore",  "EXPL", "Test an exploration concept or enter a new area"),
                        new FacetValueDef("Appraise", "APPR", "Evaluate the extent and quality of a known discovery"),
                        new FacetValueDef("Extend",   "EXTN", "Extend a known producing reservoir beyond its current boundary"),
                        new FacetValueDef("Develop",  "DVLP", "Produce from a known, appraised reservoir"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 6. Outcome ─────────────────────────────────────────────────────────
                ["Outcome"] = new FacetTypeDef
                {
                    StatusType = "Outcome",
                    LongName   = "The result of the Business Intention.  Only populated once it is known whether the well achieved its stated purpose.",
                    Scope      = "Well",
                    Values = new[]
                    {
                        new FacetValueDef("Achieved",   "ACHV", "Business Intention was fully achieved as planned"),
                        new FacetValueDef("Unachieved", "UNAC", "Business Intention was not achieved"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 7. Play Type ───────────────────────────────────────────────────────
                ["Play Type"] = new FacetTypeDef
                {
                    StatusType = "Play Type",
                    LongName   = "The geological play concept being targeted by this well.  May change if the Role or target formation changes over the well's life.",
                    Scope      = "Well",
                    Values = new[]
                    {
                        new FacetValueDef("Conventional",       "CONV", "Conventional trap-and-seal reservoir"),
                        new FacetValueDef("Shale",              "SHLE", "Low-permeability shale reservoir (gas or oil)"),
                        new FacetValueDef("Oil Sands",          "OILS", "Bitumen or heavy oil in sand matrix"),
                        new FacetValueDef("Coalbed Methane",    "CBM",  "Methane adsorbed in coal seams (CBM / coal seam gas)"),
                        new FacetValueDef("Gas Hydrate",        "GHYD", "Methane locked in ice-like hydrate structures on the seabed or permafrost"),
                        new FacetValueDef("Tight Sand",         "TGSD", "Low-permeability sandstone reservoir requiring stimulation"),
                        new FacetValueDef("Sub-salt",           "SBSL", "Reservoir located structurally below a salt body"),
                        new FacetValueDef("Nonhydrocarbon",     "NHYD", "Non-hydrocarbon gas target (CO₂, N₂, H₂, H₂S)"),
                        new FacetValueDef("CCS",                "CCS",  "Carbon Capture and Storage — injection into a suitable geological formation"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 8. Well Structure ──────────────────────────────────────────────────
                ["Well Structure"] = new FacetTypeDef
                {
                    StatusType = "Well Structure",
                    LongName   = "The structural complexity of the well, describing how many wellbores and completions it contains.  May change as additional wellbores are added.",
                    Scope      = "Well",
                    Values = new[]
                    {
                        new FacetValueDef("Simple",   "SMPL", "Single vertical wellbore with a single completion interval"),
                        new FacetValueDef("Simplex",  "SPLX", "Single wellbore with multiple completions within the same reservoir zone"),
                        new FacetValueDef("Compound", "CMPD", "Multiple wellbores drilled from a single surface location, each targeting separate zones"),
                        new FacetValueDef("Complex",  "CPLX", "Multiple wellbores including at least one lateral or multilateral branch"),
                        new FacetValueDef("Network",  "NTWK", "Integrated network of wellbores operated and managed as a single system"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 9. Profile Type ────────────────────────────────────────────────────
                ["Profile Type"] = new FacetTypeDef
                {
                    StatusType = "Profile Type",
                    LongName   = "The geometric trajectory of the wellbore.  Replaces the v2 'Trajectory Type' facet.  Confirmed after the wellbore is constructed.",
                    Scope      = "Wellbore",
                    Values = new[]
                    {
                        new FacetValueDef("Vertical",   "VERT", "Wellbore deviates less than 5° from vertical throughout its length"),
                        new FacetValueDef("Inclined",   "INCL", "Wellbore deviates between 5° and 80° from vertical (includes slant, S-type, deep inclined)"),
                        new FacetValueDef("Horizontal", "HORZ", "Wellbore reaches or exceeds 80° deviation from vertical over a significant interval"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 10. Product Type ───────────────────────────────────────────────────
                ["Product Type"] = new FacetTypeDef
                {
                    StatusType = "Product Type",
                    LongName   = "The primary fluid or mineral product targeted or produced by this well.  Replaces the v2 'Fluid Type' facet.  Always paired with Product Significance.",
                    Scope      = "Well / Wellbore / Wellhead Stream",
                    Values = new[]
                    {
                        new FacetValueDef("Oil",                 "OIL",  "Crude oil is the primary product"),
                        new FacetValueDef("Gas",                 "GAS",  "Natural gas (methane-dominant) is the primary product"),
                        new FacetValueDef("Geothermal",          "GEO",  "Steam or hot water extracted from a geothermal source"),
                        new FacetValueDef("Mineral",             "MINL", "Dissolved or in-situ mineral (potash, lithium, uranium, brine)"),
                        new FacetValueDef("Non-hydrocarbon Gas", "NHCG", "Non-hydrocarbon gas: CO₂, N₂, H₂, or H₂S"),
                        new FacetValueDef("Steam",               "STM",  "Steam injection (thermal EOR) or geothermal steam recovery"),
                        new FacetValueDef("Water",               "WTR",  "Fresh-water supply or produced/disposal water"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 11. Product Significance ───────────────────────────────────────────
                ["Product Significance"] = new FacetTypeDef
                {
                    StatusType = "Product Significance",
                    LongName   = "The economic significance of the associated Product Type for this well.  New in WSC v3; always paired with a Product Type entry.",
                    Scope      = "Well / Wellbore / Wellhead Stream",
                    Values = new[]
                    {
                        new FacetValueDef("Primary",   "PRI",  "Main product that defines the well's economic value"),
                        new FacetValueDef("Secondary", "SEC",  "By-product of significant but lesser economic value"),
                        new FacetValueDef("Tertiary",  "TER",  "Minor by-product with limited economic contribution"),
                        new FacetValueDef("Show",      "SHOW", "Product was detected but is not in commercial quantities"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 12. Fluid Direction ────────────────────────────────────────────────
                ["Fluid Direction"] = new FacetTypeDef
                {
                    StatusType = "Fluid Direction",
                    LongName   = "The direction of fluid movement through a Wellhead Stream (FACILITY_TYPE = WELLHEAD STREAM).  Can change over the well's producing life.",
                    Scope      = "Wellhead Stream",
                    Values = new[]
                    {
                        new FacetValueDef("Inflow",    "INFL", "Fluid flowing from the reservoir into the wellbore (production)"),
                        new FacetValueDef("Outflow",   "OUTF", "Fluid flowing from the wellbore into the reservoir (injection)"),
                        new FacetValueDef("Static",    "STAT", "No fluid movement; well is shut in or idle"),
                        new FacetValueDef("Dual Flow", "DUAL", "Simultaneous inflow in one zone and outflow in another (commingled)"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },

                // ── 13. Regulatory Approval ────────────────────────────────────────────
                ["Regulatory Approval"] = new FacetTypeDef
                {
                    StatusType = "Regulatory Approval",
                    LongName   = "The stage of the regulatory approval process for a given regulated activity on this well.  Tracked per regulated activity at the well or component level.",
                    Scope      = "Well / Component",
                    Values = new[]
                    {
                        new FacetValueDef("Reg Submission", "RGSB", "Application or notice has been submitted to the regulator"),
                        new FacetValueDef("Reg Review",     "RGRV", "Regulator is actively reviewing the submission"),
                        new FacetValueDef("Reg Outcome",    "RGOT", "Regulator has issued a formal decision, approval, or rejection"),
                        new FacetValueDef("Reg Monitoring", "RGMN", "Post-approval monitoring period is in progress"),
                        new FacetValueDef("Reg Closed",     "RGCL", "Regulatory process is fully closed; all obligations met"),
                    },
                    Qualifiers    = new Dictionary<string, IReadOnlyList<FacetQualifierDef>>(),
                    QualifierValues = new Dictionary<(string, string), IReadOnlyList<FacetQualifierValueDef>>(),
                },
            };
    }
}
