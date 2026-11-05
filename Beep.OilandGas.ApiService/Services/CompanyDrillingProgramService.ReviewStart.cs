using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyDrillingProgramService
{
    public async Task<DrillingProgramReviewStartReceipt> SubmitReviewAsync(ClaimsPrincipal user,DrillingProgramReviewStart input,bool recover=false)
    {
        input.Require();var original=CompanyInventorySourceReview.Copy(input);var scope=original.Scope();var fundingSource=original.Source.Basis.Funding.Source;var processId=fundingSource.Current.Source.Review.InstanceId;
        var access=await Capture(user,processId,fundingSource.AfeId,original.Source.Basis.Well.Uwi,!recover,recover);var binding=scope.LifecycleDatabase;var source=editor.GetDataSource(binding.ConnectionName)??throw new InvalidOperationException("The original program-review lifecycle source is unavailable.");
        async Task Guard()
        {
            await access.Check();var current=await company.RefreshCompanyAsync();company.RequireActor(scope.ActorUserId);
            if(user.ActingUserId()!=scope.ActorUserId || current.Scope!=scope.Company || await company.ResolveAsync()!=scope.CoreDatabase || await modules.ResolveAsync("LIFECYCLE")!=binding.ConnectionName || await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],binding.ConnectionName)!=binding.Fingerprint || !ReferenceEquals(source,editor.GetDataSource(binding.ConnectionName)) ||
                !recover && (!current.Permissions.Contains("Drilling.Create",StringComparer.Ordinal) || !current.Permissions.Contains("Workflow.Start",StringComparer.Ordinal)))throw new UnauthorizedAccessException("The retained program review requires its current original company/account, source bindings and submission authority.");
            MigrationConnectionTarget.Validate(editor,source,binding.ConnectionName);
            if(!recover)
                foreach(var role in original.Workflow.Stages.GroupBy(stage=>stage.Definition.AssignedRole,StringComparer.Ordinal))
                {
                    var expected=role.First().Authority;var actual=await recipients.ReadProgramAsync(original.Source,expected.StepId,expected.AssignedRole);if(JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The frozen independent program recipients changed. Recover the original assignment before preparing another.");
                }
            await access.Check();
        }
        async Task<DrillingProgramReviewSource> Read()
        {
            await Guard();var current=await ReadProgramReviewSourceAsync(access,await access.Read(),original.Source.Program.PROGRAM_ID,original.Source.Program.PROGRAM_DRAFT_REVISION);original.RequireSource(current,!recover);await access.Confirm();return current;
        }
        await Guard();if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(binding.ConnectionName) is not PPDMProcessService workflow)throw new InvalidOperationException("The canonical original lifecycle owner is required for program assignment.");
        var result=await workflow.RecordProgramReviewStartAsync(original,Guard,Read,recover);result.Require(original);if(!recover && !result.Recorded)throw new InvalidOperationException("The original program assignment was not confirmed recorded. Recover the retained original.");await access.Confirm();await Guard();return result;
    }
}
