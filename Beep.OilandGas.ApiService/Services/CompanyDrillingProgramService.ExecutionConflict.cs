using System.Security.Claims;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDrillingProgramService
{
    public async Task<DrillingExecutionConflictReview> ReadExecutionConflictAsync(ClaimsPrincipal user,DrillingExecutionStart input)
    {
        input.Require();var original=CompanyInventorySourceReview.Copy(input);var scope=original.Basis.Scope();
        if(user.ActingUserId()!=scope.ActorUserId || await account.RequireAsync()!=scope.ActorUserId || (await company.RefreshCompanyAsync()).Scope!=scope.Company)
            throw new UnauthorizedAccessException("Only the original company/account may reconcile this retained execution start.");
        var source=original.Basis.Source.Basis;var fundingSource=source.Funding.Source;
        // Bind admission to the retained source now. The native owner's opening and
        // closing callbacks still walk the complete current execution basis.
        var access=await Capture(user,fundingSource.Current.Source.Review.InstanceId,fundingSource.AfeId,source.Well.Uwi,false,true,source);
        var bindings=await CaptureExecutionBindingsAsync(original.Basis);
        var facts=await CaptureProgramFactsAsync(user,original.Basis.ReviewInstanceId,true);
        async Task Guard(){await access.Check();await facts.Check();await bindings();}
        async Task<DrillingExecutionBasis> Read()
        {
            await Guard();var current=await CreateExecutionBasisAsync(user,await facts.Read(),false);
            original.Basis.RequireSource(current,false);await access.Confirm();await Guard();return current;
        }
        var result=await native.ReadExecutionConflictAsync(original,Guard,Read);result.Require(original);
        await access.Confirm();await Guard();return result;
    }

    public async Task<DrillingExecutionConflictReview> ConfirmExecutionConflictAsync(ClaimsPrincipal user,DrillingExecutionConflictAcknowledgement input)
    {
        input.Require();var frozen=CompanyInventorySourceReview.Copy(input);
        var current=await ReadExecutionConflictAsync(user,frozen.Original);
        if(current.Revision!=frozen.Review.Revision)
            throw new InvalidOperationException("The reviewed different execution or original absence changed. Preserve the retained original and inspect again.");
        return current;
    }
}
