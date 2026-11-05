using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Security.Claims;
using System.Text.Json;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDrillingProgramService
{
    private static void RequireCompletionRecipient(ClaimsPrincipal user,ProgramReviewer reviewer,DrillingCompletionReviewStart original,string stepInstanceId)
    {
        var scope=original.Scope();var stage=original.Workflow.Stages.SingleOrDefault(stage=>DrillingCompletionReviewStartReceipt.StepId(original.InstanceId(),stage.Definition.StepId)==stepInstanceId)??throw new InvalidOperationException("The exact original completion review stage is unavailable.");
        if(reviewer.Company!=scope.Company || reviewer.Core!=scope.CoreDatabase || reviewer.Lifecycle!=scope.LifecycleDatabase ||
            reviewer.Actor==scope.ActorUserId || reviewer.Actor==original.Source.ReportedCompletion!.RecorderUserId ||
            !reviewer.Roles.Contains(stage.Definition.AssignedRole,StringComparer.Ordinal) || !user.IsInRole(stage.Definition.AssignedRole) ||
            !stage.Authority.Recipients.Any(row=>row.UserId==reviewer.Actor && row.PersonaCode==reviewer.Persona) ||
            !reviewer.Grants.Any(row=>row.UserId==reviewer.Actor && row.DatabaseScope==scope.CoreDatabase.Fingerprint && row.OrganizationId==scope.Company.BusinessAssociateId && row.AssetType=="FIELD" && row.AssetId==scope.FieldId && row.FacilityType is null && row.IsActive && row.AccessLevel is "READ" or "WRITE" or "DELETE"))
            throw new UnauthorizedAccessException("The exact independent completion recipient, configured role/workspace and original FIELD READ are required.");
    }
    public Task<DrillingCompletionAssignedReview> ReadAssignedCompletionAsync(ClaimsPrincipal user,string processId)=>ReadAssignedCompletionCoreAsync(user,processId,null);
    private async Task<DrillingCompletionAssignedReview> ReadAssignedCompletionCoreAsync(ClaimsPrincipal user,string processId,Func<DrillingCompletionAssignedReview,Func<Task>,Func<Task>,Task>? inspect)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal))
            throw new InvalidOperationException("Open an exact assigned completion-review task.");
        var reviewer=await CaptureProgramReviewerAsync(user);var first=await reviewer.Native.ReadRecordedCompletionReviewAsync(reviewer.Company,processId,reviewer.Check);
        if(first.Process.Status!=Beep.OilandGas.Models.Processes.ProcessStatus.IN_PROGRESS)throw new InvalidOperationException("This completion review has no pending stage. Open its recorded decision history.");
        RequireCompletionRecipient(user,reviewer,first.Original,DrillingCompletionReviewStartReceipt.StepId(processId,first.Process.CurrentStepId));
        var access=await CaptureExecutionReadAsync(user,first.Original.Source.Execution.Basis.ReviewInstanceId);
        async Task Guard(){await reviewer.Check();await access.Check();await RequireReportViewAsync(user,access.Basis.Scope());}
        await Guard();var execution=await native.ReadExecutionStatusAsync(access.Basis,Guard);
        // Inspection supplies the complete closing source proof after reading the native test.
        // Keep fresh authority here without repeating that full source walk before inspection.
        var current=await native.ReadDailyReportEntryAsync(execution,first.Original.Source.LatestObservedReport!,Guard,inspect is null?access.Confirm:Guard);
        first.Original.RequireSource(current,false,false);
        var closing=await reviewer.Native.ReadRecordedCompletionReviewAsync(reviewer.Company,processId,Guard);RequireCompletionRecipient(user,reviewer,closing.Original,DrillingCompletionReviewStartReceipt.StepId(processId,closing.Process.CurrentStepId));
        if(first.NativeRevision!=closing.NativeRevision || first.Original.Revision()!=closing.Original.Revision())
            throw new InvalidOperationException("The original completion review or its complete pending task graph changed during readback.");
        var stage=closing.Original.Workflow.Stages.Single(stage=>stage.Definition.StepId==closing.Process.CurrentStepId).Definition;var step=DrillingCompletionReviewStartReceipt.StepId(processId,stage.StepId);
        var matches=closing.Tasks.Where(row=>row.PROCESS_STEP_INSTANCE_ID==step && row.ASSIGNED_ROLE==stage.AssignedRole && row.TARGET_PERSONA_CODE==reviewer.Persona && row.TASK_STATUS=="PENDING").ToArray();
        if(matches.Length!=1)throw new InvalidOperationException("The original individual completion approval task is missing or ambiguous.");
        var task=matches[0];var result=new DrillingCompletionAssignedReview(closing.Original,current,task.CROSS_TASK_ID,step,stage.AssignedRole,reviewer.Persona,
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(closing.Process)),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task)),closing.NativeRevision,"",DateTimeOffset.UtcNow);
        result=result with{Revision=result.ContentRevision()};result.Require(reviewer.Company,reviewer.Actor,processId);
        if(inspect is not null)
        {
            var confirmed=false;
            async Task ConfirmInspection()
            {
                await access.Confirm();
                var final=await reviewer.Native.ReadRecordedCompletionReviewAsync(reviewer.Company,processId,Guard);
                if(final.NativeRevision!=closing.NativeRevision || final.Original.Revision()!=closing.Original.Revision())throw new InvalidOperationException("The complete assigned completion review changed during native test inspection.");
                RequireCompletionRecipient(user,reviewer,final.Original,result.StepInstanceId);await Guard();confirmed=true;
            }
            await inspect(CompanyInventorySourceReview.Copy(result),Guard,ConfirmInspection);
            if(!confirmed)throw new InvalidOperationException("Complete closing source proof is required for native test inspection.");
            var inspected=await reviewer.Native.ReadRecordedCompletionReviewAsync(reviewer.Company,processId,Guard);
            if(inspected.NativeRevision!=closing.NativeRevision || inspected.Original.Revision()!=closing.Original.Revision())throw new InvalidOperationException("The assigned completion review changed during the closing native test read.");
            RequireCompletionRecipient(user,reviewer,inspected.Original,result.StepInstanceId);
        }
        else await access.Confirm();
        await Guard();return CompanyInventorySourceReview.Copy(result);
    }
}
