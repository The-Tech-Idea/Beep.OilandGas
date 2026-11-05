#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingCompletionReviewStartNativeTests
{
    private static readonly Lazy<Task<(DrillingDailyReportPage Page, DRILLING_DAILY_REPORT[] Rows)>> reports = new(()=>SeedReports(false));
    private static readonly Lazy<Task<(DrillingDailyReportPage Page, DRILLING_DAILY_REPORT[] Rows)>> acceptanceReports = new(()=>SeedReports(true));
    private static async Task<(DrillingDailyReportPage Page,DRILLING_DAILY_REPORT[] Rows)> SeedReports(bool readyForAcceptance)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f);
        f.Command = DrillingCompletionReportNativeTests.Complete(f, rows[2]);
        if(readyForAcceptance){var first=f.Command.Completion!.Tests.Single();f.Command=f.Command with{Completion=f.Command.Completion with{OutstandingItems=[],Tests=[first,first with{TestType="Recorded cement test",Reference="Signed cement certificate / site reference 43"}]}};}
        await f.Save();
        var history = await f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard);
        var page = await f.Store.Service.ReadDailyReportPageAsync(f.Execution, 3, 1, history.Revision, f.Guard, () => Task.CompletedTask);
        return (page, f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Select(CompanyInventorySourceReview.Copy).ToArray());
    }
    internal sealed class Fixture
    {
        internal CompanyProcessStartTests.Fixture Lifecycle = new("ConfiguredCompletionReviewer");
        internal DrillingDailyReportPage Current = null!;
        internal List<DRILLING_DAILY_REPORT> Rows = [];
        internal DrillingCompletionReviewStart Original = null!;
        internal int SourceReads;
        internal bool RevokeAfterWrite;
        internal Action<int>? OnSourceRead;
        internal async Task Prepare(int stages = 1, int destinations = 1,bool acceptanceReady=false)
        {
            var seed = await (acceptanceReady?acceptanceReports:reports).Value; Current = CompanyInventorySourceReview.Copy(seed.Page);
            Rows = seed.Rows.Select(CompanyInventorySourceReview.Copy).ToList();
            Lifecycle.Service=new(Lifecycle.Native.Editor.Object,new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler(),
                Moq.Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),Moq.Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(),"lifecycle",personaReader:Lifecycle.Native.Personas.Object);
            Lifecycle.Definition.PROCESS_TYPE = "DRILLING"; Lifecycle.Definition.ENTITY_TYPE = "DRILLING_DAILY_REPORT";
            Lifecycle.Definition.PROCESS_NAME = "Independent completion claim review";
            Lifecycle.Definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = Enumerable.Range(1, stages).Select(index =>
                new ProcessStepDefinition { StepId = "completion-" + index, StepName = "Independent completion stage " + index, SequenceNumber = index,
                    StepType = "APPROVAL", RequiresApproval = true, SlaHours = 24, RequiredRoles = ["ConfiguredCompletionReviewer"] }).ToArray() });
            var configured = (await Lifecycle.Service.GetProcessDefinitionAsync("definition"))!;
            var recipients = Enumerable.Range(1, destinations).Select(index => new RepositoryCompanyRoleRecipient("completion-inspector-" + index, "COMPLETION-" + index)).ToArray();
            var frozen = configured.Steps.Select(step => new DrillingCompletionReviewStage(new(step.StepId, step.StepName, step.SequenceNumber,
                step.RequiredRoles[0], recipients.Select(row => row.PersonaCode).ToArray(), step.SlaHours), new(step.StepId, step.RequiredRoles[0], recipients, new('A', 64)))).ToArray();
            Original = new(CompanyInventorySourceReview.Copy(Current.History), new(configured.ProcessId, configured.ProcessName, configured.Version,
                ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(configured)), frozen), "ConfiguredExecutionRecorder", "DRILLING_RECORDER", new('B', 64), Guid.NewGuid(),
                "Submit the exact reported completion for independent configured inspection and acceptance");
            Original.Require(); Original.RequireSource(Current);
        }
        internal Task Guard()
        {
            if (RevokeAfterWrite && Lifecycle.Native.Writes > 0) throw new UnauthorizedAccessException("The original sender authority changed after persistence began.");
            return Lifecycle.Guard();
        }
        internal Task<DrillingDailyReportPage> Read()
        {
            Assert.DoesNotContain("begin", Lifecycle.Native.Transactions.TakeLast(1));
            OnSourceRead?.Invoke(++SourceReads); return Task.FromResult(CompanyInventorySourceReview.Copy(Current));
        }
        internal Task<DrillingCompletionReviewStartReceipt> Start(bool recover = false) => Lifecycle.Service.RecordCompletionReviewStartAsync(Original, Guard, Read, recover);
        internal Task<string?> Find() => Lifecycle.Service.FindRecordedCompletionReviewAsync(Original.Scope().Company, Original.Scope().FieldId, Original.ClaimId, Guard);
        internal Task<(DrillingCompletionReviewStart Original, ProcessInstance Process, IReadOnlyList<CROSS_PERSONA_TASK> Tasks, string NativeRevision)> Recorded(CompanyScope? company = null) =>
            Lifecycle.Service.ReadRecordedCompletionReviewAsync(company ?? Original.Scope().Company, Original.InstanceId(), Guard);
        internal void Append(bool withdraw = false)
        {
            var history = Current.History; var last = DrillingDailyReportInput.FromRecord(Rows[^1]); var head = history.Head!;
            var basis = last.Basis with { NextNumber = history.Count + 1, PreviousReportId = head.ReportId, PreviousReportRevision = head.NativeRevision,
                PreviousPeriodEndUtc = head.LatestPeriodEndUtc, CheckedAtUtc = DateTimeOffset.UtcNow };
            basis = basis with { Revision = basis.ContentRevision() };
            var input = last with { Basis = basis, IntentId = Guid.NewGuid(), Completion = null, WithdrawCompletion = withdraw,
                Correction = withdraw ? new(last.ReportId(), head.NativeRevision, history.Count, "Explicit later withdrawal pending additional work") : null,
                PeriodStartUtc = withdraw ? last.PeriodStartUtc : head.LatestPeriodEndUtc,
                PeriodEndUtc = withdraw ? last.PeriodEndUtc : DateTimeOffset.UtcNow, Provenance = "Later controlled site observation" };
            Rows.Add(input.ToRecord(DateTime.UtcNow));
            var builder = new DrillingDailyReportHistoryBuilder(history.Execution, DateTimeOffset.UtcNow);
            var observations = Rows.Select(builder.Add).ToArray(); var current = builder.Build();
            var page = new DrillingDailyReportPage(current, Original.Source.ReportedCompletion!.Number - 1, 1,
                [builder.QualifyDisplay(observations.Single(row => row.DDR_ID == Original.ClaimId))], "");
            Current = page with { Revision = page.ContentRevision() }; Current.Require();
        }
    }
    [Theory, InlineData(1, 1), InlineData(2, 2)]
    public async Task CurrentImmutableClaimCreatesOnlyConfiguredInitialReviewGraphAndFirstStageApprovalTasks(int stages, int destinations)
    {
        var f = new Fixture(); await f.Prepare(stages, destinations); var source = JsonSerializer.Serialize(f.Rows);
        var receipt = await f.Start(); receipt.Require(f.Original); Assert.True(receipt.Recorded);
        Assert.Equal(stages + destinations + 2, f.Lifecycle.Native.Writes); Assert.Equal(new[] { "begin", "commit" }, f.Lifecycle.Native.Transactions);
        var recorded = await f.Recorded(); Assert.Equal(f.Original.Revision(), recorded.Original.Revision());
        Assert.Equal(f.Original.ClaimId, recorded.Process.EntityId); Assert.Equal("DRILLING_DAILY_REPORT", recorded.Process.EntityType);
        Assert.Equal(ProcessStatus.IN_PROGRESS, recorded.Process.Status); Assert.Equal(stages - 1, recorded.Process.StepInstances.Count(step => step.Status == StepStatus.BLOCKED));
        Assert.Equal(destinations, recorded.Tasks.Count); Assert.All(recorded.Tasks, task => { Assert.Equal("PENDING", task.TASK_STATUS); Assert.Equal("APPROVAL", task.TASK_TYPE); Assert.Null(task.DUE_DATE); });
        Assert.Empty(f.Lifecycle.Native.Rows.OfType<PROCESS_APPROVAL>()); Assert.Equal(source, JsonSerializer.Serialize(f.Rows));
        Assert.Equal("STARTED", f.Original.Source.Execution.Recorded!.STATUS); Assert.Equal("COMPLETION_REPORTED", f.Original.Source.ProgressState);
        Assert.True((await f.Start()).Recorded); Assert.True((await f.Start(true)).Recorded); Assert.Equal(f.Original.InstanceId(), await f.Find());
        Assert.Equal(stages + destinations + 2, f.Lifecycle.Native.Writes);
    }
    [Theory, InlineData(1), InlineData(2), InlineData(3), InlineData(4), InlineData(5)]
    public async Task FailureAtEveryProcessStepAuditOrTaskWriteRollsBackTheEntireReview(int fail)
    {
        var f = new Fixture(); await f.Prepare(2); f.Lifecycle.FailWrite = fail;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start()); Assert.Single(f.Lifecycle.Native.Rows); Assert.Equal("rollback", f.Lifecycle.Native.Transactions.Last());
    }
    [Fact]
    public async Task LostCommitRecoversExactOriginalWithoutLiveDefinitionOrAnotherWriteAndAnotherIntentCannotDuplicateClaim()
    {
        var f = new Fixture(); await f.Prepare(); Assert.False((await f.Start(true)).Recorded); Assert.Equal(0, f.Lifecycle.Native.Writes);
        f.Lifecycle.Native.FailCommit = true; await Assert.ThrowsAnyAsync<Exception>(() => f.Start()); f.Lifecycle.Native.FailCommit = false;
        var original = f.Original; f.Original = original with { IntentId = Guid.NewGuid() };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start()); f.Original = original;
        f.Lifecycle.Native.Rows.RemoveAll(row => row is PROCESS_DEFINITION);
        Assert.True((await f.Start(true)).Recorded); Assert.Equal(4, f.Lifecycle.Native.Writes);
    }
    [Theory, InlineData(false), InlineData(true)]
    public async Task RecoveryPreservesOriginalClaimAfterLaterProgressOrExplicitWithdrawalButCannotResubmitStaleHistory(bool withdraw)
    {
        var f = new Fixture(); await f.Prepare(); await f.Start(); f.Append(withdraw);
        Assert.Null(f.Current.History.ReportedCompletion); f.Original.RequireSource(f.Current, false);
        Assert.Throws<InvalidOperationException>(() => f.Original.RequireSource(f.Current));
        Assert.True((await f.Start(true)).Recorded); await Assert.ThrowsAnyAsync<Exception>(() => f.Start()); Assert.Equal(4, f.Lifecycle.Native.Writes);
    }
    [Theory, InlineData("borrowed"), InlineData("isolation"), InlineData("revoked"), InlineData("definition"), InlineData("changed-definition")]
    public async Task UnsupportedTransactionOrChangedAuthorityOrDefinitionCannotCreateReview(string fault)
    {
        var f = new Fixture(); await f.Prepare();
        if (fault == "borrowed") f.Lifecycle.Native.Borrowed = true; if (fault == "isolation") f.Lifecycle.Native.Isolation = IsolationLevel.ReadCommitted;
        if (fault == "revoked") f.RevokeAfterWrite = true; if (fault == "definition") f.Lifecycle.Definition.VERSION = "changed";
        if (fault == "changed-definition") f.Lifecycle.ChangeDefinition = true;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start()); Assert.Single(f.Lifecycle.Native.Rows);
    }
    [Theory, InlineData("step"), InlineData("retag-step-key"), InlineData("history"), InlineData("retag-history-key"), InlineData("approval"), InlineData("task"), InlineData("retag-task-key")]
    public async Task PartialOrRetaggedExpectedKeysCannotBecomeReadOnlyConfirmedAbsence(string fault)
    {
        var f = new Fixture(); await f.Prepare(); var id = f.Original.InstanceId(); var first = f.Original.Workflow.Stages[0].Definition;
        var step = DrillingCompletionReviewStartReceipt.StepId(id, first.StepId);
        object partial = fault switch {
            "step" => new PROCESS_STEP_INSTANCE { PROCESS_STEP_INSTANCE_ID = "orphan", PROCESS_INSTANCE_ID = id },
            "retag-step-key" => new PROCESS_STEP_INSTANCE { PROCESS_STEP_INSTANCE_ID = step, PROCESS_INSTANCE_ID = "foreign" },
            "history" => new PROCESS_HISTORY { PROCESS_HISTORY_ID = "orphan", PROCESS_INSTANCE_ID = id },
            "retag-history-key" => new PROCESS_HISTORY { PROCESS_HISTORY_ID = "completion-review-start-" + ProcessDefinitionRevision.FromJson(id), PROCESS_INSTANCE_ID = "foreign" },
            "approval" => new PROCESS_APPROVAL { PROCESS_APPROVAL_ID = "orphan", PROCESS_INSTANCE_ID = id },
            "retag-task-key" => new CROSS_PERSONA_TASK { CROSS_TASK_ID = DrillingCompletionReviewStartReceipt.TaskId(id, step, first.PersonaCodes[0]), PROCESS_INSTANCE_ID = "foreign", PROCESS_STEP_INSTANCE_ID = "foreign", ENTITY_TYPE = "WELL", ENTITY_ID = "foreign" },
            _ => new CROSS_PERSONA_TASK { CROSS_TASK_ID = "orphan", PROCESS_INSTANCE_ID = id } };
        f.Lifecycle.Native.Rows.Add(partial); await Assert.ThrowsAnyAsync<Exception>(() => f.Start(true)); Assert.Equal(0, f.Lifecycle.Native.Writes);
    }
    [Theory, InlineData("context"), InlineData("missing-task"), InlineData("retag"), InlineData("route"), InlineData("approval"), InlineData("claim"), InlineData("audit"),
        InlineData("priority"), InlineData("process-created"), InlineData("step-changed"), InlineData("history-changed"), InlineData("notes"), InlineData("extra-data"), InlineData("payload-updated")]
    public async Task ChangedNativeInitialGraphCannotBeAcknowledgedOrRepaired(string fault)
    {
        var f = new Fixture(); await f.Prepare(); await f.Start(); var task = f.Lifecycle.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single();
        if (fault == "context") task.TASK_CONTEXT_JSON = "{}"; if (fault == "missing-task") f.Lifecycle.Native.Rows.Remove(task);
        if (fault == "retag") f.Lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>().Single().ENTITY_TYPE = "WELL";
        if (fault == "route") task.ROUTE = "/other"; if (fault == "approval") f.Lifecycle.Native.Rows.Add(new PROCESS_APPROVAL { PROCESS_APPROVAL_ID = "unqualified", PROCESS_INSTANCE_ID = f.Original.InstanceId() });
        if (fault == "claim") f.Lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>().Single().ENTITY_ID = "foreign-claim";
        if (fault == "audit") f.Lifecycle.Native.Rows.OfType<PROCESS_HISTORY>().Single().USER_ID = "foreign-actor";
        if (fault == "priority") task.PRIORITY=1;
        if (fault == "process-created") { var process=f.Lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>().Single();process.ROW_CREATED_DATE=process.STARTED_DATE!.Value.AddTicks(-1); }
        if (fault == "step-changed") f.Lifecycle.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().First().ROW_CHANGED_BY="unqualified-editor";
        if (fault == "history-changed") { var history=f.Lifecycle.Native.Rows.OfType<PROCESS_HISTORY>().Single();history.ROW_CHANGED_DATE=history.ROW_CREATED_DATE!.Value.AddTicks(-1); }
        if (fault == "notes") f.Lifecycle.Native.Rows.OfType<PROCESS_HISTORY>().Single().DETAILS="Unqualified acceptance narrative";
        if (fault is "extra-data" or "payload-updated")
        {
            var process=f.Lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>().Single();var payload=System.Text.Json.Nodes.JsonNode.Parse(process.INSTANCE_DATA_JSON!)!;
            if(fault=="extra-data")payload["Data"]!["UnqualifiedAcceptance"]="APPROVED";else payload["LastUpdated"]=DateTime.UtcNow.AddHours(-1);
            process.INSTANCE_DATA_JSON=payload.ToJsonString();
        }
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start(true)); await Assert.ThrowsAnyAsync<Exception>(() => f.Recorded()); Assert.Equal(4, f.Lifecycle.Native.Writes);
    }
    [Theory, InlineData(false), InlineData(true)]
    public async Task ClosingSourceCallbackCannotHideLatePartialOrChangedNativeGraph(bool recorded)
    {
        var f = new Fixture(); await f.Prepare(); if (recorded) await f.Start(); f.SourceReads = 0;
        f.OnSourceRead = count => { if (count == 2) f.Lifecycle.Native.Rows.Add(new PROCESS_HISTORY { PROCESS_HISTORY_ID = "late-orphan", PROCESS_INSTANCE_ID = f.Original.InstanceId() }); };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start(true)); Assert.Equal(recorded ? 4 : 0, f.Lifecycle.Native.Writes);
    }
    [Fact]
    public async Task ChangedSourceAfterCommitIsUnconfirmedThenOriginalHistoricalRecoveryRemainsPossible()
    {
        var f = new Fixture(); await f.Prepare(); f.OnSourceRead = count => { if (count == 2) f.Append(); };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start()); Assert.Equal(4, f.Lifecycle.Native.Writes);
        f.OnSourceRead = null; Assert.True((await f.Start(true)).Recorded); Assert.Equal(4, f.Lifecycle.Native.Writes);
    }
    [Fact]
    public async Task ReceiptRequiresExactFirstStageTaskKeysAndReaderReturnsIndependentCopiesWithCompanyAdmission()
    {
        var f = new Fixture(); await f.Prepare(); var receipt = await f.Start();
        Assert.Throws<InvalidOperationException>(() => (receipt with { TaskIds = ["foreign-task"] }).Require(f.Original));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Recorded(new("tenant", "foreign")));
        var read = await f.Recorded(); read.Original.Source.Execution.Recorded!.STATUS = "consumer mutation";
        Assert.Equal("STARTED", (await f.Recorded()).Original.Source.Execution.Recorded!.STATUS);
    }
    [Theory, InlineData("body"), InlineData("actor"), InlineData("revision"), InlineData("audit"), InlineData("claim")]
    public async Task RehashedPageCannotReplaceTheExactOriginalClaimColumnsDuringRecordOrRecovery(string fault)
    {
        var f=new Fixture();await f.Prepare();var row=f.Current.Reports.Single();
        row=fault switch {
            "body"=>row with{Completion=row.Completion! with{WorkCompleted="Changed report body"}},
            "actor"=>row with{ROW_CREATED_BY="foreign-recorder"},
            "revision"=>row with{REPORT_REVISION=new('F',64)},
            "audit"=>row with{ROW_CREATED_DATE=row.ROW_CREATED_DATE.AddTicks(1),ROW_CHANGED_DATE=row.ROW_CHANGED_DATE.AddTicks(1)},
            _=>row with{DDR_ID="foreign-claim"}};
        var changed=f.Current with{Reports=[row]};f.Current=changed with{Revision=changed.ContentRevision()};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(true));Assert.Equal(0,f.Lifecycle.Native.Writes);
    }
    [Theory, InlineData("prefix"), InlineData("payload")]
    public async Task GenericMutationAndPublicTaskRoutingRemainDeniedAfterCompletionReviewRetagging(string marker)
    {
        var f=new Fixture();await f.Prepare();await f.Start();var originalId=f.Original.InstanceId();
        var process=f.Lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>().Single();process.ENTITY_TYPE="WELL";
        var id=marker=="payload"?"retagged-generic-review":originalId;
        if(marker=="prefix")process.INSTANCE_DATA_JSON=JsonSerializer.Serialize(new PROCESS_DATA{InstanceId=id,Data=new()});
        else
        {
            process.PROCESS_INSTANCE_ID=id;
            var payload=System.Text.Json.Nodes.JsonNode.Parse(process.INSTANCE_DATA_JSON!)!;payload["InstanceId"]=id;process.INSTANCE_DATA_JSON=payload.ToJsonString();
            foreach(var step in f.Lifecycle.Native.Rows.OfType<PROCESS_STEP_INSTANCE>())step.PROCESS_INSTANCE_ID=id;
            foreach(var history in f.Lifecycle.Native.Rows.OfType<PROCESS_HISTORY>())history.PROCESS_INSTANCE_ID=id;
        }
        var actor=f.Original.Scope().ActorUserId;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.Service.CancelProcessAsync(id,"Unqualified completion decision",actor));
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.Service.AddHistoryEntryAsync(id,new(){Action="UNQUALIFIED_COMPLETION"}));
        var first=f.Original.Workflow.Stages[0].Definition;
        var router=new CrossPersonaTaskRouter(f.Lifecycle.Native.Editor.Object,Moq.Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Moq.Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),Moq.Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(),()=>Task.FromResult("lifecycle"),personas:f.Lifecycle.Native.Personas.Object);
        await Assert.ThrowsAnyAsync<Exception>(()=>router.RouteTaskAsync(id,DrillingCompletionReviewStartReceipt.StepId(originalId,first.StepId),first.AssignedRole,"WELL",f.Original.ClaimId,"Unqualified",actor));
        Assert.Equal(4,f.Lifecycle.Native.Writes);
    }
    [Fact]
    public async Task SubmittingCompletionReviewPreservesExistingOngoingReportingResponsibilityAndItsPendingTask()
    {
        var f=new Fixture();await f.Prepare();var reportingTemplate=new DrillingExecutionAssignmentNativeTests.Fixture();await reportingTemplate.Prepare();
        var definition=CompanyInventorySourceReview.Copy(reportingTemplate.Lifecycle.Definition);definition.PROCESS_DEFINITION_ID="ongoing-reporting";
        f.Lifecycle.Native.Rows.Add(definition);var configured=(await f.Lifecycle.Service.GetProcessDefinitionAsync(definition.PROCESS_DEFINITION_ID))!;
        var option=reportingTemplate.Original.Workflow with{ProcessId=configured.ProcessId,DefinitionRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(configured))};
        var reporting=reportingTemplate.Original with{Source=CompanyInventorySourceReview.Copy(f.Current.History.Execution),Workflow=option,IntentId=Guid.NewGuid()};
        var assigned=await f.Lifecycle.Service.RecordExecutionAssignmentAsync(reporting,f.Guard,()=>Task.FromResult(f.Current.History.Execution));Assert.True(assigned.Recorded);
        string OriginalResponsibility()=>JsonSerializer.Serialize(new {
            Process=f.Lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>().Single(row=>row.PROCESS_INSTANCE_ID==reporting.InstanceId()),
            Steps=f.Lifecycle.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Where(row=>row.PROCESS_INSTANCE_ID==reporting.InstanceId()).ToArray(),
            History=f.Lifecycle.Native.Rows.OfType<PROCESS_HISTORY>().Where(row=>row.PROCESS_INSTANCE_ID==reporting.InstanceId()).ToArray(),
            Tasks=f.Lifecycle.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>row.PROCESS_INSTANCE_ID==reporting.InstanceId()).ToArray()});
        var before=OriginalResponsibility();Assert.True((await f.Start()).Recorded);Assert.True((await f.Start(true)).Recorded);
        Assert.Equal(before,OriginalResponsibility());Assert.Equal(2,f.Lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>().Count());
        Assert.Equal("PENDING",f.Lifecycle.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.PROCESS_INSTANCE_ID==reporting.InstanceId()).TASK_STATUS);
        Assert.Empty(f.Lifecycle.Native.Rows.OfType<PROCESS_APPROVAL>());Assert.Equal(8,f.Lifecycle.Native.Writes);
    }
}
#endif
