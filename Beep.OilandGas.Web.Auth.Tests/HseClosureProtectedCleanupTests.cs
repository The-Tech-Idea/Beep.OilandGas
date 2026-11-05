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
    [Theory]
    [InlineData("approval", false)] [InlineData("approval", true)]
    [InlineData("correction", false)] [InlineData("correction", true)]
    [InlineData("resolution", false)] [InlineData("resolution", true)]
    public async Task ProtectedClosureCleanupRecoversDeletedSlotsWithoutClearingReplacement(string flow, bool afterDelete)
    {
        var tab = new Tab(); var protection = new EphemeralDataProtectionProvider();
        ICorrectiveActionIntentStore Store() => new CorrectiveActionIntentStore(new ProtectedSessionStorage(tab, protection));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var action = new CAVerificationAction(1, "step", "Inspect valve", "COMPLETED", "worker", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddHours(-1), "other", "Private inspection evidence");
        var actions = new CAVerificationReview(context, "incident", "plan", new string('A', 64), [action]);
        CompanyHseClosureReview Review(bool rejected) => new(new(actions, [new(Guid.NewGuid(), actions, rejected ? "REJECTED" : "VERIFIED", "Independent inspection recorded the valve evidence.")]), new("lifecycle", "binding"), "instance", "step-instance", rejected ? "INC_VERIFICATION" : "INC_CLOSURE", "Safety", new string('B', 64), new string('C', 64));
        var closure = new CompanyHseClosureCommand(Guid.NewGuid(), Review(false), "Private final closure approval evidence.");
        var correction = new CompanyHseCorrectionCommand(Guid.NewGuid(), Review(true), "Private correction return instructions.");
        var resolution = new IncidentClosureResolution(Guid.NewGuid(), new(context, closure.Review.LifecycleDatabase, "incident", "plan", closure.IntentId, new string('D', 64), closure.Review.NativeRevision, new string('E', 64), [new(1, "STATUS", "REPORTED", null, "Changed private history", "Y", DateTime.UtcNow.AddHours(-1), "HSE", "actor", DateTime.UtcNow.AddHours(-1))]), "Private native history resolution evidence.");
        async Task Retain(bool replacement)
        {
            if (flow == "approval") await Store().RetainClosureAsync(replacement ? closure with { IntentId = Guid.NewGuid() } : closure);
            else if (flow == "correction") await Store().RetainCorrectionAsync(replacement ? correction with { IntentId = Guid.NewGuid() } : correction);
            else { await Store().RetainClosureAsync(closure); await Store().RetainResolutionAsync(replacement ? resolution with { IntentId = Guid.NewGuid() } : resolution); }
        }
        async Task Clear()
        {
            if (flow == "approval") await Store().ClearClosureAsync(closure);
            else if (flow == "correction") await Store().ClearCorrectionAsync(correction);
            else { await Store().ClearResolutionAsync(resolution); await Store().ClearClosureAsync(closure); }
        }
        await Retain(false);
        Assert.All(tab.Values.Values, encrypted => Assert.DoesNotContain("Private", encrypted));
        tab.FailDeleteSuffix = flow == "correction" ? ".correction" : ".closure"; tab.FailAfterDelete = afterDelete;
        await Assert.ThrowsAsync<JSException>(Clear);
        if (flow == "resolution") Assert.Null(await Store().ReadResolutionAsync(context, "incident"));
        await Clear(); Assert.Empty(tab.Values);
        var deleted = tab.DeleteCalls; await Clear(); Assert.Equal(deleted, tab.DeleteCalls); Assert.Empty(tab.Values);
        await Retain(true); var occupied = tab.Values.ToDictionary(x => x.Key, x => x.Value); deleted = tab.DeleteCalls;
        await Assert.ThrowsAsync<InvalidOperationException>(Clear);
        Assert.Equal(deleted, tab.DeleteCalls); Assert.Equal(occupied.Count, tab.Values.Count);
        Assert.All(occupied, row => Assert.Equal(row.Value, tab.Values[row.Key]));
    }
}
