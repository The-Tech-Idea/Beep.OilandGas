using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// One recorded torque-and-drag run (ENG-07): the scenario, the friction factors and the string it was run with, and the hook
    /// load, surface torque and limiting component it produced.
    /// </summary>
    /// <remarks>
    /// The tool computes the run in memory and keeps nothing, so a prediction could not be compared with the next one or with a
    /// measured hook load. The run keeps the inputs as they were applied and the answer they gave, and the stations are recomputed
    /// from them rather than stored - the geometry belongs to the plan it ran along.
    /// </remarks>
    public class TORQUE_DRAG_RUN : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string RUN_ID
        {
            get => RunidValue;
            set => SetProperty(ref RunidValue, value);
        }

        private string RunidValue = string.Empty;

        [MaxLength(64)]
        public string UWI
        {
            get => UwiValue;
            set => SetProperty(ref UwiValue, value);
        }

        private string UwiValue = string.Empty;

        /// <summary>The saved well plan whose trajectory was run (ENG-06).</summary>
        [MaxLength(64)]
        public string? PLAN_ID
        {
            get => PlanidValue;
            set => SetProperty(ref PlanidValue, value);
        }

        private string? PlanidValue = null;

        /// <summary>PICK_UP, SLACK_OFF, ROTATE_OFF or DRILLING (AC-6).</summary>
        [MaxLength(32)]
        public string SCENARIO
        {
            get => ScenarioValue;
            set => SetProperty(ref ScenarioValue, value);
        }

        private string ScenarioValue = string.Empty;

        /// <summary>The cased-hole friction factor the run used (AC-3).</summary>
        public decimal CASED_FRICTION
        {
            get => CasedfrictionValue;
            set => SetProperty(ref CasedfrictionValue, value);
        }

        private decimal CasedfrictionValue = 0m;

        /// <summary>The open-hole friction factor the run used (AC-3).</summary>
        public decimal OPEN_HOLE_FRICTION
        {
            get => OpenholefrictionValue;
            set => SetProperty(ref OpenholefrictionValue, value);
        }

        private decimal OpenholefrictionValue = 0m;

        /// <summary>The casing shoe measured depth: cased above it, open hole below.</summary>
        public decimal? SHOE_DEPTH
        {
            get => ShoedepthValue;
            set => SetProperty(ref ShoedepthValue, value);
        }

        private decimal? ShoedepthValue = null;

        [MaxLength(16)]
        public string? SHOE_DEPTH_OUOM
        {
            get => ShoedepthouomValue;
            set => SetProperty(ref ShoedepthouomValue, value);
        }

        private string? ShoedepthouomValue = null;

        /// <summary>The mud density used for buoyancy.</summary>
        public decimal? MUD_DENSITY
        {
            get => MuddensityValue;
            set => SetProperty(ref MuddensityValue, value);
        }

        private decimal? MuddensityValue = null;

        [MaxLength(16)]
        public string? MUD_DENSITY_OUOM
        {
            get => MuddensityouomValue;
            set => SetProperty(ref MuddensityouomValue, value);
        }

        private string? MuddensityouomValue = null;

        /// <summary>The soft-string element length, in metres.</summary>
        public decimal ELEMENT_LENGTH_M
        {
            get => ElementlengthmValue;
            set => SetProperty(ref ElementlengthmValue, value);
        }

        private decimal ElementlengthmValue = 0m;

        /// <summary>Weight on bit, for DRILLING.</summary>
        public decimal? WOB
        {
            get => WobValue;
            set => SetProperty(ref WobValue, value);
        }

        private decimal? WobValue = null;

        [MaxLength(16)]
        public string? WOB_OUOM
        {
            get => WobouomValue;
            set => SetProperty(ref WobouomValue, value);
        }

        private string? WobouomValue = null;

        /// <summary>The axial force at the bit for the other operations, in newtons.</summary>
        public decimal? BIT_TENSION_N
        {
            get => BittensionnValue;
            set => SetProperty(ref BittensionnValue, value);
        }

        private decimal? BittensionnValue = null;

        /// <summary>The torque at the bit for DRILLING, in newton-metres.</summary>
        public decimal? BIT_TORQUE_NM
        {
            get => BittorquenmValue;
            set => SetProperty(ref BittorquenmValue, value);
        }

        private decimal? BittorquenmValue = null;

        /// <summary>The string from the bit up, as recorded.</summary>
        [MaxLength(2000)]
        public string? STRING_TEXT
        {
            get => StringtextValue;
            set => SetProperty(ref StringtextValue, value);
        }

        private string? StringtextValue = null;

        /// <summary>The hook load the run produced, in kilonewtons.</summary>
        public decimal? HOOK_LOAD_KN
        {
            get => HookloadknValue;
            set => SetProperty(ref HookloadknValue, value);
        }

        private decimal? HookloadknValue = null;

        /// <summary>The surface torque, in kilonewton-metres.</summary>
        public decimal? SURFACE_TORQUE_KNM
        {
            get => SurfacetorqueknmValue;
            set => SetProperty(ref SurfacetorqueknmValue, value);
        }

        private decimal? SurfacetorqueknmValue = null;

        /// <summary>The limiting factor, with the component it belongs to (AC-2).</summary>
        [MaxLength(500)]
        public string? LIMIT_TEXT
        {
            get => LimittextValue;
            set => SetProperty(ref LimittextValue, value);
        }

        private string? LimittextValue = null;

        [MaxLength(128)]
        public string? LIMIT_COMPONENT
        {
            get => LimitcomponentValue;
            set => SetProperty(ref LimitcomponentValue, value);
        }

        private string? LimitcomponentValue = null;

        /// <summary>The largest load over a component's limit.</summary>
        public decimal? MAX_UTILISATION
        {
            get => MaxutilisationValue;
            set => SetProperty(ref MaxutilisationValue, value);
        }

        private decimal? MaxutilisationValue = null;

        /// <summary>The smallest remaining capacity, in kilonewtons.</summary>
        public decimal? MARGIN_KN
        {
            get => MarginknValue;
            set => SetProperty(ref MarginknValue, value);
        }

        private decimal? MarginknValue = null;

        /// <summary>How many stations the run produced.</summary>
        public int STATION_COUNT
        {
            get => StationcountValue;
            set => SetProperty(ref StationcountValue, value);
        }

        private int StationcountValue = 0;

        /// <summary>Y when no station exceeds its component's limit.</summary>
        [MaxLength(1)]
        public string? PASSED
        {
            get => PassedValue;
            set => SetProperty(ref PassedValue, value);
        }

        private string? PassedValue = null;

        [MaxLength(128)]
        public string? RAN_BY
        {
            get => RanbyValue;
            set => SetProperty(ref RanbyValue, value);
        }

        private string? RanbyValue = null;

        public DateTime RAN_AT_UTC
        {
            get => RanatutcValue;
            set => SetProperty(ref RanatutcValue, value);
        }

        private DateTime RanatutcValue = DateTime.UtcNow;

    }
}
