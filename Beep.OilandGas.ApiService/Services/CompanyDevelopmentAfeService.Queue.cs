using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.ProductionAccounting.Services;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyDevelopmentAfeService
{
    public async Task<DevelopmentInvestmentQueue> QueueAsync(ClaimsPrincipal user)
    {
        var actor=await account.RequireAsync();company.RequireActor(actor);var selected=(await company.RefreshCompanyAsync()).Scope;var core=await company.ResolveAsync();
        var production=await modules.ResolveAsync("PRODUCTION");var database=new AssetDatabaseScope(production,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],production));var source=editor.GetDataSource(production)??throw new InvalidOperationException("The company PRODUCTION AFE source is unavailable.");
        string[] fields=[];
        async Task<string> Snapshot()
        {
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=actor || await account.RequireAsync()!=actor)throw new UnauthorizedAccessException("The current investment queue account changed.");
            var current=await company.RefreshCompanyAsync();company.RequireActor(actor);
            if(current.Scope!=selected || !current.Roles.Any(user.IsInRole) || !new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>current.Permissions.Contains(permission,StringComparer.Ordinal)) ||
                await company.ResolveAsync()!=core || await modules.ResolveAsync("PRODUCTION")!=production || await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],production)!=database.Fingerprint || !ReferenceEquals(source,editor.GetDataSource(production)))throw new UnauthorizedAccessException("Current company investment queue View/role/native bindings are required.");
            MigrationConnectionTarget.Validate(editor,source,production);var rights=await grants.ReadAsync(actor,core.Fingerprint,selected.BusinessAssociateId);
            fields=rights.Where(row=>row.UserId==actor && row.DatabaseScope==core.Fingerprint && row.OrganizationId==selected.BusinessAssociateId && row.IsActive && row.AssetType=="FIELD" && row.FacilityType is null && row.AccessLevel is "READ" or "WRITE" or "DELETE").Select(row=>row.AssetId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            if(fields.Length>128)throw new InvalidOperationException("The current investment queue FIELD scope exceeds its bound.");return JsonSerializer.Serialize(new{current.Scope,Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal),Grants=rights.OrderBy(row=>row.Id,StringComparer.Ordinal)});
        }
        var snapshot=await Snapshot();async Task Guard(){if(await Snapshot()!=snapshot)throw new UnauthorizedAccessException("Current company/account/role/FIELD queue authority changed.");}
        var visible=new List<string>();foreach(var field in fields.ToArray()){await Guard();if((await resources.CheckAssetAccessAsync(actor,field,"FIELD","READ")).HasAccess)visible.Add(field);await Guard();}
        if(budgets is not AfeService native)throw new InvalidOperationException("The canonical native AFE owner is required for the investment queue.");
        var index=visible.Count==0?Array.Empty<DevelopmentAfeIndexItem>():await native.ReadDevelopmentIndexAsync(selected,database,visible,Guard);var items=new List<DevelopmentInvestmentQueueItem>();
        var configured=await books.GetCompanyBooksAsync(selected,actor,Guard);configured.Require(selected,actor);
        var captures=new List<Capture>();var observed=new List<DevelopmentInvestmentReview>();
        foreach(var group in index.GroupBy(row=>row.ProcessId,StringComparer.Ordinal))
        {
            await Guard();var capture=await CaptureAsync(user,group.Key,false);var scope=capture.Source.Review.Scope;
            if(scope.CoreDatabase!=core || capture.Budget!=database || group.Any(row=>row.FieldId!=scope.FieldId || row.PlanName!=capture.Source.Baseline!.FDP_NAME || row.BaselineVersion!=capture.Source.Baseline.FDP_VERSION || row.Afe.BaselineId!=capture.Source.Baseline.FDP_ID))throw new InvalidOperationException("A queue item differs from its exact approved FDP source.");
            var basis=new DevelopmentAfeReview(capture.Source,database,configured,group.Select(row=>row.Afe).ToArray(),"",DateTimeOffset.UtcNow);basis=basis with{Revision=basis.ContentRevision()};basis.Require();
            async Task Scoped(){await Guard();await capture.Guard();}
            foreach(var row in group)
            {
                var current=await InvestmentOwner().ReviewDevelopmentInvestmentAsync(basis,row.Afe.AfeId,native,Scoped);if(current.Afe()!=row.Afe)throw new InvalidOperationException("The queue request differs from its exact native source.");observed.Add(current);items.Add(new(row,current.Recorded));
            }
            await capture.Confirm();captures.Add(capture);
        }
        var closing=visible.Count==0?Array.Empty<DevelopmentAfeIndexItem>():await native.ReadDevelopmentIndexAsync(selected,database,visible,Guard);
        if(JsonSerializer.Serialize(index)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The company AFE request queue changed during source qualification.");
        foreach(var capture in captures)await capture.Confirm();
        async Task ClosingAuthority(){await Guard();foreach(var capture in captures)await capture.Guard();}
        await InvestmentOwner().ConfirmDevelopmentInvestmentQueueAsync(observed,native,ClosingAuthority);
        foreach(var field in visible){if(!(await resources.CheckAssetAccessAsync(actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current exact FIELD queue access changed.");await Guard();}
        await Guard();var result=new DevelopmentInvestmentQueue(selected,actor,core,database,items,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require(selected,actor);return result;
    }
}
