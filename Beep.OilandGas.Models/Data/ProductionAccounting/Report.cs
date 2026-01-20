using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// DTO for operational report
    /// </summary>
    public class OperationalReport : ModelEntityBase
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime ReportPeriodStart { get; set; }
        public DateTime ReportPeriodEnd { get; set; }
        public DateTime GeneratedDate { get; set; }
        public ProductionSummary? ProductionSummary { get; set; }
        public List<RunTicket> RunTickets { get; set; } = new();
        public List<Inventory> Inventories { get; set; } = new();
        public List<AllocationResult> Allocations { get; set; } = new();
        public List<Measurement> Measurements { get; set; } = new();
        public List<SalesTransaction> SalesTransactions { get; set; } = new();
    }

    /// <summary>
    /// DTO for lease report
    /// </summary>
    public class LeaseReport : ModelEntityBase
    {
        public string ReportId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public string LeaseName { get; set; } = string.Empty;
        public DateTime ReportPeriodStart { get; set; }
        public DateTime ReportPeriodEnd { get; set; }
        public DateTime GeneratedDate { get; set; }
        public ProductionSummary? ProductionSummary { get; set; }
        public RevenueSummary? RevenueSummary { get; set; }
        public List<RunTicket> RunTickets { get; set; } = new();
        public List<SalesTransaction> SalesTransactions { get; set; } = new();
    }

    /// <summary>
    /// DTO for governmental report
    /// </summary>
    public class GovernmentalReport : ModelEntityBase
    {
        public string ReportId { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty;
        public DateTime ReportPeriodStart { get; set; }
        public DateTime ReportPeriodEnd { get; set; }
        public DateTime GeneratedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public ProductionSummary? ProductionSummary { get; set; }
        public RevenueSummary? RevenueSummary { get; set; }
        public List<ProductionTax> Taxes { get; set; } = new();
    }

    /// <summary>
    /// DTO for joint interest statement (JIB)
    /// </summary>
    public class JointInterestStatement : ModelEntityBase
    {
        public string StatementId { get; set; } = string.Empty;
        public string PropertyOrLeaseId { get; set; } = string.Empty;
        public DateTime StatementPeriodStart { get; set; }
        public DateTime StatementPeriodEnd { get; set; }
        public DateTime GeneratedDate { get; set; }
        public ProductionSummary? ProductionSummary { get; set; }
        public RevenueSummary? RevenueSummary { get; set; }
        public List<JointInterestOwner> Owners { get; set; } = new();
    }

    /// <summary>
    /// DTO for joint interest owner
    /// </summary>
    public class JointInterestOwner : ModelEntityBase
    {
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal WorkingInterest { get; set; }
        public decimal NetRevenueInterest { get; set; }
        public decimal AllocatedVolume { get; set; }
        public decimal AllocatedRevenue { get; set; }
        public decimal AllocatedCosts { get; set; }
        public decimal NetAmount { get; set; }
    }

    /// <summary>
    /// Request to generate operational report
    /// </summary>
    public class GenerateOperationalReportRequest : ModelEntityBase
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public string? LeaseId { get; set; }
        public string? WellId { get; set; }
    }

    /// <summary>
    /// Request to generate lease report
    /// </summary>
    public class GenerateLeaseReportRequest : ModelEntityBase
    {
        [Required]
        public string LeaseId { get; set; } = string.Empty;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Request to generate governmental report
    /// </summary>
    public class GenerateGovernmentalReportRequest : ModelEntityBase
    {
        [Required]
        public string ReportType { get; set; } = string.Empty;
        [Required]
        public string Jurisdiction { get; set; } = string.Empty;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Request to generate JIB statement
    /// </summary>
    public class GenerateJIBStatementRequest : ModelEntityBase
    {
        [Required]
        public string PropertyOrLeaseId { get; set; } = string.Empty;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }
}





