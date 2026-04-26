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

public class RCAService : IRCAService
{
    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<RCAService>       _logger;

    public RCAService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<RCAService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    public async Task<List<CauseFinding>> GetCauseChainAsync(string incidentId)
    {
        var repo = await BuildRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "ACTIVE_IND",  Operator = "=", FilterValue = "Y" },
        };

        var rows = (await repo.GetAsync(filters)).ToList();
        return rows
        .OfType<HSE_INCIDENT_CAUSE>()
        .Select(cause => new CauseFinding(
            Seq: (int)cause.CAUSE_OBS_NO,
            CauseType: cause.CAUSE_TYPE ?? string.Empty,
            Category: cause.CAUSE_CODE ?? string.Empty,
            Description: cause.REMARK ?? string.Empty))
        .OrderBy(c => c.Seq)
        .ToList();
    }

    public async Task AddCauseAsync(string incidentId, AddCauseRequest request, string userId)
    {
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
    }

    public async Task UpdateCauseAsync(string incidentId, int causeSeq, UpdateCauseRequest request, string userId)
    {
        var repo = await BuildRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = incidentId },
            new AppFilter { FieldName = "CAUSE_OBS_NO", Operator = "=", FilterValue = causeSeq.ToString() },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var existing = (await repo.GetAsync(filters)).OfType<HSE_INCIDENT_CAUSE>().FirstOrDefault();
        if (existing is null) return;

        existing.CAUSE_CODE = request.CauseCategory;
        existing.REMARK = request.CauseDesc;
        await repo.UpdateAsync(existing, userId);
    }

    public async Task<bool> IsRCACompleteAsync(string incidentId)
    {
        var causes = await GetCauseChainAsync(incidentId);
        return causes.Any(c => c.CauseType == CauseType.Root
                            && !string.IsNullOrWhiteSpace(c.Description)
                            && c.Description.Length >= 20);
    }

    private async Task<PPDMGenericRepository> BuildRepoAsync()
    {
        var meta       = await _metadata.GetTableMetadataAsync("HSE_INCIDENT_CAUSE");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                         ?? typeof(object);
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, "HSE_INCIDENT_CAUSE");
    }

}
