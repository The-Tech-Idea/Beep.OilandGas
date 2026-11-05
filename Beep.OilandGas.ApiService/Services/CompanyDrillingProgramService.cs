using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.DrillingAndConstruction.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Current company/source admission to the existing native drilling-program owner.</summary>
public sealed partial class CompanyDrillingProgramService(RepositoryRequestAccount account,CompanyModuleConnection company,ModuleConnectionResolver modules,
    IUserAssetAccessStore grants,IAccessControlService resources,CompanyDevelopmentInvestmentRoutingService funding,DrillingOperationService native,IDMEEditor editor,
    IProcessService processes,DevelopmentReviewRecipientAuthority recipients,Beep.OilandGas.Repository.RepositoryDbContext repository,Beep.OilandGas.Repository.IRepositoryAccessService accounts,Beep.OilandGas.Repository.RepositoryPersonaService workspaces)
    : IDrillingProgramAssignedReviewReader
{
    private sealed record Access(Func<Task> Check,Func<Task> Confirm,Func<Task<DrillingProgramBasis>> Read);
    private async Task<Access> Capture(ClaimsPrincipal user,string process,string afe,string well,bool write,bool recover,DrillingProgramBasis? original=null,string writePermission="Drilling.Create")
    {
        if(!LeaseSummaryValidator.Identifier(process) || !LeaseSummaryValidator.Identifier(afe) || string.IsNullOrWhiteSpace(well) || well.Length>128 || well.Any(char.IsControl))throw new InvalidOperationException("Choose an exact approved investment source and recorded well.");
        var actor=await account.RequireAsync();company.RequireActor(actor);var selected=(await company.RefreshCompanyAsync()).Scope;var core=await company.ResolveAsync();var connection=await modules.ResolveAsync("DRILLING_EXECUTION");
        var execution=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["DRILLING_EXECUTION"],connection));var physical=editor.GetDataSource(connection)??throw new InvalidOperationException("The selected drilling execution source is unavailable.");
        if(original is not null && (original.Scope().Company!=selected || original.Scope().ActorUserId!=actor || original.Scope().CoreDatabase!=core || original.ExecutionDatabase!=execution || original.Well.Uwi!=well))
            throw new UnauthorizedAccessException("The retained original program belongs to another account, company, well or binding.");
        string? field=original?.Scope().FieldId;
        async Task<string> Snapshot()
        {
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=actor || await account.RequireAsync()!=actor)throw new UnauthorizedAccessException("The current drilling account changed.");
            var current=await company.RefreshCompanyAsync();company.RequireActor(actor);
            if(current.Scope!=selected || !current.Roles.Any(user.IsInRole) || !new[]{"Drilling.View","Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>current.Permissions.Contains(permission,StringComparer.Ordinal)) || write && !current.Permissions.Contains(writePermission,StringComparer.Ordinal) ||
                await company.ResolveAsync()!=core || await modules.ResolveAsync("DRILLING_EXECUTION")!=connection || await modules.GetMigrationBindingFingerprintAsync(["DRILLING_EXECUTION"],connection)!=execution.Fingerprint || !ReferenceEquals(physical,editor.GetDataSource(connection)))
                throw new UnauthorizedAccessException("Current approved company roles, drilling permission and original bindings are required.");
            MigrationConnectionTarget.Validate(editor,physical,connection);var rights=await grants.ReadAsync(actor,core.Fingerprint,selected.BusinessAssociateId);
            if(field is not null && !rights.Any(row=>row.UserId==actor && row.DatabaseScope==core.Fingerprint && row.OrganizationId==selected.BusinessAssociateId && row.AssetType=="FIELD" && row.AssetId==field && row.FacilityType is null && row.IsActive &&
                (write?row.AccessLevel is "WRITE" or "DELETE":row.AccessLevel is "READ" or "WRITE" or "DELETE")))throw new UnauthorizedAccessException("Current exact original FIELD authority is required for this drilling program.");
            return JsonSerializer.Serialize(new{current.Scope,Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal),Grants=rights.OrderBy(row=>row.Id,StringComparer.Ordinal)});
        }
        var captured=await Snapshot();async Task Check(){if(await Snapshot()!=captured)throw new UnauthorizedAccessException("The original drilling account/roles/grants changed. Review or recover the retained program.");}
        async Task Confirm()
        {
            await Check();if(field is null || !(await resources.CheckAssetAccessAsync(actor,field,"FIELD",write?"WRITE":"READ")).HasAccess || !(await resources.CheckAssetAccessAsync(actor,well,"WELL","READ")).HasAccess)
                throw new UnauthorizedAccessException("Current original FIELD and well resource access are required.");await Check();
        }
        async Task<DrillingProgramBasis> Read()
        {
            await Check();var source=await funding.ReadHandoffStatusAsync(user,process,afe);var scope=source.Source.Current.Source.Review.Scope;
            if(scope.Company!=selected || scope.ActorUserId!=actor || scope.CoreDatabase!=core || field is not null && scope.FieldId!=field)throw new UnauthorizedAccessException("The approved investment belongs to another company, original FIELD or core source.");
            field=scope.FieldId;await Confirm();var recorded=await native.ReadProgramWellAsync(core,field,well,!recover,Check);
            var result=new DrillingProgramBasis(source,recorded,execution,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require();await Confirm();return result;
        }
        await Check();return new(Check,Confirm,Read);
    }
    public async Task<DrillingProgramBasis> ReviewAsync(ClaimsPrincipal user,string process,string afe,string well)
    {var access=await Capture(user,process,afe,well,false,false);var result=await access.Read();await access.Confirm();return CompanyInventorySourceReview.Copy(result);}
    public async Task<DrillingProgramDraftList> ListAsync(ClaimsPrincipal user,string process,string afe,string well)
    {
        var access=await Capture(user,process,afe,well,false,true);var basis=await access.Read();var result=await native.ReadProgramDraftsAsync(basis,access.Check);result.Require();var closing=await access.Read();if(closing.Revision!=basis.Revision)throw new InvalidOperationException("The approved source or recorded well changed during program reload.");await access.Confirm();await access.Check();return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<DrillingProgramDraftReceipt> RecordAsync(ClaimsPrincipal user,CreateDrillingProgramDraft input,bool recover=false)
    {
        input.Require();var original=CompanyInventorySourceReview.Copy(input);var access=await Capture(user,original.Basis.Funding.Source.Current.Source.Review.InstanceId,original.Basis.Funding.Source.AfeId,original.Basis.Well.Uwi,!recover,recover,original.Basis);
        var result=await native.RecordProgramDraftAsync(original,access.Check,access.Read,recover);result.Require(original);await access.Confirm();await access.Check();return CompanyInventorySourceReview.Copy(result);
    }
}
