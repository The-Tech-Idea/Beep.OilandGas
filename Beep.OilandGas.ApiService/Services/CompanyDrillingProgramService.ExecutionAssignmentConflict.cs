using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Security.Claims;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyDrillingProgramService
{
    public async Task<DrillingExecutionAssignmentConflictReview> ReadExecutionAssignmentConflictAsync(ClaimsPrincipal user,DrillingExecutionAssignment input)
    {
        input.Require();var original=CompanyInventorySourceReview.Copy(input);var scope=original.Scope();
        if(user.ActingUserId()!=scope.ActorUserId || await account.RequireAsync()!=scope.ActorUserId || (await company.RefreshCompanyAsync()).Scope!=scope.Company)
            throw new UnauthorizedAccessException("Only the original company/account may reconcile this retained reporting assignment.");
        await RequireReportViewAsync(user,scope);
        var basis=original.Source.Basis.Source.Basis;var fundingSource=basis.Funding.Source;
        var access=await Capture(user,fundingSource.Current.Source.Review.InstanceId,fundingSource.AfeId,basis.Well.Uwi,false,true,basis);
        var facts=await CaptureProgramFactsAsync(user,original.Source.Basis.ReviewInstanceId,true);
        var bindings=await CaptureExecutionBindingsAsync(original.Source.Basis);
        async Task Guard(){await access.Check();await facts.Check();await bindings();await RequireReportViewAsync(user,scope);}
        async Task<DrillingExecutionStatus> Read()
        {
            await Guard();var currentBasis=await CreateExecutionBasisAsync(user,await facts.Read(),false);
            var current=await native.ReadExecutionStatusAsync(currentBasis,Guard);original.RequireSource(current);
            await access.Confirm();await Guard();return current;
        }
        var owner=await ExecutionAssignmentOwner(original.Source);var result=await owner.ReadExecutionAssignmentConflictAsync(original,Guard,Read);
        result.Require(original);await access.Confirm();await Guard();return result;
    }
    public async Task<DrillingExecutionAssignmentConflictReview> ConfirmExecutionAssignmentConflictAsync(ClaimsPrincipal user,DrillingExecutionAssignmentConflictAcknowledgement input)
    {
        input.Require();var frozen=CompanyInventorySourceReview.Copy(input);var current=await ReadExecutionAssignmentConflictAsync(user,frozen.Original);
        if(current.Revision!=frozen.ReviewRevision)throw new InvalidOperationException("The reviewed reporting responsibility or original absence changed. Preserve the original and inspect again.");
        return current;
    }
}
