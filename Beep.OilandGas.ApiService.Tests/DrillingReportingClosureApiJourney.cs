using Beep.OilandGas.Models.Data.Accounting;
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
using Beep.OilandGas.PPDM39.Models;
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
    private static async Task ExerciseReportingClosure(DrillingProgramApiTests.Fixture f,string assignmentId)
    {
        var native=f.Funding.Routing.Catalog.Workflow.Native;var db=f.Funding.Routing.Afe.Db;
        var completion=Assert.Single(native.Rows.OfType<PROCESS_INSTANCE>(),row=>row.ENTITY_TYPE=="DRILLING_DAILY_REPORT");
        var assignment=DrillingExecutionAssignment.FromPayload(native.Rows.OfType<PROCESS_INSTANCE>().Single(row=>row.PROCESS_INSTANCE_ID==assignmentId).INSTANCE_DATA_JSON!,assignmentId,"field",f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>().Single().DRILLING_OPERATION_ID);
        var acceptedBefore=JsonSerializer.Serialize(native.Rows.Where(row=>row.GetType().GetProperty("PROCESS_INSTANCE_ID")?.GetValue(row)?.ToString()==completion.PROCESS_INSTANCE_ID));
        var operationBefore=JsonSerializer.Serialize(f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>());var wellBefore=JsonSerializer.Serialize(f.Well);var finance=f.Funding.Routing.Afe.Native.Native.Writes;
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>
        {
            services.AddScoped<CompanyDrillingProgramService>(p=>f.Build(p.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));
            services.AddScoped<IDrillingProgramAssignedReviewReader>(p=>p.GetRequiredService<CompanyDrillingProgramService>());services.AddSingleton(db);
            services.AddScoped<RepositoryTenantContext>(_=>f.Funding.Routing.Afe.Receiver.Tenant);services.AddScoped<IRepositoryAccessService>(_=>f.Funding.Routing.Afe.Receiver.Accounts);
            services.AddScoped<RepositoryPersonaService>(_=>f.Funding.Routing.Afe.Receiver.Workspaces);services.AddScoped<IAccessControlService>(_=>f.Funding.Routing.Afe.Receiver.Resources.Object);
            services.AddScoped<IProcessService>(_=>f.Funding.Routing.Afe.Receiver.Processes);services.AddScoped<ICrossPersonaTaskRouter>(_=>native.Router);services.AddSingleton(Mock.Of<IFailureReporter>());
        });
        using var company=new CompanyClientContext();
        async Task Select(string actor,string role)
        {
            await f.Select(actor,role);host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");
            host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role",role);
            var current=f.Funding.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),actor,new(current.Scope,current.Roles.ToArray(),current.Permissions.ToArray()));
        }
        var client=new DrillingServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));
        var path="/api/drilling/programs/execution/reporting/closure/review?assignmentId="+Uri.EscapeDataString(assignmentId)+"&completionId="+Uri.EscapeDataString(completion.PROCESS_INSTANCE_ID);
        await Select("site-recorder","ConfiguredExecutionRole");
        var pending=await MeasureCompletion("reporting-status-readonly",()=>client.GetReportingResponsibilityAsync(assignmentId));Assert.Null(pending.Closure);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(path)).StatusCode);
        foreach(var permission in new[]{"Drilling.Edit","Drilling.ManageWellProgram"})db.RoleClaims.Add(new(){RoleId="execution-role",ClaimType="permission",ClaimValue=permission});
        var profile=await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId=="site-recorder");profile.PersonaCode="DRILLING_SITE";
        var grant=await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="site-recorder");grant.AccessLevel="WRITE";await db.SaveChangesAsync();f.Well.ACTIVE_IND="Y";
        Assert.NotNull(f.AcceptedPhysicalTest);f.Core.Rows.Add(CompanyInventorySourceReview.Copy(f.AcceptedPhysicalTest!));await Select("site-recorder","ConfiguredExecutionRole");
        grant.IsActive=false;await db.SaveChangesAsync();Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(path)).StatusCode);grant.IsActive=true;await db.SaveChangesAsync();await Select("site-recorder","ConfiguredExecutionRole");
        var review=await MeasureCompletion("reporting-closure-review",()=>client.ReviewReportingClosureAsync(assignmentId,completion.PROCESS_INSTANCE_ID));review.Require();Assert.Equal("site-recorder",review.Scope().ActorUserId);
        var command=new DrillingReportingClosure(review,Guid.NewGuid(),DateTimeOffset.UtcNow,"Accepted physical completion verified; close the original ongoing reporting responsibility",true);command.Require();
        var test=Assert.Single(f.Core.Rows.OfType<WELL_TEST>());var remark=test.REMARK;test.REMARK="Changed after closure readiness";var before=JsonSerializer.Serialize(native.Rows);
        Assert.Equal(HttpStatusCode.Conflict,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/reporting/closure",command)).StatusCode);Assert.Equal(before,JsonSerializer.Serialize(native.Rows));test.REMARK=remark;
        var writes=native.Writes;
        var committed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var lostAcknowledgementArmed=false;
        native.AfterCommit=()=>{if(native.Rows.OfType<PROCESS_HISTORY>().Any(row=>row.EVENT_TYPE=="REPORTING_CLOSED")){native.FailCommit=true;lostAcknowledgementArmed=true;committed.TrySetResult();}};
        try
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>MeasureCompletion("reporting-closure-lost-ack",()=>client.CloseReportingAsync(command)));
            // A client timeout does not end its server-side transaction. Keep the fault armed
            // and await the actual commit before issuing another request against this source.
            await committed.Task.WaitAsync(TimeSpan.FromMinutes(3));Assert.True(lostAcknowledgementArmed);
        }
        finally{native.AfterCommit=null;native.FailCommit=false;}
        Assert.Single(native.Rows.OfType<PROCESS_HISTORY>(),row=>row.EVENT_TYPE=="REPORTING_CLOSED");Assert.Equal(writes+4,native.Writes);
        var acknowledged=await MeasureCompletion("reporting-closure-normal-original-retry",()=>client.CloseReportingAsync(command));acknowledged.Require(command);Assert.True(acknowledged.Recorded);Assert.Equal(writes+4,native.Writes);
        db.RemoveRange(await db.RoleClaims.Where(row=>row.RoleId=="execution-role" && (row.ClaimValue=="Drilling.Edit" || row.ClaimValue=="Drilling.ManageWellProgram" || row.ClaimValue=="Workflow.Start")).ToArrayAsync());
        profile.PersonaCode="CORRECTION_READ_ONLY";grant.AccessLevel="READ";await db.SaveChangesAsync();f.Core.Rows.RemoveAll(row=>row is WELL_TEST);f.Well.ACTIVE_IND="N";await Select("site-recorder","ConfiguredExecutionRole");
        var recovered=await MeasureCompletion("reporting-closure-readonly-recovery",()=>client.RecoverReportingClosureAsync(command));recovered.Require(command);Assert.True(recovered.Recorded);Assert.Equal(writes+4,native.Writes);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/reporting/closure",command)).StatusCode);
        Assert.True((await client.RecoverExecutionAssignmentAsync(assignment)).Recorded);
        var closed=await client.GetReportingResponsibilityAsync(assignmentId);Assert.Equal(command.Revision(),closed.Closure!.Revision());Assert.Equal("STARTED",closed.Execution.Recorded!.STATUS);
        await Select("execution-recipient","ConfiguredReportingRole");var recipient=await client.GetReportingResponsibilityAsync(assignmentId);Assert.Equal("execution-recipient",recipient.Scope().ActorUserId);Assert.NotNull(recipient.Closure);
        var inbox=await host.Client.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox?personaCode=DRILLING_REPORTER");Assert.Equal(0,inbox!.Counts.TotalPending);Assert.Equal(0,inbox.Counts.DataEntry);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/reporting/closure/recover",command)).StatusCode);
        Assert.Equal(acceptedBefore,JsonSerializer.Serialize(native.Rows.Where(row=>row.GetType().GetProperty("PROCESS_INSTANCE_ID")?.GetValue(row)?.ToString()==completion.PROCESS_INSTANCE_ID)));
        Assert.Equal(operationBefore,JsonSerializer.Serialize(f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>()));Assert.Equal(wellBefore,JsonSerializer.Serialize(f.Well));Assert.Equal(finance,f.Funding.Routing.Afe.Native.Native.Writes);Assert.Equal(writes+4,native.Writes);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync("/api/drilling/programs/execution/reporting/status?assignmentId="+Uri.EscapeDataString(assignmentId))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.PostAsJsonAsync("/api/drilling/programs/execution/reporting/closure/recover",command)).StatusCode);
    }
}
#endif
