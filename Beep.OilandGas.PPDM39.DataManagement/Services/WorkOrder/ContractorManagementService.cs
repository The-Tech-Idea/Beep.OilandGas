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

public class ContractorManagementService : IContractorManagementService
{
    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<ContractorManagementService> _logger;

    private static readonly Dictionary<string, (string LicenseType, int Level)> _requirements = new()
    {
        [WorkOrderSubType.Preventive]        = ("CONTRACTOR_GENERAL",       2),
        [WorkOrderSubType.Corrective]        = ("CONTRACTOR_GENERAL",       2),
        [WorkOrderSubType.Safety]            = ("SAFETY_CRITICAL_CERT",     3),
        [WorkOrderSubType.Environmental]     = ("ENV_REMEDIATION_CERT",     2),
        [WorkOrderSubType.RegulatoryInspect] = ("REGULATORY_INSPECTION_CERT", 3),
        [WorkOrderSubType.Turnaround]        = ("TURNAROUND_SUPERVISOR",    3),
    };

    private static readonly Dictionary<(string WoType, string Jurisdiction), string> _jurisdictionOverride = new()
    {
        [(WorkOrderSubType.Safety,       "USA")]    = "BSEE_CERT",
        [(WorkOrderSubType.Safety,       "CANADA")] = "AER_CONTRACTOR_CERT",
        [(WorkOrderSubType.Environmental,"USA")]    = "EPA_CERT",
        [(WorkOrderSubType.Environmental,"CANADA")] = "ECCC_CERT",
    };

    public ContractorManagementService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<ContractorManagementService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    public async Task<ContractorAssignment> AssignContractorAsync(
        string instanceId, string stepId, string baId,
        string roleCode, string userId)
    {
        Log.Information("Assigning contractor {BaId} to step {StepId} on WO {InstanceId}",
            baId, stepId, instanceId);
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP_BA");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT_STEP_BA");

            dynamic entity = Activator.CreateInstance(entityType!)!;
            entity.PROJECT_ID    = instanceId;
            entity.STEP_SEQ      = stepId;
            entity.BA_ID         = baId;
            entity.STEP_BA_TYPE  = roleCode;
            entity.ACTIVE_IND    = "Y";
            entity.ASSIGNED_DATE = DateTime.UtcNow;
            await repo.InsertAsync(entity, userId);

            return new ContractorAssignment(stepId, baId, string.Empty, roleCode, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to assign contractor {BaId}", baId);
            throw;
        }
    }

    public async Task RemoveContractorAsync(
        string instanceId, string stepId, string baId, string userId)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP_BA");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT_STEP_BA");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "PROJECT_ID",   Operator = "=", FilterValue = instanceId },
                new() { FieldName = "STEP_SEQ",     Operator = "=", FilterValue = stepId     },
                new() { FieldName = "BA_ID",        Operator = "=", FilterValue = baId       },
                new() { FieldName = "ACTIVE_IND",   Operator = "=", FilterValue = "Y"        }
            };
            var rows = (await repo.GetAsync(filters)).ToList();
            foreach (var row in rows)
            {
                dynamic d  = row;
                string  pk = GetStr(d, "BA_ID");  // soft-delete by BA_ID composite
                await repo.SoftDeleteAsync(pk, userId);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to remove contractor {BaId} from WO {InstanceId}", baId, instanceId);
            throw;
        }
    }

    public async Task<List<ContractorAssignment>> GetAssignmentsAsync(string instanceId)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP_BA");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT_STEP_BA");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = instanceId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"        }
            };
            return (await repo.GetAsync(filters)).Select(r =>
            {
                dynamic d = r;
                return new ContractorAssignment(
                    GetStr(d, "STEP_SEQ"),
                    GetStr(d, "BA_ID"),
                    string.Empty,
                    GetStr(d, "STEP_BA_TYPE"),
                    GetDate(d, "ASSIGNED_DATE") ?? DateTime.UtcNow);
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get assignments for WO {InstanceId}", instanceId);
            return new();
        }
    }

    public async Task<ContractorQualificationResult> ValidateContractorAsync(
        string baId, string woType, string jurisdiction)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("BA_LICENSE");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "BA_LICENSE");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "BA_ID",      Operator = "=", FilterValue = baId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"  }
            };
            var licenses = (await repo.GetAsync(filters)).ToList();

            var (baseType, minLevel) = _requirements.GetValueOrDefault(woType, ("CONTRACTOR_GENERAL", 2));
            if (_jurisdictionOverride.TryGetValue((woType, jurisdiction.ToUpperInvariant()), out var overrideType))
                baseType = overrideType;

            var now     = DateTime.UtcNow;
            var matches = licenses
                .Select(l => (dynamic)l)
                .Where(l =>
                {
                    var licType  = GetStr(l, "LICENSE_TYPE");
                    var licLevel = GetInt(l, "LICENSE_LEVEL");
                    var expiry   = GetDate(l, "EXPIRY_DATE");
                    return licType == baseType && licLevel >= minLevel && expiry > now;
                })
                .ToList();

            if (matches.Count == 0)
                return new ContractorQualificationResult(false,
                    $"No valid {baseType} license (level ≥ {minLevel}) on file or license expired.", null);

            var maxExpiry = matches.Max(l => GetDate(l, "EXPIRY_DATE"));
            return new ContractorQualificationResult(true, null, maxExpiry);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Qualification check failed for BA {BaId}", baId);
            return new ContractorQualificationResult(false, $"Validation error: {ex.Message}", null);
        }
    }

    public async Task<bool> HasContractorAssignedAsync(string instanceId)
    {
        var assignments = await GetAssignmentsAsync(instanceId);
        return assignments.Count > 0;
    }

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
