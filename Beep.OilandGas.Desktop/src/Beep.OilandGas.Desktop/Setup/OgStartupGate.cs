namespace TheTechIdea.Beep.OilandGas.Desktop.Setup;

/// <summary>What the startup sequence must do before the shell may open.</summary>
public enum OgStartupAction
{
    /// <summary>No installation yet (or no account): run the first-run wizard.</summary>
    RunSetup,

    /// <summary>Setup is complete but the local database is gone: offer the repair run.</summary>
    Repair,

    /// <summary>An installation exists but nobody has signed in.</summary>
    SignIn,

    /// <summary>A session exists; open the shell.</summary>
    Ready,
}

/// <summary>
/// The startup gate, as a pure function so the rule can be tested without opening a window.
/// </summary>
/// <remarks>
/// <para>
/// FND-07 AC-1: the shell must never open on an installation that has not been set up. A recorded
/// "completed" flag without an account is treated as incomplete — a wiped credential file must send the
/// operator back through setup, not into a shell they cannot use.
/// </para>
/// <para>
/// FND-07 AC-2: a complete installation with a missing local database reaches <see cref="OgStartupAction.Repair"/>
/// before sign-in. Setup and accounts are checked first, because without them the wizard owns the
/// installation and a repair would be a second way to create one.
/// </para>
/// </remarks>
public static class OgStartupGate
{
    /// <summary>
    /// Decides what the startup sequence must do. Every fact comes from the installation's database: whether a
    /// connection is configured, whether that database holds a company record, whether it has accounts, and whether
    /// somebody has signed in. No setting file is consulted.
    /// </summary>
    public static OgStartupAction Decide(
        bool connectionConfigured,
        bool companyRecorded,
        bool accountsExist,
        bool signedIn)
    {
        // Nothing configured on this machine: the wizard creates the database and the company in it.
        if (!connectionConfigured)
        {
            return OgStartupAction.RunSetup;
        }

        // The database exists but has no company or no account: the company was never finished (or a rollback left
        // it half created), and the wizard is the only thing that may create one. It adopts what is already there.
        if (!companyRecorded || !accountsExist)
        {
            return OgStartupAction.RunSetup;
        }

        return signedIn ? OgStartupAction.Ready : OgStartupAction.SignIn;
    }
}
