using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Setup;

/// <summary>
/// The installation as the running application sees it: the machine's deployment and connection, plus the company
/// record once it has been read.
/// </summary>
/// <remarks>
/// <para>
/// This type is never written to disk. It is assembled in memory by <c>OgApplication</c> from the installation's
/// database (<c>BEEPSETTINGS</c>) and handed to the session, the wizard and the settings page. There is deliberately
/// no store: a settings file was the defect this replaced — a company setting read from a workstation is a second
/// truth.
/// </para>
/// <para>
/// The only fact that survives a restart is the connection that reaches the database (see <c>OgPaths.ConnectionName</c>
/// and the BeepDM connection store); whether the deployment is personal or a company database is read from that
/// connection, and everything else from the database itself.
/// </para>
/// </remarks>
public sealed record OgSetupState(
    bool Completed,
    string CompanyName,
    string DataRoot,
    string AdministratorUserName,
    DateTimeOffset CompletedAtUtc,
    string CompanyCode = "",
    string CountryCode = "",
    string CultureName = "",
    string TimeZoneId = "",
    string BaseCurrencyCode = "",
    int FiscalYearStartMonth = 0,
    string CompanyTypeCode = "",
    string UnitSystemCode = "",
    string DefaultFieldName = "",
    OgDeploymentMode Deployment = OgDeploymentMode.Personal,
    string DataConnectionName = "",
    DateTimeOffset? SharedSettingsLoadedAtUtc = null)
{
    /// <summary>True when the installation's database is a server rather than a file on this computer.</summary>
    public bool UsesCompanyDatabase => Deployment == OgDeploymentMode.CompanyDatabase;

    /// <summary>The connection that reaches the installation's database.</summary>
    public string ConnectionName => string.IsNullOrWhiteSpace(DataConnectionName)
        ? OgPaths.ConnectionName
        : DataConnectionName;

    /// <summary>True when the company record has been read from the database in this session.</summary>
    public bool HasSharedSettings => SharedSettingsLoadedAtUtc is not null;
}
