using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Repository;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Read-only preflight for an explicit investment handoff; never creates a process, approval, task or grant.</summary>
public sealed partial class CompanyDevelopmentInvestmentRoutingService(CompanyDevelopmentAfeService finance,CompanyModuleConnection company,
    ModuleConnectionResolver modules,IProcessService processes,IRepositoryAccessService accounts,RepositoryPersonaService workspaces,
    IWorkflowRecipientAssetReader assets,IDMEEditor editor)
{
    private static readonly string[] SenderPermissions=["Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Start",CompanyDevelopmentAfeService.PreparePermission];
    private static readonly string[] RecipientPermissions=["Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Approve",CompanyDevelopmentAfeService.ReviewPermission];
    public async Task<DevelopmentInvestmentRoutingReview> ReadAsync(ClaimsPrincipal user,string processId,string afeId)
    {
        var initial=await finance.ReviewInvestmentAsync(user,processId,afeId);initial.Require();var scope=initial.Current.Source.Review.Scope;
        var binding=scope.LifecycleDatabase;var source=editor.GetDataSource(binding.ConnectionName)??throw new InvalidOperationException("The original investment workflow target is unavailable.");
        async Task<string> Authority()
        {
            var access=await company.RefreshCompanyAsync();company.RequireActor(scope.ActorUserId);
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=scope.ActorUserId || access.Scope!=scope.Company || !access.Roles.Any(user.IsInRole) ||
                !SenderPermissions.All(permission=>access.Permissions.Contains(permission,StringComparer.Ordinal)) || await company.ResolveAsync()!=scope.CoreDatabase ||
                await modules.ResolveAsync("LIFECYCLE")!=binding.ConnectionName || await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],binding.ConnectionName)!=binding.Fingerprint ||
                !ReferenceEquals(source,editor.GetDataSource(binding.ConnectionName)))throw new UnauthorizedAccessException("Current company investment preparation, workflow start, standard role and original bindings are required to review routing.");
            MigrationConnectionTarget.Validate(editor,source,binding.ConnectionName);
            return JsonSerializer.Serialize(new{access.Scope,Roles=access.Roles.Order(StringComparer.Ordinal),Permissions=access.Permissions.Order(StringComparer.Ordinal)});
        }
        var authority=await Authority();async Task Guard(){if(await Authority()!=authority)throw new UnauthorizedAccessException("The original investment routing authority changed.");}
        if(processes is not BoundProcessService bound)throw new InvalidOperationException("The canonical bound native workflow reader is required.");
        if(await bound.CreateBoundServiceAsync(binding.ConnectionName) is not PPDMProcessService native)throw new InvalidOperationException("The canonical native investment workflow owner is required.");
        async Task<DevelopmentInvestmentRoutingReview> Capture()
        {
            // The canonical financial/source read brackets both catalog observations. Do not reread
            // the same complete cross-module source graph inside each role/definition observation.
            await Guard();var current=CompanyInventorySourceReview.Copy(initial);
            var recorded=await native.ReadRecordedInvestmentHandoffAsync(scope.Company,scope.FieldId,afeId,Guard);
            recorded?.Original.RequireSource(current,false);
            var options=new List<DevelopmentInvestmentRoutingOption>();
            var roles=new Dictionary<string,DevelopmentReviewRecipientStep>(StringComparer.Ordinal);var stageCount=0;
            if(!current.BlocksRouting() && recorded is null)
            {
                var definitions=CompanyInventorySourceReview.Copy(await native.GetProcessDefinitionsByTypeAsync("ACCOUNTING")??throw new InvalidOperationException("The configured investment workflow catalog is unavailable."));
                if(definitions.Count>128 || definitions.Any(definition=>definition is null || !definition.IsActive || definition.ProcessType!="ACCOUNTING" || !LeaseSummaryValidator.Identifier(definition.ProcessId)) ||
                    definitions.Select(definition=>definition.ProcessId).Distinct(StringComparer.Ordinal).Count()!=definitions.Count)
                    throw new InvalidOperationException("The complete accounting workflow catalog is ambiguous or exceeds its bound.");
                foreach(var definition in definitions.Where(definition=>definition.EntityType=="AFE").OrderBy(definition=>definition.ProcessId,StringComparer.Ordinal))
                {
                    var stages=new List<DevelopmentReviewRecipientStep>();var blockers=new List<string>();
                    var configured=DevelopmentReviewDefinition.RequireInvestment(definition);stageCount+=configured.Count;
                    if(stageCount>256)throw new InvalidOperationException("The complete investment routing catalog exceeds 256 configured stages. Narrow its configured workflows before reviewing assignment readiness.");
                    foreach(var step in configured)
                    {
                        await Guard();var role=step.RequiredRoles[0];
                        if(!roles.TryGetValue(role,out var known)){known=await ReadRecipients(current,step.StepId,role,Guard);roles.Add(role,known);}
                        var stage=known with{StepId=step.StepId};stages.Add(stage);
                        if(stage.Recipients.Count==0)blockers.Add("Stage "+step.StepId+" requires an independent company reviewer with role "+step.RequiredRoles[0]+", investment/workflow approval, workspace and original FIELD write access.");
                    }
                    options.Add(new(definition.ProcessId,definition.ProcessName,definition.Version,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),stages,blockers));
                }
                await Guard();var closingDefinitions=await native.GetProcessDefinitionsByTypeAsync("ACCOUNTING")??throw new InvalidOperationException("The complete closing investment workflow catalog is unavailable.");
                if(closingDefinitions.Any(definition=>definition is null) || JsonSerializer.Serialize(definitions.OrderBy(definition=>definition.ProcessId,StringComparer.Ordinal))!=
                    JsonSerializer.Serialize(closingDefinitions.OrderBy(definition=>definition.ProcessId,StringComparer.Ordinal)))
                    throw new InvalidOperationException("The configured investment workflow catalog changed while its recipients were being reviewed.");
            }
            await Guard();var result=new DevelopmentInvestmentRoutingReview(current,options,"",DateTimeOffset.UtcNow){ExistingHandoffId=recorded?.Original.InstanceId()};result=result with{Revision=result.ContentRevision()};
            result.Require(scope.Company,scope.ActorUserId,processId,afeId);return result;
        }
        var first=await Capture();var closing=await Capture();
        if(first.Revision!=closing.Revision)throw new InvalidOperationException("The configured investment workflow or current independent recipient authority changed. Reload before continuing.");
        var after=await finance.ReviewInvestmentAsync(user,processId,afeId);await Guard();
        if(after.Current.Revision!=initial.Current.Revision || after.Recorded!=initial.Recorded || !after.Blockers.SequenceEqual(initial.Blockers,StringComparer.Ordinal))
            throw new InvalidOperationException("The original investment source changed before the routing review completed.");
        return CompanyInventorySourceReview.Copy(closing);
    }
    private async Task<DevelopmentReviewRecipientStep> ReadRecipients(DevelopmentInvestmentReview current,string step,string role,Func<Task> guard)
    {
        var scope=current.Current.Source.Review.Scope;await guard();
        var candidates=await accounts.GetCompanyRoleRecipientsAsync(scope.Company,role)??throw new InvalidOperationException("Current investment role recipients are unavailable.");
        if(candidates.Length>256 || candidates.Any(candidate=>candidate is null || !LeaseSummaryValidator.Identifier(candidate.UserId) || !LeaseSummaryValidator.Identifier(candidate.PersonaCode)) ||
            candidates.Select(candidate=>candidate.UserId).Distinct(StringComparer.Ordinal).Count()!=candidates.Length)
            throw new InvalidOperationException("The current investment role recipient set is ambiguous or exceeds its bound.");
        candidates=candidates.Where(candidate=>candidate.UserId!=scope.ActorUserId && candidate.UserId!=current.Afe().CreatedBy).OrderBy(candidate=>candidate.UserId,StringComparer.Ordinal).ToArray();
        IReadOnlyList<AppUserAssetAccess> grants=candidates.Length==0?[]:await assets.ReadInvestmentRecipientFieldGrantsAsync(scope,candidates.Select(candidate=>candidate.UserId).ToArray());
        if(grants is null || grants.Any(grant=>grant is null) || grants.Select(grant=>grant.Id).Distinct(StringComparer.Ordinal).Count()!=grants.Count)
            throw new InvalidOperationException("The recipient FIELD authority is ambiguous.");
        var eligible=new List<RepositoryCompanyRoleRecipient>();var snapshots=new List<object>();
        foreach(var candidate in candidates)
        {
            await guard();var access=await accounts.GetCompanyAccessAsync(candidate.UserId,scope.Company);
            if(access?.Scope!=scope.Company || !access.Roles.Contains(role,StringComparer.Ordinal) || !RecipientPermissions.All(permission=>access.Permissions.Contains(permission,StringComparer.Ordinal)))continue;
            var workspace=await workspaces.ReviewWorkspacesAsync(candidate.UserId,scope.Company,accounts);
            var field=grants.Where(grant=>grant.UserId==candidate.UserId && grant.DatabaseScope==scope.CoreDatabase.Fingerprint && grant.OrganizationId==scope.Company.BusinessAssociateId &&
                grant.AssetType=="FIELD" && grant.AssetId==scope.FieldId && grant.FacilityType is null && grant.IsActive && grant.AccessLevel is "WRITE" or "DELETE").OrderBy(grant=>grant.Id,StringComparer.Ordinal).ToArray();
            if(field.Length==0 || !workspace.Personas.Any(persona=>persona.Code==candidate.PersonaCode))continue;
            eligible.Add(candidate);snapshots.Add(new{candidate,Roles=access.Roles.Order(StringComparer.Ordinal),Permissions=access.Permissions.Order(StringComparer.Ordinal),workspace.Revision,Grants=field});
        }
        await guard();return new(step,role,eligible,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{scope.Company,Source=current.Current.Revision,current.AfeId,role,Snapshots=snapshots})));
    }
}
