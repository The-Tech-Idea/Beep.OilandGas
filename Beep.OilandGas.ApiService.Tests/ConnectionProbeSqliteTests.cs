#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using TheTechIdea.Beep;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using Xunit;

public sealed class ConnectionProbeSqliteTests
{
    [Fact]
    public void BeepLifecycleOpensSQLiteAndDoesNotPersistTheProposedConnection()
    {
        var path = Path.Combine(Path.GetTempPath(), "oilgas-probe-" + Guid.NewGuid().ToString("N") + ".db");
        System.Data.SQLite.SQLiteConnection.CreateFile(path);
        try
        {
            var config = new Mock<IConfigEditor>(); var drivers = ConnectionHelper.GetAllConnectionConfigs().Where(d => d.classHandler == "SQLiteDataSource").ToList();
            config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties>());
            config.SetupGet(x => x.DataDriversClasses).Returns(drivers);
            config.SetupGet(x => x.DataSourcesClasses).Returns(new List<AssemblyClassDefinition> { new() { className = "SQLiteDataSource", type = typeof(SQLiteDataSource) } });
            var loader = new Mock<IAssemblyHandler>(); loader.Setup(x => x.CreateInstanceFromString(It.IsAny<string>(), It.IsAny<object[]>())).Returns((string _, object[] args) => Activator.CreateInstance(typeof(SQLiteDataSource), args)!); loader.SetupGet(x => x.DataSourcesClasses).Returns(config.Object.DataSourcesClasses); loader.Setup(x => x.GetInstance("System.Data.SQLite.SQLiteConnection")).Returns(() => new System.Data.SQLite.SQLiteConnection()); loader.Setup(x => x.GetType(It.IsAny<string>())).Returns((string name) => name.Contains("SQLiteDataSource") ? typeof(SQLiteDataSource) : Type.GetType(name));
            Assert.NotEmpty(drivers);
            var logs = new List<string>(); var log = new Mock<IDMLogger>(); log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(logs.Add);
            var errors = new ErrorsInfo(); var editor = new DMEEditor(log.Object, Mock.Of<IUtil>(), errors, config.Object, loader.Object);
            try
            {
                var cp = new ConnectionProperties { ConnectionName = "collision", DatabaseType = DataSourceType.SqlLite, ConnectionString = "Data Source=" + path, Database = path, GuidID = Guid.NewGuid().ToString() };
                try { Assert.Equal(ConnectionState.Open, ConnectionProbe.Test(editor, cp)); }
                catch (Exception error) { throw new InvalidOperationException(string.Join(" | ", logs.TakeLast(10)) + " ERROR=" + errors.Message, error); }
                Assert.Empty(config.Object.DataConnections); Assert.DoesNotContain(editor.DataSources, x => x.DatasourceName == cp.ConnectionName);
                config.Verify(x => x.SaveDataconnectionsValues(), Times.Never); config.Verify(x => x.AddDataConnection(It.IsAny<ConnectionProperties>()), Times.Never);
            }
            finally { editor.Dispose(); }
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}






#endif
