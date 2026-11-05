using Beep.OilandGas.Repository.Models;
#if MIGRATION_CONTROLLER_FOCUSED
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.PPDM39;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class MigrationDispatchAuthorityTests
{
    [Fact]
    public async Task RevocationPreventsAllMigrationDispatchEvenWithAdministratorClaim()
    {
        var migration = new Mock<IPPDM39SchemaMigrationService>(MockBehavior.Strict);
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetAccessAsync("actor", It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryUserAccess("actor", true, [], []));
        var controller = new PPDM39SetupController(migration.Object)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test")) } } };
        await Assert.ThrowsAsync<RefusalException>(() => controller.PlanSchemaMigration(new(), access.Object));
        await Assert.ThrowsAsync<RefusalException>(() => controller.ApproveSchemaMigration(new(), access.Object));
        await Assert.ThrowsAsync<RefusalException>(() => controller.ExecuteSchemaMigration(new(), access.Object));
        await Assert.ThrowsAsync<RefusalException>(() => controller.StartSchemaMigration(new(), access.Object));
        migration.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ApprovedCommandsRecordTheResolvedActorRatherThanClientInput()
    {
        var migration = new Mock<IPPDM39SchemaMigrationService>(MockBehavior.Strict);
        migration.Setup(x => x.ApproveSchemaMigrationPlanAsync(It.Is<SchemaMigrationApprovalRequest>(r => r.ApprovedBy == "actor"))).ReturnsAsync(new SchemaMigrationApprovalResult { Success = true });
        migration.Setup(x => x.ExecuteSchemaMigrationPlanAsync(It.Is<SchemaMigrationExecuteRequest>(r => r.ExecutedBy == "actor"))).ReturnsAsync(new SchemaMigrationExecuteResult { Success = true });
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetAccessAsync("actor", It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryUserAccess("actor", true, ["Administrator"], []));
        var controller = new PPDM39SetupController(migration.Object)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor")], "test")) } } };
        Assert.IsType<OkObjectResult>((await controller.ApproveSchemaMigration(new() { ApprovedBy = "forged" }, access.Object)).Result);
        Assert.IsType<OkObjectResult>((await controller.ExecuteSchemaMigration(new() { ExecutedBy = "forged" }, access.Object)).Result);
        migration.VerifyAll();
    }
}
#endif
