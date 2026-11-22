using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.Security;

/// <summary>
/// Company settings shared by every user of one database: one row per settings document, stored as JSON.
/// </summary>
/// <remarks>
/// <para>
/// The first administrator creates the company, which writes the <c>COMPANY</c> row. Every user who signs in
/// against that database then reads the same rows, so the company, its calendar and currency, the PPDM module set
/// and the Oil &amp; Gas environment are not a per-workstation decision. Only an administrator changes them.
/// </para>
/// <para>
/// <see cref="REVISION"/> is a hash of the document and of the revision the writer expected to replace, and
/// <see cref="INTENT_ID"/> makes a retried save the same change rather than a second one. A writer that did not
/// see the current revision is refused instead of overwriting another administrator's change.
/// </para>
/// </remarks>
public sealed class BEEPSETTINGS : ModelEntityBase
{
    /// <summary>The company record: name, code, region, calendar, currency and the oil-and-gas defaults.</summary>
    public const string Company = "COMPANY";

    /// <summary>The OilandGasTools environment document every user shares.</summary>
    public const string ToolsEnvironment = "TOOLS_ENVIRONMENT";

    /// <summary>Which PPDM modules (and therefore which reference lists) this database contains.</summary>
    public const string Modules = "MODULES";

    /// <summary>The schema version of the settings this database holds.</summary>
    public const string System = "SYSTEM";

    /// <summary>How far an interrupted first-run setup got, so it can resume at the step it stopped on.</summary>
    public const string SetupProgress = "SETUP_PROGRESS";

    /// <summary>
    /// The marker a company setup leaves on the account it created: the administrator must replace the password
    /// somebody else typed for it before using the application (Keycloak's bootstrap-admin rule).
    /// </summary>
    public const string Bootstrap = "BOOTSTRAP";

    /// <summary>The table both the wizard and the settings page use.</summary>
    public const string TableName = "BEEPSETTINGS";

    [Key, Required, MaxLength(64)]
    public string SETTINGS_ID { get; set; } = string.Empty;

    /// <summary>
    /// The document. One megabyte, the same ceiling the installation-wide setup table uses: a resolved country
    /// environment is far more than a company record and must not be truncated into an unreadable document.
    /// </summary>
    [Required, MaxLength(1048576)]
    public string SETTINGS_JSON { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string REVISION { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? PREVIOUS_REVISION { get; set; }

    [MaxLength(32)]
    public string INTENT_ID { get; set; } = string.Empty;

    [MaxLength(128)]
    public string UPDATED_BY { get; set; } = string.Empty;

    public DateTime? UPDATED_UTC { get; set; }
}
