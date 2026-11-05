using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using TheTechIdeaWeb.Diagnostics;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class MaterialReservationLocalTimeTests
{
    private sealed class Auth : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "actor")], "test"))));
    }

    [Fact]
    public async Task ReservationQueueDisplaysLocalTimesAlongsideRecordedUtc()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Warehouse"], ["Inventory.View"])));
        var scope = company.Selection!.Scope; var utc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var task = new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(scope, Guid.NewGuid()), "field", "order"); var id = Guid.NewGuid();
        var row = new CompanyWorkOrderMaterialReservationFacts(scope, task, id, CompanyWorkOrderMaterialReservationFacts.IdFor(scope, id), new string('A', 64), "actor", "item", 1, "ea", "warehouse", "bin", utc, utc.AddHours(1));
        var queue = CompanyMaterialReservationQueue.Create(new(scope, new("db", "fingerprint"), "actor", "authority", "field"), [new(row, null)], utc);
        var settings = new CompanyEnvironmentPreview(scope, "KW", "Kuwait", "en-US", "metric", "m", "bar", "mm", [], [], [])
        { TimeZoneId = "Asia/Kuwait", DisplayCulture = "en-US", DateTimeFormat = "yyyy-MM-dd HH:mm", Calendar = "Gregorian" };
        var review = new CompanyEnvironmentEffectiveReview(scope, new string('A', 64), new string('B', 64), null, new string('C', 64), settings) { ReviewStatus = "Current" };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());
        ui.Services.AddSingleton(Mock.Of<IFieldSelectionContext>()); ui.Services.AddSingleton(Mock.Of<IAccountingServiceClient>());
        using var state = new InventoryWorkspaceState(company);
        await state.LoadMaterialReservationsAsync("field", _ => Task.FromResult(queue));
        ui.Services.AddSingleton(state); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var panel = ui.RenderComponent<CompanyMaterialReservationQueuePanel>(parameters => parameters.AddCascadingValue(new CompanyEnvironmentDisplay(company.Version, review)));
        Assert.Contains("Valid UTC: 2026-01-01T12:00:00", panel.Markup);
        Assert.Contains("Valid in company time zone", panel.Markup);
        Assert.Contains("2026-01-01 15:00", panel.Markup);
        Assert.Contains("Asia/Kuwait", panel.Markup);
        await panel.InvokeAsync(company.Clear);
        panel.WaitForAssertion(() => Assert.DoesNotContain("Valid in company time zone", panel.Markup));
        Assert.Null(state.MaterialReservations);
    }

    [Theory]
    [InlineData("company")][InlineData("actor")][InlineData("field")]
    public async Task ReservationEvidenceForAnotherSelectionNeverReachesTheWorkspace(string mismatch)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Warehouse"], ["Inventory.View"])));
        using var state = new InventoryWorkspaceState(company);
        var owner = mismatch == "company" ? new CompanyScope("tenant", "other") : company.Selection!.Scope;
        var actor = mismatch == "actor" ? "other" : "actor"; var field = mismatch == "field" ? "other" : "field";
        var queue = CompanyMaterialReservationQueue.Create(new(owner, new("db", "fingerprint"), actor, "authority", field), [], DateTime.UtcNow);
        await Assert.ThrowsAsync<InvalidOperationException>(() => state.LoadMaterialReservationsAsync("field", _ => Task.FromResult(queue)));
        Assert.Null(state.MaterialReservations);
    }

    [Theory]
    [InlineData("Asia/Kuwait", "2026-01-01 15:00", true)]
    [InlineData("Europe/Oslo", "2026-01-01 13:00", true)]
    [InlineData("invalid-zone", "", false)]
    public void UsesReviewedCompanyTimeZoneWithoutChangingRecordedUtc(string zone, string expected, bool confirmed)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Warehouse"], ["Inventory.View"])));
        var scope = company.Selection!.Scope;
        var settings = new CompanyEnvironmentPreview(scope, "KW", "Kuwait", "en-US", "metric", "m", "bar", "mm", [], [], [])
        { TimeZoneId = zone, DisplayCulture = "en-US", DateTimeFormat = "yyyy-MM-dd HH:mm", Calendar = "Gregorian" };
        var review = new CompanyEnvironmentEffectiveReview(scope, new string('A', 64), new string('B', 64), null, new string('C', 64), settings) { ReviewStatus = "Current" };
        var panel = new CompanyMaterialReservationQueuePanel { CompanyEnvironment = new(company.Version, review) };
        typeof(CompanyMaterialReservationQueuePanel).GetProperty("Company", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(panel, company);
        var queue = new CompanyMaterialReservationQueue(new(scope, new("db", "fingerprint"), "actor", "authority", "field"), [], DateTime.UtcNow, "revision");
        var utc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var method = typeof(CompanyMaterialReservationQueuePanel).GetMethod("LocalTime", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var result = (string?)method.Invoke(panel, [queue, utc]);
        if (confirmed) { Assert.Contains(expected, result); Assert.Contains(zone, result); }
        else Assert.Null(result);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Null(method.Invoke(panel, [queue with { Context = queue.Context with { ActorUserId = "other" } }, utc]));
        Assert.Null(method.Invoke(panel, [queue with { Context = queue.Context with { Company = new("tenant", "other") } }, utc]));
        Assert.Null(method.Invoke(panel, [queue, DateTime.SpecifyKind(utc, DateTimeKind.Unspecified)]));
    }
}
#endif
