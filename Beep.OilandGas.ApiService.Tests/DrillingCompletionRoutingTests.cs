using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingCompletionRoutingTests
{
    private static readonly Lazy<Task<DrillingDailyReportHistory>> source = new(CreateSource);
    private static async Task<DrillingDailyReportHistory> CreateSource()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f);
        f.Command = DrillingCompletionReportNativeTests.Complete(f, rows[2]); await f.Save();
        return await f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard);
    }
    internal static async Task<DrillingDailyReportHistory> Source() => CompanyInventorySourceReview.Copy(await source.Value);
    internal static DrillingCompletionReviewReadiness Review(DrillingDailyReportHistory history, string mode = "ready")
    {
        var recipients = mode == "blocked" ? Array.Empty<RepositoryCompanyRoleRecipient>() : [new RepositoryCompanyRoleRecipient("completion-inspector", "INDEPENDENT_COMPLETION")];
        var stage = new DrillingCompletionReviewStage(new("inspect", "Independent completion and test review", 1, "ConfiguredCompletionReviewer",
            recipients.Select(row => row.PersonaCode).Distinct().ToArray(), 24), new("inspect", "ConfiguredCompletionReviewer", recipients, new('A', 64)));
        var option = new DrillingCompletionReviewOption("completion-review", "Company completion review", "1.0", new('B', 64), [stage]);
        var value = new DrillingCompletionReviewReadiness(history, mode is "empty" or "reader" ? [] : [option],
            mode == "reader" ? [] : ["ConfiguredExecutionRecorder"], mode == "reader" ? null : "DRILLING_RECORDER", mode == "reader" ? null : new('C', 64), "", DateTimeOffset.UtcNow);
        value = value with { Revision = value.ContentRevision() }; var scope = history.Execution.Basis.Scope();
        value.Require(scope.Company, scope.ActorUserId, history.Execution.Basis.ReviewInstanceId); return value;
    }
    [Theory, InlineData("ready"), InlineData("blocked"), InlineData("empty"), InlineData("reader")]
    public async Task ConfiguredReadinessDoesNotAcceptReportedTestsCloseResponsibilityOrChangeExecution(string mode)
    {
        var history = await Source(); var before = JsonSerializer.Serialize(history); var value = Review(history, mode);
        if (mode is "empty" or "reader") Assert.Empty(value.Options); else Assert.Equal(mode == "ready", value.Options.Single().Ready);
        Assert.Equal(before, JsonSerializer.Serialize(history)); Assert.Equal("COMPLETION_REPORTED", value.Source.ProgressState);
        Assert.Equal("STARTED", value.Source.Execution.Recorded!.STATUS);
        var roundtrip = JsonSerializer.Deserialize<DrillingCompletionReviewReadiness>(JsonSerializer.Serialize(value))!;
        var scope = history.Execution.Basis.Scope(); roundtrip.Require(scope.Company, scope.ActorUserId, history.Execution.Basis.ReviewInstanceId);
    }
    [Theory, InlineData("claimant"), InlineData("sender"), InlineData("duplicate"), InlineData("workspace"), InlineData("role"),
        InlineData("stage-order"), InlineData("repeated-role"), InlineData("empty-stages"), InlineData("source"), InlineData("company"),
        InlineData("actor"), InlineData("review"), InlineData("sender-authority"), InlineData("reader-private-options"), InlineData("timestamp")]
    public async Task CurrentClaimIdentityIndependentRecipientsAndExactConfiguredRoutingCannotBeRehashedAway(string fault)
    {
        var value = Review(await Source()); var option = value.Options.Single(); var stage = option.Stages.Single(); var receiver = stage.Authority.Recipients.Single();
        var scope = value.Source.Execution.Basis.Scope();
        if (fault is "claimant" or "sender" or "duplicate" or "workspace" or "role")
        {
            var rows = fault switch {
                "claimant" => new[] { receiver with { UserId = value.Source.ReportedCompletion!.RecorderUserId } },
                "sender" => new[] { receiver with { UserId = scope.ActorUserId } },
                "duplicate" => new[] { receiver, receiver }, "workspace" => new[] { receiver with { PersonaCode = "FOREIGN" } }, _ => new[] { receiver } };
            stage = stage with { Authority = stage.Authority with { Recipients = rows, AssignedRole = fault == "role" ? "ForeignRole" : stage.Authority.AssignedRole } };
            value = value with { Options = [option with { Stages = [stage] }] };
        }
        if (fault is "stage-order" or "repeated-role")
        {
            var later = stage with { Definition = stage.Definition with { StepId = "later", Sequence = 2 }, Authority = stage.Authority with { StepId = "later", AuthorityRevision = fault == "repeated-role" ? new('F', 64) : stage.Authority.AuthorityRevision } };
            value = value with { Options = [option with { Stages = fault == "stage-order" ? [later, stage] : [stage, later] }] };
        }
        if (fault == "empty-stages") value = value with { Options = [option with { Stages = [] }] };
        if (fault == "source")
        {
            var changed = value.Source with { ReportedCompletion = null };
            value = value with { Source = changed with { Revision = changed.ContentRevision() } };
        }
        if (fault == "sender-authority") value = value with { SenderAuthorityRevision = "invalid" };
        if (fault == "reader-private-options") value = value with { SenderRoles = [], SenderPersonaCode = null, SenderAuthorityRevision = null };
        if (fault == "timestamp") value = value with { CheckedAtUtc = value.Source.CheckedAtUtc.AddTicks(-1) };
        value = value with { Revision = value.ContentRevision() };
        Assert.ThrowsAny<Exception>(() => value.Require(fault == "company" ? new("tenant", "foreign") : scope.Company,
            fault == "actor" ? "foreign-account" : scope.ActorUserId, fault == "review" ? "foreign-review" : value.Source.Execution.Basis.ReviewInstanceId));
    }
    [Theory, InlineData("valid"), InlineData("operation"), InlineData("type"), InlineData("optional"), InlineData("next"), InlineData("conditional"), InlineData("automatic"), InlineData("two-roles")]
    public void CompletionRequiresExplicitOrderedMandatoryApprovalOnTheCanonicalReportEntity(string fault)
    {
        var step = new ProcessStepDefinition { StepId = "inspect", StepName = "Independent completion inspection", SequenceNumber = 1, StepType = "APPROVAL", RequiresApproval = true, RequiredRoles = ["ConfiguredCompletionReviewer"] };
        var definition = new ProcessDefinition { ProcessId = "completion-review", ProcessName = "Company completion review", Version = "1.0", ProcessType = fault == "type" ? "DEVELOPMENT" : "DRILLING", EntityType = fault == "operation" ? "DRILLING_OPERATION" : "DRILLING_DAILY_REPORT", IsActive = true, Steps = [step] };
        if (fault == "optional") step.IsRequired = false; if (fault == "next") step.NextStepId = "guessed";
        if (fault == "conditional") step.ConditionalNextSteps = ["branch"]; if (fault == "automatic") step.StepConfiguration = new() { ["AutoApprove"] = true };
        if (fault == "two-roles") step.RequiredRoles.Add("OtherRole");
        if (fault == "valid") Assert.Single(DevelopmentReviewDefinition.RequireCompletion(definition)); else Assert.Throws<InvalidOperationException>(() => DevelopmentReviewDefinition.RequireCompletion(definition));
    }
}
#endif
