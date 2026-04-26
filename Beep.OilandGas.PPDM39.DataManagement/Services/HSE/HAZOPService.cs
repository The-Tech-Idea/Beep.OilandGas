using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;

public class HAZOPService : IHAZOPService
{
    private const string HazopProjectType = "HAZOP";
    private const string HazopNodeType = "HAZOP_NODE";
    private const string HazopStudyStatusType = "HAZOP_STUDY";
    private const string HazopDeviationStatusType = "HAZOP_DEVIATION";
    private const string FieldContextSource = "Beep.OilandGas.HSE.Field";
    private const string DraftStatus = "DRAFT";
    private const string OpenStatus = "OPEN";
    private const string ActionRequiredStatus = "ACTION_REQUIRED";
    private const string ClosedStatus = "CLOSED";

    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<HAZOPService>     _logger;

    public HAZOPService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<HAZOPService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    private sealed record HazopConditionPayload(string Deviation, string Consequence);

    public async Task<List<HAZOPSummary>> GetStudiesAsync(string fieldId)
    {
        var summaries = new List<HAZOPSummary>();
        var studyIds = await GetStudyIdsForFieldAsync(fieldId);

        foreach (var studyId in studyIds)
        {
            try { summaries.Add(await GetSummaryAsync(studyId)); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to load HAZOP summary for {StudyId}", studyId); }
        }

        return summaries;
    }

    public async Task<string> CreateStudyAsync(CreateHAZOPStudyRequest request, string userId)
    {
        var studyId = $"HAZOP-{request.FieldId}-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks % 1000:D3}";
        var repo = BuildProjectRepo();

        var entity = new PROJECT
        {
            PROJECT_ID = studyId,
            ACTIVE_IND = "Y",
            DESCRIPTION = request.Scope,
            EFFECTIVE_DATE = DateTime.UtcNow,
            PROJECT_NAME = request.StudyName,
            PROJECT_TYPE = HazopProjectType,
            REMARK = request.Scope,
            SOURCE = "Beep.OilandGas.HSE",
            START_DATE = DateTime.UtcNow,
        };

        await repo.InsertAsync(entity, userId);
        await AddFieldContextAsync(studyId, request.FieldId, request.Scope, userId);
        await UpsertStudyStatusAsync(studyId, DraftStatus, userId);

        return studyId;
    }

    public async Task<List<HAZOPNode>> GetNodesAsync(string studyId)
    {
        var steps = await GetStepsAsync(studyId);
        var conditions = await GetConditionsAsync(studyId);
        var statuses = await GetDeviationStatusesAsync(studyId);

        return steps
            .OrderBy(step => step.STEP_SEQ_NO)
            .Select(step =>
            {
                var deviations = conditions
                    .Where(condition => string.Equals(condition.SOURCE, step.STEP_ID, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(condition => condition.CONDITION_OBS_NO)
                    .Select(condition =>
                    {
                        var payload = ParseConditionPayload(condition);
                        var statusId = GetDeviationStatusId(step.STEP_ID, condition.CONDITION_OBS_NO);

                        return new HAZOPDeviation
                        {
                            CondSeq = (int)condition.CONDITION_OBS_NO,
                            GuideWord = condition.CONDITION_TYPE ?? string.Empty,
                            Deviation = payload.Deviation,
                            Consequence = payload.Consequence,
                            Status = statuses.GetValueOrDefault(statusId)?.STATUS ?? OpenStatus,
                        };
                    })
                    .ToList();

                return new HAZOPNode
                {
                    NodeSeq = (int)step.STEP_SEQ_NO,
                    NodeName = step.STEP_NAME ?? step.DESCRIPTION ?? string.Empty,
                    Safeguard = step.REMARK ?? step.DESCRIPTION ?? string.Empty,
                    Deviations = deviations,
                };
            })
            .ToList();
    }

    public async Task<string> AddNodeAsync(string studyId, AddNodeRequest request, string userId)
    {
        var repo = BuildStepRepo();
        var existingSteps = await GetStepsAsync(studyId);
        var nextSeq = (existingSteps.Count > 0 ? (int)existingSteps.Max(step => step.STEP_SEQ_NO) : 0) + 1;
        var stepId = $"{studyId}-NODE-{nextSeq:D3}";

        var entity = new PROJECT_STEP
        {
            PROJECT_ID = studyId,
            STEP_ID = stepId,
            ACTIVE_IND = "Y",
            DESCRIPTION = request.Safeguard,
            EFFECTIVE_DATE = DateTime.UtcNow,
            REMARK = request.Safeguard,
            SOURCE = "Beep.OilandGas.HSE",
            STEP_NAME = request.NodeName,
            STEP_SEQ_NO = nextSeq,
            STEP_TYPE = HazopNodeType,
        };

        await repo.InsertAsync(entity, userId);
        return stepId;
    }

    public async Task<string> AddDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, string userId)
    {
        var step = await GetStepAsync(studyId, nodeSeq);
        if (step is null)
        {
            throw new InvalidOperationException($"HAZOP node {nodeSeq} not found for study {studyId}");
        }

        var repo = BuildConditionRepo();
        var nextSeq = await GetNextConditionObsNoAsync(studyId);

        var entity = new PROJECT_CONDITION
        {
            PROJECT_ID = studyId,
            CONDITION_OBS_NO = nextSeq,
            ACTIVE_IND = "Y",
            CONDITION_TYPE = request.GuideWord,
            EFFECTIVE_DATE = DateTime.UtcNow,
            PROJECT_TYPE = HazopProjectType,
            REMARK = SerializeConditionPayload(new HazopConditionPayload(request.Deviation, request.Consequence)),
            SOURCE = step.STEP_ID,
            START_DATE = DateTime.UtcNow,
        };

        await repo.InsertAsync(entity, userId);
        await UpsertDeviationStatusAsync(studyId, step.STEP_ID, nextSeq, OpenStatus, userId);

        return $"{studyId}-NODE{nodeSeq}-DEV{nextSeq}";
    }

    public async Task UpdateDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, string userId)
    {
        var step = await GetStepAsync(studyId, nodeSeq);
        if (step is null)
        {
            return;
        }

        var condition = await GetConditionAsync(studyId, condSeq);
        if (condition is null || !string.Equals(condition.SOURCE, step.STEP_ID, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await UpsertDeviationStatusAsync(studyId, step.STEP_ID, condSeq, status, userId);
    }

    public async Task<HAZOPSummary> GetSummaryAsync(string studyId)
    {
        var nodes = await GetNodesAsync(studyId);
        var allDevs = nodes.SelectMany(n => n.Deviations).ToList();

        var project = await GetProjectAsync(studyId);
        var name = project?.PROJECT_NAME ?? studyId;

        return new HAZOPSummary(
            StudyId:         studyId,
            StudyName:       name,
            TotalNodes:      nodes.Count,
            TotalDeviations: allDevs.Count,
            OpenDeviations:  allDevs.Count(d => string.Equals(d.Status, OpenStatus, StringComparison.OrdinalIgnoreCase)),
            ActionRequired:  allDevs.Count(d => string.Equals(d.Status, ActionRequiredStatus, StringComparison.OrdinalIgnoreCase)),
            Closed:          allDevs.Count(d => string.Equals(d.Status, ClosedStatus, StringComparison.OrdinalIgnoreCase)));
    }

    private PPDMGenericRepository BuildProjectRepo()
        => new(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROJECT), _connectionName, "PROJECT");

    private PPDMGenericRepository BuildProjectComponentRepo()
        => new(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROJECT_COMPONENT), _connectionName, "PROJECT_COMPONENT");

    private PPDMGenericRepository BuildStepRepo()
        => new(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROJECT_STEP), _connectionName, "PROJECT_STEP");

    private PPDMGenericRepository BuildConditionRepo()
        => new(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROJECT_CONDITION), _connectionName, "PROJECT_CONDITION");

    private PPDMGenericRepository BuildStatusRepo()
        => new(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROJECT_STATUS), _connectionName, "PROJECT_STATUS");

    private async Task<PROJECT?> GetProjectAsync(string studyId)
    {
        var repo = BuildProjectRepo();
        return await repo.GetByIdAsync(studyId) as PROJECT;
    }

    private async Task<List<string>> GetStudyIdsForFieldAsync(string fieldId)
    {
        var repo = BuildProjectComponentRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var components = (await repo.GetAsync(filters)).OfType<PROJECT_COMPONENT>();
        var studyIds = new List<string>();

        foreach (var component in components)
        {
            if (string.IsNullOrWhiteSpace(component.PROJECT_ID))
            {
                continue;
            }

            var project = await GetProjectAsync(component.PROJECT_ID);
            if (project is null)
            {
                continue;
            }

            if (!string.Equals(project.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(project.PROJECT_TYPE, HazopProjectType, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            studyIds.Add(project.PROJECT_ID);
        }

        return studyIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private async Task AddFieldContextAsync(string studyId, string fieldId, string scope, string userId)
    {
        var repo = BuildProjectComponentRepo();
        var entity = new PROJECT_COMPONENT
        {
            PROJECT_ID = studyId,
            COMPONENT_OBS_NO = await GetNextComponentObsNoAsync(studyId),
            ACTIVE_IND = "Y",
            EFFECTIVE_DATE = DateTime.UtcNow,
            FIELD_ID = fieldId,
            FIELD_SOURCE = FieldContextSource,
            REMARK = scope,
            SOURCE = FieldContextSource,
        };

        await repo.InsertAsync(entity, userId);
    }

    private async Task<decimal> GetNextComponentObsNoAsync(string studyId)
    {
        var repo = BuildProjectComponentRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var maxObsNo = (await repo.GetAsync(filters))
            .OfType<PROJECT_COMPONENT>()
            .Select(component => component.COMPONENT_OBS_NO)
            .DefaultIfEmpty(0)
            .Max();

        return maxObsNo + 1;
    }

    private async Task<List<PROJECT_STEP>> GetStepsAsync(string studyId)
    {
        var repo = BuildStepRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters)).OfType<PROJECT_STEP>().ToList();
    }

    private async Task<PROJECT_STEP?> GetStepAsync(string studyId, int nodeSeq)
    {
        var repo = BuildStepRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
            new() { FieldName = "STEP_SEQ_NO", Operator = "=", FilterValue = nodeSeq.ToString() },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters)).OfType<PROJECT_STEP>().FirstOrDefault();
    }

    private async Task<List<PROJECT_CONDITION>> GetConditionsAsync(string studyId)
    {
        var repo = BuildConditionRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<PROJECT_CONDITION>()
            .Where(condition => string.Equals(condition.PROJECT_TYPE, HazopProjectType, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task<PROJECT_CONDITION?> GetConditionAsync(string studyId, int conditionObsNo)
    {
        var repo = BuildConditionRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
            new() { FieldName = "CONDITION_OBS_NO", Operator = "=", FilterValue = conditionObsNo.ToString() },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<PROJECT_CONDITION>()
            .FirstOrDefault(condition => string.Equals(condition.PROJECT_TYPE, HazopProjectType, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<decimal> GetNextConditionObsNoAsync(string studyId)
    {
        var conditions = await GetConditionsAsync(studyId);
        return conditions.Select(condition => condition.CONDITION_OBS_NO).DefaultIfEmpty(0).Max() + 1;
    }

    private async Task UpsertStudyStatusAsync(string studyId, string status, string userId)
    {
        var repo = BuildStatusRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
            new() { FieldName = "STATUS_TYPE", Operator = "=", FilterValue = HazopStudyStatusType },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var existing = (await repo.GetAsync(filters)).OfType<PROJECT_STATUS>().FirstOrDefault();
        if (existing is null)
        {
            var entity = new PROJECT_STATUS
            {
                PROJECT_ID = studyId,
                STATUS_ID = $"{studyId}-STUDY",
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = DateTime.UtcNow,
                REMARK = status,
                SOURCE = "Beep.OilandGas.HSE",
                STATUS = status,
                STATUS_TYPE = HazopStudyStatusType,
            };

            await repo.InsertAsync(entity, userId);
            return;
        }

        existing.STATUS = status;
        existing.EFFECTIVE_DATE = DateTime.UtcNow;
        existing.REMARK = status;
        await repo.UpdateAsync(existing, userId);
    }

    private async Task<Dictionary<string, PROJECT_STATUS>> GetDeviationStatusesAsync(string studyId)
    {
        var repo = BuildStatusRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
            new() { FieldName = "STATUS_TYPE", Operator = "=", FilterValue = HazopDeviationStatusType },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        return (await repo.GetAsync(filters))
            .OfType<PROJECT_STATUS>()
            .Where(status => !string.IsNullOrWhiteSpace(status.STATUS_ID))
            .ToDictionary(status => status.STATUS_ID, status => status, StringComparer.OrdinalIgnoreCase);
    }

    private async Task UpsertDeviationStatusAsync(string studyId, string stepId, decimal conditionObsNo, string status, string userId)
    {
        var repo = BuildStatusRepo();
        var statusId = GetDeviationStatusId(stepId, conditionObsNo);
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
            new() { FieldName = "STATUS_ID", Operator = "=", FilterValue = statusId },
            new() { FieldName = "STATUS_TYPE", Operator = "=", FilterValue = HazopDeviationStatusType },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var existing = (await repo.GetAsync(filters)).OfType<PROJECT_STATUS>().FirstOrDefault();
        if (existing is null)
        {
            var entity = new PROJECT_STATUS
            {
                PROJECT_ID = studyId,
                STATUS_ID = statusId,
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = DateTime.UtcNow,
                REMARK = status,
                SOURCE = "Beep.OilandGas.HSE",
                STATUS = status,
                STATUS_TYPE = HazopDeviationStatusType,
                STEP_ID = stepId,
            };

            await repo.InsertAsync(entity, userId);
            return;
        }

        existing.STATUS = status;
        existing.EFFECTIVE_DATE = DateTime.UtcNow;
        existing.REMARK = status;
        await repo.UpdateAsync(existing, userId);
    }

    private static string GetDeviationStatusId(string stepId, decimal conditionObsNo)
        => $"{stepId}-DEV-{conditionObsNo:000}";

    private static string SerializeConditionPayload(HazopConditionPayload payload)
        => JsonSerializer.Serialize(payload);

    private static HazopConditionPayload ParseConditionPayload(PROJECT_CONDITION condition)
    {
        if (!string.IsNullOrWhiteSpace(condition.REMARK))
        {
            try
            {
                var payload = JsonSerializer.Deserialize<HazopConditionPayload>(condition.REMARK);
                if (payload is not null)
                {
                    return payload;
                }
            }
            catch
            {
            }
        }

        return new HazopConditionPayload(condition.REMARK ?? string.Empty, string.Empty);
    }
}
