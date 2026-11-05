#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed partial class DrillingExecutionStartApiTests
{
    private static async Task ExercisePhysicalCompletionAcceptance(DrillingProgramApiTests.Fixture f,DrillingServiceClient receiver,HttpClient http,CompanyClientContext company,DrillingCompletionAssignedReview review)
    {
        var row=Assert.Single(f.Core.Rows.OfType<WELL_TEST>());var selected=new DrillingCompletionTestSelection(review,0,row.SOURCE,row.TEST_TYPE,row.RUN_NUM,row.TEST_NUM);
        var inspected=await MeasureCompletion("test-before-physical-acceptance",()=>receiver.InspectCompletionTestAsync(selected));
        var accepted=new DrillingCompletionAcceptedTest(0,inspected.Native,inspected.Revision,inspected.CheckedAtUtc,"Independent approved test acceptance specification / 42","Signed certificate verifying the reported test UTC / 42","","Independent certificate/result meets the referenced criteria; no unresolved work",true,true);
        var command=new DrillingCompletionReviewDecision(review,Guid.NewGuid(),"APPROVE","Independent physical completion accepted against the current claim and every source-bound test")
            {Acceptance=new(review.Revision,DateTimeOffset.UtcNow,[accepted],true)};command.Require();
        var native=f.Funding.Routing.Catalog.Workflow.Native;var writes=native.Writes;
        native.AfterCommit=()=>{if(native.Rows.OfType<PROCESS_APPROVAL>().Any(value=>value.APPROVAL_TYPE=="COMPLETION_ACCEPTANCE"))native.FailCommit=true;};
        await Assert.ThrowsAnyAsync<Exception>(()=>MeasureCompletion("physical-acceptance-lost-ack",()=>receiver.DecideCompletionReviewAsync(command)));native.AfterCommit=null;native.FailCommit=false;
        var audit=Assert.Single(native.Rows.OfType<PROCESS_APPROVAL>(),value=>value.APPROVAL_TYPE=="COMPLETION_ACCEPTANCE");Assert.Equal(command.AuditId("approval"),audit.PROCESS_APPROVAL_ID);Assert.Equal(writes+6,native.Writes);
        var db=f.Funding.Routing.Afe.Db;var removed=await db.RoleClaims.Where(value=>value.RoleId=="completion-role" && (value.ClaimValue=="Drilling.Approve" || value.ClaimValue=="Workflow.Approve")).ToArrayAsync();db.RemoveRange(removed);
        var profile=await db.Set<AppUserPersona>().SingleAsync(value=>value.UserId=="completion-inspector");profile.PersonaCode="COMPLETION_READ_ONLY";await db.SaveChangesAsync();
        async Task Select(){await f.Select("completion-inspector","ConfiguredCompletionReviewer");var current=f.Funding.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),"completion-inspector",new(current.Scope,current.Roles.ToArray(),current.Permissions.ToArray()));}
        await Select();f.AcceptedPhysicalTest=CompanyInventorySourceReview.Copy(row);Assert.True(f.Core.Rows.Remove(row));var receipt=await MeasureCompletion("physical-acceptance-readonly-recovery",()=>receiver.RecoverCompletionReviewDecisionAsync(command));receipt.Require(command);Assert.True(receipt.CompletionAccepted);Assert.Equal(writes+6,native.Writes);
        Assert.Equal(HttpStatusCode.Forbidden,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/decision",command)).StatusCode);
        var history=await receiver.GetCompletionReviewProgressAsync(review.InstanceId);Assert.True(history.OriginalCompletionAccepted);Assert.True(history.AcceptedClaimIsCurrent);Assert.Equal(command.Revision(),history.Stages.Last().AcceptedDecision!.Revision());Assert.Empty(f.Core.Rows.OfType<WELL_TEST>());
        profile.PersonaCode="COMPLETION_INSPECTOR";foreach(var claim in removed){claim.Id=0;db.RoleClaims.Add(claim);}await db.SaveChangesAsync();await Select();
    }
}
#endif
