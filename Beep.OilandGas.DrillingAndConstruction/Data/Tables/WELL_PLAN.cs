using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// A directional well plan (ENG-06): the tie-on, the target, the method and the dogleg it was built at, with the outcome the
    /// stations produced.
    /// </summary>
    /// <remarks>
    /// The stations are not stored. The plan is a few design inputs and the owner's minimum-curvature construction reproduces them
    /// exactly, so a saved plan keeps the method and the inputs and the trajectory is recomputed when it is opened - which is also
    /// what makes the handoff a trajectory rather than a copy of the record.
    /// </remarks>
    public class WELL_PLAN : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string PLAN_ID
        {
            get => PlanidValue;
            set => SetProperty(ref PlanidValue, value);
        }

        private string PlanidValue = string.Empty;

        [MaxLength(64)]
        public string UWI
        {
            get => UwiValue;
            set => SetProperty(ref UwiValue, value);
        }

        private string UwiValue = string.Empty;

        [MaxLength(64)]
        public string? FIELD_NAME
        {
            get => FieldnameValue;
            set => SetProperty(ref FieldnameValue, value);
        }

        private string? FieldnameValue = null;

        [MaxLength(128)]
        public string? PLAN_NAME
        {
            get => PlannameValue;
            set => SetProperty(ref PlannameValue, value);
        }

        private string? PlannameValue = null;

        /// <summary>BUILD_AND_HOLD or EXPLICIT - the method that produced the stations (AC-6).</summary>
        [MaxLength(32)]
        public string DESIGN_METHOD
        {
            get => DesignmethodValue;
            set => SetProperty(ref DesignmethodValue, value);
        }

        private string DesignmethodValue = string.Empty;

        /// <summary>The unit every geometric value on the plan is in.</summary>
        [MaxLength(16)]
        public string DEPTH_OUOM
        {
            get => DepthouomValue;
            set => SetProperty(ref DepthouomValue, value);
        }

        private string DepthouomValue = string.Empty;

        /// <summary>Where the plan starts: measured depth at the tie-on.</summary>
        public decimal TIE_ON_MD
        {
            get => TieonmdValue;
            set => SetProperty(ref TieonmdValue, value);
        }

        private decimal TieonmdValue = 0m;

        public decimal TIE_ON_NORTH
        {
            get => TieonnorthValue;
            set => SetProperty(ref TieonnorthValue, value);
        }

        private decimal TieonnorthValue = 0m;

        public decimal TIE_ON_EAST
        {
            get => TieoneastValue;
            set => SetProperty(ref TieoneastValue, value);
        }

        private decimal TieoneastValue = 0m;

        public decimal TIE_ON_TVD
        {
            get => TieontvdValue;
            set => SetProperty(ref TieontvdValue, value);
        }

        private decimal TieontvdValue = 0m;

        public decimal TIE_ON_INCLINATION
        {
            get => TieoninclinationValue;
            set => SetProperty(ref TieoninclinationValue, value);
        }

        private decimal TieoninclinationValue = 0m;

        public decimal TIE_ON_AZIMUTH
        {
            get => TieonazimuthValue;
            set => SetProperty(ref TieonazimuthValue, value);
        }

        private decimal TieonazimuthValue = 0m;

        /// <summary>The target the plan is designed to, when the method uses one.</summary>
        public decimal? TARGET_NORTH
        {
            get => TargetnorthValue;
            set => SetProperty(ref TargetnorthValue, value);
        }

        private decimal? TargetnorthValue = null;

        public decimal? TARGET_EAST
        {
            get => TargeteastValue;
            set => SetProperty(ref TargeteastValue, value);
        }

        private decimal? TargeteastValue = null;

        public decimal? TARGET_TVD
        {
            get => TargettvdValue;
            set => SetProperty(ref TargettvdValue, value);
        }

        private decimal? TargettvdValue = null;

        /// <summary>The radius within which the plan counts as on target.</summary>
        public decimal TARGET_TOLERANCE
        {
            get => TargettoleranceValue;
            set => SetProperty(ref TargettoleranceValue, value);
        }

        private decimal TargettoleranceValue = 0m;

        /// <summary>The dogleg severity the curve was built at.</summary>
        public decimal DESIGN_DOGLEG
        {
            get => DesigndoglegValue;
            set => SetProperty(ref DesigndoglegValue, value);
        }

        private decimal DesigndoglegValue = 0m;

        /// <summary>The limit the stations were checked against, as configured when the plan was made.</summary>
        public decimal? MAX_DOGLEG
        {
            get => MaxdoglegValue;
            set => SetProperty(ref MaxdoglegValue, value);
        }

        private decimal? MaxdoglegValue = null;

        /// <summary>The explicit sections, as recorded, when the method is EXPLICIT.</summary>
        [MaxLength(2000)]
        public string? SECTIONS_TEXT
        {
            get => SectionstextValue;
            set => SetProperty(ref SectionstextValue, value);
        }

        private string? SectionstextValue = null;

        /// <summary>How many stations the plan produced.</summary>
        public int STATION_COUNT
        {
            get => StationcountValue;
            set => SetProperty(ref StationcountValue, value);
        }

        private int StationcountValue = 0;

        public decimal? FINAL_MD
        {
            get => FinalmdValue;
            set => SetProperty(ref FinalmdValue, value);
        }

        private decimal? FinalmdValue = null;

        public decimal? FINAL_NORTH
        {
            get => FinalnorthValue;
            set => SetProperty(ref FinalnorthValue, value);
        }

        private decimal? FinalnorthValue = null;

        public decimal? FINAL_EAST
        {
            get => FinaleastValue;
            set => SetProperty(ref FinaleastValue, value);
        }

        private decimal? FinaleastValue = null;

        public decimal? FINAL_TVD
        {
            get => FinaltvdValue;
            set => SetProperty(ref FinaltvdValue, value);
        }

        private decimal? FinaltvdValue = null;

        public decimal? FINAL_INCLINATION
        {
            get => FinalinclinationValue;
            set => SetProperty(ref FinalinclinationValue, value);
        }

        private decimal? FinalinclinationValue = null;

        public decimal? FINAL_AZIMUTH
        {
            get => FinalazimuthValue;
            set => SetProperty(ref FinalazimuthValue, value);
        }

        private decimal? FinalazimuthValue = null;

        /// <summary>How far the plan ends from the target - zero when the method lands on it.</summary>
        public decimal? MISS_DISTANCE
        {
            get => MissdistanceValue;
            set => SetProperty(ref MissdistanceValue, value);
        }

        private decimal? MissdistanceValue = null;

        /// <summary>Y when the miss is within the tolerance.</summary>
        [MaxLength(1)]
        public string? TARGET_HIT
        {
            get => TargethitValue;
            set => SetProperty(ref TargethitValue, value);
        }

        private string? TargethitValue = null;

        /// <summary>The largest dogleg the stations actually show.</summary>
        public decimal? MAX_STATION_DOGLEG
        {
            get => MaxstationdoglegValue;
            set => SetProperty(ref MaxstationdoglegValue, value);
        }

        private decimal? MaxstationdoglegValue = null;

        /// <summary>How the tortuosity figure was formed, so it is not a bare number.</summary>
        [MaxLength(256)]
        public string? TORTUOSITY_BASIS
        {
            get => TortuositybasisValue;
            set => SetProperty(ref TortuositybasisValue, value);
        }

        private string? TortuositybasisValue = null;

        public decimal? TORTUOSITY
        {
            get => TortuosityValue;
            set => SetProperty(ref TortuosityValue, value);
        }

        private decimal? TortuosityValue = null;

        /// <summary>When the computed trajectory was handed to torque and drag.</summary>
        public DateTime? HANDED_OFF_AT_UTC
        {
            get => HandedoffatutcValue;
            set => SetProperty(ref HandedoffatutcValue, value);
        }

        private DateTime? HandedoffatutcValue = null;

        [MaxLength(128)]
        public string? HANDED_OFF_BY
        {
            get => HandedoffbyValue;
            set => SetProperty(ref HandedoffbyValue, value);
        }

        private string? HandedoffbyValue = null;

        [MaxLength(128)]
        public string? SAVED_BY
        {
            get => SavedbyValue;
            set => SetProperty(ref SavedbyValue, value);
        }

        private string? SavedbyValue = null;

        public DateTime SAVED_AT_UTC
        {
            get => SavedatutcValue;
            set => SetProperty(ref SavedatutcValue, value);
        }

        private DateTime SavedatutcValue = DateTime.UtcNow;

    }
}
