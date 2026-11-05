using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService
{
    // Project only the complete graph returned by ReadRecordedInvestmentHandoffAsync.
    // Original/approval/history/task/source validation stays with that native reader and financial owner.
    public static DevelopmentInvestmentProgress InvestmentProgress(DevelopmentInvestmentHandoff original,ProcessInstance process,
        IReadOnlyList<CROSS_PERSONA_TASK> tasks,DevelopmentInvestmentReview source)
    {
        original.RequireSource(source,false);
        if(process.InstanceId!=original.InstanceId())throw new InvalidOperationException("The investment progress belongs to another original assignment.");
        var proofs=InvestmentProofs(process);var pending=InvestmentPending(process);var stages=new List<DevelopmentInvestmentStageProgress>();
        foreach(var stage in original.Workflow.Steps)
        {
            var proof=proofs.SingleOrDefault(item=>item.Command.Review.StepId==stage.StepId);var row=process.StepInstances.Single(item=>item.StepId==stage.StepId);
            var state=proof is not null?(proof.Command.Decision=="APPROVE"?"APPROVED":"REJECTED"):
                process.Status==ProcessStatus.IN_PROGRESS && process.CurrentStepId==stage.StepId?(pending is null?"PENDING":"RESERVED"):"WAITING";
            var at=proof is null?(DateTimeOffset?)null:new DateTimeOffset(DateTime.SpecifyKind(proof.At,DateTimeKind.Utc));
            stages.Add(new(stages.Count+1,stage.StepId,stage.StepName,stage.AssignedRole,stage.PersonaCodes.ToArray(),state,
                proof?.Command.Scope().ActorUserId,proof?.Command.Decision,proof?.Command.Rationale,at,proof?.Approval.PROCESS_APPROVAL_ID,proof?.History.PROCESS_HISTORY_ID));
        }
        var final=proofs.LastOrDefault();var financial=source.Recorded;string stateName;
        if(final?.Financial is not null)
        {
            if(final.Financial!=financial)throw new InvalidOperationException("The complete native investment outcome differs from the current canonical financial decision.");
            stateName=final.Command.Decision=="APPROVE"?"APPROVED":"REJECTED";
        }
        else if(financial is not null)
        {
            stateName="RECONCILIATION_REQUIRED";
            if(pending is not null && pending.Command.Terminal() && financial.AtUtc>=pending.Command.Review.Source.CheckedAtUtc)
            {
                // A competing valid financial record must not become this reservation's confirmed outcome.
                var expected=DevelopmentInvestmentCommand.Facts(pending.Command.FinancialCommand().ToRecord(financial.AtUtc.UtcDateTime));
                if(expected==financial)stateName="FINANCIAL_RECORDED";
            }
        }
        else stateName=pending is null?"IN_REVIEW":"RESERVED";
        var native=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Process=process,Tasks=tasks.OrderBy(task=>task.CROSS_TASK_ID,StringComparer.Ordinal)}));
        var result=new DevelopmentInvestmentProgress(process.InstanceId,original.Workflow.Version,original.Workflow.DefinitionRevision,stateName,
            tasks.Count(task=>task.TASK_STATUS=="PENDING"),stages,pending?.Command.Scope().ActorUserId,pending?.Command.Revision(),financial?.RecordedRevision,native,"");
        result=result with{Revision=result.ContentRevision()};result.Require(source,original.InstanceId(),original.Scope().ActorUserId,new DateTimeOffset(DateTime.SpecifyKind(process.StartDate,DateTimeKind.Utc)));
        return CompanyInventorySourceReview.Copy(result);
    }
}
