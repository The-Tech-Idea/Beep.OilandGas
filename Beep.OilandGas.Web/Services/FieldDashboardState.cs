using System.Globalization;
using System.Net;
using System.Text.Json;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services;

public sealed class FieldDashboardState(Func<Task<FieldDashboard>> load) : IDisposable
{
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
        catch (Exception ex)
        {
            if (Current(version)) Error = ex is HttpRequestException http ? http.StatusCode switch {
                HttpStatusCode.Unauthorized => "Your session has expired. Sign in again to load the dashboard.",
                HttpStatusCode.Forbidden => "You do not have access to this field dashboard. Select another field or contact your app administrator.",
                HttpStatusCode.NotFound => "The selected field could not be found. Select another field and retry.",
                _ => "Field data could not be loaded. Retry when the service is available."
            } : "Field data could not be confirmed for the selected field. Refresh to retry.";
        }
        finally { if (Current(version)) Loading = false; }
    }
    private bool Current(int version) => !_disposed && version == _version;
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
