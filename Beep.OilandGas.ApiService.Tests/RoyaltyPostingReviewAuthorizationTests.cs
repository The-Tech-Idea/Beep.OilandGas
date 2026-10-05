using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Accounting.Royalty;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RoyaltyPostingReviewAuthorizationTests
{
    [Theory]
    [InlineData(false, null)]
    [InlineData(true, "https://idp.example.test/")]
    public async Task RequiresTheAccountThisApiResolved(bool local, string? issuer)
    {
        var royalties = new Mock<IRoyaltyService>(MockBehavior.Strict);
        var access = new Mock<IAccessControlService>(MockBehavior.Strict);
        await Refusals.ForbiddenAsync(() =>
            Controller(royalties.Object, access.Object, local, issuer).ReviewPostings("royalty"));
        royalties.VerifyNoOtherCalls();
        access.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PermissionDenialDoesNotReadSourceOrWrite()
    {
        var royalties = new Mock<IRoyaltyService>(MockBehavior.Strict);
        var access = new Mock<IAccessControlService>(MockBehavior.Strict);
        access.Setup(a => a.HasPermissionAsync("local", "Accounting.PostJournal", null)).ReturnsAsync(false);
        Assert.IsType<ForbidResult>((await Controller(royalties.Object, access.Object).ReviewPostings("royalty")));
        royalties.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AuthorizesStoredSourceFieldBeforePayment(bool allowed)
    {
        var royalties = new Mock<IRoyaltyService>(MockBehavior.Strict);
        var access = new Mock<IAccessControlService>(MockBehavior.Strict);
        access.Setup(a => a.HasPermissionAsync("local", "Accounting.PostJournal", null)).ReturnsAsync(true);
        royalties.Setup(r => r.GetAsync("royalty", "PPDM39")).ReturnsAsync(new ROYALTY_CALCULATION { ALLOCATION_DETAIL_ID = "detail" });
        royalties.Setup(r => r.GetAllocationFieldAsync("detail")).ReturnsAsync("stored-field");
        access.Setup(a => a.CheckAssetAccessAsync("local", "stored-field", "FIELD", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = allowed });
        if (allowed) royalties.Setup(r => r.ReviewPostingsAsync("royalty")).ReturnsAsync(new List<RoyaltyPostingReview>());
        var result = await Controller(royalties.Object, access.Object).ReviewPostings("royalty");
        if (allowed) Assert.IsType<OkObjectResult>(result);
        else Assert.IsType<ForbidResult>(result);
        royalties.Verify(r => r.ReviewPostingsAsync(It.IsAny<string>()), allowed ? Times.Once() : Times.Never());
    }

    private static RoyaltyController Controller(IRoyaltyService royalties, IAccessControlService access,
        bool local = true, string? issuer = null)
    {
        // Without an account this API resolved, the caller carries only what a token or another scheme could name.
        var claims = new List<Claim> { new("sub", "external"), new(ClaimTypes.Role, "Administrator") };
        claims.Add(!local ? new Claim(ClaimTypes.NameIdentifier, "local")
            : issuer is null ? new Claim("party_id", "local") : new Claim("party_id", "local", ClaimValueTypes.String, issuer));
        return new(royalties, access, NullLogger<RoyaltyController>.Instance) { ControllerContext = new() {
            HttpContext = new DefaultHttpContext { User = new(new ClaimsIdentity(claims, "test")) }
        } };
    }
}
