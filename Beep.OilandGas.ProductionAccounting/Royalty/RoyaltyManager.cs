using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.ProductionAccounting.Ownership;

namespace Beep.OilandGas.ProductionAccounting.Royalty
{
    /// <summary>
    /// Manages royalty calculations, payments, and reporting.
    /// </summary>
    public class RoyaltyManager
    {
        private readonly Dictionary<string, RoyaltyInterest> royaltyInterests = new();
        private readonly Dictionary<string, RoyaltyPayment> payments = new();
        private readonly Dictionary<string, RoyaltyStatement> statements = new();
        private readonly Dictionary<string, List<RoyaltyPayment>> paymentsByOwner = new();

        /// <summary>
        /// Registers a royalty interest.
        /// </summary>
        public void RegisterRoyaltyInterest(RoyaltyInterest interest)
        {
            if (interest == null)
                throw new ArgumentNullException(nameof(interest));

            if (string.IsNullOrEmpty(interest.RoyaltyInterestId))
                interest.RoyaltyInterestId = Guid.NewGuid().ToString();

            royaltyInterests[interest.RoyaltyInterestId] = interest;
        }

        /// <summary>
        /// Calculates and creates a royalty payment.
        /// </summary>
        public RoyaltyPayment CalculateAndCreatePayment(
            SalesTransaction transaction,
            string royaltyOwnerId,
            decimal royaltyInterest,
            DateTime paymentDate)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var calculation = RoyaltyCalculator.CalculateRoyalty(transaction, royaltyInterest);

            var payment = new RoyaltyPayment
            {
                PaymentId = Guid.NewGuid().ToString(),
                RoyaltyOwnerId = royaltyOwnerId,
                PropertyOrLeaseId = transaction.TransactionId, // Would be property/lease ID
                PaymentPeriodStart = transaction.TransactionDate,
                PaymentPeriodEnd = transaction.TransactionDate,
                RoyaltyAmount = calculation.RoyaltyAmount,
                PaymentDate = paymentDate,
                Status = PaymentStatus.Pending
            };

            payments[payment.PaymentId] = payment;

            if (!paymentsByOwner.ContainsKey(royaltyOwnerId))
                paymentsByOwner[royaltyOwnerId] = new List<RoyaltyPayment>();
            paymentsByOwner[royaltyOwnerId].Add(payment);

            return payment;
        }

        /// <summary>
        /// Creates a royalty statement.
        /// </summary>
        public RoyaltyStatement CreateStatement(
            string royaltyOwnerId,
            string propertyOrLeaseId,
            DateTime periodStart,
            DateTime periodEnd,
            List<SalesTransaction> transactions,
            decimal royaltyInterest)
        {
            if (transactions == null || transactions.Count == 0)
                throw new ArgumentException("Transactions list cannot be null or empty.", nameof(transactions));

            var periodTransactions = transactions
                .Where(t => t.TransactionDate >= periodStart && t.TransactionDate <= periodEnd)
                .ToList();

            var calculation = RoyaltyCalculator.CalculateRoyaltyForPeriod(
                periodTransactions,
                royaltyInterest,
                periodStart,
                periodEnd);

            var statement = new RoyaltyStatement
            {
                StatementId = Guid.NewGuid().ToString(),
                StatementPeriodStart = periodStart,
                StatementPeriodEnd = periodEnd,
                RoyaltyOwnerId = royaltyOwnerId,
                PropertyOrLeaseId = propertyOrLeaseId,
                Production = new ProductionSummary
                {
                    TotalOilProduction = periodTransactions.Sum(t => t.NetVolume),
                    ProducingDays = (periodEnd - periodStart).Days
                },
                Revenue = new RevenueSummary
                {
                    GrossRevenue = calculation.GrossRevenue,
                    AveragePricePerBarrel = periodTransactions.Count > 0 
                        ? periodTransactions.Average(t => t.PricePerBarrel)
                        : 0,
                    TransactionCount = periodTransactions.Count
                },
                Deductions = new DeductionsSummary
                {
                    TotalProductionTaxes = calculation.Deductions.ProductionTaxes,
                    TotalTransportationCosts = calculation.Deductions.TransportationCosts,
                    TotalProcessingCosts = calculation.Deductions.ProcessingCosts,
                    TotalOtherDeductions = calculation.Deductions.OtherDeductions
                },
                Calculation = calculation
            };

            statements[statement.StatementId] = statement;
            return statement;
        }

        /// <summary>
        /// Applies tax withholdings to a payment.
        /// </summary>
        public void ApplyTaxWithholdings(
            string paymentId,
            string taxId,
            string ownerState,
            string productionState,
            bool isResidentAlien = true)
        {
            if (!payments.TryGetValue(paymentId, out var payment))
                throw new ArgumentException($"Payment {paymentId} not found.", nameof(paymentId));

            payment.TaxWithholdings.Clear();

            // Check for invalid tax ID
            if (!TaxReporting.ValidateTaxId(taxId))
            {
                decimal withholding = TaxReporting.CalculateInvalidTaxIdWithholding(payment.RoyaltyAmount);
                payment.TaxWithholdings.Add(new TaxWithholding
                {
                    WithholdingId = Guid.NewGuid().ToString(),
                    WithholdingType = TaxWithholdingType.InvalidTaxId,
                    WithholdingRate = 0.24m,
                    Amount = withholding,
                    Reason = "Invalid Tax ID - Backup Withholding"
                });
            }

            // Check for out of state withholding
            if (!string.IsNullOrEmpty(ownerState) && !string.IsNullOrEmpty(productionState) && ownerState != productionState)
            {
                decimal withholding = TaxReporting.CalculateOutOfStateWithholding(
                    payment.RoyaltyAmount,
                    ownerState,
                    productionState,
                    0.05m); // Example 5% rate

                payment.TaxWithholdings.Add(new TaxWithholding
                {
                    WithholdingId = Guid.NewGuid().ToString(),
                    WithholdingType = TaxWithholdingType.OutOfState,
                    WithholdingRate = 0.05m,
                    Amount = withholding,
                    Reason = $"Out of state withholding - {ownerState} to {productionState}"
                });
            }

            // Check for alien withholding
            if (!isResidentAlien)
            {
                decimal withholding = TaxReporting.CalculateAlienWithholding(payment.RoyaltyAmount, false);
                payment.TaxWithholdings.Add(new TaxWithholding
                {
                    WithholdingId = Guid.NewGuid().ToString(),
                    WithholdingType = TaxWithholdingType.Alien,
                    WithholdingRate = 0.30m,
                    Amount = withholding,
                    Reason = "Non-resident alien withholding"
                });
            }
        }

        /// <summary>
        /// Gets payments for a royalty owner.
        /// </summary>
        public IEnumerable<RoyaltyPayment> GetPaymentsByOwner(string royaltyOwnerId)
        {
            return paymentsByOwner.TryGetValue(royaltyOwnerId, out var ownerPayments) 
                ? ownerPayments 
                : Enumerable.Empty<RoyaltyPayment>();
        }

        /// <summary>
        /// Gets a statement by ID.
        /// </summary>
        public RoyaltyStatement? GetStatement(string statementId)
        {
            return statements.TryGetValue(statementId, out var statement) ? statement : null;
        }

        /// <summary>
        /// Gets suspended payments.
        /// </summary>
        public IEnumerable<RoyaltyPayment> GetSuspendedPayments()
        {
            return payments.Values.Where(p => p.Status == PaymentStatus.Suspended);
        }
    }
}

