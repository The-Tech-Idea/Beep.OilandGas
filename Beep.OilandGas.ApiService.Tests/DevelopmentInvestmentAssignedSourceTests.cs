using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentInvestmentAssignedSourceTests
{
    [Fact]
    public async Task OriginalIndependentIndividualReadsExactSourceAndCurrentRightsCannotBeBorrowed()
    {
        using var f=new DevelopmentInvestmentHandoffApiTests.Fixture();await f.Prepare();await f.Start();
        await f.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
        var db=f.Routing.Afe.Db;
        db.Add(new AppCompany{TenantId="tenant",BusinessAssociateId="other-company"});
        db.Add(new AppPersona{Code="OTHER_COMPANY",Name="Other company workspace"});
        db.Add(new AppUserPersona{UserId="investor",TenantId="tenant",BusinessAssociateId="other-company",PersonaCode="OTHER_COMPANY"});
        await db.SaveChangesAsync();
        var service=f.Routing.Service();var source=await service.ReadAssignedAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId());
        source.Require(f.Original.Scope().Company,"investor",f.Original.InstanceId());Assert.Equal(f.Original.Review.Source.Afe(),source.Source.Afe());
        Assert.Equal(DevelopmentAfeCommand.FromRecord(f.Routing.Afe.Native.Native.Rows.OfType<Beep.OilandGas.Models.Data.ProductionAccounting.AFE>().Single()).Rationale,source.FundingRationale);
        Assert.Equal("FINANCE",source.PersonaCode);Assert.Equal("InvestmentReviewer",source.AssignedRole);Assert.Equal("accountant",source.Sender);
        var writes=f.Routing.Catalog.Workflow.Native.Writes;
        var grant=await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="investor");grant.AccessLevel="READ";await db.SaveChangesAsync();
        var refused=await Assert.ThrowsAnyAsync<Exception>(()=>service.ReadAssignedAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId()));Assert.IsNotType<MockException>(refused);
        grant.AccessLevel="WRITE";var profile=await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId=="investor" && row.TenantId=="tenant" && row.BusinessAssociateId=="company");db.Add(new AppPersona{Code="NO_WORKSPACE",Name="Other company workspace"});profile.PersonaCode="NO_WORKSPACE";await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.ReadAssignedAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId()));
        profile.PersonaCode="FINANCE";var role=await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="investor");role.ApprovalStatus="Revoked";await db.SaveChangesAsync();
        refused=await Assert.ThrowsAnyAsync<Exception>(()=>service.ReadAssignedAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId()));Assert.IsNotType<MockException>(refused);
        Assert.Equal(writes,f.Routing.Catalog.Workflow.Native.Writes);Assert.Equal(1,f.Routing.Afe.Native.Native.Writes);
    }
    [Fact]
    public async Task ActualPersonaInboxClosesAssignedSourceAndNeverPublishesCountsAfterLateRevocation()
    {
        using var f=new DevelopmentInvestmentHandoffApiTests.Fixture();await f.Prepare();await f.Start();await f.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
        var service=f.Routing.Service();var receiver=f.Routing.Afe.Receiver;var writes=f.Routing.Catalog.Workflow.Native.Writes;
        var result=await receiver.Inbox(investments:service).Inbox("FINANCE",CancellationToken.None);
        var inbox=Assert.IsType<UnifiedInbox>(Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result).Value);
        var row=Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks));Assert.Equal(1,inbox.Counts.TotalPending);
        Assert.Equal(f.Original.InstanceId(),row.WorkflowName);Assert.Equal(f.Original.Review.Source.AfeId,row.EntityId);Assert.True(ProcessDefinitionRevision.IsValid(row.InvestmentReviewRevision));
        Assert.Equal("/economics/afe?investmentHandoff="+Uri.EscapeDataString(row.WorkflowName),row.Route);
        var grant=await f.Routing.Afe.Db.Set<AppUserAssetAccess>().SingleAsync(item=>item.UserId=="investor");var reads=0;var changing=new Mock<IDevelopmentInvestmentAssignedReader>(MockBehavior.Strict);
        changing.Setup(value=>value.ReadAssignedAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(),f.Original.InstanceId())).Returns(async(System.Security.Claims.ClaimsPrincipal user,string instance)=>{
            var current=await service.ReadAssignedAsync(user,instance);if(++reads==1){grant.AccessLevel="READ";await f.Routing.Afe.Db.SaveChangesAsync();}return current;
        });
        Assert.IsType<Microsoft.AspNetCore.Mvc.ForbidResult>(await receiver.Inbox(investments:changing.Object).Inbox("FINANCE",CancellationToken.None));
        result=await receiver.Inbox(investments:service).Inbox("FINANCE",CancellationToken.None);
        Assert.Equal(0,Assert.IsType<UnifiedInbox>(Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result).Value).Counts.TotalPending);
        Assert.Equal(writes,f.Routing.Catalog.Workflow.Native.Writes);Assert.Equal(1,f.Routing.Afe.Native.Native.Writes);
    }
    [Fact]
    public async Task ActualTypedHttpOpensAssignedSourceAndDoesNotAdmitSenderOrAnonymous()
    {
        using var f=new DevelopmentInvestmentHandoffApiTests.Fixture();await f.Prepare();await f.Start();await f.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{
            services.AddScoped<CompanyDevelopmentAfeService>(provider=>f.Routing.Afe.Build(provider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));
            services.AddSingleton<IDevelopmentInvestmentAssignedReader>(f.Routing.Service());services.AddSingleton(Mock.Of<IFailureReporter>());
            services.AddSingleton<RepositoryDbContext>(f.Routing.Afe.Db);services.AddSingleton(f.Routing.Afe.Receiver.Tenant);
            services.AddSingleton<IRepositoryAccessService>(f.Routing.Afe.Receiver.Accounts);services.AddSingleton(f.Routing.Afe.Receiver.Workspaces);
            services.AddSingleton(f.Routing.Afe.Receiver.Resources.Object);services.AddSingleton<IProcessService>(f.Routing.Afe.Receiver.Processes);
            services.AddSingleton<ICrossPersonaTaskRouter>(f.Routing.Catalog.Workflow.Native.Router);
        });host.Client.DefaultRequestHeaders.Add("X-Test-Actor","investor");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","InvestmentReviewer");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();var current=f.Routing.Afe.Receiver.Tenant.Company!;
        company.TrySelect(company.Clear(),"investor",new(current.Scope,current.Roles.ToArray(),current.Permissions.ToArray()));
        var client=new Beep.OilandGas.Web.Services.AfeServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));
        var source=await client.ReadAssignedInvestmentAsync(f.Original.InstanceId());Assert.Equal(f.Routing.Investment.AfeId,source.Source.AfeId);Assert.Equal("investor",source.Source.Current.Source.Review.Scope.ActorUserId);
        var inboxClient=new Beep.OilandGas.Web.Services.UnifiedTaskInboxService(new(host.Client,Mock.Of<IFailureReporter>(),company));
        var inbox=await inboxClient.GetInboxAsync("FINANCE");var assigned=Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks));
        Assert.Equal(source.Revision,assigned.InvestmentReviewRevision);Assert.Equal(source.Source.AfeId,assigned.EntityId);
        Assert.Equal("/economics/afe?investmentHandoff="+Uri.EscapeDataString(source.InstanceId),assigned.Route);
        Assert.Equal(1,(await inboxClient.GetInboxCountsAsync("FINANCE")).TotalPending);
        var filtered=await inboxClient.GetFilteredTasksAsync(new InboxFilter{WorkflowName=source.InstanceId,PageSize=1},"FINANCE");Assert.Equal(assigned.TaskId,Assert.Single(filtered).TaskId);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync("/api/accounting/cost/afe/development/assigned/"+Uri.EscapeDataString(f.Original.InstanceId()))).StatusCode);
        await f.Routing.Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Routing.Service().ReadAssignedAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId()));
    }
}
#endif
