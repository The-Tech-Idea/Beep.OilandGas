using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;

public class HSEKPIService : IHSEKPIService
{
    private const string ExposureHoursSourceTable = "PDEN_SOURCE";

    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<HSEKPIService>    _logger;

    public HSEKPIService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<HSEKPIService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    public async Task<HSEKPISet> GetKPIsAsync(string fieldId, DateRangeFilter range)
    {
        var exposureHours = await GetExposureHoursAsync(fieldId, range);

        var incidents  = await CountIncidentsAsync(fieldId, range.Start, range.End);
        var tier1Count = incidents.Tier1;
        var tier2Count = incidents.Tier2;
        var ltis       = await CountInjuriesAsync(fieldId, range, "LTI");
        var fatalities = await CountInjuriesAsync(fieldId, range, "FATALITY");
        var recordable = await CountInjuriesAsync(fieldId, range, null);  // all injury types

        double safeExp = exposureHours > 0 ? exposureHours : 1;

        double tier1Rate = (tier1Count * 1_000_000.0) / safeExp;
        double tier2Rate = (tier2Count * 1_000_000.0) / safeExp;
        double trir      = (recordable  * 1_000_000.0) / safeExp;
        double ltif      = (ltis        * 1_000_000.0) / safeExp;
        double fatRate   = (fatalities  * 100_000_000.0) / safeExp;

        return new HSEKPISet(
            Tier1PSERate:          tier1Rate,
            Tier2PSERate:          tier2Rate,
            TRIR:                  trir,
            LTIF:                  ltif,
            FatalityRate:          fatRate,
            CAOnTimeRate:          0,   // requires CA service join — stub
            BarrierDegradationRate: 0,  // requires barrier service — stub
            HAZOPClosureRate:      0,   // requires HAZOP service — stub
            ExposureHours:         exposureHours,
            Period:                range);
    }

    public async Task<List<TierRateTrend>> GetTierRateTrendAsync(string fieldId, int months)
    {
        var result = new List<TierRateTrend>();
        var end    = DateTime.UtcNow;

        for (var i = months - 1; i >= 0; i--)
        {
            var periodStart = new DateTime(end.Year, end.Month, 1).AddMonths(-i);
            var periodEnd   = periodStart.AddMonths(1).AddDays(-1);
            var range       = new DateRangeFilter(periodStart, periodEnd);

            var exp    = await GetExposureHoursAsync(fieldId, range);
            var counts = await CountIncidentsAsync(fieldId, range.Start, range.End);
            var t1     = counts.Tier1;
            var t2     = counts.Tier2;
            double s   = exp > 0 ? exp : 1;

            result.Add(new TierRateTrend(
                Month:     periodStart.ToString("MMM-yy"),
                Tier1Rate: Math.Round((t1 * 1_000_000.0) / s, 3),
                Tier2Rate: Math.Round((t2 * 1_000_000.0) / s, 3)));
        }

        return result;
    }

    public async Task<double> GetExposureHoursAsync(string fieldId, DateRangeFilter range)
    {
        try
        {
            _ = await _metadata.GetTableMetadataAsync(ExposureHoursSourceTable);
            _logger.LogWarning(
                "Exposure hours requested for field {FieldId} between {Start} and {End}, but {TableName} is not mapped to a verified generated PPDM model in this workspace. Returning 0 until a supported PPDM denominator source is identified.",
                fieldId,
                range.Start,
                range.End,
                ExposureHoursSourceTable);

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Exposure hours requested for field {FieldId} between {Start} and {End}, but {TableName} is not available from PPDM metadata. Returning 0.",
                fieldId,
                range.Start,
                range.End,
                ExposureHoursSourceTable);
            return 0;
        }
    }

    public async Task<double> GetTRIRAsync(string fieldId, DateRangeFilter range)
    {
        var exp = await GetExposureHoursAsync(fieldId, range);
        var rec = await CountInjuriesAsync(fieldId, range, null);
        return exp > 0 ? (rec * 1_000_000.0) / exp : 0;
    }

    /// <summary>
    /// The one count of HSE incidents: active incidents — in the field when one is named (through its HSE_INCIDENT_COMPONENT
    /// rows), in every field otherwise — dated within the range when one is given; how many are open and how many are in each
    /// tier, by <see cref="HSEIncidentRules"/>.
    /// </summary>
    public async Task<IncidentCounts> CountIncidentsAsync(string? fieldId, DateTime? start, DateTime? end)
    {
        HashSet<string>? fieldIncidentIds = null;
        if (!string.IsNullOrWhiteSpace(fieldId))
        {
            fieldIncidentIds = await GetFieldIncidentIdsAsync(fieldId);
            if (fieldIncidentIds.Count == 0)
            {
                return new IncidentCounts(0, 0, 0, 0, 0, 0);
            }
        }

        var incidentFilters = new List<AppFilter>
        {
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };
        if (start.HasValue)
        {
            incidentFilters.Add(new AppFilter { FieldName = "INCIDENT_DATE", Operator = ">=", FilterValue = start.Value.ToString("yyyy-MM-dd") });
        }
        if (end.HasValue)
        {
            incidentFilters.Add(new AppFilter { FieldName = "INCIDENT_DATE", Operator = "<=", FilterValue = end.Value.ToString("yyyy-MM-dd") });
        }

        var incidentIds = (await BuildRepo<HSE_INCIDENT>("HSE_INCIDENT").GetAsync(incidentFilters))
            .OfType<HSE_INCIDENT>()
            .Select(incident => incident.INCIDENT_ID)
            .Where(id => !string.IsNullOrWhiteSpace(id) && (fieldIncidentIds is null || fieldIncidentIds.Contains(id)))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (incidentIds.Count == 0)
        {
            return new IncidentCounts(0, 0, 0, 0, 0, 0);
        }

        var activeFilter = new List<AppFilter>
        {
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var detailsByIncident = (await BuildRepo<HSE_INCIDENT_DETAIL>("HSE_INCIDENT_DETAIL").GetAsync(activeFilter))
            .OfType<HSE_INCIDENT_DETAIL>()
            .Where(detail => incidentIds.Contains(detail.INCIDENT_ID))
            .ToLookup(detail => detail.INCIDENT_ID, StringComparer.OrdinalIgnoreCase);

        var responsesByIncident = (await BuildRepo<HSE_INCIDENT_RESPONSE>("HSE_INCIDENT_RESPONSE").GetAsync(activeFilter))
            .OfType<HSE_INCIDENT_RESPONSE>()
            .Where(response => incidentIds.Contains(response.INCIDENT_ID))
            .ToLookup(response => response.INCIDENT_ID, StringComparer.OrdinalIgnoreCase);

        var tiers = incidentIds
            .Select(id => HSEIncidentRules.TierOf(HSEIncidentRules.PrimaryDetail(detailsByIncident[id])?.INCIDENT_SEVERITY_ID))
            .ToList();
        var open = incidentIds
            .Count(id => HSEIncidentRules.IsOpen(HSEIncidentRules.StateOf(HSEIncidentRules.LatestStatus(responsesByIncident[id]))));

        return new IncidentCounts(
            Total: incidentIds.Count,
            Open:  open,
            Tier1: tiers.Count(tier => tier == 1),
            Tier2: tiers.Count(tier => tier == 2),
            Tier3: tiers.Count(tier => tier == 3),
            Tier4: tiers.Count(tier => tier == HSEIncidentRules.UnrecordedTier));
    }

    private async Task<int> CountInjuriesAsync(string fieldId, DateRangeFilter range, string? injuryType)
    {
        try
        {
            var incidentsInRange = await GetIncidentIdsForRangeAsync(fieldId, range);
            if (incidentsInRange.Count == 0)
            {
                return 0;
            }

            var repo = BuildRepo<HSE_INCIDENT_BA>("HSE_INCIDENT_BA");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND",       Operator = "=", FilterValue = "Y" },
            };

            var injuries = (await repo.GetAsync(filters))
                .OfType<HSE_INCIDENT_BA>()
                .Where(ba => incidentsInRange.Contains(ba.INCIDENT_ID))
                .Where(ba => !string.IsNullOrWhiteSpace(ba.INVOLVED_BA_ROLE)
                    && ba.INVOLVED_BA_ROLE.Contains("INJURED", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(injuryType))
            {
                injuries = injuries.Where(ba => string.Equals(ba.INVOLVED_BA_STATUS, injuryType, StringComparison.OrdinalIgnoreCase));
            }

            return injuries.Count();
        }
        catch { return 0; }
    }

    private async Task<HashSet<string>> GetFieldIncidentIdsAsync(string fieldId)
    {
        var repo = BuildRepo<HSE_INCIDENT_COMPONENT>("HSE_INCIDENT_COMPONENT");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_COMPONENT>()
            .Select(component => component.INCIDENT_ID)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<HashSet<string>> GetIncidentIdsForRangeAsync(string fieldId, DateRangeFilter range)
    {
        var incidentIds = await GetFieldIncidentIdsAsync(fieldId);
        if (incidentIds.Count == 0)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var repo = BuildRepo<HSE_INCIDENT>("HSE_INCIDENT");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_DATE", Operator = ">=", FilterValue = range.Start.ToString("yyyy-MM-dd") },
            new AppFilter { FieldName = "INCIDENT_DATE", Operator = "<=", FilterValue = range.End.ToString("yyyy-MM-dd") },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT>()
            .Select(incident => incident.INCIDENT_ID)
            .Where(id => !string.IsNullOrWhiteSpace(id) && incidentIds.Contains(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private PPDMGenericRepository BuildRepo<TEntity>(string tableName)
        where TEntity : class
        => new(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(TEntity), _connectionName, tableName);
}
