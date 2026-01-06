using System;

namespace Beep.OilandGas.Models.DTOs.Accounting.Reporting
{
    /// <summary>
    /// Request DTO for generating operational report
    /// </summary>
    public class GenerateOperationalReportRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Request DTO for generating lease report
    /// </summary>
    public class GenerateLeaseReportRequest
    {
        public string LeaseId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}



