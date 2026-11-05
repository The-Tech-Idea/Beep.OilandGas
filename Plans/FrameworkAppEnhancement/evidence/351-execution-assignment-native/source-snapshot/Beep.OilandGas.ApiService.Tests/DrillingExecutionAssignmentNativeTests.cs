#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingExecutionAssignmentNativeTests
{
    // Build approval/start evidence through the actual existing native owners once, then give every case a
    // private source snapshot and a fresh lifecycle transaction store. External source access remains a seam.
    private static readonly Lazy<Task<DrillingExecutionStatus>> ApprovedExecution = new(async () =>
    {
        using var execution = new DrillingExecutionStartNativeTests.Fixture();
        await execution.Prepare(); await execution.Save();
        return await execution.Store.Service.ReadExecutionStatusAsync(execution.Current, execution.Guard);
    });

    internal sealed class Fixture
    {
        internal CompanyProcessStartTests.Fixture Lifecycle = new("ConfiguredReportingRole");
        internal DrillingExecutionAssignment Original = null!;
        internal DrillingExecutionStatus Current = null!;
        internal bool ChangeSourceAfterCommit, AddPartialAfterCommit, RevokeAfterWrite;
        internal int SourceReads;

        internal async Task Prepare(int destinations = 1)
        {
            Current = CompanyInventorySourceReview.Copy(await ApprovedExecution.Value);
            Lifecycle.Service = new(Lifecycle.Native.Editor.Object, new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler(),
                Moq.Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),
                Moq.Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(), "lifecycle", personaReader: Lifecycle.Native.Personas.Object);
            Lifecycle.Definition.PROCESS_TYPE = "DRILLING";
            Lifecycle.Definition.ENTITY_TYPE = "DRILLING_OPERATION";
            Lifecycle.Definition.PROCESS_NAME = "Configured ongoing execution reporting";
            Lifecycle.Definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
                new ProcessStepDefinition { StepId = "report-progress", StepName = "Record observed execution progress", SequenceNumber = 1,
                    StepType = "DATA_ENTRY", RequiresApproval = false, RequiredRoles = ["ConfiguredReportingRole"] } } });
            var definition = (await Lifecycle.Service.GetProcessDefinitionAsync("definition"))!;
            var step = definition.Steps.Single();
            var recipients = Enumerable.Range(1, destinations).Select(index => new RepositoryCompanyRoleRecipient("site-recorder-" + index, "SITE-" + index)).ToArray();
            Original = new(CompanyInventorySourceReview.Copy(Current), new(definition.ProcessId, definition.ProcessName, definition.Version,
                ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),
                new(step.StepId, step.StepName, step.SequenceNumber, step.RequiredRoles[0], recipients.Select(row => row.PersonaCode).ToArray(), step.SlaHours),
                new(step.StepId, step.RequiredRoles[0], recipients, new('A', 64))),
                "ConfiguredExecutionAssigner", "EXECUTION_COORDINATION", new('B', 64), Guid.NewGuid(), "Assign the original recorded execution to configured site reporting");
            Original.Require();
        }
        internal Task Guard()
        {
            if (RevokeAfterWrite && Lifecycle.Native.Writes > 0) throw new UnauthorizedAccessException("Current original assignment authority was revoked after persistence began");
            return Lifecycle.Guard();
        }
        internal Task<DrillingExecutionStatus> Read()
        {
            Assert.DoesNotContain("begin", Lifecycle.Native.Transactions.TakeLast(1));
            if (++SourceReads > 1)
            {
                if (ChangeSourceAfterCommit) Current.Recorded!.STATUS = "UNQUALIFIED";
                if (AddPartialAfterCommit) Lifecycle.Native.Rows.Add(new PROCESS_HISTORY { PROCESS_HISTORY_ID = "late-orphan", PROCESS_INSTANCE_ID = Original.InstanceId() });
            }
            return Task.FromResult(Current);
        }
        internal Task<DrillingExecutionAssignmentReceipt> Start(bool recover = false) =>
            Lifecycle.Service.RecordExecutionAssignmentAsync(Original, Guard, Read, recover);
        internal Task<string?> Find() => Lifecycle.Service.FindRecordedExecutionAssignmentAsync(Original.Scope().Company, Original.Scope().FieldId, Original.OperationId, Guard);
        internal Task<(DrillingExecutionAssignment Original, ProcessInstance Process, IReadOnlyList<CROSS_PERSONA_TASK> Tasks, string NativeRevision)> Recorded(CompanyScope? company = null) =>
            Lifecycle.Service.ReadRecordedExecutionAssignmentAsync(company ?? Original.Scope().Company, Original.InstanceId(), Guard);
    }

    [Theory, InlineData(1), InlineData(2)]
    public async Task ExactRecordedExecutionCreatesOneOngoingResponsibilityAndOneTaskPerConfiguredPersona(int destinations)
    {
        var f = new Fixture(); await f.Prepare(destinations);
        var nativeExecution = JsonSerializer.Serialize(f.Current.Recorded);
        var result = await f.Start(); result.Require(f.Original);
        Assert.True(result.Recorded); Assert.Equal(destinations, result.TaskIds.Count); Assert.Equal(3 + destinations, f.Lifecycle.Native.Writes);
        Assert.Equal(new[] { "begin", "commit" }, f.Lifecycle.Native.Transactions);
        var recorded = await f.Recorded();
        Assert.Equal(f.Original.Revision(), recorded.Original.Revision()); Assert.Equal("DRILLING_OPERATION", recorded.Process.EntityType);
        Assert.Equal(f.Original.InstanceId(), await f.Find());
        Assert.Equal(f.Original.OperationId, recorded.Process.EntityId); Assert.Equal(ProcessStatus.IN_PROGRESS, recorded.Process.Status);
        Assert.All(recorded.Tasks, task => { Assert.Equal("DATA_ENTRY", task.TASK_TYPE); Assert.Equal("PENDING", task.TASK_STATUS);
            Assert.Null(task.DUE_DATE); Assert.Equal("ConfiguredReportingRole", task.ASSIGNED_ROLE);
            Assert.Equal(WorkflowTaskRoute.For("FIELD", f.Original.InstanceId(), "DRILLING_OPERATION"), task.ROUTE); });
        Assert.Empty(f.Lifecycle.Native.Rows.OfType<PROCESS_APPROVAL>());
        Assert.True((await f.Start()).Recorded); Assert.True((await f.Start(true)).Recorded);
        Assert.Equal(3 + destinations, f.Lifecycle.Native.Writes);
        Assert.Equal(nativeExecution, JsonSerializer.Serialize(f.Current.Recorded));
        f.Lifecycle.Native.Editor.Verify(editor => editor.GetDataSource(Moq.It.Is<string>(connection => connection != "lifecycle")), Moq.Times.Never());
        recorded.Tasks[0].TASK_STATUS = "consumer mutation";
        Assert.All((await f.Recorded()).Tasks, task => Assert.Equal("PENDING", task.TASK_STATUS));
    }

    [Theory, InlineData(1), InlineData(2), InlineData(3), InlineData(4), InlineData(5)]
    public async Task EveryProcessStepHistoryOrPersonaTaskWriteFailureRollsBackTheWholeAssignment(int fail)
    {
        var f = new Fixture(); await f.Prepare(2); f.Lifecycle.FailWrite = fail;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start());
        Assert.Single(f.Lifecycle.Native.Rows); Assert.Equal("rollback", f.Lifecycle.Native.Transactions.Last());
        f.Lifecycle.Native.Editor.Verify(editor => editor.GetDataSource(Moq.It.Is<string>(connection => connection != "lifecycle")), Moq.Times.Never());
    }

    [Fact]
    public async Task LostCommitIsRecoverableReadOnlyWithoutAnotherWriteOrLiveDefinition()
    {
        var f = new Fixture(); await f.Prepare(); f.Lifecycle.Native.FailCommit = true;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start()); f.Lifecycle.Native.FailCommit = false;
        f.Lifecycle.Native.Rows.RemoveAll(row => row is PROCESS_DEFINITION);
        var result = await f.Start(true); Assert.True(result.Recorded); Assert.Equal(4, f.Lifecycle.Native.Writes);
        Assert.Equal(f.Original.Revision(), (await f.Recorded()).Original.Revision());
    }

    [Fact]
    public async Task RecoveryAllowsCurrentReadOnlySourceWithoutInventingNewSenderRights()
    {
        var f = new Fixture(); await f.Prepare(); await f.Start();
        var basis = f.Current.Basis with { RecorderRoles = [], PersonaCode = null, RecorderAuthorityRevision = null };
        basis = basis with { Revision = basis.ContentRevision() };
        f.Current = f.Current with { Basis = basis }; f.Current = f.Current with { Revision = f.Current.ContentRevision() };
        Assert.True((await f.Start(true)).Recorded); Assert.Equal(4, f.Lifecycle.Native.Writes);
    }

    [Fact]
    public async Task AnotherOriginalIsConflictForBothNewWriteAndReadOnlyRecovery()
    {
        var f = new Fixture(); await f.Prepare(); await f.Start(); f.Original = f.Original with { IntentId = Guid.NewGuid() };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start()); await Assert.ThrowsAnyAsync<Exception>(() => f.Start(true));
        Assert.Equal(4, f.Lifecycle.Native.Writes);
    }

    [Fact]
    public async Task SourceChangeAfterCommitReturnsUnconfirmedUntilOriginalNativeSourceIsReconciled()
    {
        var f = new Fixture(); await f.Prepare(); f.ChangeSourceAfterCommit = true;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start()); Assert.Equal(4, f.Lifecycle.Native.Writes);
        f.ChangeSourceAfterCommit = false;
        f.Current = CompanyInventorySourceReview.Copy(await ApprovedExecution.Value);
        f.Original = (await f.Recorded()).Original;
        Assert.True((await f.Start(true)).Recorded); Assert.Equal(4, f.Lifecycle.Native.Writes);
    }

    [Fact]
    public async Task PartialGraphAppearingAtClosingSourceReadCannotBecomeConfirmedAbsence()
    {
        var f = new Fixture(); await f.Prepare(); f.AddPartialAfterCommit = true;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start(true)); Assert.Equal(0, f.Lifecycle.Native.Writes);
        Assert.Single(f.Lifecycle.Native.Rows.OfType<PROCESS_HISTORY>());
    }

    [Theory, InlineData("step"), InlineData("history"), InlineData("approval"), InlineData("task")]
    public async Task PartialEvidenceCannotBecomeConfirmedAbsenceOrBeRepaired(string partial)
    {
        var f = new Fixture(); await f.Prepare(); Assert.Null(await f.Find()); Assert.False((await f.Start(true)).Recorded);
        var id = f.Original.InstanceId();
        f.Lifecycle.Native.Rows.Add(partial switch {
            "step" => new PROCESS_STEP_INSTANCE { PROCESS_STEP_INSTANCE_ID = "orphan", PROCESS_INSTANCE_ID = id },
            "history" => new PROCESS_HISTORY { PROCESS_HISTORY_ID = "orphan", PROCESS_INSTANCE_ID = id },
            "approval" => new PROCESS_APPROVAL { PROCESS_APPROVAL_ID = "orphan", PROCESS_INSTANCE_ID = id },
            _ => (object)new CROSS_PERSONA_TASK { CROSS_TASK_ID = "orphan", PROCESS_INSTANCE_ID = id, ENTITY_ID = f.Original.OperationId, ENTITY_TYPE = "DRILLING_OPERATION" } });
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start(true)); await Assert.ThrowsAnyAsync<Exception>(() => f.Start());
        Assert.Equal(0, f.Lifecycle.Native.Writes);
    }

    [Theory, InlineData("missing-task"), InlineData("retag"), InlineData("route"), InlineData("context"), InlineData("role"),
        InlineData("report-proof"), InlineData("approval"), InlineData("process-audit"), InlineData("extra-task"), InlineData("duplicate"), InlineData("inactive"),
        InlineData("extra-process-data"), InlineData("process-data-time"), InlineData("extra-step-data"), InlineData("step-outcome"),
        InlineData("priority"), InlineData("task-creation-time"), InlineData("task-change-actor")]
    public async Task CorruptOrAmbiguousNativeGraphCannotBeReadAcknowledgedOrRepaired(string fault)
    {
        var f = new Fixture(); await f.Prepare(); await f.Start();
        var task = Assert.Single(f.Lifecycle.Native.Rows.OfType<CROSS_PERSONA_TASK>()); var process = Assert.Single(f.Lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>());
        if (fault == "missing-task") f.Lifecycle.Native.Rows.Remove(task);
        if (fault == "retag") process.ENTITY_TYPE = "WELL";
        if (fault == "route") task.ROUTE = "/other";
        if (fault == "context") task.TASK_CONTEXT_JSON = "{}";
        if (fault == "role") task.ASSIGNED_ROLE = "ForeignRole";
        if (fault == "report-proof") task.TASK_TYPE = "APPROVAL";
        if (fault == "approval") f.Lifecycle.Native.Rows.Add(new PROCESS_APPROVAL { PROCESS_APPROVAL_ID = "unqualified", PROCESS_INSTANCE_ID = f.Original.InstanceId() });
        if (fault == "process-audit") process.ROW_CREATED_BY = "another";
        if (fault == "extra-task") { var copy = CompanyInventorySourceReview.Copy(task); copy.CROSS_TASK_ID = "unqualified"; f.Lifecycle.Native.Rows.Add(copy); }
        if (fault == "duplicate") { var copy = CompanyInventorySourceReview.Copy(process); copy.PROCESS_INSTANCE_ID = "another"; f.Lifecycle.Native.Rows.Add(copy); }
        if (fault == "inactive") process.ACTIVE_IND = "N";
        if (fault is "extra-process-data" or "process-data-time")
        {
            var payload = JsonSerializer.Deserialize<Beep.OilandGas.Models.Data.Process.PROCESS_DATA>(process.INSTANCE_DATA_JSON!)!;
            if (fault == "extra-process-data") payload.Data.Add("UnqualifiedProgress", "Complete");
            else payload.LastUpdated = payload.LastUpdated!.Value.AddHours(1);
            process.INSTANCE_DATA_JSON = JsonSerializer.Serialize(payload);
        }
        if (fault is "extra-step-data" or "step-outcome")
        {
            var step = Assert.Single(f.Lifecycle.Native.Rows.OfType<PROCESS_STEP_INSTANCE>());
            if (fault == "step-outcome") step.OUTCOME = "COMPLETED";
            else step.STEP_DATA_JSON = JsonSerializer.Serialize(new Beep.OilandGas.Models.Data.Process.PROCESS_STEP_DATA { Data = new() { ["UnqualifiedProgress"] = "Complete" } });
        }
        if (fault == "priority") task.PRIORITY = 1;
        if (fault == "task-creation-time") task.ROW_CREATED_DATE = DateTime.UnixEpoch;
        if (fault == "task-change-actor") task.ROW_CHANGED_BY = "foreign-actor";
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start(true)); await Assert.ThrowsAnyAsync<Exception>(() => f.Recorded());
        await Assert.ThrowsAnyAsync<Exception>(() => f.Find());
        Assert.Equal(4, f.Lifecycle.Native.Writes);
    }

    [Theory, InlineData("borrowed"), InlineData("isolation"), InlineData("revoked"), InlineData("revoked-after-write"), InlineData("binding-after-write"), InlineData("definition"), InlineData("changed-source"), InlineData("missing-execution")]
    public async Task UnsupportedTransactionOrChangedAuthorityDefinitionOrExecutionCannotCreateAssignment(string fault)
    {
        var f = new Fixture(); await f.Prepare();
        if (fault == "borrowed") f.Lifecycle.Native.Borrowed = true;
        if (fault == "isolation") f.Lifecycle.Native.Isolation = IsolationLevel.ReadCommitted;
        if (fault == "revoked") f.Lifecycle.RevokeAt = 1;
        if (fault == "revoked-after-write") f.RevokeAfterWrite = true;
        if (fault == "binding-after-write")
            f.Lifecycle.Native.Editor.Setup(editor => editor.GetDataSource("lifecycle")).Returns(() => f.Lifecycle.Native.Writes > 0
                ? Moq.Mock.Of<TheTechIdea.Beep.IDataSource>() : f.Lifecycle.Native.Source.Object);
        if (fault == "definition") f.Lifecycle.ChangeDefinition = true;
        if (fault == "changed-source") { var copy = CompanyInventorySourceReview.Copy(f.Current); copy.Recorded!.PROGRAM_ID = "changed"; f.Current = copy; }
        if (fault == "missing-execution") { f.Current = f.Current with { Recorded = null }; f.Current = f.Current with { Revision = f.Current.ContentRevision() }; }
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start()); Assert.Single(f.Lifecycle.Native.Rows);
    }

    [Theory, InlineData("approval"), InlineData("multiple"), InlineData("action"), InlineData("optional"), InlineData("alternate"), InlineData("role"), InlineData("condition")]
    public void OngoingReportingMustUseAnExplicitConfiguredSingleRoleDataEntryStep(string fault)
    {
        var step = new ProcessStepDefinition { StepId = "progress", StepName = "Reporting", StepType = "DATA_ENTRY", RequiredRoles = ["ConfiguredRole"] };
        var definition = new ProcessDefinition { ProcessId = "configured", ProcessType = "DRILLING", EntityType = "DRILLING_OPERATION", Steps = [step] };
        if (fault == "approval") step.RequiresApproval = true;
        if (fault == "multiple") definition.Steps.Add(new() { StepId = "completion", RequiredRoles = ["AnotherRole"] });
        if (fault == "action") step.StepType = "ACTION";
        if (fault == "optional") step.IsRequired = false;
        if (fault == "alternate") step.NextStepId = "completion";
        if (fault == "role") step.RequiredRoles.Add("AnotherRole");
        if (fault == "condition") step.ConditionalNextSteps.Add("another");
        Assert.ThrowsAny<Exception>(() => DevelopmentReviewDefinition.RequireExecutionReporting(definition));
    }

    [Fact]
    public async Task GenericProcessAndPublicRouterCannotBypassExecutionAssignmentOwnerEvenAfterRetagging()
    {
        var f = new Fixture(); await f.Prepare(); await f.Start(); var id = f.Original.InstanceId(); var actor = f.Original.Scope().ActorUserId;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Lifecycle.Service.StartProcessAsync("definition", f.Original.OperationId, "DRILLING_OPERATION", f.Original.Scope().FieldId, actor));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Lifecycle.Service.CancelProcessAsync(id, "Unqualified", actor));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Lifecycle.Service.SkipStepAsync(id, f.Original.Workflow.Step.StepId, "Unqualified", actor));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Lifecycle.Service.AddHistoryEntryAsync(id, new() { Action = "UNQUALIFIED" }));
        var router = new CrossPersonaTaskRouter(f.Lifecycle.Native.Editor.Object, Moq.Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),
            Moq.Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(), Moq.Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(),
            () => Task.FromResult("lifecycle"), personas: f.Lifecycle.Native.Personas.Object);
        await Assert.ThrowsAnyAsync<Exception>(() => router.RouteTaskAsync(id, DrillingExecutionAssignmentReceipt.StepId(id, f.Original.Workflow.Step.StepId),
            f.Original.Workflow.Step.AssignedRole, "DRILLING_OPERATION", f.Original.OperationId, "Unqualified", actor));
        Assert.Single(f.Lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>()).ENTITY_TYPE = "WELL";
        await Assert.ThrowsAnyAsync<Exception>(() => router.RouteTaskAsync(id, DrillingExecutionAssignmentReceipt.StepId(id, f.Original.Workflow.Step.StepId),
            f.Original.Workflow.Step.AssignedRole, "WELL", f.Original.OperationId, "Unqualified", actor));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Lifecycle.Service.CancelProcessAsync(id, "Unqualified", actor));
        Assert.Equal(4, f.Lifecycle.Native.Writes);
    }

    [Fact]
    public async Task AnotherCompanyCannotReadTheAssignmentAndCompanyAdmissionUsesItsOriginalProof()
    {
        var f = new Fixture(); await f.Prepare(); await f.Start();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Recorded(new("tenant", "other-company")));
        var process = (await f.Recorded()).Process;
        Assert.True(WorkflowTaskProcessAdmission.CompanyMatches(process, f.Original.Scope().Company));
        Assert.False(WorkflowTaskProcessAdmission.CompanyMatches(process, new("tenant", "other-company")));
        Assert.Equal(4, f.Lifecycle.Native.Writes);
    }

    [Fact]
    public async Task ExecutionInboxRowsRequireQualifiedSourceProofAndCannotBeDisguisedAsReviewWork()
    {
        var f = new Fixture(); await f.Prepare(); await f.Start(); var recorded = await f.Recorded(); var task = recorded.Tasks.Single();
        var row = new UnifiedTask { TaskId = task.CROSS_TASK_ID, TaskType = "DATA_ENTRY", Priority = 3, Status = "PENDING", ScopeKind = "FIELD",
            AssignedRole = task.ASSIGNED_ROLE, EntityType = task.ENTITY_TYPE, EntityId = task.ENTITY_ID, WorkflowName = task.PROCESS_INSTANCE_ID,
            StepName = task.PROCESS_STEP_INSTANCE_ID, Route = task.ROUTE };
        Assert.ThrowsAny<Exception>(() => WorkflowInboxReview.RequireTask(row, [task.ASSIGNED_ROLE]));
        row.ExecutionAssignmentRevision = recorded.NativeRevision; WorkflowInboxReview.RequireTask(row, [task.ASSIGNED_ROLE]);
        row.TaskType = "REVIEW"; Assert.ThrowsAny<Exception>(() => WorkflowInboxReview.RequireTask(row, [task.ASSIGNED_ROLE]));
        row.TaskType = "DATA_ENTRY"; row.EntityType = "WELL"; row.Route = WorkflowTaskRoute.For("FIELD", row.WorkflowName, "WELL");
        Assert.ThrowsAny<Exception>(() => WorkflowInboxReview.RequireTask(row, [task.ASSIGNED_ROLE]));
    }
}
#endif
