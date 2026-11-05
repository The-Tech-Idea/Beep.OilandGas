#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Approval=Beep.OilandGas.Models.Data.ProductionAccounting.APPROVAL_WORKFLOW;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentInvestmentApiTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentAfeApiTests.Fixture Afe=new();internal string AfeId=null!;internal bool FailInsert,CorruptInsert;internal Action? OnInsert;
        internal async Task Prepare()
        {
            await Afe.Prepare(true);var original=await Afe.Command();AfeId=original.AfeId();await Afe.Service.RecordAsync(Afe.Receiver.User,original);
            var db=Afe.Db;db.Users.Add(new(){Id="investor",IsActive=true});db.Roles.Add(new(){Id="investment-role",Name="InvestmentReviewer",NormalizedName="INVESTMENTREVIEWER"});
            db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="company",UserId="investor",GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
            db.UserRoles.Add(new(){UserId="investor",RoleId="investment-role"});db.Add(new AppUserRoleExtension{UserId="investor",RoleId="investment-role",TenantId="tenant",BusinessAssociateId="company",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
            db.Add(new AppUserAssetAccess{Id="investment-field",UserId="investor",DatabaseScope=original.Review.Source.Review.Scope.CoreDatabase.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId="field",AccessLevel="WRITE",IsActive=true});
            foreach(var permission in new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View",CompanyDevelopmentAfeService.ReviewPermission})db.RoleClaims.Add(new(){RoleId="investment-role",ClaimType="permission",ClaimValue=permission});await db.SaveChangesAsync();
            await Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
            Afe.Native.Native.Source.Setup(source=>source.InsertEntity("APPROVAL_WORKFLOW",It.IsAny<object>())).Returns((string table,object value)=>{
                Afe.Native.Native.Writes++;if(FailInsert)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((Approval)value);if(CorruptInsert)row.AMOUNT=999m;Afe.Native.Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        internal Task<DevelopmentInvestmentReview> Read()=>Afe.Service.ReviewInvestmentAsync(Afe.Receiver.User,Afe.Process,AfeId);
        internal async Task<DevelopmentInvestmentCommand> Command(string decision="APPROVE")=>new(await Read(),Guid.NewGuid(),decision,"Independent company investment budget decision");
        internal Task<DevelopmentInvestmentReceipt> Decide(DevelopmentInvestmentCommand original,bool recover=false)=>Afe.Service.DecideInvestmentAsync(Afe.Receiver.User,original,recover);
        public void Dispose()=>Afe.Dispose();
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task ThirdCurrentFinanceActorReadsQueueAndDecidesWithoutTechnicalApprovalOrPrepareRights(string decision)
    {
        using var f=new Fixture();await f.Prepare();var queue=await f.Afe.Service.QueueAsync(f.Afe.Receiver.User);var entry=Assert.Single(queue.Items);Assert.Equal(f.AfeId,entry.Source.Afe.AfeId);Assert.Null(entry.Recorded);
        Assert.DoesNotContain("Accounting.PrepareDevelopmentBudget",f.Afe.Receiver.Tenant.Company!.Permissions);Assert.DoesNotContain("DevelopmentPlanning.Approve",f.Afe.Receiver.Tenant.Company.Permissions);
        var command=await f.Command(decision);var before=JsonSerializer.Serialize(f.Afe.Native.Native.Rows.OfType<Beep.OilandGas.Models.Data.ProductionAccounting.AFE>().Single());var receipt=await f.Decide(command);receipt.Require(command);
        Assert.Equal(decision,(await f.Read()).Recorded!.Decision);Assert.Equal(decision,Assert.Single((await f.Afe.Service.QueueAsync(f.Afe.Receiver.User)).Items).Recorded!.Decision);Assert.Equal(2,f.Afe.Native.Native.Writes);
        Assert.Equal(before,JsonSerializer.Serialize(f.Afe.Native.Native.Rows.OfType<Beep.OilandGas.Models.Data.ProductionAccounting.AFE>().Single()));await f.Decide(command,true);Assert.Equal(2,f.Afe.Native.Native.Writes);
        await f.Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");Assert.Equal(decision,(await f.Read()).Recorded!.Decision);
        f.Afe.Db.RoleClaims.Add(new(){RoleId="review-role",ClaimType="permission",ClaimValue="Accounting.View"});await f.Afe.Db.SaveChangesAsync();await f.Afe.Receiver.SelectReceiver();Assert.Equal(decision,(await f.Read()).Recorded!.Decision);
        await f.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Afe.Receiver.Service.ReadProgressAsync(f.Afe.Receiver.User,f.Afe.Process));
    }
    [Theory][InlineData("permission")][InlineData("role")][InlineData("grant")][InlineData("resource")][InlineData("binding")][InlineData("actor")]
    public async Task NewIndependentDecisionRequiresCurrentOriginalCompanyActorRolePermissionAndFieldWrite(string fault)
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();var db=f.Afe.Db;
        if(fault=="permission")db.Remove(await db.RoleClaims.SingleAsync(row=>row.ClaimValue==CompanyDevelopmentAfeService.ReviewPermission));
        if(fault=="role")(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="investor")).ApprovalStatus="Revoked";
        if(fault=="grant")(await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="investor")).AccessLevel="READ";
        if(fault=="resource")f.Afe.Receiver.Resources.Setup(value=>value.CheckAssetAccessAsync("investor","field","FIELD","WRITE",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
        if(fault=="binding")(await db.Set<ModuleDatabaseBinding>().SingleAsync(row=>row.ModuleId=="PRODUCTION")).ConcurrencyStamp="changed";
        await db.SaveChangesAsync();var user=fault=="actor"?new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","another"),new Claim(ClaimTypes.Role,"InvestmentReviewer")],"fixture")):f.Afe.Receiver.User;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Afe.Service.DecideInvestmentAsync(user,command));Assert.Equal(1,f.Afe.Native.Native.Writes);
    }
    [Fact] public async Task MakerCannotSelfApproveEvenAfterReceivingReviewPermission()
    {
        using var f=new Fixture();await f.Prepare();f.Afe.Db.RoleClaims.Add(new(){RoleId="finance-role",ClaimType="permission",ClaimValue=CompanyDevelopmentAfeService.ReviewPermission});await f.Afe.Db.SaveChangesAsync();await f.Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");
        var review=await f.Read();Assert.Contains(review.Blockers,text=>text.Contains("maker"));Assert.Throws<InvalidOperationException>(()=>new DevelopmentInvestmentCommand(review,Guid.NewGuid(),"APPROVE","Attempted own funding approval").Require());Assert.Equal(1,f.Afe.Native.Native.Writes);
    }
    [Fact] public async Task OriginalRecoveryAndPublicOutcomeUseCurrentViewReadAfterReviewPermissionAndWriteRevocation()
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();await f.Decide(command);
        f.Afe.Db.Remove(await f.Afe.Db.RoleClaims.SingleAsync(row=>row.ClaimValue==CompanyDevelopmentAfeService.ReviewPermission));(await f.Afe.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="investor")).AccessLevel="READ";await f.Afe.Db.SaveChangesAsync();await f.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
        Assert.NotNull((await f.Decide(command,true)).Recorded);Assert.NotNull((await f.Read()).Recorded);Assert.Single((await f.Afe.Service.QueueAsync(f.Afe.Receiver.User)).Items);await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(command));Assert.Equal(2,f.Afe.Native.Native.Writes);
    }
    [Fact] public async Task CurrentFieldReadDenialOmitsQueueBeforePrivateRequestProofAndEmptyAuthorityNeverInventsApproval()
    {
        using var f=new Fixture();await f.Prepare();f.Afe.Receiver.Resources.Setup(value=>value.CheckAssetAccessAsync("investor","field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
        f.Afe.Native.Native.Rows.OfType<Beep.OilandGas.Models.Data.ProductionAccounting.AFE>().Single().DEVELOPMENT_BUDGET_JSON="malformed private body";
        var queue=await f.Afe.Service.QueueAsync(f.Afe.Receiver.User);Assert.Empty(queue.Items);Assert.Equal(1,f.Afe.Native.Native.Writes);
    }
    [Fact] public async Task FreshRoleRevocationDuringNativeDecisionRollsBackAndLostCommitCanRecoverExactOriginal()
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();var claim=await f.Afe.Db.RoleClaims.SingleAsync(row=>row.ClaimValue==CompanyDevelopmentAfeService.ReviewPermission);
        f.OnInsert=()=>{f.Afe.Db.Remove(claim);f.Afe.Db.SaveChanges();};await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(command));Assert.Empty(f.Afe.Native.Native.Rows.OfType<Approval>());
        f.Afe.Db.RoleClaims.Add(new(){RoleId="investment-role",ClaimType="permission",ClaimValue=CompanyDevelopmentAfeService.ReviewPermission});await f.Afe.Db.SaveChangesAsync();await f.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");f.OnInsert=()=>f.Afe.Native.Native.FailCommit=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(command));f.Afe.Native.Native.FailCommit=false;f.OnInsert=null;var writes=f.Afe.Native.Native.Writes;Assert.NotNull((await f.Decide(command,true)).Recorded);Assert.Equal(writes,f.Afe.Native.Native.Writes);
    }
    [Fact] public async Task DecisionRecordedDuringQueueLoadingCannotBePublishedAsStillPending()
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();var reads=0;var native=f.Afe.Native.Native;
        native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>{
            if(table=="APPROVAL_WORKFLOW" && ++reads==5)native.Rows.Add(command.ToRecord(DateTime.UtcNow));
            return native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
        });
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Afe.Service.QueueAsync(f.Afe.Receiver.User));Assert.True(reads>=5);Assert.Equal(1,native.Writes);
    }
    [Fact] public async Task AnotherAuthorizedCompanyWithSameActorRoleAndFieldCannotSeeOriginalNativeRequests()
    {
        using var f=new Fixture();await f.Prepare();var db=f.Afe.Db;
        db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="foreign",UserId="investor",GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Add(new AppUserRoleExtension{UserId="investor",RoleId="investment-role",TenantId="tenant",BusinessAssociateId="foreign",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});await db.SaveChangesAsync();await f.Afe.Receiver.SelectReceiver("investor",new("tenant","foreign"),"InvestmentReviewer");
        var core=await f.Afe.Receiver.Company.ResolveAsync();db.Add(new AppUserAssetAccess{Id="foreign-investment-field",UserId="investor",DatabaseScope=core.Fingerprint,OrganizationId="foreign",AssetType="FIELD",AssetId="field",AccessLevel="READ",IsActive=true});await db.SaveChangesAsync();
        f.Afe.Native.Native.Rows.OfType<Beep.OilandGas.Models.Data.ProductionAccounting.AFE>().Single().DEVELOPMENT_BUDGET_JSON="private malformed original must not be parsed";
        var queue=await f.Afe.Service.QueueAsync(f.Afe.Receiver.User);Assert.Empty(queue.Items);Assert.Equal("foreign",queue.Company.BusinessAssociateId);
        f.Afe.Native.Native.Source.Verify(source=>source.GetEntityAsync("AFE",It.Is<List<TheTechIdea.Beep.Report.AppFilter>>(filters=>filters.Any(filter=>filter.FieldName=="OWNER_COMPANY_BA_ID" && filter.FilterValue=="foreign"))),Times.AtLeastOnce());
    }
    [Fact] public async Task ActualTypedHttpFinancialEvidenceIsReadOnlyAndStandaloneWriteRoutesAreRetired()
    {
        using var f=new Fixture();await f.Prepare();await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{
            services.AddScoped<CompanyDevelopmentAfeService>(provider=>f.Afe.Build(provider.GetRequiredService<IHttpContextAccessor>()));services.AddSingleton(Mock.Of<IFailureReporter>());
        });host.Client.DefaultRequestHeaders.Add("X-Test-Actor","investor");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","InvestmentReviewer");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();var selected=f.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),"investor",new(selected.Scope,selected.Roles.ToArray(),selected.Permissions.ToArray()));
        var client=new Beep.OilandGas.Web.Services.AfeServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));Assert.Single((await client.QueueAsync()).Items);var review=await client.ReviewInvestmentAsync(f.Afe.Process,f.AfeId);var command=new DevelopmentInvestmentCommand(review,Guid.NewGuid(),"REJECT","Independent HTTP investment decision");
        foreach(var route in new[]{"/api/accounting/cost/afe/development/investments","/api/accounting/cost/afe/development/investments/recover"})
        {var response=await host.Client.PostAsJsonAsync(route,command);Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);}
        Assert.Null((await client.ReviewInvestmentAsync(f.Afe.Process,f.AfeId)).Recorded);Assert.Equal(1,f.Afe.Native.Native.Writes);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync("/api/accounting/cost/afe/development/queue")).StatusCode);
    }
}
#endif
