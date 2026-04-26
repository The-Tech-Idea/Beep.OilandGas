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

public class CorrectiveActionService : ICorrectiveActionService
{
    private const string CorrectiveActionProjectType = "CORRECTIVE_ACTION";
    private const string CorrectiveActionStatusType = "CA_STEP";
    private const string OpenStatus = "OPEN";
    private const string CompletedStatus = "COMPLETED";
    private const string ResponsibleRole = "RESPONSIBLE";

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
        var repo = await BuildProjectRepoAsync();
        var existing = await repo.GetByIdAsync(planId) as PROJECT;
        if (existing is not null)
        {
            return planId;
        }

        var entity = new PROJECT
        {
            PROJECT_ID = planId,
            ACTIVE_IND = "Y",
            DESCRIPTION = $"Corrective action plan for incident {incidentId}",
            EFFECTIVE_DATE = DateTime.UtcNow,
            PROJECT_NAME = $"Corrective Actions for {incidentId}",
            PROJECT_NUM = incidentId,
            PROJECT_TYPE = CorrectiveActionProjectType,
            REMARK = $"Linked incident: {incidentId}",
            SOURCE = "Beep.OilandGas.HSE",
            START_DATE = DateTime.UtcNow,
        };

        await repo.InsertAsync(entity, userId);
        return planId;
    }

    public async Task<string> AddCorrectiveActionAsync(string incidentId, AddCARequest request, string userId)
    {
        var planId = await CreateCAPlanAsync(incidentId, userId);
        var repo = await BuildStepRepoAsync();
        var steps = await GetStepsAsync(planId);
        var nextSeq = (steps.Count > 0 ? (int)steps.Max(step => step.STEP_SEQ_NO) : 0) + 1;
        var stepId = $"{planId}-STEP-{nextSeq:D3}";

        var entity = new PROJECT_STEP
        {
            PROJECT_ID = planId,
            STEP_ID = stepId,
            ACTIVE_IND = "Y",
            DESCRIPTION = request.CADescription,
            DUE_DATE = request.DueDate,
            EFFECTIVE_DATE = DateTime.UtcNow,
            REMARK = request.CADescription,
            SOURCE = "Beep.OilandGas.HSE",
            STEP_NAME = request.CADescription,
            STEP_SEQ_NO = nextSeq,
            STEP_TYPE = request.CAType,
        };

        await repo.InsertAsync(entity, userId);
        await AddStepStatusAsync(planId, stepId, OpenStatus, request.CADescription, userId);

        if (!string.IsNullOrEmpty(request.ResponsibleBaId))
            await AssignResponsiblePersonAsync(incidentId, nextSeq, request.ResponsibleBaId, userId);

        return stepId;
    }

    public async Task AssignResponsiblePersonAsync(string incidentId, int stepSeq, string baId, string userId)
    {
        var planId = $"CA-{incidentId}";
        var step = await GetStepAsync(planId, stepSeq);
        if (step is null)
        {
            return;
        }

        var repo = await BuildStepBaRepoAsync();
        var existing = await GetResponsibleAssignmentAsync(planId, step.STEP_ID);

        if (existing is not null)
        {
            existing.BUSINESS_ASSOCIATE_ID = baId;
            existing.REMARK = "Responsible person";
            await repo.UpdateAsync(existing, userId);
            return;
        }

        var entity = new PROJECT_STEP_BA
        {
            PROJECT_ID = planId,
            BUSINESS_ASSOCIATE_ID = baId,
            ROLE = ResponsibleRole,
            ROLE_SEQ_NO = 1,
            STEP_ID = step.STEP_ID,
            STEP_BA_OBS_NO = await GetNextStepBaObsNoAsync(planId, step.STEP_ID),
            ACTIVE_IND = "Y",
            ACTUAL_IND = "Y",
            EFFECTIVE_DATE = DateTime.UtcNow,
            PLAN_IND = "Y",
            REMARK = "Responsible person",
            SOURCE = "Beep.OilandGas.HSE",
        };

        await repo.InsertAsync(entity, userId);
    }

    public async Task SetDueDateAsync(string incidentId, int stepSeq, DateTime dueDate, string userId)
    {
        var planId = $"CA-{incidentId}";
        var repo = await BuildStepRepoAsync();
        var step = await GetStepAsync(planId, stepSeq);
        if (step is null) return;

        step.DUE_DATE = dueDate;
        await repo.UpdateAsync(step, userId);
    }

    public async Task RecordCompletionAsync(string incidentId, int stepSeq, string completionNotes, string userId)
    {
        var planId = $"CA-{incidentId}";
        var repo = await BuildStepRepoAsync();
        var step = await GetStepAsync(planId, stepSeq);
        if (step is null) return;

        step.ACTUAL_END_DATE = DateTime.UtcNow;
        step.REMARK = string.IsNullOrWhiteSpace(completionNotes)
            ? step.REMARK
            : completionNotes;
        await repo.UpdateAsync(step, userId);

        await AddStepStatusAsync(planId, step.STEP_ID, CompletedStatus, completionNotes, userId);
    }

    public async Task<bool> AllCAsMeetDeadlineAsync(string incidentId)
    {
        var statuses = await GetCAStatusAsync(incidentId);
        return statuses.All(ca => !ca.IsOverdue && ca.Status == "COMPLETED");
    }

    public async Task<List<CAStatus>> GetCAStatusAsync(string incidentId)
    {
        var planId = $"CA-{incidentId}";
        var steps = await GetStepsAsync(planId);
        var statuses = await GetLatestStepStatusesAsync(planId);
        var responsiblePeople = await GetResponsiblePeopleAsync(planId);
        var now = DateTime.UtcNow;

        return steps
            .OrderBy(step => step.STEP_SEQ_NO)
            .Select(step =>
            {
                var stepStatus = statuses.GetValueOrDefault(step.STEP_ID);
                var status = stepStatus?.STATUS ?? OpenStatus;
                var dueDate = step.DUE_DATE ?? now.AddDays(30);

                return new CAStatus(
                    StepSeq: (int)step.STEP_SEQ_NO,
                    Description: step.STEP_NAME ?? step.DESCRIPTION ?? string.Empty,
                    CAType: step.STEP_TYPE ?? string.Empty,
                    Status: status,
                    DueDate: dueDate,
                    IsOverdue: !string.Equals(status, CompletedStatus, StringComparison.OrdinalIgnoreCase) && dueDate < now,
                    ResponsiblePerson: responsiblePeople.GetValueOrDefault(step.STEP_ID),
                    CompletedDate: step.ACTUAL_END_DATE);
            })
            .ToList();
    }

    private async Task<PPDMGenericRepository> BuildProjectRepoAsync()
        => new(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROJECT), _connectionName, "PROJECT");

    private async Task<PPDMGenericRepository> BuildStepRepoAsync()
        => new(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROJECT_STEP), _connectionName, "PROJECT_STEP");

    private async Task<PPDMGenericRepository> BuildStepBaRepoAsync()
        => new(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROJECT_STEP_BA), _connectionName, "PROJECT_STEP_BA");

    private async Task<PPDMGenericRepository> BuildStatusRepoAsync()
        => new(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROJECT_STATUS), _connectionName, "PROJECT_STATUS");

    private async Task<List<PROJECT_STEP>> GetStepsAsync(string planId)
    {
        var repo = await BuildStepRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = planId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters)).OfType<PROJECT_STEP>().ToList();
    }

    private async Task<PROJECT_STEP?> GetStepAsync(string planId, int stepSeq)
    {
        var repo = await BuildStepRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = planId },
            new AppFilter { FieldName = "STEP_SEQ_NO", Operator = "=", FilterValue = stepSeq.ToString() },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters)).OfType<PROJECT_STEP>().FirstOrDefault();
    }

    private async Task AddStepStatusAsync(string planId, string stepId, string status, string? remark, string userId)
    {
        var repo = await BuildStatusRepoAsync();
        var entity = new PROJECT_STATUS
        {
            PROJECT_ID = planId,
            STATUS_ID = $"{stepId}-{status}-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            ACTIVE_IND = "Y",
            DEFINED_BY_BA_ID = userId,
            EFFECTIVE_DATE = DateTime.UtcNow,
            REMARK = string.IsNullOrWhiteSpace(remark) ? status : remark,
            SOURCE = "Beep.OilandGas.HSE",
            STATUS = status,
            STATUS_TYPE = CorrectiveActionStatusType,
            STEP_ID = stepId,
        };

        await repo.InsertAsync(entity, userId);
    }

    private async Task<Dictionary<string, PROJECT_STATUS>> GetLatestStepStatusesAsync(string planId)
    {
        var repo = await BuildStatusRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = planId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
            new AppFilter { FieldName = "STATUS_TYPE", Operator = "=", FilterValue = CorrectiveActionStatusType },
        };

        return (await repo.GetAsync(filters))
            .OfType<PROJECT_STATUS>()
            .Where(status => !string.IsNullOrWhiteSpace(status.STEP_ID))
            .GroupBy(status => status.STEP_ID)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(status => status.EFFECTIVE_DATE ?? DateTime.MinValue)
                    .ThenByDescending(status => status.ROW_CREATED_DATE ?? DateTime.MinValue)
                    .First(),
                StringComparer.OrdinalIgnoreCase);
    }

    private async Task<PROJECT_STEP_BA?> GetResponsibleAssignmentAsync(string planId, string stepId)
    {
        var repo = await BuildStepBaRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = planId },
            new AppFilter { FieldName = "STEP_ID", Operator = "=", FilterValue = stepId },
            new AppFilter { FieldName = "ROLE", Operator = "=", FilterValue = ResponsibleRole },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters)).OfType<PROJECT_STEP_BA>().FirstOrDefault();
    }

    private async Task<Dictionary<string, string>> GetResponsiblePeopleAsync(string planId)
    {
        var repo = await BuildStepBaRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = planId },
            new AppFilter { FieldName = "ROLE", Operator = "=", FilterValue = ResponsibleRole },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<PROJECT_STEP_BA>()
            .Where(assignment => !string.IsNullOrWhiteSpace(assignment.STEP_ID))
            .GroupBy(assignment => assignment.STEP_ID)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(assignment => assignment.EFFECTIVE_DATE ?? DateTime.MinValue)
                    .ThenByDescending(assignment => assignment.STEP_BA_OBS_NO)
                    .First()
                    .BUSINESS_ASSOCIATE_ID ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);
    }

    private async Task<decimal> GetNextStepBaObsNoAsync(string planId, string stepId)
    {
        var repo = await BuildStepBaRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = planId },
            new AppFilter { FieldName = "STEP_ID", Operator = "=", FilterValue = stepId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var maxObsNo = (await repo.GetAsync(filters))
            .OfType<PROJECT_STEP_BA>()
            .Select(assignment => assignment.STEP_BA_OBS_NO)
            .DefaultIfEmpty(0)
            .Max();

        return maxObsNo + 1;
    }
}
