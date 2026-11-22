using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-07 AC-2, restated for the database model: there is no local database to "repair". The company, its settings
/// and its accounts live in the installation's database, so a missing or unreachable one is a setup run, and the
/// repair path refuses rather than pretending it restored anything.
/// </summary>
public sealed class LocalRepairTests
{
    [Fact]
    public void A_repair_run_is_never_needed_and_the_reason_says_why()
    {
        var repair = new OgRepairService(Path.Combine(OgPaths.DataRoot, "probe.db"));

        var assessment = repair.Assess();

        Assert.False(assessment.Needed);
        Assert.Contains("database", assessment.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("setup", assessment.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A_repair_run_refuses_with_a_code_the_caller_can_branch_on()
    {
        var repair = new OgRepairService(Path.Combine(OgPaths.DataRoot, "probe.db"));

        var result = await repair.RepairAsync(new UnexpectedCatalog());

        Assert.False(result.Success);
        Assert.Equal("repair-not-applicable", result.Reference);
    }

    [Fact]
    public void The_repair_assessment_never_creates_a_file()
    {
        var path = Path.Combine(OgPaths.DataRoot, "repair-" + Guid.NewGuid().ToString("n")[..8] + ".db");
        var repair = new OgRepairService(path);

        _ = repair.Assess();

        Assert.False(File.Exists(path));
    }

    /// <summary>A catalog that fails the test if the repair path ever uses it.</summary>
    private sealed class UnexpectedCatalog : IOgConnectionCatalog
    {
        public Task<OgResult<IReadOnlyList<OgConnectionInfo>>> ListAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("ListAsync should not be called by a repair refusal.");

        public Task<OgResult<OgConnectionInfo>> TestAsync(string name, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("TestAsync should not be called by a repair refusal.");

        public Task<OgResult<OgConnectionInfo>> EnsureLocalAsync(
            OgLocalDatabaseRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("EnsureLocalAsync should not be called by a repair refusal.");

        public Task<OgResult<IReadOnlyList<OgConnectionProvider>>> ProvidersAsync(
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("ProvidersAsync should not be called by a repair refusal.");

        public Task<OgResult<OgConnectionInfo>> EnsureRemoteAsync(
            OgRemoteDatabaseRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("EnsureRemoteAsync should not be called by a repair refusal.");

        public Task<OgResult<OgConnectionInfo>> AdoptAsync(
            string sourceName,
            string targetName,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("AdoptAsync should not be called by a repair refusal.");

        public Task<OgResult> RemoveAsync(string name, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("RemoveAsync should not be called by a repair refusal.");
    }
}
