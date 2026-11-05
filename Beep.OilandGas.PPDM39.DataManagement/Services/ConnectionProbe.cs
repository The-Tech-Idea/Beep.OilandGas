using System.Data;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Utilities;

namespace Beep.OilandGas.PPDM39.DataManagement.Services;

/// <summary>Tests proposed settings using a standalone Beep datasource without registering a connection.</summary>
public static class ConnectionProbe
{
    public static ConnectionState Test(IDMEEditor editor, ConnectionProperties proposed)
    {
        ArgumentNullException.ThrowIfNull(editor); ArgumentNullException.ThrowIfNull(proposed);
        var config = editor.ConfigEditor ?? throw new InvalidOperationException("The Beep driver catalog is unavailable.");
        var loader = editor.assemblyHandler ?? throw new InvalidOperationException("The Beep assembly handler is unavailable.");
        var driver = ConnectionHelper.LinkConnection2Drivers(proposed, config) ?? throw new InvalidOperationException("No Beep driver matches the proposed connection.");
        var definitions = config.DataSourcesClasses?.Where(x => string.Equals(x.className, driver.classHandler, StringComparison.OrdinalIgnoreCase)).ToArray() ?? [];
        if (definitions.Length != 1 || definitions[0].type is not { } type || !typeof(IDataSource).IsAssignableFrom(type))
            throw new InvalidOperationException("The Beep datasource implementation is unavailable or ambiguous.");
        var name = "__test_" + Guid.NewGuid().ToString("N"); proposed.ConnectionName = name;
        var fingerprint = Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Fingerprint(proposed);
        var source = loader.CreateInstanceFromString(type.AssemblyQualifiedName!, new object[] { name, editor.Logger, editor, proposed.DatabaseType, new ErrorsInfo() }) as IDataSource
            ?? throw new InvalidOperationException("The test datasource could not be created. Check the installed driver.");
        try
        {
            if (source.Dataconnection is null) throw new InvalidOperationException("The test datasource has no connection.");
            source.Dataconnection.ConnectionProp = proposed; source.Dataconnection.DataSourceDriver = driver;
            if (source.DatasourceName != name || source.Dataconnection.ConnectionProp is not { } actual ||
                Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Fingerprint(actual) !=
                fingerprint)
                throw new InvalidOperationException("The test datasource does not match the proposed target.");
            return source.Openconnection();
        }
        finally { source.Dispose(); }
    }
}
