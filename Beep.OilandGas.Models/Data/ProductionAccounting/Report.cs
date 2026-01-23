using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// DTO for operational report
    /// </summary>
    public class OperationalReport : ModelEntityBase
    {
        private string ReportIdValue = string.Empty;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private DateTime ReportPeriodStartValue;

        public DateTime ReportPeriodStart

        {

            get { return this.ReportPeriodStartValue; }

            set { SetProperty(ref ReportPeriodStartValue, value); }

        }
        private DateTime ReportPeriodEndValue;

        public DateTime ReportPeriodEnd

        {

            get { return this.ReportPeriodEndValue; }

            set { SetProperty(ref ReportPeriodEndValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private ProductionSummary? ProductionSummaryValue;

        public ProductionSummary? ProductionSummary

        {

            get { return this.ProductionSummaryValue; }

            set { SetProperty(ref ProductionSummaryValue, value); }

        }
        private List<RunTicket> RunTicketsValue = new();

        public List<RunTicket> RunTickets

        {

            get { return this.RunTicketsValue; }

            set { SetProperty(ref RunTicketsValue, value); }

        }
        private List<Inventory> InventoriesValue = new();

        public List<Inventory> Inventories

        {

            get { return this.InventoriesValue; }

            set { SetProperty(ref InventoriesValue, value); }

        }
        private List<AllocationResult> AllocationsValue = new();

        public List<AllocationResult> Allocations

        {

            get { return this.AllocationsValue; }

            set { SetProperty(ref AllocationsValue, value); }

        }
        private List<Measurement> MeasurementsValue = new();

        public List<Measurement> Measurements

        {

            get { return this.MeasurementsValue; }

            set { SetProperty(ref MeasurementsValue, value); }

        }
        private List<SalesTransaction> SalesTransactionsValue = new();

        public List<SalesTransaction> SalesTransactions

        {

            get { return this.SalesTransactionsValue; }

            set { SetProperty(ref SalesTransactionsValue, value); }

        }
    }

    /// <summary>
    /// DTO for lease report
    /// </summary>
    public class LeaseReport : ModelEntityBase
    {
        private string ReportIdValue = string.Empty;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string LeaseNameValue = string.Empty;

        public string LeaseName

        {

            get { return this.LeaseNameValue; }

            set { SetProperty(ref LeaseNameValue, value); }

        }
        private DateTime ReportPeriodStartValue;

        public DateTime ReportPeriodStart

        {

            get { return this.ReportPeriodStartValue; }

            set { SetProperty(ref ReportPeriodStartValue, value); }

        }
        private DateTime ReportPeriodEndValue;

        public DateTime ReportPeriodEnd

        {

            get { return this.ReportPeriodEndValue; }

            set { SetProperty(ref ReportPeriodEndValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private ProductionSummary? ProductionSummaryValue;

        public ProductionSummary? ProductionSummary

        {

            get { return this.ProductionSummaryValue; }

            set { SetProperty(ref ProductionSummaryValue, value); }

        }
        private RevenueSummary? RevenueSummaryValue;

        public RevenueSummary? RevenueSummary

        {

            get { return this.RevenueSummaryValue; }

            set { SetProperty(ref RevenueSummaryValue, value); }

        }
        private List<RunTicket> RunTicketsValue = new();

        public List<RunTicket> RunTickets

        {

            get { return this.RunTicketsValue; }

            set { SetProperty(ref RunTicketsValue, value); }

        }
        private List<SalesTransaction> SalesTransactionsValue = new();

        public List<SalesTransaction> SalesTransactions

        {

            get { return this.SalesTransactionsValue; }

            set { SetProperty(ref SalesTransactionsValue, value); }

        }
    }

    /// <summary>
    /// DTO for governmental report
    /// </summary>
    public class GovernmentalReport : ModelEntityBase
    {
        private string ReportIdValue = string.Empty;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private string ReportTypeValue = string.Empty;

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private string JurisdictionValue = string.Empty;

        public string Jurisdiction

        {

            get { return this.JurisdictionValue; }

            set { SetProperty(ref JurisdictionValue, value); }

        }
        private DateTime ReportPeriodStartValue;

        public DateTime ReportPeriodStart

        {

            get { return this.ReportPeriodStartValue; }

            set { SetProperty(ref ReportPeriodStartValue, value); }

        }
        private DateTime ReportPeriodEndValue;

        public DateTime ReportPeriodEnd

        {

            get { return this.ReportPeriodEndValue; }

            set { SetProperty(ref ReportPeriodEndValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private ProductionSummary? ProductionSummaryValue;

        public ProductionSummary? ProductionSummary

        {

            get { return this.ProductionSummaryValue; }

            set { SetProperty(ref ProductionSummaryValue, value); }

        }
        private RevenueSummary? RevenueSummaryValue;

        public RevenueSummary? RevenueSummary

        {

            get { return this.RevenueSummaryValue; }

            set { SetProperty(ref RevenueSummaryValue, value); }

        }
        private List<ProductionTax> TaxesValue = new();

        public List<ProductionTax> Taxes

        {

            get { return this.TaxesValue; }

            set { SetProperty(ref TaxesValue, value); }

        }
    }

    /// <summary>
    /// DTO for joint interest statement (JIB)
    /// </summary>
    public class JointInterestStatement : ModelEntityBase
    {
        private string StatementIdValue = string.Empty;

        public string StatementId

        {

            get { return this.StatementIdValue; }

            set { SetProperty(ref StatementIdValue, value); }

        }
        private string PropertyOrLeaseIdValue = string.Empty;

        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }
        private DateTime StatementPeriodStartValue;

        public DateTime StatementPeriodStart

        {

            get { return this.StatementPeriodStartValue; }

            set { SetProperty(ref StatementPeriodStartValue, value); }

        }
        private DateTime StatementPeriodEndValue;

        public DateTime StatementPeriodEnd

        {

            get { return this.StatementPeriodEndValue; }

            set { SetProperty(ref StatementPeriodEndValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private ProductionSummary? ProductionSummaryValue;

        public ProductionSummary? ProductionSummary

        {

            get { return this.ProductionSummaryValue; }

            set { SetProperty(ref ProductionSummaryValue, value); }

        }
        private RevenueSummary? RevenueSummaryValue;

        public RevenueSummary? RevenueSummary

        {

            get { return this.RevenueSummaryValue; }

            set { SetProperty(ref RevenueSummaryValue, value); }

        }
        private List<JointInterestOwner> OwnersValue = new();

        public List<JointInterestOwner> Owners

        {

            get { return this.OwnersValue; }

            set { SetProperty(ref OwnersValue, value); }

        }
    }

    /// <summary>
    /// DTO for joint interest owner
    /// </summary>
    public class JointInterestOwner : ModelEntityBase
    {
        private string OwnerIdValue = string.Empty;

        public string OwnerId

        {

            get { return this.OwnerIdValue; }

            set { SetProperty(ref OwnerIdValue, value); }

        }
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private decimal AllocatedVolumeValue;

        public decimal AllocatedVolume

        {

            get { return this.AllocatedVolumeValue; }

            set { SetProperty(ref AllocatedVolumeValue, value); }

        }
        private decimal AllocatedRevenueValue;

        public decimal AllocatedRevenue

        {

            get { return this.AllocatedRevenueValue; }

            set { SetProperty(ref AllocatedRevenueValue, value); }

        }
        private decimal AllocatedCostsValue;

        public decimal AllocatedCosts

        {

            get { return this.AllocatedCostsValue; }

            set { SetProperty(ref AllocatedCostsValue, value); }

        }
        private decimal NetAmountValue;

        public decimal NetAmount

        {

            get { return this.NetAmountValue; }

            set { SetProperty(ref NetAmountValue, value); }

        }
    }

    /// <summary>
    /// Request to generate operational report
    /// </summary>
    public class GenerateOperationalReportRequest : ModelEntityBase
    {
        private DateTime StartDateValue;

        [Required]
        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        [Required]
        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private string? LeaseIdValue;

        public string? LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
    }

    /// <summary>
    /// Request to generate lease report
    /// </summary>
    public class GenerateLeaseReportRequest : ModelEntityBase
    {
        private string LeaseIdValue = string.Empty;

        [Required]
        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime StartDateValue;

        [Required]
        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        [Required]
        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }

    /// <summary>
    /// Request to generate governmental report
    /// </summary>
    public class GenerateGovernmentalReportRequest : ModelEntityBase
    {
        private string ReportTypeValue = string.Empty;

        [Required]
        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private string JurisdictionValue = string.Empty;

        [Required]
        public string Jurisdiction

        {

            get { return this.JurisdictionValue; }

            set { SetProperty(ref JurisdictionValue, value); }

        }
        private DateTime StartDateValue;

        [Required]
        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        [Required]
        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }

    /// <summary>
    /// Request to generate JIB statement
    /// </summary>
    public class GenerateJIBStatementRequest : ModelEntityBase
    {
        private string PropertyOrLeaseIdValue = string.Empty;

        [Required]
        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }
        private DateTime StartDateValue;

        [Required]
        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        [Required]
        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }
}








