using System.IO;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>What a LAS file says before it is committed (INT-01 §3 step 3).</summary>
public sealed record OgLasPreview(
    string Standard,
    string? Encoding,
    string IndexType,
    string IndexUnit,
    IReadOnlyList<string> Curves,
    int Rows,
    double Top,
    double Bottom,
    bool Decimated,
    string? DecimationNote,
    bool HasErrors,
    IReadOnlyList<string> Warnings)
{
    /// <summary>One sentence for the page and the summary line.</summary>
    public string Describe() =>
        $"{Standard} · {Curves.Count} curve(s) · {Rows} row(s) · {Top:0.###}–{Bottom:0.###} {IndexUnit}" +
        (Decimated ? $" · {DecimationNote}" : string.Empty);
}

/// <summary>A loaded LAS/DLIS file plus the preview that describes it.</summary>
public sealed record OgLasImport(WellModel Model, LogSet? Log, OgLasPreview Preview);

/// <summary>A DLIS file: every frame the reader could load, and every frame it could not.</summary>
public sealed record OgDlisImport(WellModel Model, IReadOnlyList<LogSet> LogSets, IReadOnlyList<string> Warnings)
{
    public int Frames => LogSets.Count;
}

/// <summary>
/// INT-01: LAS 2.0 / LAS 3.0 / DLIS through the tools' loaders and writers.
/// </summary>
/// <remarks>
/// <para>
/// The reader never throws on bad <b>content</b>: problems arrive as warnings and <c>HasErrors</c>, and every
/// warning is kept — a file that loaded "with two warnings" is different from one that loaded cleanly, and the
/// page must be able to say which. Reading a <b>missing</b> file is an I/O failure, so the page reports it by
/// reference rather than pretending the file was empty.
/// </para>
/// <para>
/// Decimation is stated, never silent: <see cref="OgLasPreview.DecimationNote"/> carries both counts and the
/// ratio, because a curve drawn from every tenth sample is a different curve from the one on disk.
/// </para>
/// <para>
/// DLIS is read-only here: the tools ship no DLIS writer, and the export tab says so rather than offering a
/// format that would fail.
/// </para>
/// </remarks>
public static class OgLasWorkspace
{
    public const string Las20 = "2.0";
    public const string Las30 = "3.0";

    public const string CodeExportExists = "las-export-exists";
    public const string CodeNoLog = "las-no-log-set";
    public const string CodeWriteFailed = "las-write-failed";

    /// <summary>A small, deterministic LAS 2.0 file used by the page's example button and the smoke run.</summary>
    public const string ExampleLas = """
                                     ~Version Information
                                      VERS.                  2.0 : CWLS LOG ASCII STANDARD - VERSION 2.0
                                      WRAP.                  NO  : One line per depth step
                                     ~Well Information
                                      STRT.M                1000.0 : Start depth
                                      STOP.M                1004.0 : Stop depth
                                      STEP.M                   0.5 : Step
                                      NULL.               -999.25 : Null value
                                      WELL.              DEMO-LAS-1 : Well
                                      COMP.              Probe Energy : Company
                                      UWI.               DEMO-LAS-1-UWI : Unique well identifier
                                     ~Curve Information
                                      DEPT.M                        : Index
                                      GR.API                        : Gamma ray
                                      RHOB.g/cm3                    : Bulk density
                                     ~ASCII
                                     1000.0   45.0   2.35
                                     1000.5   47.5   2.36
                                     1001.0   52.0   2.37
                                     1001.5   49.0   2.36
                                     1002.0   44.0   2.34
                                     1002.5   41.0   2.33
                                     1003.0   39.5   2.32
                                     1003.5   42.0   2.33
                                     1004.0   46.0   2.35
                                     """;

    /// <summary>Loads a LAS file and keeps every warning the reader produced.</summary>
    public static OgLasImport Load(string path, string? encoding = null, int? maxSamples = null)
    {
        var result = LasReader.LoadFile(path, into: null, new LasReadOptions
        {
            Encoding = string.IsNullOrWhiteSpace(encoding) ? null : encoding,
            MaxSamples = maxSamples,
        });

        return new OgLasImport(result.Model, result.Log, Preview(result));
    }

    /// <summary>Loads LAS text, for a file already in memory or a test fixture.</summary>
    public static OgLasImport LoadText(string text, string? name = null, int? maxSamples = null)
    {
        // The direct-text reader has the "empty content" guard; keep one refusal sentence for both entry points.
        if (string.IsNullOrWhiteSpace(text))
        {
            var blank = LasReader.Load(text, into: null, name);
            return new OgLasImport(blank.Model, blank.Log, Preview(blank));
        }

        using var reader = new StringReader(text);

        var result = LasReader.LoadFrom(
            reader,
            into: null,
            name,
            new LasReadOptions { MaxSamples = maxSamples });

        return new OgLasImport(result.Model, result.Log, Preview(result));
    }

    /// <summary>Loads a DLIS file. Read-only: the tools ship no DLIS writer.</summary>
    public static OgDlisImport LoadDlis(string path)
    {
        var result = DlisReader.LoadFile(path);

        return new OgDlisImport(
            result.Model,
            result.LogSets,
            result.Warnings.Concat(result.File.Warnings).Distinct(StringComparer.Ordinal).ToList());
    }

    /// <summary>Writes a log set as LAS 2.0 or 3.0 text (INT-01 AC-2, AC-6).</summary>
    public static OgResult<string> ExportText(WellModel model, LogSet? set, string version)
    {
        ArgumentNullException.ThrowIfNull(model);

        var chosen = set ?? model.LogSets.FirstOrDefault(item => item.IndexType == LogIndexType.Depth);

        if (chosen is null)
        {
            return OgResult<string>.Failed(
                "The model has no depth-indexed log set to write as LAS.",
                CodeNoLog);
        }

        var text = LasWriter.Write(model, chosen, new LasWriteOptions
        {
            Version = string.Equals(version, Las30, StringComparison.Ordinal) ? Las30 : Las20,
        });

        return OgResult<string>.Ok(text);
    }

    /// <summary>
    /// Writes an export to disk. An existing file is refused unless <paramref name="overwrite"/> is set, so a
    /// mis-typed path cannot destroy a delivered log (INT-01 §5).
    /// </summary>
    public static OgResult<string> ExportFile(
        WellModel model,
        LogSet? set,
        string version,
        string path,
        bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return OgResult<string>.Failed("An export needs a destination path.", CodeWriteFailed);
        }

        var text = ExportText(model, set, version);

        if (!text.Success || text.Value is null)
        {
            return text;
        }

        if (File.Exists(path) && !overwrite)
        {
            return OgResult<string>.Failed(
                $"'{path}' already exists. Tick overwrite to replace it.",
                CodeExportExists);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, text.Value);

        return OgResult<string>.Ok(path);
    }

    /// <summary>The tools' LAS 3.0 compliance findings; empty means the file is compliant (AC-2).</summary>
    public static IReadOnlyList<string> CheckLas3(string text) => Las3Checker.Check(text);

    private static OgLasPreview Preview(LasLoadResult result)
    {
        var log = result.Log;
        var rows = log?.Index.Length ?? 0;

        var top = rows > 0 ? log!.Index[0] : 0d;
        var bottom = rows > 0 ? log!.Index[^1] : 0d;

        var note = result.Decimated
            ? rows > 0
                ? $"Decimated: {result.SourceRows} source row(s) shown as {rows} ({(result.SourceRows / (double)rows):0.#}:1)."
                : $"Decimated from {result.SourceRows} source row(s)."
            : null;

        return new OgLasPreview(
            Standard: result.Standard,
            Encoding: result.Encoding,
            IndexType: log?.IndexType.ToString() ?? "(none)",
            IndexUnit: log?.IndexUnit ?? string.Empty,
            Curves: log?.Curves.Select(curve => curve.Mnemonic).ToList() ?? [],
            Rows: rows,
            Top: top,
            Bottom: bottom,
            Decimated: result.Decimated,
            DecimationNote: note,
            HasErrors: result.HasErrors,
            Warnings: result.Warnings.ToList());
    }
}
