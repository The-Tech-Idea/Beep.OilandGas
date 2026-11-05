#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentInvestmentProgressApiTests
{
    [Fact]
    public async Task ReadOnlySenderFollowsActualOrderedReviewAndFinalRejectionThroughTheProtectedTypedSource()
    {
        using var f=new DevelopmentInvestmentHandoffApiTests.Fixture();await f.Prepare(2);await f.Start();var native=f.Routing.Catalog.Workflow.Native;
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{
            services.AddScoped<CompanyDevelopmentAfeService>(p=>f.Routing.Afe.Build(p.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));services.AddScoped<CompanyDevelopmentInvestmentRoutingService>(_=>f.Routing.Service());services.AddSingleton(Mock.Of<IFailureReporter>());
            services.AddSingleton<RepositoryDbContext>(f.Routing.Afe.Db);services.AddSingleton(f.Routing.Afe.Receiver.Tenant);services.AddSingleton<IRepositoryAccessService>(f.Routing.Afe.Receiver.Accounts);services.AddSingleton(f.Routing.Afe.Receiver.Workspaces);
            services.AddSingleton(f.Routing.Afe.Receiver.Resources.Object);services.AddSingleton<IProcessService>(f.Routing.Afe.Receiver.Processes);services.AddSingleton<ICrossPersonaTaskRouter>(f.Routing.Catalog.Workflow.Native.Router);

        });host.Client.DefaultRequestHeaders.Add("X-Test-Actor","accountant");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","FinanceRole");
        var db=f.Routing.Afe.Db;db.RemoveRange(await db.RoleClaims.Where(row=>row.RoleId=="finance-role" && (row.ClaimValue=="Workflow.Start" || row.ClaimValue==CompanyDevelopmentAfeService.PreparePermission)).ToArrayAsync());
        (await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="accountant")).AccessLevel="READ";await db.SaveChangesAsync();await f.Routing.Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();var selected=f.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),"accountant",new(selected.Scope,selected.Roles.ToArray(),selected.Permissions.ToArray()));var client=new Beep.OilandGas.Web.Services.AfeServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));
        var writes=native.Writes;var initial=await client.ReadInvestmentHandoffAsync(f.Routing.Afe.Process,f.Original.Review.Source.AfeId);Assert.Equal("IN_REVIEW",initial.Progress!.State);Assert.Equal(1,initial.Progress.PendingTasks);Assert.Equal(writes,native.Writes);
        await f.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");var service=f.Routing.Service();var assigned=await service.ReadAssignedAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId());var first=new DevelopmentInvestmentStageDecision(assigned,Guid.NewGuid(),"APPROVE","First independent investment stage is supported");await service.DecideAssignedStageAsync(f.Routing.Afe.Receiver.User,first);
        await f.Routing.Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");writes=native.Writes;var next=await client.ReadInvestmentHandoffAsync(f.Routing.Afe.Process,f.Original.Review.Source.AfeId);Assert.Equal("IN_REVIEW",next.Progress!.State);Assert.Equal(new[]{"APPROVED","PENDING"},next.Progress.Stages.Select(stage=>stage.Status));Assert.Equal("investor",next.Progress.Stages[0].Reviewer);Assert.Equal(first.Rationale,next.Progress.Stages[0].Rationale);Assert.Equal(1,next.Progress.PendingTasks);Assert.Null(next.Source.Recorded);Assert.Equal(writes,native.Writes);
        await f.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");service=f.Routing.Service();assigned=await service.ReadAssignedAsync(f.Routing.Afe.Receiver.User,f.Original.InstanceId());var final=new DevelopmentInvestmentStageDecision(assigned,Guid.NewGuid(),"REJECT","Independent final stage rejection retains the funding basis");await service.DecideAssignedStageAsync(f.Routing.Afe.Receiver.User,final);
        await f.Routing.Afe.Receiver.SelectReceiver("accountant",role:"FinanceRole");writes=native.Writes;var result=await client.ReadInvestmentHandoffAsync(f.Routing.Afe.Process,f.Original.Review.Source.AfeId);Assert.Equal("REJECTED",result.Progress!.State);Assert.Equal(0,result.Progress.PendingTasks);Assert.Equal(new[]{"APPROVED","REJECTED"},result.Progress.Stages.Select(stage=>stage.Status));Assert.Equal(final.Rationale,result.Progress.Stages[1].Rationale);Assert.Equal("REJECT",result.Source.Recorded!.Decision);Assert.Equal(writes,native.Writes);Assert.Equal(2,f.Routing.Afe.Native.Native.Writes);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync("/api/accounting/cost/afe/development/"+Uri.EscapeDataString(f.Original.Review.Source.AfeId)+"/handoff?processId="+Uri.EscapeDataString(f.Routing.Afe.Process))).StatusCode);
    }
    [Fact]
    public async Task RevokedReadFieldAndForeignCompanyCannotBorrowAnOriginalPrivateTimeline()
    {
        using var f=new DevelopmentInvestmentHandoffApiTests.Fixture();await f.Prepare();await f.Start();var native=f.Routing.Catalog.Workflow.Native;var writes=native.Writes;var db=f.Routing.Afe.Db;
        var grant=await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="accountant");grant.IsActive=false;await db.SaveChangesAsync();await Assert.ThrowsAnyAsync<Exception>(()=>f.Status());Assert.Equal(writes,native.Writes);
        grant.IsActive=true;db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="foreign",UserId="accountant",GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Add(new AppUserRoleExtension{UserId="accountant",RoleId="finance-role",TenantId="tenant",BusinessAssociateId="foreign",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});await db.SaveChangesAsync();await f.Routing.Afe.Receiver.SelectReceiver("accountant",new("tenant","foreign"),"FinanceRole");var core=await f.Routing.Afe.Receiver.Company.ResolveAsync();
        db.Add(new AppUserAssetAccess{Id="foreign-status-field",UserId="accountant",DatabaseScope=core.Fingerprint,OrganizationId="foreign",AssetType="FIELD",AssetId="field",AccessLevel="READ",IsActive=true});await db.SaveChangesAsync();
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Status());Assert.Equal(writes,native.Writes);Assert.Equal(1,f.Routing.Afe.Native.Native.Writes);
    }
}
#endif
