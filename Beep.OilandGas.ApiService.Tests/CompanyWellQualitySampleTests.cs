#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyWellQualitySampleTests
{
    private sealed class Fixture
    {
        public readonly PersonaTaskRoutingTests.Fixture Native = new();
        public readonly Mock<IPPDMRowAccessPolicy> Policy = new();
        public readonly Mock<IPPDMMetadataRepository> Metadata = new();
        public int Reads; public bool Foreign, Oversized, Revoked, Duplicate;
        public PPDMGenericRepository Repository;
        public Fixture()
        {
            Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync("WELL")).ReturnsAsync(new List<string> { "UWI" });
            Policy.Setup(x => x.GetReadConjunctionsAsync(typeof(WELL), "WELL"))
                .ReturnsAsync(() => Revoked && Reads > 0 ? [] : new List<IReadOnlyList<AppFilter>>
                    { new[] { new AppFilter { FieldName = "ASSIGNED_FIELD", Operator = "=", FilterValue = "field" } } });
            Policy.Setup(x => x.GetReadConstraintsAsync(typeof(WELL), "WELL"))
                .ReturnsAsync(new[] { new AppFilter { FieldName = "OPERATOR", Operator = "=", FilterValue = "company" } });
            Policy.Setup(x => x.ValidateRowsAsync(typeof(WELL), "WELL", It.IsAny<IReadOnlyList<object>>()))
                .Returns(() => Revoked && Reads > 0 ? Task.FromException(new UnauthorizedAccessException("Access changed")) : Task.CompletedTask);
            Native.Source.Setup(x => x.GetEntity("WELL", It.IsAny<List<AppFilter>>(), 1, 21))
                .Returns((string _, List<AppFilter> filters, int page, int size) =>
                {
                    Reads++; Assert.Contains(filters, x => x.FieldName == "OPERATOR" && x.FilterValue == "company");
                    var rows = Enumerable.Range(0, Oversized ? 22 : 21).Select(i => new WELL
                    { UWI = Duplicate ? "same-well" : $"well-{i:D2}", ASSIGNED_FIELD = "field", OPERATOR = Foreign ? "other" : "company" }).ToArray();
                    return new PagedResult(rows, page, size, rows.Length);
                });
            Repository = new(Native.Editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
                Metadata.Object, typeof(WELL), "lifecycle", "WELL", rowAccess: Policy.Object, requiredReadColumns: ["UWI"]);
        }
    }
    [Fact]
    public async Task RepositoryReturnsBoundedCompanySampleThroughBeepPaging()
    {
        var fixture = new Fixture(); var rows = await fixture.Repository.GetScopedSampleAsync(20);
        Assert.Equal(20, rows.Count); Assert.Equal(1, fixture.Reads);
        Assert.All(rows.Cast<WELL>(), row => Assert.Equal("company", row.OPERATOR));
        Assert.Equal(0, fixture.Native.Writes);
    }
    [Theory]
    [InlineData("foreign")]
    [InlineData("oversized")]
    [InlineData("duplicate")]
    public async Task SampleRefusesProviderRowsOutsideTheReviewedBoundOrCompany(string failure)
    {
        var fixture = new Fixture { Foreign = failure == "foreign", Oversized = failure == "oversized", Duplicate = failure == "duplicate" };
        await Assert.ThrowsAsync<RefusalException>(() => fixture.Repository.GetScopedSampleAsync(20));
        Assert.Equal(0, fixture.Native.Writes);
    }
    [Fact]
    public async Task SampleRequiresCompanyRowPolicy()
    {
        var fixture = new Fixture();
        var unscoped = new PPDMGenericRepository(fixture.Native.Editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            fixture.Metadata.Object, typeof(WELL), "lifecycle", "WELL");
        await Assert.ThrowsAsync<RefusalException>(() => unscoped.GetScopedSampleAsync(20)); Assert.Equal(0, fixture.Reads);
    }
    [Fact]
    public async Task SampleDoesNotReturnRowsWhenAccessChangesDuringRead()
    {
        var fixture = new Fixture { Revoked = true };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Repository.GetScopedSampleAsync(20));
        Assert.Equal(1, fixture.Reads);
    }
}
#endif
