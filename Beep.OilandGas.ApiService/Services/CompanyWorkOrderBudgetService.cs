using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>API admission to the existing native order, configured book and AFE budget owners.</summary>
public sealed partial class CompanyWorkOrderBudgetService(CompanyModuleConnection company,ModuleConnectionResolver modules,
    ICompanyFieldSelectionService selection,ICompanyFieldSelectionAuthority preferences,IUserAssetAccessStore grants,
    IFieldOrchestrator field,IWorkOrderService orders,ICompanyLedgerBookReader books,IAfeService budgets,IAuthorizationWorkflowService? decisions=null,
    Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder.WorkOrderExecutionPermitService? labor=null,
    Beep.OilandGas.Accounting.Services.InventoryService? material=null)
{
    private const string Prepare="Accounting.PrepareWorkOrderBudget";
    private Task<(FieldSelectionReview Context,AssetDatabaseScope Budget,Func<Task> Guard)> Capture(ClaimsPrincipal user,CompanyWorkOrderBudgetQuery query,string permission=Prepare,string accessLevel="WRITE")
    {
        query.Require();return CaptureOrder(user,new(query.WorkOrderId,query.FieldId),permission,accessLevel);
    }
    private async Task<(FieldSelectionReview Context,AssetDatabaseScope Budget,Func<Task> Guard)> CaptureOrder(ClaimsPrincipal user,CompanyWorkOrderFinancialQuery query,string permission=Prepare,string accessLevel="WRITE")
    {
        query.Require();var selected=await selection.GetCurrentFieldSelectionAsync();var access=await company.RefreshCompanyAsync();var actor=user.ActingUserId();
        FieldSelectionReviewValidator.RequireCurrent(selected,access.Scope,actor);
        if(!selected.SelectionAvailable || selected.FieldId!=query.FieldId || field.CurrentFieldId!=query.FieldId)throw new UnauthorizedAccessException("The original current work-order budget FIELD is required.");
        var preference=selected.SelectionContext!;
        async Task<(FieldSelectionReview Context,AssetDatabaseScope Budget)> Snapshot()
        {
            await preferences.RequireStoredSelectionAsync(preference);var current=await company.RefreshCompanyAsync();
            if(user.Identity?.IsAuthenticated!=true || current.UserId!=actor || current.Scope!=preference.Company ||
                !current.Roles.Any(user.IsInRole) || !current.Permissions.Contains("Accounting.View",StringComparer.Ordinal) ||
                !current.Permissions.Contains(permission,StringComparer.Ordinal) || field.CurrentFieldId!=query.FieldId)
                throw new UnauthorizedAccessException("Current standard company roles and explicit work-order budget preparation capability are required.");
            var rights=await grants.ReadAsync(actor,preference.Database.Fingerprint);
            foreach(var resource in new[]{("FIELD",query.FieldId),("WORK_ORDER",query.WorkOrderId)})
            {
                var matching=rights.Where(row=>row.IsActive && row.UserId==actor && row.OrganizationId==current.Scope.BusinessAssociateId &&
                    row.DatabaseScope==preference.Database.Fingerprint && row.AssetType==resource.Item1 && row.AssetId==resource.Item2).ToArray();
                if(matching.Length!=1 || !AssetAccessLevelChecks.Meets(matching[0].AccessLevel,accessLevel))throw new UnauthorizedAccessException("Current original FIELD/order resource grants are required for the budget operation.");
            }
            var core=await modules.ResolveAsync("PPDM_CORE");var coreDatabase=new AssetDatabaseScope(core,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],core));
            var production=await modules.ResolveAsync("PRODUCTION");var budgetDatabase=new AssetDatabaseScope(production,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],production));
            var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Preference=preference,Company=current.Scope,Roles=current.Roles.Order(StringComparer.Ordinal),
                Permissions=current.Permissions.Order(StringComparer.Ordinal),Grants=rights.OrderBy(row=>row.Id,StringComparer.Ordinal),coreDatabase,budgetDatabase,Permission=permission,AccessLevel=accessLevel}));
            await preferences.RequireStoredSelectionAsync(preference);if(field.CurrentFieldId!=query.FieldId)throw new UnauthorizedAccessException("Original budget FIELD changed.");
            return(preference with{Database=coreDatabase,Revision=revision},budgetDatabase);
        }
        var original=await Snapshot();async Task Guard(){if(await Snapshot()!=original)throw new UnauthorizedAccessException("Original company/account/roles/grants/native budget binding changed.");}
        await Guard();return(original.Context,original.Budget,Guard);
    }
    private async Task<CompanyWorkOrderBudgetReview> ReviewCore(ClaimsPrincipal user,CompanyWorkOrderBudgetQuery query,
        (FieldSelectionReview Context,AssetDatabaseScope Budget,Func<Task> Guard) captured)
    {
        async Task<CompanyWorkOrderBudgetReview> Basis()
        {
            await captured.Guard();var order=CompanyInventorySourceReview.Copy(await orders.GetByIdAsync(query.FieldId,query.WorkOrderId)??throw new InvalidOperationException("Original native work-order scope is unavailable."));
            await captured.Guard();var configured=await books.GetCompanyBooksAsync(captured.Context.Company,captured.Context.ActorUserId,captured.Guard);
            configured.Require(captured.Context.Company,captured.Context.ActorUserId);var book=configured.Books.SingleOrDefault(row=>row.BookId==query.BookId)??throw new InvalidOperationException("Choose the exact recorded company book; currency is not inferred.");
            var result=new CompanyWorkOrderBudgetReview(captured.Context,captured.Budget,query,order,book,[],DateTime.UtcNow);result.Require();await captured.Guard();return result;
        }
        var first=await Basis();var result=await budgets.ReviewWorkOrderBudgetAsync(first,captured.Guard);var closing=await Basis();
        if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Original native work scope or configured book changed around budget review.");
        result.Require();await captured.Guard();return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderBudgetReview> ReviewAsync(ClaimsPrincipal user,CompanyWorkOrderBudgetQuery query)=>await ReviewCore(user,query,await Capture(user,query));
    public async Task<CompanyWorkOrderBudgetRecovery> CreateAsync(ClaimsPrincipal user,CompanyWorkOrderBudgetCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var captured=await Capture(user,command.Review.Query);
        if(command.Review.Context!=captured.Context || command.Review.BudgetDatabase!=captured.Budget)throw new UnauthorizedAccessException("Original budget company/account/FIELD and bindings are required.");
        var result=await budgets.CreateWorkOrderBudgetAsync(command,captured.Guard,()=>ReviewCore(user,command.Review.Query,captured));
        result.Require(captured.Context.Company,captured.Context.ActorUserId,new(command.Review.Query,command.IntentId));await captured.Guard();return result;
    }
    public async Task<CompanyWorkOrderBudgetRecovery> RecoverAsync(ClaimsPrincipal user,CompanyWorkOrderBudgetReference reference)
    {
        reference.Require();var captured=await Capture(user,reference.Query);
        var result=await budgets.RecoverWorkOrderBudgetAsync(captured.Context,captured.Budget,reference,captured.Guard);
        result.Require(captured.Context.Company,captured.Context.ActorUserId,reference);await captured.Guard();return result;
    }
}
