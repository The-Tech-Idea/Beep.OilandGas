using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Repository.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Microsoft.EntityFrameworkCore;
namespace Beep.OilandGas.ApiService.Services;
public interface IDrillingProgramAssignedReviewReader
{
    Task<DrillingProgramAssignedReview> ReadAssignedProgramAsync(ClaimsPrincipal user,string processId);
    Task<DrillingExecutionAssignedWork> ReadAssignedExecutionAsync(ClaimsPrincipal user,string processId);
    Task<DrillingCompletionAssignedReview> ReadAssignedCompletionAsync(ClaimsPrincipal user,string processId);
}
public sealed partial class CompanyDrillingProgramService
{
    private sealed record ProgramReviewer(CompanyScope Company,string Actor,AssetDatabaseScope Core,AssetDatabaseScope Lifecycle,string Persona,string[] Roles,IReadOnlyList<AppUserAssetAccess> Grants,PPDMProcessService Native,Func<Task> Check);
    private async Task<ProgramReviewer> CaptureProgramReviewerAsync(ClaimsPrincipal user,bool reportingAssignment=false)
    {
        var actor=await account.RequireAsync();company.RequireActor(actor);var selected=company.Company.Scope;var core=await company.ResolveAsync();var connection=await modules.ResolveAsync("LIFECYCLE");var lifecycle=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],connection));var source=editor.GetDataSource(connection)??throw new InvalidOperationException("The original program review source is unavailable.");string? persona=null;string[] roles=[];IReadOnlyList<AppUserAssetAccess> fieldGrants=[];
        async Task<string> Snapshot()
        {
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=actor || await account.RequireAsync()!=actor)throw new UnauthorizedAccessException("The assigned program account changed.");var current=await company.RefreshCompanyAsync();company.RequireActor(actor);
            var required=reportingAssignment?new[]{"Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"}:new[]{"Drilling.View","Drilling.Approve","Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Approve"};
            if(current.Scope!=selected || !current.Roles.Any(user.IsInRole) || !required.All(permission=>current.Permissions.Contains(permission,StringComparer.Ordinal)) || await company.ResolveAsync()!=core || await modules.ResolveAsync("LIFECYCLE")!=connection || await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],connection)!=lifecycle.Fingerprint || !ReferenceEquals(source,editor.GetDataSource(connection)))throw new UnauthorizedAccessException("Current company drilling/workflow permissions, standard roles and original bindings are required.");
            MigrationConnectionTarget.Validate(editor,source,connection);var profile=await repository.Set<AppUserPersona>().AsNoTracking().SingleOrDefaultAsync(row=>row.UserId==actor && row.TenantId==selected.TenantId && row.BusinessAssociateId==selected.BusinessAssociateId);var workspace=await workspaces.ReviewWorkspacesAsync(actor,selected,accounts);if(profile is null || !workspace.Personas.Any(row=>row.Code==profile.PersonaCode))throw new UnauthorizedAccessException("The current assigned company workspace is unavailable.");
            var rows=await grants.ReadAsync(actor,core.Fingerprint,selected.BusinessAssociateId);persona=profile.PersonaCode;roles=current.Roles.ToArray();fieldGrants=rows;return JsonSerializer.Serialize(new{current.Scope,Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal),Profile=profile,workspace.Revision,Grants=rows.OrderBy(row=>row.Id,StringComparer.Ordinal)});
        }
        var captured=await Snapshot();var initialPersona=persona!;var initialRoles=roles;var initialGrants=fieldGrants;async Task Check(){if(await Snapshot()!=captured)throw new UnauthorizedAccessException("The original independent program review authority changed.");}
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(connection) is not PPDMProcessService nativeReview)throw new InvalidOperationException("The canonical bound native program review owner is required.");return new(selected,actor,core,lifecycle,initialPersona,initialRoles,initialGrants,nativeReview,Check);
    }
    private static void RequireProgramRecipient(ClaimsPrincipal user,ProgramReviewer access,DrillingProgramReviewStart original,string stepInstanceId)
    {
        var scope=original.Scope();var first=original.Workflow.Stages.Single(stage=>DrillingProgramReviewStartReceipt.StepId(original.InstanceId(),stage.Definition.StepId)==stepInstanceId);if(scope.Company!=access.Company || scope.CoreDatabase!=access.Core || scope.LifecycleDatabase!=access.Lifecycle)throw new UnauthorizedAccessException("The assigned program belongs to another original company or binding.");
        if(access.Actor==scope.ActorUserId || access.Actor==original.Source.Program.ROW_CREATED_BY || !access.Roles.Contains(first.Definition.AssignedRole,StringComparer.Ordinal) || !user.IsInRole(first.Definition.AssignedRole) || !first.Authority.Recipients.Any(row=>row.UserId==access.Actor && row.PersonaCode==access.Persona))throw new UnauthorizedAccessException("Only the original independent recipient with the current configured role and workspace may open this program review.");
        if(!access.Grants.Any(row=>row.UserId==access.Actor && row.DatabaseScope==access.Core.Fingerprint && row.OrganizationId==access.Company.BusinessAssociateId && row.AssetType=="FIELD" && row.AssetId==scope.FieldId && row.FacilityType is null && row.IsActive && row.AccessLevel is "READ" or "WRITE" or "DELETE"))throw new UnauthorizedAccessException("Current original FIELD READ authority is required for the assigned program.");
    }
    public async Task<DrillingProgramAssignedReview> ReadAssignedProgramAsync(ClaimsPrincipal user,string processId)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Choose an exact assigned program review from the company inbox.");var reviewer=await CaptureProgramReviewerAsync(user);var first=await reviewer.Native.ReadRecordedProgramReviewAsync(reviewer.Company,processId,reviewer.Check);RequireProgramRecipient(user,reviewer,first.Original,DrillingProgramReviewStartReceipt.StepId(processId,first.Process.CurrentStepId));await reviewer.Check();var original=first.Original;var fundingSource=original.Source.Basis.Funding.Source;
        var sourceAccess=await Capture(user,fundingSource.Current.Source.Review.InstanceId,fundingSource.AfeId,original.Source.Basis.Well.Uwi,false,true);var current=await ReadProgramReviewSourceAsync(sourceAccess,await sourceAccess.Read(),original.Source.Program.PROGRAM_ID,original.Source.Program.PROGRAM_DRAFT_REVISION);original.RequireSource(current,false,false);await reviewer.Check();
        var closing=await reviewer.Native.ReadRecordedProgramReviewAsync(reviewer.Company,processId,reviewer.Check);RequireProgramRecipient(user,reviewer,closing.Original,DrillingProgramReviewStartReceipt.StepId(processId,closing.Process.CurrentStepId));if(first.NativeRevision!=closing.NativeRevision || original.Revision()!=closing.Original.Revision())throw new InvalidOperationException("The exact original program process, task or source changed during assigned review.");var stage=original.Workflow.Stages.Single(stage=>stage.Definition.StepId==closing.Process.CurrentStepId);var matches=closing.Tasks.Where(task=>task.TARGET_PERSONA_CODE==reviewer.Persona && task.ASSIGNED_ROLE==stage.Definition.AssignedRole && task.PROCESS_STEP_INSTANCE_ID==DrillingProgramReviewStartReceipt.StepId(processId,stage.Definition.StepId) && task.TASK_STATUS=="PENDING").ToArray();if(matches.Length!=1)throw new InvalidOperationException("The original individual program task is missing or ambiguous.");var task=matches[0];
        var result=new DrillingProgramAssignedReview(original,current,task.PROCESS_STEP_INSTANCE_ID,task.ASSIGNED_ROLE,reviewer.Persona,task.CROSS_TASK_ID,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(closing.Process)),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task)),closing.NativeRevision,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require(reviewer.Company,reviewer.Actor,processId);await sourceAccess.Confirm();await sourceAccess.Check();await reviewer.Check();return CompanyInventorySourceReview.Copy(result);
    }
}
