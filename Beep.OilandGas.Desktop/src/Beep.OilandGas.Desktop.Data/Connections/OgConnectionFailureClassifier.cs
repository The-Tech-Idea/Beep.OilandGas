using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DriversConfigurations;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;

/// <summary>How a connection test ended. Distinct values so a page can offer the right next step.</summary>
public enum OgConnectionState
{
    /// <summary>Configured but never opened in this session.</summary>
    NeverTested,

    Ok,

    /// <summary>The driver's database file is not there.</summary>
    MissingDatabase,

    /// <summary>The host could not be reached (DNS, refused, timeout, network).</summary>
    HostUnreachable,

    /// <summary>The host answered and refused this caller (bad user, password, or permission).</summary>
    CredentialsRejected,

    /// <summary>No installed driver can open this connection's database type.</summary>
    DriverMissing,

    /// <summary>The provider failed for a reason the classifier could not place.</summary>
    Failed,
}

/// <summary>What a connection test found, in words and in a stable code.</summary>
public sealed record OgConnectionTest(
    string Name,
    string Provider,
    OgConnectionState State,
    string Code,
    string Message);

/// <summary>
/// Turns a provider's failure into one of the four outcomes a person can act on.
/// </summary>
/// <remarks>
/// <para>
/// The two definite cases are exact: a file database whose file is absent, and a database type no installed
/// driver can open. The rest is best effort from the provider's own message — providers do not agree on
/// codes, and database products embed their errors in different ways. The message is therefore always
/// treated as a hint for choosing the sentence, never as evidence that the connection works.
/// </para>
/// <para>
/// Pure and public so the mapping is tested against representative provider messages instead of being
/// buried in a catch block.
/// </para>
/// </remarks>
public static class OgConnectionFailureClassifier
{
    public const string CodeDatabaseMissing = "connection-database-missing";
    public const string CodeHostUnreachable = "connection-host-unreachable";
    public const string CodeCredentialsRejected = "connection-credentials-rejected";
    public const string CodeDriverMissing = "connection-driver-missing";
    public const string CodeFailed = "connection-failed";

    private static readonly string[] CredentialHints =
    [
        "login failed", "authentication", "access denied", "not authorized", "unauthorized",
        "invalid password", "password", "user name", "username", "credential", "permission denied",
        "401", "403",
    ];

    private static readonly string[] UnreachableHints =
    [
        "network", "host", "server was not found", "no such host", "could not connect", "unable to connect",
        "connection refused", "timed out", "timeout", "not reachable", "unreachable", "socket",
        "no route to host", "instance failure", "service not found",
    ];

    /// <summary>
    /// Classifies a failed open from the provider's exception. Reading its message happens here, outside any
    /// catch, so the text is used only to choose the kind and never travels as a message.
    /// </summary>
    public static (OgConnectionState State, string Code) Classify(
        Exception? error,
        bool driverMissing = false,
        bool databaseMissing = false) =>
        Classify(error?.Message, driverMissing, databaseMissing);

    /// <summary>
    /// Classifies a failed open. <paramref name="message"/> is the provider's error text, if any.
    /// </summary>
    public static (OgConnectionState State, string Code) Classify(
        string? message,
        bool driverMissing = false,
        bool databaseMissing = false)
    {
        // Definite facts win over a provider's wording.
        if (driverMissing)
        {
            return (OgConnectionState.DriverMissing, CodeDriverMissing);
        }

        if (databaseMissing)
        {
            return (OgConnectionState.MissingDatabase, CodeDatabaseMissing);
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return (OgConnectionState.Failed, CodeFailed);
        }

        // Credentials are checked first: "login failed for user 'x' on host 'y'" is a credential failure,
        // and saying "unreachable" would send the person to the network team for nothing.
        if (Contains(message, CredentialHints))
        {
            return (OgConnectionState.CredentialsRejected, CodeCredentialsRejected);
        }

        if (Contains(message, UnreachableHints))
        {
            return (OgConnectionState.HostUnreachable, CodeHostUnreachable);
        }

        return (OgConnectionState.Failed, CodeFailed);
    }

    /// <summary>The sentence the page shows for a state.</summary>
    public static string Describe(OgConnectionState state) => state switch
    {
        OgConnectionState.Ok => "The connection opened.",
        OgConnectionState.MissingDatabase => "The database file is missing.",
        OgConnectionState.HostUnreachable => "The server could not be reached. Check the host name, the port and the network.",
        OgConnectionState.CredentialsRejected => "The server refused these credentials. Check the user and the password.",
        OgConnectionState.DriverMissing => "No installed driver can open this kind of database.",
        OgConnectionState.NeverTested => "This connection has not been tested in this session.",
        OgConnectionState.Failed => "The provider failed; the reason is in the failure store.",
    };

    private static bool Contains(string message, string[] hints) =>
        hints.Any(hint => message.Contains(hint, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// True when BeepDM knows a driver for <paramref name="databaseType"/>. A connection whose type has no
    /// driver fails the same way whatever the server does, so it is worth separating.
    /// </summary>
    public static bool HasDriverFor(
        IEnumerable<ConnectionDriversConfig>? drivers,
        TheTechIdea.Beep.Utilities.DataSourceType databaseType) =>
        drivers is not null && drivers.Any(driver => driver.DatasourceType == databaseType);
}
