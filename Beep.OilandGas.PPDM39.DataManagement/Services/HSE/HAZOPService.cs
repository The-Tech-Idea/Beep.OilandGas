using Beep.OilandGas.Models.Constants;
using System.Globalization;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
public class HAZOPService : IHAZOPService
{
    private const string HazopProjectType = HseReferenceCodes.Hazop.ProjectType;
    private const string HazopNodeType = HseReferenceCodes.Hazop.NodeType;
    private const string HazopStudyStatusType = HseReferenceCodes.Hazop.StudyStatusType;
    private const string HazopDeviationStatusType = HseReferenceCodes.Hazop.DeviationStatusType;
    private const string FieldContextSource = HseReferenceCodes.Hazop.FieldContextSource;
    private const string DraftStatus = HseReferenceCodes.Hazop.Draft;
    private const string OpenStatus = HseReferenceCodes.Hazop.Open;
    private const string ActionRequiredStatus = HseReferenceCodes.Hazop.ActionRequired;
    private const string ClosedStatus = HseReferenceCodes.Hazop.Closed;
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ICompanyFieldSelectionService _selections;
    private readonly IPPDMRowAccessPolicy _rowAccess;
    private FieldSelectionReview? _review;
    private readonly ILogger<HAZOPService> _logger;
    public HAZOPService(IDMEEditor editor, ICommonColumnHandler commonColumnHandler, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, ICompanyFieldSelectionService selections, IPPDMRowAccessPolicy rowAccess, ILogger<HAZOPService> logger)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _selections = selections ?? throw new ArgumentNullException(nameof(selections));
        _rowAccess = rowAccess ?? throw new ArgumentNullException(nameof(rowAccess));
        _logger = logger;
    }

    private sealed record HazopConditionPayload(string Deviation, string Consequence);
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

    public async Task<List<HAZOPSummary>> GetStudiesAsync(string fieldId)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            throw RefusalException.Invalid("fieldId is required.");
        await RequireCurrentBindingAsync(fieldId, null, true);
        var summaries = new List<HAZOPSummary>();
        var studyIds = await GetStudyIdsForFieldAsync(fieldId);
        foreach (var studyId in studyIds)
        {
            try
            {
                summaries.Add(await GetSummaryAsync(studyId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load HAZOP summary for {StudyId}", studyId);
                throw;
            }
        }

        return await ConfirmAsync(summaries);
    }

    public async Task<string> CreateStudyAsync(CreateHAZOPStudyRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        ArgumentNullException.ThrowIfNull(request);
        if (request.FieldId != _review!.FieldId)
            throw RefusalException.Conflict("The submitted HSE FIELD differs from the reviewed selection.");
        var studyId = $"HAZOP-{request.FieldId}-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks % 1000:D3}";
        var repo = await BuildProjectRepo();
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
        return await ConfirmAsync(studyId);
    }

    public async Task<List<HAZOPNode>> GetNodesAsync(string studyId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        await RequireStudyAsync(studyId);
        var steps = await GetStepsAsync(studyId);
        var conditions = await GetConditionsAsync(studyId);
        var statuses = await GetDeviationStatusesAsync(studyId);
        if (steps.Any(step => step.PROJECT_ID != studyId || step.STEP_TYPE != HazopNodeType || !Identifier(step.STEP_ID) || !Sequence(step.STEP_SEQ_NO) || string.IsNullOrWhiteSpace(step.STEP_NAME)) ||
            steps.GroupBy(step => step.STEP_SEQ_NO).Any(group => group.Count() != 1) || steps.GroupBy(step => step.STEP_ID, StringComparer.Ordinal).Any(group => group.Count() != 1))
            throw RefusalException.Conflict("The recorded HAZOP nodes have missing or ambiguous identities, sequence or names.");
        var stepIds = steps.Select(step => step.STEP_ID).ToHashSet(StringComparer.Ordinal);
        if (conditions.Any(condition => condition.PROJECT_ID != studyId || condition.PROJECT_TYPE != HazopProjectType || !Sequence(condition.CONDITION_OBS_NO) || !stepIds.Contains(condition.SOURCE)) ||
            conditions.GroupBy(condition => condition.CONDITION_OBS_NO).Any(group => group.Count() != 1))
            throw RefusalException.Conflict("A recorded HAZOP condition has no unique node in this study.");
        var expectedStatuses = conditions.ToDictionary(condition => GetDeviationStatusId(condition.SOURCE, condition.CONDITION_OBS_NO), StringComparer.Ordinal);
        if (statuses.Count != expectedStatuses.Count || statuses.Any(pair => !expectedStatuses.TryGetValue(pair.Key, out var condition) || pair.Value.STEP_ID != condition.SOURCE ||
            pair.Value.PROJECT_ID != studyId || pair.Value.STATUS_TYPE != HazopDeviationStatusType || pair.Value.STATUS is not (OpenStatus or ActionRequiredStatus or ClosedStatus) ||
            pair.Value.EFFECTIVE_DATE is not { Kind: DateTimeKind.Utc }))
            throw RefusalException.Conflict("Every recorded HAZOP deviation requires one explicit valid status associated with its exact node.");
        var nodes = steps.OrderBy(step => step.STEP_SEQ_NO).Select(step => new HAZOPNode
        {
            NodeSeq = checked((int)step.STEP_SEQ_NO),
            NodeName = step.STEP_NAME,
            Safeguard = step.REMARK ?? string.Empty,
            Deviations = conditions.Where(condition => condition.SOURCE == step.STEP_ID).OrderBy(condition => condition.CONDITION_OBS_NO).Select(condition =>
            {
                var payload = ParseConditionPayload(condition);
                return new HAZOPDeviation
                {
                    CondSeq = checked((int)condition.CONDITION_OBS_NO), GuideWord = condition.CONDITION_TYPE,
                    Deviation = payload.Deviation, Consequence = payload.Consequence,
                    Status = statuses[GetDeviationStatusId(step.STEP_ID, condition.CONDITION_OBS_NO)].STATUS,
                };
            }).ToList(),
        }).ToList();
        return await ConfirmAsync(nodes);
    }

    public async Task<string> AddNodeAsync(string studyId, AddNodeRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        ArgumentNullException.ThrowIfNull(request);
        await RequireStudyAsync(studyId);
        var repo = await BuildStepRepo();
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
        return await ConfirmAsync(stepId);
    }

    public async Task<string> AddDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        ArgumentNullException.ThrowIfNull(request);
        await RequireStudyAsync(studyId);
        var step = await GetStepAsync(studyId, nodeSeq);
        if (step is null)
        {
            throw new InvalidOperationException($"HAZOP node {nodeSeq} not found for study {studyId}");
        }

        var repo = await BuildConditionRepo();
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
        return await ConfirmAsync($"{studyId}-NODE{nodeSeq}-DEV{nextSeq}");
    }

    public async Task UpdateDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        if (nodeSeq <= 0 || condSeq <= 0 || status is not (OpenStatus or ActionRequiredStatus or ClosedStatus))
            throw RefusalException.Invalid("A HAZOP status change requires positive node/condition identities and an explicit supported status.");
        await RequireStudyAsync(studyId);
        var step = await GetStepAsync(studyId, nodeSeq);
        if (step is null) throw RefusalException.NotFound("The HAZOP node is unavailable in this study.");

        var condition = await GetConditionAsync(studyId, condSeq);
        if (condition is null || condition.SOURCE != step.STEP_ID)
            throw RefusalException.NotFound("The HAZOP deviation is unavailable in this node.");

        await UpsertDeviationStatusAsync(studyId, step.STEP_ID, condSeq, status, userId);
        await ConfirmAsync();
    }

    public async Task<HAZOPSummary> GetSummaryAsync(string studyId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        await RequireStudyAsync(studyId);
        var nodes = await GetNodesAsync(studyId);
        var allDevs = nodes.SelectMany(n => n.Deviations).ToList();
        var project = await GetProjectAsync(studyId);
        var name = project?.PROJECT_NAME;
        if (string.IsNullOrWhiteSpace(name)) throw RefusalException.Conflict("The recorded HAZOP study name is unavailable.");
        return await ConfirmAsync(new HAZOPSummary(StudyId: studyId, StudyName: name, TotalNodes: nodes.Count, TotalDeviations: allDevs.Count, OpenDeviations: allDevs.Count(d => string.Equals(d.Status, OpenStatus, StringComparison.Ordinal)), ActionRequired: allDevs.Count(d => string.Equals(d.Status, ActionRequiredStatus, StringComparison.Ordinal)), Closed: allDevs.Count(d => string.Equals(d.Status, ClosedStatus, StringComparison.Ordinal))));
    }

    private async Task<PPDMGenericRepository> BuildProjectRepo() => new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PROJECT), await RequireCurrentBindingAsync(), "PROJECT", rowAccess: _rowAccess);
    private async Task<PPDMGenericRepository> BuildProjectComponentRepo() => new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PROJECT_COMPONENT), await RequireCurrentBindingAsync(), "PROJECT_COMPONENT", rowAccess: _rowAccess);
    private async Task<PPDMGenericRepository> BuildStepRepo() => new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PROJECT_STEP), await RequireCurrentBindingAsync(), "PROJECT_STEP", rowAccess: _rowAccess);
    private async Task<PPDMGenericRepository> BuildConditionRepo() => new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PROJECT_CONDITION), await RequireCurrentBindingAsync(), "PROJECT_CONDITION", rowAccess: _rowAccess);
    private async Task<PPDMGenericRepository> BuildStatusRepo() => new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PROJECT_STATUS), await RequireCurrentBindingAsync(), "PROJECT_STATUS", rowAccess: _rowAccess);
    private async Task RequireStudyAsync(string studyId)
    {
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        var repo = await BuildProjectComponentRepo();
        var rows = (await repo.GetAsync(new List<AppFilter> { new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId }, new() { FieldName = "FIELD_ID", Operator = "=", FilterValue = _review!.FieldId }, new() { FieldName = "FIELD_SOURCE", Operator = "=", FilterValue = FieldContextSource }, new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } })).Cast<PROJECT_COMPONENT>().ToList();
        if (rows.Count != 1 || rows[0].PROJECT_ID != studyId || rows[0].FIELD_ID != _review!.FieldId || rows[0].FIELD_SOURCE != FieldContextSource || rows[0].ACTIVE_IND != "Y")
            throw RefusalException.NotFound("The HAZOP study has no unique current FIELD context.");
        var project = await GetProjectAsync(studyId);
        if (project is null || project.PROJECT_ID != studyId || project.ACTIVE_IND != "Y" || project.PROJECT_TYPE != HazopProjectType)
            throw RefusalException.NotFound("The active HAZOP study is unavailable.");
        await RequireCurrentBindingAsync();
    }

    private async Task<PROJECT?> GetProjectAsync(string studyId)
    {
        var repo = await BuildProjectRepo();
        return await repo.GetByIdAsync(studyId) as PROJECT;
    }

    private async Task<List<string>> GetStudyIdsForFieldAsync(string fieldId)
    {
        var repo = await BuildProjectComponentRepo();
        var filters = new List<AppFilter>
        {
            new()
            {
                FieldName = "FIELD_ID",
                Operator = "=",
                FilterValue = fieldId
            },
            new()
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        filters.Add(new AppFilter { FieldName = "FIELD_SOURCE", Operator = "=", FilterValue = FieldContextSource });
        filters.Add(new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = FieldContextSource });
        return (await repo.GetAsync(filters)).Cast<PROJECT_COMPONENT>().Select(component => component.PROJECT_ID).Distinct(StringComparer.Ordinal).ToList();
    }

    private async Task AddFieldContextAsync(string studyId, string fieldId, string scope, string userId)
    {
        var repo = await BuildProjectComponentRepo();
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
        var repo = await BuildProjectComponentRepo();
        var filters = new List<AppFilter>
        {
            new()
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = studyId
            },
            new()
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var maxObsNo = (await repo.GetAsync(filters)).OfType<PROJECT_COMPONENT>().Select(component => component.COMPONENT_OBS_NO).DefaultIfEmpty(0).Max();
        return maxObsNo + 1;
    }

    private async Task<List<PROJECT_STEP>> GetStepsAsync(string studyId)
    {
        var repo = await BuildStepRepo();
        var filters = new List<AppFilter>
        {
            new()
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = studyId
            },
            new()
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        return (await repo.GetAsync(filters)).OfType<PROJECT_STEP>().ToList();
    }

    private async Task<PROJECT_STEP?> GetStepAsync(string studyId, int nodeSeq)
    {
        var repo = await BuildStepRepo();
        var filters = new List<AppFilter>
        {
            new()
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = studyId
            },
            new()
            {
                FieldName = "STEP_SEQ_NO",
                Operator = "=",
                FilterValue = nodeSeq.ToString(CultureInfo.InvariantCulture)
            },
            new()
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var rows = (await repo.GetAsync(filters)).Cast<PROJECT_STEP>().ToArray();
        if (rows.Length > 1) throw RefusalException.Conflict("The HAZOP node sequence is ambiguous.");
        return rows.SingleOrDefault();
    }

    private async Task<List<PROJECT_CONDITION>> GetConditionsAsync(string studyId)
    {
        var repo = await BuildConditionRepo();
        var filters = new List<AppFilter>
        {
            new()
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = studyId
            },
            new()
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        return (await repo.GetAsync(filters)).OfType<PROJECT_CONDITION>().Where(condition => string.Equals(condition.PROJECT_TYPE, HazopProjectType, StringComparison.Ordinal)).ToList();
    }

    private async Task<PROJECT_CONDITION?> GetConditionAsync(string studyId, int conditionObsNo)
    {
        var repo = await BuildConditionRepo();
        var filters = new List<AppFilter>
        {
            new()
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = studyId
            },
            new()
            {
                FieldName = "CONDITION_OBS_NO",
                Operator = "=",
                FilterValue = conditionObsNo.ToString(CultureInfo.InvariantCulture)
            },
            new()
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var rows = (await repo.GetAsync(filters)).Cast<PROJECT_CONDITION>().ToArray();
        if (rows.Length > 1) throw RefusalException.Conflict("The HAZOP condition identity is ambiguous.");
        return rows.SingleOrDefault();
    }

    private async Task<decimal> GetNextConditionObsNoAsync(string studyId)
    {
        var conditions = await GetConditionsAsync(studyId);
        return conditions.Select(condition => condition.CONDITION_OBS_NO).DefaultIfEmpty(0).Max() + 1;
    }

    private async Task UpsertStudyStatusAsync(string studyId, string status, string userId)
    {
        var repo = await BuildStatusRepo();
        var filters = new List<AppFilter>
        {
            new()
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = studyId
            },
            new()
            {
                FieldName = "STATUS_TYPE",
                Operator = "=",
                FilterValue = HazopStudyStatusType
            },
            new()
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
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
        var repo = await BuildStatusRepo();
        var filters = new List<AppFilter>
        {
            new()
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = studyId
            },
            new()
            {
                FieldName = "STATUS_TYPE",
                Operator = "=",
                FilterValue = HazopDeviationStatusType
            },
            new()
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var rows = (await repo.GetAsync(filters)).Cast<PROJECT_STATUS>().ToArray();
        if (rows.Any(status => !Identifier(status.STATUS_ID)) || rows.GroupBy(status => status.STATUS_ID, StringComparer.Ordinal).Any(group => group.Count() != 1))
            throw RefusalException.Conflict("The HAZOP deviation status identities are missing or ambiguous.");
        return rows.ToDictionary(status => status.STATUS_ID, StringComparer.Ordinal);
    }

    private async Task UpsertDeviationStatusAsync(string studyId, string stepId, decimal conditionObsNo, string status, string userId)
    {
        var repo = await BuildStatusRepo();
        var statusId = GetDeviationStatusId(stepId, conditionObsNo);
        var filters = new List<AppFilter>
        {
            new()
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = studyId
            },
            new()
            {
                FieldName = "STATUS_ID",
                Operator = "=",
                FilterValue = statusId
            },
            new()
            {
                FieldName = "STATUS_TYPE",
                Operator = "=",
                FilterValue = HazopDeviationStatusType
            },
            new()
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
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

    private static bool Sequence(decimal value) => value > 0 && value <= int.MaxValue && decimal.Truncate(value) == value;
    private static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value) && value == value.Trim() && value.Length <= 128 && !value.Any(char.IsControl);
    private static string GetDeviationStatusId(string stepId, decimal conditionObsNo) => stepId + "-DEV-" + conditionObsNo.ToString("000", CultureInfo.InvariantCulture);
    private static string SerializeConditionPayload(HazopConditionPayload payload) => JsonSerializer.Serialize(payload);
    private static HazopConditionPayload ParseConditionPayload(PROJECT_CONDITION condition)
    {
        if (string.IsNullOrWhiteSpace(condition.REMARK) || condition.REMARK.Length > 65536)
            throw RefusalException.Conflict("The recorded HAZOP condition payload is unavailable or exceeds its supported size.");
        try
        {
            using var document = JsonDocument.Parse(condition.REMARK);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw RefusalException.Conflict("The recorded HAZOP condition payload must be an object.");
            var properties = document.RootElement.EnumerateObject().ToArray();
            if (properties.Length != 2 || properties.Count(value => value.Name == "Deviation") != 1 || properties.Count(value => value.Name == "Consequence") != 1 ||
                properties.Any(value => value.Value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.Value.GetString())))
                throw RefusalException.Conflict("The recorded HAZOP payload requires exactly one explicit deviation and consequence.");
            return new HazopConditionPayload(document.RootElement.GetProperty("Deviation").GetString()!, document.RootElement.GetProperty("Consequence").GetString()!);
        }
        catch (JsonException ex)
        {
            throw new RefusalException(RefusalKind.Conflict, "The recorded HAZOP condition payload is malformed.", ex);
        }
    }
}
