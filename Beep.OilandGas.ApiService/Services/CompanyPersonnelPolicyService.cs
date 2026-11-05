using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed class CompanyPersonnelPolicyService(CompanyModuleConnection company,ModuleConnectionResolver modules,IContractorManagementService contractors)
{
    private ICompanyPersonnelPolicyOwner Owner=>contractors as ICompanyPersonnelPolicyOwner??throw new InvalidOperationException("Existing native personnel policy owner is unavailable.");
    private async Task<CompanyPersonnelPolicyContext> Authority(ClaimsPrincipal user)
    {
        var access=await company.RefreshCompanyAsync();company.RequireActor(user.ActingUserId());
        if(!user.IsInRole("Administrator") || !access.Roles.Contains("Administrator",StringComparer.Ordinal) || !access.Permissions.Contains("HSE.ConfigurePersonnelPolicy",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current app-owned Administrator role and explicit personnel policy configuration permission are required.");
        var connection=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],connection));
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{access.Scope,access.UserId,Roles=access.Roles.Order(StringComparer.Ordinal).ToArray(),Permissions=access.Permissions.Order(StringComparer.Ordinal).ToArray(),Database=database}));
        var context=new CompanyPersonnelPolicyContext(access.Scope,database,access.UserId,revision);context.Require();return context;
    }
    public async Task<CompanyPersonnelPolicyCatalog> ReviewAsync(ClaimsPrincipal user)
    {
        var context=await Authority(user);async Task Guard(){if(await Authority(user)!=context)throw new UnauthorizedAccessException("Original policy administrator/company/binding authority changed.");}
        var result=await Owner.ReadPersonnelPoliciesAsync(context,Guard);result.Require();await Guard();return result;
    }
    public async Task<WORK_ORDER_PERSONNEL_POLICY> RecordAsync(ClaimsPrincipal user,CompanyPersonnelPolicyCommand original,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=await Authority(user);
        if(command.Context.Company!=context.Company || command.Context.ActorUserId!=context.ActorUserId || command.Context.Database!=context.Database || !readOnly && command.Context!=context)throw new UnauthorizedAccessException("Original policy administrator/company/native binding/source authority differs.");
        async Task Guard(){if(await Authority(user)!=context)throw new UnauthorizedAccessException("Current original policy administrator authority changed.");}
        return await Owner.RecordPersonnelPolicyAsync(command,Guard,readOnly);
    }
    public async Task<CompanyPersonnelPolicyRecovery> RecoverReferenceAsync(ClaimsPrincipal user,CompanyPersonnelPolicyReference reference)
    {
        reference.Require();var context=await Authority(user);async Task Guard(){if(await Authority(user)!=context)throw new UnauthorizedAccessException("Original policy administrator reference authority changed.");}
        var result=await Owner.RecoverPersonnelPolicyReferenceAsync(context,reference,Guard);result.Require(context.Company,context.ActorUserId,reference);await RecordAsync(user,result.Command,true);await Guard();return result;
    }
}
