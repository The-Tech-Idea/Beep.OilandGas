using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
    /// <summary>
    /// One saved gas-property calculation (RES-01): the inputs as they were entered, the inputs the engine actually used,
    /// the correlations chosen, the results, and who saved it.
    /// </summary>
    /// <remarks>
    /// The engine works in US customary units, so the run stores both what the engineer typed (with its unit) and the
    /// converted psia/Rankine values it was actually run with. That pair is what makes a saved result reproducible: the
    /// conversion is recorded rather than re-derived from a factor that might later change.
    /// </remarks>
    public class GAS_PROPERTY_RUN : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string RUN_ID
        {
            get => RunIdValue;
            set => SetProperty(ref RunIdValue, value);
        }

        private string RunIdValue = string.Empty;

        /// <summary>OK or FAILED. A failed calculation is stored as a failure, never as a number (AC-3).</summary>
        [MaxLength(16)]
        public string STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string StatusValue = "OK";

        /// <summary>The pressure the engineer typed.</summary>
        public decimal? PRESSURE_INPUT
        {
            get => PressureInputValue;
            set => SetProperty(ref PressureInputValue, value);
        }

        private decimal? PressureInputValue;

        [MaxLength(16)]
        public string? PRESSURE_UNIT
        {
            get => PressureUnitValue;
            set => SetProperty(ref PressureUnitValue, value);
        }

        private string? PressureUnitValue;

        /// <summary>The pressure the engine ran with, in psia - the conversion of <see cref="PRESSURE_INPUT"/>.</summary>
        public decimal? PRESSURE_PSIA
        {
            get => PressurePsiaValue;
            set => SetProperty(ref PressurePsiaValue, value);
        }

        private decimal? PressurePsiaValue;

        public decimal? TEMPERATURE_INPUT
        {
            get => TemperatureInputValue;
            set => SetProperty(ref TemperatureInputValue, value);
        }

        private decimal? TemperatureInputValue;

        [MaxLength(16)]
        public string? TEMPERATURE_UNIT
        {
            get => TemperatureUnitValue;
            set => SetProperty(ref TemperatureUnitValue, value);
        }

        private string? TemperatureUnitValue;

        /// <summary>The temperature the engine ran with, in Rankine.</summary>
        public decimal? TEMPERATURE_R
        {
            get => TemperatureRankineValue;
            set => SetProperty(ref TemperatureRankineValue, value);
        }

        private decimal? TemperatureRankineValue;

        public decimal? SPECIFIC_GRAVITY
        {
            get => SpecificGravityValue;
            set => SetProperty(ref SpecificGravityValue, value);
        }

        private decimal? SpecificGravityValue;

        [MaxLength(64)]
        public string? Z_CORRELATION
        {
            get => ZCorrelationValue;
            set => SetProperty(ref ZCorrelationValue, value);
        }

        private string? ZCorrelationValue;

        [MaxLength(64)]
        public string? VISCOSITY_CORRELATION
        {
            get => ViscosityCorrelationValue;
            set => SetProperty(ref ViscosityCorrelationValue, value);
        }

        private string? ViscosityCorrelationValue;

        public decimal? Z_FACTOR
        {
            get => ZFactorValue;
            set => SetProperty(ref ZFactorValue, value);
        }

        private decimal? ZFactorValue;

        /// <summary>Gas viscosity in centipoise.</summary>
        public decimal? VISCOSITY_CP
        {
            get => ViscosityValue;
            set => SetProperty(ref ViscosityValue, value);
        }

        private decimal? ViscosityValue;

        /// <summary>Density in lb/ft3 at the run's pressure and temperature.</summary>
        public decimal? DENSITY
        {
            get => DensityValue;
            set => SetProperty(ref DensityValue, value);
        }

        private decimal? DensityValue;

        /// <summary>Formation volume factor in ft3/scf.</summary>
        public decimal? FORMATION_VOLUME_FACTOR
        {
            get => FormationVolumeFactorValue;
            set => SetProperty(ref FormationVolumeFactorValue, value);
        }

        private decimal? FormationVolumeFactorValue;

        /// <summary>Gas pseudo-pressure, psi2/cp, integrated from zero to the run's pressure.</summary>
        public decimal? PSEUDO_PRESSURE
        {
            get => PseudoPressureValue;
            set => SetProperty(ref PseudoPressureValue, value);
        }

        private decimal? PseudoPressureValue;

        /// <summary>The applicability warnings the engine's validator gave, one per line.</summary>
        public string? APPLICABILITY_TEXT
        {
            get => ApplicabilityTextValue;
            set => SetProperty(ref ApplicabilityTextValue, value);
        }

        private string? ApplicabilityTextValue;

        [MaxLength(64)]
        public string? FAILURE_CODE
        {
            get => FailureCodeValue;
            set => SetProperty(ref FailureCodeValue, value);
        }

        private string? FailureCodeValue;

        public string? FAILURE_MESSAGE
        {
            get => FailureMessageValue;
            set => SetProperty(ref FailureMessageValue, value);
        }

        private string? FailureMessageValue;

        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue;

        public DateTime? RECORDED_AT
        {
            get => RecordedAtValue;
            set => SetProperty(ref RecordedAtValue, value);
        }

        private DateTime? RecordedAtValue;
    }
}
