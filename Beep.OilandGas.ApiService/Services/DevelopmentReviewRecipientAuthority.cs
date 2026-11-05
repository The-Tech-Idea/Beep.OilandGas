using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Source-specific assignment eligibility through the existing company roles, workspace and grant owners.</summary>
public sealed class DevelopmentReviewRecipientAuthority(IRepositoryAccessService accounts,RepositoryPersonaService workspaces,IWorkflowRecipientAssetReader assets)
{
    public Task<DevelopmentReviewRecipientStep> ReadCompletionReviewAsync(DrillingDailyReportPage source,string stepId,string role,DrillingCompletionReviewStart original)
    {
        original.RequireSource(source,false,false);var scope=source.History.Execution.Basis.Scope();
        return ReadEligibleAsync(scope.Company,scope.CoreDatabase,scope.FieldId,original.Scope().ActorUserId,original.Source.ReportedCompletion!.RecorderUserId,source.History.Revision,stepId,role,
            ["Drilling.View","Drilling.ViewDailyReports","Drilling.Approve","Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Approve"],
            users=>assets.ReadCompletionReviewRecipientFieldGrantsAsync(scope,users),false);
    }
    public Task<DevelopmentReviewRecipientStep> ReadCompletionAsync(DrillingDailyReportHistory source,string stepId,string role)
    {
        source.Require();var claim=source.ReportedCompletion??throw new InvalidOperationException("A current reported completion is required before independent routing.");var scope=source.Execution.Basis.Scope();
        return ReadEligibleAsync(scope.Company,scope.CoreDatabase,scope.FieldId,scope.ActorUserId,claim.RecorderUserId,source.Revision,stepId,role,
            ["Drilling.View","Drilling.ViewDailyReports","Drilling.Approve","Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Approve"],
            users=>assets.ReadExecutionRecipientFieldGrantsAsync(scope,users),false);
    }
    public Task<DevelopmentReviewRecipientStep> ReadExecutionReportingAsync(DrillingExecutionStatus source,string stepId,string role)
    {
        source.Require();if(source.Recorded is null)throw new InvalidOperationException("A recorded execution is required before reporting assignment.");var scope=source.Basis.Scope();
        return ReadEligibleAsync(scope.Company,scope.CoreDatabase,scope.FieldId,scope.ActorUserId,source.Recorded.ROW_CREATED_BY,source.Revision,stepId,role,
            ["Drilling.View","Drilling.Edit","Drilling.ManageWellProgram","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"],
            users=>assets.ReadExecutionRecipientFieldGrantsAsync(scope,users),false,independent:false,writeField:true);
    }
    public Task<DevelopmentReviewRecipientStep> ReadProgramAsync(DrillingProgramReviewSource source,string stepId,string role,DrillingProgramReviewStart? original=null)
    {
        source.Require();original?.RequireSource(source,false,false);var scope=source.Basis.Scope();
        return ReadEligibleAsync(scope.Company,scope.CoreDatabase,scope.FieldId,original?.Scope().ActorUserId??scope.ActorUserId,source.Program.ROW_CREATED_BY,source.Revision,stepId,role,
            ["Drilling.View","Drilling.Approve","Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Approve"],
            users=>original is null?assets.ReadProgramRecipientFieldGrantsAsync(scope,users):assets.ReadProgramReviewRecipientFieldGrantsAsync(scope,users),false);
    }
    public Task<DevelopmentReviewRecipientStep> ReadAsync(DevelopmentPlanSourceReview source,string stepId,string role,DevelopmentAssignedReviewScope? reviewer=null)
    {
        source.Require(source.Context,source.PlanId);
        if(source.Plan is null)throw RefusalException.Conflict("A recorded development draft is required before assignment.");
        if(reviewer is not null && (reviewer.Company!=source.Context.Company || reviewer.CoreDatabase!=source.Context.Database || reviewer.PlanningDatabase!=source.PlanningDatabase || reviewer.FieldId!=source.Context.FieldId))
            throw RefusalException.Forbidden("The next recipient lookup belongs to another original development source.");
        return ReadEligibleAsync(source.Context.Company,source.Context.Database,source.Context.FieldId!,source.Context.ActorUserId,source.Plan.ROW_CREATED_BY,source.Revision,stepId,role,
            ["DevelopmentPlanning.View","DevelopmentPlanning.Approve","Workflow.View","Workflow.Approve"],
            users=>reviewer is null?assets.ReadRecipientFieldGrantsAsync(source.Context,users):assets.ReadReviewRecipientFieldGrantsAsync(reviewer,users),true);
    }
    private async Task<DevelopmentReviewRecipientStep> ReadEligibleAsync(CompanyScope company,AssetDatabaseScope database,string field,string actor,string? maker,string sourceRevision,string stepId,string role,
        string[] permissions,Func<string[],Task<IReadOnlyList<AppUserAssetAccess>>> loadGrants,bool requireAny,bool independent=true,bool writeField=false)
    {
        if(!LeaseSummaryValidator.Identifier(stepId) || !LeaseSummaryValidator.Identifier(role))throw new InvalidOperationException("An exact configured review stage and role are required.");
        var candidates=await accounts.GetCompanyRoleRecipientsAsync(company,role)??throw new InvalidOperationException("Current company recipients are unavailable.");
        if(candidates.Length>256 || candidates.Any(row=>row is null || !LeaseSummaryValidator.Identifier(row.UserId) || !LeaseSummaryValidator.Identifier(row.PersonaCode)) || candidates.Select(row=>row.UserId).Distinct(StringComparer.Ordinal).Count()!=candidates.Length)
            throw new InvalidOperationException("The current company recipient set is ambiguous or exceeds its bound.");
        if(independent)candidates=candidates.Where(row=>row.UserId!=maker && row.UserId!=actor).ToArray();
        if(requireAny && candidates.Length==0)throw RefusalException.Conflict("The configured review role has no independent company recipient.");
        var grants=candidates.Length==0?Array.Empty<AppUserAssetAccess>():await loadGrants(candidates.Select(row=>row.UserId).ToArray());
        if(grants is null || grants.Any(grant=>grant is null) || grants.Select(grant=>grant.Id).Distinct(StringComparer.Ordinal).Count()!=grants.Count)throw new InvalidOperationException("The current review FIELD grants are unavailable or ambiguous.");
        var recipients=new List<RepositoryCompanyRoleRecipient>();var snapshots=new List<object>();
        foreach(var candidate in candidates.OrderBy(row=>row.UserId,StringComparer.Ordinal))
        {
            var access=await accounts.GetCompanyAccessAsync(candidate.UserId,company);
            if(access?.Scope!=company || !access.Roles.Contains(role,StringComparer.Ordinal) || !permissions.All(permission=>access.Permissions.Contains(permission,StringComparer.Ordinal)))continue;
            var workspace=await workspaces.ReviewWorkspacesAsync(candidate.UserId,company,accounts);
            var fieldGrants=grants.Where(grant=>grant.UserId==candidate.UserId && grant.DatabaseScope==database.Fingerprint && grant.OrganizationId==company.BusinessAssociateId && grant.AssetType=="FIELD" && grant.AssetId==field &&
                grant.FacilityType is null && grant.IsActive && (writeField?grant.AccessLevel is "WRITE" or "DELETE":grant.AccessLevel is "READ" or "WRITE" or "DELETE")).ToArray();
            if(!workspace.Personas.Any(persona=>persona.Code==candidate.PersonaCode) || fieldGrants.Length==0)continue;
            recipients.Add(candidate);snapshots.Add(new{Recipient=candidate,Roles=access.Roles.Order(StringComparer.Ordinal),Permissions=access.Permissions.Order(StringComparer.Ordinal),workspace.Revision,Grants=fieldGrants.OrderBy(grant=>grant.Id,StringComparer.Ordinal)});
        }
        if(requireAny && recipients.Count==0)throw RefusalException.Conflict("The configured review role needs an independent company reviewer with current approval, workspace and FIELD access.");
        return new(stepId,role,recipients,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{company,Source=sourceRevision,Role=role,Snapshots=snapshots})));
    }
}
