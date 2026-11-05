using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Publishes only recorded orders whose current FIELD and individual order access were confirmed.</summary>
public static class CompanyWorkOrderListReview
{
    public static async Task<List<WorkOrderSummary>> ReadAsync(string field, Func<Task<List<WorkOrderSummary>>> read,
        Func<string, Task<bool>> canRead, Func<Task> guard, CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentNullException.ThrowIfNull(read); ArgumentNullException.ThrowIfNull(canRead); ArgumentNullException.ThrowIfNull(guard);
        async Task<WorkOrderSummary[]> Capture()
        {
            token.ThrowIfCancellationRequested(); await guard();
            var rows = await read(); await guard();
            if (rows is null || rows.Any(row => row is null || row.FieldId != field || row.Source != "WORK_ORDER" ||
                string.IsNullOrWhiteSpace(row.InstanceId) || row.Components is null) ||
                rows.Select(row => row.InstanceId).Distinct(StringComparer.Ordinal).Count() != rows.Count)
                throw RefusalException.Conflict("The recorded work-order list is inconsistent with the selected FIELD.");
            return rows.OrderBy(row => row.InstanceId, StringComparer.Ordinal)
                .Select(row => JsonSerializer.Deserialize<WorkOrderSummary>(JsonSerializer.Serialize(row))!).ToArray();
        }
        var first = await Capture();
        var firstAccess = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var row in first) { token.ThrowIfCancellationRequested(); await guard(); firstAccess.Add(row.InstanceId, await canRead(row.InstanceId)); }
        var second = await Capture();
        if (JsonSerializer.Serialize(first) != JsonSerializer.Serialize(second))
            throw RefusalException.Conflict("The recorded work-order list changed. Reload the FIELD worklist.");
        var visible = new List<WorkOrderSummary>();
        foreach (var row in second)
        {
            token.ThrowIfCancellationRequested(); await guard();
            var allowed = await canRead(row.InstanceId);
            if (allowed != firstAccess[row.InstanceId]) throw RefusalException.Conflict("Work-order access changed. Reload the FIELD worklist.");
            if (allowed) visible.Add(row);
        }
        await guard();
        return visible;
    }
}
