using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// <see cref="RepositoryRolesClaimsTransformation"/>: the repository's roles for the account the client library resolved,
/// and nothing a token carried.
/// </summary>
public class RepositoryClaimsTransformationTests
{
    private const string Issuer = "https://idp.oilgas.test/";

    [Fact]
    public async Task An_active_account_carries_the_repository_s_roles_and_none_of_the_token_s()
    {
        var access = Access(new RepositoryUserAccess("local-id", true, ["Viewer"], ["Read"]));
        var source = Resolved("local-id");

        var result = await Transformation(access.Object).TransformAsync(source);

        Assert.True(result.IsInRole("Viewer"));
        Assert.False(result.IsInRole("Administrator"));
        Assert.Equal(["Read"], result.FindAll(RepositoryRolesClaimsTransformation.Permission).Select(claim => claim.Value));
        Assert.Null(result.FindFirst("permissions"));
        Assert.Null(result.FindFirst("elevated_permissions"));
        Assert.Null(result.FindFirst(ClaimTypes.NameIdentifier));
        Assert.True(RepositoryAuthorization.IsActiveAccount(result));
        Assert.Equal("local-id", PartyIdClaims.Find(result));
        Assert.True(source.IsInRole("Administrator"));
    }

    [Fact]
    public async Task Roles_are_read_once_per_request_and_a_marker_a_token_carried_does_not_skip_them()
    {
        var access = Access(new RepositoryUserAccess("local-id", true, ["Viewer"], []));
        var transformation = Transformation(access.Object);

        var result = await transformation.TransformAsync(Resolved("local-id"));
        Assert.Same(result, await transformation.TransformAsync(result));

        access.Verify(x => x.GetAccessAsync("local-id", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("disabled")]
    [InlineData("gone")]
    [InlineData("outage")]
    public async Task An_account_that_is_off_gone_or_unreadable_holds_no_role_and_is_not_active(string state)
    {
        var access = new Mock<IRepositoryAccessService>(MockBehavior.Strict);
        var lookup = access.Setup(x => x.GetAccessAsync("local-id", It.IsAny<CancellationToken>()));
        if (state == "outage") lookup.ThrowsAsync(new InvalidOperationException("unavailable"));
        else lookup.ReturnsAsync(state == "gone" ? null : new RepositoryUserAccess("local-id", false, ["Administrator"], ["Read"]));

        var result = await Transformation(access.Object).TransformAsync(Resolved("local-id"));

        Assert.Empty(result.FindAll(ClaimTypes.Role));
        Assert.Empty(result.FindAll(RepositoryRolesClaimsTransformation.Permission));
        Assert.False(RepositoryAuthorization.IsActiveAccount(result));
        Assert.True(result.Identity!.IsAuthenticated);
    }

    [Fact]
    public async Task A_person_with_no_account_this_API_resolved_is_not_asked_about_and_holds_nothing()
    {
        var access = new Mock<IRepositoryAccessService>(MockBehavior.Strict);
        var fromToken = Token(("party_id", "local-id"), (RepositoryRolesClaimsTransformation.ActiveAccount, "true"));

        var result = await Transformation(access.Object).TransformAsync(fromToken);

        Assert.Empty(result.FindAll(ClaimTypes.Role));
        Assert.False(RepositoryAuthorization.IsActiveAccount(result));
        access.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task An_application_acting_for_itself_holds_nothing()
    {
        var access = new Mock<IRepositoryAccessService>(MockBehavior.Strict);
        var machine = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", "reporting-job", ClaimValueTypes.String, Issuer),
            new Claim("client_id", "reporting-job", ClaimValueTypes.String, Issuer),
            new Claim(PartyIdClaimsTransformation<string>.ClaimType, "local-id")
        ], "Bearer"));

        var result = await Transformation(access.Object).TransformAsync(machine);

        Assert.False(RepositoryAuthorization.IsActiveAccount(result));
        access.VerifyNoOtherCalls();
    }

    private static RepositoryRolesClaimsTransformation Transformation(IRepositoryAccessService access) =>
        new(access, NullLogger<RepositoryRolesClaimsTransformation>.Instance);

    private static Mock<IRepositoryAccessService> Access(RepositoryUserAccess answer)
    {
        var access = new Mock<IRepositoryAccessService>(MockBehavior.Strict);
        access.Setup(x => x.GetAccessAsync(answer.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(answer);
        return access;
    }

    /// <summary>A validated token's principal, carrying authorization claims of its own that must count for nothing.</summary>
    private static ClaimsPrincipal Token(params (string Type, string Value)[] extra) => new(new ClaimsIdentity(
        new[]
        {
            new Claim("sub", "subject", ClaimValueTypes.String, Issuer),
            new Claim("client_id", "oilgas-web", ClaimValueTypes.String, Issuer),
            new Claim(ClaimTypes.Role, "Administrator", ClaimValueTypes.String, Issuer),
            new Claim("role", "Administrator", ClaimValueTypes.String, Issuer),
            new Claim("permission", "Admin.ManageUsers", ClaimValueTypes.String, Issuer),
            new Claim("permissions", "Admin.ManageUsers,Admin.AssignRoles", ClaimValueTypes.String, Issuer),
            new Claim("elevated_permissions", "Admin.AssignRoles", ClaimValueTypes.String, Issuer),
            new Claim(ClaimTypes.NameIdentifier, "forged-local-id", ClaimValueTypes.String, Issuer),
            new Claim(RepositoryRolesClaimsTransformation.ResolvedMarker, "true", ClaimValueTypes.String, Issuer)
        }.Concat(extra.Select(claim => new Claim(claim.Type, claim.Value, ClaimValueTypes.String, Issuer))),
        "Bearer", "name", ClaimTypes.Role));

    /// <summary>The token's principal after the client library resolved it: <c>party_id</c> issued here.</summary>
    private static ClaimsPrincipal Resolved(string partyId)
    {
        var principal = Token();
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(PartyIdClaimsTransformation<string>.ClaimType, partyId));
        return principal;
    }
}
