using System.Globalization;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.WellTest;

/// <summary>One test in the register (WTA-02), matching the plan's <c>OgWellTestSummary</c> plus the units and the link.</summary>
public sealed record OgWellTestSummary(
    string RegisterId,
    string TestKey,
    string? WellId,
    DateTime TestDate,
    string TestType,
    string? TestTypeName,
    decimal? Duration,
    string? DurationUnit,
    string? ResultCode,
    string? ResultName,
    string? ProductionMethod,
    string? Remarks,
    decimal? OilRate,
    string? OilRateUnit,
    decimal? GasRate,
    string? GasRateUnit,
    decimal? FlowingPressure,
    string? FlowingPressureUnit,
    int AnalysisCount,
    string? Actor,
    DateTime? RecordedAt)
{
    public bool IsSaved => !string.IsNullOrWhiteSpace(RegisterId);

    /// <summary>AC-4: no result code is a pending test, not a failed one.</summary>
    public bool PendingResult => string.IsNullOrWhiteSpace(ResultCode);

    public bool HasAnalysis => AnalysisCount > 0;

    public string TypeText => string.IsNullOrWhiteSpace(TestTypeName)
        ? TestType
        : TestType + " (" + TestTypeName + ")";

    public string DurationText => Duration is { } duration
        ? duration.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + (DurationUnit ?? string.Empty).Trim()
        : "no duration recorded";

    /// <summary>AC-4: the state is said in words, and pending is never rendered as a failure.</summary>
    public string StatusText => PendingResult
        ? "pending result"
        : string.IsNullOrWhiteSpace(ResultName)
            ? "result " + ResultCode
            : "result " + ResultCode + " (" + ResultName + ")";

    /// <summary>AC-5: a rate or pressure is never shown without its unit.</summary>
    public string RatesText
    {
        get
        {
            var parts = new List<string>();

            if (OilRate is { } oil)
            {
                parts.Add(Number(oil) + " " + Unit(OilRateUnit));
            }

            if (GasRate is { } gas)
            {
                parts.Add(Number(gas) + " " + Unit(GasRateUnit));
            }

            if (FlowingPressure is { } pressure)
            {
                parts.Add("flowing " + Number(pressure) + " " + Unit(FlowingPressureUnit));
            }

            return parts.Count == 0 ? "no rate or pressure recorded" : string.Join(" · ", parts);
        }
    }

    public string AnalysisText => AnalysisCount switch
    {
        0 => "not analysed - link it on the well test analysis screen",
        1 => "1 analysis",
        _ => AnalysisCount.ToString(CultureInfo.InvariantCulture) + " analyses",
    };

    public string Describe() =>
        $"{WellId ?? "(no well)"} · {TestDate:yyyy-MM-dd} · {TypeText} · {DurationText} · {StatusText} · " +
        AnalysisText;

    private static string Number(decimal value) =>
        value.ToString("#,0.###", CultureInfo.InvariantCulture);

    private static string Unit(string? unit) =>
        string.IsNullOrWhiteSpace(unit) ? "(no unit)" : unit.Trim();
}

/// <summary>The fields a register entry is created or edited from (WTA-02).</summary>
public sealed record OgWellTestEntry(
    string? RegisterId,
    string? WellId,
    DateTime? TestDate,
    string? TestType,
    int? RunNumber,
    int? TestNumber,
    decimal? Duration,
    string? DurationUnit,
    string? ResultCode,
    string? ProductionMethod,
    string? Remarks,
    decimal? OilRate,
    string? OilRateUnit,
    decimal? GasRate,
    string? GasRateUnit,
    decimal? FlowingPressure,
    string? FlowingPressureUnit);

/// <summary>The register workspace (WTA-02).</summary>
public sealed record OgWellTestRegisterWorkspace(
    IReadOnlyList<OgWellTestSummary> Tests,
    OgWellTestSummary? Selected,
    IReadOnlyList<string> TestTypes,
    IReadOnlyList<string> ResultCodes,
    IReadOnlyList<string> DurationUnits,
    bool ReferenceFromTables,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string TestsText => Tests.Count == 0
        ? "No well test is registered yet."
        : string.Join(Environment.NewLine, Tests.Select(test => test.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the register could not be read; new tests can still be recorded"
        : $"{Tests.Count.ToString(CultureInfo.InvariantCulture)} registered test(s)";

    public int PendingCount => Tests.Count(test => test.PendingResult);

    public int UnanalysedCount => Tests.Count(test => !test.HasAnalysis);

    public string ReferenceText => ReferenceFromTables
        ? "test types from R_WELL_TEST_TYPE and result codes from R_TEST_RESULT"
        : "the reference tables have no rows, so the type and result lists are empty and a test cannot be " +
          "registered until they are seeded";

    public string SummaryText =>
        $"{Tests.Count.ToString(CultureInfo.InvariantCulture)} test(s) · " +
        $"{PendingCount.ToString(CultureInfo.InvariantCulture)} pending a result · " +
        $"{UnanalysedCount.ToString(CultureInfo.InvariantCulture)} without an analysis";
}

/// <summary>The register rules, stated once so the page, the service and the tests agree.</summary>
public static class WellTestRegisterRules
{
    public const string CodeNoConnection = "welltest-register-no-connection";
    public const string CodeReadFailed = "welltest-register-read-failed";
    public const string CodeWriteFailed = "welltest-register-write-failed";
    public const string CodeActorRequired = "welltest-register-actor-required";
    public const string CodeWellRequired = "welltest-register-well-required";
    public const string CodeInputRequired = "welltest-register-input-required";
    public const string CodeTypeUnknown = "welltest-register-type-unknown";
    public const string CodeResultUnknown = "welltest-register-result-unknown";
    public const string CodeDuplicate = "welltest-register-duplicate";
    public const string CodeAnalysisLinked = "welltest-register-analysis-linked";
    public const string CodeUnknownTest = "welltest-register-unknown-test";
    public const string CodeUnitMissing = "welltest-register-unit-missing";

    /// <summary>The duration units the PPDM unit codes use, so a duration is never a bare number.</summary>
    public static IReadOnlyList<string> DurationUnits { get; } = ["h", "min", "d"];

    /// <summary>The natural key the register and an analysis share.</summary>
    public static string TestKey(string wellId, string testType, int runNumber, int testNumber) =>
        $"{wellId.Trim()}|{testType.Trim().ToUpperInvariant()}|{runNumber.ToString(CultureInfo.InvariantCulture)}|" +
        $"{testNumber.ToString(CultureInfo.InvariantCulture)}";

    public static IReadOnlyList<string> Missing(OgWellTestEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(entry.WellId))
        {
            missing.Add("well");
        }

        if (entry.TestDate is null)
        {
            missing.Add("test date");
        }

        if (string.IsNullOrWhiteSpace(entry.TestType))
        {
            missing.Add("test type");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(
        OgWellTestEntry entry,
        IReadOnlyList<string> testTypes,
        IReadOnlyList<string> resultCodes)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var missing = Missing(entry);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well the test belongs to; a test links to exactly one well.")
                : (CodeInputRequired, $"The register entry needs {string.Join(", ", missing)}.");
        }

        // AC-2: the type comes from the reference table, not from free text.
        if (!testTypes.Contains(entry.TestType!.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return (CodeTypeUnknown,
                $"'{entry.TestType}' is not a test type in R_WELL_TEST_TYPE; the seeded types are " +
                $"{(testTypes.Count == 0 ? "(none - seed the reference table first)" : string.Join(", ", testTypes))}.");
        }

        // A result code is optional (a pending test), but when it is given it comes from R_TEST_RESULT.
        if (!string.IsNullOrWhiteSpace(entry.ResultCode)
            && !resultCodes.Contains(entry.ResultCode.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return (CodeResultUnknown,
                $"'{entry.ResultCode}' is not a result code in R_TEST_RESULT; the seeded codes are " +
                $"{(resultCodes.Count == 0 ? "(none - seed the reference table first)" : string.Join(", ", resultCodes))}.");
        }

        if (entry.Duration is not null && string.IsNullOrWhiteSpace(entry.DurationUnit))
        {
            return (CodeUnitMissing, "A duration needs its unit (h, min or d), or the number means nothing.");
        }

        if (entry.DurationUnit is not null
            && !DurationUnits.Contains(entry.DurationUnit.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return (CodeUnitMissing,
                $"'{entry.DurationUnit}' is not a duration unit; use {string.Join(", ", DurationUnits)}.");
        }

        // AC-5: a rate or a pressure with a value needs its unit.
        foreach (var (value, unit, name) in new (decimal?, string?, string)[]
                 {
                     (entry.OilRate, entry.OilRateUnit, "oil rate"),
                     (entry.GasRate, entry.GasRateUnit, "gas rate"),
                     (entry.FlowingPressure, entry.FlowingPressureUnit, "flowing pressure"),
                 })
        {
            if (value is not null && string.IsNullOrWhiteSpace(unit))
            {
                return (CodeUnitMissing, $"The {name} needs its unit beside the number.");
            }
        }

        return entry.Duration is < 0m
            ? (CodeInputRequired, "A duration cannot be negative.")
            : null;
    }
}

/// <summary>WTA-02: the well test register.</summary>
public interface IOgWellTestRegister
{
    /// <summary>The registered tests with their analyses; an unavailable register is a state.</summary>
    Task<OgResult<OgWellTestRegisterWorkspace>> LoadAsync(
        string? wellId = null,
        string? registerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or edits a register entry, refusing a duplicate well+date+type (AC-3).</summary>
    Task<OgResult<OgWellTestRegisterWorkspace>> SaveAsync(
        OgWellTestEntry entry,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a test, refusing when analyses were made from it (AC-6).</summary>
    Task<OgResult<OgWellTestRegisterWorkspace>> DeleteAsync(
        string registerId,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WTA-02 over PPDM's <c>WELL_TEST</c> (the owner of the test), the new LifeCycle-owned
/// <c>WELL_TEST_REGISTER</c> (the register facts), <c>R_WELL_TEST_TYPE</c> and <c>R_TEST_RESULT</c> (the codes), and
/// WTA-01's <c>WELL_TEST_ANALYSIS_RESULT</c> (the link), through a unit of work.
/// </summary>
/// <remarks>
/// A test links to exactly one well because the register entry carries one UWI and the natural key starts with it; it may
/// link to several analyses because the link runs the other way, from each analysis's <c>TEST_ID</c>. A duplicate
/// well+date+type is refused against the register. Deleting is refused while any analysis points at the test.
/// </remarks>
public sealed class OgWellTestRegisterService : IOgWellTestRegister
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellTestRegisterService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgWellTestRegisterWorkspace>> LoadAsync(
        string? wellId = null,
        string? registerId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellTestRegisterWorkspace>.Failed(
                "No connection is open, so the register cannot be read.", WellTestRegisterRules.CodeNoConnection);
        }

        try
        {
            var register = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            var selected = string.IsNullOrWhiteSpace(registerId)
                ? register.Tests.FirstOrDefault(test => string.IsNullOrWhiteSpace(wellId)
                    || string.Equals(test.WellId, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? register.Tests.FirstOrDefault()
                : register.Tests.FirstOrDefault(test =>
                      string.Equals(test.RegisterId, registerId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? register.Tests.FirstOrDefault();

            return OgResult<OgWellTestRegisterWorkspace>.Ok(register with { Selected = selected });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the well test register",
                "the register is unavailable; new tests can still be considered",
                FailureSeverity.Degraded);

            return OgResult<OgWellTestRegisterWorkspace>.Ok(new OgWellTestRegisterWorkspace(
                [],
                null,
                [],
                [],
                WellTestRegisterRules.DurationUnits,
                false,
                true,
                $"the register could not be read (reference {reference})"));
        }
    }

    public async Task<OgResult<OgWellTestRegisterWorkspace>> SaveAsync(
        OgWellTestEntry entry,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellTestRegisterWorkspace>.Failed(
                "A register entry must record who made it.", WellTestRegisterRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellTestRegisterWorkspace>.Failed(
                "No connection is open, so the test cannot be registered.", WellTestRegisterRules.CodeNoConnection);
        }

        try
        {
            var register = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            if (WellTestRegisterRules.Validate(entry, register.TestTypes, register.ResultCodes) is { } refusal)
            {
                return OgResult<OgWellTestRegisterWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var type = Canonical(entry.TestType!, register.TestTypes);
            var result = string.IsNullOrWhiteSpace(entry.ResultCode)
                ? null
                : Canonical(entry.ResultCode!, register.ResultCodes);
            var runNumber = entry.RunNumber ?? NextRunNumber(register.Tests, entry.WellId!, entry.TestDate!.Value, type);
            var testNumber = entry.TestNumber ?? 1;
            var testKey = WellTestRegisterRules.TestKey(entry.WellId!, type, runNumber, testNumber);

            // AC-3: one test per well, date and type. An edit of the same row is not a duplicate.
            var duplicate = register.Tests.FirstOrDefault(test =>
                string.Equals(test.WellId, entry.WellId!.Trim(), StringComparison.OrdinalIgnoreCase)
                && test.TestDate.Date == entry.TestDate.Value.Date
                && string.Equals(test.TestType, type, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(test.RegisterId, entry.RegisterId, StringComparison.OrdinalIgnoreCase));

            if (duplicate is not null)
            {
                return OgResult<OgWellTestRegisterWorkspace>.Failed(
                    $"A {type} test is already registered for {entry.WellId!.Trim()} on " +
                    $"{entry.TestDate.Value:yyyy-MM-dd} ({duplicate.RegisterId}); a well cannot have two tests of the " +
                    "same type on the same date.",
                    WellTestRegisterRules.CodeDuplicate);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<WELL_TEST_REGISTER>(source);
            unit.PrimaryKey = "REGISTER_ID";

            var existing = string.IsNullOrWhiteSpace(entry.RegisterId)
                ? null
                : (await OgUow.ReadAsync(
                    unit,
                    [OgUow.Equal("REGISTER_ID", entry.RegisterId!.Trim())],
                    cancellationToken).ConfigureAwait(false)).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(entry.RegisterId) && existing is null)
            {
                return OgResult<OgWellTestRegisterWorkspace>.Failed(
                    $"Register entry '{entry.RegisterId}' is not in this database.",
                    WellTestRegisterRules.CodeUnknownTest);
            }

            var row = new WELL_TEST_REGISTER
            {
                REGISTER_ID = existing?.REGISTER_ID ?? Guid.NewGuid().ToString("N"),
                TEST_KEY = testKey,
                WELL_UWI = entry.WellId!.Trim(),
                TEST_TYPE = type,
                RUN_NUM = runNumber,
                TEST_NUM = testNumber,
                TEST_DATE = entry.TestDate.Value.Date,
                DURATION = entry.Duration,
                DURATION_OUOM = Blank(entry.DurationUnit),
                RESULT_CODE = result,
                PRODUCTION_METHOD = Blank(entry.ProductionMethod),
                REMARKS = Blank(entry.Remarks),
                OIL_RATE = entry.OilRate,
                OIL_RATE_OUOM = Blank(entry.OilRateUnit),
                GAS_RATE = entry.GasRate,
                GAS_RATE_OUOM = Blank(entry.GasRateUnit),
                FLOWING_PRESSURE = entry.FlowingPressure,
                FLOWING_PRESSURE_OUOM = Blank(entry.FlowingPressureUnit),
                ACTOR = actor.Trim(),
                RECORDED_AT = existing?.RECORDED_AT ?? now,
                ROW_CREATED_BY = existing?.ROW_CREATED_BY ?? actor.Trim(),
                ROW_CREATED_DATE = existing?.ROW_CREATED_DATE ?? now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            };

            // Add returns void and Update returns the unit's error record, so they cannot share one expression.
            if (existing is null)
            {
                unit.Add(row);
            }
            else
            {
                var written = unit.Update(row);

                if (!OgUow.IsOk(written))
                {
                    return OgResult<OgWellTestRegisterWorkspace>.Failed(
                        OgUow.Message(written, "The test was not registered."),
                        WellTestRegisterRules.CodeWriteFailed);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWellTestRegisterWorkspace>.Failed(
                    OgUow.Message(committed, "The test was not registered."), WellTestRegisterRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWellTestRegisterWorkspace>.Ok(after with
            {
                Selected = after.Tests.FirstOrDefault(test => test.RegisterId == row.REGISTER_ID),
            });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"registering a well test for {entry.WellId}",
                "the test was not registered",
                FailureSeverity.Error);

            return OgResult<OgWellTestRegisterWorkspace>.Failed(
                "The test could not be registered.", reference ?? WellTestRegisterRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellTestRegisterWorkspace>> DeleteAsync(
        string registerId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellTestRegisterWorkspace>.Failed(
                "A deletion must record who made it.", WellTestRegisterRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellTestRegisterWorkspace>.Failed(
                "No connection is open, so the test cannot be deleted.", WellTestRegisterRules.CodeNoConnection);
        }

        try
        {
            var register = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            var test = register.Tests.FirstOrDefault(candidate =>
                string.Equals(candidate.RegisterId, registerId?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (test is null)
            {
                return OgResult<OgWellTestRegisterWorkspace>.Failed(
                    $"Register entry '{registerId}' is not in this database.", WellTestRegisterRules.CodeUnknownTest);
            }

            // AC-6: an analysis points at the test through its own TEST_ID, so deleting the test would orphan it.
            if (test.HasAnalysis)
            {
                return OgResult<OgWellTestRegisterWorkspace>.Failed(
                    $"This test has {test.AnalysisCount.ToString(CultureInfo.InvariantCulture)} analysis(es) linked to " +
                    "it, so it cannot be deleted: remove or re-link them first.",
                    WellTestRegisterRules.CodeAnalysisLinked);
            }

            var unit = OgUow.For<WELL_TEST_REGISTER>(source);
            unit.PrimaryKey = "REGISTER_ID";

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal("REGISTER_ID", test.RegisterId)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgWellTestRegisterWorkspace>.Failed(
                    $"Register entry '{registerId}' is not in this database.", WellTestRegisterRules.CodeUnknownTest);
            }

            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var deleted = unit.Delete(row);

            if (!OgUow.IsOk(deleted))
            {
                return OgResult<OgWellTestRegisterWorkspace>.Failed(
                    OgUow.Message(deleted, "The test was not deleted."), WellTestRegisterRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWellTestRegisterWorkspace>.Failed(
                    OgUow.Message(committed, "The test was not deleted."), WellTestRegisterRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWellTestRegisterWorkspace>.Ok(after with { Selected = after.Tests.FirstOrDefault() });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"deleting well test {registerId}",
                "the test was not deleted",
                FailureSeverity.Error);

            return OgResult<OgWellTestRegisterWorkspace>.Failed(
                "The test could not be deleted.", reference ?? WellTestRegisterRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<OgWellTestRegisterWorkspace> ReadAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var register = await OgUow.ReadAllAsync<WELL_TEST_REGISTER>(source).ConfigureAwait(false);
        var analyses = await OgUow.ReadAllAsync<WELL_TEST_ANALYSIS_RESULT>(source).ConfigureAwait(false);
        var types = await OgUow.ReadAllAsync<R_WELL_TEST_TYPE>(source).ConfigureAwait(false);
        var results = await OgUow.ReadAllAsync<R_TEST_RESULT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var testTypes = types
            .Where(row => !string.IsNullOrWhiteSpace(row.WELL_TEST_TYPE))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Select(row => row.WELL_TEST_TYPE!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var resultCodes = results
            .Where(row => !string.IsNullOrWhiteSpace(row.TEST_RESULT))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Select(row => row.TEST_RESULT!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var typeNames = types
            .Where(row => !string.IsNullOrWhiteSpace(row.WELL_TEST_TYPE))
            .GroupBy(row => row.WELL_TEST_TYPE!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().LONG_NAME, StringComparer.OrdinalIgnoreCase);

        var resultNames = results
            .Where(row => !string.IsNullOrWhiteSpace(row.TEST_RESULT))
            .GroupBy(row => row.TEST_RESULT!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().LONG_NAME, StringComparer.OrdinalIgnoreCase);

        // The link: an analysis belongs to one test through its own TEST_ID.
        var byTest = analyses
            .Where(row => !string.IsNullOrWhiteSpace(row.TEST_ID))
            .GroupBy(row => row.TEST_ID!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        var tests = register
            .Where(row => !string.IsNullOrWhiteSpace(row.REGISTER_ID))
            .OrderByDescending(row => row.TEST_DATE)
            .ThenBy(row => row.WELL_UWI, StringComparer.OrdinalIgnoreCase)
            .Select(row => new OgWellTestSummary(
                row.REGISTER_ID,
                row.TEST_KEY,
                Blank(row.WELL_UWI),
                row.TEST_DATE,
                row.TEST_TYPE,
                typeNames.TryGetValue(row.TEST_TYPE, out var typeName) ? Blank(typeName) : null,
                row.DURATION,
                Blank(row.DURATION_OUOM),
                Blank(row.RESULT_CODE),
                row.RESULT_CODE is null
                    ? null
                    : resultNames.TryGetValue(row.RESULT_CODE, out var resultName) ? Blank(resultName) : null,
                Blank(row.PRODUCTION_METHOD),
                Blank(row.REMARKS),
                row.OIL_RATE,
                Blank(row.OIL_RATE_OUOM),
                row.GAS_RATE,
                Blank(row.GAS_RATE_OUOM),
                row.FLOWING_PRESSURE,
                Blank(row.FLOWING_PRESSURE_OUOM),
                byTest.TryGetValue(row.TEST_KEY, out var count) ? count : 0,
                Blank(row.ACTOR),
                row.RECORDED_AT))
            .Take(500)
            .ToList();

        return new OgWellTestRegisterWorkspace(
            tests,
            null,
            testTypes,
            resultCodes,
            WellTestRegisterRules.DurationUnits,
            testTypes.Count > 0 && resultCodes.Count > 0,
            false,
            null);
    }

    private static int NextRunNumber(
        IReadOnlyList<OgWellTestSummary> tests,
        string wellId,
        DateTime date,
        string type)
    {
        var same = tests
            .Where(test => string.Equals(test.WellId, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(test => test.TestDate.Date == date.Date)
            .Where(test => string.Equals(test.TestType, type, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return same.Count + 1;
    }

    private static string Canonical(string value, IReadOnlyList<string> candidates) =>
        candidates.FirstOrDefault(candidate =>
            string.Equals(candidate, value.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? value.Trim();

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
