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

        // Map generic objects to CauseFinding via property reflection (dynamic type)
        return rows.Select(r =>
        {
            int    seq  = GetInt(r, "CAUSE_SEQ");
            string type = GetStr(r, "CAUSE_TYPE");
            string cat  = GetStr(r, "CAUSE_CATEGORY");
            string desc = GetStr(r, "CAUSE_DESC");
            return new CauseFinding(seq, type, cat, desc);
        })
        .OrderBy(c => c.Seq)
        .ToList();
    }

    public async Task AddCauseAsync(string incidentId, AddCauseRequest request, string userId)
    {
        var repo = await BuildRepoAsync();

        var entity = new
        {
            INCIDENT_ID    = incidentId,
            CAUSE_SEQ      = request.Seq,
            CAUSE_TYPE     = request.CauseType,
            CAUSE_DESC     = request.CauseDesc,
            CAUSE_CATEGORY = request.CauseCategory,
            ACTIVE_IND     = "Y",
        };

        await repo.InsertAsync(entity, userId);
    }

    public async Task UpdateCauseAsync(string incidentId, int causeSeq, UpdateCauseRequest request, string userId)
    {
        var repo = await BuildRepoAsync();
        var id   = $"{incidentId}|{causeSeq}";
        var existing = await repo.GetByIdAsync(id);
        if (existing is null) return;

        SetStr(existing, "CAUSE_DESC",     request.CauseDesc);
        SetStr(existing, "CAUSE_CATEGORY", request.CauseCategory);
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

    private static int    GetInt(object o, string p) => int.TryParse(GetStr(o, p), out var v) ? v : 0;
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
