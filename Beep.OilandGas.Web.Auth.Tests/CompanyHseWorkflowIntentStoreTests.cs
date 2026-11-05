using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.HSE.Models;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed partial class CompanyHseWorkflowIntentStoreTests
{
    [Fact]
    public async Task ProtectedClosurePreservesExactApprovalAcrossFieldReviewAndRejectsReplacement()
    {
        var tab = new Tab(); var protection = new EphemeralDataProtectionProvider(); ICorrectiveActionIntentStore Store() => new CorrectiveActionIntentStore(new ProtectedSessionStorage(tab, protection));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "other", "Private valve inspection");
        var actions = new CAVerificationReview(context, "incident", "plan", new string('A', 64), [action]); var decision = new CAVerificationDecision(Guid.NewGuid(), actions, "VERIFIED", "Independent private inspection confirmed the repair.");
        var original = new CompanyHseClosureCommand(Guid.NewGuid(), new(new(actions, [decision]), new("lifecycle", "binding"), "instance", "step-instance", "INC_CLOSURE", "Safety", new string('B', 64), new string('C', 64)), "Private final closure approval evidence.");
        await Store().RetainClosureAsync(original); Assert.DoesNotContain(original.Reason, Assert.Single(tab.Values).Value);
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(await Store().ReadClosureAsync(context with { Revision = "new-review" }, "incident")));
        Assert.Null(await Store().ReadClosureAsync(context with { ActorUserId = "other" }, "incident"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainClosureAsync(original with { IntentId = Guid.NewGuid() }));
        tab.Unavailable = true; await Assert.ThrowsAsync<JSException>(() => Store().ClearClosureAsync(original)); tab.Unavailable = false;
        await Store().ClearClosureAsync(original); Assert.Empty(tab.Values);
        var resolution = new IncidentClosureResolution(Guid.NewGuid(), new(context, original.Review.LifecycleDatabase, "incident", "plan", original.IntentId, new string('D', 64), original.Review.NativeRevision, new string('E', 64),
            [new(1, "STATUS", "REPORTED", null, "Private updated native history", "Y", DateTime.UtcNow.AddHours(-1), "HSE", "actor", DateTime.UtcNow.AddHours(-1))]), "Private reviewed history resolution reason.");
        await Store().RetainResolutionAsync(resolution); Assert.DoesNotContain(resolution.Reason, Assert.Single(tab.Values).Value);
        Assert.Equal(JsonSerializer.Serialize(resolution), JsonSerializer.Serialize(await Store().ReadResolutionAsync(context with { Revision = "new-review" }, "incident")));
        Assert.Null(await Store().ReadResolutionAsync(context with { Company = new("tenant", "other") }, "incident"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainResolutionAsync(resolution with { IntentId = Guid.NewGuid() }));
        await Store().ClearResolutionAsync(resolution); Assert.Empty(tab.Values);
    }
    [Fact]
    public async Task ProtectedCorrectionReturnPreservesOriginalAndExcludesClosureIntent()
    {
        var tab = new Tab(); var protection = new EphemeralDataProtectionProvider(); ICorrectiveActionIntentStore Store() => new CorrectiveActionIntentStore(new ProtectedSessionStorage(tab, protection));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "other", "Private failed valve inspection");
        var actions = new CAVerificationReview(context, "incident", "plan", new string('A', 64), [action]); var decision = new CAVerificationDecision(Guid.NewGuid(), actions, "REJECTED", "Independent private inspection rejected the repair.");
        var original = new CompanyHseCorrectionCommand(Guid.NewGuid(), new(new(actions, [decision]), new("lifecycle", "binding"), "instance", "step-instance", "INC_VERIFICATION", "Safety", new string('B', 64), new string('C', 64)), "Private instructions to replace the failed valve.");
        await Store().RetainCorrectionAsync(original); Assert.DoesNotContain(original.Reason, Assert.Single(tab.Values).Value);
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(await Store().ReadCorrectionAsync(context with { Revision = "new-review" }, "incident")));
        Assert.Null(await Store().ReadCorrectionAsync(context with { ActorUserId = "other" }, "incident"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainCorrectionAsync(original with { IntentId = Guid.NewGuid() }));
        var closure = new CompanyHseClosureCommand(Guid.NewGuid(), original.Review with { Stage = "INC_CORRECTIVE_ACTIONS" }, "Hand completed action evidence to independent verification.");
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainClosureAsync(closure));
        tab.Unavailable = true; await Assert.ThrowsAsync<JSException>(() => Store().ClearCorrectionAsync(original)); tab.Unavailable = false;
        await Store().ClearCorrectionAsync(original); Assert.Empty(tab.Values);
        await Store().RetainClosureAsync(closure);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainCorrectionAsync(original));
    }
    [Fact]
    public async Task ProtectedVerificationPreservesOriginalAcrossReloadAndRejectsReplacement()
    {
        var tab = new Tab(); var protection = new EphemeralDataProtectionProvider();
        ICorrectiveActionIntentStore Store() => new CorrectiveActionIntentStore(new ProtectedSessionStorage(tab, protection));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), "other-operator", "Private completion evidence");
        var original = new CAVerificationDecision(Guid.NewGuid(), new(context, "incident", "plan", new string('A', 64), [action]), "VERIFIED", "Independent private inspection confirmed the repair.");
        await Store().RetainVerificationAsync(original);
        Assert.DoesNotContain(original.Reason, Assert.Single(tab.Values).Value);
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(await Store().ReadVerificationAsync(context with { Revision = "new-review" }, "incident")));
        Assert.Null(await Store().ReadVerificationAsync(context with { ActorUserId = "other" }, "incident"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainVerificationAsync(original with { Decision = "REJECTED" }));
        tab.Unavailable = true; await Assert.ThrowsAsync<JSException>(() => Store().ClearVerificationAsync(original)); tab.Unavailable = false;
        await Store().ClearVerificationAsync(original); Assert.Empty(tab.Values);
    }
    [Fact]
    public async Task ProtectedCompletionSurvivesReloadAndRejectsReplacement()
    {
        var tab = new Tab(); var protection = new EphemeralDataProtectionProvider();
        ICorrectiveActionIntentStore Store() => new CorrectiveActionIntentStore(new ProtectedSessionStorage(tab, protection));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var review = new CACompletionReview(context, "incident", "plan", 3, "step", "Inspect", "worker", new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), new string('A', 64));
        var original = new CorrectiveActionCompletionOriginal(review, "Private completion evidence");
        await Store().RetainCompletionAsync(original);
        Assert.DoesNotContain(original.Notes, Assert.Single(tab.Values).Value);
        Assert.Equal(original, await Store().ReadCompletionAsync(context with { Revision = "new-review" }, "incident"));
        Assert.Null(await Store().ReadCompletionAsync(context with { Company = new("tenant", "other") }, "incident"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainCompletionAsync(original with { Notes = "Different evidence" }));
        tab.Unavailable = true; await Assert.ThrowsAsync<JSException>(() => Store().ClearCompletionAsync(original)); tab.Unavailable = false;
        await Store().ClearCompletionAsync(original); Assert.Empty(tab.Values);
    }
    [Fact]
    public async Task ProtectedCorrectiveActionSurvivesReloadAndRejectsAnotherIntent()
    {
        var tab = new Tab(); var protection = new EphemeralDataProtectionProvider();
        ICorrectiveActionIntentStore Store() => new CorrectiveActionIntentStore(new ProtectedSessionStorage(tab, protection));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var request = new AddCARequest(Guid.NewGuid(), "Repair the recorded valve", "CORRECTIVE", new DateTime(2027, 1, 2, 0, 0, 0, DateTimeKind.Utc), "participant");
        var options = new CorrectiveActionOptions(context, "incident", "plan", new string('A', 64), ["CORRECTIVE"], ["participant"]);
        var original = new CorrectiveActionOriginal(context, "incident", request, options);
        await Store().RetainAsync(original);
        Assert.DoesNotContain(request.CADescription, Assert.Single(tab.Values).Value);
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(await Store().ReadAsync(context with { Revision = "new-review" }, "incident")));
        Assert.Null(await Store().ReadAsync(context with { Company = new("tenant", "other") }, "incident"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainAsync(original with { Request = request with { ActionId = Guid.NewGuid() } }));
        tab.Unavailable = true; await Assert.ThrowsAsync<JSException>(() => Store().ClearAsync(original)); tab.Unavailable = false;
        await Store().ClearAsync(original); Assert.Empty(tab.Values);
    }
    [Fact]
    public async Task ProtectedRcaHandoffSurvivesReloadAndRejectsReplacement()
    {
        var tab = new Tab(); var protection = new EphemeralDataProtectionProvider();
        IIncidentRcaHandoffIntentStore Store() => new IncidentReportIntentStore(new ProtectedSessionStorage(tab, protection));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var cause = new IncidentRcaRecordedCause(context.Company, context.Database, context.FieldId!, "incident-private", Guid.NewGuid(), 1, "PROCESS", "Reviewed private incident cause.", "another-author", new string('A', 64));
        var original = new CompanyHseRcaHandoffCommand(Guid.NewGuid(), new(context, new("lifecycle", "binding"), cause, "instance", "step", "Safety", new string('B', 64)));
        await Store().RetainAsync(original);
        Assert.DoesNotContain(cause.Summary, Assert.Single(tab.Values).Value);
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(await Store().ReadHandoffAsync(context with { Revision = "new-review" }, "incident-private")));
        Assert.Null(await Store().ReadHandoffAsync(context with { Company = new("tenant", "other") }, "incident-private"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainAsync(original with { IntentId = Guid.NewGuid() }));
        tab.Unavailable = true; await Assert.ThrowsAsync<JSException>(() => Store().ClearAsync(original)); tab.Unavailable = false;
        await Store().ClearAsync(original); Assert.Empty(tab.Values);
    }
    [Fact]
    public async Task ProtectedCauseSurvivesReloadAndRejectsReplacementOrCorruption()
    {
        var tab = new Tab(); var protection = new EphemeralDataProtectionProvider();
        IIncidentRcaIntentStore Store() => new IncidentReportIntentStore(new ProtectedSessionStorage(tab, protection));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var option = new IncidentRcaOption("ROOT", "PROCESS", "Process");
        var original = new IncidentRcaIntent(Guid.NewGuid(), new(context, "incident-private", new string('A', 64), 1, [], [option]), option, "Reviewed private incident cause.");
        await Store().RetainAsync(original);
        Assert.DoesNotContain(original.Summary, Assert.Single(tab.Values).Value);
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(await Store().ReadRcaAsync(context with { Revision = "new-review" }, "incident-private")));
        Assert.Null(await Store().ReadRcaAsync(context with { Company = new("tenant", "other") }, "incident-private"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainAsync(original with { IntentId = Guid.NewGuid() }));
        tab.Unavailable = true;
        await Assert.ThrowsAsync<JSException>(() => Store().ClearAsync(original));
        tab.Unavailable = false;
        await Store().ClearAsync(original);
        Assert.Empty(tab.Values);
    }
    private sealed class Tab : IJSRuntime
    {
        public Dictionary<string, string> Values { get; } = [];
        public bool Unavailable { get; set; }
        public string? FailDeleteSuffix { get; set; }
        public bool FailAfterDelete { get; set; }
        public int DeleteCalls { get; private set; }
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, default, args);
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (Unavailable) throw new JSException("Session storage unavailable");
            var key = (string)args![0]!;
            if (identifier == "sessionStorage.setItem") Values[key] = (string)args[1]!;
            else if (identifier == "sessionStorage.removeItem")
            {
                DeleteCalls++;
                var fail = FailDeleteSuffix is not null && key.EndsWith(FailDeleteSuffix, StringComparison.Ordinal);
                if (fail && !FailAfterDelete) { FailDeleteSuffix = null; throw new JSException("Controlled failure before deletion"); }
                Values.Remove(key);
                if (fail) { FailDeleteSuffix = null; throw new JSException("Controlled acknowledgement loss after deletion"); }
            }
            else if (identifier == "sessionStorage.getItem") return ValueTask.FromResult(Values.TryGetValue(key, out var value) ? JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))! : default!);
            else throw new InvalidOperationException(identifier);
            return ValueTask.FromResult(default(T)!);
        }
    }

    [Fact]
    public async Task UnavailableOrCorruptedStorageDoesNotPermitAReplacement()
    {
        var tab = new Tab(); var protection = new EphemeralDataProtectionProvider();
        ICompanyHseWorkflowIntentStore Store() => new IncidentReportIntentStore(new ProtectedSessionStorage(tab, protection));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var original = new CompanyHseWorkflowStart(context, new("lifecycle", "binding"), Guid.NewGuid(), "incident", new string('A', 64), new string('B', 64));
        await Store().RetainAsync(original);
        tab.Unavailable = true;
        await Assert.ThrowsAsync<JSException>(() => Store().RetainAsync(original with { IntentId = Guid.NewGuid() }));
        tab.Unavailable = false;
        tab.Values[Assert.Single(tab.Values).Key] = "invalid-protected-data";
        await Assert.ThrowsAnyAsync<Exception>(() => Store().RetainAsync(original with { IntentId = Guid.NewGuid() }));
        Assert.Equal("invalid-protected-data", Assert.Single(tab.Values).Value);
    }

    [Fact]
    public async Task ProtectedOriginalSurvivesNewOwnerAndRefusesReplacementAndForeignScope()
    {
        var tab = new Tab(); var protection = new EphemeralDataProtectionProvider();
        ICompanyHseWorkflowIntentStore Store() => new IncidentReportIntentStore(new ProtectedSessionStorage(tab, protection));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var original = new CompanyHseWorkflowStart(context, new("lifecycle", "binding"), Guid.NewGuid(), "incident-private", new string('A', 64), new string('B', 64));
        await Store().RetainAsync(original);
        Assert.DoesNotContain(original.IncidentId, Assert.Single(tab.Values).Value);
        Assert.Equal(original, await Store().ReadAsync(context with { Revision = "new-review" }, original.IncidentId));
        Assert.Null(await Store().ReadAsync(context with { Company = new("tenant", "other") }, original.IncidentId));
        Assert.Null(await Store().ReadAsync(context, "other-incident"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainAsync(original with { IntentId = Guid.NewGuid() }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().ClearAsync(original with { DefinitionRevision = new string('C', 64) }));
        Assert.Equal(original, await Store().ReadAsync(context, original.IncidentId));
        await Store().ClearAsync(original);
        Assert.Empty(tab.Values);
    }
}
