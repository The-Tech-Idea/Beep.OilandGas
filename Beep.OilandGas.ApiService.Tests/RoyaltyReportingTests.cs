using System.Globalization;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Accounting.Royalty;
using Beep.OilandGas.LifeCycle.Services.Accounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RoyaltyReportingTests
{
    private readonly Mock<IDMEEditor> _editor = new(MockBehavior.Strict);
    private readonly Mock<IDataSource> _source = new();
    private readonly Dictionary<string, List<object>> _rows = new();
    private PPDMAccountingService Service(Func<Task<string>>? resolve = null) => new(_editor.Object,
        Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
        resolveProductionConnection: resolve ?? (() => Task.FromResult("bound-production")));

    public RoyaltyReportingTests()
    {
        _editor.Setup(e => e.GetDataSource("bound-production")).Returns(_source.Object);
        foreach (var table in new[] { "RUN_TICKET", "ALLOCATION_RESULT", "ALLOCATION_DETAIL", "ROYALTY_CALCULATION" })
            _rows[table] = new();
        _source.Setup(s => s.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>()))
            .Returns((string table, List<AppFilter> filters) => Task.FromResult<IEnumerable<object>>(
                _rows[table].Where(row => filters.All(f => Matches(row, f))).ToList()));
        AddChain("a", "field-a");
        AddChain("b", "field-b");
    }

    private void AddChain(string id, string field)
    {
        _rows["RUN_TICKET"].Add(new RUN_TICKET { RUN_TICKET_ID = id, FIELD_ID = field, LEASE_ID = "shared-lease", ACTIVE_IND = "N" });
        _rows["ALLOCATION_RESULT"].Add(new ALLOCATION_RESULT { ALLOCATION_RESULT_ID = id, ALLOCATION_REQUEST_ID = id, ACTIVE_IND = "N" });
        _rows["ALLOCATION_DETAIL"].Add(new ALLOCATION_DETAIL { ALLOCATION_DETAIL_ID = id, ALLOCATION_RESULT_ID = id, ACTIVE_IND = "N" });
        _rows["ROYALTY_CALCULATION"].Add(new ROYALTY_CALCULATION { ROYALTY_CALCULATION_ID = id, ALLOCATION_DETAIL_ID = id,
            PROPERTY_OR_LEASE_ID = "shared-lease", CALCULATION_DATE = new(2026, 9, 2, 23, 59, 59), ACTIVE_IND = "Y" });
    }

    private static bool Matches(object row, AppFilter filter)
    {
        var value = row.GetType().GetProperty(filter.FieldName)!.GetValue(row);
        if (filter.Operator == "=") return value?.ToString() == filter.FilterValue;
        if (value is not DateTime date) return false;
        var bound = DateTime.Parse(filter.FilterValue, CultureInfo.InvariantCulture);
        return filter.Operator == ">=" ? date >= bound : date < bound;
    }

    [Fact]
    public async Task FieldComesFromTicketNotLeaseAndArchivedParentsRemainReportable()
    {
        var records = await Service().GetRoyaltyCalculationsAsync("field-a", new(2026, 9, 2), new(2026, 9, 2));
        Assert.Equal("a", Assert.Single(records).ROYALTY_CALCULATION_ID);
        _editor.Verify(e => e.GetDataSource("bound-production"), Times.AtLeastOnce);
        _source.Verify(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _source.Verify(s => s.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task InclusiveCalculationDatesExcludeNextDayAndInactiveRecords()
    {
        _rows["ROYALTY_CALCULATION"].AddRange(new[] {
            new ROYALTY_CALCULATION { ROYALTY_CALCULATION_ID = "tomorrow", ALLOCATION_DETAIL_ID = "a", CALCULATION_DATE = new(2026, 9, 3), ACTIVE_IND = "Y" },
            new ROYALTY_CALCULATION { ROYALTY_CALCULATION_ID = "inactive", ALLOCATION_DETAIL_ID = "a", CALCULATION_DATE = new(2026, 9, 2), ACTIVE_IND = "N" }
        });
        Assert.Equal("a", Assert.Single(await Service().GetRoyaltyCalculationsAsync("field-a", new(2026, 9, 2), new(2026, 9, 2))).ROYALTY_CALCULATION_ID);
        Assert.Empty(await Service().GetRoyaltyCalculationsAsync("unknown"));
    }

    [Theory]
    [InlineData("RUN_TICKET")]
    [InlineData("ALLOCATION_RESULT")]
    [InlineData("ALLOCATION_DETAIL")]
    [InlineData("ROYALTY_CALCULATION")]
    public async Task DuplicateIdentitiesFailInsteadOfDoublingFinancialTotals(string table)
    {
        _rows[table].Add(_rows[table][0]);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().GetRoyaltyCalculationsAsync("field-a"));
    }

    [Fact]
    public async Task MissingBindingAndInvalidFiltersNeverOpenDatabase()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(() => Task.FromResult("")).GetRoyaltyCalculationsAsync("field-a"));
        await Assert.ThrowsAsync<ArgumentException>(() => Service().GetRoyaltyCalculationsAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => Service().GetRoyaltyCalculationsAsync("field-a", new(2026, 9, 3), new(2026, 9, 2)));
        await Assert.ThrowsAsync<ArgumentException>(() => Service().GetRoyaltyCalculationsAsync("field-a", endDate: DateTime.MaxValue));
        _editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProviderFailureIsNotAnEmptyReport()
    {
        _source.Setup(s => s.GetEntityAsync("ALLOCATION_DETAIL", It.IsAny<List<AppFilter>>())).ThrowsAsync(new InvalidOperationException("provider failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().GetRoyaltyCalculationsAsync("field-a"));
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, "https://idp.example.test/")]
    public async Task EndpointRequiresTheAccountThisApiResolved(bool local, string? issuer)
    {
        var accounting = new Mock<IAccountingService>(MockBehavior.Strict);
        var access = new Mock<IAccessControlService>(MockBehavior.Strict);
        var controller = Controller(accounting.Object, local, issuer);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.GetRoyaltyCalculations(access.Object, "field-a"));
        accounting.VerifyNoOtherCalls();
        access.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EndpointChecksFieldBeforeReadingAndUsesLocalActor()
    {
        var accounting = new Mock<IAccountingService>(MockBehavior.Strict);
        var access = new Mock<IAccessControlService>(MockBehavior.Strict);
        access.Setup(a => a.CheckAssetAccessAsync("local", "field-a", "FIELD", null)).ReturnsAsync(new AccessCheckResponse { HasAccess = false });
        var controller = Controller(accounting.Object, true);
        Assert.IsType<ForbidResult>((await controller.GetRoyaltyCalculations(access.Object, "field-a")).Result);
        accounting.VerifyNoOtherCalls();
        access.Setup(a => a.CheckAssetAccessAsync("local", "field-a", "FIELD", null)).ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        accounting.Setup(a => a.GetRoyaltyCalculationsAsync("field-a", null, null)).ReturnsAsync(new List<ROYALTY_CALCULATION>());
        Assert.IsType<OkObjectResult>((await controller.GetRoyaltyCalculations(access.Object, "field-a")).Result);
    }

    private static RoyaltyReportsController Controller(IAccountingService accounting, bool local, string? issuer = null)
    {
        var claims = new List<Claim> { new("sub", "external") };
        claims.Add(!local ? new Claim(ClaimTypes.NameIdentifier, "local")
            : issuer is null ? new Claim("party_id", "local") : new Claim("party_id", "local", ClaimValueTypes.String, issuer));
        return new(accounting, NullLogger<RoyaltyReportsController>.Instance) { ControllerContext = new() {
            HttpContext = new DefaultHttpContext { User = new(new ClaimsIdentity(claims, "test")) }
        } };
    }
}
