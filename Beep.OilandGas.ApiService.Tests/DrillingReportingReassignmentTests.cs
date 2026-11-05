#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingReportingReassignmentTests
{
    internal sealed class Fixture
    {
        internal DrillingExecutionAssignmentNativeTests.Fixture Original = new();
        internal DrillingExecutionAssignmentOption Destination = null!;
        internal string ManagerRole = null!;
        internal int DestinationReads, ExecutionReads;
        internal Action<int>? OnDestination, OnExecution;
        internal Action? OnGuard;
        internal async Task Prepare()
        {
            await Original.Prepare(2); await Original.Start(); ManagerRole=Original.Original.SenderRole;
            var definition = CompanyInventorySourceReview.Copy(Original.Lifecycle.Definition);
            definition.PROCESS_DEFINITION_ID = "replacement-reporting";
            definition.PROCESS_NAME = "Replacement reporting coordination";
            definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
                new ProcessStepDefinition { StepId = "replacement-progress", StepName = "Replacement observed progress",
                    SequenceNumber = 1, StepType = "DATA_ENTRY", RequiredRoles = ["ReplacementReportingRole"] } } });
            Original.Lifecycle.Native.Rows.Add(definition);
            var configured = (await Original.Lifecycle.Service.GetProcessDefinitionAsync(definition.PROCESS_DEFINITION_ID))!;
            var step = configured.Steps.Single();
            Destination = new(configured.ProcessId, configured.ProcessName, configured.Version,
                ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(configured)),
                new(step.StepId, step.StepName, step.SequenceNumber, step.RequiredRoles.Single(), ["NEW-SITE"], step.SlaHours),
                new(step.StepId, step.RequiredRoles.Single(), [new("new-reporter", "NEW-SITE")], new('C', 64)));
        }
        internal Task Guard() { OnGuard?.Invoke(); return Original.Guard(); }
        internal Task<DrillingExecutionStatus> Execution()
        {
            Assert.DoesNotContain("begin", Original.Lifecycle.Native.Transactions.TakeLast(1));
            ++ExecutionReads; OnExecution?.Invoke(ExecutionReads); return Task.FromResult(CompanyInventorySourceReview.Copy(Original.Current));
        }
        internal Task<DrillingExecutionAssignmentOption> CurrentDestination()
        {
            Assert.DoesNotContain("begin", Original.Lifecycle.Native.Transactions.TakeLast(1));
            ++DestinationReads; OnDestination?.Invoke(DestinationReads); return Task.FromResult(CompanyInventorySourceReview.Copy(Destination));
        }
        internal Task<DrillingReportingReassignmentReview> Review() => Original.Lifecycle.Service.ReviewReportingReassignmentAsync(
            Original.Original.Scope().Company, Original.Original.InstanceId(), Destination, ManagerRole, Original.Original.SenderPersonaCode,
            Original.Original.SenderAuthorityRevision, Guard, Execution, CurrentDestination);
    }

    [Fact]
    public async Task CurrentOriginalGenerationAndChangedDestinationAreCheckedWithoutTransferringOrCompletingAnything()
    {
        var f = new Fixture(); await f.Prepare(); var before = JsonSerializer.Serialize(f.Original.Lifecycle.Native.Rows);
        var writes = f.Original.Lifecycle.Native.Writes; var execution = JsonSerializer.Serialize(f.Original.Current);
        var review = await f.Review(); review.Require();
        Assert.Equal(1, review.Responsibility.Number); Assert.Equal(f.Original.Original.Revision(), review.Responsibility.Revision);
        Assert.Equal(JsonSerializer.Serialize(f.Original.Original.Workflow), JsonSerializer.Serialize(review.Responsibility.Workflow)); Assert.Equal("ReplacementReportingRole", review.Destination.Step.AssignedRole);
        Assert.Equal(new[] { "SITE-1", "SITE-2" }, review.Responsibility.Workflow.Step.PersonaCodes); Assert.Equal("NEW-SITE", Assert.Single(review.Destination.Step.PersonaCodes));
        Assert.Equal(2, f.ExecutionReads); Assert.Equal(3, f.DestinationReads); Assert.Equal(writes, f.Original.Lifecycle.Native.Writes);
        Assert.Equal(before, JsonSerializer.Serialize(f.Original.Lifecycle.Native.Rows)); Assert.Equal(execution, JsonSerializer.Serialize(f.Original.Current));
        Assert.All((await f.Original.Recorded()).Tasks, task => Assert.Equal("PENDING", task.TASK_STATUS));
        var command = new DrillingReportingReassignment(review, Guid.NewGuid(), DateTimeOffset.UtcNow, "Transfer reporting accountability to the configured new site", true);
        command.Require(); var copy = JsonSerializer.Deserialize<DrillingReportingReassignment>(JsonSerializer.Serialize(command))!;
        Assert.Equal(command.Revision(), copy.Revision()); Assert.Equal(command.HistoryId(), copy.HistoryId());
        Assert.Single(command.TaskIds()); Assert.DoesNotContain(command.TaskIds()[0], review.Responsibility.TaskIds());
    }

    [Fact]
    public async Task ChangedReceivingSetWithinSameWorkflowIsARealReplacementButAuthorityRevisionAloneIsNot()
    {
        var f = new Fixture(); await f.Prepare(); var original = f.Original.Original.Workflow;
        f.Destination = original with { Authority = original.Authority with { AuthorityRevision = new('D', 64) } };
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Review());
        f.Destination = original with { Step = original.Step with { PersonaCodes = original.Step.PersonaCodes.Reverse().ToArray() },
            Authority = original.Authority with { Recipients = original.Authority.Recipients.Reverse().ToArray(), AuthorityRevision = new('D', 64) } };
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Review());
        f.Destination = original with { Step = original.Step with { PersonaCodes = ["NEW-SITE"] },
            Authority = original.Authority with { Recipients = [new("new-reporter", "NEW-SITE")], AuthorityRevision = new('D', 64) } };
        Assert.Equal(original.ProcessId, (await f.Review()).Destination.ProcessId);
    }

    [Theory, InlineData("definition"), InlineData("recipient"), InlineData("execution"), InlineData("task"), InlineData("authority"), InlineData("target")]
    public async Task ChangedClosingWorkflowRecipientsSourceTaskAuthorityOrTargetRefusesWithoutMutation(string fault)
    {
        var f = new Fixture(); await f.Prepare(); var writes = f.Original.Lifecycle.Native.Writes;
        if (fault == "definition") f.OnDestination = call => { if (call == 2) f.Original.Lifecycle.Native.Rows.OfType<PROCESS_DEFINITION>().Single(row => row.PROCESS_DEFINITION_ID == f.Destination.ProcessId).PROCESS_NAME = "Changed during review"; };
        if (fault == "recipient") f.OnDestination = call => { if (call == 2) f.Destination = f.Destination with { Authority = f.Destination.Authority with { AuthorityRevision = new('F', 64) } }; };
        if (fault == "execution") f.OnExecution = call => { if (call == 2) f.Original.Current.Recorded!.STATUS = "Changed source"; };
        if (fault == "task") f.OnDestination = call => { if (call == 3) f.Original.Lifecycle.Native.Rows.OfType<CROSS_PERSONA_TASK>().First().TASK_CONTEXT_JSON = "Changed during closing destination proof"; };
        if (fault == "authority") f.OnGuard = () => { if (f.ExecutionReads > 0) throw new UnauthorizedAccessException("Management authority revoked"); };
        var changed = false;
        if (fault == "target") f.OnGuard = () => { if (!changed && f.ExecutionReads > 0) { changed = true; f.Original.Lifecycle.Native.Editor.Setup(editor => editor.GetDataSource("lifecycle")).Returns(new Mock<TheTechIdea.Beep.IDataSource>(MockBehavior.Strict).Object); } };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Review()); Assert.Equal(writes, f.Original.Lifecycle.Native.Writes);
        if (fault == "target") Assert.True(changed);
        Assert.DoesNotContain(f.Original.Lifecycle.Native.Rows.OfType<PROCESS_HISTORY>(), row => row.EVENT_TYPE == "REPORTING_TRANSFERRED");
    }

    [Theory, InlineData("missing-definition"), InlineData("inactive-definition"), InlineData("duplicate-definition"), InlineData("foreign-company"), InlineData("completed-task")]
    public async Task MissingAmbiguousInactiveForeignOrUnqualifiedCurrentEvidenceCannotPrepareTransfer(string fault)
    {
        var f = new Fixture(); await f.Prepare();
        var definition = f.Original.Lifecycle.Native.Rows.OfType<PROCESS_DEFINITION>().Single(row => row.PROCESS_DEFINITION_ID == f.Destination.ProcessId);
        if (fault == "missing-definition") f.Original.Lifecycle.Native.Rows.Remove(definition);
        if (fault == "inactive-definition") definition.IS_ACTIVE = "N";
        if (fault == "duplicate-definition") f.Original.Lifecycle.Native.Rows.Add(CompanyInventorySourceReview.Copy(definition));
        if (fault == "foreign-company")
        {
            await Assert.ThrowsAnyAsync<Exception>(() => f.Original.Lifecycle.Service.ReviewReportingReassignmentAsync(new("tenant", "foreign"),
                f.Original.Original.InstanceId(), f.Destination, f.ManagerRole, f.Original.Original.SenderPersonaCode, f.Original.Original.SenderAuthorityRevision,
                f.Guard, f.Execution, f.CurrentDestination)); return;
        }
        if (fault == "completed-task") f.Original.Lifecycle.Native.Rows.OfType<CROSS_PERSONA_TASK>().First().TASK_STATUS = "COMPLETED";
        var before = JsonSerializer.Serialize(f.Original.Lifecycle.Native.Rows); var writes = f.Original.Lifecycle.Native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Review()); Assert.Equal(before, JsonSerializer.Serialize(f.Original.Lifecycle.Native.Rows)); Assert.Equal(writes, f.Original.Lifecycle.Native.Writes);
    }

    [Theory, InlineData("unconfirmed"), InlineData("reason"), InlineData("offset"), InlineData("future"), InlineData("before-review"), InlineData("empty-intent"), InlineData("former-recipients"), InlineData("manager-role")]
    public async Task FrozenTransferRejectsMissingIntentUtcReasonConfirmationOrChangedReviewedResponsibility(string fault)
    {
        var f = new Fixture(); await f.Prepare(); var review = await f.Review();
        var command = new DrillingReportingReassignment(review, Guid.NewGuid(), DateTimeOffset.UtcNow, "Explicit accountable transfer", true);
        command = fault switch {
            "unconfirmed" => command with { Confirmed = false }, "reason" => command with { Reason = "" },
            "offset" => command with { ReassignedAtUtc = command.ReassignedAtUtc.ToOffset(TimeSpan.FromHours(3)) },
            "future" => command with { ReassignedAtUtc = DateTimeOffset.UtcNow.AddDays(1) },
            "before-review" => command with { ReassignedAtUtc = review.CheckedAtUtc.AddTicks(-1) },
            "empty-intent" => command with { IntentId = Guid.Empty },
            "former-recipients" => command with { Review = review with { Responsibility=review.Responsibility with { Workflow=review.Responsibility.Workflow with { Step=review.Responsibility.Workflow.Step with { PersonaCodes=[] } } } } },
            "manager-role" => command with { Review = review with { ManagerRole = "ForeignManager" } }, _ => command };
        Assert.Throws<InvalidOperationException>(command.Require);
    }

    [Fact]
    public async Task LegitimatelyClosedResponsibilityCannotBePreparedAsANewTransfer()
    {
        var f = new DrillingReportingClosureNativeTests.Fixture(); await f.Prepare(); await f.Close();
        var original = f.Original.Workflow;
        var destination = original with { Step = original.Step with { PersonaCodes = ["NEW-SITE"] },
            Authority = original.Authority with { Recipients = [new("new-reporter", "NEW-SITE")], AuthorityRevision = new('D', 64) } };
        var before = JsonSerializer.Serialize(f.Store.Rows); var writes = f.Accepted.Native.Mutations; var externalReads = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.ReviewReportingReassignmentAsync(f.Original.Scope().Company,
            f.Original.InstanceId(), destination, f.Original.SenderRole, f.Original.SenderPersonaCode, f.Original.SenderAuthorityRevision, f.Guard,
            () => { ++externalReads; return Task.FromResult(f.Execution); },
            () => { ++externalReads; return Task.FromResult(destination); }));
        Assert.Equal(0, externalReads); Assert.Equal(writes, f.Accepted.Native.Mutations); Assert.Equal(before, JsonSerializer.Serialize(f.Store.Rows));
    }
}
#endif
