using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Reporting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using System.Security.Claims;
using PpdmEntities=Beep.OilandGas.PPDM39.Models;
namespace Beep.OilandGas.ApiService.Services;
public sealed class ExecutiveAggregationService(IDMEEditor editor,ICommonColumnHandler commonColumnHandler,
    IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,CompanyModuleConnection company,
    IPPDMRowAccessPolicy rowAccess)
{
    public async Task<ExecutivePortfolioReview> GetPortfolioAsync(ClaimsPrincipal user)
    {
        var actor=user.ActingUserId();
        async Task<RepositoryCompanyContext> Access()
        {
            var current=await company.RefreshCompanyAsync();company.RequireActor(actor);
            if(user.Identity?.IsAuthenticated!=true || !current.Roles.Any(user.IsInRole))throw RefusalException.Forbidden("Current selected company/account and standard app roles are required for executive evidence.");
            return current;
        }
        var original=await Access();var roles=original.Roles.Order(StringComparer.Ordinal).ToArray();var permissions=original.Permissions.Order(StringComparer.Ordinal).ToArray();
        var binding=await company.ResolveAsync();var source=editor.GetDataSource(binding.ConnectionName)??throw RefusalException.Conflict("The original executive FIELD source is unavailable.");
        async Task Guard()
        {
            var current=await Access();
            if(!CompanyBackgroundScope.SameOwner(original,current) || !roles.SequenceEqual(current.Roles.Order(StringComparer.Ordinal)) || !permissions.SequenceEqual(current.Permissions.Order(StringComparer.Ordinal)))throw RefusalException.Forbidden("Executive company/account or app authority changed during the read.");
            if(await company.ResolveAsync()!=binding || !ReferenceEquals(source,editor.GetDataSource(binding.ConnectionName)))throw RefusalException.Conflict("Executive portfolio database/source binding changed during the read.");
        }
        await Guard();
        var repo=new PPDMGenericRepository(editor,commonColumnHandler,defaults,metadata,typeof(PpdmEntities.FIELD),binding.ConnectionName,"FIELD",rowAccess:rowAccess);
        var fields=(await repo.GetAsync([new AppFilter{FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
        foreach(var field in fields)
        {
            if(!AssetAccessLevelChecks.Meets(await rowAccess.GetEffectiveAccessLevelAsync(typeof(PpdmEntities.FIELD),"FIELD",field),"READ"))throw RefusalException.Forbidden("Current FIELD read access changed before the executive evidence was returned.");
            await Guard();
        }
        var result=ExecutivePortfolioReviews.Create(fields,original.Scope,actor,DateTimeOffset.UtcNow);await Guard();return result;
    }
}
