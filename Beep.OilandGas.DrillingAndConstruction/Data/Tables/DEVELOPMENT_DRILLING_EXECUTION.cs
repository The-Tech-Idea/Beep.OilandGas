using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// The execution of one approved drilling program (DEV-06): the well, the program version it runs and whether the
    /// operation is active, suspended or complete.
    /// </summary>
    /// <remarks>
    /// The module's <c>DRILLING_OPERATION</c> is the company execution record: its schema requires the program review
    /// instance, the execution-start proof and the recorder's role/persona, none of which a plan-stage execution has.
    /// This row is the plan-stage execution, and the company operation is started from it when the governance gate is
    /// met. Depth, current activity and cost are derived from the daily reports, never stored here.
    /// </remarks>
    public class DEVELOPMENT_DRILLING_EXECUTION : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string EXECUTION_ID
        {
            get => ExecutionIdValue;
            set => SetProperty(ref ExecutionIdValue, value);
        }

        private string ExecutionIdValue = string.Empty;

        /// <summary>The DEV-05 program this execution runs.</summary>
        [MaxLength(64)]
        public string PROGRAM_ID
        {
            get => ProgramIdValue;
            set => SetProperty(ref ProgramIdValue, value);
        }

        private string ProgramIdValue = string.Empty;

        [MaxLength(64)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        [MaxLength(128)]
        public string WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string WellIdValue = string.Empty;

        [MaxLength(128)]
        public string? COMPANY_ID
        {
            get => CompanyIdValue;
            set => SetProperty(ref CompanyIdValue, value);
        }

        private string? CompanyIdValue;

        /// <summary>ACTIVE, SUSPENDED or COMPLETED.</summary>
        [MaxLength(32)]
        public string? STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string? StatusValue;

        /// <summary>The program version the execution started from, so a later program revision does not rewrite it.</summary>
        public int PROGRAM_VERSION
        {
            get => ProgramVersionValue;
            set => SetProperty(ref ProgramVersionValue, value);
        }

        private int ProgramVersionValue;

        public DateTime? SPUD_DATE
        {
            get => SpudDateValue;
            set => SetProperty(ref SpudDateValue, value);
        }

        private DateTime? SpudDateValue;
    }
}
