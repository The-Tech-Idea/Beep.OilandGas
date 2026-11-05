using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProductionOperations.Services;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
namespace Beep.OilandGas.ApiService.Services;

public sealed class CompanyMaintenanceReturnPolicyService(CompanyModuleConnection company,ModuleConnectionResolver modules,RepositoryDbContext roles,IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,IFailureReporter failures)
{
    private sealed record ConfiguredReturnRole(string Id,string? Name,string? ConcurrencyStamp);
    private async Task<ConfiguredReturnRole[]> ConfiguredReturnRolesAsync()
    {
        // Existing app RBAC only. IdentityServer roles and native business rows are not role sources.
        var claims=await (from claim in roles.RoleClaims.AsNoTracking()
            join role in roles.Roles.AsNoTracking() on claim.RoleId equals role.Id
            where claim.ClaimType=="permission" && claim.ClaimValue=="Maintenance.ReturnToService"
            orderby role.Id
            select new{role.Id,role.Name,role.ConcurrencyStamp,claim.ClaimType,claim.ClaimValue}).ToArrayAsync();
        // Database collations must not turn differently cased permission strings into an app capability.
        var configured=claims.Where(row=>row.ClaimType=="permission" && row.ClaimValue=="Maintenance.ReturnToService")
            .Select(row=>new ConfiguredReturnRole(row.Id,row.Name,row.ConcurrencyStamp)).Distinct().ToArray();
        if(configured.Length>1024 || configured.Any(row=>!LeaseSummaryValidator.Identifier(row.Name)) || configured.Select(row=>row.Name).Distinct(StringComparer.Ordinal).Count()!=configured.Length)throw new InvalidOperationException("Complete unambiguous app-owned return-role configuration is unavailable.");
        return configured;
    }
    internal async Task<string[]> ReadEvidenceReturnRolesAsync(FieldSelectionReview context)
    {
        var access=await company.RefreshCompanyAsync();company.RequireActor(context.ActorUserId);
        if(access.Scope!=context.Company || access.UserId!=context.ActorUserId || access.Roles.Count==0 || !access.Permissions.Contains("Maintenance.ReturnToService",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original app company/account/standard role and maintenance return evidence permission are required.");
        var connection=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],connection));if(database!=context.Database)throw new UnauthorizedAccessException("Original return policy evidence CORE binding changed.");
        var configured=await ConfiguredReturnRolesAsync();return access.Roles.Where(role=>configured.Any(row=>row.Name==role)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }
    private async Task<(CompanyMaintenanceReturnPolicyContext Context,string[] ReturnRoles)> Authority(ClaimsPrincipal user)
    {
        var access=await company.RefreshCompanyAsync();company.RequireActor(user.ActingUserId());
        if(!user.IsInRole("Administrator") || !access.Roles.Contains("Administrator",StringComparer.Ordinal) || !access.Permissions.Contains("Maintenance.ConfigureReturnPolicy",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current app-owned Administrator role and explicit maintenance return policy permission are required.");
        var configured=await ConfiguredReturnRolesAsync();
        var connection=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],connection));
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{access.Scope,access.UserId,Roles=access.Roles.Order(StringComparer.Ordinal).ToArray(),Permissions=access.Permissions.Order(StringComparer.Ordinal).ToArray(),Database=database,ReturnRoles=configured}));
        var context=new CompanyMaintenanceReturnPolicyContext(access.Scope,database,access.UserId,revision);context.Require();return(context,configured.Select(row=>row.Name!).Order(StringComparer.Ordinal).ToArray());
    }
    private ICompanyMaintenanceReturnPolicyOwner Owner(CompanyMaintenanceReturnPolicyContext context)=>new FacilityManagementService(editor,columns,defaults,metadata,failures,context.Database.ConnectionName,resolveModuleConnection:module=>modules.ResolveAsync(module));
    public async Task<CompanyMaintenanceReturnPolicyCatalog> ReviewAsync(ClaimsPrincipal user)
    {
        var snapshot=await Authority(user);async Task Guard(){if((await Authority(user)).Context!=snapshot.Context)throw new UnauthorizedAccessException("Original maintenance policy administrator/company/role configuration/binding changed.");}
        var result=await Owner(snapshot.Context).ReadMaintenanceReturnPoliciesAsync(snapshot.Context,Guard);result.Require();await Guard();return result;
    }
    public async Task<WORK_ORDER_MAINTENANCE_RETURN_POLICY> RecordAsync(ClaimsPrincipal user,CompanyMaintenanceReturnPolicyCommand original,bool readOnly=false)
    {
        ArgumentNullException.ThrowIfNull(original);var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=await Authority(user);var context=snapshot.Context;
        if(command.Context.Company!=context.Company || command.Context.Database!=context.Database || command.Context.ActorUserId!=context.ActorUserId || !readOnly && command.Context!=context)throw new UnauthorizedAccessException("Original maintenance policy administrator/company/native binding/source authority differs.");
        if(!readOnly && command.Policy.AuthorizedReturnRoles.Any(role=>!snapshot.ReturnRoles.Contains(role,StringComparer.Ordinal)))throw new ArgumentException("Every configured return role must exist in the app RBAC role store with the explicit Maintenance.ReturnToService permission.");
        async Task Guard(){if((await Authority(user)).Context!=context)throw new UnauthorizedAccessException("Original maintenance policy administrator or return-role configuration changed.");}
        return await Owner(context).RecordMaintenanceReturnPolicyAsync(command,Guard,readOnly);
    }
    public async Task<CompanyMaintenanceReturnPolicyRecovery> RecoverReferenceAsync(ClaimsPrincipal user,CompanyMaintenanceReturnPolicyReference reference)
    {
        reference.Require();var context=(await Authority(user)).Context;async Task Guard(){if((await Authority(user)).Context!=context)throw new UnauthorizedAccessException("Original maintenance policy reference authority changed.");}
        var result=await Owner(context).RecoverMaintenanceReturnPolicyReferenceAsync(context,reference,Guard);result.Require(context.Company,context.ActorUserId,reference);await RecordAsync(user,result.Command,true);await Guard();return result;
    }
}
