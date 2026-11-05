using System.Security.Claims;
using Beep.OilandGas.Models.Core.Refusals;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Standard app roles plus fresh company/FIELD authority. Native FIELD reads bracket the owned planning transaction.</summary>
public sealed class DevelopmentPlanRequestAuthority(RepositoryRequestAccount account,CompanyModuleConnection company,
    ModuleConnectionResolver modules,ICompanyFieldSelectionService fields,ICompanyFieldSelectionAuthority preferences,
    IAccessControlService access,IUserAssetAccessStore grants) : IDevelopmentPlanRequestAuthority
{
    public async Task<DevelopmentPlanRequestScope> CaptureAsync(ClaimsPrincipal user,string permission,bool write,FieldSelectionReview? assertion=null,bool exactReview=false)
    {
        if(permission is not (PermissionConstants.DevelopmentPlanning.View or PermissionConstants.DevelopmentPlanning.Create))throw new ArgumentException("Unsupported development-planning permission.");
        var actor=await account.RequireAsync();company.RequireActor(actor);
        var database=await company.ResolveAsync();var selected=await fields.GetCurrentFieldSelectionAsync();
        var scope=company.Company.Scope;FieldSelectionReviewValidator.RequireCurrent(selected,scope,actor);
        if(!selected.SelectionAvailable)throw RefusalException.Conflict("Choose an available FIELD before opening development drafts.");
        var context=selected.SelectionContext!;
        if(context.Database!=database)throw RefusalException.Conflict("The reviewed FIELD belongs to another current database binding.");
        if(assertion is not null && (assertion.Company!=scope || assertion.ActorUserId!=actor || assertion.Database!=database || assertion.FieldId!=context.FieldId || exactReview && assertion!=context))
            throw RefusalException.Forbidden("Return to the original account/company/FIELD and review the draft before saving.");
        var planning=await modules.ResolveAsync("DEVELOPMENT_PLANNING");
        var planningRevision=await modules.GetMigrationBindingFingerprintAsync(["DEVELOPMENT_PLANNING"],planning);
        async Task<string> AppSnapshot()
        {
            if(user.Identity?.IsAuthenticated!=true || await account.RequireAsync()!=actor)throw RefusalException.Forbidden("The development account changed.");
            var current=await company.RefreshCompanyAsync();company.RequireActor(actor);
            if(current.Scope!=scope || current.Roles.Count==0 || !current.Roles.Any(user.IsInRole) || !current.Permissions.Contains(permission,StringComparer.Ordinal) ||
                await company.ResolveAsync()!=database || await modules.ResolveAsync("DEVELOPMENT_PLANNING")!=planning ||
                await modules.GetMigrationBindingFingerprintAsync(["DEVELOPMENT_PLANNING"],planning)!=planningRevision)
                throw RefusalException.Forbidden("Current company roles, development permission or module bindings changed.");
            await preferences.RequireStoredSelectionAsync(context);
            var rows=await grants.ReadAsync(actor,database.Fingerprint,scope.BusinessAssociateId);
            return JsonSerializer.Serialize(new{Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal),
                Grants=rows.OrderBy(row=>row.Id,StringComparer.Ordinal)});
        }
        var captured=await AppSnapshot();
        async Task Check()
        {
            if(await AppSnapshot()!=captured)throw RefusalException.Forbidden("Original development roles, permissions or asset grants changed. Review again.");
        }
        async Task Confirm()
        {
            await Check();
            var current=await fields.GetCurrentFieldSelectionAsync();FieldSelectionReviewValidator.RequireCurrent(current,scope,actor);
            if(!current.SelectionAvailable || current.SelectionContext!=context || !(await access.CheckAssetAccessAsync(actor,context.FieldId!,"FIELD",write?"WRITE":"READ")).HasAccess)
                throw RefusalException.Forbidden("Current original FIELD authority could not be confirmed.");
            await Check();
        }
        await Confirm();return new(context,new(planning,planningRevision),Check,Confirm);
    }
}
