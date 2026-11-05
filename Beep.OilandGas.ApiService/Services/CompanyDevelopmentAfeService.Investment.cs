using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ProductionAccounting.Services;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDevelopmentAfeService
{
    public const string ReviewPermission="Accounting.ReviewDevelopmentBudget";
    private AuthorizationWorkflowService InvestmentOwner()=>investments as AuthorizationWorkflowService??throw new InvalidOperationException("The canonical native approval-workflow owner is required for investment decisions.");
    private async Task<DevelopmentInvestmentReview> InvestmentCore(Capture capture,string afeId)
    {
        var basis=await ReviewCore(capture);var owner=InvestmentOwner();var first=await owner.ReviewDevelopmentInvestmentAsync(basis,afeId,capture.Native,capture.Guard);
        var closing=await ReviewCore(capture);if(closing.Revision!=basis.Revision)throw new InvalidOperationException("The approved FDP, configured company books or complete AFE requests changed during investment review.");
        var after=await owner.ReviewDevelopmentInvestmentAsync(closing,afeId,capture.Native,capture.Guard);
        if(first.Recorded!=after.Recorded || !first.Blockers.SequenceEqual(after.Blockers,StringComparer.Ordinal))throw new InvalidOperationException("The recorded investment outcome changed during review.");await capture.Confirm();after.Require();return after;
    }
    public async Task<DevelopmentInvestmentReview> ReviewInvestmentAsync(ClaimsPrincipal user,string processId,string afeId)
    {
        if(!LeaseSummaryValidator.Identifier(afeId))throw new InvalidOperationException("Choose a recorded source-bound AFE request.");return await InvestmentCore(await CaptureAsync(user,processId,false),afeId);
    }
    public async Task<DevelopmentInvestmentReceipt> DecideInvestmentAsync(ClaimsPrincipal user,DevelopmentInvestmentCommand original,bool recover=false)
    {
        original.Require();original=CompanyInventorySourceReview.Copy(original);var before=original.Review.Current;
        var capture=await CaptureAsync(user,before.Source.Review.InstanceId,!recover,ReviewPermission);var scope=capture.Source.Review.Scope;
        if(before.Source.Review.Scope!=scope || before.BudgetDatabase!=capture.Budget || DevelopmentPlanRelease.ReviewEvidenceRevision(before.Source.Review)!=DevelopmentPlanRelease.ReviewEvidenceRevision(capture.Source.Review) ||
            JsonSerializer.Serialize(before.Source.Baseline)!=JsonSerializer.Serialize(capture.Source.Baseline))throw new UnauthorizedAccessException("The original investment review belongs to another account, company, approved source or native binding.");
        var receipt=await InvestmentOwner().RecordDevelopmentInvestmentAsync(original,capture.Native,capture.Guard,()=>InvestmentCore(capture,original.Review.AfeId),recover);receipt.Require(original);await capture.Confirm();
        if(!recover && receipt.Recorded is null)throw new InvalidOperationException("The original investment decision is unconfirmed. Recover its exact retained input.");return receipt;
    }
}
