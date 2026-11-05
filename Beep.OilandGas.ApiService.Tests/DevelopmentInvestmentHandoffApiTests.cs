#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentInvestmentHandoffApiTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentInvestmentRoutingTests.Fixture Routing=new();internal DevelopmentInvestmentHandoff Original=null!;
        internal async Task Prepare(int stages=1)
        {
            await Routing.Prepare();
            if(stages>1)Routing.Definition.PROCESS_CONFIG_JSON=System.Text.Json.JsonSerializer.Serialize(new{Steps=Enumerable.Range(1,stages).Select(index=>new Beep.OilandGas.Models.Processes.ProcessStepDefinition{
                StepId="investment-"+index,StepName="Independent investment review "+index,SequenceNumber=index,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["InvestmentReviewer"]}).ToArray()});
            var service=Routing.Service();var review=await Routing.Read();Original=await service.PrepareHandoffAsync(Routing.Afe.Receiver.User,Routing.Afe.Process,Routing.Investment.AfeId,new(review.Revision,review.Options.Single().ProcessId));
        }
        internal Task<DevelopmentInvestmentHandoffReceipt> Start(bool recover=false)=>Routing.Service().SubmitHandoffAsync(Routing.Afe.Receiver.User,Original,recover);
        internal Task<DevelopmentInvestmentHandoffStatus> Status()=>Routing.Service().ReadHandoffStatusAsync(Routing.Afe.Receiver.User,Routing.Afe.Process,Routing.Investment.AfeId);
        public void Dispose()=>Routing.Dispose();
    }
    [Fact]public async Task ActualFrozenPreparationCreatesOneNativeAssignmentAndSurvivesSenderReloadWithoutAnotherBudgetWrite()
    {
        using var f=new Fixture();await f.Prepare();var native=f.Routing.Catalog.Workflow.Native;var writes=native.Writes;Assert.Null((await f.Status()).HandoffId);Assert.Equal(writes,native.Writes);
        var receipt=await f.Start();receipt.Require(f.Original);Assert.True(receipt.Recorded);Assert.Equal(writes+4,native.Writes);Assert.Equal(1,f.Routing.Afe.Native.Native.Writes);
        var status=await f.Status();Assert.Equal(receipt.InstanceId,status.HandoffId);Assert.Equal("accountant",status.Sender);Assert.Equal(f.Original.Workflow.ProcessName,status.WorkflowName);Assert.Equal(1,status.Progress!.PendingTasks);
        var reloaded=await f.Routing.Read();Assert.Equal(receipt.InstanceId,reloaded.ExistingHandoffId);Assert.Empty(reloaded.Options);
        Assert.True((await f.Start(true)).Recorded);Assert.Equal(writes+4,native.Writes);Assert.Equal(1,f.Routing.Afe.Native.Native.Writes);
    }
    [Fact]public async Task CurrentWorkflowStartAndFieldWriteCannotBeReplacedByPreviouslyPreparedInput()
    {
        using var f=new Fixture();await f.Prepare();var db=f.Routing.Afe.Db;var native=f.Routing.Catalog.Workflow.Native;var writes=native.Writes;
        var grant=await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="accountant");grant.AccessLevel="READ";await db.SaveChangesAsync();
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(writes,native.Writes);
        grant.AccessLevel="WRITE";var permission=await db.RoleClaims.SingleAsync(row=>row.RoleId=="finance-role" && row.ClaimValue=="Workflow.Start");db.Remove(permission);await db.SaveChangesAsync();await f.Routing.Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(writes,native.Writes);
    }
    [Fact]public async Task CommitUncertaintyRecoversWithCurrentViewReadAfterStartAndRecipientAuthorityAreRevoked()
    {
        using var f=new Fixture();await f.Prepare();var native=f.Routing.Catalog.Workflow.Native;var writes=native.Writes;
        native.AfterCommit=()=>{if(native.Rows.OfType<PROCESS_INSTANCE>().Any(row=>row.ENTITY_TYPE=="AFE"))native.FailCommit=true;};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());native.AfterCommit=null;native.FailCommit=false;
        Assert.Single(native.Rows.OfType<PROCESS_INSTANCE>().Where(row=>row.ENTITY_TYPE=="AFE"));Assert.Equal(writes+4,native.Writes);
        var db=f.Routing.Afe.Db;db.Remove(await db.RoleClaims.SingleAsync(row=>row.RoleId=="finance-role" && row.ClaimValue=="Workflow.Start"));
        db.Remove(await db.RoleClaims.SingleAsync(row=>row.RoleId=="finance-role" && row.ClaimValue==CompanyDevelopmentAfeService.PreparePermission));
        (await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="accountant")).AccessLevel="READ";(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="investor")).ApprovalStatus="Revoked";await db.SaveChangesAsync();await f.Routing.Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");
        native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION definition && definition.PROCESS_DEFINITION_ID==f.Original.Workflow.ProcessId);
        var recovered=await f.Start(true);Assert.True(recovered.Recorded);Assert.Equal(writes+4,native.Writes);Assert.Equal(1,f.Routing.Afe.Native.Native.Writes);Assert.Equal(recovered.InstanceId,(await f.Status()).HandoffId);
    }
    [Fact]public async Task ChangedIndependentRecipientsOrWorkflowCannotBeSubmittedAsTheOldReadyReview()
    {
        using var f=new Fixture();await f.Prepare();var native=f.Routing.Catalog.Workflow.Native;var writes=native.Writes;
        var grant=await f.Routing.Afe.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="investor");grant.AccessLevel="READ";await f.Routing.Afe.Db.SaveChangesAsync();await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(writes,native.Writes);
        grant.AccessLevel="WRITE";await f.Routing.Afe.Db.SaveChangesAsync();f.Routing.Definition.VERSION="changed";await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(writes,native.Writes);
    }
    [Fact]public async Task ActualTypedHttpPreparesRecordsReloadsAndRecoversTheSameOwnedHandoff()
    {
        using var f=new Fixture();await f.Routing.Prepare();await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{
            services.AddScoped<CompanyDevelopmentAfeService>(provider=>f.Routing.Afe.Build(provider.GetRequiredService<IHttpContextAccessor>()));services.AddSingleton(f.Routing.Service());services.AddSingleton(Mock.Of<IFailureReporter>());
        });host.Client.DefaultRequestHeaders.Add("X-Test-Actor","accountant");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","FinanceRole");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();var current=f.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),"accountant",new(current.Scope,current.Roles.ToArray(),current.Permissions.ToArray()));
        var client=new Beep.OilandGas.Web.Services.AfeServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));var review=await client.ReviewInvestmentRoutingAsync(f.Routing.Afe.Process,f.Routing.Investment.AfeId);
        var original=await client.PrepareInvestmentHandoffAsync(f.Routing.Afe.Process,f.Routing.Investment.AfeId,new(review.Revision,review.Options.Single().ProcessId));
        var receipt=await client.StartInvestmentHandoffAsync(original);Assert.True(receipt.Recorded);Assert.Equal(receipt.InstanceId,(await client.ReadInvestmentHandoffAsync(f.Routing.Afe.Process,f.Routing.Investment.AfeId)).HandoffId);Assert.True((await client.RecoverInvestmentHandoffAsync(original)).Recorded);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync("/api/accounting/cost/afe/development/"+Uri.EscapeDataString(f.Routing.Investment.AfeId)+"/handoff?processId="+Uri.EscapeDataString(f.Routing.Afe.Process))).StatusCode);Assert.Equal(1,f.Routing.Afe.Native.Native.Writes);
    }
}
#endif
