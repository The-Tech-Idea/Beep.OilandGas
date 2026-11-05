using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Pages.PPDM39.Drilling;
using Beep.OilandGas.Web.Pages.PPDM39.Reservoir;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class EngineeringWorkspaceLandingTests
{
    public static IEnumerable<object[]> Cases =>
        from drilling in new[] { true, false }
        from mode in new[] { "current", "seeded", "empty", "failed", "missing-company", "other-workspace", "foreign-company", "foreign-actor", "company-change", "late-company" }
        select new object[] { drilling, mode };

    private sealed class Navigation : NavigationManager
    {
        public Navigation() => Initialize("https://render.invalid/", "https://render.invalid/");
        protected override void NavigateToCore(string uri, bool forceLoad) => throw new InvalidOperationException("Static rendering cannot navigate.");
    }

    private sealed class NoBrowser : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string name, object?[]? args) => ValueTask.FromResult(default(T)!);
        public ValueTask<T> InvokeAsync<T>(string name, CancellationToken token, object?[]? args) => InvokeAsync<T>(name, args);
    }

    [Theory, MemberData(nameof(Cases))]
    public async Task ActualEngineeringLandingsShowCurrentAssignedSourcesAndKeepUnavailableMetricsExplicit(bool drilling, string mode)
    {
        var home = mode=="seeded" ? drilling ? "/drilling/dashboard" : "/reservoir/dashboard" : drilling ? "/drilling/workspace" : "/reservoir/workspace";
        var page = drilling ? typeof(DrillingEngineerWorkspace) : typeof(ReservoirEngineerWorkspace);
        Assert.NotEmpty(page.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.Contains(page.GetCustomAttributes(typeof(RouteAttribute),true).Cast<RouteAttribute>(),route=>route.Template==home);
        using var company = new CompanyClientContext();
        if (mode != "missing-company")
            Assert.True(company.TrySelect(company.Clear(), "engineer", new(new("tenant", "company"), ["ConfiguredEngineerRole"], [])));
        var persona = new AppPersona { Code = "ENGINEER", Name = "Company engineer", DefaultRoute = mode == "other-workspace" ? "/other" : home };
        var personas = new Mock<IPersonaContextService>();
        personas.SetupGet(value => value.CurrentPersona).Returns(persona);
        personas.SetupGet(value => value.CurrentProfile).Returns(new AppUserPersona { UserId = "engineer", PersonaCode = persona.Code });
        personas.SetupGet(value => value.Catalog).Returns([persona]);
        personas.Setup(value => value.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox = new Mock<IUnifiedTaskInboxService>();
        var reads = 0;
        const string process = "program/مراجعة%2F";
        var route = "/ppdm39/process/instances/" + Uri.EscapeDataString(process);
        inbox.Setup(value => value.GetInboxAsync(persona.Code)).Returns(() =>
        {
            ++reads;
            if (mode == "failed") throw new HttpRequestException("Unavailable assigned work");
            if (mode == "late-company") company.Clear();
            var response = new UnifiedInbox
            {
                Company = new("tenant", mode == "foreign-company" ? "other" : "company"),
                ActorUserId = mode == "foreign-actor" ? "other" : "engineer", PersonaCode = persona.Code
            };
            if (mode != "empty")
            {
                response.CriticalTasks = [new() { TaskId = "review", TaskType = "REVIEW", Priority = 1,
                    ScopeKind = "FIELD", AssignedRole = "ConfiguredEngineerRole", EntityType = "WELL", EntityId = "well/1",
                    WorkflowName = process, StepName = "technical-review", Route = route,
                    EntityDescription = "<script>Recorded engineering review</script>" }];
                response.Counts = new() { TotalPending = 1, Critical = 1, Reviews = 1 };
            }
            return Task.FromResult(response);
        });
        var services = new ServiceCollection();
        services.AddLogging(); services.AddMudServices();
        services.AddSingleton(company); services.AddSingleton(personas.Object); services.AddSingleton(inbox.Object);
        services.AddSingleton<IJSRuntime, NoBrowser>(); services.AddSingleton<NavigationManager, Navigation>();
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var root = await renderer.RenderComponentAsync(page, ParameterView.Empty);
            if (mode == "company-change") { company.Clear(); await Task.Yield(); }
            var html = root.ToHtmlString();
            Assert.Contains(drilling ? "Drilling engineer workspace" : "Reservoir engineer workspace", html);
            Assert.Contains("totals are unavailable here", html);
            Assert.Contains("href=\"/tasks/inbox\"", html);
            Assert.Contains(drilling ? "href=\"/development/well-design\"" : "href=\"/reservoir/reserves\"", html);
            Assert.Contains(drilling ? "Follow execution and handover" : "Compare analysis and scenarios", html);
            Assert.DoesNotContain("W-2025-042", html); Assert.DoesNotContain("PL-003", html);
            Assert.DoesNotContain("<script>", html); Assert.DoesNotContain("$0", html);
            if (mode is "current" or "seeded")
            {
                Assert.Equal(1, reads);
                Assert.Contains("Company: tenant / company", html);
                Assert.DoesNotContain("&#xC2;", html); Assert.DoesNotContain("\u00c2\u00b7", html);
                Assert.Contains("Reviews: 1", html); Assert.Contains(route, html);
                Assert.Contains("&lt;script&gt;Recorded engineering review", html);
                Assert.Contains("Assigned role: ConfiguredEngineerRole", html);
                Assert.Contains("Field workflow assignments: 1", html);
                Assert.DoesNotContain("Invoice workflow assignments:", html);
                Assert.DoesNotContain("Payment review/rejection assignments:", html);
            }
            else
            {
                Assert.DoesNotContain(route, html); Assert.DoesNotContain("Recorded engineering review", html);
                if (mode == "empty") Assert.Contains("No pending work is visible", html);
                else Assert.DoesNotContain("Pending work: 0", html);
                if (mode is "failed" or "foreign-company" or "foreign-actor") Assert.Contains("Assigned work could not be confirmed", html);
                if (mode == "other-workspace") Assert.Contains("mapped to another landing page", html);
                if (mode is "missing-company" or "other-workspace") Assert.Equal(0, reads);
                if (mode is "missing-company" or "company-change" or "late-company") Assert.Contains("Select a company", html);
            }
        });
    }
}
