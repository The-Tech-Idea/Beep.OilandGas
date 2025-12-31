using Beep.OilandGas.Models.Data.Royalty;
using Beep.OilandGas.Models.DTOs.Royalty;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Royalty
{
    /// <summary>
    /// Service for managing royalty calculations, payments, and reporting.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class RoyaltyService : IRoyaltyService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<RoyaltyService>? _logger;
        private readonly string _connectionName;
        private const string ROYALTY_INTEREST_TABLE = "ROYALTY_INTEREST";
        private const string ROYALTY_PAYMENT_TABLE = "ROYALTY_PAYMENT";
        private const string ROYALTY_STATEMENT_TABLE = "ROYALTY_STATEMENT";
        private const string ROYALTY_CALCULATION_TABLE = "ROYALTY_CALCULATION";

        public RoyaltyService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<RoyaltyService>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Registers a royalty interest.
        /// </summary>
        public async Task<ROYALTY_INTEREST> RegisterRoyaltyInterestAsync(CreateRoyaltyInterestRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.InterestPercentage < 0 || request.InterestPercentage > 1)
                throw new ArgumentException("Interest percentage must be between 0 and 1.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetRoyaltyInterestRepositoryAsync(connName);

            var interest = new ROYALTY_INTEREST
            {
                ROYALTY_INTEREST_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = request.PropertyId,
                ROYALTY_OWNER_BA_ID = request.RoyaltyOwnerBaId,
                INTEREST_TYPE = request.InterestType,
                INTEREST_PERCENTAGE = request.InterestPercentage,
                ROYALTY_RATE = request.RoyaltyRate ?? request.InterestPercentage,
                EFFECTIVE_DATE = request.EffectiveDate,
                EXPIRY_DATE = request.ExpiryDate,
                DIVISION_ORDER_ID = request.DivisionOrderId,
                ACTIVE_IND = "Y"
            };

            if (interest is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(interest);
            _logger?.LogDebug("Registered royalty interest for property {PropertyId}", request.PropertyId);

            return interest;
        }

        /// <summary>
        /// Gets a royalty interest by ID.
        /// </summary>
        public async Task<ROYALTY_INTEREST?> GetRoyaltyInterestAsync(string interestId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(interestId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetRoyaltyInterestRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROYALTY_INTEREST_ID", Operator = "=", FilterValue = interestId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<ROYALTY_INTEREST>().FirstOrDefault();
        }

        /// <summary>
        /// Gets royalty interests by property.
        /// </summary>
        public async Task<List<ROYALTY_INTEREST>> GetRoyaltyInterestsByPropertyAsync(string propertyId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return new List<ROYALTY_INTEREST>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetRoyaltyInterestRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<ROYALTY_INTEREST>().OrderByDescending(i => i.EFFECTIVE_DATE).ToList();
        }

        /// <summary>
        /// Calculates and creates a royalty payment.
        /// </summary>
        public async Task<ROYALTY_PAYMENT> CalculateAndCreatePaymentAsync(
            string revenueTransactionId,
            string royaltyOwnerBaId,
            decimal royaltyInterest,
            DateTime paymentDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(revenueTransactionId))
                throw new ArgumentException("Revenue transaction ID is required.", nameof(revenueTransactionId));
            if (royaltyInterest < 0 || royaltyInterest > 1)
                throw new ArgumentException("Royalty interest must be between 0 and 1.", nameof(royaltyInterest));

            var connName = connectionName ?? _connectionName;

            // In a full implementation, would retrieve SalesTransaction from revenue module
            // For now, create payment with basic information
            var payment = new ROYALTY_PAYMENT
            {
                ROYALTY_PAYMENT_ID = Guid.NewGuid().ToString(),
                REVENUE_TRANSACTION_ID = revenueTransactionId,
                ROYALTY_OWNER_BA_ID = royaltyOwnerBaId,
                PAYMENT_DATE = paymentDate,
                PAYMENT_PERIOD_START = paymentDate,
                PAYMENT_PERIOD_END = paymentDate,
                ROYALTY_AMOUNT = 0m, // Would be calculated from transaction
                NET_PAYMENT_AMOUNT = 0m, // Would be calculated after withholdings
                PAYMENT_STATUS = "Pending",
                ACTIVE_IND = "Y"
            };

            if (payment is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            var repo = await GetRoyaltyPaymentRepositoryAsync(connName);
            await repo.InsertAsync(payment);

            _logger?.LogDebug("Created royalty payment {PaymentId} for transaction {TransactionId}", payment.ROYALTY_PAYMENT_ID, revenueTransactionId);

            return payment;
        }

        /// <summary>
        /// Gets a royalty payment by ID.
        /// </summary>
        public async Task<ROYALTY_PAYMENT?> GetRoyaltyPaymentAsync(string paymentId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(paymentId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetRoyaltyPaymentRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROYALTY_PAYMENT_ID", Operator = "=", FilterValue = paymentId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<ROYALTY_PAYMENT>().FirstOrDefault();
        }

        /// <summary>
        /// Gets royalty payments by owner.
        /// </summary>
        public async Task<List<ROYALTY_PAYMENT>> GetRoyaltyPaymentsByOwnerAsync(string ownerId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(ownerId))
                return new List<ROYALTY_PAYMENT>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetRoyaltyPaymentRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROYALTY_OWNER_BA_ID", Operator = "=", FilterValue = ownerId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "PAYMENT_DATE", Operator = ">=", FilterValue = startDate.Value });
            }

            if (endDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "PAYMENT_DATE", Operator = "<=", FilterValue = endDate.Value });
            }

            var results = await repo.GetAsync(filters);
            return results.Cast<ROYALTY_PAYMENT>().OrderByDescending(p => p.PAYMENT_DATE).ToList();
        }

        /// <summary>
        /// Creates a royalty statement.
        /// </summary>
        public async Task<ROYALTY_STATEMENT> CreateStatementAsync(CreateRoyaltyStatementRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;

            // Get payments for the period
            var payments = await GetRoyaltyPaymentsByOwnerAsync(request.RoyaltyOwnerBaId, request.PeriodStart, request.PeriodEnd, connName);
            var totalRoyaltyAmount = payments.Sum(p => p.ROYALTY_AMOUNT ?? 0m);
            var totalDeductions = 0m; // Would be calculated from payments
            var netPaymentAmount = totalRoyaltyAmount - totalDeductions;

            var statement = new ROYALTY_STATEMENT
            {
                ROYALTY_STATEMENT_ID = Guid.NewGuid().ToString(),
                ROYALTY_OWNER_BA_ID = request.RoyaltyOwnerBaId,
                PROPERTY_ID = request.PropertyId,
                STATEMENT_PERIOD_START = request.PeriodStart,
                STATEMENT_PERIOD_END = request.PeriodEnd,
                TOTAL_ROYALTY_AMOUNT = totalRoyaltyAmount,
                TOTAL_DEDUCTIONS = totalDeductions,
                NET_PAYMENT_AMOUNT = netPaymentAmount,
                STATEMENT_DATE = DateTime.UtcNow,
                STATUS = "Generated",
                ACTIVE_IND = "Y"
            };

            if (statement is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            var repo = await GetRoyaltyStatementRepositoryAsync(connName);
            await repo.InsertAsync(statement);

            _logger?.LogDebug("Created royalty statement {StatementId} for owner {OwnerId}", statement.ROYALTY_STATEMENT_ID, request.RoyaltyOwnerBaId);

            return statement;
        }

        /// <summary>
        /// Gets a royalty statement by ID.
        /// </summary>
        public async Task<ROYALTY_STATEMENT?> GetStatementAsync(string statementId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(statementId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetRoyaltyStatementRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROYALTY_STATEMENT_ID", Operator = "=", FilterValue = statementId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<ROYALTY_STATEMENT>().FirstOrDefault();
        }

        /// <summary>
        /// Gets royalty owner summary.
        /// </summary>
        public async Task<List<RoyaltyOwnerSummary>> GetRoyaltyOwnerSummaryAsync(string ownerId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(ownerId))
                return new List<RoyaltyOwnerSummary>();

            var connName = connectionName ?? _connectionName;
            var interestRepo = await GetRoyaltyInterestRepositoryAsync(connName);
            var paymentRepo = await GetRoyaltyPaymentRepositoryAsync(connName);

            var interestFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROYALTY_OWNER_BA_ID", Operator = "=", FilterValue = ownerId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var interests = await interestRepo.GetAsync(interestFilters);
            var interestList = interests.Cast<ROYALTY_INTEREST>().ToList();

            var paymentFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROYALTY_OWNER_BA_ID", Operator = "=", FilterValue = ownerId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var payments = await paymentRepo.GetAsync(paymentFilters);
            var paymentList = payments.Cast<ROYALTY_PAYMENT>().ToList();

            var summary = new RoyaltyOwnerSummary
            {
                OwnerBaId = ownerId,
                OwnerName = ownerId, // Would lookup from BUSINESS_ASSOCIATE
                PropertyCount = interestList.Select(i => i.PROPERTY_ID).Distinct().Count(),
                TotalRoyaltyInterest = interestList.Sum(i => i.INTEREST_PERCENTAGE ?? 0m),
                TotalRoyaltyPaid = paymentList.Where(p => p.PAYMENT_STATUS == "Paid").Sum(p => p.ROYALTY_AMOUNT ?? 0m),
                TotalRoyaltyPending = paymentList.Where(p => p.PAYMENT_STATUS == "Pending").Sum(p => p.ROYALTY_AMOUNT ?? 0m),
                LastPaymentDate = paymentList.Where(p => p.PAYMENT_STATUS == "Paid").OrderByDescending(p => p.PAYMENT_DATE).FirstOrDefault()?.PAYMENT_DATE
            };

            return new List<RoyaltyOwnerSummary> { summary };
        }

        /// <summary>
        /// Approves a royalty payment.
        /// </summary>
        public async Task<RoyaltyPaymentApprovalResult> ApprovePaymentAsync(string paymentId, string approverId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(paymentId))
                throw new ArgumentException("Payment ID is required.", nameof(paymentId));
            if (string.IsNullOrEmpty(approverId))
                throw new ArgumentException("Approver ID is required.", nameof(approverId));

            var connName = connectionName ?? _connectionName;
            var payment = await GetRoyaltyPaymentAsync(paymentId, connName);

            if (payment == null)
                throw new InvalidOperationException($"Royalty payment {paymentId} not found.");

            payment.PAYMENT_STATUS = "Approved";
            payment.APPROVED_BY = approverId;
            payment.APPROVAL_DATE = DateTime.UtcNow;

            if (payment is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, approverId, connName);
            }

            var repo = await GetRoyaltyPaymentRepositoryAsync(connName);
            await repo.UpdateAsync(payment);

            _logger?.LogDebug("Approved royalty payment {PaymentId} by {ApproverId}", paymentId, approverId);

            return new RoyaltyPaymentApprovalResult
            {
                PaymentId = paymentId,
                IsApproved = true,
                ApproverId = approverId,
                ApprovalDate = DateTime.UtcNow,
                Status = "Approved"
            };
        }

        /// <summary>
        /// Gets royalty audit trail.
        /// </summary>
        public async Task<List<RoyaltyAuditTrail>> GetRoyaltyAuditTrailAsync(string interestId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(interestId))
                return new List<RoyaltyAuditTrail>();

            var connName = connectionName ?? _connectionName;
            var interest = await GetRoyaltyInterestAsync(interestId, connName);

            if (interest == null)
                return new List<RoyaltyAuditTrail>();

            // In a full implementation, would track all changes in an audit table
            // For now, return basic audit information from the interest entity
            var auditTrail = new List<RoyaltyAuditTrail>();

            if (interest.ROW_CREATED_DATE.HasValue)
            {
                auditTrail.Add(new RoyaltyAuditTrail
                {
                    AuditId = Guid.NewGuid().ToString(),
                    InterestId = interestId,
                    ChangeType = "Created",
                    ChangeDate = interest.ROW_CREATED_DATE.Value,
                    ChangedBy = interest.ROW_CREATED_BY ?? string.Empty,
                    Description = "Royalty interest created"
                });
            }

            if (interest.ROW_CHANGED_DATE.HasValue)
            {
                auditTrail.Add(new RoyaltyAuditTrail
                {
                    AuditId = Guid.NewGuid().ToString(),
                    InterestId = interestId,
                    ChangeType = "Modified",
                    ChangeDate = interest.ROW_CHANGED_DATE.Value,
                    ChangedBy = interest.ROW_CHANGED_BY ?? string.Empty,
                    Description = "Royalty interest modified"
                });
            }

            return auditTrail.OrderByDescending(a => a.ChangeDate).ToList();
        }

        // Repository helper methods
        private async Task<PPDMGenericRepository> GetRoyaltyInterestRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(ROYALTY_INTEREST_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Royalty.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(ROYALTY_INTEREST);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, ROYALTY_INTEREST_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetRoyaltyPaymentRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(ROYALTY_PAYMENT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Royalty.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(ROYALTY_PAYMENT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, ROYALTY_PAYMENT_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetRoyaltyStatementRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(ROYALTY_STATEMENT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Royalty.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(ROYALTY_STATEMENT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, ROYALTY_STATEMENT_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetRoyaltyCalculationRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(ROYALTY_CALCULATION_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Royalty.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(ROYALTY_CALCULATION);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, ROYALTY_CALCULATION_TABLE,
                null);
        }
    }
}
