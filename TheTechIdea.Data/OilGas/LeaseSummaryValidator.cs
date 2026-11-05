using Beep.OilandGas.Models.Data.Lease;

namespace TheTechIdea.Data.OilGas;

// Recorded-fact integrity shared by the SDK and experience state; no legal or financial interpretation.
public static class LeaseSummaryValidator
{
    public static void RequireValid(LeaseSummary? row, string? field = null, string? id = null)
    {
        if (row is null || !Identifier(row.LeaseId) || !Identifier(row.FieldId) || !Identifier(row.LandRightSubtype) ||
            field is not null && row.FieldId != field || id is not null && row.LeaseId != id ||
            !Zone(row.DateZoneId) || row.Status is null || row.Status != "" && !Identifier(row.Status) ||
            row.WorkingInterest is not null || row.PrimaryTermMonths is <= 0)
            throw new InvalidOperationException("Lease evidence is missing, inconsistent or belongs to another FIELD.");
    }
    public static void RequireValid(IReadOnlyList<LeaseSummary>? rows, string field)
    {
        if (!Identifier(field) || rows is null) throw new InvalidOperationException("An explicit FIELD and lease response are required.");
        foreach (var row in rows) RequireValid(row, field);
        if (rows.Select(row => (row.LandRightSubtype, row.LeaseId)).Distinct().Count() != rows.Count)
            throw new InvalidOperationException("The lease response has duplicate complete identities.");
    }
    public static LeaseSummary Snapshot(LeaseSummary row) => new()
    {
        LeaseId = row.LeaseId, LandRightSubtype = row.LandRightSubtype, FieldId = row.FieldId, DateZoneId = row.DateZoneId,
        LeaseNumber = row.LeaseNumber, LeaseName = row.LeaseName, LeaseType = row.LeaseType, Status = row.Status,
        EffectiveDate = row.EffectiveDate, ExpirationDate = row.ExpirationDate, PrimaryTermMonths = row.PrimaryTermMonths,
        CalculatedInterest = row.CalculatedInterest, WorkingInterest = row.WorkingInterest
    };
    public static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value == value.Trim() && !value.Any(char.IsControl);
    private static bool Zone(string? id)
    { try { return Identifier(id) && TimeZoneInfo.FindSystemTimeZoneById(id!) is not null; } catch (TimeZoneNotFoundException) { return false; } catch (InvalidTimeZoneException) { return false; } }
}
