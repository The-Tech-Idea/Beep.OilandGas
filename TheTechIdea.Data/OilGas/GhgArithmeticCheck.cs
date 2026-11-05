namespace TheTechIdea.Data.OilGas;

/// <summary>An arithmetic check for one recorded volume and factor; it is not an approved GHG report.</summary>
public sealed record GhgArithmeticCheck(GhgDispositionFact Disposition, GhgFactorDefinition Factor, decimal Result, string OutputUnit);
public sealed record GhgCalculationBasis(FieldSelectionReview Field, CompanyGhgSelection Selection, GhgFactorCatalog Catalog);
public sealed record GhgArithmeticRequest(GhgDispositionQuery Query, string ExpectedDispositionRevision, string ExpectedSelectionRevision,
    string VolumeMethod, string ActivityType, string ProductType, string PeriodType, decimal Observation, decimal Amendment, string FactorId);
public sealed record FieldGhgArithmeticCheck(FieldSelectionReview Field, GhgArithmeticRequest Request, string CatalogRevision,
    GhgArithmeticCheck Check);

public static class GhgArithmetic
{
    public static GhgArithmeticCheck Check(GhgDispositionFact disposition, GhgFactorDefinition factor)
    {
        ArgumentNullException.ThrowIfNull(disposition); ArgumentNullException.ThrowIfNull(factor);
        if (string.IsNullOrWhiteSpace(disposition.ActivityType) || string.IsNullOrWhiteSpace(disposition.Unit) ||
            disposition.ActivityType != factor.SourceCategory || disposition.Unit != factor.InputUnit ||
            string.IsNullOrWhiteSpace(factor.Id) || string.IsNullOrWhiteSpace(factor.SourceReference) || string.IsNullOrWhiteSpace(factor.OutputUnit) ||
            factor.EffectiveFrom == default || factor.EffectiveThrough < factor.EffectiveFrom || factor.Factor < 0 || disposition.Volume < 0 ||
            disposition.StartDate is null || disposition.EndDate is null)
            throw new InvalidOperationException("The recorded activity, units or dates do not match the selected factor.");
        var start = DateOnly.FromDateTime(disposition.StartDate.Value);
        var end = DateOnly.FromDateTime(disposition.EndDate.Value);
        if (start > end || start < factor.EffectiveFrom || end > factor.EffectiveThrough)
            throw new InvalidOperationException("The recorded activity dates fall outside the selected factor period.");
        try { return new(disposition, factor, checked(disposition.Volume * factor.Factor), factor.OutputUnit); }
        catch (OverflowException error) { throw new InvalidOperationException("The recorded value and factor exceed the supported calculation range.", error); }
    }
}
