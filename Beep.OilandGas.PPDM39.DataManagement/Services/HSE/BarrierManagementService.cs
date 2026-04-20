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

public class BarrierManagementService : IBarrierManagementService
{
    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<BarrierManagementService> _logger;

    public BarrierManagementService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<BarrierManagementService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    public async Task<List<BarrierRecord>> GetBarriersForIncidentAsync(string incidentId)
    {
        var repo = await BuildComponentRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "ACTIVE_IND",  Operator = "=", FilterValue = "Y" },
        };

        var rows = (await repo.GetAsync(filters)).ToList();
        var result = new List<BarrierRecord>();

        foreach (var row in rows)
        {
            var compType = GetStr(row, "COMPONENT_TYPE");
            if (!compType.StartsWith("BARRIER")) continue;

            var equipId = GetStr(row, "EQUIP_ID");
            var status = compType switch
            {
                "BARRIER_FAILED"    => BarrierStatus.Failed,
                "BARRIER_DEGRADED"  => BarrierStatus.Degraded,
                "BARRIER_EFFECTIVE" => BarrierStatus.Effective,
                _                   => BarrierStatus.NotApplicable,
            };

            result.Add(new BarrierRecord(
                EquipId:     equipId,
                BarrierName: GetStr(row, "COMPONENT_NOTES"),
                BarrierType: "PHYSICAL",
                BarrierSide: BarrierSide.Left,
                Status:      status,
                FailureDesc: GetStr(row, "COMPONENT_NOTES")));
        }

        return result;
    }

    public async Task AddBarrierAsync(string incidentId, AddBarrierRequest request, string userId)
    {
        var repo = await BuildComponentRepoAsync();
        var compType = request.BarrierSide == BarrierSide.Left
            ? "BARRIER_EFFECTIVE"
            : "BARRIER_EFFECTIVE";

        var entity = new
        {
            INCIDENT_ID      = incidentId,
            EQUIP_ID         = request.EquipId,
            COMPONENT_TYPE   = compType,
            COMPONENT_NOTES  = request.FailureDesc ?? string.Empty,
            ACTIVE_IND       = "Y",
        };

        await repo.InsertAsync(entity, userId);
    }

    public async Task SetBarrierStatusAsync(string incidentId, string equipId, string status, string userId)
    {
        var repo = await BuildComponentRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "EQUIP_ID",    Operator = "=", FilterValue = equipId    },
            new AppFilter { FieldName = "ACTIVE_IND",  Operator = "=", FilterValue = "Y"        },
        };

        var rows = (await repo.GetAsync(filters)).ToList();
        if (rows.Count == 0) return;

        var row = rows[0];
        var compType = status switch
        {
            BarrierStatus.Failed        => "BARRIER_FAILED",
            BarrierStatus.Degraded      => "BARRIER_DEGRADED",
            BarrierStatus.Effective     => "BARRIER_EFFECTIVE",
            _                           => "BARRIER_NOT_APPLICABLE",
        };
        SetStr(row, "COMPONENT_TYPE", compType);
        await repo.UpdateAsync(row, userId);
    }

    public async Task<BarrierSummary> GetBarrierSummaryAsync(string incidentId)
    {
        var barriers = await GetBarriersForIncidentAsync(incidentId);
        var failed    = barriers.Count(b => b.Status == BarrierStatus.Failed);
        var degraded  = barriers.Count(b => b.Status == BarrierStatus.Degraded);
        var effective = barriers.Count(b => b.Status == BarrierStatus.Effective);

        // API RP 754 Tier influence: ≥2 failed → suggest Tier 1
        var tierInfluence = failed >= 2 ? 1 : failed == 1 ? 2 : 3;

        return new BarrierSummary(
            TotalBarriers:      barriers.Count,
            FailedBarriers:     failed,
            DegradedBarriers:   degraded,
            EffectiveBarriers:  effective,
            API754TierInfluence: tierInfluence);
    }

    private async Task<PPDMGenericRepository> BuildComponentRepoAsync()
    {
        var meta       = await _metadata.GetTableMetadataAsync("HSE_INCIDENT_COMPONENT");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                         ?? typeof(object);
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, "HSE_INCIDENT_COMPONENT");
    }

    private static string GetStr(object o, string p)
    {
        var prop = o.GetType().GetProperty(p);
        return prop?.GetValue(o)?.ToString() ?? string.Empty;
    }
    private static void SetStr(object o, string p, string v)
    {
        var prop = o.GetType().GetProperty(p);
        prop?.SetValue(o, v);
    }
}
