using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.ProductionAccounting.Services;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Company/FIELD financial admission to the canonical approved FDP, book and AFE owners.</summary>
public sealed partial class CompanyDevelopmentAfeService(RepositoryRequestAccount account,CompanyModuleConnection company,ModuleConnectionResolver modules,
    IUserAssetAccessStore grants,IAccessControlService resources,IDevelopmentPlanBaselineService baselines,ICompanyLedgerBookReader books,IAfeService budgets,IDMEEditor editor,IAuthorizationWorkflowService? investments=null)
{
    public const string PreparePermission="Accounting.PrepareDevelopmentBudget";
    internal async Task<(Func<Task> Guard,Func<Task> Confirm)> CaptureInvestmentHandoffAuthorityAsync(ClaimsPrincipal user,string processId,bool write)
    {
        var captured=await CaptureAsync(user,processId,write);var current=await company.RefreshCompanyAsync();
        if(write && !current.Permissions.Contains("Workflow.Start",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current company Workflow Start authority is required to assign investment review.");
        await captured.Guard();return(captured.Guard,captured.Confirm);
    }
    internal async Task<(Func<Task> Guard,Func<Task> Confirm,Func<DevelopmentInvestmentReview,Task<string>> ReadFunding,Func<string,Task<DevelopmentInvestmentReview>> ReadSource)> CaptureInvestmentRecipientAuthorityAsync(ClaimsPrincipal user,string processId,bool write=true)
    {
        var captured=await CaptureAsync(user,processId,write,ReviewPermission);
        async Task<string> Funding(DevelopmentInvestmentReview review)
        {
            if(review.Current.Source.Review.Scope!=captured.Source.Review.Scope || review.Current.BudgetDatabase!=captured.Budget)
                throw new UnauthorizedAccessException("The funding request belongs to another original financial source.");
            return await captured.Native.ReadDevelopmentFundingRationaleAsync(review.Current,review.AfeId,captured.Guard);
        }
        // Reuse this operation's frozen original source anchor; InvestmentCore still performs
        // fresh authority, approved-baseline, book, AFE and financial-outcome closure on every read.
        return(captured.Guard,captured.Confirm,Funding,afeId=>InvestmentCore(captured,afeId));
    }
    private sealed record Capture(DevelopmentPlanBaselineReview Source,AssetDatabaseScope Budget,Func<Task> Guard,Func<Task> Confirm,AfeService Native);
    private async Task<Capture> CaptureAsync(ClaimsPrincipal user,string processId,bool write,string writePermission=PreparePermission)
    {
        var actor=await account.RequireAsync();company.RequireActor(actor);var source=await baselines.ReadBudgetSourceAsync(user,processId);var scope=source.Review.Scope;
        if(actor!=scope.ActorUserId || source.Baseline is null)throw new UnauthorizedAccessException("The actual current actor and approved FDP baseline are required.");
        var production=await modules.ResolveAsync("PRODUCTION");var database=new AssetDatabaseScope(production,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],production));
        var nativeSource=editor.GetDataSource(production)??throw new InvalidOperationException("The company PRODUCTION source is unavailable.");
        async Task<string> Snapshot()
        {
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=actor || await account.RequireAsync()!=actor)throw new UnauthorizedAccessException("The current AFE request account changed.");
            var current=await company.RefreshCompanyAsync();company.RequireActor(actor);
            if(current.Scope!=scope.Company || !current.Roles.Any(user.IsInRole) || !new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>current.Permissions.Contains(permission,StringComparer.Ordinal)) ||
                write && !current.Permissions.Contains(writePermission,StringComparer.Ordinal) || await company.ResolveAsync()!=scope.CoreDatabase ||
                await modules.ResolveAsync("PRODUCTION")!=production || await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],production)!=database.Fingerprint || !ReferenceEquals(nativeSource,editor.GetDataSource(production)))
                throw new UnauthorizedAccessException("Current company financial preparation/view authority and original native bindings are required.");
            foreach(var (module,expected) in new[]{("DEVELOPMENT_PLANNING",scope.PlanningDatabase),("LIFECYCLE",scope.LifecycleDatabase)})
                if(await modules.ResolveAsync(module)!=expected.ConnectionName || await modules.GetMigrationBindingFingerprintAsync([module],expected.ConnectionName)!=expected.Fingerprint)
                    throw new UnauthorizedAccessException("The original FDP source binding changed.");
            MigrationConnectionTarget.Validate(editor,nativeSource,production);var rights=await grants.ReadAsync(actor,scope.CoreDatabase.Fingerprint,scope.Company.BusinessAssociateId);
            if(!rights.Any(row=>row.UserId==actor && row.DatabaseScope==scope.CoreDatabase.Fingerprint && row.OrganizationId==scope.Company.BusinessAssociateId && row.AssetType=="FIELD" && row.AssetId==scope.FieldId &&
                row.FacilityType is null && row.IsActive && (write?row.AccessLevel is "WRITE" or "DELETE":row.AccessLevel is "READ" or "WRITE" or "DELETE")))
                throw new UnauthorizedAccessException("Current exact original FIELD resource authority is required for the AFE request.");
            return JsonSerializer.Serialize(new{current.Scope,Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal),Grants=rights.OrderBy(row=>row.Id,StringComparer.Ordinal)});
        }
        var snapshot=await Snapshot();async Task Guard(){if(await Snapshot()!=snapshot)throw new UnauthorizedAccessException("Original company/account/financial rights changed during the AFE request.");}
        async Task Confirm()
        {
            await Guard();var current=await baselines.ReadBudgetSourceAsync(user,processId);current.Require(source.Review);
            if(JsonSerializer.Serialize(current.Baseline)!=JsonSerializer.Serialize(source.Baseline))throw new InvalidOperationException("The original approved FDP baseline changed.");
            if(!(await resources.CheckAssetAccessAsync(actor,scope.FieldId,"FIELD",write?"WRITE":"READ")).HasAccess)throw new UnauthorizedAccessException("Current original FIELD resource access is required for financial work.");await Guard();
        }
        await Confirm();if(budgets is not AfeService native)throw new InvalidOperationException("The canonical native AFE owner is required for development budgets.");return new(source,database,Guard,Confirm,native);
    }
    private async Task<DevelopmentAfeReview> ReviewCore(Capture capture)
    {
        await capture.Confirm();var scope=capture.Source.Review.Scope;var configured=await books.GetCompanyBooksAsync(scope.Company,scope.ActorUserId,capture.Guard);configured.Require(scope.Company,scope.ActorUserId);
        var basis=new DevelopmentAfeReview(capture.Source,capture.Budget,configured,[],"",DateTimeOffset.UtcNow);basis=basis with{Revision=basis.ContentRevision()};basis.Require();
        var result=await capture.Native.ReviewDevelopmentAfesAsync(basis,capture.Guard);var closing=await books.GetCompanyBooksAsync(scope.Company,scope.ActorUserId,capture.Guard);
        if(JsonSerializer.Serialize(configured)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The configured company books changed during the AFE review.");await capture.Confirm();result.Require();return result;
    }
    public async Task<DevelopmentAfeReview> ReviewAsync(ClaimsPrincipal user,string processId)=>await ReviewCore(await CaptureAsync(user,processId,false));
    public async Task<DevelopmentAfeReceipt> RecordAsync(ClaimsPrincipal user,DevelopmentAfeCommand original,bool recover=false)
    {
        original.Require();original=CompanyInventorySourceReview.Copy(original);var capture=await CaptureAsync(user,original.Review.Source.Review.InstanceId,!recover);var scope=capture.Source.Review.Scope;
        if(original.Review.Source.Review.Scope!=scope || original.Review.BudgetDatabase!=capture.Budget ||
            DevelopmentPlanRelease.ReviewEvidenceRevision(original.Review.Source.Review)!=DevelopmentPlanRelease.ReviewEvidenceRevision(capture.Source.Review) ||
            JsonSerializer.Serialize(original.Review.Source.Baseline)!=JsonSerializer.Serialize(capture.Source.Baseline))throw new UnauthorizedAccessException("The retained AFE belongs to another original source, account, company or binding.");
        var receipt=await capture.Native.RecordDevelopmentAfeAsync(original,capture.Guard,()=>ReviewCore(capture),recover);receipt.Require(original);await capture.Confirm();
        if(!recover && receipt.Recorded is null)throw new InvalidOperationException("The original development AFE request was not confirmed recorded. Recover its retained intent.");return receipt;
    }
}
