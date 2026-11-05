using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class MigrationAdministratorAuthorityTests
{
    private static ClaimsPrincipal Principal() => new(new ClaimsIdentity(
        [new("party_id", "local-admin"), new(ClaimTypes.Role, "Administrator")], "test"));

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task SignedRoleClaimCannotReplaceCurrentRepositoryAuthority(bool active, bool administrator)
    {
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetAccessAsync("local-admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RepositoryUserAccess("local-admin", active, administrator ? ["Administrator"] : [], []));
        await Assert.ThrowsAsync<RefusalException>(() => MigrationAdministratorAuthority.RequireAsync(Principal(), access.Object, default));
    }

    [Fact]
    public async Task ReturnsResolvedLocalActorAndPassesRequestCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var access = new Mock<IRepositoryAccessService>(MockBehavior.Strict);
        access.Setup(x => x.GetAccessAsync("local-admin", cancellation.Token))
            .ReturnsAsync(new RepositoryUserAccess("local-admin", true, ["Administrator"], []));
        Assert.Equal("local-admin", await MigrationAdministratorAuthority.RequireAsync(Principal(), access.Object, cancellation.Token));
        access.VerifyAll();
    }

    [Fact]
    public async Task MissingLocalActorNeverQueriesRepository()
    {
        var access = new Mock<IRepositoryAccessService>(MockBehavior.Strict);
        await Assert.ThrowsAnyAsync<Exception>(() => MigrationAdministratorAuthority.RequireAsync(new ClaimsPrincipal(), access.Object, default));
        access.VerifyNoOtherCalls();
    }
}
#endif
