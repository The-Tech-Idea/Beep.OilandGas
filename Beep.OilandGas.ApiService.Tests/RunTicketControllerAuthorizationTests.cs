using System.Security.Claims;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Controllers.Accounting.Production;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class RunTicketControllerAuthorizationTests
{
    [Theory]
    [InlineData("sub", null, false)]
    [InlineData(ClaimTypes.NameIdentifier, null, false)]
    [InlineData("party_id", "https://idp.example.test/", false)]
    [InlineData("sub", null, true)]
    [InlineData(ClaimTypes.NameIdentifier, null, true)]
    [InlineData("party_id", "https://idp.example.test/", true)]
    public async Task WritesRejectACallerWithoutTheAccountThisApiResolved(string claimType, string? issuer, bool cycle)
    {
        var production = new Mock<IProductionAccountingService>(MockBehavior.Strict);
        var journal = new Mock<IJournalEntryService>(MockBehavior.Strict);
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var store = new RunTicketStore(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object,
            () => throw new InvalidOperationException("Unauthorized requests must not resolve storage."));
        var integration = new GLIntegrationService(journal.Object,
            new GLAccountMappingService(journal.Object, NullLogger<GLAccountMappingService>.Instance),
            NullLogger<GLIntegrationService>.Instance);
        var controller = new RunTicketController(store, production.Object, integration,
            NullLogger<RunTicketController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([issuer is null
                        ? new Claim(claimType, "actor")
                        : new Claim(claimType, "actor", ClaimValueTypes.String, issuer)], "test"))
                }
            }
        };

        if (cycle)
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.ProcessProductionCycleAsync(new RUN_TICKET()));
        else
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.CreateRunTicket(new CreateRunTicketRequest()));

        production.VerifyNoOtherCalls();
        journal.VerifyNoOtherCalls();
        editor.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }
}
