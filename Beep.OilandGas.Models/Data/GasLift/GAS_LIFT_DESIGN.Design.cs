using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// The design columns of a gas lift record (LFT-02): the spacing method, the port size, the units, the valve list, the
    /// limits that were evaluated and the actor.
    /// </summary>
    /// <remarks>
    /// The generated table held the valve count, the injection rate, the injection pressure and the expected rate. AC-6
    /// needs the method and every limit that was evaluated, and AC-5 needs the units, so those are added here additively.
    /// </remarks>
    public partial class GAS_LIFT_DESIGN
    {
        /// <summary>UNIFORM_SPACING or EQUAL_PRESSURE_DROP - from the design-method reference set.</summary>
        [MaxLength(32)]
        public string? SPACING_METHOD
        {
            get => SpacingMethodValue;
            set => SetProperty(ref SpacingMethodValue, value);
        }

        private string? SpacingMethodValue;

        /// <summary>The valve port diameter in inches, from the port-size reference set.</summary>
        public decimal? PORT_SIZE_IN
        {
            get => PortSizeValue;
            set => SetProperty(ref PortSizeValue, value);
        }

        private decimal? PortSizeValue;

        [MaxLength(16)]
        public string? INJECTION_UNIT
        {
            get => InjectionUnitValue;
            set => SetProperty(ref InjectionUnitValue, value);
        }

        private string? InjectionUnitValue;

        [MaxLength(16)]
        public string? PRODUCTION_UNIT
        {
            get => ProductionUnitValue;
            set => SetProperty(ref ProductionUnitValue, value);
        }

        private string? ProductionUnitValue;

        /// <summary>The valve depths in feet, one per line, so the placed valves can be read back.</summary>
        public string? VALVE_DEPTHS
        {
            get => ValveDepthsValue;
            set => SetProperty(ref ValveDepthsValue, value);
        }

        private string? ValveDepthsValue;

        /// <summary>The openings and closings, one per line, matching <see cref="VALVE_DEPTHS"/>.</summary>
        public string? VALVE_PRESSURES
        {
            get => ValvePressuresValue;
            set => SetProperty(ref ValvePressuresValue, value);
        }

        private string? ValvePressuresValue;

        /// <summary>The evaluated limit codes, comma separated, so the record says what was checked (AC-6).</summary>
        [MaxLength(512)]
        public string? LIMIT_CODES
        {
            get => LimitCodesValue;
            set => SetProperty(ref LimitCodesValue, value);
        }

        private string? LimitCodesValue;

        /// <summary>The catalog message of each violated limit, one per line (AC-1).</summary>
        public string? LIMIT_MESSAGES
        {
            get => LimitMessagesValue;
            set => SetProperty(ref LimitMessagesValue, value);
        }

        private string? LimitMessagesValue;

        /// <summary>'Y' when a blocking limit was violated: such a design is not saved.</summary>
        [MaxLength(1)]
        public string BLOCKING_IND
        {
            get => BlockingIndValue;
            set => SetProperty(ref BlockingIndValue, value);
        }

        private string BlockingIndValue = "N";

        /// <summary>The optimum injection rate the potential analysis recommends, in Mscf/day.</summary>
        public decimal? RECOMMENDED_INJECTION_RATE
        {
            get => RecommendedInjectionRateValue;
            set => SetProperty(ref RecommendedInjectionRateValue, value);
        }

        private decimal? RecommendedInjectionRateValue;

        /// <summary>The inputs as entered, so the design can be reproduced.</summary>
        public string? INPUTS_JSON
        {
            get => InputsJsonValue;
            set => SetProperty(ref InputsJsonValue, value);
        }

        private string? InputsJsonValue;

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
