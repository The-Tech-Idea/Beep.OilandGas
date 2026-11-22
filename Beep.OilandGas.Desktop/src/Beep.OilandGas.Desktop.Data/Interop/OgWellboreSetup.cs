using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;
using TheTechIdea.OilGas.Wellbore.Setup;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;

/// <summary>One country preset the wizard can start from (INT-05).</summary>
public sealed record OgCountryOption(string Code, string Name)
{
    public override string ToString() => Name;
}

/// <summary>One step of the setup, in the plan's vocabulary (INT-05's <c>OgSetupStep</c>).</summary>
/// <param name="State">NotStarted, Done or Blocked - the engine's own three, named as the plan names them.</param>
public sealed record OgSetupStep(string Key, string Title, string State, string Detail)
{
    public bool Done => string.Equals(State, OgSetupRules.Done, StringComparison.Ordinal);

    public bool Blocked => string.Equals(State, OgSetupRules.Blocked, StringComparison.Ordinal);

    public string Describe() => Key + " · " + Title + " · " + State.ToLowerInvariant() + " · " + Detail;
}

/// <summary>What testing a data source gave (INT-05, AC-2).</summary>
public sealed record OgSourceReport(
    string Name,
    string Kind,
    bool Loaded,
    string? Error,
    IReadOnlyList<string> Counts,
    IReadOnlyList<string> Warnings)
{
    public string Describe() => Name + " (" + Kind + ") · " +
                                (Loaded ? "loaded · " + string.Join(", ", Counts) : "failed · " + (Error ?? "no reason given"));
}

/// <summary>One source's coverage (INT-05, AC-4) - or an explicit unknown when it cannot be computed.</summary>
public sealed record OgCoverageReport(
    string Source,
    bool Known,
    int Wells,
    IReadOnlyList<string> Checked,
    IReadOnlyList<string> Problems,
    IReadOnlyList<string> Failed,
    string Reason)
{
    public string Describe() => !Known
        ? Source + " · coverage unknown: " + Reason
        : Source + " · " + Wells.ToString(CultureInfo.InvariantCulture) + " well(s) scanned · " +
          Problems.Count.ToString(CultureInfo.InvariantCulture) + " unresolved value(s)" +
          (Failed.Count == 0 ? string.Empty : " · " + Failed.Count.ToString(CultureInfo.InvariantCulture) + " well(s) failed");
}

/// <summary>A proposal the wizard shows and never applies (INT-05, AC-3).</summary>
public sealed record OgSetupSuggestion(
    string Area,
    string Where,
    string Value,
    string Target,
    string MatchedOn,
    double Score,
    string Source)
{
    public string Describe() =>
        Source + " · " + Area + " · " + Where + " · '" + Value + "' → '" + Target + "' (matched '" + MatchedOn + "', " +
        (Score * 100d).ToString("0", CultureInfo.InvariantCulture) + " %)";
}

/// <summary>One verification case's result (INT-05, AC-5).</summary>
public sealed record OgVerificationRow(string Name, string Kind, bool Passed, string? Error, IReadOnlyList<string> Values)
{
    public string Describe() => (Passed ? "PASS " : "FAIL ") + Name + " (" + Kind + ")" +
                                (Error is null ? string.Empty : ": " + Error) +
                                (Values.Count == 0 ? string.Empty : " · " + string.Join("; ", Values));
}

/// <summary>The setup workspace: the environment being set up and the engine's view of it (INT-05).</summary>
public sealed record OgSetupWorkspace(
    bool Exists,
    string? EnvironmentFile,
    string? Folder,
    string? Country,
    string? CountryName,
    string? CompanyName,
    IReadOnlyList<OgSetupStep> Steps,
    IReadOnlyList<OgSourceReport> Sources,
    IReadOnlyList<OgCoverageReport> Coverage,
    IReadOnlyList<OgSetupSuggestion> Suggestions,
    IReadOnlyList<OgVerificationRow> Verification,
    IReadOnlyList<string> Validation,
    bool HandedOver,
    string? ArchiveFolder,
    IReadOnlyList<string> Warnings,
    DateTime ReadAt)
{
    public int Outstanding => Verification.Count(row => !row.Passed);

    public int BlockedSteps => Steps.Count(step => step.Blocked);

    public bool CanHandOver => Exists && Outstanding == 0;

    public string SummaryText => !Exists
        ? "No environment is set up yet: choose a country preset and create one."
        : (CompanyName ?? "(no company name)") + " · extends " + (CountryName ?? Country ?? "(no preset)") +
          " · " + Steps.Count.ToString(CultureInfo.InvariantCulture) + " step(s), " +
          BlockedSteps.ToString(CultureInfo.InvariantCulture) + " needing attention · " +
          Validation.Count.ToString(CultureInfo.InvariantCulture) + " configuration problem(s) · " +
          Verification.Count.ToString(CultureInfo.InvariantCulture) + " verification case(s), " +
          Outstanding.ToString(CultureInfo.InvariantCulture) + " outstanding";

    public string StepsText => Steps.Count == 0
        ? "No steps: there is no environment to judge."
        : string.Join(Environment.NewLine, Steps.Select(step => step.Describe()));

    public string SourcesText => Sources.Count == 0
        ? "No data source is configured."
        : string.Join(Environment.NewLine, Sources.Select(source => source.Describe()));

    public string CoverageText => Coverage.Count == 0
        ? "No coverage has been computed: name a data source first."
        : string.Join(Environment.NewLine, Coverage.Select(coverage => coverage.Describe()));

    public string SuggestionsText => Suggestions.Count == 0
        ? "No suggestion is offered; nothing has been applied."
        : string.Join(Environment.NewLine, Suggestions.Select(suggestion => suggestion.Describe()));

    public string VerificationText => Verification.Count == 0
        ? "Verification has not been run."
        : string.Join(Environment.NewLine, Verification.Select(row => row.Describe()));

    public string ValidationText => Validation.Count == 0
        ? "The environment passes the engine's own validation."
        : string.Join(Environment.NewLine, Validation.Select(issue => "✗ " + issue));

    public string HandoverText => !Exists
        ? "There is nothing to hand over yet."
        : Outstanding == 0
            ? "Handover is allowed: every verification case passes."
            : "Handover is refused while " + Outstanding.ToString(CultureInfo.InvariantCulture) +
              " verification case(s) fail: " + string.Join("; ", Verification.Where(row => !row.Passed).Select(row => row.Name));

    public string WarningsText => Warnings.Count == 0
        ? "The engine raised no warning."
        : string.Join(Environment.NewLine, Warnings.Select(warning => "! " + warning));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
        (EnvironmentFile is null ? string.Empty : " · " + EnvironmentFile);
}

/// <summary>The setup rules INT-05 states once.</summary>
public static class OgSetupRules
{
    public const string CodeNoEnvironment = "setup-no-environment";
    public const string CodeReadFailed = "setup-read-failed";
    public const string CodeWriteFailed = "setup-write-failed";
    public const string CodeActorRequired = "setup-actor-required";
    public const string CodeCountryRequired = "setup-country-required";
    public const string CodeCountryUnknown = "setup-country-unknown";
    public const string CodeAlreadyExists = "setup-environment-exists";
    public const string CodePathRequired = "setup-path-required";
    public const string CodeValueInvalid = "setup-value-invalid";
    public const string CodeSourceRequired = "setup-source-required";
    public const string CodeSourceUnknown = "setup-source-unknown";
    public const string CodeSourceInvalid = "setup-source-invalid";
    public const string CodeSuggestionUnknown = "setup-suggestion-unknown";
    public const string CodeHandoverBlocked = "setup-handover-blocked";
    public const string CodeHandoverFolder = "setup-handover-folder";

    public const string NotStarted = "NotStarted";
    public const string InProgress = "InProgress";
    public const string Done = "Done";
    public const string Blocked = "Blocked";

    /// <summary>All four states the plan names, so a page can label them without a literal.</summary>
    public static IReadOnlyList<string> States { get; } = [NotStarted, InProgress, Done, Blocked];

    /// <summary>
    /// The engine's three states in the plan's vocabulary: complete is done, something to resolve is blocked with the engine's own
    /// detail as the reason. The engine has no 'in progress' - it judges a step rather than tracking a cursor - so that state is the
    /// page's, not the engine's, and is recorded as such.
    /// </summary>
    public static string StateOf(SetupStepState state) => state switch
    {
        SetupStepState.Done => Done,
        SetupStepState.NeedsAttention => Blocked,
        SetupStepState.NotStarted => NotStarted,
    };

    /// <summary>AC-5: the cases still failing, or null when handover may proceed.</summary>
    public static IReadOnlyList<string> Outstanding(IReadOnlyList<OgVerificationRow> verification) =>
        verification.Where(row => !row.Passed)
            .Select(row => row.Name + (row.Error is null ? string.Empty : " (" + row.Error + ")"))
            .ToList();

    /// <summary>
    /// Whether a written value is shaped like JSON at all. A value that is not - a company name, a locale - is text, and asking the
    /// parser would be asking an exception what an ordinary answer is.
    /// </summary>
    public static bool LooksLikeJson(string text) =>
        text.Length > 0 && (text[0] is '{' or '[' or '"' or '-' || char.IsDigit(text[0]) ||
                            text is "true" or "false" or "null");

    /// <summary>The company name the environment records, from either the session document or the effective environment.</summary>
    public static string? CompanyNameOf(SetupSession session) =>
        session.Effective("name")?.ToString() is { Length: > 0 } name ? name : null;
}

/// <summary>INT-05: the customer's environment, set up through the tools' own engine.</summary>
public interface IOgWellboreSetup
{
    /// <summary>The country presets the wizard can start from.</summary>
    Task<OgResult<IReadOnlyList<OgCountryOption>>> CountriesAsync(CancellationToken cancellationToken = default);

    /// <summary>The environment in effect and the engine's view of it - or an empty state when there is none.</summary>
    Task<OgResult<OgSetupWorkspace>> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>AC-1: starts the environment from a country preset.</summary>
    Task<OgResult<OgSetupWorkspace>> CreateAsync(
        string countryCode, string? companyName, string actor, CancellationToken cancellationToken = default);

    /// <summary>Writes one setting; a blank value removes it and lets the preset show through.</summary>
    Task<OgResult<OgSetupWorkspace>> SetAsync(
        string path, string? value, string actor, CancellationToken cancellationToken = default);

    /// <summary>Adds or replaces a data source from its definition JSON.</summary>
    Task<OgResult<OgSetupWorkspace>> AddSourceAsync(
        string name, string definitionJson, string actor, CancellationToken cancellationToken = default);

    /// <summary>Removes a data source.</summary>
    Task<OgResult<OgSetupWorkspace>> RemoveSourceAsync(
        string name, string actor, CancellationToken cancellationToken = default);

    /// <summary>AC-2: tests a data source and records what it gave.</summary>
    Task<OgResult<OgSetupWorkspace>> TestSourceAsync(
        string name, string? wellId, CancellationToken cancellationToken = default);

    /// <summary>AC-4: coverage per source, with an explicit unknown when it cannot be computed.</summary>
    Task<OgResult<OgSetupWorkspace>> CoverageAsync(
        string name, CancellationToken cancellationToken = default);

    /// <summary>AC-3: what the engine proposes for a source - a proposal only, never applied.</summary>
    Task<OgResult<OgSetupWorkspace>> SuggestAsync(
        string name, CancellationToken cancellationToken = default);

    /// <summary>AC-3: applies one named proposal, because a person accepted it.</summary>
    Task<OgResult<OgSetupWorkspace>> AcceptAsync(
        string name, string area, string where, string value, string target, string actor,
        CancellationToken cancellationToken = default);

    /// <summary>AC-5: runs every verification case.</summary>
    Task<OgResult<OgSetupWorkspace>> VerifyAsync(CancellationToken cancellationToken = default);

    /// <summary>AC-5: writes the hand-over configuration, or refuses while a case fails.</summary>
    Task<OgResult<OgSetupWorkspace>> HandoverAsync(
        string archiveFolder, string actor, CancellationToken cancellationToken = default);
}

/// <summary>
/// INT-05 over the tools' <see cref="SetupSession"/> and the environment file the application itself loads.
/// </summary>
/// <remarks>
/// <para>
/// The wizard is a view over the engine: every step's state, every source test, the coverage, the suggestions and the verification
/// are the session's own. What this service adds is where the environment lives, the refusal to hand over with a failing case, the
/// stateliness that makes a failed save harmless, and the mapping of the engine's three states into the plan's four.
/// </para>
/// <para>
/// A session is opened from the file for every command and never held: the engine writes on every change, so a session that survived
/// a failed write would carry a mutated document the file does not contain. Opening from the file means the file is always the
/// truth.
/// </para>
/// </remarks>
public sealed class OgWellboreSetupService : IOgWellboreSetup
{
    private readonly IFailureReporter _failures;
    private readonly Func<string> _environmentFile;

    public OgWellboreSetupService(IFailureReporter failures, Func<string> environmentFile)
    {
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _environmentFile = environmentFile ?? throw new ArgumentNullException(nameof(environmentFile));
    }

    public Task<OgResult<IReadOnlyList<OgCountryOption>>> CountriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var countries = SetupSession.Countries
                .Select(country => new OgCountryOption(country.Code, country.Name))
                .ToList();

            return Task.FromResult(OgResult<IReadOnlyList<OgCountryOption>>.Ok(countries));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "listing the country presets", "no preset can be chosen", FailureSeverity.Error);

            return Task.FromResult(OgResult<IReadOnlyList<OgCountryOption>>.Failed(
                "The country presets could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgSetupRules.CodeReadFailed));
        }
    }

    public Task<OgResult<OgSetupWorkspace>> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Opened like every other command: a workspace that did not open the file would report the absent state over a file
            // that is there, which is the one thing this page must never do.
            using var opened = Open();

            return Task.FromResult(OgResult<OgSetupWorkspace>.Ok(Build(opened.Session, [], [])));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the wellbore environment", "the setup is not shown", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "The environment could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgSetupRules.CodeReadFailed));
        }
    }

    public Task<OgResult<OgSetupWorkspace>> CreateAsync(
        string countryCode, string? companyName, string actor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "Setting up an environment records who did it.", OgSetupRules.CodeActorRequired));
        }

        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "A country preset is what an environment starts from; none was given.", OgSetupRules.CodeCountryRequired));
        }

        var file = _environmentFile();

        if (File.Exists(file))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "An environment already exists at '" + file +
                "'. Open it and change it rather than creating a second one, so the settings the application loads cannot be " +
                "split between two files.", OgSetupRules.CodeAlreadyExists));
        }

        try
        {
            var folder = Path.GetDirectoryName(file) ?? throw new InvalidOperationException("the environment file has no folder");

            var session = SetupSession.Create(folder, countryCode.Trim(), companyName);

            return Task.FromResult(OgResult<OgSetupWorkspace>.Ok(Build(session, [], [])));
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or JsonException)
        {
            var reference = _failures.ReportHandled(
                ex, "creating the environment from " + countryCode, "no environment is created", FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "The environment could not be created from '" + countryCode.Trim() + "': " +
                (ex is ArgumentException ? "it is not a country preset. " : "the file could not be written. ") +
                "(reference " + reference + ")", OgSetupRules.CodeCountryUnknown));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "creating the environment from " + countryCode, "no environment is created", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "The environment could not be created (reference " + reference + ").",
                reference ?? OgSetupRules.CodeWriteFailed));
        }
    }

    public Task<OgResult<OgSetupWorkspace>> SetAsync(
        string path, string? value, string actor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "Changing the environment records who did it.", OgSetupRules.CodeActorRequired));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "A setting is written by its path; none was given.", OgSetupRules.CodePathRequired));
        }

        return Write(session =>
        {
            session.Set(path.Trim(), ValueOf(value));

            return ([], null);
        }, "writing " + path.Trim());
    }

    public Task<OgResult<OgSetupWorkspace>> AddSourceAsync(
        string name, string definitionJson, string actor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "Changing the environment records who did it.", OgSetupRules.CodeActorRequired));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "A data source needs a name; none was given.", OgSetupRules.CodeSourceRequired));
        }

        return Write(session =>
        {
            if (DefinitionOf(definitionJson) is not { } definition)
            {
                return ([], "the data source definition is not JSON: a source is described by its kind and its path, for example " +
                             "{ \"kind\": \"csv\", \"path\": \"wells\" }");
            }

            session.SetDataSource(name.Trim(), definition);

            return ([], null);
        }, "adding the data source " + name.Trim(), OgSetupRules.CodeSourceInvalid);
    }

    public Task<OgResult<OgSetupWorkspace>> RemoveSourceAsync(
        string name, string actor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "Changing the environment records who did it.", OgSetupRules.CodeActorRequired));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "A data source is removed by its name; none was given.", OgSetupRules.CodeSourceRequired));
        }

        return Write(session =>
        {
            if (!session.DataSources.ContainsKey(name.Trim()))
            {
                return (new List<string> { "there is no data source called '" + name.Trim() + "', so nothing was removed" }, null);
            }

            session.RemoveDataSource(name.Trim());

            return ([], null);
        }, "removing the data source " + name.Trim());
    }

    public Task<OgResult<OgSetupWorkspace>> TestSourceAsync(
        string name, string? wellId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "A data source is tested by its name; none was given.", OgSetupRules.CodeSourceRequired));
        }

        try
        {
            using var opened = Open();

            if (opened.Session is null)
            {
                return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                    "There is no environment to test a source against.", OgSetupRules.CodeNoEnvironment));
            }

            var result = opened.Session.TestSource(name.Trim(), string.IsNullOrWhiteSpace(wellId) ? null : wellId.Trim());
            var warnings = result.Loaded
                ? result.Warnings.ToList()
                : ["'" + name.Trim() + "' did not load: " + (result.Error ?? "no reason given")];

            // AC-2: the result is what the workspace shows, whether it loaded or not - a failure is recorded, not thrown away.
            return Task.FromResult(OgResult<OgSetupWorkspace>.Ok(Build(opened.Session, warnings, [name.Trim()])));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "testing the data source " + name, "the test result is not shown", FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "The source could not be tested (reference " + reference + ").", reference ?? OgSetupRules.CodeReadFailed));
        }
    }

    public Task<OgResult<OgSetupWorkspace>> CoverageAsync(
        string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "Coverage is computed per source; none was named.", OgSetupRules.CodeSourceRequired));
        }

        try
        {
            using var opened = Open();

            if (opened.Session is null)
            {
                return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                    "There is no environment to compute coverage for.", OgSetupRules.CodeNoEnvironment));
            }

            if (!opened.Session.DataSources.ContainsKey(name.Trim()))
            {
                return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                    "There is no data source called '" + name.Trim() + "', so it has no coverage.",
                    OgSetupRules.CodeSourceUnknown));
            }

            return Task.FromResult(OgResult<OgSetupWorkspace>.Ok(Build(opened.Session, [], [name.Trim()])));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            // AC-4's own state: coverage that cannot be computed is reported as unknown with its reason, not as zero problems.
            var reference = _failures.ReportHandled(
                ex, "computing coverage for " + name, "coverage is reported as unknown", FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgSetupWorkspace>.Ok(Build(
                null,
                ["coverage for '" + name.Trim() + "' could not be computed (reference " + reference + ")"],
                [name.Trim()],
                coverageFailure: (name.Trim(), "it could not be computed (reference " + reference + ")"))));
        }
    }

    public Task<OgResult<OgSetupWorkspace>> SuggestAsync(
        string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "Suggestions are proposed per source; none was named.", OgSetupRules.CodeSourceRequired));
        }

        try
        {
            using var opened = Open();

            if (opened.Session is null)
            {
                return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                    "There is no environment to propose mappings for.", OgSetupRules.CodeNoEnvironment));
            }

            if (!opened.Session.DataSources.ContainsKey(name.Trim()))
            {
                return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                    "There is no data source called '" + name.Trim() + "'.", OgSetupRules.CodeSourceUnknown));
            }

            // AC-3: the proposals are returned and nothing is written - the session is opened read-only for this call.
            var suggestions = opened.Session.Suggestions(name.Trim())
                .Select(suggestion => new OgSetupSuggestion(
                    suggestion.Area, suggestion.Where, suggestion.Value, suggestion.Target,
                    suggestion.MatchedOn, suggestion.Score, suggestion.By))
                .ToList();

            var warnings = suggestions.Count == 0
                ? new List<string> { "no suggestion was offered for '" + name.Trim() + "'" }
                : new List<string>();

            return Task.FromResult(OgResult<OgSetupWorkspace>.Ok(Build(opened.Session, warnings, [], suggestions)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "proposing mappings for " + name, "no suggestion is shown", FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "The suggestions could not be computed (reference " + reference + ").", reference ?? OgSetupRules.CodeReadFailed));
        }
    }

    public Task<OgResult<OgSetupWorkspace>> AcceptAsync(
        string name, string area, string where, string value, string target, string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "Accepting a suggestion records who did it.", OgSetupRules.CodeActorRequired));
        }

        return Write(session =>
        {
            if (!session.DataSources.ContainsKey(name.Trim()))
            {
                return ([], "there is no data source called '" + name.Trim() + "', so no suggestion matches it");
            }

            // The proposal is looked up again, so what is applied is what the engine offers now rather than what a screen remembered.
            var suggestion = session.Suggestions(name.Trim())
                .FirstOrDefault(candidate =>
                    string.Equals(candidate.Area, area, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(candidate.Where, where, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(candidate.Value, value, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(candidate.Target, target, StringComparison.OrdinalIgnoreCase));

            if (suggestion is null)
            {
                return ([], "the engine no longer offers '" + value + "' → '" + target + "' for " + where +
                             ", so nothing was applied");
            }

            session.Accept(name.Trim(), suggestion);

            return (new List<string> { "applied '" + value + "' → '" + target + "' for " + where + ", because it was accepted" }, null);
        }, "accepting a suggestion for " + name.Trim(), OgSetupRules.CodeSuggestionUnknown);
    }

    public Task<OgResult<OgSetupWorkspace>> VerifyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var opened = Open();

            if (opened.Session is null)
            {
                return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                    "There is no environment to verify.", OgSetupRules.CodeNoEnvironment));
            }

            return Task.FromResult(OgResult<OgSetupWorkspace>.Ok(Build(opened.Session, [], [], verify: true)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "verifying the environment", "the verification is not shown", FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "The environment could not be verified (reference " + reference + ").", reference ?? OgSetupRules.CodeReadFailed));
        }
    }

    public Task<OgResult<OgSetupWorkspace>> HandoverAsync(
        string archiveFolder, string actor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "A hand-over records who made it.", OgSetupRules.CodeActorRequired));
        }

        if (string.IsNullOrWhiteSpace(archiveFolder))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "A hand-over writes an archive; no folder was given.", OgSetupRules.CodeHandoverFolder));
        }

        try
        {
            using var opened = Open();

            if (opened.Session is null)
            {
                return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                    "There is no environment to hand over.", OgSetupRules.CodeNoEnvironment));
            }

            // AC-5: verification is run here, so the refusal is about the checks as they are now rather than as a screen remembers
            // them.
            var workspace = Build(opened.Session, [], [], verify: true);

            if (OgSetupRules.Outstanding(workspace.Verification) is { Count: > 0 } outstanding)
            {
                return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                    "The hand-over is refused while " + outstanding.Count.ToString(CultureInfo.InvariantCulture) +
                    " verification case(s) fail: " + string.Join("; ", outstanding) +
                    ". Resolve them and verify again.",
                    OgSetupRules.CodeHandoverBlocked));
            }

            var report = opened.Session.Handover(archiveFolder.Trim());

            return Task.FromResult(OgResult<OgSetupWorkspace>.Ok(
                Build(opened.Session, report.Skipped, [], verify: false, handedOver: true, archive: archiveFolder.Trim(),
                    extra: report.Files.Count.ToString(CultureInfo.InvariantCulture) + " file(s) written to the archive")));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "handing the environment over", "nothing is handed over", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "The hand-over failed (reference " + reference + ").", reference ?? OgSetupRules.CodeHandoverBlocked));
        }
    }

    // ------------------------------------------------------------------ the session and the workspace

    private Task<OgResult<OgSetupWorkspace>> Write(
        Func<SetupSession, (List<string> Warnings, string? Problem)> change,
        string what,
        string refusalCode = OgSetupRules.CodeWriteFailed)
    {
        var file = _environmentFile();

        if (!File.Exists(file))
        {
            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "There is no environment at '" + file + "' to write to: create one from a country preset first.",
                OgSetupRules.CodeNoEnvironment));
        }

        try
        {
            // Opened, changed and dropped inside the call: a failed write leaves nothing behind that a later call could read as
            // state, because the file is what every call opens.
            using var opened = Open();

            if (opened.Session is null)
            {
                return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                    "The environment could not be opened.", OgSetupRules.CodeNoEnvironment));
            }

            var (warnings, problem) = change(opened.Session);

            if (problem is not null)
            {
                // An ordinary refusal (an unknown suggestion, a source that is not there) is a verdict, not a failure: nothing was
                // thrown, so nothing is reported as one.
                return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(problem, refusalCode));
            }

            return Task.FromResult(OgResult<OgSetupWorkspace>.Ok(Build(opened.Session, warnings, [])));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            // AC-6: the write failed, so what is on disk is what was there before - which the caller can see by reloading.
            var reference = _failures.ReportHandled(
                ex, what, "the change is not saved and the previous environment is unchanged", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "The change could not be saved, so the previous environment is unchanged (reference " + reference + ").",
                reference ?? OgSetupRules.CodeWriteFailed));
        }
    }

    /// <summary>
    /// The value a setting is written as: JSON when it is shaped like JSON, otherwise the text as a string. A company name is a
    /// string, a number stays a number, and a gap - null - removes the setting so the preset shows through again. A value that is
    /// shaped like JSON but is not valid JSON is written as text and reported, because that is a person's typing rather than a fault.
    /// </summary>
    private JsonNode? ValueOf(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var trimmed = text.Trim();

        if (!OgSetupRules.LooksLikeJson(trimmed))
        {
            return JsonValue.Create(trimmed);
        }

        try
        {
            return JsonNode.Parse(trimmed);
        }
        catch (JsonException ex)
        {
            _failures.ReportHandled(
                ex, "reading the value written to the environment", "the value is written as text",
                FailureSeverity.Degraded);

            return JsonValue.Create(trimmed);
        }
    }

    private Opened Open()
    {
        var file = _environmentFile();

        return File.Exists(file)
            ? new Opened(SetupSession.Open(file))
            : new Opened(null);
    }

    private OgSetupWorkspace Build(
        SetupSession? session,
        IReadOnlyList<string> warnings,
        IReadOnlyList<string> coverageSources,
        IReadOnlyList<OgSetupSuggestion>? suggestions = null,
        bool verify = false,
        bool handedOver = false,
        string? archive = null,
        (string Source, string Reason)? coverageFailure = null,
        string? extra = null)
    {
        var file = _environmentFile();
        var notes = warnings.ToList();

        if (extra is not null)
        {
            notes.Add(extra);
        }

        if (session is null)
        {
            // No environment yet: the steps the engine would judge are reported as not started, so the page shows the whole wizard
            // rather than an empty list.
            var cold = KnownStepIds.Select(id => new OgSetupStep(id, TitleOf(id), OgSetupRules.NotStarted,
                "no environment yet - choose a country preset")).ToList();

            return new OgSetupWorkspace(
                false, File.Exists(file) ? file : null, Path.GetDirectoryName(file),
                null, null, null, cold, [], [], suggestions ?? [], [], [], handedOver, archive, notes, DateTime.UtcNow);
        }

        var steps = session.Steps()
            .Select(step => new OgSetupStep(step.Id, step.Title, OgSetupRules.StateOf(step.State), step.Detail))
            .ToList();

        var sources = session.DataSources
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair =>
            {
                var result = session.TestSource(pair.Key);

                return new OgSourceReport(
                    pair.Key,
                    pair.Value?.Kind ?? "(no kind)",
                    result.Loaded,
                    result.Error,
                    result.Counts.OrderBy(count => count.Key, StringComparer.Ordinal)
                        .Select(count => count.Key + " " + count.Value.ToString(CultureInfo.InvariantCulture))
                        .ToList(),
                    result.Warnings.ToList());
            })
            .ToList();

        var coverage = new List<OgCoverageReport>();

        foreach (var name in coverageSources.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (coverageFailure is { } failure && string.Equals(failure.Source, name, StringComparison.OrdinalIgnoreCase))
            {
                // AC-4: unknown, with the reason - never "no problems found".
                coverage.Add(new OgCoverageReport(name, false, 0, [], [], [], failure.Reason));
                continue;
            }

            try
            {
                var report = session.Coverage(name);

                coverage.Add(new OgCoverageReport(
                    name,
                    true,
                    report.Wells,
                    report.Checked.OrderBy(check => check.Key, StringComparer.Ordinal)
                        .Select(check => check.Key + " " + check.Value.ToString(CultureInfo.InvariantCulture))
                        .ToList(),
                    report.Problems.Select(problem =>
                        problem.Area + " · " + problem.Where + " · '" + problem.Value + "' · " + problem.Problem +
                        (string.IsNullOrWhiteSpace(problem.Fix) ? string.Empty : " (fix: " + problem.Fix + ")")).ToList(),
                    report.Failed.ToList(),
                    "computed"));
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
            {
                var reference = _failures.ReportHandled(
                    ex, "computing coverage for " + name, "coverage is reported as unknown", FailureSeverity.Degraded);

                coverage.Add(new OgCoverageReport(name, false, 0, [], [], [],
                    "it could not be computed (reference " + reference + ")"));
            }
        }

        var verification = verify
            ? session.Verify().Select(result => new OgVerificationRow(
                result.Name,
                result.Kind,
                result.Passed,
                result.Error,
                result.Values.Select(value => value.quantity + " " + value.actual.ToString("0.####", CultureInfo.InvariantCulture) +
                                             " against " + value.expected.ToString("0.####", CultureInfo.InvariantCulture) +
                                             " ± " + value.tolerance.ToString("0.####", CultureInfo.InvariantCulture)).ToList()))
                .ToList()
            : [];

        var country = session.Country;
        var countryName = country is null ? null : CountryPresets.NameOf(country) ?? country;

        return new OgSetupWorkspace(
            true,
            session.EnvironmentFile,
            session.Folder,
            country,
            countryName,
            OgSetupRules.CompanyNameOf(session),
            steps,
            sources,
            coverage,
            suggestions ?? [],
            verification,
            session.Validate().ToList(),
            handedOver,
            archive,
            notes,
            DateTime.UtcNow);
    }

    /// <summary>The steps the engine judges, so a page with no environment still shows the whole wizard.</summary>
    private static readonly string[] KnownStepIds =
        ["country", "sources", "codes", "equipment", "stratigraphy", "lithology", "logs", "units", "tubulars", "surveys", "verification"];

    private static string TitleOf(string id) => id switch
    {
        "country" => "Country and company",
        "sources" => "Data sources",
        "codes" => "Codes and values",
        "equipment" => "Equipment and symbols",
        "stratigraphy" => "Stratigraphy",
        "lithology" => "Lithology",
        "logs" => "Logs",
        "units" => "Units",
        "tubulars" => "Tubulars and tool models",
        "surveys" => "Surveys",
        "verification" => "Verify and hand over",
        _ => id,
    };

    /// <summary>
    /// A data source definition read from JSON, or null when the text is not one. The parser's own sentence is not used: the caller
    /// gets a sentence about what a definition is.
    /// </summary>
    private DataSourceDefinition? DefinitionOf(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<DataSourceDefinition>(json, WellboreEnvironment.JsonOptions);
        }
        catch (JsonException ex)
        {
            // Reading a definition is where a person's text arrives; the parser's objection is reported, and the caller is told what
            // the shape should be rather than what the reader disliked.
            _failures.ReportHandled(
                ex, "reading a data source definition", "the definition is refused", FailureSeverity.Degraded);

            return null;
        }
    }

    /// <summary>The session and the lifetime that keeps it in one scope, so a command cannot leave a scope behind.</summary>
    private sealed class Opened : IDisposable
    {
        public Opened(SetupSession? session) => Session = session;

        public SetupSession? Session { get; }

        public void Dispose()
        {
            // The session itself owns no unmanaged state; the scope it uses inside each call is the engine's.
        }
    }
}
