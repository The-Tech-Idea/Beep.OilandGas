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
public class BarrierManagementService : IBarrierManagementService
{
    private const string PrimaryFieldComponentRole = "PRIMARY_FIELD_CONTEXT";
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ICompanyFieldSelectionService _selections;
    private readonly IPPDMRowAccessPolicy _rowAccess;
    private FieldSelectionReview? _review;
    private readonly IHSEService _incidents;
    private readonly ILogger<BarrierManagementService> _logger;
    public BarrierManagementService(IDMEEditor editor, ICommonColumnHandler commonColumnHandler, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, ICompanyFieldSelectionService selections, IPPDMRowAccessPolicy rowAccess, IHSEService incidents, ILogger<BarrierManagementService> logger)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _selections = selections ?? throw new ArgumentNullException(nameof(selections));
        _rowAccess = rowAccess ?? throw new ArgumentNullException(nameof(rowAccess));
        _incidents = incidents ?? throw new ArgumentNullException(nameof(incidents));
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

    private async Task RequireIncidentAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var incident = await _incidents.GetByIdAsync(incidentId);
        if (incident is null || incident.IncidentId != incidentId || incident.FieldId != _review!.FieldId)
            throw RefusalException.NotFound("The incident is not recorded in the reviewed HSE FIELD.");
        await RequireCurrentBindingAsync();
    }

    public async Task<List<BarrierRecord>> GetBarriersForIncidentAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await RequireIncidentAsync(incidentId);
        var repo = await BuildComponentRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_ID",
                Operator = "=",
                FilterValue = incidentId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        return await ConfirmAsync((await repo.GetAsync(filters)).OfType<HSE_INCIDENT_COMPONENT>().Where(IsBarrierComponent).OrderBy(component => component.COMPONENT_OBS_NO).Select(component => new BarrierRecord(EquipId: component.EQUIPMENT_ID ?? string.Empty, BarrierName: !string.IsNullOrWhiteSpace(component.EQUIPMENT_ID) ? component.EQUIPMENT_ID : component.COMPONENT_TYPE ?? string.Empty, BarrierType: component.COMPONENT_TYPE ?? string.Empty, BarrierSide: component.COMPONENT_ROLE ?? BarrierSide.Left, Status: string.IsNullOrWhiteSpace(component.INCIDENT_TYPE) ? BarrierStatus.NotApplicable : component.INCIDENT_TYPE, FailureDesc: component.REMARK)).ToList());
    }

    public async Task AddBarrierAsync(string incidentId, AddBarrierRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        ArgumentNullException.ThrowIfNull(request);
        await RequireIncidentAsync(incidentId);
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
        await ConfirmAsync();
    }

    public async Task SetBarrierStatusAsync(string incidentId, string equipId, string status, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await RequireIncidentAsync(incidentId);
        var repo = await BuildComponentRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_ID",
                Operator = "=",
                FilterValue = incidentId
            },
            new AppFilter
            {
                FieldName = "EQUIPMENT_ID",
                Operator = "=",
                FilterValue = equipId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var row = (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_COMPONENT>().FirstOrDefault(IsBarrierComponent);
        if (row is null)
        {
            await ConfirmAsync();
            return;
        }

        row.INCIDENT_TYPE = status;
        await repo.UpdateAsync(row, userId);
        await ConfirmAsync();
    }

    public async Task<BarrierSummary> GetBarrierSummaryAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await RequireIncidentAsync(incidentId);
        var barriers = await GetBarriersForIncidentAsync(incidentId);
        var failed = barriers.Count(b => b.Status == BarrierStatus.Failed);
        var degraded = barriers.Count(b => b.Status == BarrierStatus.Degraded);
        var effective = barriers.Count(b => b.Status == BarrierStatus.Effective);
        // API RP 754 Tier influence: ≥2 failed → suggest Tier 1
        var tierInfluence = failed >= 2 ? 1 : failed == 1 ? 2 : 3;
        return await ConfirmAsync(new BarrierSummary(TotalBarriers: barriers.Count, FailedBarriers: failed, DegradedBarriers: degraded, EffectiveBarriers: effective, API754TierInfluence: tierInfluence));
    }

    private async Task<PPDMGenericRepository> BuildComponentRepoAsync()
    {
        var meta = await _metadata.GetTableMetadataAsync("HSE_INCIDENT_COMPONENT");
        if (meta is null || meta.EntityTypeName != "HSE_INCIDENT_COMPONENT")
            throw RefusalException.Conflict("Canonical HSE metadata is unavailable.");
        var entityType = typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + meta.EntityTypeName) ?? throw RefusalException.Conflict("The canonical HSE entity is unavailable.");
        return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, await RequireCurrentBindingAsync(), "HSE_INCIDENT_COMPONENT", rowAccess: _rowAccess);
    }

    private async Task<HSE_INCIDENT_COMPONENT?> GetPrimaryFieldComponentAsync(string incidentId)
    {
        var repo = await BuildComponentRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_ID",
                Operator = "=",
                FilterValue = incidentId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        return (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_COMPONENT>().FirstOrDefault(component => string.Equals(component.COMPONENT_ROLE, PrimaryFieldComponentRole, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<decimal> GetNextComponentObsNoAsync(string incidentId)
    {
        var repo = await BuildComponentRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "INCIDENT_ID",
                Operator = "=",
                FilterValue = incidentId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var maxObsNo = (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_COMPONENT>().Select(component => component.COMPONENT_OBS_NO).DefaultIfEmpty(0).Max();
        return maxObsNo + 1;
    }

    private static bool IsBarrierComponent(HSE_INCIDENT_COMPONENT component)
    {
        return string.Equals(component.COMPONENT_ROLE, BarrierSide.Left, StringComparison.OrdinalIgnoreCase) || string.Equals(component.COMPONENT_ROLE, BarrierSide.Right, StringComparison.OrdinalIgnoreCase);
    }
}
