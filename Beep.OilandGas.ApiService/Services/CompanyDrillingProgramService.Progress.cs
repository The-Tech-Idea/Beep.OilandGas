using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyDrillingProgramService
{
    public Task<DrillingProgramReviewProgress> ReadProgramProgressAsync(ClaimsPrincipal user,string processId)=>ReadProgramFactsAsync(user,processId,false);
    private sealed record ProgramFactsAccess(Func<Task<DrillingProgramReviewProgress>> Read,Func<Task> Check,Func<Task> ConfirmSource);
    private async Task<DrillingProgramReviewProgress> ReadProgramFactsAsync(ClaimsPrincipal user,string processId,bool executionFacts)
    {
        var access=await CaptureProgramFactsAsync(user,processId,executionFacts);
        await access.Read();var closing=await access.Read();await access.Check();return closing;
    }
    // An enclosing native command may use the same opening/closing reader instead
    // of nesting a second complete pair inside each of its source callbacks.
    private async Task<ProgramFactsAccess> CaptureProgramFactsAsync(ClaimsPrincipal user,string processId,bool executionFacts)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Choose an exact recorded program review.");
        var actor=await account.RequireAsync();company.RequireActor(actor);var selected=(await company.RefreshCompanyAsync()).Scope;var core=await company.ResolveAsync();var connection=await modules.ResolveAsync("LIFECYCLE");var binding=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],connection));var physical=editor.GetDataSource(connection)??throw new InvalidOperationException("The original program lifecycle source is unavailable.");string? field=null;Access? sourceAccess=null;
        async Task<string> Snapshot()
        {
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=actor || await account.RequireAsync()!=actor)throw new UnauthorizedAccessException("The current program outcome account changed.");var current=await company.RefreshCompanyAsync();company.RequireActor(actor);
            if(current.Scope!=selected || !current.Roles.Any(user.IsInRole) || !new[]{"Drilling.View","Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>current.Permissions.Contains(permission,StringComparer.Ordinal)) || await company.ResolveAsync()!=core || await modules.ResolveAsync("LIFECYCLE")!=connection || await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],connection)!=binding.Fingerprint || !ReferenceEquals(physical,editor.GetDataSource(connection)))throw new UnauthorizedAccessException("Current company program view roles and original source bindings are required.");
            MigrationConnectionTarget.Validate(editor,physical,connection);var rights=await grants.ReadAsync(actor,core.Fingerprint,selected.BusinessAssociateId);
            if(field is not null && !rights.Any(row=>row.UserId==actor && row.DatabaseScope==core.Fingerprint && row.OrganizationId==selected.BusinessAssociateId && row.AssetType=="FIELD" && row.AssetId==field && row.FacilityType is null && row.IsActive && row.AccessLevel is "READ" or "WRITE" or "DELETE"))throw new UnauthorizedAccessException("Current original FIELD READ access is required for program outcome history.");
            return JsonSerializer.Serialize(new{current.Scope,current.UserId,Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal),Grants=rights.OrderBy(row=>row.Id,StringComparer.Ordinal)});
        }
        var captured=await Snapshot();async Task Guard(){if(await Snapshot()!=captured)throw new UnauthorizedAccessException("The original program outcome authority changed.");if(sourceAccess is not null)await sourceAccess.Check();}
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(connection) is not PPDMProcessService workflow)throw new InvalidOperationException("The canonical native program outcome owner is required.");
        DrillingProgramReviewStart? original=null;string? nativeRevision=null;string? originalRevision=null;string? baseline=null;
        async Task<DrillingProgramReviewProgress> Read()
        {
            await Guard();var graph=await workflow.ReadRecordedProgramReviewAsync(selected,processId,Guard);var owner=graph.Original.Scope();
            if(owner.Company!=selected || owner.CoreDatabase!=core || owner.LifecycleDatabase!=binding || !executionFacts && actor!=owner.ActorUserId && actor!=graph.Original.Source.Program.ROW_CREATED_BY && !graph.Original.Workflow.Stages.Any(stage=>stage.Authority.Recipients.Any(row=>row.UserId==actor)))
                throw new UnauthorizedAccessException("Only an original program participant with current company/source read access may open this history.");
            if(originalRevision is not null && (nativeRevision!=graph.NativeRevision || originalRevision!=graph.Original.Revision()))
                throw new InvalidOperationException("The complete original program decision/history/task graph changed during outcome reload.");
            original??=graph.Original;originalRevision??=original.Revision();nativeRevision??=graph.NativeRevision;field??=owner.FieldId;await Guard();
            if(sourceAccess is null)
            {
                var funding=original.Source.Basis.Funding.Source;
                sourceAccess=await Capture(user,funding.Current.Source.Review.InstanceId,funding.AfeId,original.Source.Basis.Well.Uwi,false,true);
            }
            var current=await ReadProgramReviewSourceAsync(sourceAccess,await sourceAccess.Read(),original.Source.Program.PROGRAM_ID,original.Source.Program.PROGRAM_DRAFT_REVISION);
            original.RequireSource(current,false,false);await sourceAccess.Confirm();
            var result=PPDMProcessService.ProgramReviewProgress(original,graph.Process,graph.Tasks,current,graph.NativeRevision);
            if(executionFacts)result.RequireFacts(selected,actor,processId);else result.Require(selected,actor,processId);
            if(baseline is not null && baseline!=result.Revision)
                throw new InvalidOperationException("The original program or current funding/well evidence changed during outcome reload.");
            baseline??=result.Revision;await sourceAccess.Confirm();await Guard();return result;
        }
        async Task ConfirmSource()
        {
            await Guard();
            if(sourceAccess is null)throw new InvalidOperationException("Read the complete approved program before confirming its source resources.");
            await sourceAccess.Confirm();await Guard();
        }
        return new(Read,Guard,ConfirmSource);
    }
}
