#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyHseWorkflowStartTests
{
    [Theory]
    [InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)]
    public async Task RcaHandoffRollsBackEachLifecycleWriteFailure(int offset)
    {
        var f = new Fixture();
        f.Owner.Definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
            new ProcessStepDefinition { StepId = "INC_RCA", StepName = "Root cause", StepType = "ACTION", SequenceNumber = 1, NextStepId = "INC_CORRECTIVE_ACTIONS", RequiredRoles = ["HSEOfficer"] },
            new ProcessStepDefinition { StepId = "INC_CORRECTIVE_ACTIONS", StepName = "Corrective actions", StepType = "ACTION", SequenceNumber = 2, RequiredRoles = ["HSEOfficer"] } } });
        CompanyStepCommandTests.ConfigureUpdates(f.Owner); var start = await f.Original(); await f.Start(start);
        var originalWrites = f.Owner.Native.Writes;
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Owner.Service.CompleteStepAsync(start.InstanceId(), "INC_CORRECTIVE_ACTIONS", "COMPLETED", "actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Owner.Service.SkipStepAsync(start.InstanceId(), "INC_CLOSURE", "Skip closure", "actor"));
        Assert.Equal(originalWrites, f.Owner.Native.Writes);
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var cause = new IncidentRcaRecordedCause(start.Context.Company, start.Context.Database, start.Context.FieldId!, "incident", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('C', 64));
        Task Guard(string role) { Assert.Equal("HSEOfficer", role); return f.Owner.Guard(); }
        var review = await f.Owner.Service.ReviewRcaHandoffAsync(start.Context, start.LifecycleDatabase, cause, Guard);
        var command = new CompanyHseRcaHandoffCommand(Guid.NewGuid(), review); var before = JsonSerializer.Serialize(f.Owner.Native.Rows);
        f.Owner.FailWrite = f.Owner.Native.Writes + offset;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.CompleteRcaHandoffAsync(command, Guard, () => Task.CompletedTask));
        Assert.Equal(before, JsonSerializer.Serialize(f.Owner.Native.Rows)); Assert.Equal("rollback", f.Owner.Native.Transactions.Last());
    }

    [Theory]
    [InlineData("success")]
    [InlineData("lost-commit")]
    [InlineData("write-failure")]
    [InlineData("changed-review")]
    public async Task RcaHandoffCompletesOnlyReviewedStepAndReconcilesOriginal(string scenario)
    {
        var f = new Fixture();
        f.Owner.Definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
            new ProcessStepDefinition { StepId = "INC_RCA", StepName = "Root cause", StepType = "ACTION", SequenceNumber = 1, NextStepId = "INC_CORRECTIVE_ACTIONS", RequiredRoles = ["HSEOfficer"] },
            new ProcessStepDefinition { StepId = "INC_CORRECTIVE_ACTIONS", StepName = "Corrective actions", StepType = "ACTION", SequenceNumber = 2, RequiredRoles = ["HSEOfficer"] } } });
        CompanyStepCommandTests.ConfigureUpdates(f.Owner);
        var start = await f.Original(); await f.Start(start);
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var cause = new IncidentRcaRecordedCause(start.Context.Company, start.Context.Database, start.Context.FieldId!, "incident", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('C', 64));
        Task Guard(string role) { Assert.Equal("HSEOfficer", role); return f.Owner.Guard(); }
        var review = await f.Owner.Service.ReviewRcaHandoffAsync(start.Context, start.LifecycleDatabase, cause, Guard);
        var command = new CompanyHseRcaHandoffCommand(Guid.NewGuid(), review);
        Assert.Null(await f.Owner.Service.RecoverRcaHandoffAsync(start.Context, start.LifecycleDatabase, "incident", Guard, _ => Task.CompletedTask));
        if (scenario == "write-failure") f.Owner.FailWrite = f.Owner.Native.Writes + 2;
        if (scenario == "changed-review") command = command with { Review = review with { GraphRevision = new string('D', 64) } };
        if (scenario == "lost-commit") f.Owner.Native.FailCommit = true;
        var before = JsonSerializer.Serialize(f.Owner.Native.Rows);
        if (scenario is "write-failure" or "changed-review")
        {
            await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.CompleteRcaHandoffAsync(command, Guard, () => Task.CompletedTask));
            Assert.Equal(before, JsonSerializer.Serialize(f.Owner.Native.Rows)); return;
        }
        if (scenario == "lost-commit")
        {
            await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.CompleteRcaHandoffAsync(command, Guard, () => Task.CompletedTask));
            f.Owner.Native.FailCommit = false;
        }
        var receipt = await f.Owner.Service.CompleteRcaHandoffAsync(command, Guard, () => Task.CompletedTask); receipt.Require(command);
        var writes = f.Owner.Native.Writes;
        var retry = await f.Owner.Service.CompleteRcaHandoffAsync(command, Guard, () => Task.CompletedTask);
        Assert.Equal(JsonSerializer.Serialize(receipt), JsonSerializer.Serialize(retry)); Assert.Equal(writes, f.Owner.Native.Writes);
        var recovered = await f.Owner.Service.RecoverRcaHandoffAsync(start.Context with { Revision = "new-review" }, start.LifecycleDatabase, "incident", Guard,
            item => { Assert.Equal(JsonSerializer.Serialize(cause), JsonSerializer.Serialize(item)); return Task.CompletedTask; });
        Assert.NotNull(recovered); recovered.Require(); Assert.Equal(JsonSerializer.Serialize(command), JsonSerializer.Serialize(recovered.Original)); Assert.Equal(writes, f.Owner.Native.Writes);
        await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.RecoverRcaHandoffAsync(start.Context with { ActorUserId = "other" }, start.LifecycleDatabase, "incident", Guard, _ => Task.CompletedTask));
        Assert.Equal(writes, f.Owner.Native.Writes);
        var process = await f.Owner.Service.GetProcessInstanceAsync(start.InstanceId());
        Assert.Equal("INC_CORRECTIVE_ACTIONS", process!.CurrentStepId);
        Assert.Equal(StepStatus.COMPLETED, process.StepInstances.Single(step => step.StepId == "INC_RCA").Status);
        Assert.Equal(StepStatus.PENDING, process.StepInstances.Single(step => step.StepId == "INC_CORRECTIVE_ACTIONS").Status);
        if (scenario == "success")
        {
            // Model later work by the next assignee without rewriting the original handoff.
            var nextStep = f.Owner.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(step => step.STEP_ID == "INC_CORRECTIVE_ACTIONS");
            nextStep.STATUS = "IN_PROGRESS";
            var nextTask = f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().First(task => receipt.NextTasks.Contains(task.CROSS_TASK_ID));
            nextTask.TASK_STATUS = "IN_PROGRESS"; nextTask.ROW_CHANGED_BY = "next-user"; nextTask.ROW_CHANGED_DATE = DateTime.UtcNow;
            var progressed = await f.Owner.Service.RecoverRcaHandoffAsync(start.Context, start.LifecycleDatabase, "incident", Guard, _ => Task.CompletedTask);
            Assert.NotNull(progressed); progressed.Require();
            Assert.Equal(JsonSerializer.Serialize(receipt), JsonSerializer.Serialize(progressed.Receipt));
            Assert.Equal(writes, f.Owner.Native.Writes);
            var reconciled = await f.Owner.Service.CompleteRcaHandoffAsync(command, Guard, () => Task.CompletedTask);
            Assert.Equal(JsonSerializer.Serialize(receipt), JsonSerializer.Serialize(reconciled)); Assert.Equal(writes, f.Owner.Native.Writes);
            nextStep.STATUS = "COMPLETED";
            foreach (var task in f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(task => receipt.NextTasks.Contains(task.CROSS_TASK_ID)))
            { task.TASK_STATUS = "COMPLETED"; task.ROW_CHANGED_BY = "next-user"; task.ROW_CHANGED_DATE = DateTime.UtcNow; }
            var completed = await f.Owner.Service.RecoverRcaHandoffAsync(start.Context, start.LifecycleDatabase, "incident", Guard, _ => Task.CompletedTask);
            Assert.NotNull(completed); Assert.Equal(JsonSerializer.Serialize(receipt), JsonSerializer.Serialize(completed.Receipt)); Assert.Equal(writes, f.Owner.Native.Writes);
            nextTask.TARGET_PERSONA_CODE = "wrong-persona";
            await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.RecoverRcaHandoffAsync(start.Context, start.LifecycleDatabase, "incident", Guard, _ => Task.CompletedTask));
            Assert.Equal(writes, f.Owner.Native.Writes);
        }
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("wrong-step")]
    [InlineData("task-changed")]
    [InlineData("role-revoked")]
    [InlineData("other-assignee")]
    [InlineData("earlier-incomplete")]
    [InlineData("no-next")]
    [InlineData("next-persona-unavailable")]
    public async Task RcaHandoffReviewsOnlyCurrentAssignedStep(string scenario)
    {
        var f = new Fixture();
        if (scenario == "earlier-incomplete")
            f.Owner.Definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
                new ProcessStepDefinition { StepId = "report", StepName = "Incident report", StepType = "ACTION", SequenceNumber = 1, RequiredRoles = ["HSEOfficer"] },
                new ProcessStepDefinition { StepId = "INC_RCA", StepName = "Root cause", StepType = "ACTION", SequenceNumber = 2, NextStepId = "INC_CORRECTIVE_ACTIONS", RequiredRoles = ["HSEOfficer"] },
                new ProcessStepDefinition { StepId = "INC_CORRECTIVE_ACTIONS", StepName = "Corrective actions", StepType = "ACTION", SequenceNumber = 3, RequiredRoles = ["HSEOfficer"] } } });
        else if (scenario != "wrong-step")
            f.Owner.Definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
                new ProcessStepDefinition { StepId = "INC_RCA", StepName = "Root cause", StepType = "ACTION", SequenceNumber = 1, NextStepId = scenario == "no-next" ? "" : "INC_CORRECTIVE_ACTIONS", RequiredRoles = ["HSEOfficer"] },
                new ProcessStepDefinition { StepId = "INC_CORRECTIVE_ACTIONS", StepName = "Corrective actions", StepType = "ACTION", SequenceNumber = 2, RequiredRoles = [scenario == "next-persona-unavailable" ? "NextRole" : "HSEOfficer"] } } });
        if (scenario == "next-persona-unavailable") f.Owner.Native.Personas.Setup(value => value.GetActivePersonasAsync("NextRole")).ReturnsAsync([]);
        var start = await f.Original(); await f.Start(start);
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var cause = new IncidentRcaRecordedCause(start.Context.Company, start.Context.Database, start.Context.FieldId!, "incident", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('C', 64));
        if (scenario == "task-changed") f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().First().TASK_STATUS = "COMPLETED";
        if (scenario == "other-assignee") f.Owner.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(step => step.STEP_ID == "INC_RCA").ASSIGNED_TO = "other";
        if (scenario == "earlier-incomplete")
        {
            f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>().Single().CURRENT_STEP_ID = "INC_RCA";
            f.Owner.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(step => step.STEP_ID == "INC_RCA").STATUS = "PENDING";
        }
        Task Guard(string role)
        {
            Assert.Equal("HSEOfficer", role);
            if (scenario == "role-revoked") throw new UnauthorizedAccessException();
            return f.Owner.Guard();
        }
        var writes = f.Owner.Native.Writes;
        if (scenario == "valid")
        {
            var review = await f.Owner.Service.ReviewRcaHandoffAsync(start.Context, start.LifecycleDatabase, cause, Guard);
            review.Require(); Assert.Equal(start.InstanceId(), review.InstanceId); Assert.Equal("HSEOfficer", review.AssignedRole);
        }
        else await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.ReviewRcaHandoffAsync(start.Context, start.LifecycleDatabase, cause, Guard));
        Assert.Equal(writes, f.Owner.Native.Writes);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("write-failure")]
    [InlineData("lost-commit")]
    [InlineData("changed-review")]
    [InlineData("self-review")]
    [InlineData("independent-reviewer")]
    [InlineData("independent-reviewer-denied")]
    [InlineData("correction")][InlineData("correction-twice")][InlineData("correction-retagged-parent")][InlineData("correction-no-work")][InlineData("correction-altered-work")][InlineData("correction-predated-work")][InlineData("correction-denied")][InlineData("correction-write-failure")][InlineData("correction-drift")]
    [InlineData("source-drift-before-write")][InlineData("source-drift-after-commit")][InlineData("correction-source-drift-before-write")][InlineData("correction-source-drift-after-commit")]
    public async Task HseClosureStagesConfirmIndependentApprovalAndTerminalGraph(string scenario)
    {
        var f = new Fixture();
        f.Owner.Definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
            new ProcessStepDefinition { StepId = "INC_RCA", StepName = "Recorded root cause", StepType = "ACTION", SequenceNumber = 0, NextStepId = "INC_CORRECTIVE_ACTIONS", RequiredRoles = ["HSEOfficer"] },
            new ProcessStepDefinition { StepId = "INC_CORRECTIVE_ACTIONS", StepName = "Corrective actions", StepType = "ACTION", SequenceNumber = 1, NextStepId = "INC_VERIFICATION", RequiredRoles = ["HSEOfficer"] },
            new ProcessStepDefinition { StepId = "INC_VERIFICATION", StepName = "Independent verification", StepType = "APPROVAL", RequiresApproval = true, SequenceNumber = 2, NextStepId = "INC_CLOSURE", RequiredRoles = ["HSEOfficer"] },
            new ProcessStepDefinition { StepId = "INC_CLOSURE", StepName = "Final closure", StepType = "APPROVAL", RequiresApproval = true, SequenceNumber = 3, NextStepId = "", RequiredRoles = ["HSEOfficer"] } } });
        CompanyStepCommandTests.ConfigureUpdates(f.Owner); var start = await f.Original(); await f.Start(start);
        var action = new CAVerificationAction(1, "action", "Valve tested", "COMPLETED", "operator", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), scenario == "self-review" ? "actor" : "operator", "Valve inspected and tested");
        var actions = new CAVerificationReview(start.Context, "incident", "plan", new string('C', 64), [action]);
        var verification = new CAVerificationDecision(Guid.NewGuid(), actions, "VERIFIED", "Independent inspection confirmed action effectiveness.");
        var evidence = new IncidentClosureReview(actions, scenario == "self-review" || scenario.StartsWith("correction", StringComparison.Ordinal) ? [] : [verification]);
        Task Role(string role) { Assert.Equal("HSEOfficer", role); return f.Owner.Guard(); }
        Task RecordedSource() { PPDMDataSourceTransaction.RequireIdle(f.Owner.Native.Source.Object); return Task.CompletedTask; }
        var cause = new IncidentRcaRecordedCause(start.Context.Company, start.Context.Database, start.Context.FieldId!, "incident", Guid.NewGuid(), 1, "PROCESS", "Recorded cause for this incident.", "another-author", new string('F', 64));
        var rcaReview = await f.Owner.Service.ReviewRcaHandoffAsync(start.Context, start.LifecycleDatabase, cause, Role);
        await f.Owner.Service.CompleteRcaHandoffAsync(new(Guid.NewGuid(), rcaReview), Role, RecordedSource);
        CompanyHseClosureCommand? terminal = null; CompanyHseClosureReceipt? terminalReceipt = null; var correctionHandled = false; var correctionCount = 0; CompanyHseCorrectionCommand? retainedReturn = null;
        for (var i = 0; i < 3; i++)
        {
            if (scenario.StartsWith("correction", StringComparison.Ordinal) && i == 1 && (!correctionHandled || scenario == "correction-twice" && correctionCount < 2))
            {
                var rejection = verification with { Decision = "REJECTED", Reason = "Independent inspection found the valve repair ineffective." };
                var rejectedDecisions = evidence.Decisions.Where(d => d.Decision == "REJECTED").Append(rejection).ToArray();
                evidence = new(actions, rejectedDecisions);
                var returnReview = await f.Owner.Service.ReviewCorrectionReturnAsync(evidence, start.LifecycleDatabase, new string('D', 64), Role);
                var returnCommand = new CompanyHseCorrectionCommand(Guid.NewGuid(), returnReview, "Return to corrective work after independent inspection rejected the repair.");
                var originalRows = JsonSerializer.Serialize(f.Owner.Native.Rows);
                if (scenario == "correction-denied")
                {
                    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Owner.Service.ReturnForCorrectionAsync(returnCommand, _ => throw new UnauthorizedAccessException(), RecordedSource, RecordedSource));
                    Assert.Equal(originalRows, JsonSerializer.Serialize(f.Owner.Native.Rows)); return;
                }
                if (scenario == "correction-drift")
                {
                    await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.ReturnForCorrectionAsync(returnCommand with { Review = returnReview with { GraphRevision = new string('E', 64) } }, Role, RecordedSource, RecordedSource));
                    Assert.Equal(originalRows, JsonSerializer.Serialize(f.Owner.Native.Rows)); return;
                }
                if (scenario == "correction-write-failure")
                {
                    f.Owner.FailWrite = f.Owner.Native.Writes + 3;
                    await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.ReturnForCorrectionAsync(returnCommand, Role, RecordedSource, RecordedSource));
                    Assert.Equal(originalRows, JsonSerializer.Serialize(f.Owner.Native.Rows)); f.Owner.FailWrite = 0;
                }
                if (scenario is "correction-source-drift-before-write" or "correction-source-drift-after-commit")
                {
                    var checks = 0;
                    Task CurrentSource()
                    {
                        PPDMDataSourceTransaction.RequireIdle(f.Owner.Native.Source.Object);
                        if (++checks == (scenario == "correction-source-drift-before-write" ? 1 : 2)) throw new InvalidOperationException("Changed native corrective-action source.");
                        return Task.CompletedTask;
                    }
                    await Assert.ThrowsAsync<InvalidOperationException>(() => f.Owner.Service.ReturnForCorrectionAsync(returnCommand, Role, CurrentSource, RecordedSource));
                    if (scenario == "correction-source-drift-before-write") { Assert.Equal(originalRows, JsonSerializer.Serialize(f.Owner.Native.Rows)); return; }
                    var confirmedWrites = f.Owner.Native.Writes;
                    (await f.Owner.Service.ReturnForCorrectionAsync(returnCommand, Role, () => throw new InvalidOperationException("The current action source has changed."), RecordedSource)).Require();
                    Assert.Equal(confirmedWrites, f.Owner.Native.Writes);
                }
                retainedReturn ??= returnCommand;
                var returned = await f.Owner.Service.ReturnForCorrectionAsync(returnCommand, Role, RecordedSource, RecordedSource); returned.Require();
                var returnWrites = f.Owner.Native.Writes;
                Assert.Equal(JsonSerializer.Serialize(returned), JsonSerializer.Serialize(await f.Owner.Service.ReturnForCorrectionAsync(returnCommand, Role, RecordedSource, RecordedSource)));
                Assert.Equal(returnWrites, f.Owner.Native.Writes);
                if (scenario == "correction-retagged-parent")
                {
                    f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>().Single().ENTITY_ID = "another-incident";
                    var retagged = JsonSerializer.Serialize(f.Owner.Native.Rows); var retaggedWrites = f.Owner.Native.Writes;
                    await Assert.ThrowsAsync<InvalidOperationException>(() => f.Owner.Service.ReturnForCorrectionAsync(returnCommand, Role, () => throw new InvalidOperationException("Current evidence changed"), RecordedSource));
                    Assert.Equal(retaggedWrites, f.Owner.Native.Writes); Assert.Equal(retagged, JsonSerializer.Serialize(f.Owner.Native.Rows)); return;
                }
                await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.ReviewClosureStageAsync(evidence, start.LifecycleDatabase, new string('D', 64), Role));
                Assert.Equal("INC_CORRECTIVE_ACTIONS", (await f.Owner.Service.GetProcessInstanceAsync(start.InstanceId())).CurrentStepId);
                Assert.Equal(correctionCount + 1, f.Owner.Native.Rows.OfType<PROCESS_APPROVAL>().Count(a => a.APPROVAL_STATUS == "REJECTED"));
                Assert.All(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(t => returned.CompletedTasks.Contains(t.CROSS_TASK_ID)), t => Assert.Equal("COMPLETED", t.TASK_STATUS));
                actions = actions with { Revision = new string((char)('A' + correctionCount), 64), Actions = actions.Actions.Append(action with { StepSequence = correctionCount + 2, StepId = "repair-" + (correctionCount + 2), CompletedAt = DateTime.UtcNow, CompletionEvidence = "Replacement valve independently pressure tested" }).ToArray() };
                verification = new CAVerificationDecision(Guid.NewGuid(), actions, "VERIFIED", "Independent pressure test confirmed replacement valve operation.");
                if (scenario is "correction-no-work" or "correction-altered-work" or "correction-predated-work")
                {
                    actions = actions with { Actions = scenario == "correction-no-work" ? [action] : scenario == "correction-altered-work" ? [action with { Description = "Changed old rejected evidence" }, actions.Actions.Last()] : [action, actions.Actions.Last() with { CompletedAt = DateTime.UtcNow.AddHours(-1) }] };
                    var invalidEvidence = new IncidentClosureReview(actions, [rejection, verification with { Review = actions }]);
                    var unchanged = JsonSerializer.Serialize(f.Owner.Native.Rows);
                    await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.ReviewClosureStageAsync(invalidEvidence, start.LifecycleDatabase, new string('D', 64), Role));
                    Assert.Equal(unchanged, JsonSerializer.Serialize(f.Owner.Native.Rows)); return;
                }
                correctionCount++; evidence = new(actions, scenario == "correction-twice" && correctionCount < 2 ? rejectedDecisions : rejectedDecisions.Append(verification).ToArray()); correctionHandled = true; i = -1; continue;
            }
            if (scenario.StartsWith("independent-reviewer", StringComparison.Ordinal) && i == 1)
            {
                actions = actions with { Context = start.Context with { ActorUserId = "independent-reviewer" } };
                verification = verification with { Review = actions };
                evidence = new(actions, [verification]);
            }
            if (scenario == "self-review" && i == 1) { await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.ReviewClosureStageAsync(evidence, start.LifecycleDatabase, new string('D', 64), Role)); return; }
            var review = await f.Owner.Service.ReviewClosureStageAsync(evidence, start.LifecycleDatabase, new string('D', 64), Role);
            var command = new CompanyHseClosureCommand(Guid.NewGuid(), review, "Reviewed evidence and approve this assigned incident stage.");
            var before = JsonSerializer.Serialize(f.Owner.Native.Rows);
            if (scenario == "independent-reviewer-denied" && i == 1)
            {
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Owner.Service.CompleteClosureStageAsync(command, _ => throw new UnauthorizedAccessException("Current assigned role revoked"), RecordedSource));
                Assert.Equal(before, JsonSerializer.Serialize(f.Owner.Native.Rows)); return;
            }
            if (scenario == "changed-review" && i == 0)
            {
                await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.CompleteClosureStageAsync(command with { Review = review with { GraphRevision = new string('E', 64) } }, Role, RecordedSource));
                Assert.Equal(before, JsonSerializer.Serialize(f.Owner.Native.Rows)); return;
            }
            if (scenario is "source-drift-before-write" or "source-drift-after-commit" && i == 0)
            {
                var checks = 0;
                Task CurrentSource()
                {
                    PPDMDataSourceTransaction.RequireIdle(f.Owner.Native.Source.Object);
                    if (++checks == (scenario == "source-drift-before-write" ? 1 : 2)) throw new InvalidOperationException("Changed native action evidence.");
                    return Task.CompletedTask;
                }
                await Assert.ThrowsAsync<InvalidOperationException>(() => f.Owner.Service.CompleteClosureStageAsync(command, Role, CurrentSource));
                if (scenario == "source-drift-before-write") { Assert.Equal(before, JsonSerializer.Serialize(f.Owner.Native.Rows)); return; }
                var confirmedWrites = f.Owner.Native.Writes;
                (await f.Owner.Service.CompleteClosureStageAsync(command, Role, RecordedSource)).Require(command);
                Assert.Equal(confirmedWrites, f.Owner.Native.Writes);
            }
            if (scenario == "write-failure" && i == 2)
            {
                f.Owner.FailWrite = f.Owner.Native.Writes + 2;
                await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.CompleteClosureStageAsync(command, Role, RecordedSource));
                Assert.Equal(before, JsonSerializer.Serialize(f.Owner.Native.Rows)); f.Owner.FailWrite = 0;
            }
            if (scenario == "lost-commit" && i == 2)
            {
                f.Owner.Native.FailCommit = true;
                await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.CompleteClosureStageAsync(command, Role, RecordedSource)); f.Owner.Native.FailCommit = false;
            }
            var receipt = await f.Owner.Service.CompleteClosureStageAsync(command, Role, RecordedSource); receipt.Require(command);
            var writes = f.Owner.Native.Writes;
            Assert.Equal(JsonSerializer.Serialize(receipt), JsonSerializer.Serialize(await f.Owner.Service.CompleteClosureStageAsync(command, Role, RecordedSource)));
            Assert.Equal(writes, f.Owner.Native.Writes); terminal = command; terminalReceipt = receipt;
        }
        var process = await f.Owner.Service.GetProcessInstanceAsync(start.InstanceId()); Assert.Equal(ProcessStatus.COMPLETED, process.Status);
        Assert.All(process.StepInstances, step => Assert.Equal(StepStatus.COMPLETED, step.Status));
        Assert.Equal(correctionCount + 2, f.Owner.Native.Rows.OfType<PROCESS_APPROVAL>().Count());
        if (retainedReturn is not null)
        {
            var archivedWrites = f.Owner.Native.Writes;
            (await f.Owner.Service.ReturnForCorrectionAsync(retainedReturn, Role, () => throw new InvalidOperationException("The actions have progressed beyond the rejected source."), RecordedSource)).Require();
            Assert.Equal(archivedWrites, f.Owner.Native.Writes);
        }
        var finalWrites = f.Owner.Native.Writes;
        var recovered = await f.Owner.Service.RecoverClosureStageAsync(evidence.Actions.Context with { Revision = "new-review" }, start.LifecycleDatabase, "incident", Role, _ => Task.CompletedTask);
        Assert.NotNull(recovered); recovered.Require(); Assert.Equal(JsonSerializer.Serialize(terminalReceipt), JsonSerializer.Serialize(recovered.Receipt));
        Assert.Equal(finalWrites, f.Owner.Native.Writes);
        if (scenario == "independent-reviewer")
        {
            Assert.Equal("actor", process.StartedBy); Assert.Equal("independent-reviewer", process.CompletedBy);
            Assert.All(f.Owner.Native.Rows.OfType<PROCESS_APPROVAL>(), row => Assert.Equal("independent-reviewer", row.APPROVED_BY));
        }
        await Assert.ThrowsAnyAsync<Exception>(() => f.Owner.Service.CompleteClosureStageAsync(terminal! with { IntentId = Guid.NewGuid() }, Role, RecordedSource));
        Assert.Equal(finalWrites, f.Owner.Native.Writes);
    }

    private sealed class Fixture
    {
        internal readonly CompanyProcessStartTests.Fixture Owner = new("HSEOfficer");
        internal string Revision = new('A', 64);
        internal Fixture()
        {
            Owner.Definition.PROCESS_DEFINITION_ID = CompanyHseWorkflowStart.ProcessId;
            Owner.Definition.PROCESS_NAME = "Incident reporting";
            Owner.Definition.PROCESS_TYPE = "HSE";
            Owner.Definition.ENTITY_TYPE = CompanyHseWorkflowStart.EntityType;
            Owner.Definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
                new ProcessStepDefinition { StepId = "report", StepName = "Incident review", StepType = "ACTION", SequenceNumber = 1, RequiredRoles = ["HSEOfficer"] },
                new ProcessStepDefinition { StepId = "investigate", StepName = "Investigation", StepType = "ACTION", SequenceNumber = 2, RequiredRoles = ["HSEOfficer"] } } });
        }
        internal async Task<CompanyHseWorkflowStart> Original()
        {
            var definition = await Owner.Service.GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId);
            return new(new(new("tenant", "company"), new("core", "core-binding"), "actor", "review", "field"),
                new("lifecycle", "lifecycle-binding"), Guid.NewGuid(), "incident", Revision, ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)));
        }
        internal Task<string> Read()
        {
            PPDMDataSourceTransaction.RequireIdle(Owner.Native.Source.Object);
            return Task.FromResult(Revision);
        }
        internal Task<CompanyHseWorkflowReceipt> Start(CompanyHseWorkflowStart intent) => Owner.Service.StartHseWorkflowAsync(intent, Owner.Guard, Read);
    }

    [Fact]
    public async Task StartCommitsTheReviewedProcessHistoryAndPersonaTasksOnce()
    {
        var f = new Fixture(); var original = await f.Original();
        var receipt = await f.Start(original); receipt.Require(original);
        Assert.Equal(6, f.Owner.Native.Writes);
        Assert.Equal(new[] { "begin", "commit" }, f.Owner.Native.Transactions);
        var process = Assert.Single(f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>());
        Assert.Equal("REPORTED", process.CURRENT_STATE);
        Assert.Equal(original, CompanyHseWorkflowStart.FromPayload(process.INSTANCE_DATA_JSON!, process.PROCESS_INSTANCE_ID!, "field", "incident"));
        Assert.Single(f.Owner.Native.Rows.OfType<PROCESS_HISTORY>());
        Assert.Equal(2, f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().Count());
        var retry = await f.Start(original);
        Assert.Equal(receipt.GraphRevision, retry.GraphRevision);
        Assert.Equal(6, f.Owner.Native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Owner.Native.Router.RouteTaskAsync(receipt.InstanceId,
            receipt.StepInstanceId, "HSEOfficer", CompanyHseWorkflowStart.EntityType, original.IncidentId, null, original.Context.ActorUserId));
        Assert.Equal(6, f.Owner.Native.Writes);
    }

    [Fact]
    public async Task FailedGraphWriteRollsBackAndChangedIncidentRefusesBeforeWriting()
    {
        var f = new Fixture(); var original = await f.Original();
        f.Owner.FailWrite = 3;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start(original));
        Assert.Empty(f.Owner.Native.Rows.OfType<PROCESS_INSTANCE>());
        Assert.Empty(f.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>());
        Assert.Equal(new[] { "begin", "rollback" }, f.Owner.Native.Transactions);
        f.Owner.FailWrite = 0; f.Revision = new('B', 64); var writes = f.Owner.Native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start(original));
        Assert.Equal(writes, f.Owner.Native.Writes);
    }

    [Fact]
    public async Task UnconfirmedCommitReconcilesOnlyTheOriginalHseReview()
    {
        var f = new Fixture(); var original = await f.Original();
        f.Owner.Native.FailCommit = true;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start(original));
        f.Owner.Native.FailCommit = false;
        var writes = f.Owner.Native.Writes;
        var foreign = original with { IntentId = Guid.NewGuid(), Context = original.Context with { ActorUserId = "other" } };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start(foreign));
        Assert.Equal(writes, f.Owner.Native.Writes);
        var receipt = await f.Start(original); receipt.Require(original);
        Assert.Equal(writes, f.Owner.Native.Writes);
    }

    [Fact]
    public async Task ChangedWorkflowStartHistoryIsNotAcceptedAsRecovery()
    {
        var f = new Fixture(); var original = await f.Original(); await f.Start(original);
        var writes = f.Owner.Native.Writes;
        f.Owner.Native.Rows.OfType<PROCESS_HISTORY>().Single().TO_STATUS = "CLOSED";
        await Assert.ThrowsAnyAsync<Exception>(() => f.Start(original));
        Assert.Equal(writes, f.Owner.Native.Writes);
    }
}
#endif
