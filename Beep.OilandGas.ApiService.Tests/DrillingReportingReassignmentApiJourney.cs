using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed partial class DrillingExecutionStartApiTests
{
    private static async Task ExerciseReportingTransfer(DrillingProgramApiTests.Fixture f,string assignmentId)
    {
        var native=f.Funding.Routing.Catalog.Workflow.Native;var db=f.Funding.Routing.Afe.Db;
        var executionBefore=JsonSerializer.Serialize(f.Store.Store.Native.Rows);var wellBefore=JsonSerializer.Serialize(f.Well);var finance=f.Funding.Routing.Afe.Native.Native.Writes;
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>
        {
            services.AddScoped<CompanyDrillingProgramService>(p=>f.Build(p.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));services.AddScoped<IDrillingProgramAssignedReviewReader>(p=>p.GetRequiredService<CompanyDrillingProgramService>());services.AddSingleton(db);
            services.AddScoped<RepositoryTenantContext>(_=>f.Funding.Routing.Afe.Receiver.Tenant);services.AddScoped<IRepositoryAccessService>(_=>f.Funding.Routing.Afe.Receiver.Accounts);services.AddScoped<RepositoryPersonaService>(_=>f.Funding.Routing.Afe.Receiver.Workspaces);
            services.AddScoped<IAccessControlService>(_=>f.Funding.Routing.Afe.Receiver.Resources.Object);services.AddScoped<IProcessService>(_=>f.Funding.Routing.Afe.Receiver.Processes);services.AddScoped<ICrossPersonaTaskRouter>(_=>native.Router);services.AddSingleton(Mock.Of<IFailureReporter>());
        });
        using var company=new CompanyClientContext();
        async Task Select(string actor,string role)
        {
            await f.Select(actor,role);host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role",role);
            var current=f.Funding.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),actor,new(current.Scope,current.Roles.ToArray(),current.Permissions.ToArray()));
        }
        var client=new DrillingServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));var catalogPath="/api/drilling/programs/execution/reporting/transfer/catalog?assignmentId="+Uri.EscapeDataString(assignmentId);
        await Select("execution-recipient","ConfiguredReportingRole");Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(catalogPath)).StatusCode);
        foreach(var permission in new[]{"Drilling.Edit","Workflow.Start"})if(!await db.RoleClaims.AnyAsync(row=>row.RoleId=="reporting-role" && row.ClaimValue==permission))db.RoleClaims.Add(new(){RoleId="reporting-role",ClaimType="permission",ClaimValue=permission});
        var grant=await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="execution-recipient");grant.AccessLevel="WRITE";await db.SaveChangesAsync();
        f.Funding.Routing.Afe.Receiver.Resources.Setup(r=>r.CheckAssetAccessAsync("execution-recipient","field","FIELD","WRITE",null)).ReturnsAsync(()=>{Assert.DoesNotContain("begin",native.Transactions.TakeLast(1));return new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true};});
        await Select("execution-recipient","ConfiguredReportingRole");var catalog=await client.GetReportingReassignmentCatalogAsync(assignmentId);Assert.Equal("site-recorder",catalog.Responsibility.ManagerUserId);Assert.Contains("ConfiguredReportingRole",catalog.ManagerRoles);
        var option=Assert.Single(catalog.Options,row=>row.ProcessId==catalog.Responsibility.Workflow.ProcessId);Assert.True(option.Ready);
        var selection=new DrillingReportingReassignmentSelection(assignmentId,catalog.Revision,option.ProcessId,"ConfiguredReportingRole");
        var role=await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="execution-recipient" && row.RoleId=="reporting-role");var revokedDuringReview=false;var beforeReview=native.Writes;
        native.AfterCommit=()=>{if(!revokedDuringReview){revokedDuringReview=true;role.ApprovalStatus="Revoked";db.SaveChanges();}};
        try
        {
            Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/reporting/transfer/review",selection)).StatusCode);
            Assert.True(revokedDuringReview);Assert.Equal(beforeReview,native.Writes);
        }
        finally{native.AfterCommit=null;role.ApprovalStatus="Approved";await db.SaveChangesAsync();await Select("execution-recipient","ConfiguredReportingRole");}
        catalog=await client.GetReportingReassignmentCatalogAsync(assignmentId);selection=selection with{CatalogRevision=catalog.Revision};
        var review=await client.ReviewReportingReassignmentAsync(selection);Assert.Equal("execution-recipient",review.Scope().ActorUserId);Assert.True(DrillingReportingReassignmentReview.SameDestination(review.Responsibility.Workflow,review.Destination));
        var command=new DrillingReportingReassignment(review,Guid.NewGuid(),DateTimeOffset.UtcNow,"Current company manager accepts ongoing reporting accountability for the unchanged crew",true);var writes=native.Writes;
        var committed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);native.AfterCommit=()=>{if(native.Rows.OfType<PROCESS_HISTORY>().Any(row=>row.EVENT_TYPE=="REPORTING_TRANSFERRED")){native.FailCommit=true;committed.TrySetResult();}};
        try{await Assert.ThrowsAnyAsync<Exception>(()=>client.ReassignReportingAsync(command));await committed.Task.WaitAsync(TimeSpan.FromMinutes(3));}
        finally{native.AfterCommit=null;native.FailCommit=false;}
        var persisted=native.Writes;Assert.True(persisted>writes);Assert.Single(native.Rows.OfType<PROCESS_HISTORY>(),row=>row.EVENT_TYPE=="REPORTING_TRANSFERRED");
        var acknowledged=await client.ReassignReportingAsync(command);Assert.Equal(2,acknowledged.DestinationGeneration);Assert.Equal(persisted,native.Writes);
        var status=await client.GetReportingResponsibilityAsync(assignmentId);Assert.Equal(2,status.Responsibility.Number);Assert.Equal("execution-recipient",status.Responsibility.ManagerUserId);Assert.Equal("site-recorder",status.Responsibility.Root.Scope().ActorUserId);
        var assigned=await client.GetAssignedExecutionAsync(assignmentId);Assert.Equal(command.StepInstanceId(),assigned.StepInstanceId);Assert.Equal(2,assigned.Responsibility.Number);
        var inbox=await host.Client.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox?personaCode=DRILLING_REPORTER");Assert.Equal(1,inbox!.Counts.TotalPending);Assert.Equal(assigned.TaskId,Assert.Single(inbox.NormalTasks).TaskId);
        db.RemoveRange(await db.RoleClaims.Where(row=>row.RoleId=="reporting-role" && (row.ClaimValue=="Drilling.Edit" || row.ClaimValue=="Workflow.Start")).ToArrayAsync());
        (await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId=="execution-recipient")).PersonaCode="DRILLING_UNMAPPED";grant.AccessLevel="READ";await db.SaveChangesAsync();await Select("execution-recipient","ConfiguredReportingRole");
        native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION definition && definition.PROCESS_DEFINITION_ID==command.Review.Destination.ProcessId);
        Assert.True((await client.RecoverReportingReassignmentAsync(command)).Recorded);Assert.Equal(persisted,native.Writes);Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/reporting/transfer",command)).StatusCode);
        await Select("site-recorder","ConfiguredExecutionRole");Assert.Equal(2,(await client.GetReportingResponsibilityAsync(assignmentId)).Responsibility.Number);Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/reporting/transfer/recover",command)).StatusCode);
        Assert.Equal(executionBefore,JsonSerializer.Serialize(f.Store.Store.Native.Rows));Assert.Equal(wellBefore,JsonSerializer.Serialize(f.Well));Assert.Equal(finance,f.Funding.Routing.Afe.Native.Native.Writes);Assert.Equal(persisted,native.Writes);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync(catalogPath)).StatusCode);
    }
}
#endif
