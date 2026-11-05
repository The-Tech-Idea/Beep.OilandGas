using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
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
    private static async Task<DrillingCompletionReviewStart> SubmitObservedCompletion(DrillingProgramApiTests.Fixture f,DrillingServiceClient client,HttpClient http,DrillingCompletionReviewReadiness routing)
    {
        var original=new DrillingCompletionReviewStart(routing.Source,Assert.Single(routing.Options),Assert.Single(routing.SenderRoles),routing.SenderPersonaCode!,routing.SenderAuthorityRevision!,Guid.NewGuid(),"Inspect actual reported completion and referenced tests independently before any acceptance");
        var native=f.Funding.Routing.Catalog.Workflow.Native;var writes=native.Writes;
        native.AfterCommit=()=>{if(native.Rows.OfType<PROCESS_INSTANCE>().Any(row=>row.ENTITY_TYPE=="DRILLING_DAILY_REPORT"))native.FailCommit=true;};
        await Assert.ThrowsAnyAsync<Exception>(()=>MeasureCompletion("submit-lost-ack",()=>client.SubmitCompletionReviewAsync(original)));native.AfterCommit=null;native.FailCommit=false;
        var process=Assert.Single(native.Rows.OfType<PROCESS_INSTANCE>(),row=>row.ENTITY_TYPE=="DRILLING_DAILY_REPORT");
        Assert.Equal(original.InstanceId(),process.PROCESS_INSTANCE_ID);Assert.Equal(writes+5,native.Writes);
        Assert.DoesNotContain(native.Rows.OfType<PROCESS_APPROVAL>(),row=>row.PROCESS_INSTANCE_ID==original.InstanceId());
        Assert.Equal(HttpStatusCode.Conflict,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/review",original with{IntentId=Guid.NewGuid()})).StatusCode);
        Assert.Equal(writes+5,native.Writes);return original;
    }
    private static async Task RecoverObservedCompletionAndOpenInbox(DrillingProgramApiTests.Fixture f,DrillingServiceClient client,HttpClient http,Action<string> selection,DrillingCompletionReviewStart original,bool decisionEvidence=false,bool acceptanceEvidence=false)
    {
        var lifecycle=f.Funding.Routing.Catalog.Workflow;var native=lifecycle.Native;var before=JsonSerializer.Serialize(native.Rows);var writes=native.Writes;
        var receipt=await MeasureCompletion("readonly-recovery",()=>client.RecoverCompletionReviewAsync(original));receipt.Require(original);Assert.True(receipt.Recorded);
        Assert.Equal(HttpStatusCode.Forbidden,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/review",original)).StatusCode);
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{
            services.AddScoped<CompanyDrillingProgramService>(p=>f.Build(p.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));
            services.AddScoped<IDrillingProgramAssignedReviewReader>(p=>p.GetRequiredService<CompanyDrillingProgramService>());services.AddSingleton(f.Funding.Routing.Afe.Db);
            services.AddScoped<RepositoryTenantContext>(_=>f.Funding.Routing.Afe.Receiver.Tenant);services.AddScoped<IRepositoryAccessService>(_=>f.Funding.Routing.Afe.Receiver.Accounts);
            services.AddScoped<RepositoryPersonaService>(_=>f.Funding.Routing.Afe.Receiver.Workspaces);services.AddScoped<IAccessControlService>(_=>f.Funding.Routing.Afe.Receiver.Resources.Object);
            services.AddScoped<IProcessService>(_=>f.Funding.Routing.Afe.Receiver.Processes);services.AddScoped<ICrossPersonaTaskRouter>(_=>native.Router);services.AddSingleton(Mock.Of<IFailureReporter>());
        });
        await f.Select("completion-inspector","ConfiguredCompletionReviewer");
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","completion-inspector");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredCompletionReviewer");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();var selected=f.Funding.Routing.Afe.Receiver.Tenant.Company!;
        company.TrySelect(company.Clear(),"completion-inspector",new(selected.Scope,selected.Roles.ToArray(),selected.Permissions.ToArray()));
        var receiver=new DrillingServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));
        var assigned=await MeasureCompletion("assigned-recipient",()=>receiver.GetAssignedCompletionReviewAsync(original.InstanceId()));
        Assert.Equal("completion-inspector",assigned.Source.History.Execution.Basis.Scope().ActorUserId);Assert.Equal("site-recorder",assigned.Original.Source.ReportedCompletion!.RecorderUserId);
        Assert.True(assigned.IsCurrentClaim);Assert.Equal(acceptanceEvidence?"REPORTED_PASS":"REPORTED_FAIL",assigned.Original.Source.ReportedCompletion.Report.Tests.Single().ReportedResult);
        var inbox=await MeasureCompletion("qualified-inbox",()=>host.Client.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox?personaCode=COMPLETION_INSPECTOR"));
        var task=Assert.Single(inbox!.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks));
        Assert.Equal(assigned.TaskId,task.TaskId);Assert.Equal("APPROVAL",task.TaskType);Assert.True(ProcessDefinitionRevision.IsValid(task.CompletionReviewRevision));
        Assert.Equal(WorkflowTaskRoute.For("FIELD",original.InstanceId(),"DRILLING_DAILY_REPORT"),task.Route);Assert.Equal(1,inbox.Counts.TotalPending);Assert.Equal(1,inbox.Counts.Approvals);
        var grant=await f.Funding.Routing.Afe.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.Id=="completion-inspection-field");grant.IsActive=false;await f.Funding.Routing.Afe.Db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync("/api/drilling/programs/execution/completion/assigned?processId="+Uri.EscapeDataString(original.InstanceId()))).StatusCode);
        grant.IsActive=true;await f.Funding.Routing.Afe.Db.SaveChangesAsync();
        if(decisionEvidence){await ExerciseCompletionTestInspection(f,receiver,host.Client,assigned);await ExerciseOrderedCompletionDecisions(f,receiver,host.Client,company,original,assigned,acceptanceEvidence);}
        await f.Select("site-recorder","ConfiguredExecutionRole");selection("site-recorder");
        if(!decisionEvidence){Assert.Equal(writes,native.Writes);Assert.Equal(before,JsonSerializer.Serialize(native.Rows));}
        Assert.All(native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>row.PROCESS_INSTANCE_ID==original.InstanceId()),row=>Assert.Equal(decisionEvidence?"COMPLETED":"PENDING",row.TASK_STATUS));
        if(decisionEvidence){var progress=await client.GetCompletionReviewProgressAsync(original.InstanceId());Assert.Equal(acceptanceEvidence?"APPROVED":"REJECTED",progress.State);Assert.Equal(acceptanceEvidence,progress.OriginalCompletionAccepted);Assert.Equal("site-recorder",progress.Source.History.Execution.Basis.Scope().ActorUserId);}
    }
    private static async Task<T> MeasureCompletion<T>(string label,Func<Task<T>> action)
    {
        var watch=System.Diagnostics.Stopwatch.StartNew();
        try{return await action();}finally{Console.WriteLine($"Completion review {label}: {watch.Elapsed.TotalSeconds:F3} seconds");}
    }
}
#endif
