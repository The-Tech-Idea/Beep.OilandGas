#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Web.Services;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed partial class DrillingExecutionStartApiTests
{
    private static async Task ExerciseOrderedCompletionDecisions(DrillingProgramApiTests.Fixture f,DrillingServiceClient receiver,HttpClient http,CompanyClientContext company,DrillingCompletionReviewStart original,DrillingCompletionAssignedReview assigned,bool physicalAcceptance=false)
    {
        var native=f.Funding.Routing.Catalog.Workflow.Native;var db=f.Funding.Routing.Afe.Db;var before=native.Writes;
        var command=new DrillingCompletionReviewDecision(assigned,Guid.NewGuid(),"APPROVE","Independent review of reported failure and outstanding work; accepted physical tests and reporting closure remain separate");
        native.AfterCommit=()=>{if(native.Rows.OfType<PROCESS_APPROVAL>().Any(row=>row.APPROVAL_TYPE=="COMPLETION_REVIEW"))native.FailCommit=true;};
        await Assert.ThrowsAnyAsync<Exception>(()=>MeasureCompletion("decision-lost-ack",()=>receiver.DecideCompletionReviewAsync(command)));native.AfterCommit=null;native.FailCommit=false;
        Assert.Single(native.Rows.OfType<PROCESS_APPROVAL>(),row=>row.APPROVAL_TYPE=="COMPLETION_REVIEW");Assert.Equal(before+8,native.Writes);
        var removed=await db.RoleClaims.Where(row=>row.RoleId=="completion-role" && (row.ClaimValue=="Drilling.Approve" || row.ClaimValue=="Workflow.Approve")).ToArrayAsync();db.RemoveRange(removed);
        db.Add(new AppPersona{Code="COMPLETION_READ_ONLY",Name="Unmapped completion decision recovery reader"});var profile=await db.Set<AppUserPersona>().SingleAsync(row=>row.UserId=="completion-inspector");profile.PersonaCode="COMPLETION_READ_ONLY";await db.SaveChangesAsync();
        async Task Select(){await f.Select("completion-inspector","ConfiguredCompletionReviewer");var selected=f.Funding.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),"completion-inspector",new(selected.Scope,selected.Roles.ToArray(),selected.Permissions.ToArray()));}
        await Select();var receipt=await MeasureCompletion("decision-readonly-recovery",()=>receiver.RecoverCompletionReviewDecisionAsync(command));receipt.Require(command);Assert.True(receipt.Recorded);Assert.Equal(before+8,native.Writes);
        Assert.Equal(HttpStatusCode.Forbidden,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/decision",command)).StatusCode);
        profile.PersonaCode="COMPLETION_INSPECTOR";foreach(var claim in removed){claim.Id=0;db.RoleClaims.Add(claim);}await db.SaveChangesAsync();await Select();
        var next=await receiver.GetAssignedCompletionReviewAsync(original.InstanceId());Assert.Equal(2,next.StageNumber);Assert.NotEqual(assigned.TaskId,next.TaskId);
        var pending=await http.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox?personaCode=COMPLETION_INSPECTOR");var task=Assert.Single(pending!.CriticalTasks.Concat(pending.HighPriorityTasks).Concat(pending.NormalTasks));Assert.Equal(next.TaskId,task.TaskId);Assert.Equal(1,pending.Counts.Approvals);
        var history=await receiver.GetCompletionReviewProgressAsync(original.InstanceId());Assert.Equal("IN_REVIEW",history.State);Assert.Equal("APPROVED",history.Stages[0].State);Assert.Equal("PENDING",history.Stages[1].State);Assert.Equal(1,history.PendingTasks);
        if(physicalAcceptance)await ExercisePhysicalCompletionAcceptance(f,receiver,http,company,next);
        else{var reject=new DrillingCompletionReviewDecision(next,Guid.NewGuid(),"REJECT","The original reported failed test and outstanding discrepancy require corrective work before acceptance");Assert.True((await receiver.DecideCompletionReviewAsync(reject)).Recorded);}
        var writes=native.Writes;Assert.True((await receiver.RecoverCompletionReviewDecisionAsync(command)).Recorded);Assert.Equal(writes,native.Writes);
        history=await receiver.GetCompletionReviewProgressAsync(original.InstanceId());Assert.Equal(physicalAcceptance?"APPROVED":"REJECTED",history.State);Assert.Equal(0,history.PendingTasks);Assert.Equal(physicalAcceptance?"APPROVED":"REJECTED",history.Stages[1].State);Assert.Equal(physicalAcceptance,history.OriginalCompletionAccepted);
        Assert.Equal(physicalAcceptance?"REPORTED_PASS":"REPORTED_FAIL",history.Original.Source.ReportedCompletion!.Report.Tests.Single().ReportedResult);Assert.Equal("STARTED",history.Source.History.Execution.Recorded!.STATUS);
        var empty=await http.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox?personaCode=COMPLETION_INSPECTOR");Assert.Equal(0,empty!.Counts.TotalPending);
        Assert.Equal(HttpStatusCode.Conflict,(await http.GetAsync("/api/drilling/programs/execution/completion/assigned?processId="+Uri.EscapeDataString(original.InstanceId()))).StatusCode);
    }
}
#endif
