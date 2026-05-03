namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    /// <summary>
    /// Optional calculation/API wiring not represented as columns on the welltestanalysis <c>WELL_TEST_DATA</c> table.
    /// Used when mapping from PPDM <c>WELL_TEST</c> or when clients must load
    /// <c>WELL_TEST_PRESS_MEAS</c> by <c>TEST_NUM</c> without embedding manual <see cref="Time"/>/<see cref="Pressure"/> points.
    /// </summary>
    public partial class WELL_TEST_DATA
    {
        /// <summary>PPDM <c>WELL.UWI</c> / <c>WELL_TEST.UWI</c> for calculation APIs — do not store the UWI in <see cref="AREA_ID"/>; that column is for PPDM <c>AREA</c> linkage.</summary>
        public string? CalculationWellUwi { get; set; }

        /// <summary>PPDM <c>WELL_TEST.TEST_NUM</c> for stored pressure series; must not be confused with <see cref="WELL_TEST_DATA_ID"/>.</summary>
        public string? CalculationPpdmTestNumber { get; set; }

        /// <summary>Optional PPDM field id (e.g. <c>WELL.ASSIGNED_FIELD</c>) for scoping stored <c>WELL_TEST_ANALYSIS_RESULT.FIELD_ID</c>.</summary>
        public string? CalculationFieldId { get; set; }
    }
}
