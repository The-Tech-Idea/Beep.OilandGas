
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Inventory;
using Beep.OilandGas.ProductionAccounting.Allocation;
using Beep.OilandGas.ProductionAccounting.Royalty;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.Reporting
{
    /// <summary>
    /// Provides report generation functionality.
    /// </summary>
    public static class ReportGenerator
    {
        /// <summary>
        /// Generates an operational report.
        /// </summary>
        public static OperationalReport GenerateOperationalReport(
            DateTime periodStart,
            DateTime periodEnd,
            List<RunTicket> runTickets,
            List<CrudeOilInventory> inventories,
            List<AllocationResult> allocations,
            List<Measurement.MeasurementRecord> measurements,
            List<SalesTransaction> transactions)
        {
            var report = new OperationalReport
            {
                ReportId = Guid.NewGuid().ToString(),
                ReportType = ReportType.InternalOperational,
                ReportPeriodStart = periodStart,
                ReportPeriodEnd = periodEnd,
                GenerationDate = DateTime.Now
            };

            // Production summary
            var periodTickets = runTickets.Where(t => 
                t.TicketDateTime >= periodStart && t.TicketDateTime <= periodEnd).ToList();
            
            report.Production = new ProductionReportSummary
            {
                TotalProduction = periodTickets.Sum(t => t.NetVolume),
                ProducingDays = (periodEnd - periodStart).Days,
                AverageDailyProduction = periodTickets.Count > 0 
                    ? periodTickets.Sum(t => t.NetVolume) / (periodEnd - periodStart).Days 
                    : 0
            };

            // Inventory summary
            var periodInventories = inventories.Where(i => 
                i.InventoryDate >= periodStart && i.InventoryDate <= periodEnd).ToList();
            
            if (periodInventories.Count > 0)
            {
                report.Inventory = new InventoryReportSummary
                {
                    OpeningInventory = periodInventories.First().Volume,
                    ClosingInventory = periodInventories.Last().Volume,
                    Receipts = periodTickets.Sum(t => t.NetVolume), // Simplified
                    Deliveries = periodTickets.Where(t => t.DispositionType == DispositionType.Sale)
                        .Sum(t => t.NetVolume)
                };
            }

            // Allocation summary
            report.Allocation = new AllocationReportSummary
            {
                WellsAllocated = allocations.SelectMany(a => a.Details).Select(d => d.EntityId).Distinct().Count(),
                LeasesAllocated = allocations.Count,
                TotalAllocatedVolume = allocations.Sum(a => a.TotalVolume)
            };

            // Measurement summary
            var periodMeasurements = measurements.Where(m => 
                m.MeasurementDateTime >= periodStart && m.MeasurementDateTime <= periodEnd).ToList();
            
            report.Measurement = new MeasurementReportSummary
            {
                MeasurementCount = periodMeasurements.Count,
                AverageAccuracy = periodMeasurements.Where(m => m.Accuracy.HasValue)
                    .Average(m => m.Accuracy!.Value),
                CalibrationsDue = 0 // Would be calculated from calibration schedule
            };

            // Cost summary
            var periodTransactions = transactions.Where(t => 
                t.TransactionDate >= periodStart && t.TransactionDate <= periodEnd).ToList();
            
            report.Costs = new CostReportSummary
            {
                TotalLiftingCosts = periodTransactions.Sum(t => t.Costs.TotalLiftingCosts),
                TotalOperatingCosts = periodTransactions.Sum(t => t.Costs.TotalOperatingCosts),
                TotalMarketingCosts = periodTransactions.Sum(t => t.Costs.TotalMarketingCosts)
            };

            return report;
        }

        /// <summary>
        /// Generates a lease report.
        /// </summary>
        public static LeaseReport GenerateLeaseReport(
            string leaseId,
            DateTime periodStart,
            DateTime periodEnd,
            List<RunTicket> runTickets,
            List<SalesTransaction> transactions)
        {
            var leaseTickets = runTickets.Where(t => 
                t.LeaseId == leaseId &&
                t.TicketDateTime >= periodStart && t.TicketDateTime <= periodEnd).ToList();

            var leaseTransactions = transactions.Where(t => 
                t.TransactionDate >= periodStart && t.TransactionDate <= periodEnd).ToList();

            var report = new LeaseReport
            {
                ReportId = Guid.NewGuid().ToString(),
                ReportType = ReportType.InternalLease,
                ReportPeriodStart = periodStart,
                ReportPeriodEnd = periodEnd,
                LeaseId = leaseId,
                ProductionVolume = leaseTickets.Sum(t => t.NetVolume),
                Revenue = leaseTransactions.Sum(t => t.TotalValue),
                Costs = leaseTransactions.Sum(t => t.Costs.TotalCosts)
            };

            return report;
        }

        /// <summary>
        /// Generates a governmental report.
        /// </summary>
        public static GovernmentalReport GenerateGovernmentalReport(
            string reportingAgency,
            string reportFormat,
            DateTime periodStart,
            DateTime periodEnd,
            List<RunTicket> runTickets,
            List<SalesTransaction> transactions,
            List<RoyaltyPayment> royaltyPayments)
        {
            var periodTickets = runTickets.Where(t => 
                t.TicketDateTime >= periodStart && t.TicketDateTime <= periodEnd).ToList();

            var periodTransactions = transactions.Where(t => 
                t.TransactionDate >= periodStart && t.TransactionDate <= periodEnd).ToList();

            var periodRoyaltyPayments = royaltyPayments.Where(p => 
                p.PaymentPeriodStart >= periodStart && p.PaymentPeriodEnd <= periodEnd).ToList();

            var report = new GovernmentalReport
            {
                ReportId = Guid.NewGuid().ToString(),
                ReportType = ReportType.Governmental,
                ReportPeriodStart = periodStart,
                ReportPeriodEnd = periodEnd,
                ReportingAgency = reportingAgency,
                ReportFormat = reportFormat,
                Production = new GovernmentalProductionData
                {
                    OilProduction = periodTickets.Sum(t => t.NetVolume),
                    GasProduction = 0, // Would be from gas production data
                    WaterProduction = periodTickets.Sum(t => t.BSWVolume)
                },
                Royalty = new GovernmentalRoyaltyData
                {
                    RoyaltyVolume = periodRoyaltyPayments.Sum(p => p.RoyaltyAmount) / 
                        (periodTransactions.Average(t => t.PricePerBarrel) > 0 
                            ? periodTransactions.Average(t => t.PricePerBarrel) 
                            : 1),
                    RoyaltyValue = periodRoyaltyPayments.Sum(p => p.RoyaltyAmount),
                    RoyaltyRate = 0.125m // Would be from lease
                },
                Taxes = new GovernmentalTaxData
                {
                    SeveranceTax = periodTransactions.Sum(t => 
                        t.Taxes.Where(tax => tax.TaxType == ProductionTaxType.Severance)
                        .Sum(tax => tax.Amount)),
                    AdValoremTax = periodTransactions.Sum(t => 
                        t.Taxes.Where(tax => tax.TaxType == ProductionTaxType.AdValorem)
                        .Sum(tax => tax.Amount))
                }
            };

            return report;
        }

        /// <summary>
        /// Generates a joint interest statement.
        /// </summary>
        public static JointInterestStatement GenerateJointInterestStatement(
            string jibId,
            string operatorName,
            DateTime periodStart,
            DateTime periodEnd,
            List<JIBParticipant> participants,
            List<JIBCharge> charges,
            List<JIBCredit> credits)
        {
            var statement = new JointInterestStatement
            {
                ReportId = Guid.NewGuid().ToString(),
                ReportType = ReportType.JointInterest,
                ReportPeriodStart = periodStart,
                ReportPeriodEnd = periodEnd,
                JIBId = jibId,
                Operator = operatorName,
                Participants = participants,
                Charges = charges,
                Credits = credits
            };

            return statement;
        }
    }
}
