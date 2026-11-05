using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Fresh app-owned admission around a company/FIELD facility read.</summary>
public sealed class DevelopmentFacilityReadAuthority(RepositoryRequestAccount account,CompanyModuleConnection company,
    ICompanyFieldSelectionService fields,ICompanyFieldSelectionAuthority preferences,IAccessControlService access,IUserAssetAccessStore grants)
{
    public Task<DevelopmentFacilityReadScope> CaptureAsync(ClaimsPrincipal user)=>CaptureCoreAsync(user,null,null,null,false,false);
    public Task<DevelopmentFacilityReadScope> CaptureCreateAsync(ClaimsPrincipal user,string creatorRole)
    {
        if(!LeaseSummaryValidator.Identifier(creatorRole))throw RefusalException.Invalid("Select a current creator role.");
        return CaptureCoreAsync(user,null,null,creatorRole,false,true);
    }
    public Task<DevelopmentFacilityReadScope> CaptureEditAsync(ClaimsPrincipal user,string facilityId,string facilityType,string editorRole)
    {
        if(!LeaseSummaryValidator.Identifier(facilityId) || !LeaseSummaryValidator.Identifier(facilityType) || !LeaseSummaryValidator.Identifier(editorRole))
            throw RefusalException.Invalid("Select an exact facility identity and current editor role.");
        return CaptureCoreAsync(user,facilityId,facilityType,editorRole,false,false);
    }
    public Task<DevelopmentFacilityReadScope> CaptureRecoveryAsync(ClaimsPrincipal user,string facilityId,string facilityType)
    {
        if(!LeaseSummaryValidator.Identifier(facilityId) || !LeaseSummaryValidator.Identifier(facilityType))throw RefusalException.Invalid("Select the exact original facility identity.");
        return CaptureCoreAsync(user,facilityId,facilityType,null,true,false);
    }
    private async Task<DevelopmentFacilityReadScope> CaptureCoreAsync(ClaimsPrincipal user,string? facilityId,string? facilityType,string? editorRole,bool recovery,bool creation)
    {
        var actor=await account.RequireAsync();
        if(user.Identity?.IsAuthenticated!=true || user.FindActingUserId()!=actor)throw RefusalException.Forbidden("The current facility reader account is required.");
        company.RequireActor(actor);var database=await company.ResolveAsync();var scope=company.Company.Scope;
        var selection=await fields.GetCurrentFieldSelectionAsync();FieldSelectionReviewValidator.RequireCurrent(selection,scope,actor);
        if(!selection.SelectionAvailable || selection.SelectionContext!.Database!=database)throw RefusalException.Conflict("Choose a currently reviewed company FIELD for facility evidence.");
        var original=selection.SelectionContext;
        async Task<string> CheckApp()
        {
            if(user.Identity?.IsAuthenticated!=true || user.FindActingUserId()!=actor || await account.RequireAsync()!=actor)throw RefusalException.Forbidden("The facility reader account changed.");
            var current=await company.RefreshCompanyAsync();company.RequireActor(actor);
            if(current.Scope!=scope || current.Roles.Count==0 || !current.Roles.Any(user.IsInRole) || !current.Permissions.Contains(PermissionConstants.DevelopmentPlanning.View,StringComparer.Ordinal) ||
                editorRole is not null && (!current.Roles.Contains(editorRole,StringComparer.Ordinal) || !user.IsInRole(editorRole) || !current.Permissions.Contains(creation?PermissionConstants.DevelopmentPlanning.Create:PermissionConstants.DevelopmentPlanning.Edit,StringComparer.Ordinal)) ||
                await company.ResolveAsync()!=database)
                throw RefusalException.Forbidden("Current company roles, development view permission or database binding changed.");
            await preferences.RequireStoredSelectionAsync(original);
            var rows=await grants.ReadAsync(actor,database.Fingerprint,scope.BusinessAssociateId);
            return JsonSerializer.Serialize(new{Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal),Grants=rows.OrderBy(row=>row.Id,StringComparer.Ordinal)});
        }
        var initial=await CheckApp();
        async Task CheckCurrent(){if(await CheckApp()!=initial)throw RefusalException.Forbidden("The original facility reader authority changed.");}
        async Task Confirm()
        {
            await CheckCurrent();var now=await fields.GetCurrentFieldSelectionAsync();FieldSelectionReviewValidator.RequireCurrent(now,scope,actor);
            if(!now.SelectionAvailable || now.SelectionContext!=original)throw RefusalException.Forbidden("The original facility FIELD selection changed.");
            var level=editorRole is null?"READ":"WRITE";
            var permitted=await access.CheckAssetAccessAsync(actor,original.FieldId!,"FIELD",level);
            if(permitted is null || !permitted.HasAccess)throw RefusalException.Forbidden("Current FIELD access is required.");
            if(!creation && (editorRole is not null || recovery))
            {
                var facility=await access.CheckAssetAccessAsync(actor,facilityId!,"FACILITY",level,facilityType);
                if(facility is null || !facility.HasAccess)throw RefusalException.Forbidden("Current original facility access is required.");
            }
            await CheckCurrent();
        }
        await Confirm();return new(original,ProcessDefinitionRevision.FromJson(initial),CheckCurrent,Confirm);
    }
}
public sealed record DevelopmentFacilityReadScope(FieldSelectionReview Context,string AuthorityRevision,Func<Task> CheckCurrentAsync,Func<Task> ConfirmAsync);
