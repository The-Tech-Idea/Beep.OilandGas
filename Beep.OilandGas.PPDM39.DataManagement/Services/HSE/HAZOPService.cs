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

public class HAZOPService : IHAZOPService
{
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

    public async Task<List<HAZOPSummary>> GetStudiesAsync(string fieldId)
    {
        var repo = await BuildProjectRepoAsync();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "FIELD_ID",     Operator = "=", FilterValue = fieldId },
            new() { FieldName = "PROJECT_TYPE", Operator = "=", FilterValue = "HAZOP" },
            new() { FieldName = "ACTIVE_IND",   Operator = "=", FilterValue = "Y" },
        };
        var rows = (await repo.GetAsync(filters)).ToList();
        var summaries = new List<HAZOPSummary>();
        foreach (var row in rows)
        {
            var studyId = GetStr(row, "PROJECT_ID");
            try { summaries.Add(await GetSummaryAsync(studyId)); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to load HAZOP summary for {StudyId}", studyId); }
        }
        return summaries;
    }

    public async Task<string> CreateStudyAsync(CreateHAZOPStudyRequest request, string userId)
    {
        var studyId = $"HAZOP-{request.FieldId}-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks % 1000:D3}";
        var repo    = await BuildProjectRepoAsync();

        var entity = new
        {
            PROJECT_ID   = studyId,
            PROJECT_NAME = request.StudyName,
            FIELD_ID     = request.FieldId,
            PROJECT_TYPE = "HAZOP",
            SCOPE_DESC   = request.Scope,
            STATUS       = "DRAFT",
            ACTIVE_IND   = "Y",
        };

        await repo.InsertAsync(entity, userId);
        return studyId;
    }

    public async Task<List<HAZOPNode>> GetNodesAsync(string studyId)
    {
        var stepRepo = await BuildStepRepoAsync();
        var stepFilters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
        };

        var steps = (await stepRepo.GetAsync(stepFilters)).ToList();
        var condRepo = await BuildConditionRepoAsync();
        var nodes    = new List<HAZOPNode>();

        foreach (var step in steps)
        {
            var seq  = GetInt(step, "STEP_SEQ");
            var node = new HAZOPNode
            {
                NodeSeq   = seq,
                NodeName  = GetStr(step, "STEP_NAME"),
                Safeguard = GetStr(step, "STEP_NOTES"),
            };

            var condFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
                new AppFilter { FieldName = "STEP_SEQ",   Operator = "=", FilterValue = seq.ToString() },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
            };

            var conds = (await condRepo.GetAsync(condFilters)).ToList();
            node.Deviations = conds.Select(c => new HAZOPDeviation
            {
                CondSeq     = GetInt(c, "COND_SEQ"),
                GuideWord   = GetStr(c, "COND_TYPE"),
                Deviation   = GetStr(c, "COND_DESC"),
                Consequence = GetStr(c, "RESULT_TEXT"),
                Status      = GetStr(c, "COND_STATUS"),
            }).ToList();

            nodes.Add(node);
        }

        return nodes.OrderBy(n => n.NodeSeq).ToList();
    }

    public async Task<string> AddNodeAsync(string studyId, AddNodeRequest request, string userId)
    {
        var repo = await BuildStepRepoAsync();

        // Get next seq
        var existing = await GetNodesAsync(studyId);
        var nextSeq  = (existing.Count > 0 ? existing.Max(n => n.NodeSeq) : 0) + 1;

        var entity = new
        {
            PROJECT_ID  = studyId,
            STEP_SEQ    = nextSeq,
            STEP_NAME   = request.NodeName,
            STEP_NOTES  = request.Safeguard,
            STEP_STATUS = "DRAFT",
            ACTIVE_IND  = "Y",
        };

        await repo.InsertAsync(entity, userId);
        return $"{studyId}-NODE-{nextSeq}";
    }

    public async Task<string> AddDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, string userId)
    {
        var repo = await BuildConditionRepoAsync();

        var condFilters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = studyId },
            new AppFilter { FieldName = "STEP_SEQ",   Operator = "=", FilterValue = nodeSeq.ToString() },
        };
        var existing = (await repo.GetAsync(condFilters)).ToList();
        var nextSeq  = existing.Count + 1;

        var entity = new
        {
            PROJECT_ID  = studyId,
            STEP_SEQ    = nodeSeq,
            COND_SEQ    = nextSeq,
            COND_TYPE   = request.GuideWord,
            COND_DESC   = request.Deviation,
            RESULT_TEXT = request.Consequence,
            COND_STATUS = "OPEN",
            ACTIVE_IND  = "Y",
        };

        await repo.InsertAsync(entity, userId);
        return $"{studyId}-NODE{nodeSeq}-DEV{nextSeq}";
    }

    public async Task UpdateDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, string userId)
    {
        var repo = await BuildConditionRepoAsync();
        var id   = $"{studyId}|{nodeSeq}|{condSeq}";
        var existing = await repo.GetByIdAsync(id);
        if (existing is null) return;

        SetStr(existing, "COND_STATUS", status);
        await repo.UpdateAsync(existing, userId);
    }

    public async Task<HAZOPSummary> GetSummaryAsync(string studyId)
    {
        var nodes = await GetNodesAsync(studyId);
        var allDevs = nodes.SelectMany(n => n.Deviations).ToList();

        var projRepo = await BuildProjectRepoAsync();
        var proj     = await projRepo.GetByIdAsync(studyId);
        var name     = proj is not null ? GetStr(proj, "PROJECT_NAME") : studyId;

        return new HAZOPSummary(
            StudyId:         studyId,
            StudyName:       name,
            TotalNodes:      nodes.Count,
            TotalDeviations: allDevs.Count,
            OpenDeviations:  allDevs.Count(d => d.Status == "OPEN"),
            ActionRequired:  allDevs.Count(d => d.Status == "ACTION_REQUIRED"),
            Closed:          allDevs.Count(d => d.Status == "CLOSED"));
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

    private async Task<PPDMGenericRepository> BuildConditionRepoAsync()
    {
        var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP_CONDITION");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                         ?? typeof(object);
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, "PROJECT_STEP_CONDITION");
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
