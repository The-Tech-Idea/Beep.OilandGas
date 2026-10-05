namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

/// <summary>
/// Reads a column's value off a PPDM row the work order services loaded, by asking its type.
/// </summary>
/// <remarks>
/// OILGAS-CATCH-01. Each work order service had its own copy of these readers, which cast the value and caught the cast's
/// failure to answer empty — the same answer as an absent value, so the catch hid nothing it needed to. One copy now, and it
/// asks rather than catches.
/// </remarks>
internal static class WorkOrderRowValues
{
    /// <summary>The column's text; empty when it is absent or not text.</summary>
    public static string Str(object row, string column) =>
        row.GetType().GetProperty(column)?.GetValue(row) as string ?? string.Empty;

    /// <summary>The column's whole number; 0 when it is absent or not a whole number.</summary>
    public static int Int(object row, string column)
    {
        var value = row.GetType().GetProperty(column)?.GetValue(row);
        return value is int i ? i : value is long l ? (int)l : 0;
    }

    /// <summary>The column's date; null when it is absent or not a date.</summary>
    public static DateTime? Date(object row, string column)
    {
        var value = row.GetType().GetProperty(column)?.GetValue(row);
        if (value is DateTime dt) return dt;
        if (value is string s && DateTime.TryParse(s, out var parsed)) return parsed;
        return null;
    }

    /// <summary>The column's number; 0 when it is absent or not a number.</summary>
    public static decimal Decimal(object row, string column)
    {
        var value = row.GetType().GetProperty(column)?.GetValue(row);
        return value switch
        {
            decimal m => m,
            double d => (decimal)d,
            float f => (decimal)f,
            int i => i,
            long l => l,
            _ => 0m
        };
    }
}
