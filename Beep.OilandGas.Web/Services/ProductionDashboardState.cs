using System.Net;
using Beep.OilandGas.Models.Data.Production;

namespace Beep.OilandGas.Web.Services;

public sealed class ProductionDashboardState(
    Func<string, Task<ProductionDashboardResponse>> load) : IDisposable
{
    private int _version;
    private bool _disposed;
    public ProductionDashboardSummary? Summary { get; private set; }
    public IReadOnlyList<ProductionWellStatusDto> Wells { get; private set; } = Array.Empty<ProductionWellStatusDto>();
    public bool Loading { get; private set; }
    public bool NeedsField { get; private set; }
    public string? Error { get; private set; }

    public async Task LoadAsync(Func<Task<string?>> currentField)
    {
        if (_disposed) return;
        var version = ++_version;
        Summary = null; Wells = Array.Empty<ProductionWellStatusDto>(); Error = null; NeedsField = false; Loading = true;
        try
        {
            var fieldId = await currentField();
            if (!Current(version)) return;
            if (string.IsNullOrWhiteSpace(fieldId)) { NeedsField = true; return; }
            var response = await load(fieldId);
            var result = response?.Summary;
            if (!Current(version)) return;
            if (result is null || result.FieldId != fieldId) throw new InvalidOperationException();
            var rows = response?.Wells;
            if (!Current(version)) return;
            if (rows is null || rows.Any(w => string.IsNullOrWhiteSpace(w.WellId)) || rows.Select(w => w.WellId).Distinct().Count() != rows.Count)
                throw new InvalidOperationException();
            if (!Current(version)) return;
            Summary = result; Wells = rows;
        }
        catch (Exception ex)
        {
            if (Current(version)) Error = ex is HttpRequestException http ? http.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Your session has expired. Sign in again to load production data.",
                HttpStatusCode.Forbidden => "You do not have access to this field's production data.",
                _ => "Production data could not be loaded. Refresh to retry."
            } : "Production data could not be confirmed for this field. Refresh to retry.";
        }
        finally { if (Current(version)) Loading = false; }
    }
    private bool Current(int version) => !_disposed && version == _version;
    public void Dispose() { _disposed = true; ++_version; Summary = null; Wells = Array.Empty<ProductionWellStatusDto>(); }
}
