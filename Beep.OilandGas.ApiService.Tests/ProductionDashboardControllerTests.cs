using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionDashboardControllerTests
{
    [Fact]
    public async Task BothReadsUseTheExplicitFieldWithoutCurrentFieldDependency()
    {
        var service = new Mock<IPPDMProductionService>(MockBehavior.Strict);
        service.Setup(s => s.GetProductionDashboardSummaryAsync("field-b")).ReturnsAsync(new ProductionDashboardSummary { FieldId = "field-b" });
        service.Setup(s => s.GetProductionWellStatusAsync("field-b")).ReturnsAsync(new List<ProductionWellStatusDto> { new() { WellId = "b1" } });
        var controller = new ProductionDashboardController(service.Object, NullLogger<ProductionDashboardController>.Instance);
        var result = Assert.IsType<OkObjectResult>((await controller.Get("field-b")).Result);
        var response = Assert.IsType<ProductionDashboardResponse>(result.Value);
        Assert.Equal("field-b", response.Summary.FieldId); Assert.Equal("b1", Assert.Single(response.Wells).WellId);
        service.VerifyAll();
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MismatchedSummaryOrFailedWellsCannotReturnPartialSuccess(bool mismatch)
    {
        var service = new Mock<IPPDMProductionService>(MockBehavior.Strict);
        service.Setup(s => s.GetProductionDashboardSummaryAsync("a")).ReturnsAsync(new ProductionDashboardSummary { FieldId = mismatch ? "wrong" : "a" });
        if (!mismatch) service.Setup(s => s.GetProductionWellStatusAsync("a")).ThrowsAsync(new Exception("private details"));
        var controller = new ProductionDashboardController(service.Object, NullLogger<ProductionDashboardController>.Instance);
        var result = Assert.IsType<ObjectResult>((await controller.Get("a")).Result);
        Assert.Equal(500, result.StatusCode); Assert.DoesNotContain("private details", result.Value!.ToString());
        if (mismatch) service.Verify(s => s.GetProductionWellStatusAsync(It.IsAny<string>()), Times.Never);
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EndpointAuthorizesTheRouteFieldThroughExistingAppAccessService(bool allowed)
    {
        Assert.NotNull(typeof(ProductionDashboardController).GetCustomAttribute<AuthorizeAttribute>());
        var guard = typeof(ProductionDashboardController).GetMethod("Get")!.GetCustomAttribute<RequireAssetAccessAttribute>()!;
        var access = new Mock<IAccessControlService>(MockBehavior.Strict);
        access.Setup(a => a.CheckAssetAccessAsync("user", "field-b", "FIELD", null)).ReturnsAsync(new AccessCheckResponse { HasAccess = allowed });
        using var services = new ServiceCollection().AddSingleton(access.Object).BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services, User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user") }, "test")) };
        var routes = new RouteData(); routes.Values["fieldId"] = "field-b";
        var context = new AuthorizationFilterContext(new ActionContext(http, routes, new ActionDescriptor()), new List<IFilterMetadata>());
        await guard.OnAuthorizationAsync(context);
        if (allowed) Assert.Null(context.Result); else Assert.IsType<ForbidResult>(context.Result);
        access.VerifyAll();
    }
}
