using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class CompanyPersonaContextTests
{
    private sealed class Auth : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Reader")], "test"))));
    }
    private sealed class Client(CompanyClientContext company) : IPersonaClient
    {
        public AppUserPersona? Profile = new() { UserId = "actor", TenantId = "tenant", BusinessAssociateId = "A", PersonaCode = "ACCOUNTANT" };
        public Func<Task<AppUserPersona?>>? Pending;
        public int Calls;
        public int Saves;
        public Func<CompanyPersonaWorkspaceReview, CompanyPersonaWorkspaceReview>? ChangeReview;
        public Func<Task<CompanyPersonaWorkspaceReview>>? PendingReview;
        public Func<AppUserPersona, AppUserPersona>? ChangeSaved;
        public Task<AppUserPersona?> GetAsync(string user) { Calls++; if (Pending is not null) return Pending(); if (Profile is not null) { Profile.TenantId = company.RequireSelection().Scope.TenantId; Profile.BusinessAssociateId = company.RequireSelection().Scope.BusinessAssociateId; } return Task.FromResult(Profile); }
        public Task<CompanyPersonaWorkspaceReview> ReviewWorkspacesAsync()
        {
            Calls++;
            var selected = company.RequireSelection();
            var review = new CompanyPersonaWorkspaceReview(selected.Scope, selected.UserId, selected.Roles,
                [new() { Code = "ACCOUNTANT", Name = "Accountant", DefaultRoute = "/accounting/dashboard", IsActive = true }], new string('A', 64));
            return PendingReview?.Invoke() ?? Task.FromResult(ChangeReview?.Invoke(review) ?? review);
        }
        public Task<AppUserPersona> SaveAsync(string user, PersonaProfileUpdate request)
        {
            Saves++; var selected = company.RequireSelection(); var saved = new AppUserPersona { UserId = user, TenantId = selected.Scope.TenantId, BusinessAssociateId = selected.Scope.BusinessAssociateId, PersonaCode = request.PersonaCode };
            return Task.FromResult(ChangeSaved?.Invoke(saved) ?? saved);
        }
    }
    private static void Select(CompanyClientContext company, string name, string actor = "actor") => Assert.True(company.TrySelect(company.Clear(), actor, new(new("tenant", name), ["Reader"], [])));
    [Fact]
    public async Task SwitchingCompanyClearsTheOldWorkspaceAndRequiresFreshData()
    {
        using var company = new CompanyClientContext(); Select(company, "A"); var client = new Client(company);
        using var state = new PersonaContextService(client, new Auth(), company, user => user.FindFirst("party_id")?.Value);
        await state.EnsureLoadedAsync(); Assert.NotNull(state.CurrentPersona); Assert.Equal(2, client.Calls);
        Select(company, "B"); Assert.Null(state.CurrentPersona); Assert.Null(state.CurrentProfile); Assert.Empty(state.Catalog);
        await state.EnsureLoadedAsync(); Assert.NotNull(state.CurrentPersona); Assert.Equal(4, client.Calls);
    }
    [Fact]
    public async Task AReturnedProfileCannotPublishAfterAnAwayAndBackCompanySwitch()
    {
        using var company = new CompanyClientContext(); Select(company, "A"); var client = new Client(company);
        var pending = new TaskCompletionSource<AppUserPersona?>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Pending = () => pending.Task;
        using var state = new PersonaContextService(client, new Auth(), company, user => user.FindFirst("party_id")?.Value);
        var load = state.EnsureLoadedAsync(); Assert.Equal(1, client.Calls);
        Select(company, "B"); Select(company, "A"); pending.SetResult(client.Profile); await load;
        Assert.Equal(1, client.Calls);
        Assert.Null(state.CurrentPersona); Assert.Null(state.CurrentProfile); Assert.Empty(state.Catalog);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task MissingOrForeignAccountCompanyDoesNotRequestPersonaData(bool foreign)
    {
        using var company = new CompanyClientContext(); if (foreign) Select(company, "A", "other"); var client = new Client(company);
        using var state = new PersonaContextService(client, new Auth(), company, user => user.FindFirst("party_id")?.Value);
        await state.EnsureLoadedAsync(); Assert.Equal(0, client.Calls); Assert.Empty(state.Catalog);
    }
    [Fact]
    public async Task AForeignAccountProfileIsNotPublished()
    {
        using var company = new CompanyClientContext(); Select(company, "A"); var client = new Client(company) { Profile = new() { UserId = "other", PersonaCode = "ACCOUNTANT" } };
        using var state = new PersonaContextService(client, new Auth(), company, user => user.FindFirst("party_id")?.Value);
        await Assert.ThrowsAsync<InvalidOperationException>(() => state.EnsureLoadedAsync()); Assert.Null(state.CurrentProfile); Assert.Empty(state.Catalog);
    }
    [Fact]
    public async Task AProfileFromAnotherCompanyIsNotPublished()
    {
        using var company = new CompanyClientContext(); Select(company, "A"); var client = new Client(company);
        client.Pending = () => Task.FromResult<AppUserPersona?>(new() { UserId = "actor", TenantId = "tenant", BusinessAssociateId = "B", PersonaCode = "ACCOUNTANT" });
        using var state = new PersonaContextService(client, new Auth(), company, user => user.FindFirst("party_id")?.Value);
        await Assert.ThrowsAsync<InvalidOperationException>(() => state.EnsureLoadedAsync());
        Assert.Null(state.CurrentProfile); Assert.Empty(state.Catalog);
    }
    [Fact]
    public async Task DisposingTheContextUnsubscribesFromCompanyChanges()
    {
        using var company = new CompanyClientContext(); Select(company, "A");
        var state = new PersonaContextService(new Client(company), new Auth(), company, user => user.FindFirst("party_id")?.Value); await state.EnsureLoadedAsync(); state.Dispose();
        var events = 0; state.Changed += () => events++; Select(company, "B"); Assert.Equal(0, events);
    }

    [Theory]
    [InlineData("company")] [InlineData("actor")] [InlineData("roles")] [InlineData("revision")]
    [InlineData("inactive")] [InlineData("duplicate")]
    public async Task UntrustedWorkspaceReviewIsRejectedBeforePublishing(string mutation)
    {
        using var company = new CompanyClientContext(); Select(company, "A"); var client = new Client(company);
        client.ChangeReview = row => mutation switch {
            "company" => row with { Company = new("tenant", "B") },
            "actor" => row with { ActorUserId = "other" },
            "roles" => row with { Roles = ["Administrator"] },
            "revision" => row with { Revision = "invalid" },
            "inactive" => row with { Personas = [new() { Code = "ACCOUNTANT", Name = "Accountant", IsActive = false }] },
            _ => row with { Personas = [row.Personas[0], row.Personas[0]] }
        };
        using var state = new PersonaContextService(client, new Auth(), company, user => user.FindFirst("party_id")?.Value);
        await Assert.ThrowsAsync<InvalidOperationException>(() => state.EnsureLoadedAsync());
        Assert.Empty(state.Catalog); Assert.Null(state.CurrentProfile); Assert.Equal(0, client.Saves);
        Assert.Equal(["Reader"], company.Selection!.Roles);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task ChangedEligibilityIsRejectedBeforeSaving(bool removed)
    {
        using var company = new CompanyClientContext(); Select(company, "A"); var client = new Client(company);
        using var state = new PersonaContextService(client, new Auth(), company, user => user.FindFirst("party_id")?.Value);
        await state.EnsureLoadedAsync();
        client.ChangeReview = row => removed ? row with { Personas = [] } : row with { Revision = new string('B', 64) };
        await Assert.ThrowsAsync<InvalidOperationException>(() => state.SwitchAsync("ACCOUNTANT")); Assert.Equal(0, client.Saves);
    }
    [Fact]
    public async Task CompanySwitchDuringFreshReviewPreventsPreferenceWrite()
    {
        using var company = new CompanyClientContext(); Select(company, "A"); var client = new Client(company);
        using var state = new PersonaContextService(client, new Auth(), company, user => user.FindFirst("party_id")?.Value);
        await state.EnsureLoadedAsync(); var review = await client.ReviewWorkspacesAsync();
        var pending = new TaskCompletionSource<CompanyPersonaWorkspaceReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.PendingReview = () => pending.Task; var change = state.SwitchAsync("ACCOUNTANT");
        Select(company, "B"); pending.SetResult(review);
        await Assert.ThrowsAsync<OperationCanceledException>(() => change); Assert.Equal(0, client.Saves); Assert.Empty(state.Catalog);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task MismatchedSaveReadbackCannotPublish(bool foreignActor)
    {
        using var company = new CompanyClientContext(); Select(company, "A"); var client = new Client(company);
        using var state = new PersonaContextService(client, new Auth(), company, user => user.FindFirst("party_id")?.Value);
        await state.EnsureLoadedAsync();
        client.ChangeSaved = row => new() { UserId = foreignActor ? "other" : row.UserId, PersonaCode = foreignActor ? row.PersonaCode : "OTHER" };
        await Assert.ThrowsAsync<InvalidOperationException>(() => state.SwitchAsync("ACCOUNTANT"));
        Assert.Equal("actor", state.CurrentProfile!.UserId); Assert.Equal("ACCOUNTANT", state.CurrentPersona!.Code);
    }
}
