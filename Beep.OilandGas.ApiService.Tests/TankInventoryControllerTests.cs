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
    [InlineData("sub", null)]
    [InlineData(ClaimTypes.NameIdentifier, null)]
    [InlineData("party_id", "https://idp.example.test/")]
    public async Task CreateRequiresTheAccountThisApiResolved(string claimType, string? issuer)
    {
        var claim = issuer is null ? new Claim(claimType, "actor") : new Claim(claimType, "actor", ClaimValueTypes.String, issuer);
        var identity = new ClaimsIdentity([claim], "test");
        await Refusals.ForbiddenAsync(() =>
            Controller(identity).CreateTankInventory(new CreateTankInventoryRequest { TankBatteryId = "tank" }));
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
        // OILGAS-CATCH-01: the controller no longer answers a failure itself; it reaches the API's exception handler,
        // which reports it and answers 500 with its reference and never its text (ExceptionAnswerTests).
        await Assert.ThrowsAsync<InvalidOperationException>(() => Controller(LocalIdentity()).GetTankInventory("inventory", "other-db"));
        Assert.Equal(1, _resolutions);
        _editor.VerifyNoOtherCalls();
        _metadata.VerifyNoOtherCalls();
    }

    private static ClaimsIdentity LocalIdentity() => new([new Claim("party_id", "local-user")], "test");
}
