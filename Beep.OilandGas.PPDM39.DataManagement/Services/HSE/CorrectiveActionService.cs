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

public class CorrectiveActionService : ICorrectiveActionService
{
    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<CorrectiveActionService> _logger;

    public CorrectiveActionService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<CorrectiveActionService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    // Create a CA project linked to the incident
    public async Task<string> CreateCAPlanAsync(string incidentId, string userId)
    {
        var planId = $"CA-{incidentId}";
        var repo   = await BuildProjectRepoAsync();

        var entity = new
        {
            PROJECT_ID   = planId,
            PROJECT_NAME = $"Corrective Actions for {incidentId}",
            PROJECT_TYPE = "CORRECTIVE_ACTION",
            STATUS       = "ACTIVE",
            INCIDENT_ID  = incidentId,
            ACTIVE_IND   = "Y",
        };

        await repo.InsertAsync(entity, userId);
        return planId;
    }

    public async Task<string> AddCorrectiveActionAsync(string incidentId, AddCARequest request, string userId)
    {
        var planId  = $"CA-{incidentId}";
        var repo    = await BuildStepRepoAsync();
        var existing = await GetCAStatusAsync(incidentId);
        var nextSeq  = existing.Count + 1;
        var stepId   = $"{planId}-{nextSeq:D3}";

        var entity = new
        {
            PROJECT_ID  = planId,
            STEP_SEQ    = nextSeq,
            STEP_NAME   = request.CADescription,
            STEP_TYPE   = request.CAType,
            DUE_DATE    = request.DueDate.ToString("yyyy-MM-dd"),
            STEP_STATUS = "OPEN",
            ACTIVE_IND  = "Y",
        };

        await repo.InsertAsync(entity, userId);

        if (!string.IsNullOrEmpty(request.ResponsibleBaId))
            await AssignResponsiblePersonAsync(incidentId, nextSeq, request.ResponsibleBaId, userId);

        return stepId;
    }

    public async Task AssignResponsiblePersonAsync(string incidentId, int stepSeq, string baId, string userId)
    {
        var planId = $"CA-{incidentId}";
        var repo   = await BuildStepBaRepoAsync();

        var entity = new
        {
            PROJECT_ID   = planId,
            STEP_SEQ     = stepSeq,
            BA_ID        = baId,
            ROLE_CODE    = "RESPONSIBLE",
            ACTIVE_IND   = "Y",
        };

        await repo.InsertAsync(entity, userId);
    }

    public async Task SetDueDateAsync(string incidentId, int stepSeq, DateTime dueDate, string userId)
    {
        var planId = $"CA-{incidentId}";
        var repo   = await BuildStepRepoAsync();
        var id     = $"{planId}|{stepSeq}";
        var row    = await repo.GetByIdAsync(id);
        if (row is null) return;

        SetStr(row, "DUE_DATE", dueDate.ToString("yyyy-MM-dd"));
        await repo.UpdateAsync(row, userId);
    }

    public async Task RecordCompletionAsync(string incidentId, int stepSeq, string completionNotes, string userId)
    {
        var planId = $"CA-{incidentId}";
        var repo   = await BuildStepRepoAsync();
        var id     = $"{planId}|{stepSeq}";
        var row    = await repo.GetByIdAsync(id);
        if (row is null) return;

        SetStr(row, "STEP_STATUS",    "COMPLETED");
        SetStr(row, "COMPLETION_NOTES", completionNotes);
        SetStr(row, "ACTUAL_DATE",    DateTime.UtcNow.ToString("yyyy-MM-dd"));
        await repo.UpdateAsync(row, userId);
    }

    public async Task<bool> AllCAsMeetDeadlineAsync(string incidentId)
    {
        var statuses = await GetCAStatusAsync(incidentId);
        return statuses.All(ca => !ca.IsOverdue && ca.Status == "COMPLETED");
    }

    public async Task<List<CAStatus>> GetCAStatusAsync(string incidentId)
    {
        var planId = $"CA-{incidentId}";
        var repo   = await BuildStepRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = planId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"   },
        };

        var rows   = (await repo.GetAsync(filters)).ToList();
        var result = new List<CAStatus>();
        var now    = DateTime.UtcNow;

        foreach (var row in rows)
        {
            var dueStr  = GetStr(row, "DUE_DATE");
            var due     = DateTime.TryParse(dueStr, out var d) ? d : now.AddDays(30);
            var status  = GetStr(row, "STEP_STATUS");
            var completedStr = GetStr(row, "ACTUAL_DATE");
            DateTime? completedDate = DateTime.TryParse(completedStr, out var cd) ? cd : null;

            result.Add(new CAStatus(
                StepSeq:          GetInt(row, "STEP_SEQ"),
                Description:      GetStr(row, "STEP_NAME"),
                CAType:           GetStr(row, "STEP_TYPE"),
                Status:           status,
                DueDate:          due,
                IsOverdue:        status != "COMPLETED" && due < now,
                ResponsiblePerson: null,
                CompletedDate:    completedDate));
        }

        return result.OrderBy(c => c.StepSeq).ToList();
    }

    private async Task<PPDMGenericRepository> BuildProjectRepoAsync()
    {
        var meta       = await _metadata.GetTableMetadataAsync("PROJECT");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                         ?? typeof(object);
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, "PROJECT");
    }

    private async Task<PPDMGenericRepository> BuildStepRepoAsync()
    {
        var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                         ?? typeof(object);
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, "PROJECT_STEP");
    }

    private async Task<PPDMGenericRepository> BuildStepBaRepoAsync()
    {
        var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP_BA");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                         ?? typeof(object);
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, "PROJECT_STEP_BA");
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
