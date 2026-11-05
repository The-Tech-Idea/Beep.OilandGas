using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDrillingProgramService
{
    public async Task<DrillingDailyReportConflictReview> ReadDailyReportConflictAsync(ClaimsPrincipal user, DrillingDailyReportInput input)
    {
        input.Require(); var original = CompanyInventorySourceReview.Copy(input); var scope = original.Basis.Scope();
        if (user.ActingUserId() != scope.ActorUserId || await account.RequireAsync() != scope.ActorUserId ||
            (await company.RefreshCompanyAsync()).Scope != scope.Company)
            throw new UnauthorizedAccessException("Only the original company/account may reconcile this retained drilling report.");
        await RequireReportViewAsync(user, scope);
        var basis = original.Basis.Execution.Basis; var source = basis.Source.Basis; var funding = source.Funding.Source;
        var access = await Capture(user, funding.Current.Source.Review.InstanceId, funding.AfeId, source.Well.Uwi, false, true, source);
        var facts = await CaptureProgramFactsAsync(user, basis.ReviewInstanceId, true);
        var bindings = await CaptureExecutionBindingsAsync(basis);
        async Task Guard() { await access.Check(); await facts.Check(); await bindings(); await RequireReportViewAsync(user, scope); }
        async Task<DrillingExecutionStatus> Read()
        {
            await Guard(); var currentBasis = await CreateExecutionBasisAsync(user, await facts.Read(), false);
            var current = await native.ReadExecutionStatusAsync(currentBasis, Guard); original.Basis.RequireExecution(current);
            await access.Confirm(); await Guard(); return current;
        }
        var result = await native.ReadDailyReportConflictAsync(original, Guard, Read); result.Require(original);
        await access.Confirm(); await Guard(); return result;
    }
    public async Task<DrillingDailyReportConflictReview> ConfirmDailyReportConflictAsync(ClaimsPrincipal user, DrillingDailyReportConflictAcknowledgement input)
    {
        input.Require(); var frozen = CompanyInventorySourceReview.Copy(input);
        var current = await ReadDailyReportConflictAsync(user, frozen.Original);
        if (current.Revision != frozen.ReviewRevision)
            throw new InvalidOperationException("The reviewed report history or original absence changed. Preserve the original and inspect again.");
        return current;
    }
}
