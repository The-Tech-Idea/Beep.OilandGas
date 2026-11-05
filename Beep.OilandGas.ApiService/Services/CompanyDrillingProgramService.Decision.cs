using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyDrillingProgramService
{
    public async Task<DrillingProgramReviewDecisionReceipt> DecideProgramAsync(ClaimsPrincipal user,DrillingProgramReviewDecision input,bool recover=false)
    {
        input.Require();var command=CompanyInventorySourceReview.Copy(input);var scope=command.Scope();var original=command.Review.Original;var basis=command.Review.Source.Basis;var binding=scope.LifecycleDatabase;
        if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=scope.ActorUserId || await account.RequireAsync()!=scope.ActorUserId)throw new UnauthorizedAccessException("Only the original independent decision account may record or recover this intent.");
        var access=await Capture(user,basis.Funding.Source.Current.Source.Review.InstanceId,basis.Funding.Source.AfeId,basis.Well.Uwi,false,true);var physical=editor.GetDataSource(binding.ConnectionName)??throw new InvalidOperationException("The original program lifecycle source is unavailable.");
        var reviewer=recover?null:await CaptureProgramReviewerAsync(user);if(reviewer is not null)RequireProgramRecipient(user,reviewer,original,command.Review.StepInstanceId);
        string? nextAuthority=null;
        async Task Guard()
        {
            await access.Check();var current=await company.RefreshCompanyAsync();company.RequireActor(scope.ActorUserId);
            if(current.Scope!=scope.Company || await company.ResolveAsync()!=scope.CoreDatabase || await modules.ResolveAsync("LIFECYCLE")!=binding.ConnectionName || await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],binding.ConnectionName)!=binding.Fingerprint || !ReferenceEquals(physical,editor.GetDataSource(binding.ConnectionName)))throw new UnauthorizedAccessException("The original program decision company/account or source bindings changed.");
            MigrationConnectionTarget.Validate(editor,physical,binding.ConnectionName);
            if(reviewer is not null)
            {
                await reviewer.Check();RequireProgramRecipient(user,reviewer,original,command.Review.StepInstanceId);
                if(!command.Terminal())
                {
                    var next=original.Workflow.Stages[command.Review.StageNumber];var actual=await recipients.ReadProgramAsync(command.Review.Source,next.Definition.StepId,next.Definition.AssignedRole,original);
                    if(!actual.Recipients.OrderBy(row=>row.UserId,StringComparer.Ordinal).ThenBy(row=>row.PersonaCode,StringComparer.Ordinal).SequenceEqual(next.Authority.Recipients.OrderBy(row=>row.UserId,StringComparer.Ordinal).ThenBy(row=>row.PersonaCode,StringComparer.Ordinal)))throw new InvalidOperationException("The next original program stage needs its complete current independent receiving set.");
                    var snapshot=JsonSerializer.Serialize(actual);if(nextAuthority is not null && nextAuthority!=snapshot)throw new InvalidOperationException("The next program recipients changed during this decision.");nextAuthority=snapshot;
                }
            }
            await access.Check();
        }
        async Task<DrillingProgramReviewSource> Read()
        {
            await Guard();var current=await ReadProgramReviewSourceAsync(access,await access.Read(),original.Source.Program.PROGRAM_ID,original.Source.Program.PROGRAM_DRAFT_REVISION);original.RequireSource(current,false,false);if(current.Basis.Scope()!=scope)throw new UnauthorizedAccessException("The retained program decision belongs to another original reader or source.");await access.Confirm();return current;
        }
        await Guard();if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(binding.ConnectionName) is not PPDMProcessService workflow)throw new InvalidOperationException("The original native program lifecycle owner is required.");
        var receipt=await workflow.RecordProgramReviewDecisionAsync(command,Guard,Read,recover);receipt.Require(command);if(!recover && !receipt.Recorded)throw new InvalidOperationException("The independent program decision is unconfirmed. Recover the retained original.");await access.Confirm();await Guard();return receipt;
    }
}
