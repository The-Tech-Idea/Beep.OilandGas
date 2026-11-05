using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.Models.Data;
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
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

// Actual selector actions/ASP.NET binding/authentication on loopback. Production company/profile/Beep gates are tested separately.
public sealed class FieldSelectionEndpointTests
{
    private static FieldSelectionReview Review => new(new("tenant", "company"), new("connection", "fingerprint"), "actor", "revision", null);
    [Theory] [InlineData("fields")] [InlineData("current")] [InlineData("set-active")]
    public async Task All_selection_operations_require_authentication(string action)
    {
        var service = new Mock<ICompanyFieldSelectionService>(MockBehavior.Strict); await using var host = await Host.Start(service, false);
        using var response = action == "set-active" ? await host.Http.PostAsJsonAsync(host.Url + "/api/field/" + action, new SetActiveFieldRequest { FieldId = "A", Context = Review }) : await host.Http.GetAsync(host.Url + "/api/field/" + action);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); service.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task An_authenticated_member_gets_an_explicit_reviewed_absence_and_the_bound_catalog()
    {
        var service = new Mock<ICompanyFieldSelectionService>(MockBehavior.Strict);
        service.Setup(x => x.GetAvailableFieldsAsync("connection")).ReturnsAsync([new() { FieldId = "A", FieldName = "Recorded" }]);
        service.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(new FieldResponse { FieldId = "", SelectionAvailable = false, SelectionContext = Review });
        await using var host = await Host.Start(service);
        Assert.Equal("A", Assert.Single((await host.Http.GetFromJsonAsync<List<FieldListItem>>(host.Url + "/api/field/fields?connectionName=connection"))!).FieldId);
        var current = await host.Http.GetFromJsonAsync<FieldResponse>(host.Url + "/api/field/current"); Assert.False(current!.SelectionAvailable); Assert.Equal(Review, current.SelectionContext); Assert.Equal("", current.FieldId);
    }
    [Fact]
    public async Task The_exact_body_reaches_the_existing_profile_owner_and_its_acknowledgement_reaches_the_member()
    {
        SetActiveFieldRequest? captured = null; var service = new Mock<ICompanyFieldSelectionService>(MockBehavior.Strict);
        service.Setup(x => x.SetCurrentFieldSelectionAsync(It.IsAny<SetActiveFieldRequest>())).Returns((SetActiveFieldRequest request) => {
            captured = request; return Task.FromResult(new SetActiveFieldResponse { Success = true, FieldId = "A", Context = Review with { FieldId = "A", Revision = "new" } }); });
        await using var host = await Host.Start(service); var intent = new SetActiveFieldRequest { FieldId = "A", ConnectionName = "connection", Context = Review };
        using var response = await host.Http.PostAsJsonAsync(host.Url + "/api/field/set-active", intent); Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var acknowledgement = await response.Content.ReadFromJsonAsync<SetActiveFieldResponse>(); FieldSelectionReviewValidator.RequireAcknowledgement(acknowledgement, intent);
        Assert.Equal("A", captured!.FieldId); Assert.Equal("connection", captured.ConnectionName); Assert.Equal(Review, captured.Context);
    }
    [Fact]
    public async Task A_body_without_required_review_evidence_is_rejected_by_MVC_before_the_owner()
    {
        var service = new Mock<ICompanyFieldSelectionService>(MockBehavior.Strict); await using var host = await Host.Start(service);
        using var response = await host.Http.PostAsJsonAsync(host.Url + "/api/field/set-active", new { FieldId = "A" }); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); service.VerifyNoOtherCalls();
    }
    private sealed class OnlySelection : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> _, ControllerFeature feature)
        { foreach (var type in feature.Controllers.Where(x => x.AsType() != typeof(FieldOrchestratorController)).ToArray()) feature.Controllers.Remove(type); }
    }
    private sealed class Host(WebApplication app, string url) : IAsyncDisposable
    {
        public string Url => url; public HttpClient Http { get; } = new();
        public static async Task<Host> Start(Mock<ICompanyFieldSelectionService> service, bool authenticated = true)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" }); builder.WebHost.UseUrls("http://127.0.0.1:0"); builder.Logging.ClearProviders();
            builder.Services.AddAuthentication("fixture").AddScheme<AuthenticationSchemeOptions, Authentication>("fixture", _ => { }); builder.Services.AddAuthorization();
            builder.Services.AddSingleton(new PrincipalFixture(authenticated)); builder.Services.AddSingleton(service.Object);
            builder.Services.AddControllers().AddApplicationPart(typeof(FieldOrchestratorController).Assembly).ConfigureApplicationPartManager(manager => manager.FeatureProviders.Add(new OnlySelection()));
            var app = builder.Build(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); await app.StartAsync();
            return new(app, app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
        }
        public async ValueTask DisposeAsync() { Http.Dispose(); await app.StopAsync(); await app.DisposeAsync(); }
    }
    public sealed record PrincipalFixture(bool Authenticated);
    public sealed class Authentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, PrincipalFixture principal)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(!principal.Authenticated ? AuthenticateResult.NoResult() :
            AuthenticateResult.Success(new(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "fixture-member")], Scheme.Name)), Scheme.Name)));
    }
}
