using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    /// <summary>
    /// Request for generating production forecast
    /// </summary>
    public class GenerateForecastRequest
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier) - optional if FieldId is provided
        /// </summary>
        public string? WellUWI { get; set; }

        /// <summary>
        /// Field identifier - optional if WellUWI is provided
        /// </summary>
        public string? FieldId { get; set; }

        /// <summary>
        /// Forecast method (e.g., "DCA", "ARPS", "HYP")
        /// </summary>
        [Required(ErrorMessage = "ForecastMethod is required")]
        public string ForecastMethod { get; set; } = string.Empty;

        /// <summary>
        /// Forecast period in months
        /// </summary>
        [Required]
        [Range(1, 600, ErrorMessage = "ForecastPeriod must be between 1 and 600 months")]
        public int ForecastPeriod { get; set; }
    }

    /// <summary>
    /// Request for decline curve analysis
    /// </summary>
    public class DeclineCurveAnalysisRequest
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Start date for analysis period
        /// </summary>
        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for analysis period
        /// </summary>
        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate { get; set; }
    }
}
