#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentInvestmentRoutingTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentInvestmentApiTests.Fixture Investment=new();
        internal DevelopmentAfeApiTests.Fixture Afe=>Investment.Afe;
        internal DevelopmentReviewCatalogTests.Fixture Catalog=>Afe.Receiver.Sender.Catalog;
        internal PROCESS_DEFINITION Definition=null!;
        internal CompanyDevelopmentInvestmentRoutingService Service(IRepositoryAccessService? accounts=null)=>new(Afe.Service,Afe.Receiver.Company,Afe.Receiver.Modules,
            new BoundProcessService(()=>Task.FromResult("lifecycle"),_=>Catalog.Workflow.Service),accounts??new RepositoryAccessService(Afe.Db),new RepositoryPersonaService(Afe.Db),
            new RepositoryAssetAccessStore(Afe.Db,Afe.Receiver.Company,Mock.Of<IAssetGrantTargetVerifier>()),Catalog.Draft.Drafts.Native.Editor.Object);
        internal async Task Prepare()
        {
            await Investment.Prepare();var db=Afe.Db;
            db.RoleClaims.Add(new(){RoleId="finance-role",ClaimType="permission",ClaimValue="Workflow.Start"});
            db.RoleClaims.Add(new(){RoleId="investment-role",ClaimType="permission",ClaimValue="Workflow.Approve"});
            db.RoleClaims.Add(new(){RoleId="investment-role",ClaimType=RepositoryPersonaService.WorkspaceClaimType,ClaimValue="FINANCE"});
            db.Add(new AppPersona{Code="FINANCE",Name="Investment finance"});db.Add(new AppUserPersona{UserId="investor",TenantId="tenant",BusinessAssociateId="company",PersonaCode="FINANCE"});await db.SaveChangesAsync();
            Definition=CompanyInventorySourceReview.Copy(Catalog.Workflow.Definition);Definition.PROCESS_DEFINITION_ID="company-investment-review";Definition.PROCESS_TYPE="ACCOUNTING";Definition.ENTITY_TYPE="AFE";Definition.PROCESS_NAME="Configured company investment review";
            Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new ProcessStepDefinition{StepId="investment",StepName="Independent investment review",SequenceNumber=1,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["InvestmentReviewer"]}}});
            Catalog.Workflow.Native.Rows.Add(Definition);await Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");
        }
        internal Task<DevelopmentInvestmentRoutingReview> Read()=>Service().ReadAsync(Afe.Receiver.User,Afe.Process,Investment.AfeId);
        public void Dispose()=>Investment.Dispose();
    }
    [Fact]public async Task ActualMakerCanInspectCurrentConfiguredIndependentRecipientsWithoutAnyBusinessWrite()
    {
        using var f=new Fixture();await f.Prepare();var writes=f.Catalog.Workflow.Native.Writes;
        var review=await f.Read();review.Require(new("tenant","company"),"accountant",f.Afe.Process,f.Investment.AfeId);
        var option=Assert.Single(review.Options);Assert.True(option.Ready);Assert.Equal(f.Definition.PROCESS_DEFINITION_ID,option.ProcessId);
        Assert.Equal("InvestmentReviewer",Assert.Single(option.Stages).AssignedRole);Assert.Equal(new RepositoryCompanyRoleRecipient("investor","FINANCE"),Assert.Single(option.Stages[0].Recipients));
        Assert.Contains(DevelopmentInvestmentReview.MakerDecisionBlocker,review.Source.Blockers);Assert.Equal(writes,f.Catalog.Workflow.Native.Writes);Assert.Equal(1,f.Afe.Native.Native.Writes);
        Assert.Empty(f.Catalog.Workflow.Native.Rows.OfType<PROCESS_INSTANCE>().Where(row=>row.ENTITY_TYPE=="AFE"));Assert.Empty(f.Catalog.Workflow.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>row.ENTITY_TYPE=="AFE"));
        Assert.Equal(JsonSerializer.Serialize(review),JsonSerializer.Serialize(JsonSerializer.Deserialize<DevelopmentInvestmentRoutingReview>(JsonSerializer.Serialize(review))));
    }
    [Theory][InlineData("role")][InlineData("permission")][InlineData("grant")][InlineData("grant-company")][InlineData("workspace")][InlineData("profile")][InlineData("inactive")]
    public async Task IneligibleIndependentRecipientsRemainAnExplicitBlockedStage(string fault)
    {
        using var f=new Fixture();await f.Prepare();var db=f.Afe.Db;
        if(fault=="role")(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="investor")).ApprovalStatus="Revoked";
        if(fault=="permission")db.Remove(await db.RoleClaims.SingleAsync(row=>row.RoleId=="investment-role" && row.ClaimValue=="Workflow.Approve"));
        if(fault=="grant")(await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="investor")).AccessLevel="READ";
        if(fault=="grant-company")(await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="investor")).OrganizationId="foreign";
        if(fault=="workspace")db.Remove(await db.RoleClaims.SingleAsync(row=>row.RoleId=="investment-role" && row.ClaimType==RepositoryPersonaService.WorkspaceClaimType));
        if(fault=="profile")(await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId=="investor")).PersonaCode="FOREIGN_REVIEWER";
        if(fault=="inactive")(await db.Users.SingleAsync(row=>row.Id=="investor")).IsActive=false;
        await db.SaveChangesAsync();var option=Assert.Single((await f.Read()).Options);Assert.False(option.Ready);Assert.Empty(Assert.Single(option.Stages).Recipients);Assert.Single(option.Blockers);Assert.Equal(1,f.Afe.Native.Native.Writes);
    }
    [Theory][InlineData("permission")][InlineData("role")][InlineData("field")][InlineData("binding")]
    public async Task CurrentSenderAndOriginalFinancialSourceAuthorityAreRequiredBeforePrivateRecipientLookup(string fault)
    {
        using var f=new Fixture();await f.Prepare();var db=f.Afe.Db;
        if(fault=="permission")db.Remove(await db.RoleClaims.SingleAsync(row=>row.RoleId=="finance-role" && row.ClaimValue=="Workflow.Start"));
        if(fault=="role")(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="accountant")).ApprovalStatus="Revoked";
        if(fault=="field")(await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="accountant")).IsActive=false;
        if(fault=="binding")(await db.Set<ModuleDatabaseBinding>().SingleAsync(row=>row.ModuleId=="LIFECYCLE")).ConcurrencyStamp="changed";
        await db.SaveChangesAsync();await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());Assert.Equal(1,f.Afe.Native.Native.Writes);
    }
    [Fact]public async Task MissingConfigurationDoesNotInventDefaultInvestmentRoleOrTasks()
    {
        using var f=new Fixture();await f.Prepare();f.Catalog.Workflow.Native.Rows.Remove(f.Definition);Assert.Empty((await f.Read()).Options);Assert.Equal(1,f.Afe.Native.Native.Writes);
    }
    [Theory][InlineData("definition")][InlineData("grant")][InlineData("permission")][InlineData("source")]
    public async Task SourceDefinitionAndRecipientChangesAcrossObservationsCannotPublishReadiness(string fault)
    {
        using var f=new Fixture();await f.Prepare();var accounts=new RepositoryAccessService(f.Afe.Db);var reads=0;var reader=new Mock<IRepositoryAccessService>(MockBehavior.Strict);
        reader.Setup(value=>value.GetCompanyAccessAsync(It.IsAny<string>(),It.IsAny<CompanyScope>(),It.IsAny<CancellationToken>()))
            .Returns((string actor,CompanyScope company,CancellationToken token)=>accounts.GetCompanyAccessAsync(actor,company,token));
        reader.Setup(value=>value.GetCompanyRoleRecipientsAsync(It.IsAny<CompanyScope>(),It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(async(CompanyScope company,string role,CancellationToken token)=>{
                if(++reads==2)
                {
                    if(fault=="definition")f.Definition.VERSION="changed";
                    if(fault=="grant")(await f.Afe.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="investor")).AccessLevel="READ";
                    if(fault=="permission")f.Afe.Db.Remove(await f.Afe.Db.RoleClaims.SingleAsync(row=>row.RoleId=="investment-role" && row.ClaimValue=="Workflow.Approve"));
                    if(fault=="source")f.Afe.Native.Planning.Planning.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single(row=>row.FDP_STATUS=="APPROVED").FDP_NAME="Changed approved source";
                    await f.Afe.Db.SaveChangesAsync();
                }
                return await accounts.GetCompanyRoleRecipientsAsync(company,role,token);
            });
        var failure=await Assert.ThrowsAnyAsync<Exception>(()=>f.Service(reader.Object).ReadAsync(f.Afe.Receiver.User,f.Afe.Process,f.Investment.AfeId));
        Assert.IsNotType<MockException>(failure);Assert.Equal(2,reads);Assert.Equal(1,f.Afe.Native.Native.Writes);
    }
    [Theory][InlineData("role")][InlineData("conditional")][InlineData("configuration")][InlineData("non-approval")][InlineData("optional")][InlineData("next")][InlineData("name")]
    public async Task UnsupportedConfiguredDecisionSemanticsCannotBePresentedAsReady(string fault)
    {
        using var f=new Fixture();await f.Prepare();var steps=new[]{new ProcessStepDefinition{StepId="investment",StepName="Investment",SequenceNumber=1,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["InvestmentReviewer"]}};
        if(fault=="role")steps[0].RequiredRoles.Add("AnotherRole");if(fault=="conditional")steps[0].ConditionalNextSteps=["guessed"];if(fault=="configuration")steps[0].StepConfiguration=new(){["AmountLimit"]=1};if(fault=="non-approval")steps[0].RequiresApproval=false;
        if(fault=="optional")steps[0].IsRequired=false;if(fault=="next")steps[0].NextStepId="unknown";if(fault=="name")steps[0].StepName="";
        f.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=steps});await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());
    }
    [Fact]public async Task DecidedRequestHasNoProposedAssignmentAndKeepsItsRecordedOutcome()
    {
        using var f=new Fixture();await f.Prepare();await f.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");await f.Investment.Decide(await f.Investment.Command());await f.Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");
        var review=await f.Read();Assert.Equal("APPROVE",review.Source.Recorded!.Decision);Assert.Empty(review.Options);Assert.Equal(2,f.Afe.Native.Native.Writes);
    }
    [Fact]public async Task ActualProtectedTypedHttpReturnsExactRoutingAndRejectsChangedAuthority()
    {
        using var f=new Fixture();await f.Prepare();await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{
            services.AddScoped<CompanyDevelopmentAfeService>(provider=>f.Afe.Build(provider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));
            services.AddSingleton(f.Service());services.AddSingleton(Mock.Of<IFailureReporter>());
        });host.Client.DefaultRequestHeaders.Add("X-Test-Actor","accountant");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","FinanceRole");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();var current=f.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),"accountant",new(current.Scope,current.Roles.ToArray(),current.Permissions.ToArray()));
        var client=new Beep.OilandGas.Web.Services.AfeServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));Assert.True(Assert.Single((await client.ReviewInvestmentRoutingAsync(f.Afe.Process,f.Investment.AfeId)).Options).Ready);
        f.Afe.Db.Remove(await f.Afe.Db.RoleClaims.SingleAsync(row=>row.RoleId=="finance-role" && row.ClaimValue=="Workflow.Start"));await f.Afe.Db.SaveChangesAsync();
        var path="/api/accounting/cost/afe/development/"+Uri.EscapeDataString(f.Investment.AfeId)+"/routing?processId="+Uri.EscapeDataString(f.Afe.Process);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(path)).StatusCode);host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync(path)).StatusCode);Assert.Equal(1,f.Afe.Native.Native.Writes);
    }
}
#endif
