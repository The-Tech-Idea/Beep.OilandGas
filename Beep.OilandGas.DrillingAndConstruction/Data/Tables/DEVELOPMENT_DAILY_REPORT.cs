using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// One daily drilling report of a plan-stage execution (DEV-06): the 24-hour period, the depth run, the cost, the
    /// HSE events and the submission state. The activity breakdown lives in the module's <c>DRILLING_ACTIVITY</c> rows
    /// that point at this report by <c>DDR_ID</c>.
    /// </summary>
    public class DEVELOPMENT_DAILY_REPORT : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string REPORT_ID
        {
            get => ReportIdValue;
            set => SetProperty(ref ReportIdValue, value);
        }

        private string ReportIdValue = string.Empty;

        [MaxLength(64)]
        public string EXECUTION_ID
        {
            get => ExecutionIdValue;
            set => SetProperty(ref ExecutionIdValue, value);
        }

        private string ExecutionIdValue = string.Empty;

        [MaxLength(64)]
        public string PROGRAM_ID
        {
            get => ProgramIdValue;
            set => SetProperty(ref ProgramIdValue, value);
        }

        private string ProgramIdValue = string.Empty;

        [MaxLength(128)]
        public string WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string WellIdValue = string.Empty;

        /// <summary>The calendar day the report covers; with the shift it identifies the report (AC-1).</summary>
        public DateTime? REPORT_DATE
        {
            get => ReportDateValue;
            set => SetProperty(ref ReportDateValue, value);
        }

        private DateTime? ReportDateValue;

        /// <summary>DAY or NIGHT.</summary>
        [MaxLength(16)]
        public string? SHIFT
        {
            get => ShiftValue;
            set => SetProperty(ref ShiftValue, value);
        }

        private string? ShiftValue;

        /// <summary>The revision of the report; a correction is a new revision with a reason (AC-2).</summary>
        public int REVISION
        {
            get => RevisionValue;
            set => SetProperty(ref RevisionValue, value);
        }

        private int RevisionValue = 1;

        /// <summary>The report this one corrects, when it is a correction.</summary>
        [MaxLength(64)]
        public string? CORRECTS_REPORT_ID
        {
            get => CorrectsReportIdValue;
            set => SetProperty(ref CorrectsReportIdValue, value);
        }

        private string? CorrectsReportIdValue;

        public string? CORRECTION_REASON
        {
            get => CorrectionReasonValue;
            set => SetProperty(ref CorrectionReasonValue, value);
        }

        private string? CorrectionReasonValue;

        public DateTime? PERIOD_START
        {
            get => PeriodStartValue;
            set => SetProperty(ref PeriodStartValue, value);
        }

        private DateTime? PeriodStartValue;

        public DateTime? PERIOD_END
        {
            get => PeriodEndValue;
            set => SetProperty(ref PeriodEndValue, value);
        }

        private DateTime? PeriodEndValue;

        public decimal DEPTH_START
        {
            get => DepthStartValue;
            set => SetProperty(ref DepthStartValue, value);
        }

        private decimal DepthStartValue;

        public decimal DEPTH_END
        {
            get => DepthEndValue;
            set => SetProperty(ref DepthEndValue, value);
        }

        private decimal DepthEndValue;

        /// <summary>The unit of both depths; required (the depth is meaningless without it).</summary>
        [MaxLength(16)]
        public string? DEPTH_OUOM
        {
            get => DepthOuomValue;
            set => SetProperty(ref DepthOuomValue, value);
        }

        private string? DepthOuomValue;

        public decimal? COST
        {
            get => CostValue;
            set => SetProperty(ref CostValue, value);
        }

        private decimal? CostValue;

        [MaxLength(16)]
        public string? CURRENCY
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string? CurrencyValue;

        public string? HSE_EVENTS
        {
            get => HseEventsValue;
            set => SetProperty(ref HseEventsValue, value);
        }

        private string? HseEventsValue;

        public string? PERSONNEL
        {
            get => PersonnelValue;
            set => SetProperty(ref PersonnelValue, value);
        }

        private string? PersonnelValue;

        public string? EQUIPMENT
        {
            get => EquipmentValue;
            set => SetProperty(ref EquipmentValue, value);
        }

        private string? EquipmentValue;

        /// <summary>DRAFT or SUBMITTED. A submitted report is read-only (AC-2).</summary>
        [MaxLength(16)]
        public string? STATE
        {
            get => StateValue;
            set => SetProperty(ref StateValue, value);
        }

        private string? StateValue;

        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        [MaxLength(128)]
        public string? SUBMITTED_BY
        {
            get => SubmittedByValue;
            set => SetProperty(ref SubmittedByValue, value);
        }

        private string? SubmittedByValue;

        public DateTime? SUBMITTED_AT
        {
            get => SubmittedAtValue;
            set => SetProperty(ref SubmittedAtValue, value);
        }

        private DateTime? SubmittedAtValue;
    }
}
