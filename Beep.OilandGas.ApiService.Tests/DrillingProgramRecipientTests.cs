using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingProgramRecipientTests
{
    [Theory][InlineData("review-current")][InlineData("current")][InlineData("role")][InlineData("permission")][InlineData("grant")][InlineData("grant-company")][InlineData("workspace")][InlineData("profile")][InlineData("inactive")][InlineData("maker")][InlineData("sender")]
    public async Task ActualCompanyRolesAndWorkspaceOwnersQualifyOnlyIndependentCurrentOriginalFieldReviewers(string fault)
    {
        using var program=new DrillingProgramDraftNativeTests.Fixture();await program.Prepare();var source=DrillingProgramReviewTests.Review(program).Source;var scope=source.Basis.Scope();using var f=new DevelopmentReviewCatalogTests.Fixture(true);await f.UseCompanyRecipients();var db=f.Draft.Db;
        var actor=fault=="maker"?source.Program.ROW_CREATED_BY:fault=="sender"?scope.ActorUserId:"program-reviewer";
        if(!await db.Users.AnyAsync(row=>row.Id==actor))db.Users.Add(new(){Id=actor,IsActive=true});
        if(!await db.Set<AppCompanyMembership>().AnyAsync(row=>row.UserId==actor && row.BusinessAssociateId=="company"))db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="company",UserId=actor,GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Roles.Add(new(){Id="program-review-role",Name="ConfiguredProgramReviewer",NormalizedName="CONFIGUREDPROGRAMREVIEWER"});db.UserRoles.Add(new(){UserId=actor,RoleId="program-review-role"});db.Add(new AppUserRoleExtension{TenantId="tenant",BusinessAssociateId="company",UserId=actor,RoleId="program-review-role",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Add(new AppPersona{Code="DRILLING_REVIEW",Name="Program engineering review"});var profile=await db.Set<AppUserPersona>().SingleOrDefaultAsync(row=>row.UserId==actor);if(profile is null)db.Add(new AppUserPersona{UserId=actor,TenantId="tenant",BusinessAssociateId="company",PersonaCode="DRILLING_REVIEW"});else profile.PersonaCode="DRILLING_REVIEW";
        foreach(var permission in new[]{"Drilling.View","Drilling.Approve","Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Approve"})db.RoleClaims.Add(new(){RoleId="program-review-role",ClaimType="permission",ClaimValue=permission});db.RoleClaims.Add(new(){RoleId="program-review-role",ClaimType=RepositoryPersonaService.WorkspaceClaimType,ClaimValue="DRILLING_REVIEW"});
        var grant=new AppUserAssetAccess{Id="program-review-field",UserId=actor,DatabaseScope=scope.CoreDatabase.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId=scope.FieldId,AccessLevel="READ",IsActive=true};db.Add(grant);await db.SaveChangesAsync();
        if(fault=="role")(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId==actor && row.RoleId=="program-review-role")).ApprovalStatus="Revoked";
        if(fault=="permission")db.Remove(await db.RoleClaims.SingleAsync(row=>row.RoleId=="program-review-role" && row.ClaimValue=="Drilling.Approve"));
        if(fault=="grant")grant.IsActive=false;if(fault=="grant-company")grant.OrganizationId="foreign";
        if(fault=="workspace")db.Remove(await db.RoleClaims.SingleAsync(row=>row.RoleId=="program-review-role" && row.ClaimType==RepositoryPersonaService.WorkspaceClaimType));
        if(fault=="profile")(await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId==actor)).PersonaCode="FOREIGN_REVIEWER";
        if(fault=="inactive")(await db.Users.SingleAsync(row=>row.Id==actor)).IsActive=false;await db.SaveChangesAsync();
        var assets=new Mock<IWorkflowRecipientAssetReader>(MockBehavior.Strict);assets.Setup(reader=>reader.ReadProgramRecipientFieldGrantsAsync(scope,It.IsAny<IReadOnlyList<string>>())).ReturnsAsync(()=>db.Set<AppUserAssetAccess>().AsNoTracking().ToArray());
        DrillingProgramReviewStart? original=null;
        if(fault=="review-current")
        {
            original=new(source,DrillingProgramReviewTests.Review(program).Options[0],Guid.NewGuid());source=DrillingProgramAssignedReviewTests.AsReader(source,actor);
            assets.Setup(reader=>reader.ReadProgramReviewRecipientFieldGrantsAsync(source.Basis.Scope(),It.IsAny<IReadOnlyList<string>>())).ReturnsAsync(()=>db.Set<AppUserAssetAccess>().AsNoTracking().ToArray());
        }
        var service=new DevelopmentReviewRecipientAuthority(new RepositoryAccessService(db),new RepositoryPersonaService(db),assets.Object);var review=await service.ReadProgramAsync(source,"technical","ConfiguredProgramReviewer",original);
        if(fault=="review-current")assets.Verify(reader=>reader.ReadProgramRecipientFieldGrantsAsync(It.IsAny<DevelopmentAssignedReviewScope>(),It.IsAny<IReadOnlyList<string>>()),Times.Never());
        if(fault is "current" or "review-current")Assert.Equal(actor,Assert.Single(review.Recipients).UserId);else Assert.Empty(review.Recipients);Assert.Equal(0,program.Store.Native.Writes);Assert.Equal(0,f.Workflow.Native.Writes);
    }
}
#endif
