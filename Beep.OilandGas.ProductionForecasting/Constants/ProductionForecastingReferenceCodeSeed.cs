using System.Collections.Generic;
using System.Text;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.ProductionForecasting.Constants;

/// <summary>
/// Seed rows for <see cref="Beep.OilandGas.Models.Data.ProductionForecasting.R_PRODUCTION_FORECASTING_REFERENCE_CODE"/>.
/// </summary>
public static class ProductionForecastingReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        foreach (var name in System.Enum.GetNames(typeof(ForecastType)))
        {
            yield return new SeedRow(
                ProductionForecastingReferenceSets.ForecastMethod,
                name,
                SpacedPascal(name));
        }

        yield return new(ProductionForecastingReferenceSets.ForecastRunStatus, ProductionForecastingReferenceCodes.RunStatusGenerated, "Forecast generated");
        yield return new(ProductionForecastingReferenceSets.ForecastRunStatus, ProductionForecastingReferenceCodes.RunStatusOk, "Analysis completed OK");
        yield return new(ProductionForecastingReferenceSets.ForecastRunStatus, ProductionForecastingReferenceCodes.RunStatusActive, "Active");
        yield return new(ProductionForecastingReferenceSets.ForecastRunStatus, ProductionForecastingReferenceCodes.RunStatusInactive, "Inactive");

        yield return new(ProductionForecastingReferenceSets.ForecastRiskRating, ProductionForecastingReferenceCodes.RiskUnknown, "Unknown risk rating");
    }

    private static string SpacedPascal(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (i > 0 && char.IsUpper(c))
                sb.Append(' ');
            sb.Append(c);
        }
        return sb.ToString();
    }
}
