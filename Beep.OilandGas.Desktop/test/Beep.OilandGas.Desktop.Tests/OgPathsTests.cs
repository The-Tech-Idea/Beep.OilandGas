using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ADR-013: every writable path resolves under the per-user data root and never beside the executable.
/// </summary>
public sealed class OgPathsTests
{
    [Fact]
    public void FailureStore_is_under_the_data_root()
    {
        Assert.True(
            OgPaths.IsWritableLocation(OgPaths.FailureStorePath),
            $"The failure store resolved to '{OgPaths.FailureStorePath}', outside '{OgPaths.DataRoot}'.");
    }

    [Fact]
    public void FailureStore_is_not_beside_the_executable()
    {
        var beside = Path.Combine(AppContext.BaseDirectory, "failures.db");
        Assert.False(OgPaths.IsWritableLocation(beside));
    }

    [Fact]
    public void Log_directory_is_under_the_data_root()
    {
        Assert.True(OgPaths.IsWritableLocation(OgPaths.LogDirectory));
    }

    [Fact]
    public void Every_writable_directory_is_under_the_data_root()
    {
        foreach (var directory in OgPaths.WritableDirectories)
        {
            Assert.True(
                OgPaths.IsWritableLocation(directory),
                $"'{directory}' is not under '{OgPaths.DataRoot}'.");
        }
    }

    [Fact]
    public void Empty_and_null_paths_are_refused()
    {
        Assert.False(OgPaths.IsWritableLocation(string.Empty));
        Assert.False(OgPaths.IsWritableLocation("   "));
    }
}
