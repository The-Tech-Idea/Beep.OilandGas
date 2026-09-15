using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Accounting.Production;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Inventory;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class TankInventoryControllerTests
{
    private readonly Mock<IDMEEditor> _editor = new(MockBehavior.Strict);
    private readonly Mock<IPPDMMetadataRepository> _metadata = new(MockBehavior.Strict);
    private int _resolutions;

    private ProductionController Controller(ClaimsIdentity identity)
    {
        Task<string> Resolve() { _resolutions++; return Task.FromResult(""); }
        var store = new TankInventoryStore(_editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), _metadata.Object, Resolve);
        return new ProductionController(store, NullLogger<ProductionController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
    }

    [Theory]
    [InlineData("sub", true)]
    [InlineData(ClaimTypes.NameIdentifier, false)]
    public async Task CreateRequiresAuthenticatedLocalIdentity(string claimType, bool authenticated)
    {
        var identity = new ClaimsIdentity([new Claim(claimType, "actor")], authenticated ? "test" : null);
        var result = await Controller(identity).CreateTankInventory(new CreateTankInventoryRequest { TankBatteryId = "tank" });
        Assert.IsType<ForbidResult>(result.Result);
        Assert.Equal(0, _resolutions);
        _editor.VerifyNoOtherCalls();
        _metadata.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvalidVolumeReturnsBadRequestWithoutDatabaseAccess()
    {
        var result = await Controller(LocalIdentity()).CreateTankInventory(new CreateTankInventoryRequest
            { TankBatteryId = "tank", ActualClosingInventory = -1m });
        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(0, _resolutions);
        _editor.VerifyNoOtherCalls();
        _metadata.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingBindingReturnsServerErrorRatherThanNotFound()
    {
        var result = await Controller(LocalIdentity()).GetTankInventory("inventory", "other-db");
        Assert.Equal(500, Assert.IsType<ObjectResult>(result.Result).StatusCode);
        Assert.Equal(1, _resolutions);
        _editor.VerifyNoOtherCalls();
        _metadata.VerifyNoOtherCalls();
    }

    private static ClaimsIdentity LocalIdentity() => new([new Claim(ClaimTypes.NameIdentifier, "local-user")], "test");
}
