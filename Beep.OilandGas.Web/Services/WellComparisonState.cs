using System.Net;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services;

public sealed class WellComparisonState(Func<CompareWellsRequest, Task<WellComparisonData?>> compare) : IDisposable
{
    private int _version;
    private bool _disposed;
    public WellComparisonData? Result { get; private set; }
    public IReadOnlyList<string> RequestedWells { get; private set; } = Array.Empty<string>();
    public bool Loading { get; private set; }
    public string? Error { get; private set; }

    public void Reset()
    {
        ++_version; Result = null; RequestedWells = Array.Empty<string>(); Loading = false; Error = null;
    }

    public async Task CompareAsync(params string[] inputs)
    {
        if (_disposed) return;
        Reset();
        var version = _version;
        var wells = inputs.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        if (wells.Length < 2 || wells.Distinct(StringComparer.Ordinal).Count() != wells.Length)
        {
            Error = "Enter at least two different well UWIs.";
            return;
        }
        Loading = true;
        try
        {
            var result = await compare(new CompareWellsRequest { WellIdentifiers = wells.ToList() });
            if (_disposed || version != _version) return;
            if (result?.Wells is null || result.ComparisonFields is null ||
                result.Wells.Count != wells.Length ||
                !result.Wells.Select(w => w.WellIdentifier).OrderBy(x => x, StringComparer.Ordinal)
                    .SequenceEqual(wells.OrderBy(x => x, StringComparer.Ordinal)))
                throw new InvalidOperationException("Incomplete or mismatched well comparison.");
            Result = result;
            RequestedWells = Array.AsReadOnly(wells);
        }
        catch (Exception ex)
        {
            if (!_disposed && version == _version)
                Error = ex is HttpRequestException http ? http.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Your session has expired. Sign in again to compare wells.",
                    HttpStatusCode.Forbidden => "You do not have access to compare these wells.",
                    _ => "The comparison could not be loaded. Retry when the service is available."
                } : "The comparison could not be confirmed for all requested wells. Check the UWIs and retry.";
        }
        finally { if (!_disposed && version == _version) Loading = false; }
    }

    public void Dispose() { _disposed = true; Reset(); }
}
