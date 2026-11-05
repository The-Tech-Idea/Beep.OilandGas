using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>Admission rules shared by the development catalog and native start owner.</summary>
public static class DevelopmentReviewDefinition
{
    public static IReadOnlyList<ProcessStepDefinition> Require(ProcessDefinition definition)
        =>Require(definition,"DEVELOPMENT","FIELD_DEVELOPMENT_PLAN");
    public static IReadOnlyList<ProcessStepDefinition> RequireInvestment(ProcessDefinition definition)
        =>RequireOrdered(definition,"ACCOUNTING","AFE");
    public static IReadOnlyList<ProcessStepDefinition> RequireProgram(ProcessDefinition definition)
        =>RequireOrdered(definition,"DRILLING","DRILLING_PROGRAM");
    public static IReadOnlyList<ProcessStepDefinition> RequireCompletion(ProcessDefinition definition)
        =>RequireOrdered(definition,"DRILLING","DRILLING_DAILY_REPORT");

    public static ProcessStepDefinition RequireExecutionReporting(ProcessDefinition definition)
    {
        if (!definition.IsActive || definition.ProcessType != "DRILLING" || definition.EntityType != "DRILLING_OPERATION" ||
            !LeaseSummaryValidator.Identifier(definition.ProcessId) || definition.Steps is not { Count: 1 } ||
            definition.Transitions?.Count > 0 || definition.Configuration?.Keys.Any(key => key is not ("Steps" or "Transitions")) == true)
            throw new InvalidOperationException("Execution reporting requires one explicitly configured ongoing data-entry responsibility.");
        var step = definition.Steps[0];
        if (step is null || step.StepType != "DATA_ENTRY" || step.RequiresApproval || !step.IsRequired ||
            !LeaseSummaryValidator.Identifier(step.StepId) || string.IsNullOrWhiteSpace(step.StepName) ||
            step.StepName.Length > 256 || step.StepName.Any(char.IsControl) || step.SequenceNumber < 0 || step.SlaHours < 0 ||
            step.RequiredRoles is not { Count: 1 } || !LeaseSummaryValidator.Identifier(step.RequiredRoles[0]) ||
            !string.IsNullOrWhiteSpace(step.NextStepId) || step.ValidationRules?.Count > 0 ||
            step.StepConfiguration?.Count > 0 || step.ConditionalNextSteps?.Count > 0)
            throw new InvalidOperationException("Execution reporting requires a named mandatory data-entry step and one configured receiving role without alternate routing.");
        return step;
    }
    private static IReadOnlyList<ProcessStepDefinition> RequireOrdered(ProcessDefinition definition,string processType,string entityType)
    {
        var steps=Require(definition,processType,entityType);
        if(steps.Any(step=>!step.IsRequired || !string.IsNullOrWhiteSpace(step.NextStepId) || string.IsNullOrWhiteSpace(step.StepName) || step.StepName.Length>256 || step.StepName.Any(char.IsControl)))
            throw new InvalidOperationException("Review routing requires named mandatory sequential approval stages without alternate next-step semantics.");
        return steps;
    }
    private static IReadOnlyList<ProcessStepDefinition> Require(ProcessDefinition definition,string processType,string entityType)
    {
        if(!definition.IsActive || definition.ProcessType!=processType || definition.EntityType!=entityType ||
            !LeaseSummaryValidator.Identifier(definition.ProcessId) || definition.Steps is null || definition.Steps.Count is <1 or >64 ||
            definition.Steps.Any(step=>step is null || !step.RequiresApproval || step.StepType!="APPROVAL" ||
                !LeaseSummaryValidator.Identifier(step.StepId) || step.SequenceNumber<0 || step.SlaHours<0 ||
                step.RequiredRoles is null || step.RequiredRoles.Count!=1 || !LeaseSummaryValidator.Identifier(step.RequiredRoles[0]) ||
                step.ValidationRules?.Count>0 || step.StepConfiguration?.Count>0 || step.ConditionalNextSteps?.Count>0) ||
            definition.Steps.Select(step=>step.StepId).Distinct(StringComparer.Ordinal).Count()!=definition.Steps.Count ||
            definition.Steps.Select(step=>step.SequenceNumber).Distinct().Count()!=definition.Steps.Count ||
            definition.Transitions?.Count>0 || definition.Configuration?.Keys.Any(key=>key is not ("Steps" or "Transitions"))==true)
            throw new InvalidOperationException("Development review workflows need bounded explicit approval steps and their assigned roles.");
        return definition.Steps.OrderBy(step=>step.SequenceNumber).ToArray();
    }
}
