using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Beep.OilandGas.ApiService.Controllers.Operations;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Lease;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using LeaseApi = Beep.OilandGas.Client.App.Services.Lease.LeaseService;

namespace Beep.OilandGas.Repository.Tests;

// Actual endpoint discovery/model binding/standard roles on loopback; fixture auth/gate/core are explicit boundaries.
// Not the full host, OIDC, claims bridge, company middleware or live provider.
public sealed class LeaseEndpointBindingTests
{
    [Theory] [InlineData("LeaseName", "Recorded name")] [InlineData("Status", "COMPANY_REVIEW")]
    public async Task SDK_filters_reach_the_actual_controller_through_MVC_model_binding(string key, string value)
    {
        Dictionary<string, string>? seen = null;
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict);
        core.Setup(x => x.GetAvailableLeasesAsync(It.IsAny<Dictionary<string, string>>())).Returns((Dictionary<string, string> filters) => { seen = filters; return Task.FromResult(new List<LeaseSummary>()); });
        await using var host = await Host.Start(core);
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        var transport = new ClientApiTransport(host.Http, new AppOptions { ApiBaseUrl = host.Address, MaxRetries = 0 }, company);
        var api = new LeaseApi(transport, company);
        Assert.Empty(await api.GetAvailableLeasesAsync(new() { ["FieldId"] = "A", [key] = value }));
        Assert.NotNull(seen); Assert.Equal("A", seen["FieldId"]); Assert.True(seen.TryGetValue(key, out var bound), "The SDK filter was silently dropped by MVC."); Assert.Equal(value, bound);
    }
    [Theory] [InlineData(null, 401)] [InlineData("Reader", 403)] [InlineData("Administrator", 200)]
    public async Task The_actual_endpoint_enforces_standard_Administrator_for_creation_context(string? role, int expected)
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict);
        core.Setup(x => x.GetCreationContextAsync("A")).ReturnsAsync(new LeaseCreationContext(new("tenant", "company"), "fixture-db", "actor", "A", "lease", "component", "status", "UTC"));
        await using var host = await Host.Start(core, role);
        using var response = await host.Http.GetAsync(host.Address + "/api/fields/A/lease-acquisition/creation-context");
        Assert.Equal(expected, (int)response.StatusCode);
        core.Verify(x => x.GetCreationContextAsync("A"), expected == 200 ? Times.Once() : Times.Never());
    }
    [Fact]
    public async Task Explicit_FIELD_and_body_status_reach_the_configured_gate_and_service()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict);
        core.Setup(x => x.UpdateLeaseStatusAsync("lease", "INACTIVE", "actor", "A")).Returns(Task.CompletedTask);
        await using var host = await Host.Start(core);
        using var response = await host.Http.PutAsJsonAsync(host.Address + "/api/fields/A/lease-acquisition/lease/status", new { Status = "INACTIVE" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        core.Verify(x => x.UpdateLeaseStatusAsync("lease", "INACTIVE", "actor", "A"), Times.Once);
        host.Gate.Verify(x => x.RequireFieldAsync(It.IsAny<LeaseDataScope>(), "A", "WRITE", "actor"), Times.Once);
    }
    private sealed class OnlyLeaseController : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> _, ControllerFeature feature)
        { foreach (var controller in feature.Controllers.Where(x => x.AsType() != typeof(LeaseAcquisitionController)).ToArray()) feature.Controllers.Remove(controller); }
    }
    private sealed class Host(WebApplication app, HttpClient http, string address, Mock<ICompanyLeaseDataAccess> gate) : IAsyncDisposable
    {
        public HttpClient Http => http; public string Address => address; public Mock<ICompanyLeaseDataAccess> Gate => gate;
        public static async Task<Host> Start(Mock<ILeaseAcquisitionService> core, string? role = "Administrator")
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" }); builder.WebHost.UseUrls("http://127.0.0.1:0"); builder.Logging.ClearProviders();
            builder.Services.AddAuthentication("fixture").AddScheme<AuthenticationSchemeOptions, FixtureAuthentication>("fixture", _ => { }); builder.Services.AddAuthorization();
            builder.Services.AddSingleton(new FixtureRole(role));
            var scope = new LeaseDataScope(new("tenant", "company"), new("fixture-connection", "fixture-fingerprint"), "actor", "lease", "component", "status", "UTC");
            var gate = new Mock<ICompanyLeaseDataAccess>(MockBehavior.Strict); gate.Setup(x => x.ResolveAsync()).ReturnsAsync(scope);
            gate.Setup(x => x.RequireFieldAsync(scope, "A", "WRITE", "actor")).Returns(Task.CompletedTask);
            builder.Services.AddSingleton(core.Object); builder.Services.AddSingleton(gate.Object);
            builder.Services.AddControllers().AddApplicationPart(typeof(LeaseAcquisitionController).Assembly).ConfigureApplicationPartManager(manager => manager.FeatureProviders.Add(new OnlyLeaseController()));
            var app = builder.Build(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            return new(app, new HttpClient(), address, gate);
        }
        public async ValueTask DisposeAsync() { http.Dispose(); await app.StopAsync(); await app.DisposeAsync(); }
    }
    public sealed record FixtureRole(string? Role);
    public sealed class FixtureAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, FixtureRole role)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(role.Role is null ? AuthenticateResult.NoResult() :
            AuthenticateResult.Success(new(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role.Role)], Scheme.Name)), Scheme.Name)));
    }
}
