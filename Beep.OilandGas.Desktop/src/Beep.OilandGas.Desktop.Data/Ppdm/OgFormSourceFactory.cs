using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

/// <summary>
/// Builds the DAT-05 reference and defaults sources over the owning services.
/// </summary>
/// <remarks>
/// This lives in <c>.Data</c> because it is the only project allowed to hold BeepDM types (FND-02 AC-4); the host
/// passes its Beep host and gets two interfaces back, so no page or bootstrapper sees <c>IDMEEditor</c>.
/// </remarks>
public static class OgFormSourceFactory
{
    /// <summary>The reference picker, over <see cref="LOVManagementService"/>.</summary>
    public static IOgFormLookups CreateLookups(
        OgBeepHost host,
        IPPDMMetadataRepository metadata,
        string connectionName)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(metadata);

        var lov = new LOVManagementService(
            host.Editor,
            new CommonColumnHandler(),
            Defaults(host, metadata, connectionName),
            metadata,
            connectionName);

        return new OgLovFormLookups(async (column, cancellationToken) =>
        {
            var values = await lov.GetLOVByTypeAsync(column, null, connectionName).ConfigureAwait(false);

            return values
                .Select(value => (
                    value.ValueCode,
                    value.ValueName,
                    string.Equals(value.ActiveInd, "Y", StringComparison.OrdinalIgnoreCase)))
                .ToList();
        });
    }

    /// <summary>The defaults source, over <see cref="PPDMDefaultValueService"/>.</summary>
    public static IOgFormDefaults CreateDefaults(
        OgBeepHost host,
        IPPDMMetadataRepository metadata,
        string connectionName)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(metadata);

        var service = new PPDMDefaultValueService(
            host.Editor,
            new CommonColumnHandler(),
            Defaults(host, metadata, connectionName),
            metadata,
            connectionName);

        return new OgDefaultValueFormDefaults(async _ =>
            (IReadOnlyDictionary<string, string>)await service
                .GetDefaultsForDatabaseAsync(connectionName)
                .ConfigureAwait(false));
    }

    private static IPPDM39DefaultsRepository Defaults(
        OgBeepHost host,
        IPPDMMetadataRepository metadata,
        string connectionName) =>
        new PPDM39DefaultsRepository(
            editor: host.Editor,
            connectionName: connectionName,
            metadata: metadata,
            commonColumnHandler: new CommonColumnHandler());
}
