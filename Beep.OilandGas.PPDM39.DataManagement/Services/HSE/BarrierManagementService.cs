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

public class BarrierManagementService : IBarrierManagementService
{
    private const string PrimaryFieldComponentRole = "PRIMARY_FIELD_CONTEXT";

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

        return (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_COMPONENT>()
            .Where(IsBarrierComponent)
            .OrderBy(component => component.COMPONENT_OBS_NO)
            .Select(component => new BarrierRecord(
                EquipId: component.EQUIPMENT_ID ?? string.Empty,
                BarrierName: !string.IsNullOrWhiteSpace(component.EQUIPMENT_ID) ? component.EQUIPMENT_ID : component.COMPONENT_TYPE ?? string.Empty,
                BarrierType: component.COMPONENT_TYPE ?? string.Empty,
                BarrierSide: component.COMPONENT_ROLE ?? BarrierSide.Left,
                Status: string.IsNullOrWhiteSpace(component.INCIDENT_TYPE) ? BarrierStatus.NotApplicable : component.INCIDENT_TYPE,
                FailureDesc: component.REMARK))
            .ToList();
    }

    public async Task AddBarrierAsync(string incidentId, AddBarrierRequest request, string userId)
    {
        var repo = await BuildComponentRepoAsync();
        var primaryFieldComponent = await GetPrimaryFieldComponentAsync(incidentId);
        var nextObsNo = await GetNextComponentObsNoAsync(incidentId);

        var entity = new HSE_INCIDENT_COMPONENT
        {
            INCIDENT_ID = incidentId,
            COMPONENT_OBS_NO = nextObsNo,
            ACTIVE_IND = "Y",
            COMPONENT_ROLE = request.BarrierSide,
            COMPONENT_TYPE = request.BarrierType,
            EQUIPMENT_ID = request.EquipId,
            FIELD_ID = primaryFieldComponent?.FIELD_ID,
            INCIDENT_TYPE = BarrierStatus.Effective,
            JURISDICTION = primaryFieldComponent?.JURISDICTION,
            REMARK = request.FailureDesc,
        };

        await repo.InsertAsync(entity, userId);
    }

    public async Task SetBarrierStatusAsync(string incidentId, string equipId, string status, string userId)
    {
        var repo = await BuildComponentRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "EQUIPMENT_ID", Operator = "=", FilterValue = equipId    },
            new AppFilter { FieldName = "ACTIVE_IND",  Operator = "=", FilterValue = "Y"        },
        };

        var row = (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_COMPONENT>()
            .FirstOrDefault(IsBarrierComponent);
        if (row is null) return;

        row.INCIDENT_TYPE = status;
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

    private async Task<HSE_INCIDENT_COMPONENT?> GetPrimaryFieldComponentAsync(string incidentId)
    {
        var repo = await BuildComponentRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_COMPONENT>()
            .FirstOrDefault(component => string.Equals(component.COMPONENT_ROLE, PrimaryFieldComponentRole, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<decimal> GetNextComponentObsNoAsync(string incidentId)
    {
        var repo = await BuildComponentRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var maxObsNo = (await repo.GetAsync(filters))
            .OfType<HSE_INCIDENT_COMPONENT>()
            .Select(component => component.COMPONENT_OBS_NO)
            .DefaultIfEmpty(0)
            .Max();

        return maxObsNo + 1;
    }

    private static bool IsBarrierComponent(HSE_INCIDENT_COMPONENT component)
    {
        return string.Equals(component.COMPONENT_ROLE, BarrierSide.Left, StringComparison.OrdinalIgnoreCase)
            || string.Equals(component.COMPONENT_ROLE, BarrierSide.Right, StringComparison.OrdinalIgnoreCase);
    }
}
