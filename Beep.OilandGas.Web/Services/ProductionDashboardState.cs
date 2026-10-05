using System.Net;
using Beep.OilandGas.Models.Data.Production;

namespace Beep.OilandGas.Web.Services;

public sealed class ProductionDashboardState(
    Func<string, Task<ProductionDashboardResponse>> load, OilGasCallFailures failures) : IDisposable
{
    private const string Operation = "loading the production dashboard";

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
        // The page renders a sentence for every failure of this load, in place of the figures; the store keeps the
        // exception (a superseded load's too, though nobody is shown it).
        catch (Exception ex)
        {
            var sentence = Refusal(ex) is { } own
                ? failures.Told(ex, Operation, own)
                : failures.Explain(ex, Operation, OilGasCallFailures.IsCallFailure(ex)
                    ? "Production data could not be loaded"
                    : "Production data could not be confirmed for this field");
            if (Current(version)) Error = sentence;
        }
        finally { if (Current(version)) Loading = false; }
    }
    private bool Current(int version) => !_disposed && version == _version;

    /// <summary>This page's own words for a refusal it recognises; null for anything else.</summary>
    private static string? Refusal(Exception ex) => ex switch
    {
        OilGasApiException { StatusCode: HttpStatusCode.Unauthorized } => "Your session has expired. Sign in again to load production data.",
        OilGasApiException { StatusCode: HttpStatusCode.Forbidden } => "You do not have access to this field's production data.",
        _ => null,
    };
    public void Dispose() { _disposed = true; ++_version; Summary = null; Wells = Array.Empty<ProductionWellStatusDto>(); }
}
