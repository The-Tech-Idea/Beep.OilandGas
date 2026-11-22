using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-11 (gap G-8): an installation carries a schema version, an older build refuses a newer database, and an
/// older record is recognised as needing the migration rather than being guessed at.
/// </summary>
public sealed class SetupVersioningTests
{
    [Theory]
    [InlineData(0)]   // written before versioning
    [InlineData(1)]
    [InlineData(2)]
    public void This_build_opens_records_at_or_below_its_own_version(int version)
    {
        Assert.Equal(
            version < OgSetupCompatibility.CurrentSchemaVersion
                ? OgSetupCompatibilityVerdict.NeedsMigration
                : OgSetupCompatibilityVerdict.Compatible,
            OgSetupCompatibility.Verdict(version));
    }

    [Fact]
    public void A_record_without_a_version_is_treated_as_the_oldest_and_needs_migration()
    {
        Assert.Equal(1, OgSetupCompatibility.Unversioned);
        Assert.Equal(OgSetupCompatibilityVerdict.NeedsMigration, OgSetupCompatibility.Verdict(0));
        Assert.Equal(OgSetupCompatibilityVerdict.NeedsMigration, OgSetupCompatibility.Verdict(-5));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(99)]
    public void A_record_written_by_a_newer_build_is_refused(int version)
    {
        Assert.False(OgSetupCompatibility.CanOpen(version));
        Assert.Equal(OgSetupCompatibilityVerdict.WrittenByNewerBuild, OgSetupCompatibility.Verdict(version));
    }

    [Fact]
    public void The_current_version_opens_itself()
    {
        Assert.True(OgSetupCompatibility.CanOpen(OgSetupCompatibility.CurrentSchemaVersion));
        Assert.Equal(
            OgSetupCompatibilityVerdict.Compatible,
            OgSetupCompatibility.Verdict(OgSetupCompatibility.CurrentSchemaVersion));
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData("", 1)]
    [InlineData("{}", 1)]
    [InlineData("{\"schemaVersion\":2}", 2)]
    [InlineData("{\"schemaVersion\":\"two\"}", 1)]   // a value that is not a number is not a version
    [InlineData("{\"schemaVersion\":0}", 1)]
    public void A_stored_version_is_read_leniently_and_never_invented(string? json, int expected)
    {
        // The parse lives with the settings service; an unreadable document is the oldest version, not a guess.
        var service = new OgSharedSetupService(new StubStore(), new SilentReporter(), new SilentAudit());

        Assert.Equal(expected, service.ParseSchemaVersion(json));
    }

    [Fact]
    public void The_settings_documents_name_the_system_row_that_carries_the_version()
    {
        Assert.Equal("SYSTEM", OgSharedSetup.SystemId);
        Assert.Equal("BEEPSETTINGS", OgSharedSetup.TableName);
    }

    private sealed class StubStore : IOgSharedSetupStore
    {
        public string ConnectionName => "version-probe";

        public bool TableExists => true;

        public Task<OgResult<OgSharedSetupRecord?>> ReadAsync(string settingsId, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgSharedSetupRecord?>.Ok(null));

        public Task<OgResult<OgSharedSetupRecord>> SaveAsync(
            OgSharedSetupChange change,
            string actor,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("A version test does not write.");

        public Task<OgResult> DeleteAsync(
            string settingsId,
            string expectedRevision,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("A version test does not delete.");
    }

    private sealed class SilentReporter : TheTechIdeaWeb.Diagnostics.IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            TheTechIdeaWeb.Diagnostics.FailureSeverity severity = TheTechIdeaWeb.Diagnostics.FailureSeverity.Error) =>
            TheTechIdeaWeb.Diagnostics.FailureReferenceFormat.New();
    }
}
