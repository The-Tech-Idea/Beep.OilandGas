using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Security;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;

/// <summary>
/// What the installed application is: where its database is, and whether that database has a company and accounts.
/// </summary>
/// <remarks>
/// <para>
/// Nothing here is read from a settings file. The only persisted fact on the machine is the connection that reaches
/// the database — a server address, or the path of the local database file — because a program cannot open a
/// database it cannot name. Whether the deployment is personal or a company database is read from that connection
/// itself, not from a recorded choice.
/// </para>
/// <para>
/// <see cref="Company"/> is null before sign-in: the company record is read from the database, and only after the
/// person has authenticated (SETUP-09 AC-1).
/// </para>
/// </remarks>
public sealed record OgInstallation(
    OgDeploymentMode Deployment,
    string ConnectionName,
    bool ConnectionConfigured,
    bool CompanyRecorded,
    bool AccountsExist,
    OgInstallationSettings? Company = null)
{
    /// <summary>A personal installation is one whose database is a file on this computer.</summary>
    public bool IsPersonal => Deployment == OgDeploymentMode.Personal;

    /// <summary>True when this installation can offer a sign-in.</summary>
    public bool CanSignIn => CompanyRecorded && AccountsExist;
}

/// <summary>
/// Probes the installation instead of reading a setup file: does the connection exist, does it open, does the
/// database hold a company record and an account directory?
/// </summary>
public sealed class OgInstallationProbe
{
    private readonly IOgConnectionCatalog _connections;
    private readonly Func<string, OgSharedSetupService> _settings;
    private readonly Func<string, IOgDatabaseIdentity> _identity;

    public OgInstallationProbe(
        IOgConnectionCatalog connections,
        Func<string, OgSharedSetupService> settings,
        Func<string, IOgDatabaseIdentity> identity)
    {
        _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _identity = identity ?? throw new ArgumentNullException(nameof(identity));
    }

    /// <summary>
    /// Probes <paramref name="connectionName"/>. A missing connection is <b>not</b> a failure: it is a machine that
    /// has never been set up, which the caller answers by running setup.
    /// </summary>
    public async Task<OgResult<OgInstallation>> ProbeAsync(
        string connectionName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);

        var listed = await _connections.ListAsync(cancellationToken).ConfigureAwait(false);

        if (!listed.Success || listed.Value is null)
        {
            return OgResult<OgInstallation>.Failed(
                listed.Message ?? "The configured connections could not be read.",
                listed.Reference ?? "installation-probe-failed");
        }

        var configured = listed.Value.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, connectionName, StringComparison.OrdinalIgnoreCase));

        if (configured is null)
        {
            // Never set up on this machine. The deployment is not known yet, so it is the caller's to choose.
            return OgResult<OgInstallation>.Ok(new OgInstallation(
                Deployment: OgDeploymentMode.Personal,
                ConnectionName: connectionName,
                ConnectionConfigured: false,
                CompanyRecorded: false,
                AccountsExist: false));
        }

        var deployment = configured.IsFile ? OgDeploymentMode.Personal : OgDeploymentMode.CompanyDatabase;

        var tested = await _connections.TestAsync(connectionName, cancellationToken).ConfigureAwait(false);

        if (!tested.Success)
        {
            return OgResult<OgInstallation>.Failed(
                tested.Message ?? $"The database '{connectionName}' could not be opened.",
                tested.Reference ?? "installation-database-unreachable");
        }

        var settings = await _settings(connectionName).ReadAsync(cancellationToken).ConfigureAwait(false);

        var companyRecorded = settings.Success && settings.Value?.HasInstallation == true;

        if (!settings.Success
            && !string.Equals(settings.Reference, OgSharedSetupCodes.TableMissing, StringComparison.Ordinal))
        {
            return OgResult<OgInstallation>.Failed(
                settings.Message ?? "The company settings could not be read.",
                settings.Reference ?? OgSharedSetupCodes.ReadFailed);
        }

        var accounts = await _identity(connectionName)
            .HasAccountsAsync(cancellationToken)
            .ConfigureAwait(false);

        return OgResult<OgInstallation>.Ok(new OgInstallation(
            deployment,
            connectionName,
            ConnectionConfigured: true,
            companyRecorded,
            accounts.Success && accounts.Value));
    }
}
