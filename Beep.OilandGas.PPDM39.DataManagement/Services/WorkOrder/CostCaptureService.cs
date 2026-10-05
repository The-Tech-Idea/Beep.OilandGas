using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

/// <remarks>
/// OILGAS-CATCH-01: a read that fails reaches the caller. Finding a work order's AFE answered "none" on a failure — so
/// adding a cost line was refused as if no AFE existed — and the variance summary answered empty. A cost line for a
/// work order with no AFE is refused in this service's words.
/// </remarks>
public class CostCaptureService : ICostCaptureService
{
    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<CostCaptureService> _logger;

    public CostCaptureService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<CostCaptureService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    public async Task UpsertAFEAsync(string instanceId, decimal budgetAmount, string userId)
    {
        _logger.LogInformation("UpsertAFE WO {InstanceId} budget {Budget}", instanceId, budgetAmount);
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("FINANCE");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "FINANCE");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = instanceId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"        }
            };
            var existing = (await repo.GetAsync(filters)).ToList();

            if (existing.Count > 0)
            {
                dynamic fin = existing[0];
                fin.BUDGET_AMT = budgetAmount;
                await repo.UpdateAsync(fin, userId);
            }
            else
            {
                dynamic fin = Activator.CreateInstance(entityType!)!;
                fin.FINANCE_ID   = Guid.NewGuid().ToString("N").ToUpperInvariant()[..20];
                fin.PROJECT_ID   = instanceId;
                fin.FINANCE_TYPE = "AFE";
                fin.BUDGET_AMT   = budgetAmount;
                fin.ACTUAL_AMT   = 0m;
                fin.ACTIVE_IND   = "Y";
                await repo.InsertAsync(fin, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpsertAFE failed for WO {InstanceId}", instanceId);
            throw;
        }
    }

    public async Task AddCostLineAsync(string instanceId, string compCode,
        decimal budgetAmt, string description, string userId)
    {
        try
        {
            var financeId = await GetFinanceIdAsync(instanceId);
            if (string.IsNullOrEmpty(financeId))
                throw RefusalException.Conflict($"Work order {instanceId} has no AFE yet; record its budget before adding cost lines.");

            var meta       = await _metadata.GetTableMetadataAsync("FIN_COMPONENT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "FIN_COMPONENT");

            dynamic line = Activator.CreateInstance(entityType!)!;
            line.FINANCE_ID = financeId;
            line.COMP_CODE  = compCode;
            line.COMP_DESC  = description;
            line.BUDGET_AMT = budgetAmt;
            line.ACTUAL_AMT = 0m;
            line.ACTIVE_IND = "Y";
            await repo.InsertAsync(line, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddCostLine failed for WO {InstanceId}", instanceId);
            throw;
        }
    }

    public async Task UpdateActualCostAsync(string instanceId, string compCode,
        decimal actualAmt, string userId)
    {
        try
        {
            var financeId = await GetFinanceIdAsync(instanceId);
            if (string.IsNullOrEmpty(financeId)) return;

            var meta       = await _metadata.GetTableMetadataAsync("FIN_COMPONENT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "FIN_COMPONENT");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "FINANCE_ID", Operator = "=", FilterValue = financeId },
                new() { FieldName = "COMP_CODE",  Operator = "=", FilterValue = compCode  },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"       }
            };
            var rows = (await repo.GetAsync(filters)).ToList();
            if (rows.Count == 0) return;

            dynamic line = rows[0];
            line.ACTUAL_AMT = actualAmt;
            await repo.UpdateAsync(line, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateActualCost failed for WO {InstanceId}", instanceId);
            throw;
        }
    }

    public async Task<List<CostVarianceLine>> GetVarianceSummaryAsync(string instanceId)
    {
        var financeId = await GetFinanceIdAsync(instanceId);
        if (string.IsNullOrEmpty(financeId)) return new();

        var meta       = await _metadata.GetTableMetadataAsync("FIN_COMPONENT");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
        var repo       = BuildRepo(entityType, "FIN_COMPONENT");

        var filters = new List<AppFilter>
        {
            new() { FieldName = "FINANCE_ID", Operator = "=", FilterValue = financeId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"       }
        };
        return (await repo.GetAsync(filters)).Select(r =>
        {
            dynamic l      = r;
            decimal budget  = WorkOrderRowValues.Decimal(l, "BUDGET_AMT");
            decimal actual  = WorkOrderRowValues.Decimal(l, "ACTUAL_AMT");
            decimal variance = budget == 0 ? 0 : ((actual - budget) / budget) * 100;
            return new CostVarianceLine(
                WorkOrderRowValues.Str(l, "COMP_CODE"),
                WorkOrderRowValues.Str(l, "COMP_DESC"),
                budget, actual, variance);
        }).ToList();
    }

    public async Task<bool> HasAFEAsync(string instanceId)
    {
        var id = await GetFinanceIdAsync(instanceId);
        return !string.IsNullOrEmpty(id);
    }

    public async Task<decimal> GetEstimatedCostAsync(string instanceId)
    {
        var lines = await GetVarianceSummaryAsync(instanceId);
        return lines.Sum(l => l.BudgetAmt);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> GetFinanceIdAsync(string instanceId)
    {
        var meta       = await _metadata.GetTableMetadataAsync("FINANCE");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
        var repo       = BuildRepo(entityType, "FINANCE");

        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = instanceId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"        }
        };
        var rows = (await repo.GetAsync(filters)).ToList();
        if (rows.Count == 0) return string.Empty;
        dynamic d = rows[0];
        return WorkOrderRowValues.Str(d, "FINANCE_ID");
    }

    private PPDMGenericRepository BuildRepo(Type? entityType, string tableName) =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, tableName);
}
