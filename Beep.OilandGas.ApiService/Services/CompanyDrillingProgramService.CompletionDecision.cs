using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDrillingProgramService
{
    public async Task<DrillingCompletionReviewDecisionReceipt> DecideCompletionReviewAsync(ClaimsPrincipal user,DrillingCompletionReviewDecision input,bool recover=false)
    {
        input.Require();var command=CompanyInventorySourceReview.Copy(input);var original=command.Review.Original;var scope=command.Scope();
        if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=scope.ActorUserId || await account.RequireAsync()!=scope.ActorUserId || (await company.RefreshCompanyAsync()).Scope!=scope.Company)
            throw new UnauthorizedAccessException("Only the original independent completion decision account may submit or recover this intent.");
        var access=await CaptureExecutionReadAsync(user,original.Source.Execution.Basis.ReviewInstanceId);
        if(access.Basis.Scope()!=scope)throw new UnauthorizedAccessException("The original completion decision source belongs to another account, company, FIELD or binding.");
        var reviewer=recover?null:await CaptureProgramReviewerAsync(user);string? nextAuthority=null;int sourceReads=0;
        async Task Guard()
        {
            await access.Check();await RequireReportViewAsync(user,scope);
            if(reviewer is not null)
            {
                await reviewer.Check();RequireCompletionRecipient(user,reviewer,original,command.Review.StepInstanceId);
                if(!command.Terminal())
                {
                    var next=original.Workflow.Stages[command.Review.StageNumber];var current=await recipients.ReadCompletionReviewAsync(command.Review.Source,next.Definition.StepId,next.Definition.AssignedRole,original);
                    if(!current.Recipients.OrderBy(row=>row.UserId,StringComparer.Ordinal).ThenBy(row=>row.PersonaCode,StringComparer.Ordinal).SequenceEqual(next.Authority.Recipients.OrderBy(row=>row.UserId,StringComparer.Ordinal).ThenBy(row=>row.PersonaCode,StringComparer.Ordinal)))
                        throw new InvalidOperationException("The next completion review stage requires its complete current independent receiving set.");
                    var snapshot=JsonSerializer.Serialize(current);if(nextAuthority is not null && snapshot!=nextAuthority)throw new InvalidOperationException("The next completion recipients changed during this decision.");nextAuthority=snapshot;
                }
            }
            await access.Check();
        }
        async Task<DrillingDailyReportPage> Read()
        {
            await Guard();sourceReads++;var execution=await native.ReadExecutionStatusAsync(access.Basis,Guard);
            var page=await native.ReadDailyReportEntryAsync(execution,original.Source.LatestObservedReport!,Guard,()=>sourceReads>1?access.Confirm():Task.CompletedTask);
            original.RequireSource(page,false,false);await Guard();return page;
        }
        await Guard();var owner=await ExecutionAssignmentOwner(original.Source.Execution);
        async Task ConfirmAcceptance(DrillingCompletionAcceptance acceptance)
        {
            acceptance.Require(command.Review);
            foreach(var test in acceptance.Tests)
            {
                await Guard();var current=await native.InspectCompletionTestAsync(test.Selection(command.Review),Guard,()=>Task.CompletedTask);
                if(current.Revision!=test.InspectionRevision || current.Native!=test.Native)throw new InvalidOperationException("An independently checked native test changed before acceptance acknowledgement. Preserve and recover the original decision.");
            }
            await Guard();
        }
        var receipt=await owner.RecordCompletionReviewDecisionAsync(command,Guard,Read,recover,ConfirmAcceptance);receipt.Require(command);
        if(!recover && !receipt.Recorded)throw new InvalidOperationException("The original independent completion decision is unconfirmed. Recover its retained input.");
        await Guard();return receipt;
    }
    public async Task<DrillingCompletionReviewProgress> ReadCompletionProgressAsync(ClaimsPrincipal user,string processId)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Open an exact recorded completion review.");
        var actor=await account.RequireAsync();var current=(await company.RefreshCompanyAsync()).Scope;
        var connection=await modules.ResolveAsync("LIFECYCLE");
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(connection) is not PPDMProcessService owner)throw new InvalidOperationException("The canonical native completion review owner is required.");
        async Task InitialGuard()
        {
            var selected=await company.RefreshCompanyAsync();
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=actor || await account.RequireAsync()!=actor || selected.Scope!=current || !selected.Roles.Any(user.IsInRole) ||
                !new[]{"Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal)) || await modules.ResolveAsync("LIFECYCLE")!=connection)
                throw new UnauthorizedAccessException("The completion history account, company, current view roles or source changed.");
        }
        var first=await owner.ReadRecordedCompletionReviewAsync(current,processId,InitialGuard);var original=first.Original;
        if(actor!=original.Scope().ActorUserId && actor!=original.Source.ReportedCompletion!.RecorderUserId && !original.Workflow.Stages.Any(stage=>stage.Authority.Recipients.Any(row=>row.UserId==actor)))throw new UnauthorizedAccessException("Only an original completion review participant may open its decision history.");
        var access=await CaptureExecutionReadAsync(user,original.Source.Execution.Basis.ReviewInstanceId);var scope=access.Basis.Scope();
        if(scope.Company!=current || scope.LifecycleDatabase!=original.Scope().LifecycleDatabase)throw new UnauthorizedAccessException("The original completion history source binding changed.");
        async Task Guard(){await InitialGuard();await access.Check();await RequireReportViewAsync(user,scope);}
        var execution=await native.ReadExecutionStatusAsync(access.Basis,Guard);var page=await native.ReadDailyReportEntryAsync(execution,original.Source.LatestObservedReport!,Guard,access.Confirm);
        var closing=await owner.ReadRecordedCompletionReviewAsync(current,processId,Guard);
        if(first.NativeRevision!=closing.NativeRevision || original.Revision()!=closing.Original.Revision())throw new InvalidOperationException("The complete original completion review changed during history reload.");
        var result=PPDMProcessService.CompletionReviewProgress(closing.Original,closing.Process,closing.Tasks,page,closing.NativeRevision);result.Require(current,actor,processId);
        await access.Confirm();await Guard();return result;
    }
}
