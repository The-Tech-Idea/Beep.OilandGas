using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
public class RCAService : IRCAService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ICompanyFieldSelectionService _selections;
    private readonly IPPDMRowAccessPolicy _rowAccess;
    private FieldSelectionReview? _review;
    private readonly IHSEService _incidents;
    private readonly ILogger<RCAService> _logger;
    public RCAService(IDMEEditor editor, ICommonColumnHandler commonColumnHandler, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, ICompanyFieldSelectionService selections, IPPDMRowAccessPolicy rowAccess, IHSEService incidents, ILogger<RCAService> logger)
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

    public async Task<List<CauseFinding>> GetCauseChainAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await RequireIncidentAsync(incidentId);
        var repo = await BuildRepoAsync();
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
        var rows = (await repo.GetAsync(filters)).ToList();
        return await ConfirmAsync(rows.OfType<HSE_INCIDENT_CAUSE>().Select(cause => new CauseFinding(Seq: (int)cause.CAUSE_OBS_NO, CauseType: cause.CAUSE_TYPE ?? string.Empty, Category: cause.CAUSE_CODE ?? string.Empty, Description: cause.REMARK ?? string.Empty)).OrderBy(c => c.Seq).ToList());
    }

    public async Task AddCauseAsync(string incidentId, AddCauseRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        ArgumentNullException.ThrowIfNull(request);
        await RequireIncidentAsync(incidentId);
        var repo = await BuildRepoAsync();
        var entity = new HSE_INCIDENT_CAUSE
        {
            INCIDENT_ID = incidentId,
            CAUSE_OBS_NO = request.Seq,
            ACTIVE_IND = "Y",
            CAUSE_CODE = request.CauseCategory,
            CAUSE_TYPE = request.CauseType,
            EFFECTIVE_DATE = DateTime.UtcNow,
            REMARK = request.CauseDesc,
            SOURCE = "Beep.OilandGas.HSE",
        };
        await repo.InsertAsync(entity, userId);
        await ConfirmAsync();
    }

    public async Task UpdateCauseAsync(string incidentId, int causeSeq, UpdateCauseRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        ArgumentNullException.ThrowIfNull(request);
        await RequireIncidentAsync(incidentId);
        var repo = await BuildRepoAsync();
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
                FieldName = "CAUSE_OBS_NO",
                Operator = "=",
                FilterValue = causeSeq.ToString()
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var existing = (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_CAUSE>().FirstOrDefault();
        if (existing is null) throw RefusalException.NotFound("The reviewed HSE mutation target is unavailable.");

        existing.CAUSE_CODE = request.CauseCategory;
        existing.REMARK = request.CauseDesc;
        await repo.UpdateAsync(existing, userId);
        await ConfirmAsync();
    }

    public async Task<bool> IsRCACompleteAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await RequireIncidentAsync(incidentId);
        var causes = await GetCauseChainAsync(incidentId);
        return await ConfirmAsync(causes.Any(c => c.CauseType == CauseType.Root && !string.IsNullOrWhiteSpace(c.Description) && c.Description.Length >= 20));
    }

    private async Task<PPDMGenericRepository> BuildRepoAsync()
    {
        var meta = await _metadata.GetTableMetadataAsync("HSE_INCIDENT_CAUSE");
        if (meta is null || meta.EntityTypeName != "HSE_INCIDENT_CAUSE")
            throw RefusalException.Conflict("Canonical HSE metadata is unavailable.");
        var entityType = typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + meta.EntityTypeName) ?? throw RefusalException.Conflict("The canonical HSE entity is unavailable.");
        return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, await RequireCurrentBindingAsync(), "HSE_INCIDENT_CAUSE", rowAccess: _rowAccess);
    }
}
