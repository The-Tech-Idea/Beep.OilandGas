namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// Which build may open which installation. A newer database must not be written by an older build, and an older
/// machine record must be migrated rather than guessed at.
/// </summary>
/// <remarks>
/// Recorded as the mitigation for gap G-8 (SETUP-11). The number is the schema version of the settings documents in
/// the installation's database (the <c>SYSTEM</c> row); a database that does not carry one was written before
/// versioning and is treated as version 1, which is exactly what it is.
/// </remarks>
public static class OgSetupCompatibility
{
    /// <summary>The schema version this build writes and understands.</summary>
    public const int CurrentSchemaVersion = 2;

    /// <summary>The version a record without one is: written before versioning existed.</summary>
    public const int Unversioned = 1;

    /// <summary>Result code when the installation was written by a newer build.</summary>
    public const string CodeDatabaseNewer = "upgrade-database-newer";

    /// <summary>Result code when the machine record was written by a newer build.</summary>
    public const string CodeMachineRecordNewer = "upgrade-record-newer";

    /// <summary>Result code when the record is older and needs the migration SETUP-11 describes.</summary>
    public const string CodeMigrationNeeded = "upgrade-migration-needed";

    /// <summary>
    /// True when this build can open an installation whose data is at <paramref name="databaseSchemaVersion"/>.
    /// </summary>
    public static bool CanOpen(int databaseSchemaVersion) =>
        databaseSchemaVersion <= CurrentSchemaVersion;

    /// <summary>
    /// What to do about a record at <paramref name="recordSchemaVersion"/>: open it, migrate it, or refuse because a
    /// newer build wrote it.
    /// </summary>
    public static OgSetupCompatibilityVerdict Verdict(int recordSchemaVersion)
    {
        var version = recordSchemaVersion <= 0 ? Unversioned : recordSchemaVersion;

        if (version > CurrentSchemaVersion)
        {
            return OgSetupCompatibilityVerdict.WrittenByNewerBuild;
        }

        return version < CurrentSchemaVersion
            ? OgSetupCompatibilityVerdict.NeedsMigration
            : OgSetupCompatibilityVerdict.Compatible;
    }
}

/// <summary>What a build may do with a record it is opening.</summary>
public enum OgSetupCompatibilityVerdict
{
    /// <summary>Same version: open it.</summary>
    Compatible,

    /// <summary>Older version: run the migration before trusting it.</summary>
    NeedsMigration,

    /// <summary>Newer version: refuse, and write nothing.</summary>
    WrittenByNewerBuild,
}
