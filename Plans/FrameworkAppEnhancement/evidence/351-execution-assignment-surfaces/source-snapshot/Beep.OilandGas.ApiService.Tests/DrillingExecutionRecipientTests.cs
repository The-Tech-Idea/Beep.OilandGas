#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingExecutionRecipientTests
{
    [Theory,InlineData("current"),InlineData("sender"),InlineData("maker"),InlineData("read-only"),InlineData("role"),InlineData("permission"),
        InlineData("workspace"),InlineData("profile"),InlineData("inactive"),InlineData("field"),InlineData("company")]
    public async Task ExistingCompanyRoleWorkspaceAndWriteGrantOwnersQualifyOperationalRecipientsWithoutApprovalIndependence(string fault)
    {
        var native=new DrillingExecutionAssignmentNativeTests.Fixture();await native.Prepare();var source=native.Current;var scope=source.Basis.Scope();
        using var f=new DevelopmentReviewCatalogTests.Fixture(true);await f.UseCompanyRecipients();var db=f.Draft.Db;
        var actor=fault=="sender"?scope.ActorUserId:fault=="maker"?source.Recorded!.ROW_CREATED_BY:"reporting-recipient";
        if(!await db.Users.AnyAsync(row=>row.Id==actor))db.Users.Add(new(){Id=actor,IsActive=true});
        if(!await db.Set<AppCompanyMembership>().AnyAsync(row=>row.UserId==actor && row.BusinessAssociateId=="company"))
            db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="company",UserId=actor,GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Roles.Add(new(){Id="reporting-role",Name="ConfiguredReportingRole",NormalizedName="CONFIGUREDREPORTINGROLE"});db.UserRoles.Add(new(){UserId=actor,RoleId="reporting-role"});
        db.Add(new AppUserRoleExtension{TenantId="tenant",BusinessAssociateId="company",UserId=actor,RoleId="reporting-role",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Add(new AppPersona{Code="REPORTING_SITE",Name="Company reporting site"});db.Add(new AppPersona{Code="UNMAPPED_REPORTING",Name="Unmapped reporting profile"});
        var profile=await db.Set<AppUserPersona>().SingleOrDefaultAsync(row=>row.UserId==actor);
        if(profile is null)db.Add(new AppUserPersona{UserId=actor,PersonaCode="REPORTING_SITE"});else profile.PersonaCode="REPORTING_SITE";
        foreach(var permission in new[]{"Drilling.View","Drilling.Edit","Drilling.ManageWellProgram","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"})
            db.RoleClaims.Add(new(){RoleId="reporting-role",ClaimType="permission",ClaimValue=permission});
        db.RoleClaims.Add(new(){RoleId="reporting-role",ClaimType=RepositoryPersonaService.WorkspaceClaimType,ClaimValue="REPORTING_SITE"});
        var grant=new AppUserAssetAccess{Id="reporting-field",UserId=actor,DatabaseScope=scope.CoreDatabase.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId=scope.FieldId,AccessLevel="WRITE",IsActive=true};db.Add(grant);await db.SaveChangesAsync();
        if(fault=="read-only")grant.AccessLevel="READ";
        if(fault=="role")(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId==actor && row.RoleId=="reporting-role")).ApprovalStatus="Revoked";
        if(fault=="permission")db.Remove(await db.RoleClaims.SingleAsync(row=>row.RoleId=="reporting-role" && row.ClaimValue=="Drilling.Edit"));
        if(fault=="workspace")db.Remove(await db.RoleClaims.SingleAsync(row=>row.RoleId=="reporting-role" && row.ClaimType==RepositoryPersonaService.WorkspaceClaimType));
        if(fault=="profile")(await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId==actor)).PersonaCode="UNMAPPED_REPORTING";
        if(fault=="inactive")(await db.Users.SingleAsync(row=>row.Id==actor)).IsActive=false;
        if(fault=="field")grant.AssetId="other-field";if(fault=="company")grant.OrganizationId="other-company";await db.SaveChangesAsync();
        var assets=new Mock<IWorkflowRecipientAssetReader>(MockBehavior.Strict);
        assets.Setup(a=>a.ReadExecutionRecipientFieldGrantsAsync(scope,It.IsAny<IReadOnlyList<string>>())).ReturnsAsync(()=>db.Set<AppUserAssetAccess>().AsNoTracking().ToArray());
        var owner=new DevelopmentReviewRecipientAuthority(new RepositoryAccessService(db),new RepositoryPersonaService(db),assets.Object);
        var result=await owner.ReadExecutionReportingAsync(source,"report-progress","ConfiguredReportingRole");
        if(fault is "current" or "sender" or "maker")Assert.Equal(actor,Assert.Single(result.Recipients).UserId);else Assert.Empty(result.Recipients);
        Assert.Equal(0,native.Lifecycle.Native.Writes);Assert.True(ProcessDefinitionRevision.IsValid(result.AuthorityRevision));
    }
}
#endif
