using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyDrillingProgramService
{
    private sealed record ExecutionRecorder(string[] Roles,string? Persona,string? Revision,Func<Task> Check);
    private async Task<ExecutionRecorder> CaptureExecutionRecorderAsync(ClaimsPrincipal user,DevelopmentAssignedReviewScope scope,bool requireWrite=false,string? requiredRole=null,bool reporting=false)
    {
        string[] roles=[];string? persona=null;string? revision=null;
        async Task<string> Snapshot()
        {
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=scope.ActorUserId || await account.RequireAsync()!=scope.ActorUserId)throw new UnauthorizedAccessException("The current execution recorder account changed.");var current=await company.RefreshCompanyAsync();company.RequireActor(scope.ActorUserId);if(current.Scope!=scope.Company || !current.Roles.Any(user.IsInRole))throw new UnauthorizedAccessException("The original execution company and approved standard roles are required.");
            var permitted=current.Permissions.Contains(reporting?"Drilling.Edit":"Drilling.Create",StringComparer.Ordinal) && current.Permissions.Contains("Drilling.ManageWellProgram",StringComparer.Ordinal) && (!reporting || current.Permissions.Contains("Drilling.ViewDailyReports",StringComparer.Ordinal));var profile=await repository.Set<AppUserPersona>().AsNoTracking().SingleOrDefaultAsync(row=>row.UserId==scope.ActorUserId && row.TenantId==scope.Company.TenantId && row.BusinessAssociateId==scope.Company.BusinessAssociateId);var workspace=permitted?await workspaces.ReviewWorkspacesAsync(scope.ActorUserId,scope.Company,accounts):null;
            var mapped=permitted && profile is not null && workspace!.Personas.Any(row=>row.Code==profile.PersonaCode);roles=mapped?current.Roles.Where(user.IsInRole).Order(StringComparer.Ordinal).ToArray():[];persona=mapped?profile!.PersonaCode:null;
            if(requireWrite && (!mapped || requiredRole is null || !roles.Contains(requiredRole,StringComparer.Ordinal)))throw new UnauthorizedAccessException(reporting?"Current report edit/manage authority, configured role and mapped workspace are required.":"Current execution create/manage authority, configured role and mapped workspace are required.");
            var snapshot=JsonSerializer.Serialize(new{current.Scope,current.UserId,Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal),Profile=profile,Workspace=workspace?.Revision});revision=mapped?ProcessDefinitionRevision.FromJson(snapshot):null;return snapshot;
        }
        var captured=await Snapshot();var resultRoles=roles;var resultPersona=persona;var resultRevision=revision;async Task Check(){if(await Snapshot()!=captured)throw new UnauthorizedAccessException("The original configured execution recorder authority changed.");}return new(resultRoles,resultPersona,resultRevision,Check);
    }
    private async Task<Func<Task>> CaptureExecutionBindingsAsync(DrillingExecutionBasis basis)
    {
        var scope=basis.Scope();var bindings=new[]{(Module:"DEVELOPMENT_PLANNING",Binding:scope.PlanningDatabase),(Module:"LIFECYCLE",Binding:scope.LifecycleDatabase)};
        var sources=bindings.Select(item=>editor.GetDataSource(item.Binding.ConnectionName)??throw new InvalidOperationException("The original execution planning/lifecycle source is unavailable.")).ToArray();
        async Task Check()
        {
            for(int index=0;index<bindings.Length;index++)
            {
                var item=bindings[index];if(await modules.ResolveAsync(item.Module)!=item.Binding.ConnectionName || await modules.GetMigrationBindingFingerprintAsync([item.Module],item.Binding.ConnectionName)!=item.Binding.Fingerprint || !ReferenceEquals(sources[index],editor.GetDataSource(item.Binding.ConnectionName)))throw new UnauthorizedAccessException("The original execution planning/lifecycle binding changed.");MigrationConnectionTarget.Validate(editor,sources[index],item.Binding.ConnectionName);
            }
        }
        await Check();return Check;
    }
    public Task<DrillingExecutionBasis> ReadExecutionBasisAsync(ClaimsPrincipal user,string reviewId)=>ReadExecutionBasisCoreAsync(user,reviewId,true);
    private async Task<DrillingExecutionBasis> ReadExecutionBasisCoreAsync(ClaimsPrincipal user,string reviewId,bool activeIfRecorder)
    {
        return await CreateExecutionBasisAsync(user,await ReadProgramFactsAsync(user,reviewId,true),activeIfRecorder);
    }
    private async Task<DrillingExecutionBasis> CreateExecutionBasisAsync(ClaimsPrincipal user,DrillingProgramReviewProgress facts,bool activeIfRecorder)
    {
        if(facts.State!="APPROVED" || facts.PendingTasks!=0)throw new InvalidOperationException("A complete independently approved program is required before recording execution.");var scope=facts.Source.Basis.Scope();var recorder=await CaptureExecutionRecorderAsync(user,scope);
        if(activeIfRecorder && recorder.Roles.Length>0){var current=await native.ReadProgramWellAsync(scope.CoreDatabase,scope.FieldId,facts.Source.Basis.Well.Uwi,true,recorder.Check);if(current!=facts.Source.Basis.Well)throw new InvalidOperationException("The exact current approved execution well changed.");}
        var result=new DrillingExecutionBasis(facts.Source,facts.InstanceId,facts.Original.Revision(),facts.NativeRevision,facts.Original.Scope().ActorUserId,facts.Stages,recorder.Roles,recorder.Persona,recorder.Revision,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require();var bindings=await CaptureExecutionBindingsAsync(result);await recorder.Check();await bindings();return result;
    }
    private async Task<(DrillingExecutionBasis Basis,Func<Task> Check,Func<Task> Confirm)> CaptureExecutionReadAsync(ClaimsPrincipal user,string reviewId)
    {
        // One complete fresh program/source read per phase, with the same paired
        // facts owner spanning the enclosing native read. Current authority is
        // still checked independently at every guard; no cross-request cache.
        var facts=await CaptureProgramFactsAsync(user,reviewId,true);
        var basis=await CreateExecutionBasisAsync(user,await facts.Read(),false);
        var bindings=await CaptureExecutionBindingsAsync(basis);
        // The facts guard already refreshes both the lifecycle authority and its
        // execution-source authority. Reuse that owner rather than capturing the
        // same execution authority again for every native row/range check.
        async Task Guard(){await facts.Check();await bindings();}
        async Task Confirm()
        {
            await Guard();var closing=await CreateExecutionBasisAsync(user,await facts.Read(),false);
            basis.RequireSource(closing,true);await facts.ConfirmSource();await Guard();
        }
        await Guard();
        return(basis,Guard,Confirm);
    }
    public async Task<DrillingExecutionStatus> ReadExecutionAsync(ClaimsPrincipal user,string reviewId)
    {
        var access=await CaptureExecutionReadAsync(user,reviewId);var result=await native.ReadExecutionStatusAsync(access.Basis,access.Check);result.Require();await access.Confirm();return result;
    }
    public async Task<DrillingExecutionStartReceipt> StartExecutionAsync(ClaimsPrincipal user,DrillingExecutionStart input,bool recover=false)
    {
        input.Require();var original=CompanyInventorySourceReview.Copy(input);var basis=original.Basis;var scope=basis.Scope();if(user.ActingUserId()!=scope.ActorUserId || await account.RequireAsync()!=scope.ActorUserId)throw new UnauthorizedAccessException("Only the original execution recorder may record or recover this intent.");var source=basis.Source.Basis;var funding=source.Funding.Source;var access=await Capture(user,funding.Current.Source.Review.InstanceId,funding.AfeId,source.Well.Uwi,!recover,recover);var observed=await access.Read();if(observed.Scope()!=scope || observed.ExecutionDatabase!=source.ExecutionDatabase)throw new UnauthorizedAccessException("The execution belongs to another original company/account/well/binding.");var recorder=recover?null:await CaptureExecutionRecorderAsync(user,scope,true,original.RecorderRole);var bindings=await CaptureExecutionBindingsAsync(basis);
        var executionAccess=await CaptureExecutionReadAsync(user,basis.ReviewInstanceId);int sourceReads=0;
        async Task Guard(){await access.Check();await executionAccess.Check();await bindings();if(recorder is not null){await recorder.Check();if(recorder.Revision!=basis.RecorderAuthorityRevision || recorder.Persona!=basis.PersonaCode || !recorder.Roles.SequenceEqual(basis.RecorderRoles))throw new UnauthorizedAccessException("The retained execution recorder role/profile/authority changed.");}}
        async Task<DrillingExecutionBasis> Read()
        {
            await Guard();++sourceReads;
            if(!recover)
            {
                var well=await native.ReadProgramWellAsync(scope.CoreDatabase,scope.FieldId,source.Well.Uwi,true,Guard);
                if(well!=source.Well)throw new InvalidOperationException("The original active execution well changed.");
            }
            basis.RequireSource(executionAccess.Basis,!recover);
            if(sourceReads>1)await executionAccess.Confirm();
            await access.Confirm();await Guard();return executionAccess.Basis;
        }
        var result=await native.RecordExecutionStartAsync(original,Guard,Read,recover);result.Require(original);if(!recover && result.Recorded is null)throw new InvalidOperationException("The original execution start is unconfirmed. Recover the retained original.");await access.Confirm();await Guard();return result;
    }
}
