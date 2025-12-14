using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Accounting;

namespace Beep.OilandGas.ProductionAccounting.Royalty
{
    /// <summary>
    /// Provides royalty calculation functionality.
    /// </summary>
    public static class RoyaltyCalculator
    {
        /// <summary>
        /// Calculates royalty for a sales transaction.
        /// </summary>
        public static RoyaltyCalculation CalculateRoyalty(
            SalesTransaction transaction,
            decimal royaltyInterest,
            RoyaltyDeductions? deductions = null)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (royaltyInterest < 0 || royaltyInterest > 1)
                throw new ArgumentException("Royalty interest must be between 0 and 1.", nameof(royaltyInterest));

            var calculation = new RoyaltyCalculation
            {
                CalculationId = Guid.NewGuid().ToString(),
                CalculationDate = transaction.TransactionDate,
                PropertyOrLeaseId = transaction.TransactionId, // Would be property/lease ID in real scenario
                GrossRevenue = transaction.TotalValue,
                RoyaltyInterest = royaltyInterest,
                Deductions = deductions ?? CalculateDefaultDeductions(transaction)
            };

            return calculation;
        }

        /// <summary>
        /// Calculates default deductions from a transaction.
        /// </summary>
        private static RoyaltyDeductions CalculateDefaultDeductions(SalesTransaction transaction)
        {
            var deductions = new RoyaltyDeductions();

            // Production taxes are typically deductible
            deductions.ProductionTaxes = transaction.Taxes.Sum(t => t.Amount);

            // Transportation costs are typically deductible
            deductions.TransportationCosts = transaction.Costs.TotalTransportationCosts;

            // Processing costs (if applicable)
            deductions.ProcessingCosts = 0; // Would be calculated separately

            // Marketing costs are typically not deductible
            deductions.MarketingCosts = 0;

            return deductions;
        }

        /// <summary>
        /// Calculates royalty for multiple transactions.
        /// </summary>
        public static RoyaltyCalculation CalculateRoyaltyForPeriod(
            List<SalesTransaction> transactions,
            decimal royaltyInterest,
            DateTime periodStart,
            DateTime periodEnd)
        {
            if (transactions == null || transactions.Count == 0)
                throw new ArgumentException("Transactions list cannot be null or empty.", nameof(transactions));

            var periodTransactions = transactions
                .Where(t => t.TransactionDate >= periodStart && t.TransactionDate <= periodEnd)
                .ToList();

            if (periodTransactions.Count == 0)
                throw new ArgumentException("No transactions found in the specified period.", nameof(periodStart));

            decimal totalGrossRevenue = periodTransactions.Sum(t => t.TotalValue);
            decimal totalProductionTaxes = periodTransactions.Sum(t => t.Taxes.Sum(tax => tax.Amount));
            decimal totalTransportationCosts = periodTransactions.Sum(t => t.Costs.TotalTransportationCosts);

            var deductions = new RoyaltyDeductions
            {
                ProductionTaxes = totalProductionTaxes,
                TransportationCosts = totalTransportationCosts
            };

            var calculation = new RoyaltyCalculation
            {
                CalculationId = Guid.NewGuid().ToString(),
                CalculationDate = periodEnd,
                PropertyOrLeaseId = periodTransactions.First().TransactionId, // Would be property/lease ID
                GrossRevenue = totalGrossRevenue,
                RoyaltyInterest = royaltyInterest,
                Deductions = deductions
            };

            return calculation;
        }

        /// <summary>
        /// Calculates royalty for joint interest leases.
        /// </summary>
        public static RoyaltyCalculation CalculateJointInterestRoyalty(
            SalesTransaction transaction,
            decimal workingInterest,
            decimal netRevenueInterest,
            decimal royaltyInterest)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            // For joint interest leases, royalty is calculated on the net revenue interest
            decimal grossRevenue = transaction.TotalValue * netRevenueInterest;
            var deductions = CalculateDefaultDeductions(transaction);

            var calculation = new RoyaltyCalculation
            {
                CalculationId = Guid.NewGuid().ToString(),
                CalculationDate = transaction.TransactionDate,
                GrossRevenue = grossRevenue,
                RoyaltyInterest = royaltyInterest,
                Deductions = deductions
            };

            return calculation;
        }
    }
}

