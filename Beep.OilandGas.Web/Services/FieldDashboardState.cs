using System.Globalization;
using System.Net;
using System.Text.Json;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services;

public sealed class FieldDashboardState(Func<Task<FieldDashboard>> load, OilGasCallFailures failures) : IDisposable
{
    private const string Operation = "loading the field dashboard";

    public FieldDashboard? Dashboard { get; private set; }
    public bool Loading { get; private set; }
    public bool NeedsField { get; private set; }
    public string? Error { get; private set; }
    private int _version;
    private bool _disposed;

    public async Task LoadAsync(Func<Task<string?>> currentField)
    {
        if (_disposed) return;
        var version = ++_version;
        Dashboard = null; Error = null; NeedsField = false; Loading = true;
        try
        {
            var field = await currentField();
            if (!Current(version)) return;
            if (string.IsNullOrWhiteSpace(field)) { NeedsField = true; return; }
            var dashboard = await load();
            if (!Current(version)) return;
            if (dashboard is null || dashboard.FieldId != field)
                throw new InvalidOperationException("Dashboard field does not match the selected field.");
            Dashboard = dashboard;
        }
        // The page renders a sentence for every failure of this load, in place of the dashboard; the store keeps the
        // exception (a superseded load's too, though nobody is shown it).
        catch (Exception ex)
        {
            var sentence = Refusal(ex) is { } own
                ? failures.Told(ex, Operation, own)
                : failures.Explain(ex, Operation, OilGasCallFailures.IsCallFailure(ex)
                    ? "Field data could not be loaded"
                    : "Field data could not be confirmed for the selected field");
            if (Current(version)) Error = sentence;
        }
        finally { if (Current(version)) Loading = false; }
    }
    private bool Current(int version) => !_disposed && version == _version;

    /// <summary>This page's own words for a refusal it recognises; null for anything else.</summary>
    private static string? Refusal(Exception ex) => ex switch
    {
        OilGasApiException { StatusCode: HttpStatusCode.Unauthorized } => "Your session has expired. Sign in again to load the dashboard.",
        OilGasApiException { StatusCode: HttpStatusCode.Forbidden } => "You do not have access to this field dashboard. Select another field or contact your app administrator.",
        OilGasApiException { StatusCode: HttpStatusCode.NotFound } => "The selected field could not be found. Select another field and retry.",
        _ => null,
    };
    public void Dispose() { _disposed = true; ++_version; Dashboard = null; }

    public static string MetricValue(object? value) => value switch {
        null => "Not available",
        JsonElement { ValueKind: JsonValueKind.Number } json when json.TryGetDecimal(out var number) => number.ToString("N2", CultureInfo.CurrentCulture),
        JsonElement { ValueKind: JsonValueKind.String } json => json.GetString() ?? "Not available",
        JsonElement => "Not available",
        decimal number => number.ToString("N2", CultureInfo.CurrentCulture),
        double number when double.IsFinite(number) => number.ToString("N2", CultureInfo.CurrentCulture),
        int number => number.ToString("N0", CultureInfo.CurrentCulture),
        long number => number.ToString("N0", CultureInfo.CurrentCulture),
        string text when !string.IsNullOrWhiteSpace(text) => text,
        _ => "Not available"
    };
}
