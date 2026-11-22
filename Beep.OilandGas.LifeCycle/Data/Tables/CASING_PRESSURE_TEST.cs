using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables
{
    /// One recorded casing-pressure test and what the API RP 90 classification made of it (ENG-04).
    /// </summary>
    /// <remarks>
    /// The readings are kept as JSON on the test rather than in a child table: they are one ordered series belonging to
    /// this test, re-read as a whole when a classification is re-run, and never queried on their own. The class, the
    /// rule that produced it and the limits it was judged against are stored with the test, so a later settings change
    /// does not rewrite what an inspection concluded.
    /// </remarks>
    public class CASING_PRESSURE_TEST : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string TEST_ID
        {
            get => TestidValue;
            set => SetProperty(ref TestidValue, value);
        }

        private string TestidValue = string.Empty;

        [MaxLength(64)]
        public string UWI
        {
            get => UwiValue;
            set => SetProperty(ref UwiValue, value);
        }

        private string UwiValue = string.Empty;

        [MaxLength(16)]
        public string ANNULUS_NAME
        {
            get => AnnulusnameValue;
            set => SetProperty(ref AnnulusnameValue, value);
        }

        private string AnnulusnameValue = string.Empty;

        /// <summary>When the bleed-down started.</summary>
        public DateTime BLEED_START_UTC
        {
            get => BleedstartutcValue;
            set => SetProperty(ref BleedstartutcValue, value);
        }

        private DateTime BleedstartutcValue = DateTime.UtcNow;

        /// <summary>When the valve was closed, so build-up starts. Null leaves the test incomplete.</summary>
        public DateTime? VALVE_CLOSED_UTC
        {
            get => ValveclosedutcValue;
            set => SetProperty(ref ValveclosedutcValue, value);
        }

        private DateTime? ValveclosedutcValue = null;

        /// <summary>The bleed-down window, in hours - null when the valve-closed time is not recorded.</summary>
        public decimal? DURATION_HOURS
        {
            get => DurationhoursValue;
            set => SetProperty(ref DurationhoursValue, value);
        }

        private decimal? DurationhoursValue = null;

        /// <summary>The pressure readings as recorded: time, pressure and unit, in order.</summary>
        [MaxLength(4000)]
        public string? READINGS_JSON
        {
            get => ReadingsjsonValue;
            set => SetProperty(ref ReadingsjsonValue, value);
        }

        private string? ReadingsjsonValue = null;

        /// <summary>The pressure when bleeding started, in psi.</summary>
        public decimal? INITIAL_PRESSURE
        {
            get => InitialpressureValue;
            set => SetProperty(ref InitialpressureValue, value);
        }

        private decimal? InitialpressureValue = null;

        /// <summary>The highest pressure the test saw, in psi - the figure compared with the limits.</summary>
        public decimal? MAX_PRESSURE
        {
            get => MaxpressureValue;
            set => SetProperty(ref MaxpressureValue, value);
        }

        private decimal? MaxpressureValue = null;

        [MaxLength(16)]
        public string? PRESSURE_OUOM
        {
            get => PressureouomValue;
            set => SetProperty(ref PressureouomValue, value);
        }

        private string? PressureouomValue = null;

        /// <summary>The annulus temperature at the test, as recorded.</summary>
        public decimal? TEMPERATURE
        {
            get => TemperatureValue;
            set => SetProperty(ref TemperatureValue, value);
        }

        private decimal? TemperatureValue = null;

        [MaxLength(16)]
        public string? TEMPERATURE_OUOM
        {
            get => TemperatureouomValue;
            set => SetProperty(ref TemperatureouomValue, value);
        }

        private string? TemperatureouomValue = null;

        /// <summary>The bleed-off behaviour in the recorder's own words.</summary>
        [MaxLength(1000)]
        public string? BLEED_OFF_TEXT
        {
            get => BleedofftextValue;
            set => SetProperty(ref BleedofftextValue, value);
        }

        private string? BleedofftextValue = null;

        /// <summary>The API RP 90 class the settings produced, or UNCLASSIFIED with the reason.</summary>
        [MaxLength(64)]
        public string? CLASS_TEXT
        {
            get => ClasstextValue;
            set => SetProperty(ref ClasstextValue, value);
        }

        private string? ClasstextValue = null;

        /// <summary>The sentence the classification produced - the rule that decided it.</summary>
        [MaxLength(1000)]
        public string? RULE_TEXT
        {
            get => RuletextValue;
            set => SetProperty(ref RuletextValue, value);
        }

        private string? RuletextValue = null;

        /// <summary>The recommended action, which follows the class and the limits.</summary>
        [MaxLength(500)]
        public string? ACTION_TEXT
        {
            get => ActiontextValue;
            set => SetProperty(ref ActiontextValue, value);
        }

        private string? ActiontextValue = null;

        /// <summary>CRITICAL, WARNING or INFO - above the MAASP is always critical.</summary>
        [MaxLength(16)]
        public string? SEVERITY
        {
            get => SeverityValue;
            set => SetProperty(ref SeverityValue, value);
        }

        private string? SeverityValue = null;

        /// <summary>The MAWOP the test was judged against, in psi.</summary>
        public decimal? MAWOP
        {
            get => MawopValue;
            set => SetProperty(ref MawopValue, value);
        }

        private decimal? MawopValue = null;

        /// <summary>The MAASP at the time, in psi.</summary>
        public decimal? MAASP
        {
            get => MaaspValue;
            set => SetProperty(ref MaaspValue, value);
        }

        private decimal? MaaspValue = null;

        /// <summary>How far the pressure was above the MAASP, in psi; null when within it.</summary>
        public decimal? EXCEEDANCE
        {
            get => ExceedanceValue;
            set => SetProperty(ref ExceedanceValue, value);
        }

        private decimal? ExceedanceValue = null;

        /// <summary>Y when the bleed-down and build-up could be judged, N when a timing is missing.</summary>
        [MaxLength(1)]
        public string? COMPLETE
        {
            get => CompleteValue;
            set => SetProperty(ref CompleteValue, value);
        }

        private string? CompleteValue = null;

        [MaxLength(128)]
        public string? RECORDED_BY
        {
            get => RecordedbyValue;
            set => SetProperty(ref RecordedbyValue, value);
        }

        private string? RecordedbyValue = null;

        public DateTime RECORDED_AT_UTC
        {
            get => RecordedatutcValue;
            set => SetProperty(ref RecordedatutcValue, value);
        }

        private DateTime RecordedatutcValue = DateTime.UtcNow;

    }
}
