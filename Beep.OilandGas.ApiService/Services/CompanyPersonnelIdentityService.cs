using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed class CompanyPersonnelIdentityService(CompanyModuleConnection company,ModuleConnectionResolver modules,IContractorManagementService contractors,RepositoryDbContext accounts)
{
    private ICompanyPersonnelIdentityOwner Owner=>contractors as ICompanyPersonnelIdentityOwner??throw new InvalidOperationException("Existing native personnel identity owner is unavailable.");
    private async Task<CompanyPersonnelPolicyContext> Authority(ClaimsPrincipal user)
    {
        var access=await company.RefreshCompanyAsync();company.RequireActor(user.ActingUserId());if(!user.IsInRole("Administrator") || !access.Roles.Contains("Administrator",StringComparer.Ordinal) || !access.Permissions.Contains("HSE.ConfigurePersonnelIdentity",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current app Administrator and explicit personnel identity configuration permission are required.");
        var connection=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],connection));var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{access.Scope,access.UserId,Roles=access.Roles.Order(StringComparer.Ordinal).ToArray(),Permissions=access.Permissions.Order(StringComparer.Ordinal).ToArray(),Database=database}));var result=new CompanyPersonnelPolicyContext(access.Scope,database,access.UserId,revision);result.Require();return result;
    }
    private async Task<CompanyApplicationPersonFacts> Account(CompanyPersonnelPolicyContext context,string userId)
    {
        // Read-only existing app RBAC/account data; native person/link data stays in Beep.
        var membership=await company.ResolveSubjectAsync(userId)??throw new UnauthorizedAccessException("Target account has no current membership in the selected company.");
        if(membership.Scope!=context.Company || membership.UserId!=userId)throw new UnauthorizedAccessException("Target account/company membership differs.");var user=await accounts.Users.AsNoTracking().SingleOrDefaultAsync(value=>value.Id==userId)??throw new UnauthorizedAccessException("Target application account is unavailable.");if(!user.IsActive || string.IsNullOrWhiteSpace(user.ConcurrencyStamp))throw new UnauthorizedAccessException("Current active account/version is required.");
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{user.Id,user.UserName,user.IsActive,user.ConcurrencyStamp,membership.Scope,Roles=membership.Roles.Order(StringComparer.Ordinal).ToArray(),Permissions=membership.Permissions.Order(StringComparer.Ordinal).ToArray()}));var result=new CompanyApplicationPersonFacts(context.Company,user.Id,user.UserName,user.IsActive,revision);result.Require(context.Company,userId);return result;
    }
    public async Task<CompanyPersonnelIdentityCatalog> CatalogAsync(ClaimsPrincipal user)
    {var context=await Authority(user);async Task Guard(){if(await Authority(user)!=context)throw new UnauthorizedAccessException("Original identity administrator/company/binding changed.");}var result=await Owner.ReadPersonnelIdentitiesAsync(context,Guard);await Guard();result.Require();return result;}
    public async Task<CompanyPersonnelIdentityReview> ReviewAsync(ClaimsPrincipal user,CompanyPersonnelIdentityQuery query)
    {
        query.Require();var context=await Authority(user);async Task Guard(){if(await Authority(user)!=context)throw new UnauthorizedAccessException("Original identity administrator/company/binding changed.");}
        await Guard();var first=await Account(context,query.UserId);var catalog=await Owner.ReadPersonnelIdentitiesAsync(context,Guard);var person=await Owner.ReadIdentityPersonAsync(context,query.PersonBaId,Guard);var closing=await Account(context,query.UserId);var closingCatalog=await Owner.ReadPersonnelIdentitiesAsync(context,Guard);await Guard();
        if(first!=closing || catalog.Revision!=closingCatalog.Revision)throw new InvalidOperationException("Complete original account/membership/identity-link population changed during review.");return CompanyPersonnelIdentityReview.Create(context,query,closing,person,closingCatalog,DateTime.UtcNow);
    }
    public async Task<COMPANY_PERSONNEL_IDENTITY> RecordAsync(ClaimsPrincipal user,CompanyPersonnelIdentityCommand original,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=await Authority(user);if(command.Review.Context.Company!=context.Company || command.Review.Context.ActorUserId!=context.ActorUserId || command.Review.Context.Database!=context.Database || !readOnly && command.Review.Context!=context)throw new UnauthorizedAccessException("Original identity administrator/company/native binding differs.");
        async Task Guard(){if(await Authority(user)!=context)throw new UnauthorizedAccessException("Original identity administrator authority changed.");}async Task ApplicationProof(){await Guard();if(await Account(context,command.Review.Query.UserId)!=command.Review.Account)throw new InvalidOperationException("Original account/member proof changed.");await Guard();}
        return await Owner.RecordPersonnelIdentityAsync(command,Guard,ApplicationProof,readOnly);
    }
    public async Task<CompanyPersonnelIdentityRecovery> RecoverReferenceAsync(ClaimsPrincipal user,CompanyPersonnelIdentityReference reference)
    {reference.Require();var context=await Authority(user);async Task Guard(){if(await Authority(user)!=context)throw new UnauthorizedAccessException("Original identity recovery authority changed.");}var result=await Owner.RecoverPersonnelIdentityReferenceAsync(context,reference,Guard);result.Require(context.Company,context.ActorUserId,reference);await Guard();return result;}
}
