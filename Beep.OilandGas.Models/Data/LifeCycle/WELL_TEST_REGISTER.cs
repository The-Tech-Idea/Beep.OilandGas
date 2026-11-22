using System;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// The register entry for one well test (WTA-02): the PPDM test it points at, the result, the duration, who recorded
    /// it, and the analyses made from it.
    /// </summary>
    /// <remarks>
    /// PPDM's <c>WELL_TEST</c> is the owner's table for the test itself, and this record does not replace it: it carries the
    /// register facts the app adds - the result code read from <c>R_TEST_RESULT</c>, the duration in the unit the register
    /// states, the method, the remarks and the actor - and it identifies its test by the PPDM natural key
    /// (<c>UWI</c> + <c>TEST_TYPE</c> + <c>RUN_NUM</c> + <c>TEST_NUM</c>), because that table has no surrogate key for a
    /// unit of work to address. The link to analyses runs the other way: <c>WELL_TEST_ANALYSIS_RESULT.TEST_ID</c> holds
    /// <see cref="TEST_KEY"/>, so one test can have several analyses and every analysis belongs to exactly one test.
    /// </remarks>
    public partial class WELL_TEST_REGISTER : ModelEntityBase
    {
        [Key, MaxLength(128)]
        public string REGISTER_ID
        {
            get => RegisterIdValue;
            set => SetProperty(ref RegisterIdValue, value);
        }

        private string RegisterIdValue = string.Empty;

        /// <summary>
        /// The test's natural key, <c>UWI|TEST_TYPE|RUN_NUM|TEST_NUM</c> - the value an analysis stores in TEST_ID.
        /// </summary>
        [MaxLength(200)]
        public string TEST_KEY
        {
            get => TestKeyValue;
            set => SetProperty(ref TestKeyValue, value);
        }

        private string TestKeyValue = string.Empty;

        /// <summary>The one well the test belongs to (AC-1).</summary>
        [MaxLength(128)]
        public string WELL_UWI
        {
            get => WellUwiValue;
            set => SetProperty(ref WellUwiValue, value);
        }

        private string WellUwiValue = string.Empty;

        /// <summary>The test type code, from <c>R_WELL_TEST_TYPE</c> (AC-2).</summary>
        [MaxLength(32)]
        public string TEST_TYPE
        {
            get => TestTypeValue;
            set => SetProperty(ref TestTypeValue, value);
        }

        private string TestTypeValue = string.Empty;

        public int RUN_NUM
        {
            get => RunNumValue;
            set => SetProperty(ref RunNumValue, value);
        }

        private int RunNumValue;

        public int TEST_NUM
        {
            get => TestNumValue;
            set => SetProperty(ref TestNumValue, value);
        }

        private int TestNumValue;

        public DateTime TEST_DATE
        {
            get => TestDateValue;
            set => SetProperty(ref TestDateValue, value);
        }

        private DateTime TestDateValue;

        /// <summary>The duration the register states, in <see cref="DURATION_OUOM"/>.</summary>
        public decimal? DURATION
        {
            get => DurationValue;
            set => SetProperty(ref DurationValue, value);
        }

        private decimal? DurationValue;

        /// <summary>The duration's unit, from the PPDM unit code (<c>h</c>, <c>min</c>, <c>d</c>).</summary>
        [MaxLength(16)]
        public string? DURATION_OUOM
        {
            get => DurationOuomValue;
            set => SetProperty(ref DurationOuomValue, value);
        }

        private string? DurationOuomValue;

        /// <summary>
        /// The result code, from <c>R_TEST_RESULT</c>. Null means the result is pending - which is not a failed test
        /// (AC-4).
        /// </summary>
        [MaxLength(32)]
        public string? RESULT_CODE
        {
            get => ResultCodeValue;
            set => SetProperty(ref ResultCodeValue, value);
        }

        private string? ResultCodeValue;

        /// <summary>The production method the test was run under, as the register records it.</summary>
        [MaxLength(64)]
        public string? PRODUCTION_METHOD
        {
            get => ProductionMethodValue;
            set => SetProperty(ref ProductionMethodValue, value);
        }

        private string? ProductionMethodValue;

        [MaxLength(512)]
        public string? REMARKS
        {
            get => RemarksValue;
            set => SetProperty(ref RemarksValue, value);
        }

        private string? RemarksValue;

        /// <summary>The rates and pressures as the test recorded them, with their units (AC-5).</summary>
        public decimal? OIL_RATE
        {
            get => OilRateValue;
            set => SetProperty(ref OilRateValue, value);
        }

        private decimal? OilRateValue;

        [MaxLength(16)]
        public string? OIL_RATE_OUOM
        {
            get => OilRateOuomValue;
            set => SetProperty(ref OilRateOuomValue, value);
        }

        private string? OilRateOuomValue;

        public decimal? GAS_RATE
        {
            get => GasRateValue;
            set => SetProperty(ref GasRateValue, value);
        }

        private decimal? GasRateValue;

        [MaxLength(16)]
        public string? GAS_RATE_OUOM
        {
            get => GasRateOuomValue;
            set => SetProperty(ref GasRateOuomValue, value);
        }

        private string? GasRateOuomValue;

        public decimal? FLOWING_PRESSURE
        {
            get => FlowingPressureValue;
            set => SetProperty(ref FlowingPressureValue, value);
        }

        private decimal? FlowingPressureValue;

        [MaxLength(16)]
        public string? FLOWING_PRESSURE_OUOM
        {
            get => FlowingPressureOuomValue;
            set => SetProperty(ref FlowingPressureOuomValue, value);
        }

        private string? FlowingPressureOuomValue;

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
