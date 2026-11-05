using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Models.Processes;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Task/process relationship checks after the owning read boundary admits the process.</summary>
public static class WorkflowTaskProcessAdmission
{
    public static bool CompanyMatches(ProcessInstance process,CompanyScope company)
    {
        try
        {
        var payload=System.Text.Json.JsonSerializer.Serialize(process.ProcessData);
        if(process.EntityType=="DRILLING_DAILY_REPORT" || process.InstanceId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal) || DrillingCompletionReviewStart.HasPayload(payload))
            return DrillingCompletionReviewStart.OwnerCompany(payload)==company;
        if(process.EntityType=="DRILLING_OPERATION" || process.InstanceId.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal) || DrillingExecutionAssignment.HasPayload(payload))
            return DrillingExecutionAssignment.OwnerCompany(payload)==company;
        if(process.EntityType=="DRILLING_PROGRAM" || process.InstanceId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal) || DrillingProgramReviewStart.HasPayload(payload))
            return DrillingProgramReviewStart.OwnerCompany(payload)==company;
        if(process.EntityType=="AFE" || process.InstanceId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal) || DevelopmentInvestmentHandoff.HasPayload(payload))
            return DevelopmentInvestmentHandoff.OwnerCompany(payload)==company;
        if(process.EntityType=="FIELD_DEVELOPMENT_PLAN" || process.InstanceId.StartsWith(DevelopmentReviewStart.InstancePrefix,StringComparison.Ordinal))
            return DevelopmentReviewStart.FromPayload(payload,process.InstanceId,process.FieldId,process.EntityId).Source.Context.Company==company;
        if(!process.InstanceId.StartsWith(CompanyMaintenanceProcessStart.InstancePrefix,StringComparison.Ordinal) && !CompanyMaintenanceProcessStart.HasPayload(payload))return true;
        if(process.EntityType!="WORK_ORDER" || string.IsNullOrWhiteSpace(process.FieldId))throw new InvalidOperationException("The recorded maintenance task has no FIELD/order ownership.");
        return CompanyMaintenanceProcessStart.FromPayload(payload,process.InstanceId,process.FieldId,process.EntityId).Context.Company==company;
        }
        catch(Exception invalid) when(invalid is System.Text.Json.JsonException or KeyNotFoundException){throw new InvalidOperationException("The recorded task ownership is malformed.",invalid);}
    }
    public static bool Matches(UnifiedTask task, ProcessInstance? process, string actor)
    {
        if(process is not null && (process.EntityType=="DRILLING_DAILY_REPORT" || process.InstanceId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal) || process.ProcessData?.Data?.ContainsKey(DrillingCompletionReviewStart.PayloadKey)==true))
        {
            try{DrillingCompletionReviewStart.FromPayload(System.Text.Json.JsonSerializer.Serialize(process.ProcessData),process.InstanceId,process.FieldId,process.EntityId);}
            catch(Exception invalid) when(invalid is InvalidOperationException or System.Text.Json.JsonException or KeyNotFoundException){return false;}
        }
        if (process is null || (process.InstanceId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal) || process.ProcessData?.Data?.ContainsKey(DrillingCompletionReviewStart.PayloadKey)==true) && process.EntityType!="DRILLING_DAILY_REPORT" || process.InstanceId != task.WorkflowName || process.EntityId != task.EntityId || process.EntityType != task.EntityType ||
            task.Status != "PENDING" || string.IsNullOrWhiteSpace(task.AssignedRole) || !WorkflowTaskRoute.Matches(task) ||
            (task.ScopeKind == "FIELD") != !string.IsNullOrWhiteSpace(process.FieldId) || process.StepInstances is null) return false;
        var steps = process.StepInstances.Where(step => step.StepInstanceId == task.StepName).ToArray();
        if (steps.Length != 1 || steps[0].InstanceId != process.InstanceId || steps[0].Status is not (StepStatus.PENDING or StepStatus.IN_PROGRESS)) return false;
        var step=steps[0];
        if(process.Status!=ProcessStatus.IN_PROGRESS || step.RequiredRole!=task.AssignedRole || step.StepId!=process.CurrentStepId ||
            !string.IsNullOrWhiteSpace(step.AssignedTo) && step.AssignedTo!=actor && step.AssignedTo!=task.AssignedRole)return false;
        return true;
    }
}
