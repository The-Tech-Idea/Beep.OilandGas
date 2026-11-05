using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService
{
    // Project only the complete native graph returned by ReadRecordedProgramReviewAsync; no arbitrary generic process read admits an outcome.
    public static DrillingProgramReviewProgress ProgramReviewProgress(DrillingProgramReviewStart original,ProcessInstance process,IReadOnlyList<CROSS_PERSONA_TASK> tasks,DrillingProgramReviewSource source,string nativeRevision)
    {
        original.RequireSource(source,false,false);if(process.InstanceId!=original.InstanceId())throw new InvalidOperationException("The program outcome belongs to another original assignment.");
        var proofs=ProgramProofs(process);var rejected=proofs.LastOrDefault()?.Command.Decision=="REJECT";var terminal=process.Status is ProcessStatus.COMPLETED or ProcessStatus.FAILED;var stages=new List<DrillingProgramStageProgress>();
        foreach(var configured in original.Workflow.Stages)
        {
            var row=configured.Definition;var proof=proofs.SingleOrDefault(item=>item.Command.Review.StepInstanceId==DrillingProgramReviewStartReceipt.StepId(process.InstanceId,row.StepId));
            var state=proof is not null?(proof.Command.Decision=="APPROVE"?"APPROVED":"REJECTED"):terminal?"NOT_REACHED":process.CurrentStepId==row.StepId?"PENDING":"WAITING";
            stages.Add(new(stages.Count+1,row.StepId,row.StepName,row.AssignedRole,row.PersonaCodes.ToArray(),state,proof?.Command.Scope().ActorUserId,proof?.Command.Review.PersonaCode,proof?.Command.Decision,proof?.Command.Rationale,
                proof is null?null:new DateTimeOffset(DateTime.SpecifyKind(proof.At,DateTimeKind.Utc)),proof?.Command.IntentId,proof?.Approval.PROCESS_APPROVAL_ID,proof?.History.PROCESS_HISTORY_ID));
        }
        var result=new DrillingProgramReviewProgress(original,source,terminal?(rejected?"REJECTED":"APPROVED"):"IN_REVIEW",tasks.Count(task=>task.TASK_STATUS=="PENDING"),stages,nativeRevision,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};var scope=source.Basis.Scope();result.RequireFacts(scope.Company,scope.ActorUserId,process.InstanceId);return CompanyInventorySourceReview.Copy(result);
    }
}
