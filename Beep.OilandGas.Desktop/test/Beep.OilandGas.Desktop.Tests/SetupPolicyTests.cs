using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-02 AC-5: the demo dataset is only loaded after an explicit confirmation that names the connection, so it
/// cannot arrive by default or be carried over to a different database.
/// </summary>
public sealed class SetupPolicyTests
{
    private const string Connection = "BeepOilGas";

    [Fact]
    public void Without_a_confirmation_the_demo_module_is_not_allowed_to_run()
    {
        Assert.False(OgSetupPolicy.MayRun("DEMO_DATA", null, Connection));
        Assert.False(OgSetupPolicy.MayRun("DEMO_DATA", OgSetupConfirmation.None, Connection));
    }

    [Fact]
    public void A_confirmation_that_does_not_name_this_connection_is_refused()
    {
        // Naming another database is not consent to load demo data into this one.
        Assert.False(OgSetupPolicy.MayRun(
            "DEMO_DATA",
            new OgSetupConfirmation("SomeOtherDatabase", IncludeDemoData: true),
            Connection));

        Assert.False(OgSetupPolicy.MayRun(
            "DEMO_DATA",
            new OgSetupConfirmation("  ", IncludeDemoData: true),
            Connection));

        // Ticking the box without naming the connection is not a confirmation either.
        Assert.False(OgSetupPolicy.MayRun(
            "DEMO_DATA",
            new OgSetupConfirmation(Connection, IncludeDemoData: false),
            Connection));
    }

    [Fact]
    public void A_confirmation_naming_this_connection_allows_the_demo_module()
    {
        Assert.True(OgSetupPolicy.MayRun(
            "DEMO_DATA",
            new OgSetupConfirmation(Connection, IncludeDemoData: true),
            Connection));

        // The name is compared case-insensitively and trimmed, because a person types it.
        Assert.True(OgSetupPolicy.MayRun(
            "demo_data",
            new OgSetupConfirmation("  beepoilgas ", IncludeDemoData: true),
            Connection));
    }

    [Fact]
    public void Every_other_module_runs_without_a_confirmation()
    {
        Assert.True(OgSetupPolicy.MayRun("PPDM_CORE", null, Connection));
        Assert.True(OgSetupPolicy.MayRun("WELL_STATUS_FACETS", OgSetupConfirmation.None, Connection));
        Assert.False(OgSetupPolicy.IsDemoModule("PPDM_CORE"));
        Assert.True(OgSetupPolicy.IsDemoModule("DEMO_DATA"));
    }

    [Fact]
    public void The_skip_reason_names_the_connection_that_would_have_to_be_confirmed()
    {
        var reason = OgSetupPolicy.DemoSkipReason(Connection);

        Assert.Contains(Connection, reason, StringComparison.Ordinal);
        Assert.Contains("not loaded", reason, StringComparison.Ordinal);
    }
}
