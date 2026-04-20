using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using Serilog;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;

public class HSEIncidentService : IHSEService
{
    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<HSEIncidentService> _logger;

    // Regulatory notification auto-creation table
    private static readonly Dictionary<(string Jurisdiction, string IncidentType, int MaxTier), string> _obligTypes =
        new()
        {
            { ("USA",           IncidentType.PseTier1, 1), "BSEE_REPORT"     },
            { ("USA",           IncidentType.Spill,    4), "NRC_REPORT"      },
            { ("CANADA",        IncidentType.Injury,   4), "AER_INCIDENT_REPORT" },
            { ("INTERNATIONAL", IncidentType.PseTier1, 1), "OSPAR_REPORT"   },
            { ("INTERNATIONAL", IncidentType.PseTier2, 2), "OSPAR_REPORT"   },
        };

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
        var seq      = DateTime.UtcNow.Ticks % 10000;
        var id       = $"HSE-{request.FieldId}-{request.IncidentDate:yyyyMMdd}-{seq:D4}";

        var record = new HSEIncidentRecord
        {
            IncidentId   = id,
            FieldId      = request.FieldId,
            IncidentType = request.IncidentType,
            Tier         = request.Tier,
            IncidentDate = request.IncidentDate,
            Location     = request.Location,
            Description  = request.Description,
            Jurisdiction = request.Jurisdiction,
            CurrentState = IncidentState.Reported,
        };

        var repo = await BuildRepoAsync("HSE_INCIDENT");
        await repo.InsertAsync(record, userId);

        Log.Information("HSE incident {Id} reported for field {Field}", id, request.FieldId);
        return record;
    }

    // ── Assign Investigator ────────────────────────────────────────────────────
    public async Task AssignInvestigatorAsync(string incidentId, string baId, string userId)
    {
        var existing = await GetByIdAsync(incidentId)
            ?? throw new InvalidOperationException($"Incident {incidentId} not found");

        existing.InvestigatorId = baId;
        var repo = await BuildRepoAsync("HSE_INCIDENT");
        await repo.UpdateAsync(existing, userId);
    }

    // ── Query ──────────────────────────────────────────────────────────────────
    public async Task<List<HSEIncidentRecord>> GetFieldIncidentsAsync(string fieldId, DateRangeFilter? range)
    {
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FIELD_ID",   Operator = "=", FilterValue = fieldId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        if (range is not null)
        {
            filters.Add(new AppFilter { FieldName = "INCIDENT_DATE", Operator = ">=",
                FilterValue = range.Start.ToString("yyyy-MM-dd") });
            filters.Add(new AppFilter { FieldName = "INCIDENT_DATE", Operator = "<=",
                FilterValue = range.End.ToString("yyyy-MM-dd") });
        }

        var repo    = await BuildRepoAsync("HSE_INCIDENT");
        var results = (await repo.GetAsync(filters)).ToList();
        return results.OfType<HSEIncidentRecord>().ToList();
    }

    public async Task<HSEIncidentRecord?> GetByIdAsync(string incidentId)
    {
        var repo = await BuildRepoAsync("HSE_INCIDENT");
        var result = await repo.GetByIdAsync(incidentId);
        return result as HSEIncidentRecord;
    }

    // ── Update Tier ────────────────────────────────────────────────────────────
    public async Task UpdateTierAsync(string incidentId, int tier, string userId)
    {
        var existing = await GetByIdAsync(incidentId)
            ?? throw new InvalidOperationException($"Incident {incidentId} not found");

        existing.Tier = tier;
        var repo = await BuildRepoAsync("HSE_INCIDENT");
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

        incident.CurrentState = transition.To;
        var repo = await BuildRepoAsync("HSE_INCIDENT");
        await repo.UpdateAsync(incident, userId);

        // Regulatory notification on investigation start
        if (trigger == "investigate")
            await CreateRegulatoryObligationsAsync(incident, userId);

        Log.Information("Incident {Id} transitioned via {Trigger} → {State}", incidentId, trigger, transition.To);
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
    private async Task CreateRegulatoryObligationsAsync(HSEIncidentRecord incident, string userId)
    {
        try
        {
            foreach (var rule in _obligTypes)
            {
                var (jurisdiction, incType, maxTier) = rule.Key;
                if (incident.Jurisdiction != jurisdiction) continue;
                if (incident.IncidentType != incType && incType != IncidentType.PseTier1) continue;
                if (incident.Tier > maxTier) continue;

                // Insert into OBLIGATION table
                var oblRepo = await BuildRepoAsync("OBLIGATION");
                var oblig = new
                {
                    OBLIGATION_ID   = $"OBLIG-{incident.IncidentId}-{rule.Value}",
                    FIELD_ID        = incident.FieldId,
                    INCIDENT_ID     = incident.IncidentId,
                    OBLIG_TYPE      = rule.Value,
                    DUE_DATE        = DateTime.UtcNow.AddHours(24),
                    OBLIG_STATUS    = "OPEN",
                    ACTIVE_IND      = "Y",
                };
                Log.Information("Regulatory obligation {Type} created for incident {Id}", rule.Value, incident.IncidentId);
                break;
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to create regulatory obligation for incident {Id}", incident.IncidentId);
        }
    }

    private async Task<PPDMGenericRepository> BuildRepoAsync(string tableName)
    {
        var meta       = await _metadata.GetTableMetadataAsync(tableName);
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                         ?? typeof(HSEIncidentRecord);
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, tableName);
    }
}
