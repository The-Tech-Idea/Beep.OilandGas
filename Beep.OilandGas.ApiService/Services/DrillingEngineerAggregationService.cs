using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using PpdmEntities= Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Aggregates drilling engineering KPIs: well counts, the drilling programmes' AFE budget against their reported cost,
    /// and the non-productive time reported this month.
    /// </summary>
    /// <remarks>
    /// Each figure is read from the record that holds it and is null when nothing records it (2026-10-10). It had answered
    /// AFE budget, spend and NPT as 0, which nothing computed, and every well as producing. The AFE figures and NPT come from
    /// the drilling programme (DRILLING_PROGRAM: its AFE cost) and its daily drilling reports (DRILLING_DAILY_REPORT: daily
    /// and cumulative cost, NPT hours).
    /// </remarks>
    public class DrillingEngineerAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<DrillingEngineerAggregationService>? _logger;

        public DrillingEngineerAggregationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<DrillingEngineerAggregationService>? logger = null)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _connectionName = connectionName;
            _logger = logger;
        }

        private PPDMGenericRepository GetRepo<T>(string tableName) =>
            new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(T), _connectionName, tableName);

        /// <summary>
        /// Gets drilling engineer KPIs for dashboard display, scoped to a field when one is named.
        /// </summary>
        public async Task<DrillingEngineerKpi> GetKpiAsync(string? fieldId = null)
        {
            var kpi = new DrillingEngineerKpi();

            // The field was accepted and never applied: every field's wells were counted for any field asked about.
            var wells = (await GetRepo<PpdmEntities.WELL>("WELL").GetAsync(ActiveIn("ASSIGNED_FIELD", fieldId)))
                .OfType<PpdmEntities.WELL>().ToList();
            kpi.TotalWells = wells.Count;
            // A well is producing when its current status is the canonical PRODUCING code; this had been every active
            // well, the ones being drilled included.
            kpi.ProducingWells = wells.Count(w =>
                string.Equals(w.CURRENT_STATUS?.Trim(), WellReferenceCodes.WellStatus.Producing, StringComparison.OrdinalIgnoreCase));

            var programs = (await GetRepo<DRILLING_PROGRAM>("DRILLING_PROGRAM").GetAsync(ActiveIn("FIELD_ID", fieldId)))
                .OfType<DRILLING_PROGRAM>()
                .Where(p => !string.IsNullOrWhiteSpace(p.PROGRAM_ID))
                .ToList();
            if (programs.Count == 0)
            {
                return kpi;
            }

            // A daily report carries no field: it belongs to the field of the programme it reports on.
            var programIds = programs.Select(p => p.PROGRAM_ID).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var reports = (await GetRepo<DRILLING_DAILY_REPORT>("DRILLING_DAILY_REPORT").GetAsync(ActiveIn(null, null)))
                .OfType<DRILLING_DAILY_REPORT>()
                .Where(r => !string.IsNullOrWhiteSpace(r.PROGRAM_ID) && programIds.Contains(r.PROGRAM_ID))
                .ToLookup(r => r.PROGRAM_ID, StringComparer.OrdinalIgnoreCase);

            ApplyAfeFigures(kpi, programs, reports);

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var nptThisMonth = reports
                .SelectMany(programReports => programReports)
                .Where(r => r.REPORT_DATE >= monthStart && r.NPT_HOURS.HasValue)
                .Select(r => r.NPT_HOURS!.Value)
                .ToList();
            kpi.NptHours = nptThisMonth.Count == 0 ? null : nptThisMonth.Sum();
            return kpi;
        }

        /// <summary>
        /// The AFE budget is the AFE cost of the programmes that carry one; the spend is those programmes' reported cost to
        /// date. Both stay null when no programme records them, and when the programmes' AFE currencies differ, since
        /// amounts in different currencies do not add up.
        /// </summary>
        private static void ApplyAfeFigures(
            DrillingEngineerKpi kpi,
            List<DRILLING_PROGRAM> programs,
            ILookup<string, DRILLING_DAILY_REPORT> reports)
        {
            var budgeted = programs.Where(p => p.AFE_COST.HasValue).ToList();
            if (budgeted.Count == 0)
            {
                return;
            }

            var currencies = budgeted
                .Select(p => p.AFE_CURRENCY?.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (currencies.Count > 1)
            {
                return;
            }

            kpi.AfeCurrency = currencies.SingleOrDefault();
            kpi.AfeBudget = budgeted.Sum(p => p.AFE_COST!.Value);

            var spent = budgeted
                .Select(p => CostToDate(reports[p.PROGRAM_ID]))
                .Where(cost => cost.HasValue)
                .Select(cost => cost!.Value)
                .ToList();
            kpi.AfeSpent = spent.Count == 0 ? null : spent.Sum();
        }

        /// <summary>
        /// A programme's reported cost to date: its latest report's cumulative cost, else the sum of its daily costs; null
        /// when its reports record neither.
        /// </summary>
        private static decimal? CostToDate(IEnumerable<DRILLING_DAILY_REPORT> programReports)
        {
            var reports = programReports.ToList();
            var latestCumulative = reports
                .Where(r => r.CUMULATIVE_COST.HasValue)
                .OrderByDescending(r => r.REPORT_DATE ?? DateTime.MinValue)
                .ThenByDescending(r => r.REPORT_NUMBER)
                .FirstOrDefault();
            if (latestCumulative is not null)
            {
                return latestCumulative.CUMULATIVE_COST;
            }

            var daily = reports.Where(r => r.DAILY_COST.HasValue).Select(r => r.DAILY_COST!.Value).ToList();
            return daily.Count == 0 ? null : daily.Sum();
        }

        private static List<AppFilter> ActiveIn(string? fieldColumn, string? fieldId)
        {
            var filters = new List<AppFilter> { new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };
            if (fieldColumn is not null && !string.IsNullOrWhiteSpace(fieldId))
            {
                filters.Add(new AppFilter { FieldName = fieldColumn, Operator = "=", FilterValue = fieldId });
            }

            return filters;
        }
    }

    /// <summary>Drilling engineer dashboard KPI data. A null figure is one nothing records.</summary>
    public class DrillingEngineerKpi
    {
        public int TotalWells { get; set; }

        /// <summary>Active wells whose current status is PRODUCING.</summary>
        public int ProducingWells { get; set; }

        /// <summary>The AFE cost of the drilling programmes that carry one.</summary>
        public decimal? AfeBudget { get; set; }

        /// <summary>Those programmes' reported cost to date, from their daily drilling reports.</summary>
        public decimal? AfeSpent { get; set; }

        /// <summary>The programmes' AFE currency, when they record one (the same for all of them).</summary>
        public string? AfeCurrency { get; set; }

        /// <summary>Non-productive hours on this calendar month's daily drilling reports (UTC).</summary>
        public decimal? NptHours { get; set; }
    }
}
