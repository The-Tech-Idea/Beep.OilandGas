using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.ProductionAccounting.Ownership;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Royalty
{
    /// <summary>
    /// Manages royalty calculations, payments, and reporting.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class RoyaltyManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<RoyaltyManager>? _logger;
        private readonly string _connectionName;
        private const string ROYALTY_INTEREST_TABLE = "ROYALTY_INTEREST";
        private const string ROYALTY_PAYMENT_TABLE = "ROYALTY_PAYMENT";
        private const string ROYALTY_STATEMENT_TABLE = "ROYALTY_STATEMENT";
        private const string ROYALTY_CALCULATION_TABLE = "ROYALTY_CALCULATION";

        public RoyaltyManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILoggerFactory loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = loggerFactory?.CreateLogger<RoyaltyManager>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Registers a royalty interest.
        /// </summary>
        public async Task RegisterRoyaltyInterestAsync(RoyaltyInterest interest, string userId = "system", string? connectionName = null)
        {
            if (interest == null)
                throw new ArgumentNullException(nameof(interest));

            if (string.IsNullOrEmpty(interest.RoyaltyInterestId))
                interest.RoyaltyInterestId = Guid.NewGuid().ToString();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(ROYALTY_INTEREST_TABLE, interest);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register royalty interest {InterestId}: {Error}", interest.RoyaltyInterestId, errorMessage);
                throw new InvalidOperationException($"Failed to save royalty interest: {errorMessage}");
            }

            _logger?.LogDebug("Registered royalty interest {InterestId} to database", interest.RoyaltyInterestId);
        }

        /// <summary>
        /// Registers a royalty interest (synchronous wrapper).
        /// </summary>
        public void RegisterRoyaltyInterest(RoyaltyInterest interest)
        {
            RegisterRoyaltyInterestAsync(interest).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Calculates and creates a royalty payment.
        /// </summary>
        public async Task<RoyaltyPayment> CalculateAndCreatePaymentAsync(
            SalesTransaction transaction,
            string royaltyOwnerId,
            decimal royaltyInterest,
            DateTime paymentDate,
            string userId = "system",
            string? connectionName = null)
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

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(ROYALTY_PAYMENT_TABLE, payment);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create royalty payment {PaymentId}: {Error}", payment.PaymentId, errorMessage);
                throw new InvalidOperationException($"Failed to save royalty payment: {errorMessage}");
            }

            _logger?.LogDebug("Created royalty payment {PaymentId} in database", payment.PaymentId);
            return payment;
        }

        /// <summary>
        /// Calculates and creates a royalty payment (synchronous wrapper).
        /// </summary>
        public RoyaltyPayment CalculateAndCreatePayment(
            SalesTransaction transaction,
            string royaltyOwnerId,
            decimal royaltyInterest,
            DateTime paymentDate)
        {
            return CalculateAndCreatePaymentAsync(transaction, royaltyOwnerId, royaltyInterest, paymentDate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a royalty statement.
        /// </summary>
        public async Task<RoyaltyStatement> CreateStatementAsync(
            string royaltyOwnerId,
            string propertyOrLeaseId,
            DateTime periodStart,
            DateTime periodEnd,
            List<SalesTransaction> transactions,
            decimal royaltyInterest,
            string userId = "system",
            string? connectionName = null)
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

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(ROYALTY_STATEMENT_TABLE, statement);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create royalty statement {StatementId}: {Error}", statement.StatementId, errorMessage);
                throw new InvalidOperationException($"Failed to save royalty statement: {errorMessage}");
            }

            _logger?.LogDebug("Created royalty statement {StatementId} in database", statement.StatementId);
            return statement;
        }

        /// <summary>
        /// Creates a royalty statement (synchronous wrapper).
        /// </summary>
        public RoyaltyStatement CreateStatement(
            string royaltyOwnerId,
            string propertyOrLeaseId,
            DateTime periodStart,
            DateTime periodEnd,
            List<SalesTransaction> transactions,
            decimal royaltyInterest)
        {
            return CreateStatementAsync(royaltyOwnerId, propertyOrLeaseId, periodStart, periodEnd, transactions, royaltyInterest).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Applies tax withholdings to a payment.
        /// </summary>
        public async Task ApplyTaxWithholdingsAsync(
            string paymentId,
            string taxId,
            string ownerState,
            string productionState,
            bool isResidentAlien = true,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROYALTY_PAYMENT_ID", Operator = "=", FilterValue = paymentId }
            };

            var results = await dataSource.GetEntityAsync(ROYALTY_PAYMENT_TABLE, filters);
            var paymentData = results?.FirstOrDefault();
            if (paymentData == null)
                throw new ArgumentException($"Payment {paymentId} not found.", nameof(paymentId));

            var payment = paymentData as RoyaltyPayment;
            if (payment == null)
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

            // Update payment in database
            dataSource.UpdateEntity(ROYALTY_PAYMENT_TABLE, payment);

            _logger?.LogDebug("Applied tax withholdings to payment {PaymentId}", paymentId);
        }

        /// <summary>
        /// Applies tax withholdings to a payment (synchronous wrapper).
        /// </summary>
        public void ApplyTaxWithholdings(
            string paymentId,
            string taxId,
            string ownerState,
            string productionState,
            bool isResidentAlien = true)
        {
            ApplyTaxWithholdingsAsync(paymentId, taxId, ownerState, productionState, isResidentAlien).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets payments for a royalty owner.
        /// </summary>
        public async Task<IEnumerable<RoyaltyPayment>> GetPaymentsByOwnerAsync(string royaltyOwnerId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(royaltyOwnerId))
                return Enumerable.Empty<RoyaltyPayment>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROYALTY_OWNER_ID", Operator = "=", FilterValue = royaltyOwnerId }
            };

            var results = await dataSource.GetEntityAsync(ROYALTY_PAYMENT_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<RoyaltyPayment>();

            return results.Cast<RoyaltyPayment>().Where(p => p != null)!;
        }

        /// <summary>
        /// Gets payments for a royalty owner (synchronous wrapper).
        /// </summary>
        public IEnumerable<RoyaltyPayment> GetPaymentsByOwner(string royaltyOwnerId)
        {
            return GetPaymentsByOwnerAsync(royaltyOwnerId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a statement by ID.
        /// </summary>
        public async Task<RoyaltyStatement?> GetStatementAsync(string statementId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(statementId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATEMENT_ID", Operator = "=", FilterValue = statementId }
            };

            var results = await dataSource.GetEntityAsync(ROYALTY_STATEMENT_TABLE, filters);
            var statementData = results?.FirstOrDefault();
            
            if (statementData == null)
                return null;

            return statementData as RoyaltyStatement;
        }

        /// <summary>
        /// Gets a statement by ID (synchronous wrapper).
        /// </summary>
        public RoyaltyStatement? GetStatement(string statementId)
        {
            return GetStatementAsync(statementId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets suspended payments.
        /// </summary>
        public async Task<IEnumerable<RoyaltyPayment>> GetSuspendedPaymentsAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = "Suspended" }
            };

            var results = await dataSource.GetEntityAsync(ROYALTY_PAYMENT_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<RoyaltyPayment>();

            return results.Cast<RoyaltyPayment>().Where(p => p != null)!;
        }

        /// <summary>
        /// Gets suspended payments (synchronous wrapper).
        /// </summary>
        public IEnumerable<RoyaltyPayment> GetSuspendedPayments()
        {
            return GetSuspendedPaymentsAsync().GetAwaiter().GetResult();
        }

    }
}

