using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.LifeCycle.Models;

/// <summary>Exact internal routes for explicitly scoped, already admitted task projections.</summary>
public static class WorkflowTaskRoute
{
    public static string For(string scopeKind, string processId, string entityType)
    {
        if (string.IsNullOrWhiteSpace(processId) || processId.Length > 128 || processId.Any(char.IsControl))
            throw new InvalidOperationException("The assigned process identity is unavailable.");
        return scopeKind switch
        {
            "FIELD" when entityType=="DRILLING_DAILY_REPORT" => "/drilling/completion-review/assigned?processId=" + Uri.EscapeDataString(processId),
            "FIELD" when entityType=="DRILLING_OPERATION" => "/drilling/program-execution/assigned?processId=" + Uri.EscapeDataString(processId),
            "FIELD" when entityType=="DRILLING_PROGRAM" => "/drilling/program-review/assigned?processId=" + Uri.EscapeDataString(processId),
            "FIELD" when entityType=="AFE" => "/economics/afe?investmentHandoff=" + Uri.EscapeDataString(processId),
            "FIELD" when entityType=="FIELD_DEVELOPMENT_PLAN" => "/development/fdp/assigned?processId=" + Uri.EscapeDataString(processId),
            "FIELD" => "/ppdm39/process/instances/" + Uri.EscapeDataString(processId),
            "COMPANY_ACCOUNTING" when entityType is "AP_INVOICE" or "PURCHASE_ORDER" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT" => "/accounting/processes/" + Uri.EscapeDataString(processId),
            _ => throw new InvalidOperationException("The assigned process scope or source type is unsupported.")
        };
    }
    public static bool Matches(UnifiedTask task)
    {
        try { return task.Route == For(task.ScopeKind, task.WorkflowName, task.EntityType); }
        catch (InvalidOperationException) { return false; }
    }
}
