using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if BEEP_SOURCE_TEST && WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyEnvironmentPreviewControllerTests
{
    [Theory]
    [InlineData("KW", "{}", true, 409)]
    [InlineData("XX", "{}", true, 400)]
    [InlineData("KW", "{}", false, 403)]
    public async Task PreviewDistinguishesMissingSharedDefaultsInvalidDraftAndLostAccess(string country, string json, bool administrator, int expected)
    {
        var scope = new CompanyScope("tenant", "company");
        var membership = new RepositoryCompanyAccess(scope, ["Administrator"], []);
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetCompanyAccessAsync("actor", scope, It.IsAny<CancellationToken>())).ReturnsAsync(() => administrator ? membership : null);
        var resolver = new RepositoryCompanyContextResolver(access.Object);
        var context = new RepositoryTenantContext();
        var initialAccess = new Mock<IRepositoryAccessService>();
        initialAccess.Setup(x => x.GetCompanyAccessAsync("actor", scope, It.IsAny<CancellationToken>())).ReturnsAsync(membership);
        var selected = await new RepositoryCompanyContextResolver(initialAccess.Object).ResolveAsync("actor", scope);
        context.SelectCompany(selected!);
        var store = new Mock<ICompanyEnvironmentStore>(); var shared = new Mock<ISystemSetupRecordReader>();
        shared.Setup(x => x.ReadAsync("TOOLS", "DEFAULT", It.IsAny<Func<Task>>())).ReturnsAsync((SystemSetupConfiguration?)null);
        var effective = new CompanyEnvironmentEffectiveResolver(store.Object, shared.Object);
        var controller = new CompanyEnvironmentController(context, resolver, store.Object, NullLogger<CompanyEnvironmentController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "actor"), new Claim(ClaimTypes.Role, "Administrator")], "test")) } } };
        var result = await controller.Preview(new(country, json), effective, default);
        if (expected == 403)
        {
            Assert.IsType<ForbidResult>(result);
            shared.Verify(x => x.ReadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<Task>>()), Times.Never);
        }
        else
        {
            var response = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(expected, response.StatusCode);
            if (expected == 409) Assert.Contains("shared system", System.Text.Json.JsonSerializer.Serialize(response.Value), StringComparison.OrdinalIgnoreCase);
        }
    }
}
#endif
