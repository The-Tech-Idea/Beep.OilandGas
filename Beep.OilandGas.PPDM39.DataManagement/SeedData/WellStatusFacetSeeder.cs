using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Seeds the four PPDM 3.9 well-status reference tables from the static WSC v3 (R-3, June 2020)
    /// catalog embedded in <see cref="WellServices.FACET_CATALOG"/>.
    ///
    /// Tables seeded (in dependency order):
    ///   1. R_WELL_STATUS_TYPE       — one row per STATUS_TYPE facet
    ///   2. R_WELL_STATUS            — valid STATUS values per STATUS_TYPE
    ///   3. R_WELL_STATUS_QUAL       — STATUS_QUALIFIER names per STATUS_TYPE
    ///   4. R_WELL_STATUS_QUAL_VALUE — QUALIFIER_VALUE options per STATUS_TYPE + STATUS + QUALIFIER
    ///
    /// All inserts are skip-if-exists (idempotent) — safe to run multiple times.
    /// </summary>
    public class WellStatusFacetSeeder
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;

        public WellStatusFacetSeeder(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _editor              = editor              ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler  ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults            = defaults             ?? throw new ArgumentNullException(nameof(defaults));
            _metadata            = metadata             ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName      = connectionName;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Seeds all six reference tables (R_* core + RA_* alias) and returns an aggregate result.
        /// Seeding order: R_WELL_STATUS_TYPE → R_WELL_STATUS → R_WELL_STATUS_QUAL → R_WELL_STATUS_QUAL_VALUE
        ///                → RA_WELL_STATUS_TYPE → RA_WELL_STATUS
        /// All operations are idempotent (skip-if-exists).
        /// </summary>
        public async Task<FacetSeedResult> SeedAllAsync(string userId = "SYSTEM")
        {
            var result = new FacetSeedResult();

            try
            {
                // ── R_* core reference tables (must come first — RA_* FK to these) ──
                result.FacetTypeRows       = await SeedFacetTypesAsync(userId);
                result.FacetValueRows      = await SeedFacetValuesAsync(userId);
                result.FacetQualifierRows  = await SeedFacetQualifiersAsync(userId);
                result.FacetQualValueRows  = await SeedFacetQualifierValuesAsync(userId);

                // ── RA_* alias tables (cross-reference / extended metadata) ──────────
                result.RaFacetTypeRows  = await SeedRaFacetTypesAsync(userId);
                result.RaFacetValueRows = await SeedRaFacetValuesAsync(userId);

                result.Success = true;
                result.Message = $"R_* seeded: {result.FacetTypeRows} types, {result.FacetValueRows} values, " +
                                 $"{result.FacetQualifierRows} qualifiers, {result.FacetQualValueRows} qualifier-values. " +
                                 $"RA_* seeded: {result.RaFacetTypeRows} types, {result.RaFacetValueRows} values.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Seeding failed: {ex.Message}";
                result.Errors.Add(ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// Returns the current row counts for all four reference tables.
        /// Used to show the UI whether seeding is needed.
        /// </summary>
        public async Task<FacetSeedStatus> GetStatusAsync()
        {
            return new FacetSeedStatus
            {
                FacetTypeCount      = await CountTableAsync("R_WELL_STATUS_TYPE"),
                FacetValueCount     = await CountTableAsync("R_WELL_STATUS"),
                QualifierCount      = await CountTableAsync("R_WELL_STATUS_QUAL"),
                QualifierValueCount = await CountTableAsync("R_WELL_STATUS_QUAL_VALUE"),
                RaFacetTypeCount    = await CountTableAsync("RA_WELL_STATUS_TYPE"),
                RaFacetValueCount   = await CountTableAsync("RA_WELL_STATUS"),
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // Individual table seeders
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Seeds R_WELL_STATUS_TYPE — one row per WSC v3 STATUS_TYPE.</summary>
        public async Task<int> SeedFacetTypesAsync(string userId = "SYSTEM")
        {
            var repo = BuildRepo<R_WELL_STATUS_TYPE>("R_WELL_STATUS_TYPE");
            int count = 0;

            foreach (var kv in WellServices.FACET_CATALOG)
            {
                var def = kv.Value;
                if (await ExistsAsync(repo, "STATUS_TYPE", def.StatusType)) continue;

                var row = new R_WELL_STATUS_TYPE
                {
                    STATUS_TYPE  = def.StatusType,
                    ABBREVIATION = Abbreviate(def.StatusType),
                    LONG_NAME    = def.LongName,
                    ACTIVE_IND   = _defaults.GetActiveIndicatorYes()
                };

                await repo.InsertAsync(row, userId);
                count++;
            }
            return count;
        }

        /// <summary>Seeds R_WELL_STATUS — valid STATUS codes per STATUS_TYPE.</summary>
        public async Task<int> SeedFacetValuesAsync(string userId = "SYSTEM")
        {
            var repo = BuildRepo<R_WELL_STATUS>("R_WELL_STATUS");
            int count = 0;

            foreach (var kv in WellServices.FACET_CATALOG)
            {
                var def = kv.Value;
                foreach (var val in def.Values)
                {
                    if (await ExistsAsync(repo, "STATUS_TYPE", def.StatusType, "STATUS", val.Status)) continue;

                    var row = new R_WELL_STATUS
                    {
                        STATUS_TYPE  = def.StatusType,
                        STATUS       = val.Status,
                        ABBREVIATION = val.Abbreviation,
                        LONG_NAME    = val.LongName,
                        ACTIVE_IND   = _defaults.GetActiveIndicatorYes()
                    };

                    await repo.InsertAsync(row, userId);
                    count++;
                }
            }
            return count;
        }

        /// <summary>Seeds R_WELL_STATUS_QUAL — qualifier definitions per STATUS_TYPE.</summary>
        public async Task<int> SeedFacetQualifiersAsync(string userId = "SYSTEM")
        {
            var repo = BuildRepo<R_WELL_STATUS_QUAL>("R_WELL_STATUS_QUAL");
            // Collect distinct qualifiers per STATUS_TYPE across all STATUS keys.
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int count = 0;

            foreach (var kv in WellServices.FACET_CATALOG)
            {
                var def = kv.Value;
                if (def.Qualifiers == null || !def.Qualifiers.Any()) continue;

                foreach (var qualList in def.Qualifiers.Values)
                {
                    foreach (var q in qualList)
                    {
                        var uniqueKey = $"{def.StatusType}|{q.Qualifier}";
                        if (!seen.Add(uniqueKey)) continue;
                        if (await ExistsAsync(repo, "STATUS_TYPE", def.StatusType, "STATUS_QUALIFIER", q.Qualifier)) continue;

                        var row = new R_WELL_STATUS_QUAL
                        {
                            STATUS_TYPE      = def.StatusType,
                            STATUS_QUALIFIER = q.Qualifier,
                            LONG_NAME        = q.LongName,
                            ACTIVE_IND       = _defaults.GetActiveIndicatorYes()
                        };

                        await repo.InsertAsync(row, userId);
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>Seeds R_WELL_STATUS_QUAL_VALUE — qualifier-value options.</summary>
        public async Task<int> SeedFacetQualifierValuesAsync(string userId = "SYSTEM")
        {
            var repo = BuildRepo<R_WELL_STATUS_QUAL_VALUE>("R_WELL_STATUS_QUAL_VALUE");
            int count = 0;

            foreach (var kv in WellServices.FACET_CATALOG)
            {
                var def = kv.Value;
                if (def.QualifierValues == null || !def.QualifierValues.Any()) continue;

                foreach (var qvKv in def.QualifierValues)
                {
                    var (status, qualifier) = qvKv.Key;
                    foreach (var qv in qvKv.Value)
                    {
                        if (await ExistsAsync(repo,
                                "STATUS_TYPE",            def.StatusType,
                                "STATUS",                 status,
                                "STATUS_QUALIFIER",       qualifier,
                                "STATUS_QUALIFIER_VALUE", qv.QualifierValue)) continue;

                        var row = new R_WELL_STATUS_QUAL_VALUE
                        {
                            STATUS_TYPE              = def.StatusType,
                            STATUS                   = status,
                            STATUS_QUALIFIER         = qualifier,
                            STATUS_QUALIFIER_VALUE   = qv.QualifierValue,
                            LONG_NAME                = qv.LongName,
                            ACTIVE_IND               = _defaults.GetActiveIndicatorYes()
                        };

                        await repo.InsertAsync(row, userId);
                        count++;
                    }
                }
            }
            return count;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        private PPDMGenericRepository BuildRepo<T>(string tableName) where T : class, IPPDMEntity, new()
            => new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), _connectionName, tableName);

        private static async Task<bool> ExistsAsync(
            PPDMGenericRepository repo,
            string col1, string val1,
            string col2 = null, string val2 = null,
            string col3 = null, string val3 = null,
            string col4 = null, string val4 = null)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = col1, Operator = "=", FilterValue = val1 }
            };
            if (col2 != null) filters.Add(new AppFilter { FieldName = col2, Operator = "=", FilterValue = val2 });
            if (col3 != null) filters.Add(new AppFilter { FieldName = col3, Operator = "=", FilterValue = val3 });
            if (col4 != null) filters.Add(new AppFilter { FieldName = col4, Operator = "=", FilterValue = val4 });

            var result = await repo.GetAsync(filters);
            return result != null && result.Any();
        }

        private static async Task<int> CountTableAsync_Internal(PPDMGenericRepository repo)
        {
            try
            {
                var all = await repo.GetAsync(new List<AppFilter>());
                return all?.Count() ?? 0;
            }
            catch { return -1; }
        }

        private async Task<int> CountTableAsync(string tableName)
        {
            try
            {
                return tableName switch
                {
                    "R_WELL_STATUS_TYPE"       => await CountTableAsync_Internal(BuildRepo<R_WELL_STATUS_TYPE>(tableName)),
                    "R_WELL_STATUS"            => await CountTableAsync_Internal(BuildRepo<R_WELL_STATUS>(tableName)),
                    "R_WELL_STATUS_QUAL"       => await CountTableAsync_Internal(BuildRepo<R_WELL_STATUS_QUAL>(tableName)),
                    "R_WELL_STATUS_QUAL_VALUE" => await CountTableAsync_Internal(BuildRepo<R_WELL_STATUS_QUAL_VALUE>(tableName)),
                    "RA_WELL_STATUS_TYPE"       => await CountTableAsync_Internal(BuildRepo<RA_WELL_STATUS_TYPE>(tableName)),
                    "RA_WELL_STATUS"            => await CountTableAsync_Internal(BuildRepo<RA_WELL_STATUS>(tableName)),
                    _                          => -1
                };
            }
            catch { return -1; }
        }

        // ─────────────────────────────────────────────────────────────────────
        // RA_* alias table seeders
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Seeds RA_WELL_STATUS_TYPE — one alias row per WSC v3 STATUS_TYPE.
        /// STATUS_TYPE values match R_WELL_STATUS_TYPE exactly so FK constraints are satisfied.
        /// </summary>
        public async Task<int> SeedRaFacetTypesAsync(string userId = "SYSTEM")
        {
            var repo = BuildRepo<RA_WELL_STATUS_TYPE>("RA_WELL_STATUS_TYPE");
            int count = 0;

            foreach (var kv in WellServices.FACET_CATALOG)
            {
                var def = kv.Value;
                if (await ExistsAsync(repo, "STATUS_TYPE", def.StatusType)) continue;

                var abbr = Abbreviate(def.StatusType);
                var row = new RA_WELL_STATUS_TYPE
                {
                    STATUS_TYPE     = def.StatusType,
                    ALIAS_ID        = $"WST-{abbr}",
                    ALIAS_LONG_NAME = def.StatusType,
                    ABBREVIATION    = abbr,
                    ACTIVE_IND      = "Y",
                };
                await repo.InsertAsync(row, userId);
                count++;
            }
            return count;
        }

        /// <summary>
        /// Seeds RA_WELL_STATUS — one alias row per STATUS value per STATUS_TYPE.
        /// Both STATUS_TYPE and STATUS match R_WELL_STATUS exactly so FK constraints are satisfied.
        /// </summary>
        public async Task<int> SeedRaFacetValuesAsync(string userId = "SYSTEM")
        {
            var repo = BuildRepo<RA_WELL_STATUS>("RA_WELL_STATUS");
            int count = 0;

            foreach (var kv in WellServices.FACET_CATALOG)
            {
                var def = kv.Value;
                foreach (var val in def.Values)
                {
                    if (await ExistsAsync(repo, "STATUS_TYPE", def.StatusType, "STATUS", val.Status)) continue;

                    var row = new RA_WELL_STATUS
                    {
                        STATUS_TYPE     = def.StatusType,
                        STATUS          = val.Status,
                        ALIAS_ID        = $"WS-{Abbreviate(def.StatusType)}-{val.Abbreviation}",
                        ALIAS_LONG_NAME = val.Status,
                        ABBREVIATION    = val.Abbreviation,
                        ACTIVE_IND      = "Y",
                    };
                    await repo.InsertAsync(row, userId);
                    count++;
                }
            }
            return count;
        }

        /// <summary>Produces a short abbreviation from a multi-word label (max 10 chars).</summary>
        private static string Abbreviate(string label)
        {
            if (string.IsNullOrWhiteSpace(label)) return label;
            var words = label.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var abbrev = string.Concat(words.Select(w => char.ToUpperInvariant(w[0])));
            return abbrev.Length > 10 ? abbrev.Substring(0, 10) : abbrev;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Result / status DTOs
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Aggregate result from <see cref="WellStatusFacetSeeder.SeedAllAsync"/>.</summary>
    public class FacetSeedResult
    {
        public bool            Success            { get; set; }
        public string          Message            { get; set; } = string.Empty;
        // R_* core tables
        public int             FacetTypeRows      { get; set; }
        public int             FacetValueRows     { get; set; }
        public int             FacetQualifierRows { get; set; }
        public int             FacetQualValueRows { get; set; }
        // RA_* alias tables
        public int             RaFacetTypeRows    { get; set; }
        public int             RaFacetValueRows   { get; set; }
        public int             TotalInserted      => FacetTypeRows + FacetValueRows + FacetQualifierRows + FacetQualValueRows
                                                  + RaFacetTypeRows + RaFacetValueRows;
        public List<string>    Errors             { get; set; } = new();
    }

    /// <summary>Current row counts for all six reference tables.</summary>
    public class FacetSeedStatus
    {
        // R_* core
        public int  FacetTypeCount       { get; set; }
        public int  FacetValueCount      { get; set; }
        public int  QualifierCount       { get; set; }
        public int  QualifierValueCount  { get; set; }
        // RA_* alias
        public int  RaFacetTypeCount     { get; set; }
        public int  RaFacetValueCount    { get; set; }
        public bool IsSeeded             => FacetTypeCount > 0 && FacetValueCount > 0
                                        && RaFacetTypeCount > 0 && RaFacetValueCount > 0;
    }
}
