#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentInvestmentStageApiTests
{
    [Fact]
    public async Task OrderedStagesAdvanceBeforeTheSingleFinalFinancialOutcomeAndCloseNativeTasks()
    {
        using var f=new DevelopmentInvestmentHandoffApiTests.Fixture();await f.Prepare(2);await f.Start();await f.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
        // The reviewer may have a different workspace in another company. Neither the
        // current-stage lookup nor next-stage recipient verification may borrow it.
        var repository = f.Routing.Afe.Db;
        repository.Add(new AppCompany { TenantId="tenant", BusinessAssociateId="other-company" });
        repository.Add(new AppPersona { Code="OTHER_COMPANY", Name="Other company workspace" });
        repository.Add(new AppUserPersona { UserId="investor", TenantId="tenant", BusinessAssociateId="other-company", PersonaCode="OTHER_COMPANY" });
        await repository.SaveChangesAsync();
        var service=f.Routing.Service();
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{
            services.AddScoped<CompanyDevelopmentAfeService>(provider=>f.Routing.Afe.Build(provider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));services.AddSingleton(service);services.AddSingleton<IDevelopmentInvestmentAssignedReader>(service);services.AddSingleton(Mock.Of<IFailureReporter>());
            services.AddSingleton<RepositoryDbContext>(f.Routing.Afe.Db);services.AddSingleton(f.Routing.Afe.Receiver.Tenant);services.AddSingleton<IRepositoryAccessService>(f.Routing.Afe.Receiver.Accounts);services.AddSingleton(f.Routing.Afe.Receiver.Workspaces);
            services.AddSingleton(f.Routing.Afe.Receiver.Resources.Object);services.AddSingleton<IProcessService>(f.Routing.Afe.Receiver.Processes);services.AddSingleton<ICrossPersonaTaskRouter>(f.Routing.Catalog.Workflow.Native.Router);
        });host.Client.DefaultRequestHeaders.Add("X-Test-Actor","investor");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","InvestmentReviewer");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();var selected=f.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),"investor",new(selected.Scope,selected.Roles.ToArray(),selected.Permissions.ToArray()));
        var client=new Beep.OilandGas.Web.Services.AfeServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));
        Assert.Null((await client.ReadOwnInvestmentStageOriginalAsync(f.Original.InstanceId())).Original);
        var review=await client.ReadAssignedInvestmentAsync(f.Original.InstanceId());Assert.Equal(1,review.StageNumber);Assert.Equal(2,review.StageCount);
        var first=new DevelopmentInvestmentStageDecision(review,Guid.NewGuid(),"APPROVE","Independent first configured investment approval");
        var initial=await client.DecideAssignedInvestmentStageAsync(first);Assert.Equal("RECORDED",initial.Status);Assert.Null(initial.Financial);Assert.NotNull(initial.NextStepInstanceId);Assert.Equal(1,f.Routing.Afe.Native.Native.Writes);
        var next=await client.ReadAssignedInvestmentAsync(f.Original.InstanceId());Assert.Equal(2,next.StageNumber);Assert.NotEqual(review.TaskId,next.TaskId);Assert.Null(next.Source.Recorded);
        var final=new DevelopmentInvestmentStageDecision(next,Guid.NewGuid(),"APPROVE","Independent final configured investment approval");
        var result=await client.DecideAssignedInvestmentStageAsync(final);Assert.Equal("RECORDED",result.Status);Assert.Equal("REVIEW_APPROVED",result.WorkflowState);Assert.NotNull(result.Financial);Assert.Equal(2,f.Routing.Afe.Native.Native.Writes);
        Assert.All(f.Routing.Catalog.Workflow.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(task=>task.PROCESS_INSTANCE_ID==f.Original.InstanceId()),task=>Assert.Equal("COMPLETED",task.TASK_STATUS));
        Assert.Equal("RECORDED",(await service.DecideAssignedStageAsync(f.Routing.Afe.Receiver.User,final,true)).Status);Assert.Equal(2,f.Routing.Afe.Native.Native.Writes);
        Assert.Equal(final.Revision(),(await client.ReadOwnInvestmentStageOriginalAsync(f.Original.InstanceId())).Original!.Revision());
        var inbox=new Beep.OilandGas.Web.Services.UnifiedTaskInboxService(new(host.Client,Mock.Of<IFailureReporter>(),company));Assert.Equal(0,(await inbox.GetInboxCountsAsync("FINANCE")).TotalPending);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(System.Net.HttpStatusCode.Unauthorized,(await host.Client.GetAsync("/api/accounting/cost/afe/development/assigned/"+Uri.EscapeDataString(f.Original.InstanceId())+"/original-decision")).StatusCode);
    }
    [Fact]
    public async Task ReservedOriginalCanBeRestoredWithReadOnlyRightsAndTamperedNativeEvidenceIsNeverReturned()
    {
        using var f=new DevelopmentInvestmentHandoffApiTests.Fixture();await f.Prepare();await f.Start();await f.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
        var service=f.Routing.Service();var review=await service.ReadAssignedAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId());var original=new DevelopmentInvestmentStageDecision(review,Guid.NewGuid(),"APPROVE","Original reserved investment rationale");
        // Seed a native reservation to exercise the read-only lost-browser recovery boundary independently of finance.
        await f.Routing.Catalog.Workflow.Service.ReserveInvestmentStageAsync(original,()=>Task.CompletedTask,()=>Task.FromResult(review.Source));var native=f.Routing.Catalog.Workflow.Native;var writes=native.Writes;
        var db=f.Routing.Afe.Db;db.RemoveRange(await db.RoleClaims.Where(row=>row.RoleId=="investment-role" && (row.ClaimValue=="Workflow.Approve" || row.ClaimValue=="Accounting.ReviewDevelopmentBudget")).ToArrayAsync());
        (await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="investor")).AccessLevel="READ";await db.SaveChangesAsync();await f.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
        var recovered=await f.Routing.Service().ReadOwnStageOriginalAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId());Assert.Equal(original.Revision(),recovered.Original!.Revision());
        Assert.Equal("RESERVED",(await f.Routing.Service().DecideAssignedStageAsync(f.Routing.Afe.Receiver.User,recovered.Original,true)).Status);Assert.Equal(writes,native.Writes);Assert.Equal(1,f.Routing.Afe.Native.Native.Writes);
        native.Rows.OfType<CROSS_PERSONA_TASK>().Single(task=>task.PROCESS_INSTANCE_ID==f.Original.InstanceId()).TASK_STATUS="COMPLETED";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Routing.Service().ReadOwnStageOriginalAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId()));Assert.Equal(writes,native.Writes);
    }
    [Fact]
    public async Task FinalFinancialAndNativeLostAcknowledgementRecoverWithViewOnlyWithoutNewWrites()
    {
        using var f=new DevelopmentInvestmentHandoffApiTests.Fixture();await f.Prepare();await f.Start();await f.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
        var service=f.Routing.Service();var review=await service.ReadAssignedAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId());var original=new DevelopmentInvestmentStageDecision(review,Guid.NewGuid(),"REJECT","Independent recorded rejection of original funding basis");
        var native=f.Routing.Catalog.Workflow.Native;
        native.AfterCommit=()=>{if(native.Rows.OfType<PROCESS_APPROVAL>().Any(row=>row.PROCESS_INSTANCE_ID==f.Original.InstanceId()))native.FailCommit=true;};
        await Assert.ThrowsAnyAsync<Exception>(()=>service.DecideAssignedStageAsync(f.Routing.Afe.Receiver.User,original));native.AfterCommit=null;native.FailCommit=false;
        Assert.Equal(2,f.Routing.Afe.Native.Native.Writes);var writes=native.Writes;var db=f.Routing.Afe.Db;
        db.RemoveRange(await db.RoleClaims.Where(row=>row.RoleId=="investment-role" && (row.ClaimValue=="Workflow.Approve" || row.ClaimValue=="Accounting.ReviewDevelopmentBudget")).ToArrayAsync());
        (await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="investor")).AccessLevel="READ";await db.SaveChangesAsync();await f.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
        var recovered=await f.Routing.Service().DecideAssignedStageAsync(f.Routing.Afe.Receiver.User,original,true);Assert.Equal("RECORDED",recovered.Status);Assert.Equal("REVIEW_REJECTED",recovered.WorkflowState);Assert.NotNull(recovered.Financial);
        var restored=await f.Routing.Service().ReadOwnStageOriginalAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId());Assert.Equal(original.Revision(),restored.Original!.Revision());
        Assert.Equal(writes,native.Writes);Assert.Equal(2,f.Routing.Afe.Native.Native.Writes);
        await f.Routing.Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Routing.Service().ReadOwnStageOriginalAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId()));
    }
}
#endif
