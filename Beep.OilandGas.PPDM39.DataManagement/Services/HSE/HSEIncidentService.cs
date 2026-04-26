using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;

public class HSEIncidentService : IHSEService
{
    private const string PrimaryDetailType = "PRIMARY";
    private const string PrimaryFieldComponentRole = "PRIMARY_FIELD_CONTEXT";
    private const string PrimaryFieldComponentType = "INCIDENT_LOCATION";
    private const string InvestigatorRole = "INVESTIGATOR";
    private const string InvestigatorStatus = "ASSIGNED";
    private const string StatusResponseType = "STATUS";

    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<HSEIncidentService> _logger;

    private static readonly Dictionary<string, List<string>> _transitions = new()
    {
        [IncidentState.Reported]                = new() { "investigate"   },
        [IncidentState.UnderInvestigation]      = new() { "rca_start", "cancel" },
        [IncidentState.PendingRCA]              = new() { "rca_complete", "cancel" },
        [IncidentState.PendingCorrectiveAction] = new() { "ca_done",     "cancel" },
        [IncidentState.PendingCloseOut]         = new() { "close"                 },
        [IncidentState.Closed]                  = new List<string>(),
        [IncidentState.Cancelled]               = new List<string>(),
    };

    private static readonly Dictionary<string, (string From, string To)> _triggerMap = new()
    {
        ["investigate"]  = (IncidentState.Reported,                IncidentState.UnderInvestigation),
        ["rca_start"]    = (IncidentState.UnderInvestigation,      IncidentState.PendingRCA),
        ["rca_complete"] = (IncidentState.PendingRCA,              IncidentState.PendingCorrectiveAction),
        ["ca_done"]      = (IncidentState.PendingCorrectiveAction,  IncidentState.PendingCloseOut),
        ["close"]        = (IncidentState.PendingCloseOut,          IncidentState.Closed),
        ["cancel"]       = ("ANY",                                  IncidentState.Cancelled),
    };

    public HSEIncidentService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<HSEIncidentService> logger)
    {
        _editor             = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults           = defaults;
        _metadata           = metadata;
        _connectionName     = connectionName;
        _logger             = logger;
    }

    // ── Create ─────────────────────────────────────────────────────────────────
    public async Task<HSEIncidentRecord> ReportIncidentAsync(ReportIncidentRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(request.FieldId))
        {
            throw new ArgumentException("Field ID is required", nameof(request));
        }

        var incidentId = BuildIncidentId(request.FieldId, request.IncidentDate);
        var incidentRepo = await BuildRepoAsync("HSE_INCIDENT");
        var detailRepo = await BuildRepoAsync("HSE_INCIDENT_DETAIL");
        var componentRepo = await BuildRepoAsync("HSE_INCIDENT_COMPONENT");
        var responseRepo = await BuildRepoAsync("HSE_INCIDENT_RESPONSE");

        var header = new HSE_INCIDENT
        {
            INCIDENT_ID = incidentId,
            ACTIVE_IND = "Y",
            EFFECTIVE_DATE = request.IncidentDate,
            INCIDENT_DATE = request.IncidentDate,
            LOST_TIME_IND = request.IncidentType == IncidentType.Injury ? "N" : null,
            REPORTED_BY_BA_ID = userId,
            REPORTED_BY_NAME = userId,
            REPORTED_IND = "Y",
            REMARK = request.Description,
            SOURCE = "Beep.OilandGas.HSE",
            WORK_RELATED_IND = "Y",
        };

        var detail = new HSE_INCIDENT_DETAIL
        {
            INCIDENT_ID = incidentId,
            DETAIL_OBS_NO = 1,
            ACTIVE_IND = "Y",
            DETAIL_TYPE = PrimaryDetailType,
            EFFECTIVE_DATE = request.IncidentDate,
            INCIDENT_DATE = request.IncidentDate,
            INCIDENT_SEVERITY_ID = MapTierToSeverityId(request.Tier),
            INCIDENT_SET_ID = "FIELD_INCIDENT",
            INCIDENT_TYPE_ID = request.IncidentType,
            REMARK = request.Description,
            SOURCE = "Beep.OilandGas.HSE",
        };

        var component = new HSE_INCIDENT_COMPONENT
        {
            INCIDENT_ID = incidentId,
            COMPONENT_OBS_NO = 1,
            ACTIVE_IND = "Y",
            COMPONENT_ROLE = PrimaryFieldComponentRole,
            COMPONENT_TYPE = PrimaryFieldComponentType,
            FIELD_ID = request.FieldId,
            INCIDENT_TYPE = request.IncidentType,
            JURISDICTION = request.Jurisdiction,
            REMARK = request.Location,
        };

        var response = new HSE_INCIDENT_RESPONSE
        {
            INCIDENT_ID = incidentId,
            RESPONSE_OBS_NO = 1,
            ACTIVE_IND = "Y",
            EFFECTIVE_DATE = DateTime.UtcNow,
            REMARK = "Incident reported",
            RESPONSE_RESULT = IncidentState.Reported,
            RESPONSE_TYPE = StatusResponseType,
            SOURCE = "Beep.OilandGas.HSE",
        };

        await incidentRepo.InsertAsync(header, userId);
        await detailRepo.InsertAsync(detail, userId);
        await componentRepo.InsertAsync(component, userId);
        await responseRepo.InsertAsync(response, userId);

        _logger.LogInformation("HSE incident {IncidentId} reported for field {FieldId}", incidentId, request.FieldId);

        return MapIncidentRecord(header, detail, component, null, response);
    }

    // ── Assign Investigator ────────────────────────────────────────────────────
    public async Task AssignInvestigatorAsync(string incidentId, string baId, string userId)
    {
        var incident = await GetIncidentHeaderAsync(incidentId)
            ?? throw new InvalidOperationException($"Incident {incidentId} not found");

        var repo = await BuildRepoAsync("HSE_INCIDENT_BA");
        var investigator = await GetInvestigatorAsync(incidentId);

        if (investigator is null)
        {
            investigator = new HSE_INCIDENT_BA
            {
                INCIDENT_ID = incident.INCIDENT_ID,
                PARTY_OBS_NO = await GetNextPartyObsNoAsync(incidentId),
                PARTY_ROLE_OBS_NO = 1,
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = DateTime.UtcNow,
                INVOLVED_BA_ROLE = InvestigatorRole,
                INVOLVED_BA_STATUS = InvestigatorStatus,
                INVOLVED_PARTY_BA_ID = baId,
                REMARK = "Lead investigator",
                SOURCE = "Beep.OilandGas.HSE",
            };

            await repo.InsertAsync(investigator, userId);
            return;
        }

        investigator.INVOLVED_BA_ROLE = InvestigatorRole;
        investigator.INVOLVED_BA_STATUS = InvestigatorStatus;
        investigator.INVOLVED_PARTY_BA_ID = baId;
        investigator.REMARK = "Lead investigator";
        await repo.UpdateAsync(investigator, userId);
    }

    // ── Query ──────────────────────────────────────────────────────────────────
    public async Task<List<HSEIncidentRecord>> GetFieldIncidentsAsync(string fieldId, DateRangeFilter? range)
    {
        var componentRepo = await BuildRepoAsync("HSE_INCIDENT_COMPONENT");
        var componentFilters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FIELD_ID",   Operator = "=", FilterValue = fieldId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var incidentIds = (await componentRepo.GetAsync(componentFilters))
            .OfType<HSE_INCIDENT_COMPONENT>()
            .Select(component => component.INCIDENT_ID)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (incidentIds.Count == 0)
        {
            return new List<HSEIncidentRecord>();
        }

        var incidentFilters = new List<AppFilter>
        {
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        if (range is not null)
        {
            incidentFilters.Add(new AppFilter { FieldName = "INCIDENT_DATE", Operator = ">=",
                FilterValue = range.Start.ToString("yyyy-MM-dd") });
            incidentFilters.Add(new AppFilter { FieldName = "INCIDENT_DATE", Operator = "<=",
                FilterValue = range.End.ToString("yyyy-MM-dd") });
        }

        var incidentRepo = await BuildRepoAsync("HSE_INCIDENT");
        var incidents = (await incidentRepo.GetAsync(incidentFilters))
            .OfType<HSE_INCIDENT>()
            .Where(incident => incidentIds.Contains(incident.INCIDENT_ID))
            .OrderByDescending(incident => incident.INCIDENT_DATE ?? DateTime.MinValue)
            .ToList();

        var records = new List<HSEIncidentRecord>(incidents.Count);
        foreach (var incident in incidents)
        {
            var record = await BuildIncidentRecordAsync(incident.INCIDENT_ID);
            if (record is not null)
            {
                records.Add(record);
            }
        }

        return records;
    }

    public async Task<HSEIncidentRecord?> GetByIdAsync(string incidentId)
    {
        return await BuildIncidentRecordAsync(incidentId);
    }

    // ── Update Tier ────────────────────────────────────────────────────────────
    public async Task UpdateTierAsync(string incidentId, int tier, string userId)
    {
        var existing = await GetPrimaryDetailAsync(incidentId)
            ?? throw new InvalidOperationException($"Incident {incidentId} not found");

        existing.INCIDENT_SEVERITY_ID = MapTierToSeverityId(tier);
        var repo = await BuildRepoAsync("HSE_INCIDENT_DETAIL");
        await repo.UpdateAsync(existing, userId);
    }

    // ── Transition ─────────────────────────────────────────────────────────────
    public async Task<bool> TransitionAsync(string incidentId, string trigger, string? reason, string userId)
    {
        var incident = await GetByIdAsync(incidentId)
            ?? throw new InvalidOperationException($"Incident {incidentId} not found");

        if (!_triggerMap.TryGetValue(trigger, out var transition))
            return false;

        if (transition.From != "ANY" && incident.CurrentState != transition.From)
            return false;

        var responseRepo = await BuildRepoAsync("HSE_INCIDENT_RESPONSE");
        var nextObsNo = await GetNextResponseObsNoAsync(incidentId);
        var response = new HSE_INCIDENT_RESPONSE
        {
            INCIDENT_ID = incidentId,
            RESPONSE_OBS_NO = nextObsNo,
            ACTIVE_IND = "Y",
            EFFECTIVE_DATE = DateTime.UtcNow,
            REMARK = string.IsNullOrWhiteSpace(reason) ? trigger : reason,
            RESPONSE_RESULT = transition.To,
            RESPONSE_TYPE = StatusResponseType,
            SOURCE = "Beep.OilandGas.HSE",
        };

        await responseRepo.InsertAsync(response, userId);

        _logger.LogInformation("Incident {IncidentId} transitioned via {Trigger} to {State}", incidentId, trigger, transition.To);
        return true;
    }

    public async Task<List<string>> GetAvailableTriggersAsync(string incidentId)
    {
        var incident = await GetByIdAsync(incidentId);
        if (incident == null)
            return new List<string>();

        if (_transitions.TryGetValue(incident.CurrentState, out var triggers))
            return new List<string>(triggers);

        return new List<string>();
    }

    // ── Internal helpers ───────────────────────────────────────────────────────
    private async Task<HSEIncidentRecord?> BuildIncidentRecordAsync(string incidentId)
    {
        var header = await GetIncidentHeaderAsync(incidentId);
        if (header is null)
        {
            return null;
        }

        var detail = await GetPrimaryDetailAsync(incidentId);
        var component = await GetPrimaryFieldComponentAsync(incidentId);
        var investigator = await GetInvestigatorAsync(incidentId);
        var response = await GetLatestStatusResponseAsync(incidentId);

        return MapIncidentRecord(header, detail, component, investigator, response);
    }

    private async Task<HSE_INCIDENT?> GetIncidentHeaderAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT");
        return await repo.GetByIdAsync(incidentId) as HSE_INCIDENT;
    }

    private async Task<HSE_INCIDENT_DETAIL?> GetPrimaryDetailAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_DETAIL");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_DETAIL>()
            .OrderBy(detail => detail.DETAIL_TYPE == PrimaryDetailType ? 0 : 1)
            .ThenBy(detail => detail.DETAIL_OBS_NO)
            .FirstOrDefault();
    }

    private async Task<HSE_INCIDENT_COMPONENT?> GetPrimaryFieldComponentAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_COMPONENT");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_COMPONENT>()
            .OrderBy(component => component.COMPONENT_ROLE == PrimaryFieldComponentRole ? 0 : 1)
            .ThenBy(component => component.COMPONENT_OBS_NO)
            .FirstOrDefault(component =>
                component.COMPONENT_ROLE == PrimaryFieldComponentRole ||
                !string.IsNullOrWhiteSpace(component.FIELD_ID));
    }

    private async Task<HSE_INCIDENT_BA?> GetInvestigatorAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_BA");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_BA>()
            .FirstOrDefault(ba => string.Equals(ba.INVOLVED_BA_ROLE, InvestigatorRole, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<HSE_INCIDENT_RESPONSE?> GetLatestStatusResponseAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_RESPONSE");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_RESPONSE>()
            .Where(response => string.Equals(response.RESPONSE_TYPE, StatusResponseType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(response => response.EFFECTIVE_DATE ?? DateTime.MinValue)
            .ThenByDescending(response => response.RESPONSE_OBS_NO)
            .FirstOrDefault();
    }

    private async Task<decimal> GetNextPartyObsNoAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_BA");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var maxObsNo = (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_BA>()
            .Select(ba => ba.PARTY_OBS_NO)
            .DefaultIfEmpty(0)
            .Max();

        return maxObsNo + 1;
    }

    private async Task<decimal> GetNextResponseObsNoAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT_RESPONSE");
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var maxObsNo = (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_RESPONSE>()
            .Select(response => response.RESPONSE_OBS_NO)
            .DefaultIfEmpty(0)
            .Max();

        return maxObsNo + 1;
    }

    private static HSEIncidentRecord MapIncidentRecord(
        HSE_INCIDENT header,
        HSE_INCIDENT_DETAIL? detail,
        HSE_INCIDENT_COMPONENT? component,
        HSE_INCIDENT_BA? investigator,
        HSE_INCIDENT_RESPONSE? response)
    {
        return new HSEIncidentRecord
        {
            IncidentId = header.INCIDENT_ID ?? string.Empty,
            FieldId = component?.FIELD_ID ?? string.Empty,
            IncidentType = detail?.INCIDENT_TYPE_ID ?? component?.INCIDENT_TYPE ?? string.Empty,
            Tier = MapSeverityIdToTier(detail?.INCIDENT_SEVERITY_ID),
            IncidentDate = header.INCIDENT_DATE ?? detail?.INCIDENT_DATE ?? header.EFFECTIVE_DATE ?? DateTime.UtcNow,
            Location = component?.REMARK ?? string.Empty,
            Description = detail?.REMARK ?? header.REMARK ?? string.Empty,
            Jurisdiction = component?.JURISDICTION ?? string.Empty,
            CurrentState = response?.RESPONSE_RESULT ?? IncidentState.Reported,
            InvestigatorId = investigator?.INVOLVED_PARTY_BA_ID,
        };
    }

    private static string BuildIncidentId(string fieldId, DateTime incidentDate)
    {
        var seq = DateTime.UtcNow.Ticks % 10000;
        return $"HSE-{fieldId}-{incidentDate:yyyyMMdd}-{seq:D4}";
    }

    private static string MapTierToSeverityId(int tier)
    {
        var normalizedTier = Math.Clamp(tier, 1, 4);
        return $"TIER_{normalizedTier}";
    }

    private static int MapSeverityIdToTier(string? severityId)
    {
        if (string.IsNullOrWhiteSpace(severityId))
        {
            return 4;
        }

        if (severityId.StartsWith("TIER_", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(severityId[5..], out var parsedTier))
        {
            return parsedTier;
        }

        if (int.TryParse(severityId, out parsedTier))
        {
            return parsedTier;
        }

        return severityId.ToUpperInvariant() switch
        {
            IncidentType.PseTier1 => 1,
            IncidentType.PseTier2 => 2,
            _ => 4,
        };
    }

    private async Task<PPDMGenericRepository> BuildRepoAsync(string tableName)
    {
        var meta       = await _metadata.GetTableMetadataAsync(tableName);
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                         ?? typeof(object);
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, tableName);
    }
}
