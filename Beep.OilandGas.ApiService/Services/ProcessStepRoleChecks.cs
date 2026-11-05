using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Security.Claims;
using Beep.OilandGas.Models.Processes;
using System.Text.Json;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Standard app-role checks and recorded maker/checker constraints for the existing process step gate.</summary>
public static class ProcessStepRoleChecks
{
    public static bool RequiresIndependentActor(ProcessStepInstance step, ProcessDefinition definition, ProcessStepDefinition template) =>
        step.ApprovalRequired || template.RequiresApproval || template.StepType=="APPROVAL" ||
        definition.ProcessType=="FINANCIAL" && template.StepType=="REVIEW";
    public static bool CanAct(ClaimsPrincipal user, string actor, ProcessInstance instance, ProcessStepInstance step, ProcessDefinition definition, ProcessStepDefinition template)
    {
        if(user.Identity?.IsAuthenticated!=true || string.IsNullOrWhiteSpace(actor) || definition.ProcessId!=instance.ProcessId ||
            template.StepId!=step.StepId || instance.CurrentStepId!=step.StepId || template.RequiredRoles is null) return false;
        var independent = RequiresIndependentActor(step,definition,template);
        if(independent && !CanIndependentlyCheck(instance,actor)) return false;
        if(user.IsInRole("Administrator")) return true;
        var assigned=!string.IsNullOrWhiteSpace(step.AssignedTo);
        if(assigned && step.AssignedTo!=actor && !user.IsInRole(step.AssignedTo!)) return false;
        if(!string.IsNullOrWhiteSpace(step.RequiredRole) && !user.IsInRole(step.RequiredRole)) return false;
        if(template.RequiredRoles.Count>0 && !template.RequiredRoles.Any(user.IsInRole)) return false;
        return assigned || !string.IsNullOrWhiteSpace(step.RequiredRole) || template.RequiredRoles.Count>0;
    }
    public static bool CanTransition(ClaimsPrincipal user, string actor, ProcessInstance instance, ProcessDefinition definition, string from, string to, string trigger)
    {
        if(user.Identity?.IsAuthenticated!=true || string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(trigger) ||
            instance.CurrentState!=from || definition.ProcessId!=instance.ProcessId || definition.Transitions is null)return false;
        var matches=definition.Transitions.Values.Where(row=>row.FromStateId==from && row.ToStateId==to && row.Trigger==trigger).ToArray();
        if(matches.Length!=1 || matches[0].RequiredRoles is null)return false;
        var selected=matches[0];
        if(selected.RequiresApproval && !CanIndependentlyCheck(instance,actor))return false;
        return user.IsInRole("Administrator") || selected.RequiredRoles.Count==0 || selected.RequiredRoles.Any(user.IsInRole);
    }

    private static bool CanIndependentlyCheck(ProcessInstance instance,string actor)
    {
        if(string.IsNullOrWhiteSpace(instance.StartedBy) || instance.StartedBy==actor)return false;
        if(!string.IsNullOrEmpty(instance.FieldId) || instance.EntityType is not ("AP_INVOICE" or "PURCHASE_ORDER" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT"))return true;
        try
        {
            if(instance.ProcessData?.InstanceId!=instance.InstanceId || instance.ProcessData.Data is null ||
                !instance.ProcessData.Data.TryGetValue(CompanyAccountingProcessStart.PayloadKey,out var payload) ||
                !instance.ProcessData.Data.TryGetValue(CompanyAccountingProcessBinding.PayloadKey,out var owner))return false;
            var start=JsonSerializer.Deserialize<CompanyAccountingProcessStart>(JsonSerializer.Serialize(payload));
            if(start is null)return false;start.Require();
            CompanyAccountingProcessBinding.FromJson(JsonSerializer.Serialize(owner),start.Company,instance.InstanceId,instance.EntityType,instance.EntityId);
            return start.InstanceId()==instance.InstanceId && start.ActorUserId==instance.StartedBy && start.ProcessId==instance.ProcessId &&
                start.EntityType==instance.EntityType && start.EntityId==instance.EntityId && start.SourcePreparerUserId!=actor;
        }
        catch(Exception invalid) when(invalid is JsonException or InvalidOperationException or ArgumentException){return false;}
    }
}
