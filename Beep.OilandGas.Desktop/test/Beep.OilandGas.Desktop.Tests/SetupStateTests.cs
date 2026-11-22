using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-02: the setup state that lets a failed run resume. A missing file is a fresh state; a saved state comes
/// back exactly, including which modules succeeded.
/// </summary>
public sealed class SetupStateTests
{
    [Fact]
    public void A_missing_state_file_is_a_fresh_state()
    {
        var store = new OgSetupStateStore(Path.Combine(OgPaths.DataDirectory, "no-state-" + Guid.NewGuid().ToString("n")[..8] + ".json"));

        var state = store.Load();

        Assert.Empty(state.Modules);
        Assert.False(state.Completed);
    }

    [Fact]
    public void Saved_state_round_trips_and_leaves_no_temporary_file()
    {
        var path = Path.Combine(OgPaths.DataDirectory, "state-" + Guid.NewGuid().ToString("n")[..8] + ".json");
        var store = new OgSetupStateStore(path);

        var state = new OgSetupState
        {
            ConnectionName = "BeepOilGas",
            Completed = false,
            LastRunUtc = new DateTimeOffset(2026, 11, 6, 12, 0, 0, TimeSpan.Zero),
            Modules =
            {
                new OgSetupModuleState { ModuleId = "WELL", Succeeded = true, RecordsInserted = 12 },
                new OgSetupModuleState { ModuleId = "HSE", Succeeded = false },
            },
        };

        store.Save(state);
        var loaded = store.Load();

        Assert.Equal("BeepOilGas", loaded.ConnectionName);
        Assert.False(loaded.Completed);
        Assert.Equal(2, loaded.Modules.Count);
        Assert.True(loaded.Modules[0].Succeeded);
        Assert.Equal(12, loaded.Modules[0].RecordsInserted);
        Assert.False(loaded.Modules[1].Succeeded);
        Assert.False(File.Exists(path + ".tmp"));
    }
}
