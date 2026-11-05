using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentInvestmentHandoffNativeTests
{
    internal sealed class Fixture:IDisposable
    {
        internal Beep.OilandGas.Web.Auth.Tests.DevelopmentInvestmentStateTests.Fixture Source=new();
        internal CompanyProcessStartTests.Fixture Native=new("InvestmentReviewer");
        internal DevelopmentInvestmentHandoff Original=null!;internal DevelopmentInvestmentReview Current;
        internal bool Revoked;internal int SourceReads;
        internal Fixture(){Current=Source.Review;}
        internal async Task Prepare(int stages=1)
        {
            Native.Definition.PROCESS_TYPE="ACCOUNTING";Native.Definition.ENTITY_TYPE="AFE";
            Native.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=Enumerable.Range(1,stages).Select(index=>new ProcessStepDefinition{
                StepId="investment-"+index,StepName="Configured investment review "+index,SequenceNumber=index,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["InvestmentReviewer"]}).ToArray()});
            var definition=(await Native.Service.GetProcessDefinitionAsync("definition"))!;
            var recipients=definition.Steps.Select(stage=>new DevelopmentReviewRecipientStep(stage.StepId,"InvestmentReviewer",[new("independent-reviewer","FINANCE")],new string('A',64))).ToArray();
            var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition));
            var option=new DevelopmentInvestmentRoutingOption(definition.ProcessId,definition.ProcessName,definition.Version,revision,recipients,[]);
            var routing=new DevelopmentInvestmentRoutingReview(Current,[option],"",DateTimeOffset.UtcNow);routing=routing with{Revision=routing.ContentRevision()};
            var workflow=new DevelopmentReviewWorkflowChoice(definition.ProcessId,definition.ProcessName,definition.Version,revision,
                definition.Steps.OrderBy(stage=>stage.SequenceNumber).Select(stage=>new DevelopmentReviewRoleStep(stage.StepId,stage.StepName,stage.SequenceNumber,"InvestmentReviewer",["FINANCE"],stage.SlaHours)).ToArray());
            Original=new(routing,workflow,Guid.NewGuid());Original.Require();
        }
        internal Task Guard(){if(Revoked)throw new UnauthorizedAccessException("Current sender authority changed.");return Task.CompletedTask;}
        internal Task<DevelopmentInvestmentReview> ReadSource(){SourceReads++;return Task.FromResult(Current);}
        internal Task<DevelopmentInvestmentHandoffReceipt> Start(bool recover=false)=>recover?Native.Service.RecoverInvestmentHandoffAsync(Original,Guard,ReadSource):Native.Service.StartInvestmentHandoffAsync(Original,Guard,ReadSource);
        public void Dispose()=>Source.Dispose();
    }
    [Theory][InlineData(1)][InlineData(2)]
    public async Task OriginalSourceWorkflowHistoryStepsAndTaskSetCommitOnceAndExactRetryAddsNothing(int stages)
    {
        using var f=new Fixture();await f.Prepare(stages);var result=await f.Start();result.Require(f.Original);Assert.True(result.Recorded);Assert.Single(result.TaskIds);Assert.Equal(stages+3,f.Native.Native.Writes);
        var process=Assert.Single(f.Native.Native.Rows.OfType<PROCESS_INSTANCE>());Assert.Equal("AFE",process.ENTITY_TYPE);Assert.Equal(f.Original.Review.Source.AfeId,process.ENTITY_ID);
        Assert.Equal(f.Original.Revision(),DevelopmentInvestmentHandoff.FromPayload(process.INSTANCE_DATA_JSON!,result.InstanceId,f.Original.Scope().FieldId,process.ENTITY_ID!).Revision());
        var task=Assert.Single(f.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>());Assert.Equal("FINANCE",task.TARGET_PERSONA_CODE);Assert.Equal("InvestmentReviewer",task.ASSIGNED_ROLE);Assert.Contains("investmentHandoff=",task.ROUTE);
        Assert.True((await f.Start()).Recorded);Assert.True((await f.Start(true)).Recorded);Assert.Equal(stages+3,f.Native.Native.Writes);Assert.Empty(f.Native.Native.Rows.OfType<PROCESS_APPROVAL>());
    }
    [Fact]public async Task AbsentRecoveryNeverCreatesAndPartialEvidenceNeverBecomesConfirmedAbsence()
    {
        using var f=new Fixture();await f.Prepare();Assert.False((await f.Start(true)).Recorded);Assert.Equal(0,f.Native.Native.Writes);
        f.Native.Native.Rows.Add(new PROCESS_STEP_INSTANCE{PROCESS_STEP_INSTANCE_ID="orphan",PROCESS_INSTANCE_ID=f.Original.InstanceId()});
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(true));await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(0,f.Native.Native.Writes);
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)]
    public async Task EveryProcessStepHistoryOrTaskInsertFailureRollsBackTheWholeAssignment(int write)
    {
        using var f=new Fixture();await f.Prepare();f.Native.FailWrite=write;await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Single(f.Native.Native.Rows);Assert.Equal("rollback",f.Native.Native.Transactions.Last());
    }
    [Fact]public async Task UnknownCommitRecoversOriginalWithoutNewWritesAndChangedIntentCannotCreateAnotherAssignment()
    {
        using var f=new Fixture();await f.Prepare();f.Native.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());f.Native.Native.FailCommit=false;
        Assert.True((await f.Start(true)).Recorded);Assert.Equal(4,f.Native.Native.Writes);f.Original=f.Original with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(4,f.Native.Native.Writes);
    }
    [Fact]public async Task OriginalReadOnlyRecoveryDoesNotDependOnTheCurrentDefinitionOrRepairMissingTasks()
    {
        using var f=new Fixture();await f.Prepare();await f.Start();f.Native.Native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);Assert.True((await f.Start(true)).Recorded);Assert.Equal(4,f.Native.Native.Writes);
        f.Native.Native.Rows.RemoveAll(row=>row is CROSS_PERSONA_TASK);await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(true));Assert.Equal(4,f.Native.Native.Writes);
    }
    [Theory][InlineData("step")][InlineData("history")][InlineData("approval")][InlineData("tasks")][InlineData("payload")]
    public async Task ChangedOrExtraOriginalGraphEvidenceCannotBeAdoptedOrRepaired(string fault)
    {
        using var f=new Fixture();await f.Prepare();await f.Start();
        if(fault=="step")f.Native.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single().STATUS="COMPLETED";
        if(fault=="history")f.Native.Native.Rows.OfType<PROCESS_HISTORY>().Single().USER_ID="another";
        if(fault=="approval")f.Native.Native.Rows.Add(new PROCESS_APPROVAL{PROCESS_APPROVAL_ID="unowned",PROCESS_INSTANCE_ID=f.Original.InstanceId()});
        if(fault=="tasks")f.Native.Native.Rows.Add(new CROSS_PERSONA_TASK{CROSS_TASK_ID="unowned",PROCESS_INSTANCE_ID=f.Original.InstanceId(),PROCESS_STEP_INSTANCE_ID="another"});
        if(fault=="payload")f.Native.Native.Rows.OfType<PROCESS_INSTANCE>().Single().INSTANCE_DATA_JSON="{}";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(true));Assert.Equal(4,f.Native.Native.Writes);
    }
    [Fact]public async Task NativeGenericMutatorsCannotSkipApproveCancelOrRewriteThisOwnedAssignment()
    {
        using var f=new Fixture();await f.Prepare();await f.Start();var id=f.Original.InstanceId();var step=f.Original.Workflow.Steps[0].StepId;var stepId=f.Native.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single().PROCESS_STEP_INSTANCE_ID!;
        foreach(var action in new Func<Task>[] {
            ()=>f.Native.Service.CancelProcessAsync(id,"Unqualified cancellation","investor"),()=>f.Native.Service.SkipStepAsync(id,step,"Skip","investor"),
            ()=>f.Native.Service.RollbackStepAsync(id,step,"Rollback","investor"),()=>f.Native.Service.TransitionStateAsync(id,"SUSPENDED","investor"),
            ()=>f.Native.Service.ExecuteStepAsync(id,step,new(),"investor"),()=>f.Native.Service.CompleteStepAsync(id,step,"APPROVED","investor"),
            ()=>f.Native.Service.AddHistoryEntryAsync(id,new(){HistoryId="unqualified",InstanceId=id,PerformedBy="investor"}),
            ()=>f.Native.Service.RequestApprovalAsync(stepId,"INVESTMENT","investor","investor")})
            await Assert.ThrowsAnyAsync<Exception>(action);
        Assert.Equal(4,f.Native.Native.Writes);Assert.True((await f.Start(true)).Recorded);
    }
    [Fact]public async Task RevocationAndLateFinancialSourceChangesNeverReturnAConfirmedAssignment()
    {
        using var f=new Fixture();await f.Prepare();f.Revoked=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(0,f.Native.Native.Writes);f.Revoked=false;
        f.Native.Native.AfterCommit=()=>{
            var basis=f.Current.Current with{Afes=[f.Current.Afe() with{Name="Changed source after commit"}]};basis=basis with{Revision=basis.ContentRevision()};f.Current=f.Current with{Current=basis};
        };
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(4,f.Native.Native.Writes);Assert.Single(f.Native.Native.Rows.OfType<PROCESS_INSTANCE>());
    }
    [Theory][InlineData("name")][InlineData("version")]
    public async Task CallerCannotRelabelTheConfiguredWorkflowWhileKeepingItsDefinitionHash(string change)
    {
        using var f=new Fixture();await f.Prepare();var option=f.Original.Option();var workflow=f.Original.Workflow;
        if(change=="name"){option=option with{Name="Caller supplied workflow"};workflow=workflow with{ProcessName=option.Name};}
        else{option=option with{Version="caller-version"};workflow=workflow with{Version=option.Version};}
        var review=f.Original.Review with{Options=[option]};review=review with{Revision=review.ContentRevision()};f.Original=f.Original with{Review=review,Workflow=workflow};f.Original.Require();
        var failure=await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Start());Assert.Contains("workflow definition changed",failure.Message);Assert.Equal(0,f.Native.Native.Writes);
    }
    [Theory][InlineData("type")][InlineData("prefix")]
    public async Task OrphanedSourceTaskCannotBecomeAbsentStatusOrPermitASecondIntent(string marker)
    {
        using var f=new Fixture();await f.Prepare();await f.Start();f.Native.Native.Rows.RemoveAll(row=>row is PROCESS_INSTANCE);
        var task=f.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single();if(marker=="type")task.PROCESS_INSTANCE_ID="missing-original-parent";else task.ENTITY_TYPE="WELL";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Native.Service.ReadRecordedInvestmentHandoffAsync(f.Original.Scope().Company,f.Original.Scope().FieldId,f.Original.Review.Source.AfeId,f.Guard));
        f.Original=f.Original with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(4,f.Native.Native.Writes);
    }
    [Fact]public async Task PublicReadbackFindsOneOriginalAssignmentAndReadOnlyAbsenceWithoutAnotherWrite()
    {
        using var f=new Fixture();await f.Prepare();var scope=f.Original.Scope();var afe=f.Original.Review.Source.AfeId;
        Assert.Null(await f.Native.Service.ReadRecordedInvestmentHandoffAsync(scope.Company,scope.FieldId,afe,f.Guard));await f.Start();
        var status=await f.Native.Service.ReadRecordedInvestmentHandoffAsync(scope.Company,scope.FieldId,afe,f.Guard);Assert.Equal(f.Original.Revision(),status!.Value.Original.Revision());Assert.Single(status.Value.Tasks);Assert.Equal(4,f.Native.Native.Writes);
        Assert.Equal(f.Original.InstanceId(),status.Value.Process.InstanceId);status.Value.Process.StepInstances.Clear();
        Assert.Single((await f.Native.Service.ReadRecordedInvestmentHandoffAsync(scope.Company,scope.FieldId,afe,f.Guard))!.Value.Process.StepInstances);Assert.Equal(4,f.Native.Native.Writes);
    }
    [Theory][InlineData("type")][InlineData("field")]
    public async Task RetaggedOriginalCannotBecomeConfirmedAbsenceOrAuthorizeAnotherAssignment(string fault)
    {
        using var f=new Fixture();await f.Prepare();await f.Start();var row=f.Native.Native.Rows.OfType<PROCESS_INSTANCE>().Single();if(fault=="type")row.ENTITY_TYPE="WELL";else row.FIELD_ID="another";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Native.Service.ReadRecordedInvestmentHandoffAsync(f.Original.Scope().Company,f.Original.Scope().FieldId,f.Original.Review.Source.AfeId,f.Guard));
        f.Original=f.Original with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(4,f.Native.Native.Writes);
    }
}
#endif
