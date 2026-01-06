namespace Beep.OilandGas.Models.WellTestAnalysis
{
    /// <summary>
    /// Represents the result of a well test analysis
    /// DTO for calculations - Entity class: WELL_TEST_ANALYSIS_RESULT
    /// </summary>
    public class WellTestAnalysisResult
    {
        /// <summary>
        /// Gets or sets the calculated permeability in md
        /// </summary>
        public double Permeability { get; set; }

        /// <summary>
        /// Gets or sets the skin factor (dimensionless)
        /// </summary>
        public double SkinFactor { get; set; }

        /// <summary>
        /// Gets or sets the reservoir pressure in psi
        /// </summary>
        public double ReservoirPressure { get; set; }

        /// <summary>
        /// Gets or sets the productivity index in BPD/psi
        /// </summary>
        public double ProductivityIndex { get; set; }

        /// <summary>
        /// Gets or sets the flow efficiency (dimensionless)
        /// </summary>
        public double FlowEfficiency { get; set; }

        /// <summary>
        /// Gets or sets the damage ratio (dimensionless)
        /// </summary>
        public double DamageRatio { get; set; }

        /// <summary>
        /// Gets or sets the radius of investigation in feet
        /// </summary>
        public double RadiusOfInvestigation { get; set; }

        /// <summary>
        /// Gets or sets the identified reservoir model
        /// </summary>
        public ReservoirModel IdentifiedModel { get; set; } = ReservoirModel.InfiniteActing;

        /// <summary>
        /// Gets or sets the R-squared value for the analysis
        /// </summary>
        public double RSquared { get; set; }

        /// <summary>
        /// Gets or sets the analysis method used
        /// </summary>
        public string AnalysisMethod { get; set; } = string.Empty;
    }
}



