using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using Serilog;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

public class InspectionService : IInspectionService
{
    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<InspectionService> _logger;

    // BSEE SEMS §250.1920–.1932 standard checklist for Turnaround WOs
    private static readonly List<(int Seq, string Type, string Text, string Ref)> _turnaroundChecklist = new()
    {
        (1,  ConditionType.PreStart,    "Process isolation verified (LOTO applied)",               "SEMS §250.1920(a)"),
        (2,  ConditionType.PreStart,    "Permits to Work issued",                                  "SEMS §250.1921"),
        (3,  ConditionType.PreStart,    "Hydrogen sulfide (H2S) monitor calibrated",               "SEMS §250.1922"),
        (4,  ConditionType.PreStart,    "Emergency shutdown system (ESD) tested",                  "SEMS §250.1923"),
        (5,  ConditionType.PreStart,    "Fire and gas detection armed",                            "SEMS §250.1924"),
        (6,  ConditionType.PreStart,    "Evacuation routes confirmed clear",                       "SEMS §250.1925"),
        (7,  ConditionType.SceVerify,   "BOP/PSV test results recorded",                          "SEMS §250.1926"),
        (8,  ConditionType.SceVerify,   "All SCEs on safe failure inventory checked",              "SEMS §250.1927–.1928"),
        (9,  ConditionType.CloseOut,    "Post-work pressure test documented",                      "SEMS §250.1930"),
        (10, ConditionType.CloseOut,    "Piping restoration and leak check",                       "SEMS §250.1931"),
        (11, ConditionType.CloseOut,    "All permits closed and HSE sign-off complete",            "SEMS §250.1932"),
        (12, ConditionType.Regulatory,  "BSEE notification filed (OCS-G form)",                   "30 CFR §250.144"),
    };

    // Safety Critical Element checklist (SEMS §250.1917)
    private static readonly List<(int Seq, string Type, string Text, string Ref)> _safetyChecklist = new()
    {
        (1, ConditionType.PreStart,    "Safety Critical Element (SCE) identified and tagged",     "SEMS §250.1917"),
        (2, ConditionType.PreStart,    "Permits to Work issued",                                  "SEMS §250.1917(b)"),
        (3, ConditionType.SceVerify,   "SCE function test completed",                             "API RP 14C"),
        (4, ConditionType.CloseOut,    "SCE restoration verified by HSE officer",                 "SEMS §250.1917(d)"),
    };

    // Generic checklist for Preventive / Corrective / Environmental / Regulatory
    private static readonly List<(int Seq, string Type, string Text, string Ref)> _genericChecklist = new()
    {
        (1, ConditionType.PreStart,    "Job safety analysis (JSA) completed and signed",          string.Empty),
        (2, ConditionType.PreStart,    "Required permits obtained",                               string.Empty),
        (3, ConditionType.CloseOut,    "Work site clean-up and reinstatement",                    string.Empty),
        (4, ConditionType.CloseOut,    "As-found / as-left documentation completed",              string.Empty),
    };

    public InspectionService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<InspectionService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    public async Task SeedChecklistAsync(
        string instanceId, string woType, string jurisdiction, string userId)
    {
        Log.Information("Seeding checklist for WO {InstanceId} type {Type}", instanceId, woType);
        try
        {
            var items = woType switch
            {
                WorkOrderSubType.Turnaround => _turnaroundChecklist,
                WorkOrderSubType.Safety     => _safetyChecklist,
                _                           => _genericChecklist
            };

            var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP_CONDITION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT_STEP_CONDITION");

            foreach (var (seq, type, text, regRef) in items)
            {
                dynamic item = Activator.CreateInstance(entityType!)!;
                item.PROJECT_ID    = instanceId;
                item.STEP_SEQ      = 1;              // All checklist items on step 1 for seed
                item.COND_SEQ      = seq;
                item.COND_TYPE     = type;
                item.COND_TEXT     = text;
                item.COND_STATUS   = "PENDING";
                item.REG_REF       = regRef;
                item.ACTIVE_IND    = "Y";
                await repo.InsertAsync(item, userId);
            }
            Log.Information("Seeded {Count} checklist items for WO {InstanceId}", items.Count, instanceId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SeedChecklist failed for WO {InstanceId}", instanceId);
        }
    }

    public async Task RecordResultAsync(
        string instanceId, int condSeq,
        string result, string? notes, string userId)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP_CONDITION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT_STEP_CONDITION");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = instanceId      },
                new() { FieldName = "COND_SEQ",   Operator = "=", FilterValue = condSeq.ToString()},
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"             }
            };
            var rows = (await repo.GetAsync(filters)).ToList();
            if (rows.Count == 0) return;

            dynamic item = rows[0];
            item.COND_STATUS  = result.ToUpperInvariant();
            item.RESULT_TEXT  = notes ?? string.Empty;
            item.INSPECT_DATE = DateTime.UtcNow;
            await repo.UpdateAsync(item, userId);

            Log.Information("Recorded {Result} for condition {Seq} on WO {InstanceId}", result, condSeq, instanceId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "RecordResult failed for WO {InstanceId} condSeq {Seq}", instanceId, condSeq);
            throw;
        }
    }

    public async Task<bool> AllConditionsPassedAsync(
        string instanceId, IEnumerable<string> condTypes)
    {
        var checklist = await GetChecklistAsync(instanceId);
        var types     = condTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return checklist
            .Where(c => types.Contains(c.CondType))
            .All(c => string.Equals(c.Status, "PASS", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<InspectionCondition>> GetChecklistAsync(string instanceId)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP_CONDITION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT_STEP_CONDITION");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = instanceId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"        }
            };
            return (await repo.GetAsync(filters))
                .Select(r =>
                {
                    dynamic d = r;
                    return new InspectionCondition(
                        GetInt(d, "COND_SEQ"),
                        GetStr(d, "COND_TYPE"),
                        GetStr(d, "COND_TEXT"),
                        GetStr(d, "COND_STATUS"),
                        GetStr(d, "RESULT_TEXT"),
                        GetDate(d, "INSPECT_DATE"),
                        GetStr(d, "REG_REF"));
                })
                .OrderBy(c => c.CondSeq)
                .ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetChecklist failed for WO {InstanceId}", instanceId);
            return new();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private PPDMGenericRepository BuildRepo(Type? entityType, string tableName) =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, tableName);

    private static string GetStr(dynamic d, string prop)
    {
        try { return (string?)d.GetType().GetProperty(prop)?.GetValue(d) ?? string.Empty; }
        catch { return string.Empty; }
    }

    private static int GetInt(dynamic d, string prop)
    {
        try
        {
            var v = d.GetType().GetProperty(prop)?.GetValue(d);
            return v is int i ? i : v is long l ? (int)l : 0;
        }
        catch { return 0; }
    }

    private static DateTime? GetDate(dynamic d, string prop)
    {
        try
        {
            var v = d.GetType().GetProperty(prop)?.GetValue(d);
            if (v is DateTime dt) return dt;
            if (v is string s && DateTime.TryParse(s, out var p)) return p;
            return null;
        }
        catch { return null; }
    }
}
