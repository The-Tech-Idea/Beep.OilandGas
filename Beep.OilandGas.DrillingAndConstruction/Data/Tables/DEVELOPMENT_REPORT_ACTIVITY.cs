using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// One activity line of a daily drilling report (ENG-09): when it ran, the IADC operation code it is reported under, the
    /// footage it made, and whether it was non-productive and why.
    /// </summary>
    /// <remarks>
    /// The daily report (DEV-06) records a shift: its period, the depth at each end and its cost. It records no operation code and
    /// no non-productive cause, so the owner's performance analysis - which is driven by exactly those two - had nothing to read. The
    /// line carries them, keeps the report as its parent, and is what the analysis, the CSV import and the offset comparison all use.
    /// </remarks>
    public class DEVELOPMENT_REPORT_ACTIVITY : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string ACTIVITY_ID
        {
            get => ActivityidValue;
            set => SetProperty(ref ActivityidValue, value);
        }

        private string ActivityidValue = string.Empty;

        /// <summary>The daily report the line belongs to (DEV-06).</summary>
        [MaxLength(64)]
        public string REPORT_ID
        {
            get => ReportidValue;
            set => SetProperty(ref ReportidValue, value);
        }

        private string ReportidValue = string.Empty;

        [MaxLength(64)]
        public string WELL_ID
        {
            get => WellidValue;
            set => SetProperty(ref WellidValue, value);
        }

        private string WellidValue = string.Empty;

        /// <summary>Start of the activity.</summary>
        public DateTime START_UTC
        {
            get => StartutcValue;
            set => SetProperty(ref StartutcValue, value);
        }

        private DateTime StartutcValue = DateTime.UtcNow;

        /// <summary>End of the activity; the hours are the difference.</summary>
        public DateTime END_UTC
        {
            get => EndutcValue;
            set => SetProperty(ref EndutcValue, value);
        }

        private DateTime EndutcValue = DateTime.UtcNow;

        /// <summary>The IADC operation code the line is reported under (drilling.operationCodes).</summary>
        [MaxLength(16)]
        public string CODE
        {
            get => CodeValue;
            set => SetProperty(ref CodeValue, value);
        }

        private string CodeValue = string.Empty;

        /// <summary>Measured depth at the start of the line.</summary>
        public decimal? DEPTH_FROM
        {
            get => DepthfromValue;
            set => SetProperty(ref DepthfromValue, value);
        }

        private decimal? DepthfromValue = null;

        /// <summary>Measured depth at the end; a drilling line carries the new hole depth.</summary>
        public decimal? DEPTH_TO
        {
            get => DepthtoValue;
            set => SetProperty(ref DepthtoValue, value);
        }

        private decimal? DepthtoValue = null;

        [MaxLength(16)]
        public string? DEPTH_OUOM
        {
            get => DepthouomValue;
            set => SetProperty(ref DepthouomValue, value);
        }

        private string? DepthouomValue = null;

        /// <summary>What was done, in the reporter's words.</summary>
        [MaxLength(500)]
        public string? DESCRIPTION
        {
            get => DescriptionValue;
            set => SetProperty(ref DescriptionValue, value);
        }

        private string? DescriptionValue = null;

        /// <summary>Y when the line is non-productive. Null leaves it to drilling.nptCodes.</summary>
        [MaxLength(1)]
        public string? NPT
        {
            get => NptValue;
            set => SetProperty(ref NptValue, value);
        }

        private string? NptValue = null;

        /// <summary>Why the line was non-productive; null on an NPT line is reported as unclassified (AC-3).</summary>
        [MaxLength(64)]
        public string? NPT_CAUSE
        {
            get => NptcauseValue;
            set => SetProperty(ref NptcauseValue, value);
        }

        private string? NptcauseValue = null;

        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue = null;

        public DateTime RECORDED_AT_UTC
        {
            get => RecordedatutcValue;
            set => SetProperty(ref RecordedatutcValue, value);
        }

        private DateTime RecordedatutcValue = DateTime.UtcNow;

    }
}
