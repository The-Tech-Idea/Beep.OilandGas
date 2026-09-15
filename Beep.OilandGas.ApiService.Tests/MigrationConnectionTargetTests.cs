using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class MigrationConnectionTargetTests
{
    [Theory]
    [InlineData("database")]
    [InlineData("host")]
    [InlineData("schema")]
    [InlineData("connection-string")]
    [InlineData("password")]
    [InlineData("driver")]
    public void StaleDatasourceIsRejectedWithoutOpeningOrChangingIt(string changed)
    {
        var configured = Connection();
        var cached = Connection();
        switch (changed)
        {
            case "database": configured.Database = "new-database"; break;
            case "host": configured.Host = "new-host"; break;
            case "schema": configured.SchemaName = "new-schema"; break;
            case "connection-string": configured.ConnectionString = "new-private-target"; break;
            case "password": configured.Password = "new-private-password"; break;
            case "driver": configured.DriverName = "new-driver"; break;
        }
        var (editor, source) = Setup(configured, cached);
        var exception = Assert.Throws<InvalidOperationException>(() =>
            MigrationConnectionTarget.Validate(editor.Object, source.Object, "module-db"));
        Assert.DoesNotContain("new-private", exception.Message);
        source.Verify(x => x.Openconnection(), Times.Never);
        source.Verify(x => x.Closeconnection(), Times.Never);
    }

    [Fact]
    public void MatchingConnectionSettingsIgnoreDisplayPreferences()
    {
        var configured = Connection();
        var cached = Connection();
        configured.Favourite = !cached.Favourite;
        var (editor, source) = Setup(configured, cached);
        MigrationConnectionTarget.Validate(editor.Object, source.Object, "MODULE-DB");
        source.Verify(x => x.Openconnection(), Times.Never);
    }

    [Fact]
    public void MissingActiveConnectionSettingsCannotBeAssumedCurrent()
    {
        var (editor, source) = Setup(Connection(), null);
        Assert.Throws<InvalidOperationException>(() =>
            MigrationConnectionTarget.Validate(editor.Object, source.Object, "module-db"));
        source.Verify(x => x.Openconnection(), Times.Never);
    }

    private static ConnectionProperties Connection() => new()
    {
        ConnectionName = "module-db", GuidID = "connection-id", Database = "old-database", Host = "host"
    };

    private static (Mock<IDMEEditor> Editor, Mock<IDataSource> Source) Setup(ConnectionProperties configured, IConnectionProperties? cached)
    {
        var config = new Mock<IConfigEditor>(MockBehavior.Strict);
        config.SetupGet(x => x.DataConnections).Returns([configured]);
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var connection = new Mock<IDataConnection>(MockBehavior.Strict);
        connection.SetupGet(x => x.ConnectionProp).Returns(cached!);
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.SetupGet(x => x.DatasourceName).Returns("module-db");
        source.SetupGet(x => x.Dataconnection).Returns(connection.Object);
        return (editor, source);
    }
}
