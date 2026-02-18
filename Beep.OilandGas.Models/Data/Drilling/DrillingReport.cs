using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public class DrillingReport : ModelEntityBase
    {
        private string ReportIdValue = string.Empty;
        public string ReportId
        {
            get { return this.ReportIdValue; }
            set { SetProperty(ref ReportIdValue, value); }
        }
        private string OperationIdValue = string.Empty;
        public string OperationId
        {
            get { return this.OperationIdValue; }
            set { SetProperty(ref OperationIdValue, value); }
        }
        private DateTime? ReportDateValue;
        public DateTime? ReportDate
        {
            get { return this.ReportDateValue; }
            set { SetProperty(ref ReportDateValue, value); }
        }
        private decimal? DepthValue;
        public decimal? Depth
        {
            get { return this.DepthValue; }
            set { SetProperty(ref DepthValue, value); }
        }
        private string? ActivityValue;
        public string? Activity
        {
            get { return this.ActivityValue; }
            set { SetProperty(ref ActivityValue, value); }
        }
        private decimal? HoursValue;
        public decimal? Hours
        {
            get { return this.HoursValue; }
            set { SetProperty(ref HoursValue, value); }
        }
        private string? RemarksValue;
        public string? Remarks
        {
            get { return this.RemarksValue; }
            set { SetProperty(ref RemarksValue, value); }
        }
        private string? ReportedByValue;
        public string? ReportedBy
        {
            get { return this.ReportedByValue; }
            set { SetProperty(ref ReportedByValue, value); }
        }
    }
}
