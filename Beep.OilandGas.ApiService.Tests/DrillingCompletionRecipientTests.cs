using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingCompletionRecipientTests
{
    [Theory, InlineData("current"), InlineData("review-current"), InlineData("claimant"), InlineData("permission"), InlineData("role"), InlineData("workspace"),
        InlineData("profile"), InlineData("inactive"), InlineData("field"), InlineData("company")]
    public async Task IndependentCompletionRecipientsUseActualCompanyRolesWorkspacesApprovalAndOriginalFieldRead(string fault)
    {
        var history = await DrillingCompletionRoutingTests.Source();
        DrillingCompletionReviewStartNativeTests.Fixture? completed=null;
        if(fault=="review-current"){completed=new();await completed.Prepare();history=completed.Current.History;}
        var scope = history.Execution.Basis.Scope();
        using var f = new DevelopmentReviewCatalogTests.Fixture(true); await f.UseCompanyRecipients(); var db = f.Draft.Db;
        var actor = fault == "claimant" ? history.ReportedCompletion!.RecorderUserId : "completion-inspector";
        if (!await db.Users.AnyAsync(row => row.Id == actor)) db.Users.Add(new() { Id = actor, IsActive = true });
        if (!await db.Set<AppCompanyMembership>().AnyAsync(row => row.UserId == actor && row.BusinessAssociateId == "company"))
            db.Add(new AppCompanyMembership { TenantId = "tenant", BusinessAssociateId = "company", UserId = actor, GrantedByUserId = "administrator", EffectiveFromUtc = DateTime.UtcNow.AddDays(-1) });
        db.Roles.Add(new() { Id = "completion-role", Name = "ConfiguredCompletionReviewer", NormalizedName = "CONFIGUREDCOMPLETIONREVIEWER" });
        db.UserRoles.Add(new() { UserId = actor, RoleId = "completion-role" });
        db.Add(new AppUserRoleExtension { TenantId = "tenant", BusinessAssociateId = "company", UserId = actor, RoleId = "completion-role", ApprovalStatus = "Approved", EffectiveFromUtc = DateTime.UtcNow.AddDays(-1) });
        db.Add(new AppPersona { Code = "COMPLETION_INSPECTOR", Name = "Independent company completion inspector" });
        db.Add(new AppPersona { Code = "UNMAPPED_COMPLETION", Name = "Unmapped completion profile" });
        var profile = await db.Set<AppUserPersona>().SingleOrDefaultAsync(row => row.UserId == actor);
        if (profile is null) db.Add(new AppUserPersona { UserId = actor,TenantId="tenant",BusinessAssociateId="company", PersonaCode = "COMPLETION_INSPECTOR" }); else profile.PersonaCode = "COMPLETION_INSPECTOR";
        foreach (var permission in new[] { "Drilling.View", "Drilling.ViewDailyReports", "Drilling.Approve", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View", "Workflow.Approve" })
            db.RoleClaims.Add(new() { RoleId = "completion-role", ClaimType = "permission", ClaimValue = permission });
        db.RoleClaims.Add(new() { RoleId = "completion-role", ClaimType = RepositoryPersonaService.WorkspaceClaimType, ClaimValue = "COMPLETION_INSPECTOR" });
        var grant = new AppUserAssetAccess { Id = "completion-field", UserId = actor, DatabaseScope = scope.CoreDatabase.Fingerprint, OrganizationId = "company", AssetType = "FIELD", AssetId = scope.FieldId, AccessLevel = "READ", IsActive = true };
        db.Add(grant); await db.SaveChangesAsync();
        if (fault == "permission") db.Remove(await db.RoleClaims.SingleAsync(row => row.RoleId == "completion-role" && row.ClaimValue == "Drilling.Approve"));
        if (fault == "role") (await db.Set<AppUserRoleExtension>().SingleAsync(row => row.UserId == actor && row.RoleId == "completion-role")).ApprovalStatus = "Revoked";
        if (fault == "workspace") db.Remove(await db.RoleClaims.SingleAsync(row => row.RoleId == "completion-role" && row.ClaimType == RepositoryPersonaService.WorkspaceClaimType));
        if (fault == "profile") (await db.Set<AppUserPersona>().SingleAsync(row => row.UserId == actor)).PersonaCode = "UNMAPPED_COMPLETION";
        if (fault == "inactive") (await db.Users.SingleAsync(row => row.Id == actor)).IsActive = false;
        if (fault == "field") grant.AssetId = "foreign-field"; if (fault == "company") grant.OrganizationId = "foreign-company";
        await db.SaveChangesAsync();
        var assets = new Mock<IWorkflowRecipientAssetReader>(MockBehavior.Strict);
        assets.Setup(a => a.ReadExecutionRecipientFieldGrantsAsync(scope, It.IsAny<IReadOnlyList<string>>())).ReturnsAsync(() => db.Set<AppUserAssetAccess>().AsNoTracking().ToArray());
        var owner = new DevelopmentReviewRecipientAuthority(new RepositoryAccessService(db), new RepositoryPersonaService(db), assets.Object);
        DevelopmentReviewRecipientStep result;
        if(fault=="review-current")
        {
            var basis=history.Execution.Basis with{Source=DrillingProgramAssignedReviewTests.AsReader(history.Execution.Basis.Source,actor),RecorderRoles=[],PersonaCode=null,RecorderAuthorityRevision=null};basis=basis with{Revision=basis.ContentRevision()};
            var execution=history.Execution with{Basis=basis};execution=execution with{Revision=execution.ContentRevision()};var current=history with{Execution=execution};current=current with{Revision=current.ContentRevision()};
            var page=completed!.Current with{History=current};page=page with{Revision=page.ContentRevision()};page.Require();
            assets.Setup(reader=>reader.ReadCompletionReviewRecipientFieldGrantsAsync(page.History.Execution.Basis.Scope(),It.IsAny<IReadOnlyList<string>>())).ReturnsAsync(()=>db.Set<AppUserAssetAccess>().AsNoTracking().ToArray());
            result=await owner.ReadCompletionReviewAsync(page,"inspect","ConfiguredCompletionReviewer",completed.Original);
            assets.Verify(reader=>reader.ReadExecutionRecipientFieldGrantsAsync(It.IsAny<DevelopmentAssignedReviewScope>(),It.IsAny<IReadOnlyList<string>>()),Times.Never());
        }
        else result = await owner.ReadCompletionAsync(history, "inspect", "ConfiguredCompletionReviewer");
        if (fault is "current" or "review-current") Assert.Equal(actor, Assert.Single(result.Recipients).UserId); else Assert.Empty(result.Recipients);
        Assert.True(ProcessDefinitionRevision.IsValid(result.AuthorityRevision));
    }
}
#endif
