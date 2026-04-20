using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;

public class HSEKPIService : IHSEKPIService
{
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

        var tier1Count = await CountIncidentsAsync(fieldId, range, tier: 1);
        var tier2Count = await CountIncidentsAsync(fieldId, range, tier: 2);
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
            var t1     = await CountIncidentsAsync(fieldId, range, tier: 1);
            var t2     = await CountIncidentsAsync(fieldId, range, tier: 2);
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
            var meta       = await _metadata.GetTableMetadataAsync("PDEN_SOURCE");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                             ?? typeof(object);
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "PDEN_SOURCE");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FIELD_ID",     Operator = "=",  FilterValue = fieldId },
                new AppFilter { FieldName = "SOURCE_TYPE",  Operator = "=",  FilterValue = "EXPOSURE_HOURS" },
                new AppFilter { FieldName = "SOURCE_DATE",  Operator = ">=", FilterValue = range.Start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "SOURCE_DATE",  Operator = "<=", FilterValue = range.End.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND",   Operator = "=",  FilterValue = "Y" },
            };

            var rows = (await repo.GetAsync(filters)).ToList();
            return rows.Sum(r =>
            {
                var prop = r.GetType().GetProperty("SOURCE_QUANTITY");
                return double.TryParse(prop?.GetValue(r)?.ToString(), out var v) ? v : 0;
            });
        }
        catch
        {
            return 0;
        }
    }

    public async Task<double> GetTRIRAsync(string fieldId, DateRangeFilter range)
    {
        var exp = await GetExposureHoursAsync(fieldId, range);
        var rec = await CountInjuriesAsync(fieldId, range, null);
        return exp > 0 ? (rec * 1_000_000.0) / exp : 0;
    }

    private async Task<int> CountIncidentsAsync(string fieldId, DateRangeFilter range, int? tier)
    {
        var meta       = await _metadata.GetTableMetadataAsync("HSE_INCIDENT");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                         ?? typeof(object);
        var repo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, "HSE_INCIDENT");

        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FIELD_ID",     Operator = "=",  FilterValue = fieldId },
            new AppFilter { FieldName = "INCIDENT_DATE", Operator = ">=", FilterValue = range.Start.ToString("yyyy-MM-dd") },
            new AppFilter { FieldName = "INCIDENT_DATE", Operator = "<=", FilterValue = range.End.ToString("yyyy-MM-dd") },
            new AppFilter { FieldName = "ACTIVE_IND",   Operator = "=",  FilterValue = "Y" },
        };

        if (tier.HasValue)
            filters.Add(new AppFilter { FieldName = "INCIDENT_TIER", Operator = "=", FilterValue = tier.Value.ToString() });

        return (await repo.GetAsync(filters)).Count();
    }

    private async Task<int> CountInjuriesAsync(string fieldId, DateRangeFilter range, string? injuryType)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("HSE_INCIDENT_BA");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                             ?? typeof(object);
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "HSE_INCIDENT_BA");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVOLVEMENT_TYPE", Operator = "=", FilterValue = "INJURED" },
                new AppFilter { FieldName = "ACTIVE_IND",       Operator = "=", FilterValue = "Y" },
            };

            if (!string.IsNullOrEmpty(injuryType))
                filters.Add(new AppFilter { FieldName = "INJURY_TYPE", Operator = "=", FilterValue = injuryType });

            return (await repo.GetAsync(filters)).Count();
        }
        catch { return 0; }
    }
}
