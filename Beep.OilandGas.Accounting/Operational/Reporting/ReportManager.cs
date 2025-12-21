using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Accounting.Operational.Reporting
{
    /// <summary>
    /// Manages report generation and storage.
    /// </summary>
    public class ReportManager
    {
        private readonly Dictionary<string, Report> reports = new();

        /// <summary>
        /// Generates and stores an operational report.
        /// </summary>
        public OperationalReport GenerateOperationalReport(
            DateTime periodStart,
            DateTime periodEnd,
            List<Production.RunTicket> runTickets,
            List<Inventory.CrudeOilInventory> inventories,
            List<Allocation.AllocationResult> allocations,
            List<Production.MeasurementRecord> measurements,
            List<Revenue.SalesTransaction> transactions)
        {
            var report = ReportGenerator.GenerateOperationalReport(
                periodStart,
                periodEnd,
                runTickets,
                inventories,
                allocations,
                measurements,
                transactions);

            reports[report.ReportId] = report;
            return report;
        }

        /// <summary>
        /// Generates and stores a lease report.
        /// </summary>
        public LeaseReport GenerateLeaseReport(
            string leaseId,
            DateTime periodStart,
            DateTime periodEnd,
            List<Production.RunTicket> runTickets,
            List<Revenue.SalesTransaction> transactions)
        {
            var report = ReportGenerator.GenerateLeaseReport(
                leaseId,
                periodStart,
                periodEnd,
                runTickets,
                transactions);

            reports[report.ReportId] = report;
            return report;
        }

        /// <summary>
        /// Generates and stores a governmental report.
        /// </summary>
        public GovernmentalReport GenerateGovernmentalReport(
            string reportingAgency,
            string reportFormat,
            DateTime periodStart,
            DateTime periodEnd,
            List<Production.RunTicket> runTickets,
            List<Revenue.SalesTransaction> transactions,
            List<Royalty.RoyaltyPayment> royaltyPayments)
        {
            var report = ReportGenerator.GenerateGovernmentalReport(
                reportingAgency,
                reportFormat,
                periodStart,
                periodEnd,
                runTickets,
                transactions,
                royaltyPayments);

            reports[report.ReportId] = report;
            return report;
        }

        /// <summary>
        /// Generates and stores a joint interest statement.
        /// </summary>
        public JointInterestStatement GenerateJointInterestStatement(
            string jibId,
            string operatorName,
            DateTime periodStart,
            DateTime periodEnd,
            List<JIBParticipant> participants,
            List<JIBCharge> charges,
            List<JIBCredit> credits)
        {
            var statement = ReportGenerator.GenerateJointInterestStatement(
                jibId,
                operatorName,
                periodStart,
                periodEnd,
                participants,
                charges,
                credits);

            reports[statement.ReportId] = statement;
            return statement;
        }

        /// <summary>
        /// Gets a report by ID.
        /// </summary>
        public Report? GetReport(string reportId)
        {
            return reports.TryGetValue(reportId, out var report) ? report : null;
        }

        /// <summary>
        /// Gets reports by type.
        /// </summary>
        public IEnumerable<Report> GetReportsByType(ReportType reportType)
        {
            return reports.Values.Where(r => r.ReportType == reportType);
        }

        /// <summary>
        /// Gets reports by date range.
        /// </summary>
        public IEnumerable<Report> GetReportsByDateRange(DateTime startDate, DateTime endDate)
        {
            return reports.Values.Where(r => 
                r.ReportPeriodStart >= startDate && r.ReportPeriodEnd <= endDate);
        }
    }
}

