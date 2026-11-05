using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
public class HSEKPIService : IHSEKPIService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ICompanyFieldSelectionService _selections;
    private readonly IPPDMRowAccessPolicy _rowAccess;
    private FieldSelectionReview? _review;
    private readonly ILogger<HSEKPIService> _logger;
    public HSEKPIService(IDMEEditor editor, ICommonColumnHandler commonColumnHandler, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, ICompanyFieldSelectionService selections, IPPDMRowAccessPolicy rowAccess, ILogger<HSEKPIService> logger)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _selections = selections ?? throw new ArgumentNullException(nameof(selections));
        _rowAccess = rowAccess ?? throw new ArgumentNullException(nameof(rowAccess));
        _logger = logger;
    }

    // The existing profile owns authority; this is only a fence for one phase request.
    private async Task<string> RequireCurrentBindingAsync(string? fieldId = null, string? actor = null, bool requireField = true)
    {
        var current = await _selections.GetCurrentFieldSelectionAsync();
        if (current.SelectionContext is not { } review)
            throw RefusalException.Conflict("The company FIELD review is unavailable.");
        FieldSelectionReviewValidator.RequireCurrent(current, review.Company, review.ActorUserId);
        if (requireField && !current.SelectionAvailable || fieldId is not null && fieldId != current.FieldId || actor is not null && actor != review.ActorUserId || _review is not null && _review != review)
            throw RefusalException.Conflict("The HSE account, company, database or FIELD differs from its reviewed request.");
        _review = review;
        return review.Database.ConnectionName;
    }

    private async Task<T> ConfirmAsync<T>(T result)
    {
        await RequireCurrentBindingAsync();
        return result;
    }

    private async Task ConfirmAsync()
    {
        await RequireCurrentBindingAsync();
    }

    public async Task<HSEKPISet> GetKPIsAsync(string fieldId, DateRangeFilter range)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            throw RefusalException.Invalid("fieldId is required.");
        await RequireCurrentBindingAsync(fieldId, null, true);
        var exposureHours = await GetExposureHoursAsync(fieldId, range);
        var tier1Count = await CountIncidentsAsync(fieldId, range, tier: 1);
        var tier2Count = await CountIncidentsAsync(fieldId, range, tier: 2);
        var ltis = await CountInjuriesAsync(fieldId, range, "LTI");
        var fatalities = await CountInjuriesAsync(fieldId, range, "FATALITY");
        var recordable = await CountInjuriesAsync(fieldId, range, null); // all injury types
        double safeExp = exposureHours > 0 ? exposureHours : 1;
        double tier1Rate = (tier1Count * 1_000_000.0) / safeExp;
        double tier2Rate = (tier2Count * 1_000_000.0) / safeExp;
        double trir = (recordable * 1_000_000.0) / safeExp;
        double ltif = (ltis * 1_000_000.0) / safeExp;
        double fatRate = (fatalities * 100_000_000.0) / safeExp;
        return await ConfirmAsync(new HSEKPISet(Tier1PSERate: tier1Rate, Tier2PSERate: tier2Rate, TRIR: trir, LTIF: ltif, FatalityRate: fatRate, CAOnTimeRate: 0, // requires CA service join — stub
 BarrierDegradationRate: 0, // requires barrier service — stub
 HAZOPClosureRate: 0, // requires HAZOP service — stub
 ExposureHours: exposureHours, Period: range));
    }

    public async Task<List<TierRateTrend>> GetTierRateTrendAsync(string fieldId, int months)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            throw RefusalException.Invalid("fieldId is required.");
        await RequireCurrentBindingAsync(fieldId, null, true);
        if (months <= 0) throw RefusalException.Invalid("A positive HSE trend interval is required.");
        var result = new List<TierRateTrend>();
        var end = DateTime.UtcNow;
        for (var i = months - 1; i >= 0; i--)
        {
            var periodStart = new DateTime(end.Year, end.Month, 1).AddMonths(-i);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);
            var range = new DateRangeFilter(periodStart, periodEnd);
            var exp = await GetExposureHoursAsync(fieldId, range);
            var t1 = await CountIncidentsAsync(fieldId, range, tier: 1);
            var t2 = await CountIncidentsAsync(fieldId, range, tier: 2);
            if (exp <= 0)
                throw RefusalException.Conflict("A positive observed HSE exposure denominator is required.");
            double s = exp;
            result.Add(new TierRateTrend(Month: periodStart.ToString("MMM-yy"), Tier1Rate: Math.Round((t1 * 1_000_000.0) / s, 3), Tier2Rate: Math.Round((t2 * 1_000_000.0) / s, 3)));
        }

        return await ConfirmAsync(result);
    }

    public async Task<double> GetExposureHoursAsync(string fieldId, DateRangeFilter range)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            throw RefusalException.Invalid("fieldId is required.");
        await RequireCurrentBindingAsync(fieldId, null, true);
        throw RefusalException.Conflict("The approved HSE exposure-hours source is unavailable; rates cannot be calculated.");
    }

    public async Task<double> GetTRIRAsync(string fieldId, DateRangeFilter range)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            throw RefusalException.Invalid("fieldId is required.");
        await RequireCurrentBindingAsync(fieldId, null, true);
        var exp = await GetExposureHoursAsync(fieldId, range);
        var rec = await CountInjuriesAsync(fieldId, range, null);
        return await ConfirmAsync(exp > 0 ? (rec * 1_000_000.0) / exp : 0);
    }

    private async Task<int> CountIncidentsAsync(string fieldId, DateRangeFilter range, int? tier)
    {
        var incidentIds = await GetFieldIncidentIdsAsync(fieldId);
        if (incidentIds.Count == 0)
        {
            return 0;
        }

        var incidentRepo = await BuildRepo<HSE_INCIDENT>("HSE_INCIDENT");
        var incidentFilters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_DATE",
                Operator = ">=",
                FilterValue = range.Start.ToString("yyyy-MM-dd")
            },
            new AppFilter
            {
                FieldName = "INCIDENT_DATE",
                Operator = "<=",
                FilterValue = range.End.ToString("yyyy-MM-dd")
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var incidentsInRange = (await incidentRepo.GetAsync(incidentFilters)).OfType<HSE_INCIDENT>().Where(incident => incidentIds.Contains(incident.INCIDENT_ID)).Select(incident => incident.INCIDENT_ID).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (incidentsInRange.Count == 0)
        {
            return 0;
        }

        if (!tier.HasValue)
        {
            return incidentsInRange.Count;
        }

        var detailRepo = await BuildRepo<HSE_INCIDENT_DETAIL>("HSE_INCIDENT_DETAIL");
        var detailFilters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var incidentsForTier = (await detailRepo.GetAsync(detailFilters)).OfType<HSE_INCIDENT_DETAIL>().Where(detail => incidentsInRange.Contains(detail.INCIDENT_ID)).GroupBy(detail => detail.INCIDENT_ID).Select(group => group.OrderBy(detail => detail.DETAIL_TYPE == "PRIMARY" ? 0 : 1).ThenBy(detail => detail.DETAIL_OBS_NO).First()).Count(detail => MapSeverityIdToTier(detail.INCIDENT_SEVERITY_ID) == tier.Value);
        return incidentsForTier;
    }

    private async Task<int> CountInjuriesAsync(string fieldId, DateRangeFilter range, string? injuryType)
    {
        var incidentsInRange = await GetIncidentIdsForRangeAsync(fieldId, range);
        if (incidentsInRange.Count == 0)
        {
            return 0;
        }

        var repo = await BuildRepo<HSE_INCIDENT_BA>("HSE_INCIDENT_BA");
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var injuries = (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_BA>().Where(ba => incidentsInRange.Contains(ba.INCIDENT_ID)).Where(ba => !string.IsNullOrWhiteSpace(ba.INVOLVED_BA_ROLE) && ba.INVOLVED_BA_ROLE.Contains("INJURED", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(injuryType))
        {
            injuries = injuries.Where(ba => string.Equals(ba.INVOLVED_BA_STATUS, injuryType, StringComparison.OrdinalIgnoreCase));
        }

        return injuries.Count();
    }

    private async Task<HashSet<string>> GetFieldIncidentIdsAsync(string fieldId)
    {
        await _rowAccess.RequireCompleteReadScopeAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", fieldId);
        var repo = await BuildRepo<HSE_INCIDENT_COMPONENT>("HSE_INCIDENT_COMPONENT");
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "FIELD_ID",
                Operator = "=",
                FilterValue = fieldId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var result = (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_COMPONENT>().Select(component => component.INCIDENT_ID).Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        await _rowAccess.RequireCompleteReadScopeAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", fieldId);
        return result;
    }

    private async Task<HashSet<string>> GetIncidentIdsForRangeAsync(string fieldId, DateRangeFilter range)
    {
        var incidentIds = await GetFieldIncidentIdsAsync(fieldId);
        if (incidentIds.Count == 0)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var repo = await BuildRepo<HSE_INCIDENT>("HSE_INCIDENT");
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_DATE",
                Operator = ">=",
                FilterValue = range.Start.ToString("yyyy-MM-dd")
            },
            new AppFilter
            {
                FieldName = "INCIDENT_DATE",
                Operator = "<=",
                FilterValue = range.End.ToString("yyyy-MM-dd")
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        return (await repo.GetAsync(filters)).OfType<HSE_INCIDENT>().Select(incident => incident.INCIDENT_ID).Where(id => !string.IsNullOrWhiteSpace(id) && incidentIds.Contains(id)).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<PPDMGenericRepository> BuildRepo<TEntity>(string tableName) where TEntity : class
        => new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(TEntity), await RequireCurrentBindingAsync(), tableName, rowAccess: _rowAccess);

    private static int MapSeverityIdToTier(string? severityId)
    {
        if (string.IsNullOrWhiteSpace(severityId))
        {
            return 4;
        }

        if (severityId.StartsWith("TIER_", StringComparison.OrdinalIgnoreCase) && int.TryParse(severityId[5..], out var parsedTier))
        {
            return parsedTier;
        }

        return int.TryParse(severityId, out parsedTier) ? parsedTier : 4;
    }
}
