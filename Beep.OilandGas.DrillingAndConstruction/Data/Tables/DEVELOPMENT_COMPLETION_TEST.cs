using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// One completion test of a well (DEV-07): what was tested, over which interval, what it measured and in which unit,
    /// who ran it and the evidence it produced. The PPDM <c>WELL_TEST</c> table stays the interchange record; this row is
    /// the completion screen's own test with its status, evidence reference and audit columns.
    /// </summary>
    public class DEVELOPMENT_COMPLETION_TEST : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string TEST_ID
        {
            get => TestIdValue;
            set => SetProperty(ref TestIdValue, value);
        }

        private string TestIdValue = string.Empty;

        [MaxLength(128)]
        public string WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string WellIdValue = string.Empty;

        [MaxLength(64)]
        public string? PROGRAM_ID
        {
            get => ProgramIdValue;
            set => SetProperty(ref ProgramIdValue, value);
        }

        private string? ProgramIdValue;

        /// <summary>The test type (for example PRESSURE_TEST, INFLOW_TEST, CBL, LEAK_OFF).</summary>
        [MaxLength(64)]
        public string TEST_TYPE
        {
            get => TestTypeValue;
            set => SetProperty(ref TestTypeValue, value);
        }

        private string TestTypeValue = string.Empty;

        public decimal TOP_DEPTH
        {
            get => TopDepthValue;
            set => SetProperty(ref TopDepthValue, value);
        }

        private decimal TopDepthValue;

        public decimal BASE_DEPTH
        {
            get => BaseDepthValue;
            set => SetProperty(ref BaseDepthValue, value);
        }

        private decimal BaseDepthValue;

        /// <summary>The unit of both depths; required.</summary>
        [MaxLength(16)]
        public string? DEPTH_OUOM
        {
            get => DepthOuomValue;
            set => SetProperty(ref DepthOuomValue, value);
        }

        private string? DepthOuomValue;

        /// <summary>PASS or FAIL - the test's own verdict, which acceptance reads (AC-1).</summary>
        [MaxLength(8)]
        public string STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string StatusValue = string.Empty;

        /// <summary>The measured result as recorded (a number or a short sentence).</summary>
        public string? RESULT
        {
            get => ResultValue;
            set => SetProperty(ref ResultValue, value);
        }

        private string? ResultValue;

        /// <summary>The unit of the result; required on every test (AC-5).</summary>
        [MaxLength(16)]
        public string? RESULT_OUOM
        {
            get => ResultOuomValue;
            set => SetProperty(ref ResultOuomValue, value);
        }

        private string? ResultOuomValue;

        [MaxLength(128)]
        public string? PERFORMED_BY
        {
            get => PerformedByValue;
            set => SetProperty(ref PerformedByValue, value);
        }

        private string? PerformedByValue;

        public DateTime? PERFORMED_AT
        {
            get => PerformedAtValue;
            set => SetProperty(ref PerformedAtValue, value);
        }

        private DateTime? PerformedAtValue;

        /// <summary>The document or system reference that holds the test's evidence.</summary>
        [MaxLength(256)]
        public string? EVIDENCE_REF
        {
            get => EvidenceRefValue;
            set => SetProperty(ref EvidenceRefValue, value);
        }

        private string? EvidenceRefValue;

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
