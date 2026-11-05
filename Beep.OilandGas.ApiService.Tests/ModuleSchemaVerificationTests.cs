using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ModuleSchemaVerificationTests
{
    [Theory]
    [InlineData(TheTechIdea.Beep.Utilities.DataSourceType.SqlServer, true)]
    [InlineData(TheTechIdea.Beep.Utilities.DataSourceType.Postgre, true)]
    [InlineData(TheTechIdea.Beep.Utilities.DataSourceType.Oracle, false)]
    public void DateOnlyCheckRespectsProviderSemantics(TheTechIdea.Beep.Utilities.DataSourceType provider, bool rejects)
    {
        var (editor, source) = Fixture("date-only");
        source.SetupGet(x => x.DatasourceType).Returns(provider);
        var error = Record.Exception(() => ModuleSchemaVerification.Verify(editor.Object, source.Object, [typeof(OIL_COMPOSITION)]));
        if (rejects) Assert.Contains("date-only", Assert.IsType<ModuleSchemaMismatchException>(error).Message);
        else Assert.Null(error); // Oracle DATE includes time-of-day.
    }

    [Theory]
    [InlineData("missing-table")]
    [InlineData("missing-column")]
    [InlineData("missing-key")]
    [InlineData("nullable-key")]
    [InlineData("read-failure")]
    public void IncompleteOrUnreadableSchemaCannotReportCompletion(string scenario)
    {
        var (editor, source) = Fixture(scenario);
        Assert.ThrowsAny<InvalidOperationException>(() => ModuleSchemaVerification.Verify(
            editor.Object, source.Object, [typeof(OIL_COMPOSITION)]));
    }

    [Theory]
    [InlineData("missing-table", true)]
    [InlineData("existence-failure", false)]
    [InlineData("missing-metadata", false)]
    [InlineData("read-failure", false)]
    [InlineData("missing-column", true)]
    public void OnlyConfirmedSchemaDifferencesAreReportedAsMismatch(string scenario, bool mismatch)
    {
        var (editor, source) = Fixture(scenario);
        var error = Record.Exception(() => ModuleSchemaVerification.Verify(editor.Object, source.Object, [typeof(OIL_COMPOSITION)]));
        Assert.NotNull(error);
        Assert.Equal(mismatch, error is ModuleSchemaMismatchException);
    }

    [Fact]
    public void CompleteSchemaIsReadWithFreshEmptyMetadata()
    {
        var (editor, source) = Fixture("complete");
        ModuleSchemaVerification.Verify(editor.Object, source.Object, [typeof(OIL_COMPOSITION)]);
        source.Verify(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true), Times.Once);
    }

    [Fact]
    public void EmptyManifestDoesNotAccessDatasource()
    {
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        Assert.Throws<InvalidOperationException>(() => ModuleSchemaVerification.Verify(
            Mock.Of<IDMEEditor>(), source.Object, []));
        source.VerifyNoOtherCalls();
    }

    private static (Mock<IDMEEditor>, Mock<IDataSource>) Fixture(string scenario)
    {
        var editor = new Mock<IDMEEditor>();
        var creator = new ClassCreator(editor.Object);
        editor.SetupGet(x => x.classCreator).Returns(creator);
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.Setup(x => x.CheckEntityExist(It.IsAny<string>())).Returns(scenario == "missing-metadata");
        source.SetupGet(x => x.ErrorObject).Returns(new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = scenario == "existence-failure" ? TheTechIdea.Beep.ConfigUtil.Errors.Failed : TheTechIdea.Beep.ConfigUtil.Errors.Ok });
        source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true))
            .Returns((EntityStructure request, bool refresh) =>
            {
                Assert.Equal(nameof(OIL_COMPOSITION), request.EntityName);
                Assert.True(request.Fields is null || request.Fields.Count == 0);
                if (scenario == "read-failure") throw new InvalidOperationException("Metadata unavailable");
                var actual = creator.ConvertToEntityStructure(typeof(OIL_COMPOSITION));
                if (scenario is "missing-table" or "existence-failure" or "missing-metadata") actual.Fields.Clear();
                if (scenario == "missing-column") actual.Fields.RemoveAt(actual.Fields.Count - 1);
                if (scenario == "missing-key") actual.Fields.Single(x => x.IsKey).IsKey = false;
                if (scenario == "nullable-key") actual.Fields.Single(x => x.IsKey).AllowDBNull = true;
                if (scenario == "date-only")
                    actual.Fields.First(x => x.Fieldtype == typeof(DateTime).FullName).ColumnTypeName = "date";
                return actual;
            });
        return (editor, source);
    }
}
