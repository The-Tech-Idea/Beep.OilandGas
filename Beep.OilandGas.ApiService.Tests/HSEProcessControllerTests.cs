using System.Security.Claims;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Process;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class HSEProcessControllerTests
{
#if WORKFLOW_INBOX_FOCUSED
    private sealed class HseHttpAuthentication(Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder)
        : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers["X-Test-Authenticated"] != "yes") return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.NoResult());
            var claims = new[] { new Claim("party_id", "planner"), new Claim(ClaimTypes.Role, "Manager"), new Claim("permission", "HSE.ManageIncidents"), new Claim("permission", "HSE.ReportIncident") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(new(principal, Scheme.Name)));
        }
    }

    [Theory]
    [InlineData("legacy-close", "allowed", 404)]
    [InlineData("legacy-actions", "allowed", 404)]
    [InlineData("legacy-complete", "allowed", 404)]
    [InlineData("report", "allowed", 200)]
    [InlineData("report", "permission", 403)]
    [InlineData("report", "write", 403)]
    [InlineData("report", "membership", 403)]
    [InlineData("report", "no-company", 400)]
    [InlineData("report", "anonymous", 401)]
    [InlineData("legacy-process", "allowed", 405)]
    [InlineData("legacy-domain", "allowed", 405)]
    [InlineData("legacy-rca", "allowed", 404)]
    public async Task HseDecisionHttpUsesCompanyMiddlewareBeforePolicyAndFieldChecks(string action, string mode, int expectedStatus)
    {
        using var f = new DevelopmentDraftAuthorityTests.Fixture();
        f.Roles = ["CompanySafetyLead"];
        f.Permissions = mode == "permission" ? action == "report" ? ["HSE.View", "HSE.ManageIncidents"] : ["HSE.View"] : ["HSE.View", "HSE.ManageIncidents", "HSE.ReportIncident"];
        if (mode == "membership") f.Available = false;
        f.Assets.Setup(x => x.CheckAssetAccessAsync("planner", "field", "FIELD", It.IsAny<string?>(), null))
            .ReturnsAsync((string _, string _, string _, string? permission, string? _) => new Beep.OilandGas.Models.Data.AccessCheckResponse
            { HasAccess = !(mode == "write" && permission == "WRITE") && (permission is not ("HSE.ManageIncidents" or "HSE.ReportIncident") || f.Permissions.Contains(permission)) });
        var field = new Mock<IFieldOrchestrator>(); field.Setup(x => x.SetActiveFieldAsync("field")).ReturnsAsync(true); field.SetupGet(x => x.CurrentFieldId).Returns("field");
        var hse = new Mock<IFieldHSEService>(); hse.Setup(x => x.GetIncidentAsync("case")).ReturnsAsync((HSEIncidentRecord?)null);
        field.Setup(x => x.GetHSEService()).Returns(hse.Object);
        ClaimsPrincipal? observed = null; IHttpContextAccessor? accessor = null;
        var process = new Mock<IProcessService>(); process.Setup(x => x.GetProcessInstancesForEntityAsync("case", "HSE_INCIDENT"))
            .Callback(() => observed = accessor!.HttpContext!.User).ReturnsAsync([]);
        var reader = new Mock<ICompanyHseWorkflowReader>(); reader.Setup(x => x.ReadAsync(It.IsAny<ClaimsPrincipal>(), "case"))
            .Callback<ClaimsPrincipal, string>((user, _) => observed = user).ReturnsAsync((ProcessInstance?)null);
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0"); builder.Services.AddHttpContextAccessor();
        builder.Services.AddAuthentication("test").AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, HseHttpAuthentication>("test", _ => { });
        builder.Services.AddAuthorization(options => { options.AddPolicy("HSE.ManageIncidents", policy => policy.RequireAuthenticatedUser().RequireClaim("permission", "HSE.ManageIncidents")); options.AddPolicy("HSE.ReportIncident", policy => policy.RequireAuthenticatedUser().RequireClaim("permission", "HSE.ReportIncident")); });
        builder.Services.AddControllers().AddApplicationPart(typeof(HSEProcessController).Assembly);
        builder.Services.AddSingleton<Func<ClaimsPrincipal, string?>>(user => user.FindFirst("party_id")?.Value);
        builder.Services.AddSingleton<Beep.OilandGas.Repository.IRepositoryAccessService>(f.Accounts.Object);
        builder.Services.AddSingleton(f.Tenant); builder.Services.AddSingleton(new Beep.OilandGas.Repository.RepositoryCompanyContextResolver(f.Accounts.Object));
        builder.Services.AddSingleton(new Beep.OilandGas.ApiService.Services.ModuleBindingService(f.Db, f.Drafts.Native.Editor.Object));
        builder.Services.AddSingleton(f.Company); builder.Services.AddSingleton<ICompanyFieldSelectionService>(f.Fields.Object);
        builder.Services.AddSingleton<IAccessControlService>(f.Assets.Object); builder.Services.AddSingleton(field.Object); builder.Services.AddSingleton(process.Object);
        builder.Services.AddSingleton(reader.Object);
        var incidentCommands = new Mock<ICompanyIncidentCommands>(MockBehavior.Strict);
        incidentCommands.Setup(x => x.CreateReportAsync(It.IsAny<IncidentReportIntent>())).ReturnsAsync((IncidentReportIntent item) => item.IncidentId());
        builder.Services.AddSingleton(incidentCommands.Object);
        builder.Services.AddScoped(sp => new Beep.OilandGas.ApiService.Services.RepositoryRequestAccount(sp.GetRequiredService<IHttpContextAccessor>(), user => user.FindFirst("party_id")?.Value, f.Accounts.Object, null!));
        await using var app = builder.Build(); accessor = app.Services.GetRequiredService<IHttpContextAccessor>(); app.UseAuthentication(); app.UseMiddleware<Beep.OilandGas.ApiService.Middleware.CompanyRequestMiddleware>(); app.UseAuthorization(); app.MapControllers(); await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        if (mode != "anonymous") client.DefaultRequestHeaders.Add("X-Test-Authenticated", "yes");
        if (mode != "no-company") { client.DefaultRequestHeaders.Add(CompanyRequestHeaders.Tenant, "tenant"); client.DefaultRequestHeaders.Add(CompanyRequestHeaders.Company, "company"); }
        using var response = action switch
        {
            "legacy-rca" => await client.PostAsJsonAsync("/api/field/current/process/hse/incident/rca?incidentId=case", new { }),
            "legacy-close" => await client.PostAsJsonAsync("/api/field/current/process/hse/incident/close?incidentId=case", new { Reason = "Review" }),
            "legacy-actions" => await client.PostAsJsonAsync("/api/field/current/process/hse/incident/actions?incidentId=case", new { }),
            "legacy-complete" => await client.PatchAsJsonAsync("/api/field/current/process/hse/incident/actions/1?incidentId=case", new { Notes = "Review" }),
            "report" => await client.PostAsJsonAsync("/api/field/current/hse/reports", new IncidentReportIntent(Guid.NewGuid(), f.Context, new("SET", "INJURY", "TIER_2"), new string('A', 64), DateTime.UtcNow, "site", "incident", "KW")),
            "legacy-process" => await client.PostAsJsonAsync("/api/field/current/process/hse/incidents", new { }),
            "legacy-domain" => await client.PostAsJsonAsync("/api/field/current/hse/incidents", new { }),
            _ => throw new InvalidOperationException()
        };
        Assert.Equal(expectedStatus, (int)response.StatusCode);
        reader.Verify(x => x.ReadAsync(It.IsAny<ClaimsPrincipal>(), "case"), mode == "allowed" && action is not ("report" or "legacy-process" or "legacy-domain" or "legacy-rca" or "legacy-actions" or "legacy-complete" or "legacy-close") ? Times.Once() : Times.Never());
        hse.Verify(x => x.GetIncidentAsync("case"), Times.Never());
        if (mode == "allowed" && action is not ("report" or "legacy-process" or "legacy-domain" or "legacy-rca" or "legacy-actions" or "legacy-complete" or "legacy-close")) { Assert.NotNull(observed); Assert.True(observed.IsInRole("CompanySafetyLead")); Assert.False(observed.IsInRole("Manager")); }
        incidentCommands.Verify(x => x.CreateReportAsync(It.IsAny<IncidentReportIntent>()), action == "report" && mode == "allowed" ? Times.Once() : Times.Never());
    }
    [Theory]
    [InlineData("CompleteCAAsync", "allowed")]
    [InlineData("CompleteCAAsync", "permission")]
    [InlineData("CompleteCAAsync", "write")]
    public async Task IncidentDecisionsRequireCompanyHsePermissionAndFieldWrite(string method, string state)
    {
        using var f = new DevelopmentDraftAuthorityTests.Fixture();
        var guard = typeof(Beep.OilandGas.ApiService.Controllers.HSE.HSEController).GetMethod(method)!.GetCustomAttributes(typeof(Beep.OilandGas.ApiService.Attributes.RequireCurrentFieldAccessAttribute), false)
            .Cast<Beep.OilandGas.ApiService.Attributes.RequireCurrentFieldAccessAttribute>().Single();
        var policy = typeof(Beep.OilandGas.ApiService.Controllers.HSE.HSEController).GetMethod(method)!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single();
        Assert.Equal(Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.ManageIncidents, policy.Policy);
        f.Assets.Setup(x => x.CheckAssetAccessAsync("planner", "field", "FIELD", It.IsAny<string?>(), null))
            .ReturnsAsync((string _, string _, string _, string? permission, string? _) => new Beep.OilandGas.Models.Data.AccessCheckResponse
            { HasAccess = !(state == "permission" && permission == "HSE.ManageIncidents" || state == "write" && permission == "WRITE") });
        var field = new Mock<IFieldOrchestrator>(); field.Setup(x => x.SetActiveFieldAsync("field")).ReturnsAsync(true); field.SetupGet(x => x.CurrentFieldId).Returns("field");
        using var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddSingleton(f.Account).AddSingleton(f.Company).AddSingleton<ICompanyFieldSelectionService>(f.Fields.Object)
            .AddSingleton<IAccessControlService>(f.Assets.Object).AddSingleton(field.Object).BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services, User = f.User }; f.Http.HttpContext = http;
        var context = new Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext(
            new ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
            new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>());
        await guard.OnAuthorizationAsync(context);
        if (state == "allowed") Assert.Null(context.Result); else Assert.IsType<ForbidResult>(context.Result);
        f.Assets.Verify(x => x.CheckAssetAccessAsync("planner", "field", "FIELD", "HSE.ManageIncidents", null), state == "allowed" ? Times.Exactly(2) : Times.AtLeastOnce());
        f.Assets.Verify(x => x.CheckAssetAccessAsync("planner", "field", "FIELD", "WRITE", null), state == "permission" ? Times.Never() : state == "allowed" ? Times.Exactly(2) : Times.Once());
    }
    [Theory]
    [InlineData("HSE.ManageIncidents")]
    [InlineData("WRITE")]
    public async Task IncidentDecisionRejectsPermissionOrFieldWriteRevokedDuringRestoration(string revoked)
    {
        using var f = new DevelopmentDraftAuthorityTests.Fixture(); var checks = 0;
        f.Assets.Setup(x => x.CheckAssetAccessAsync("planner", "field", "FIELD", It.IsAny<string?>(), null))
            .ReturnsAsync((string _, string _, string _, string? permission, string? _) => new Beep.OilandGas.Models.Data.AccessCheckResponse
            { HasAccess = permission != revoked || ++checks == 1 });
        var field = new Mock<IFieldOrchestrator>(); field.Setup(x => x.SetActiveFieldAsync("field")).ReturnsAsync(true); field.SetupGet(x => x.CurrentFieldId).Returns("field");
        using var services = new ServiceCollection().AddSingleton(f.Account).AddSingleton(f.Company).AddSingleton<ICompanyFieldSelectionService>(f.Fields.Object)
            .AddSingleton<IAccessControlService>(f.Assets.Object).AddSingleton(field.Object).BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services, User = f.User }; f.Http.HttpContext = http;
        var context = new Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext(
            new ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
            new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>());
        var guard = typeof(Beep.OilandGas.ApiService.Controllers.HSE.HSEController).GetMethod("CompleteCAAsync")!.GetCustomAttributes(typeof(Beep.OilandGas.ApiService.Attributes.RequireCurrentFieldAccessAttribute), false)
            .Cast<Beep.OilandGas.ApiService.Attributes.RequireCurrentFieldAccessAttribute>().Single();
        await Assert.ThrowsAnyAsync<Exception>(() => guard.OnAuthorizationAsync(context));
        field.Verify(x => x.ClearActiveField(), Times.Once);
    }
#endif
}
