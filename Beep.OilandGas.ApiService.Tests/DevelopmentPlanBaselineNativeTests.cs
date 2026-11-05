using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentPlanBaselineNativeTests
{
    internal sealed class Fixture : IDisposable
    {
        internal DevelopmentReviewDecisionTests.Fixture Decision=new();
        internal DevelopmentPlanRelease Original=null!;
        internal DevelopmentPlanNativeDraftTests.Fixture Planning=>Decision.Receiver.Sender.Catalog.Draft.Drafts;
        internal async Task Prepare()
        {
            await Decision.Prepare();await Decision.Decide();var review=await Decision.Receiver.Service.ReadProgressAsync(Decision.Receiver.User,Decision.Command.Review.InstanceId);
            Original=new(review,Guid.NewGuid(),"Approve the reviewed FDP baseline. Financial release remains separate.");Original.Require();
            Planning.Native.Rows.Add(new R_DEVELOPMENT_PLANNING_REFERENCE_CODE{REFERENCE_SET="FDP_STATUS",REFERENCE_CODE="APPROVED",ACTIVE_IND="Y"});
        }
        internal Task<DevelopmentPlanReleaseReceipt> Approve(bool recover=false)=>Planning.Service.ApproveBaselineAsync(Original,Planning.Authority,recover);
        public void Dispose()=>Decision.Dispose();
    }
    [Fact] public async Task ActualCompletedReviewCreatesOneApprovedVersionAndPreservesExactDraftAndHistory()
    {
        using var f=new Fixture();await f.Prepare();var draft=JsonSerializer.Serialize(f.Original.Review.Plan);var native=f.Planning.Native;var workflowWrites=f.Decision.Mutations;
        var receipt=await f.Approve();receipt.Require(f.Original);var baseline=receipt.Result.Baseline!;
        Assert.Equal("APPROVED",baseline.FDP_STATUS);Assert.Equal(2,baseline.FDP_VERSION);Assert.Equal("reviewer",baseline.APPROVED_BY);Assert.Equal(f.Original.Review.Plan.FDP_ID,baseline.SOURCE_DRAFT_FDP_ID);
        Assert.Equal(draft,JsonSerializer.Serialize(native.Rows.OfType<FIELD_DEVELOPMENT_PLAN>().Single(row=>row.FDP_STATUS=="DRAFT")));
        Assert.Single((await f.Planning.Service.GetDevelopmentPlansAsync(f.Planning.Request.SelectionContext,f.Planning.Request.PlanningDatabase,f.Planning.Authority)).Plans);
        Assert.Equal(2,native.Writes);await f.Approve();await f.Approve(true);Assert.Equal(2,native.Writes);Assert.Equal(workflowWrites,f.Decision.Mutations);
        var current=f.Original.Review with{Scope=f.Original.Review.Scope with{ActorUserId="planner"}};current=current with{Revision=current.ContentRevision()};
        Assert.NotNull((await f.Planning.Service.GetBaselineAsync(current,f.Planning.Authority)).Baseline);
    }
    [Fact] public async Task OriginalRecoveryChecksAbsenceAndLostCommitWithoutCreatingAnotherVersion()
    {
        using var f=new Fixture();await f.Prepare();Assert.Null((await f.Approve(true)).Result.Baseline);Assert.Equal(1,f.Planning.Native.Writes);
        f.Planning.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Approve());f.Planning.Native.FailCommit=false;
        Assert.NotNull((await f.Approve(true)).Result.Baseline);Assert.Equal(2,f.Planning.Native.Writes);
        f.Original=f.Original with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Approve());await Assert.ThrowsAnyAsync<Exception>(()=>f.Approve(true));Assert.Equal(2,f.Planning.Native.Writes);
    }
    [Theory][InlineData("insert")][InlineData("readback")][InlineData("authority")][InlineData("source")]
    public async Task FailedBaselineBoundaryRollsBackWithoutChangingReviewedDraft(string fault)
    {
        using var f=new Fixture();await f.Prepare();var before=JsonSerializer.Serialize(f.Planning.Native.Rows);
        f.Planning.FailInsert=fault=="insert";f.Planning.CorruptInsert=fault=="readback";
        f.Planning.OnInsert=()=>{if(fault=="authority")f.Planning.Revoke=true;if(fault=="source")f.Planning.Native.Rows.OfType<FIELD_DEVELOPMENT_PLAN>().Single(row=>row.FDP_STATUS=="DRAFT").FDP_NAME="Changed after baseline insert";};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Approve());Assert.Equal(before,JsonSerializer.Serialize(f.Planning.Native.Rows));Assert.Equal("rollback",f.Planning.Native.Transactions.Last());
    }
    [Theory][InlineData("column")][InlineData("width")][InlineData("status")][InlineData("currency")][InlineData("source")]
    public async Task UnqualifiedSchemaReferenceOrSourceCannotApproveBaseline(string fault)
    {
        using var f=new Fixture();await f.Prepare();
        if(fault=="column")f.Planning.ChangeSchema=schema=>{if(schema.EntityName=="FIELD_DEVELOPMENT_PLAN")schema.Fields.RemoveAll(field=>field.FieldName=="RELEASE_COMMAND_JSON");};
        if(fault=="width")f.Planning.ChangeSchema=schema=>{if(schema.EntityName=="FIELD_DEVELOPMENT_PLAN"){var field=schema.Fields.Single(row=>row.FieldName=="FDP_STATUS");field.Size=5;field.Size1=5;}};
        if(fault=="status")f.Planning.Native.Rows.RemoveAll(row=>row is R_DEVELOPMENT_PLANNING_REFERENCE_CODE code && code.REFERENCE_CODE=="APPROVED");
        if(fault=="currency")f.Planning.Native.Rows.RemoveAll(row=>row is R_DEVELOPMENT_PLANNING_REFERENCE_CODE code && code.REFERENCE_SET=="COST_CURRENCY");
        if(fault=="source")f.Planning.Native.Rows.OfType<FIELD_DEVELOPMENT_PLAN>().Single().FDP_NAME="Changed reviewed source";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Approve());Assert.Equal(1,f.Planning.Native.Writes);
    }
    [Theory][InlineData("audit")][InlineData("lineage")][InlineData("duplicate")][InlineData("inactive")]
    public async Task ChangedRecordedBaselineCannotBeReportedAbsentOrRepaired(string fault)
    {
        using var f=new Fixture();await f.Prepare();await f.Approve();var row=f.Planning.Native.Rows.OfType<FIELD_DEVELOPMENT_PLAN>().Single(row=>row.FDP_STATUS=="APPROVED");
        if(fault=="audit")row.APPROVED_BY="another";if(fault=="lineage")row.SOURCE_DRAFT_FDP_ID="another";if(fault=="duplicate")f.Planning.Native.Rows.Add(row);if(fault=="inactive")row.ACTIVE_IND="N";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Approve(true));await Assert.ThrowsAnyAsync<Exception>(()=>f.Approve());Assert.Equal(2,f.Planning.Native.Writes);
    }
}
#endif
