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
        Log.Information("UpsertAFE WO {InstanceId} budget {Budget}", instanceId, budgetAmount);
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
            Log.Error(ex, "UpsertAFE failed for WO {InstanceId}", instanceId);
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
                throw new InvalidOperationException($"No AFE finance record for work order {instanceId}. Call UpsertAFE first.");

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
            Log.Error(ex, "AddCostLine failed for WO {InstanceId}", instanceId);
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
            Log.Error(ex, "UpdateActualCost failed for WO {InstanceId}", instanceId);
            throw;
        }
    }

    public async Task<List<CostVarianceLine>> GetVarianceSummaryAsync(string instanceId)
    {
        try
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
                decimal budget  = GetDecimal(l, "BUDGET_AMT");
                decimal actual  = GetDecimal(l, "ACTUAL_AMT");
                decimal variance = budget == 0 ? 0 : ((actual - budget) / budget) * 100;
                return new CostVarianceLine(
                    GetStr(l, "COMP_CODE"),
                    GetStr(l, "COMP_DESC"),
                    budget, actual, variance);
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetVarianceSummary failed for WO {InstanceId}", instanceId);
            return new();
        }
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
            var rows = (await repo.GetAsync(filters)).ToList();
            if (rows.Count == 0) return string.Empty;
            dynamic d = rows[0];
            return GetStr(d, "FINANCE_ID");
        }
        catch { return string.Empty; }
    }

    private PPDMGenericRepository BuildRepo(Type? entityType, string tableName) =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, tableName);

    private static string GetStr(dynamic d, string prop)
    {
        try { return (string?)d.GetType().GetProperty(prop)?.GetValue(d) ?? string.Empty; }
        catch { return string.Empty; }
    }

    private static decimal GetDecimal(dynamic d, string prop)
    {
        try
        {
            var v = d.GetType().GetProperty(prop)?.GetValue(d);
            return v is decimal dec ? dec : v is double dbl ? (decimal)dbl : v is float f ? (decimal)f : 0m;
        }
        catch { return 0m; }
    }
}
