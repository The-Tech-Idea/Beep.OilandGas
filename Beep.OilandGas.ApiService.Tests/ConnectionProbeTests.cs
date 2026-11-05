#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class ConnectionProbeTests
{
    [Theory][InlineData("ok")][InlineData("open-fails")][InlineData("wrong-target")][InlineData("cleanup-fails")]
    public void ProposedSettingsUseStandaloneBeepDatasourceWithoutRegisteringSharedConnections(string mode)
    {
        var saved = new ConnectionProperties { ConnectionName = "production", Host = "old-host" };
        var proposed = new ConnectionProperties { ConnectionName = "production", Host = "new-host", Password = "private", DatabaseType = DataSourceType.SqlLite, DriverName = "driver" };
        var source = new Mock<IDataSource>(); var connection = new Mock<IDataConnection>();
        connection.SetupProperty(x => x.ConnectionProp); connection.SetupProperty(x => x.DataSourceDriver);
        source.SetupGet(x => x.Dataconnection).Returns(connection.Object); source.SetupProperty(x => x.DatasourceName);
        source.Setup(x => x.Openconnection()).Returns(() => mode == "open-fails" ? throw new InvalidOperationException("unavailable") : ConnectionState.Open);
        if (mode == "cleanup-fails") source.Setup(x => x.Dispose()).Throws(new InvalidOperationException("cleanup"));
        var driver = new ConnectionDriversConfig { PackageName = "driver", classHandler = "ProbeSource", DatasourceType = DataSourceType.SqlLite };
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties> { saved });
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        config.SetupGet(x => x.DataSourcesClasses).Returns(new List<AssemblyClassDefinition> { new() { className = "ProbeSource", type = source.Object.GetType() } });
        var loader = new Mock<IAssemblyHandler>(); loader.Setup(x => x.CreateInstanceFromString(It.IsAny<string>(), It.IsAny<object[]>())).Returns((string _, object[] args) =>
        {
            Assert.StartsWith("__test_", (string)args[0]); source.Object.DatasourceName = (string)args[0];
            if (mode == "wrong-target") connection.SetupGet(x => x.ConnectionProp).Returns(saved);
            return source.Object;
        });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object); editor.SetupGet(x => x.assemblyHandler).Returns(loader.Object);
        if (mode == "ok") Assert.Equal(ConnectionState.Open, ConnectionProbe.Test(editor.Object, proposed));
        else Assert.Throws<InvalidOperationException>(() => ConnectionProbe.Test(editor.Object, proposed));
        source.Verify(x => x.Dispose(), Times.Once);
        editor.Verify(x => x.GetDataSource(It.IsAny<string>()), Times.Never); editor.Verify(x => x.CreateNewDataSourceConnection(It.IsAny<ConnectionProperties>(), It.IsAny<string>()), Times.Never);
        config.Verify(x => x.AddDataConnection(It.IsAny<ConnectionProperties>()), Times.Never); config.Verify(x => x.SaveDataconnectionsValues(), Times.Never);
        Assert.Equal("old-host", Assert.Single(config.Object.DataConnections).Host);
        if (mode == "wrong-target") source.Verify(x => x.Openconnection(), Times.Never);
    }
}
#endif
