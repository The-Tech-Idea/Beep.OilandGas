#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingReportingReassignmentNativeTests
{
    private sealed class Fixture
    {
        internal DrillingReportingReassignmentTests.Fixture Ready = new();
        internal DrillingReportingReassignment Command = null!;
        internal int Mutations, FailAt;
        internal bool WriteRevoked, ReadRevoked;
        internal Action<int>? OnMutation;
        internal PersonaTaskRoutingTests.Fixture Store => Ready.Original.Lifecycle.Native;
        internal PPDMProcessService Service => Ready.Original.Lifecycle.Service;
        internal async Task Prepare()
        {
            await Ready.Prepare(); Command = await Create(); Ready.ExecutionReads = Ready.DestinationReads = 0;
            ErrorsInfo Write(string table, object value, bool insert)
            {
                ++Mutations; ++Store.Writes; if (Mutations == FailAt) return new() { Flag = Errors.Failed };
                var copy = JsonSerializer.Deserialize(JsonSerializer.Serialize(value), value.GetType())!;
                if (insert) Store.Rows.Add(copy);
                else
                {
                    var key = table switch { "PROCESS_INSTANCE" => "PROCESS_INSTANCE_ID", "PROCESS_STEP_INSTANCE" => "PROCESS_STEP_INSTANCE_ID", "CROSS_PERSONA_TASK" => "CROSS_TASK_ID", _ => throw new InvalidOperationException(table) };
                    var property = value.GetType().GetProperty(key)!; var id = property.GetValue(value);
                    var index = Store.Rows.FindIndex(row => row.GetType() == value.GetType() && Equals(property.GetValue(row), id));
                    if (index < 0) throw new InvalidOperationException("Missing native transfer row"); Store.Rows[index] = copy;
                }
                OnMutation?.Invoke(Mutations); return new() { Flag = Errors.Ok };
            }
            Store.Source.Setup(source => source.InsertEntity(It.IsAny<string>(), It.IsAny<object>())).Returns((string table, object value) => Write(table, value, true));
            Store.Source.Setup(source => source.UpdateEntity(It.IsAny<string>(), It.IsAny<object>())).Returns((string table, object value) => Write(table, value, false));
        }
        internal async Task<DrillingReportingReassignment> Create()
        {
            var review = await Ready.Review(); var result = new DrillingReportingReassignment(review, Guid.NewGuid(), DateTimeOffset.UtcNow, "Transfer current reporting accountability to the configured receiving team", true);
            result.Require(); return result;
        }
        internal Task Guard(bool recover)
        {
            if (ReadRevoked || !recover && WriteRevoked) throw new UnauthorizedAccessException("Current transfer authority revoked");
            if (!recover && JsonSerializer.Serialize(Ready.Destination) != JsonSerializer.Serialize(Command.Review.Destination))
                throw new UnauthorizedAccessException("Current receiving authority changed");
            return Ready.Guard();
        }
        internal Task<DrillingReportingReassignmentReceipt> Transfer(bool recover = false) =>
            Service.RecordReportingReassignmentAsync(Command, () => Guard(recover), Ready.Execution, Ready.CurrentDestination, recover);
    }

    [Fact]
    public async Task ExactTransferClosesOnlyFormerGenerationAndRoutesOnlyNewTasksWithOriginalAssignmentRecovery()
    {
        var f = new Fixture(); await f.Prepare(); var source = JsonSerializer.Serialize(f.Ready.Original.Current);
        f.Store.Rows.Add(new CROSS_PERSONA_TASK { CROSS_TASK_ID = "unrelated", PROCESS_INSTANCE_ID = "another-process", ENTITY_ID = "another-operation", TASK_STATUS = "PENDING", TARGET_PERSONA_CODE = "OTHER" });
        var other = JsonSerializer.Serialize(f.Store.Rows.OfType<CROSS_PERSONA_TASK>().Single(row => row.CROSS_TASK_ID == "unrelated"));
        var receipt = await f.Transfer(); receipt.Require(f.Command); Assert.True(receipt.Recorded); Assert.Equal(2, receipt.DestinationGeneration); Assert.Equal(7, f.Mutations);
        var graph = await f.Ready.Original.Recorded(); Assert.Equal(ProcessStatus.IN_PROGRESS, graph.Process.Status); Assert.Null(graph.Process.CompletionDate); Assert.Null(graph.Process.CompletedBy);
        Assert.Equal("REPORTING_REASSIGNED", graph.Process.CurrentState); Assert.Equal(2, graph.Process.StepInstances.Count);
        Assert.Equal("REPORTING_TRANSFERRED", graph.Process.StepInstances.Single(row => row.StepInstanceId == f.Command.Review.Responsibility.StepInstanceId).Outcome);
        Assert.All(graph.Tasks.Where(row => f.Command.Review.Responsibility.TaskIds().Contains(row.CROSS_TASK_ID)), row => Assert.Equal("COMPLETED", row.TASK_STATUS));
        var pending = Assert.Single(graph.Tasks, row => row.TASK_STATUS == "PENDING"); Assert.Equal("ReplacementReportingRole", pending.ASSIGNED_ROLE); Assert.Equal(f.Command.StepInstanceId(), pending.PROCESS_STEP_INSTANCE_ID);
        Assert.Equal(f.Command.Revision(), Assert.Single(PPDMProcessService.RecordedReportingReassignments(graph.Process)).Revision());
        var writes = f.Mutations; Assert.True((await f.Transfer()).Recorded); Assert.True((await f.Transfer(true)).Recorded);
        var initial = await f.Ready.Original.Start(true); initial.Require(f.Ready.Original.Original); Assert.Equal(2, initial.TaskIds.Count); Assert.Equal(writes, f.Mutations);
        Assert.Equal(source, JsonSerializer.Serialize(f.Ready.Original.Current)); Assert.Equal(other, JsonSerializer.Serialize(f.Store.Rows.OfType<CROSS_PERSONA_TASK>().Single(row => row.CROSS_TASK_ID == "unrelated")));
        Assert.DoesNotContain(f.Store.Rows.OfType<PROCESS_HISTORY>(), row => row.EVENT_TYPE == "REPORTING_CLOSED"); Assert.Empty(f.Store.Rows.OfType<PROCESS_APPROVAL>());
    }

    [Fact]
    public async Task LaterGenerationCanReturnToFormerWorkflowAndEarlierTransferStillRecoversReadOnly()
    {
        var f = new Fixture(); await f.Prepare(); var first = f.Command; await f.Transfer();
        f.Ready.Destination = f.Ready.Original.Original.Workflow; f.Command = await f.Create(); Assert.Equal(2, f.Command.Review.Responsibility.Number); Assert.Equal(first.Revision(), f.Command.Review.Responsibility.Revision);
        var second = await f.Transfer(); Assert.Equal(3, second.DestinationGeneration); var graph = await f.Ready.Original.Recorded();
        Assert.Equal(3, graph.Process.StepInstances.Count); Assert.Equal(2, graph.Tasks.Count(row => row.TASK_STATUS == "PENDING")); Assert.Equal(3, graph.Tasks.Count(row => row.TASK_STATUS == "COMPLETED"));
        Assert.All(graph.Tasks.Where(row => row.TASK_STATUS == "PENDING"), row => Assert.Equal("ConfiguredReportingRole", row.ASSIGNED_ROLE));
        var writes = f.Mutations; f.Command = first; f.WriteRevoked = true; f.Store.Rows.RemoveAll(row => row is PROCESS_DEFINITION);
        f.Ready.OnExecution = _ => throw new InvalidOperationException("Recovery must not open live execution"); f.Ready.OnDestination = _ => throw new InvalidOperationException("Recovery must not open live destination");
        var recovered = await f.Transfer(true); recovered.Require(first); Assert.Equal(2, recovered.DestinationGeneration); Assert.Equal(writes, f.Mutations);
        Assert.True((await f.Ready.Original.Start(true)).Recorded); Assert.Equal(writes, f.Mutations);
    }

    [Fact]
    public async Task CurrentAdmittedManagerMayDifferFromInitialSenderAndKeepsBothAccountsAudit()
    {
        var f = new Fixture(); await f.Prepare(); var original = f.Ready.Original.Original; var source = f.Ready.Original.Current;
        var basis = source.Basis with { Source = DrillingProgramAssignedReviewTests.AsReader(source.Basis.Source, "replacement-manager"), RecorderRoles = [], PersonaCode = null, RecorderAuthorityRevision = null };
        basis = basis with { Revision = basis.ContentRevision() }; source = source with { Basis = basis }; source = source with { Revision = source.ContentRevision() };
        f.Ready.Original.Current = source; f.Ready.ManagerRole="ConfiguredReplacementAssigner"; f.Command = await f.Create(); Assert.Equal("replacement-manager", f.Command.Scope().ActorUserId);
        Assert.Equal("ConfiguredReplacementAssigner",f.Command.Review.ManagerRole); Assert.NotEqual(original.SenderRole,f.Command.Review.ManagerRole);
        Assert.True((await f.Transfer()).Recorded); var graph = await f.Ready.Original.Recorded(); Assert.Equal(original.Scope().ActorUserId, graph.Process.StartedBy);
        Assert.All(graph.Tasks.Where(row => row.TASK_STATUS == "COMPLETED"), row => { Assert.Equal(original.Scope().ActorUserId, row.ROW_CREATED_BY); Assert.Equal("replacement-manager", row.ROW_CHANGED_BY); });
        Assert.Equal("replacement-manager", Assert.Single(graph.Tasks, row => row.TASK_STATUS == "PENDING").ROW_CREATED_BY);
        Assert.Equal("replacement-manager", f.Store.Rows.OfType<PROCESS_HISTORY>().Single(row => row.EVENT_TYPE == "REPORTING_TRANSFERRED").USER_ID);
        f.WriteRevoked = true; Assert.True((await f.Transfer(true)).Recorded);
        f.Ready.Original.Current = CompanyInventorySourceReview.Copy(original.Source); Assert.True((await f.Ready.Original.Start(true)).Recorded);
    }

    [Fact]
    public async Task ChangedManagementRoleCanTakeOverUnchangedCrewWithoutInventingAnotherHandoff()
    {
        var f=new Fixture();await f.Prepare();f.Ready.Destination=f.Ready.Original.Original.Workflow;f.Ready.ManagerRole="ReplacementManagementRole";
        f.Command=await f.Create();Assert.True((await f.Transfer()).Recorded);
        var graph=await f.Ready.Original.Recorded();var current=PPDMProcessService.CurrentReportingGeneration(graph.Process);
        Assert.Equal(2,current.Number);Assert.Equal("ReplacementManagementRole",current.ManagerRole);
        Assert.Equal(JsonSerializer.Serialize(f.Ready.Original.Original.Workflow),JsonSerializer.Serialize(current.Workflow));
        Assert.Equal(2,graph.Tasks.Count(row=>row.TASK_STATUS=="PENDING"));Assert.Equal(2,graph.Tasks.Count(row=>row.TASK_STATUS=="COMPLETED"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Ready.Review());
    }

    [Fact]
    public async Task LostAcknowledgementAndConfirmedAbsenceAreReadOnlyAndNeedNoLiveDefinitionOrSource()
    {
        var f = new Fixture(); await f.Prepare(); Assert.False((await f.Transfer(true)).Recorded); Assert.Equal(0, f.Mutations); Assert.Equal(0, f.Ready.ExecutionReads); Assert.Equal(0, f.Ready.DestinationReads);
        f.Store.AfterCommit = () => { if (f.Store.Rows.OfType<PROCESS_HISTORY>().Any(row => row.EVENT_TYPE == "REPORTING_TRANSFERRED")) f.Store.FailCommit = true; };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Transfer()); f.Store.AfterCommit = null; f.Store.FailCommit = false;
        Assert.Equal(7, f.Mutations); Assert.Single(f.Store.Rows.OfType<PROCESS_HISTORY>(), row => row.EVENT_TYPE == "REPORTING_TRANSFERRED");
        f.WriteRevoked = true; f.Store.Rows.RemoveAll(row => row is PROCESS_DEFINITION); var reads = f.Ready.ExecutionReads;
        f.Ready.OnExecution = _ => throw new InvalidOperationException("No live execution for recovery"); f.Ready.OnDestination = _ => throw new InvalidOperationException("No live destination for recovery");
        Assert.True((await f.Transfer(true)).Recorded); Assert.Equal(7, f.Mutations); Assert.Equal(reads, f.Ready.ExecutionReads);
        f.Command = f.Command with { IntentId = Guid.NewGuid() }; await Assert.ThrowsAnyAsync<Exception>(() => f.Transfer(true)); Assert.Equal(7, f.Mutations);
    }

    [Theory, InlineData(1), InlineData(2), InlineData(3), InlineData(4), InlineData(5), InlineData(6), InlineData(7)]
    public async Task EveryFormerTaskStageProcessNewTaskOrHistoryFailureRollsBackTheWholeTransfer(int failure)
    {
        var f = new Fixture(); await f.Prepare(); var before = JsonSerializer.Serialize(f.Store.Rows); f.FailAt = failure;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Transfer()); Assert.Equal(failure,f.Mutations); Assert.Equal(before, JsonSerializer.Serialize(f.Store.Rows)); Assert.Equal("rollback", f.Store.Transactions.Last());
    }

    [Theory, InlineData("old-task"), InlineData("new-task"), InlineData("old-step"), InlineData("new-audit"), InlineData("history"), InlineData("proof"), InlineData("process-audit"), InlineData("unlisted-proof"), InlineData("duplicate-proof")]
    public async Task ChangedCompleteTransferCannotBeReadRecoveredOrSilentlyRepaired(string fault)
    {
        var f = new Fixture(); await f.Prepare(); await f.Transfer(); var writes = f.Mutations;
        var process = f.Store.Rows.OfType<PROCESS_INSTANCE>().Single();
        if (fault == "old-task") f.Store.Rows.OfType<CROSS_PERSONA_TASK>().First(row => row.TASK_STATUS == "COMPLETED").TASK_STATUS = "PENDING";
        if (fault == "new-task") f.Store.Rows.OfType<CROSS_PERSONA_TASK>().Single(row => row.TASK_STATUS == "PENDING").TASK_CONTEXT_JSON = "Changed new task";
        if (fault == "old-step") f.Store.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row => row.PROCESS_STEP_INSTANCE_ID == f.Command.Review.Responsibility.StepInstanceId).OUTCOME = "PHYSICALLY_COMPLETED";
        if (fault == "new-audit") f.Store.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row => row.PROCESS_STEP_INSTANCE_ID == f.Command.StepInstanceId()).ROW_CREATED_BY = "other";
        if (fault == "history") f.Store.Rows.OfType<PROCESS_HISTORY>().Single(row => row.EVENT_TYPE == "REPORTING_TRANSFERRED").EVENT_DATA_JSON = "{}";
        if (fault == "process-audit") process.ROW_CREATED_BY = "other";
        if (fault == "proof") { var data = JsonNode.Parse(process.INSTANCE_DATA_JSON!)!; data["Data"]![DrillingReportingReassignment.PayloadKey]![0]!["Command"]!["Reason"] = "Changed transfer reason"; process.INSTANCE_DATA_JSON = data.ToJsonString(); }
        if (fault == "unlisted-proof") { var data = JsonNode.Parse(process.INSTANCE_DATA_JSON!)!; data["Data"]![DrillingReportingReassignment.PayloadKey]![0]!["UnlistedTransferData"] = true; process.INSTANCE_DATA_JSON = data.ToJsonString(); }
        if (fault == "duplicate-proof") { var at=process.INSTANCE_DATA_JSON!.IndexOf("\"Before\":",StringComparison.Ordinal);Assert.True(at>=0);process.INSTANCE_DATA_JSON=process.INSTANCE_DATA_JSON.Insert(at,"\"Before\":null,"); }
        await Assert.ThrowsAnyAsync<Exception>(() => f.Ready.Original.Recorded()); await Assert.ThrowsAnyAsync<Exception>(() => f.Transfer(true)); Assert.Equal(writes, f.Mutations);
    }

    [Fact]
    public async Task SnapshotNormalizationCannotHideAnEnvelopeChangeAfterTheProcessWasLoaded()
    {
        var f=new Fixture();await f.Prepare();await f.Transfer();var process=f.Store.Rows.OfType<PROCESS_INSTANCE>().Single();var reads=0;var changed=false;var writes=f.Mutations;
        f.Store.Source.Setup(source=>source.GetEntityAsync("PROCESS_INSTANCE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string _,List<TheTechIdea.Beep.Report.AppFilter> filters)=>
        {
            var found=f.Store.Rows.OfType<PROCESS_INSTANCE>().Where(row=>filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(CompanyInventorySourceReview.Copy).ToArray();
            if(++reads==2){var data=JsonNode.Parse(process.INSTANCE_DATA_JSON!)!;data["Data"]![DrillingReportingReassignment.PayloadKey]![0]!["Command"]!["Reason"]="Changed after model load";process.INSTANCE_DATA_JSON=data.ToJsonString();changed=true;}
            return found;
        });
        var error=await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Ready.Original.Recorded());Assert.True(changed);Assert.Equal("The stored reporting envelope changed during native snapshot loading.",error.Message);Assert.Equal(writes,f.Mutations);
    }

    [Theory, InlineData("history"), InlineData("step"), InlineData("task")]
    public async Task RetaggedOriginalKeysCannotBecomeConfirmedAbsence(string fault)
    {
        var f = new Fixture(); await f.Prepare();
        if (fault == "history") f.Store.Rows.Add(new PROCESS_HISTORY { PROCESS_HISTORY_ID = f.Command.HistoryId(), PROCESS_INSTANCE_ID = "foreign", EVENT_TYPE = "REPORTING_TRANSFERRED" });
        if (fault == "step") f.Store.Rows.Add(new PROCESS_STEP_INSTANCE { PROCESS_STEP_INSTANCE_ID = f.Command.StepInstanceId(), PROCESS_INSTANCE_ID = "foreign" });
        if (fault == "task") f.Store.Rows.Add(new CROSS_PERSONA_TASK { CROSS_TASK_ID = f.Command.TaskIds()[0], PROCESS_STEP_INSTANCE_ID = "foreign", PROCESS_INSTANCE_ID = "foreign", ENTITY_ID = "other" });
        var before = JsonSerializer.Serialize(f.Store.Rows); await Assert.ThrowsAnyAsync<Exception>(() => f.Transfer(true)); Assert.Equal(before, JsonSerializer.Serialize(f.Store.Rows)); Assert.Equal(0, f.Mutations);
    }

    [Theory, InlineData("revoked"), InlineData("receiving-authority"), InlineData("target")]
    public async Task AuthorityOrTargetChangeAfterPersistenceBeginsRollsBackAllOwnedChanges(string fault)
    {
        var f = new Fixture(); await f.Prepare(); var before = JsonSerializer.Serialize(f.Store.Rows); var triggered = false;
        var foreign = new Mock<TheTechIdea.Beep.IDataSource>(MockBehavior.Strict);
        f.OnMutation = count => { if (count != 1) return; triggered = true;
            if (fault == "revoked") f.WriteRevoked = true;
            if (fault == "receiving-authority") f.Ready.Destination = f.Ready.Destination with { Authority = f.Ready.Destination.Authority with { AuthorityRevision = new('F', 64) } };
            if (fault == "target") f.Store.Editor.Setup(editor => editor.GetDataSource("lifecycle")).Returns(foreign.Object);
        };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Transfer()); Assert.True(triggered); Assert.Equal(before, JsonSerializer.Serialize(f.Store.Rows)); Assert.Equal("rollback", f.Store.Transactions.Last());
        if (fault == "target") foreign.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ClosingSourceChangeLeavesExactTransferRecoverableButClosingNativeTamperRefusesReadback()
    {
        var f = new Fixture(); await f.Prepare(); f.Ready.OnExecution = count => { if (count == 2) f.Ready.Original.Current.Recorded!.STATUS = "Changed after commit"; };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Transfer()); Assert.Equal(7, f.Mutations); Assert.True((await f.Transfer(true)).Recorded);
        var second = new Fixture(); await second.Prepare(); second.Ready.OnDestination = count => { if (count == 2) second.Store.Rows.OfType<CROSS_PERSONA_TASK>().Single(row => row.TASK_STATUS == "PENDING").TASK_CONTEXT_JSON = "Changed after closing source"; };
        await Assert.ThrowsAnyAsync<Exception>(() => second.Transfer()); Assert.Equal(7, second.Mutations); await Assert.ThrowsAnyAsync<Exception>(() => second.Transfer(true));
    }
}
#endif
