using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDrillingProgramService
{
    public async Task<DrillingCompletionReviewStartReceipt> SubmitCompletionReviewAsync(ClaimsPrincipal user,DrillingCompletionReviewStart input,bool recover=false)
    {
        input.Require();var original=CompanyInventorySourceReview.Copy(input);var scope=original.Scope();
        if(user.ActingUserId()!=scope.ActorUserId || await account.RequireAsync()!=scope.ActorUserId || (await company.RefreshCompanyAsync()).Scope!=scope.Company)
            throw new UnauthorizedAccessException("Only the original completion-review sender in the original company may submit or recover this intent.");
        var read=await CaptureExecutionReadAsync(user,original.Source.Execution.Basis.ReviewInstanceId);
        var program=original.Source.Execution.Basis.Source.Basis;var fundingSource=program.Funding.Source;
        var access=await Capture(user,fundingSource.Current.Source.Review.InstanceId,fundingSource.AfeId,program.Well.Uwi,!recover,recover,program,"Drilling.Edit");
        var sender=recover?null:await CaptureExecutionRecorderAsync(user,scope,true,original.SenderRole,true);
        var bindings=await CaptureExecutionBindingsAsync(original.Source.Execution.Basis);
        async Task Guard()
        {
            await read.Check();await access.Check();await bindings();await RequireReportViewAsync(user,scope);
            if(sender is not null)
            {
                await sender.Check();
                if(sender.Persona!=original.SenderPersonaCode || sender.Revision!=original.SenderAuthorityRevision ||
                    !(await company.RefreshCompanyAsync()).Permissions.Contains("Workflow.Start",StringComparer.Ordinal))
                    throw new UnauthorizedAccessException("The frozen completion sender role/workspace or workflow-start authority changed.");
                foreach(var role in original.Workflow.Stages.GroupBy(stage=>stage.Definition.AssignedRole,StringComparer.Ordinal))
                {
                    var expected=role.First().Authority;var current=await recipients.ReadCompletionAsync(original.Source,expected.StepId,expected.AssignedRole);
                    if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(expected))
                        throw new InvalidOperationException("The frozen independent completion recipients changed. Recover this original before preparing another review.");
                }
            }
        }
        async Task<DrillingDailyReportPage> Read()
        {
            // External FIELD/well resource reads stay outside both owned native
            // transactions. Current registry authority/bindings remain guarded
            // during every native operation, with resource proof on both sides.
            await Guard();await access.Confirm();var execution=await native.ReadExecutionStatusAsync(read.Basis,Guard);
            if(!recover && await native.ReadProgramWellAsync(scope.CoreDatabase,scope.FieldId,program.Well.Uwi,true,Guard)!=program.Well)
                throw new InvalidOperationException("The original active completion-review well changed.");
            var current=await native.ReadDailyReportEntryAsync(execution,original.Source.LatestObservedReport!,Guard,access.Confirm);
            original.RequireSource(current,!recover);await Guard();return current;
        }
        await Guard();var owner=await ExecutionAssignmentOwner(original.Source.Execution);
        var receipt=await owner.RecordCompletionReviewStartAsync(original,Guard,Read,recover);receipt.Require(original);
        if(!recover && !receipt.Recorded)throw new InvalidOperationException("The original completion review is unconfirmed. Recover the retained input.");
        await read.Confirm();await access.Confirm();await Guard();return receipt;
    }
}
