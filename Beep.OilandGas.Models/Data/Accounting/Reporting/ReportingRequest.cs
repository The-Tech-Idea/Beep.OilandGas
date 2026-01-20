using System;

namespace Beep.OilandGas.Models.Data.Accounting.Reporting
{
    /// <summary>
    /// Request DTO for generating operational report
    /// </summary>
    public class GenerateOperationalReportRequest : ModelEntityBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Request DTO for generating lease report
    /// </summary>
    public class GenerateLeaseReportRequest : ModelEntityBase
    {
        public string LeaseId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}




