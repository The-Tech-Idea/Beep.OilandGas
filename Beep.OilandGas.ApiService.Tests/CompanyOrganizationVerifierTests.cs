#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyOrganizationVerifierTests
{
    private static readonly CompanyScope Scope = new("tenant", "company");
    [Theory]
    [InlineData("Active")][InlineData("Inactive")][InlineData("NotFound")][InlineData("Unconfirmed")]
    public async Task ReportsOnlyStableSelectedCompanyObservation(string status)
    {
        var bindings = new Mock<ICompanyEnvironmentBindingReader>(); bindings.Setup(x => x.ResolveAsync("PPDM_CORE", It.IsAny<CancellationToken>())).ReturnsAsync(new string('A',64));
        var reader = new Mock<ICompanyOrganizationReader>(); reader.Setup(x => x.ReadAsync(Scope, It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>())).ReturnsAsync(status);
        var calls = 0; var result = await new CompanyOrganizationVerifier(bindings.Object, reader.Object).ReviewAsync(Scope, () => { calls++; return Task.CompletedTask; }, default);
        Assert.Equal(Scope, result.Company); Assert.Equal(status, result.Status); Assert.True(calls >= 3);
        reader.Verify(x => x.ReadAsync(Scope, It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
    [Theory]
    [InlineData("Y", "Active")]
    [InlineData("N", "Inactive")]
    [InlineData("?", "Unconfirmed")]
    public async Task CanonicalBeepReaderUsesExactCompanyKeyAndNoWrites(string indicator, string expected)
    {
        using var fixture = new DevelopmentDraftAuthorityTests.Fixture();
        var native = fixture.Drafts.Native; var core = native.Editor.Object.ConfigEditor.DataConnections.Single(x => x.ConnectionName == "core");
        native.Source.SetupGet(x => x.DatasourceName).Returns("core");
        Mock.Get(native.Source.Object.Dataconnection).SetupGet(x => x.ConnectionProp).Returns(core);
        native.Editor.Setup(x => x.OpenDataSource("core")).Returns(System.Data.ConnectionState.Open);
        native.Source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure _, bool _) => new ClassCreator(native.Editor.Object).ConvertToEntityStructure(typeof(BUSINESS_ASSOCIATE)));
        native.Source.Setup(x => x.GetEntityAsync("BUSINESS_ASSOCIATE", It.IsAny<List<AppFilter>>())).ReturnsAsync((string _, List<AppFilter> filters) =>
        {
            var key = Assert.Single(filters); Assert.Equal("BUSINESS_ASSOCIATE_ID", key.FieldName); Assert.Equal("company", key.FilterValue);
            return new object[] { new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = indicator } };
        });
        var metadata = new Mock<IPPDMMetadataRepository>(); metadata.Setup(x => x.GetTableMetadataAsync("BUSINESS_ASSOCIATE")).ReturnsAsync(new PPDMTableMetadata { EntityTypeName = "BUSINESS_ASSOCIATE", TableName = "BUSINESS_ASSOCIATE", PrimaryKeyColumn = "BUSINESS_ASSOCIATE_ID" });
        var reader = new CompanyOrganizationReader(fixture.Company, native.Editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object);
        Assert.Equal(expected, await reader.ReadAsync(Scope, () => Task.CompletedTask, default));
        Assert.Equal(0, native.Writes); Assert.Empty(native.Transactions);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("duplicate")]
    [InlineData("missing-status")]
    public async Task CanonicalBeepReaderRejectsAmbiguousOrForeignProviderEvidence(string mode)
    {
        using var fixture = new DevelopmentDraftAuthorityTests.Fixture(); var native = fixture.Drafts.Native;
        var core = native.Editor.Object.ConfigEditor.DataConnections.Single(x => x.ConnectionName == "core");
        native.Source.SetupGet(x => x.DatasourceName).Returns("core"); Mock.Get(native.Source.Object.Dataconnection).SetupGet(x => x.ConnectionProp).Returns(core);
        native.Editor.Setup(x => x.OpenDataSource("core")).Returns(System.Data.ConnectionState.Open);
        native.Source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure _, bool _) => new ClassCreator(native.Editor.Object).ConvertToEntityStructure(typeof(BUSINESS_ASSOCIATE)));
        var valid = new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" };
        native.Source.Setup(x => x.GetEntityAsync("BUSINESS_ASSOCIATE", It.IsAny<List<AppFilter>>())).ReturnsAsync((string _, List<AppFilter> _) => mode switch
        {
            "missing" => Array.Empty<object>(),
            "foreign" => [new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "foreign", ACTIVE_IND = "Y" }],
            "duplicate" => [valid, valid],
            _ => [new Dictionary<string, object> { ["BUSINESS_ASSOCIATE_ID"] = "company" }]
        });
        var metadata = new Mock<IPPDMMetadataRepository>(); metadata.Setup(x => x.GetTableMetadataAsync("BUSINESS_ASSOCIATE")).ReturnsAsync(new PPDMTableMetadata { EntityTypeName = "BUSINESS_ASSOCIATE", TableName = "BUSINESS_ASSOCIATE", PrimaryKeyColumn = "BUSINESS_ASSOCIATE_ID" });
        var reader = new CompanyOrganizationReader(fixture.Company, native.Editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object);
        if (mode == "missing") Assert.Equal("NotFound", await reader.ReadAsync(Scope, () => Task.CompletedTask, default));
        else await Assert.ThrowsAnyAsync<Exception>(() => reader.ReadAsync(Scope, () => Task.CompletedTask, default));
        Assert.Equal(0, native.Writes); Assert.Empty(native.Transactions);
    }

    [Fact]
    public async Task RefusesChangingOrganizationOrBinding()
    {
        var bindings = new Mock<ICompanyEnvironmentBindingReader>(); bindings.Setup(x => x.ResolveAsync("PPDM_CORE", It.IsAny<CancellationToken>())).ReturnsAsync(new string('A',64));
        var reader = new Mock<ICompanyOrganizationReader>(); reader.SetupSequence(x => x.ReadAsync(Scope, It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>())).ReturnsAsync("Active").ReturnsAsync("Inactive");
        await Assert.ThrowsAsync<InvalidOperationException>(() => new CompanyOrganizationVerifier(bindings.Object, reader.Object).ReviewAsync(Scope, () => Task.CompletedTask, default));
        var changing = new Mock<ICompanyEnvironmentBindingReader>(); changing.SetupSequence(x => x.ResolveAsync("PPDM_CORE", It.IsAny<CancellationToken>())).ReturnsAsync(new string('A',64)).ReturnsAsync(new string('B',64));
        var stable = new Mock<ICompanyOrganizationReader>(); stable.Setup(x => x.ReadAsync(Scope, It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>())).ReturnsAsync("Active");
        await Assert.ThrowsAsync<InvalidOperationException>(() => new CompanyOrganizationVerifier(changing.Object, stable.Object).ReviewAsync(Scope, () => Task.CompletedTask, default));
    }
    [Fact]
    public async Task RefusesMissingBindingAndRevokedAuthority()
    {
        var bindings = new Mock<ICompanyEnvironmentBindingReader>(); bindings.Setup(x => x.ResolveAsync("PPDM_CORE", It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var reader = new Mock<ICompanyOrganizationReader>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => new CompanyOrganizationVerifier(bindings.Object, reader.Object).ReviewAsync(Scope, () => Task.CompletedTask, default));
        reader.Verify(x => x.ReadAsync(It.IsAny<CompanyScope>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Never);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new CompanyOrganizationVerifier(bindings.Object, reader.Object).ReviewAsync(Scope, () => throw new UnauthorizedAccessException(), default));
    }
}
#endif
