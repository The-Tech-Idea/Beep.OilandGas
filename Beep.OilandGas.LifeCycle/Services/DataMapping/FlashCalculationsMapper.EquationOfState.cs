using Beep.OilandGas.FlashCalculations.Constants;

namespace Beep.OilandGas.LifeCycle.Services.DataMapping;

/// <summary>
/// EOS wire string normalization for LifeCycle orchestration (aligned with <c>R_FLASH_CALCULATION_REFERENCE_CODE</c> / <c>FLASH_EOS_MODEL</c>).
/// </summary>
public partial class FlashCalculationsMapper
{
    /// <summary>
    /// Maps <see cref="Beep.OilandGas.Models.Data.Calculations.FlashCalculationOptions.EquationOfState"/> vocabulary to canonical LOV codes.
    /// </summary>
    public static string MapEquationOfStateToReferenceCode(string? equationOfState) =>
        FlashEquationOfStateMapping.ToReferenceCode(equationOfState);
}
