using System.Text.Json;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;

/// <summary>Which modules have completed for one connection, so a failed run resumes instead of repeating.</summary>
public sealed class OgSetupState
{
    public string ConnectionName { get; set; } = string.Empty;

    public List<OgSetupModuleState> Modules { get; set; } = [];

    /// <summary>True once every module completed in a single successful run.</summary>
    public bool Completed { get; set; }

    public DateTimeOffset? LastRunUtc { get; set; }
}

public sealed class OgSetupModuleState
{
    public string ModuleId { get; set; } = string.Empty;

    public bool Succeeded { get; set; }

    public int RecordsInserted { get; set; }

    public DateTimeOffset CompletedAtUtc { get; set; }
}

/// <summary>
/// Stores <see cref="OgSetupState"/> as JSON beside the caller's configuration. The runner does not know
/// the application's file layout; the host passes the path.
/// </summary>
public sealed class OgSetupStateStore
{
    private readonly string _path;

    public OgSetupStateStore(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
    }

    public string Path => _path;

    public OgSetupState Load()
    {
        if (!File.Exists(_path))
        {
            return new OgSetupState();
        }

        using var stream = File.OpenRead(_path);
        return JsonSerializer.Deserialize<OgSetupState>(stream) ?? new OgSetupState();
    }

    public void Save(OgSetupState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var directory = System.IO.Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporary = _path + ".tmp";
        using (var stream = File.Create(temporary))
        {
            JsonSerializer.Serialize(stream, state, new JsonSerializerOptions { WriteIndented = true });
        }

        File.Move(temporary, _path, overwrite: true);
    }
}
