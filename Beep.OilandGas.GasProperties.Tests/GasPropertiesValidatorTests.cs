using Beep.OilandGas.GasProperties.Exceptions;
using Beep.OilandGas.GasProperties.Validation;
using Beep.OilandGas.Models.Data.GasProperties;
using Xunit;

namespace Beep.OilandGas.GasProperties.Tests;

public class GasPropertiesValidatorTests
{
    [Fact]
    public void ValidatePressure_throws_below_minimum()
    {
        Assert.Throws<ParameterOutOfRangeException>(() =>
            GasPropertiesValidator.ValidatePressure(0.05m));
    }

    [Fact]
    public void ValidateGasComposition_throws_when_fractions_invalid()
    {
        var bad = new GasComposition { MethaneFraction = 0.5m, EthaneFraction = 0.2m };
        Assert.Throws<InvalidGasCompositionException>(() =>
            GasPropertiesValidator.ValidateGasComposition(bad));
    }

    [Fact]
    public void GetPseudoReducedPropertiesFromSpecificGravity_typical_dry_gas()
    {
        var (pr, tr) = GasPropertiesValidator.GetPseudoReducedPropertiesFromSpecificGravity(800m, 580m, 0.65m);
        Assert.InRange(pr, 1.0m, 5.0m);
        Assert.InRange(tr, 1.0m, 3.0m);
    }

    [Fact]
    public void GetApplicabilityWarnings_null_composition_includes_gamma_only_message()
    {
        var w = GasPropertiesValidator.GetApplicabilityWarnings(800m, 580m, 0.65m, composition: null);
        Assert.Contains(w, x => x.Contains("specific-gravity", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Scenario", "Facility")]
    public void GetApplicabilityWarnings_high_pr_includes_uncertainty_message()
    {
        // Heavy γg and high discharge pressure → high pseudo-reduced pressure (typical HP facility / compression context).
        var w = GasPropertiesValidator.GetApplicabilityWarnings(15000m, 620m, 1.45m, composition: null);
        Assert.Contains(w, x => x.Contains("Pseudo-reduced pressure is high", StringComparison.Ordinal));
    }

    [Fact]
    public void GetApplicabilityWarnings_sour_composition_includes_h2s_message()
    {
        var sour = new GasComposition
        {
            MethaneFraction = 0.94m,
            HydrogenSulfideFraction = 0.06m
        };

        var w = GasPropertiesValidator.GetApplicabilityWarnings(800m, 580m, 0.65m, sour);
        Assert.Contains(w, x => x.Contains("Hydrogen sulfide", StringComparison.Ordinal));
    }

    [Fact]
    public void GetApplicabilityWarnings_valid_composition_does_not_add_gamma_only_message()
    {
        var lean = new GasComposition { MethaneFraction = 1.0m };
        var w = GasPropertiesValidator.GetApplicabilityWarnings(800m, 580m, 0.65m, lean);
        Assert.DoesNotContain(w, x => x.Contains("specific-gravity", StringComparison.OrdinalIgnoreCase));
    }
}
