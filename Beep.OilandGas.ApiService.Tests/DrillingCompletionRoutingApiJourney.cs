#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Tests;

public sealed partial class DrillingExecutionStartApiTests
{
    private static async Task ConfigureCompletionRouting(DrillingProgramApiTests.Fixture f, DevelopmentAssignedReviewScope scope)
    {
        var db = f.Funding.Routing.Afe.Db;
        db.RoleClaims.Add(new() { RoleId = "execution-role", ClaimType = "permission", ClaimValue = "Workflow.Start" });
        db.Users.Add(new() { Id = "completion-inspector", IsActive = true });
        db.Add(new AppCompanyMembership { TenantId = scope.Company.TenantId, BusinessAssociateId = scope.Company.BusinessAssociateId,
            UserId = "completion-inspector", GrantedByUserId = "administrator", EffectiveFromUtc = DateTime.UtcNow.AddDays(-1) });
        db.Roles.Add(new() { Id = "completion-role", Name = "ConfiguredCompletionReviewer", NormalizedName = "CONFIGUREDCOMPLETIONREVIEWER" });
        db.UserRoles.Add(new() { UserId = "completion-inspector", RoleId = "completion-role" });
        db.Add(new AppUserRoleExtension { TenantId = scope.Company.TenantId, BusinessAssociateId = scope.Company.BusinessAssociateId,
            UserId = "completion-inspector", RoleId = "completion-role", ApprovalStatus = "Approved", EffectiveFromUtc = DateTime.UtcNow.AddDays(-1) });
        db.Add(new AppPersona { Code = "COMPLETION_INSPECTOR", Name = "Independent completion inspector" });
        db.Add(new AppUserPersona { UserId = "completion-inspector",TenantId="tenant",BusinessAssociateId="company", PersonaCode = "COMPLETION_INSPECTOR" });
        foreach (var permission in new[] { "Drilling.View", "Drilling.ViewDailyReports", "Drilling.Approve", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Workflow.Approve" })
            db.RoleClaims.Add(new() { RoleId = "completion-role", ClaimType = "permission", ClaimValue = permission });
        db.RoleClaims.Add(new() { RoleId = "completion-role", ClaimType = RepositoryPersonaService.WorkspaceClaimType, ClaimValue = "COMPLETION_INSPECTOR" });
        db.Add(new AppUserAssetAccess { Id = "completion-inspection-field", UserId = "completion-inspector", DatabaseScope = scope.CoreDatabase.Fingerprint,
            OrganizationId = scope.Company.BusinessAssociateId, AssetType = "FIELD", AssetId = scope.FieldId, AccessLevel = "READ", IsActive = true });
        await db.SaveChangesAsync(); await f.Select("site-recorder", "ConfiguredExecutionRole");
        var definition = CompanyInventorySourceReview.Copy(f.Funding.Routing.Definition);
        definition.PROCESS_DEFINITION_ID = "company-completion-review"; definition.PROCESS_TYPE = "DRILLING";
        definition.ENTITY_TYPE = "DRILLING_DAILY_REPORT"; definition.PROCESS_NAME = "Independent company completion review";
        definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
            new ProcessStepDefinition { StepId = "inspection", StepName = "Independent inspection and test review", SequenceNumber = 1, StepType = "APPROVAL", RequiresApproval = true, RequiredRoles = ["ConfiguredCompletionReviewer"] },
            new ProcessStepDefinition { StepId = "acceptance", StepName = "Completion acceptance decision", SequenceNumber = 2, StepType = "APPROVAL", RequiresApproval = true, RequiredRoles = ["ConfiguredCompletionReviewer"] } } });
        f.Funding.Routing.Catalog.Workflow.Native.Rows.Add(definition);
    }
}
#endif
