using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.Ownership
{
    /// <summary>
    /// Manages ownership, division orders, and transfers.
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
    /// </summary>
    public class OwnershipManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<OwnershipManager>? _logger;
        private readonly string _connectionName;
        private const string DIVISION_ORDER_TABLE = "DIVISION_ORDER";
        private const string TRANSFER_ORDER_TABLE = "TRANSFER_ORDER";
        private const string OWNERSHIP_INTEREST_TABLE = "OWNERSHIP_INTEREST";

        public OwnershipManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<OwnershipManager>? logger = null,
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
        /// Creates a division order.
        /// </summary>
        public async Task<DIVISION_ORDER> CreateDivisionOrderAsync(
            string propertyOrLeaseId,
            OWNER_INFORMATION owner,
            decimal workingInterest,
            decimal netRevenueInterest,
            DateTime effectiveDate,
            string userId = "system",
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyOrLeaseId))
                throw new ArgumentException("Property or lease ID cannot be null or empty.", nameof(propertyOrLeaseId));

            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            if (workingInterest < 0 || workingInterest > 1)
                throw new ArgumentException("Working interest must be between 0 and 1.", nameof(workingInterest));

            if (netRevenueInterest < 0 || netRevenueInterest > 1)
                throw new ArgumentException("Net revenue interest must be between 0 and 1.", nameof(netRevenueInterest));

            if (netRevenueInterest > workingInterest)
                throw new ArgumentException("Net revenue interest cannot exceed working interest.", nameof(netRevenueInterest));

            var divisionOrder = new DIVISION_ORDER
            {
                DIVISION_ORDER_ID = Guid.NewGuid().ToString(),
                PROPERTY_OR_LEASE_ID = propertyOrLeaseId,
                OWNER_ID = owner.OWNER_INFORMATION_ID ?? string.Empty,
                WORKING_INTEREST = workingInterest,
                NET_REVENUE_INTEREST = netRevenueInterest,
                EFFECTIVE_DATE = effectiveDate,
                STATUS = DivisionOrderStatus.Pending.ToString()
            };

            // Prepare for insert and save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(divisionOrder, userId);
            var result = dataSource.InsertEntity(DIVISION_ORDER_TABLE, divisionOrder);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create division order {OrderId}: {Error}", divisionOrder.DIVISION_ORDER_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save division order: {errorMessage}");
            }

            _logger?.LogDebug("Created division order {OrderId} in database", divisionOrder.DIVISION_ORDER_ID);
            return divisionOrder;
        }

        /// <summary>
        /// Creates a division order (synchronous wrapper).
        /// </summary>
        public DIVISION_ORDER CreateDivisionOrder(
            string propertyOrLeaseId,
            OWNER_INFORMATION owner,
            decimal workingInterest,
            decimal netRevenueInterest,
            DateTime effectiveDate)
        {
            return CreateDivisionOrderAsync(propertyOrLeaseId, owner, workingInterest, netRevenueInterest, effectiveDate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Approves a division order.
        /// </summary>
        public async Task ApproveDivisionOrderAsync(string divisionOrderId, string approvedBy, string? connectionName = null)
        {
            var divisionOrder = await GetDivisionOrderAsync(divisionOrderId, connectionName);
            if (divisionOrder == null)
                throw new ArgumentException($"Division order {divisionOrderId} not found.", nameof(divisionOrderId));

            divisionOrder.STATUS = DivisionOrderStatus.Approved.ToString();
            divisionOrder.APPROVAL_DATE = DateTime.Now;
            divisionOrder.APPROVED_BY = approvedBy;

            // Update division order in database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForUpdate(divisionOrder, approvedBy);
            var updateFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DIVISION_ORDER_ID", Operator = "=", FilterValue = divisionOrderId }
            };
            dataSource.UpdateEntity(DIVISION_ORDER_TABLE, divisionOrder);

            // Create ownership interest
            var ownershipInterest = new OWNERSHIP_INTEREST
            {
                OWNERSHIP_ID = Guid.NewGuid().ToString(),
                OWNER_ID = divisionOrder.OWNER_ID,
                PROPERTY_OR_LEASE_ID = divisionOrder.PROPERTY_OR_LEASE_ID,
                WORKING_INTEREST = divisionOrder.WORKING_INTEREST,
                NET_REVENUE_INTEREST = divisionOrder.NET_REVENUE_INTEREST,
                ROYALTY_INTEREST = divisionOrder.ROYALTY_INTEREST,
                OVERRIDING_ROYALTY_INTEREST = divisionOrder.OVERRIDING_ROYALTY_INTEREST,
                EFFECTIVE_START_DATE = divisionOrder.EFFECTIVE_DATE,
                EFFECTIVE_END_DATE = divisionOrder.EXPIRATION_DATE,
                DIVISION_ORDER_ID = divisionOrderId
            };

            _commonColumnHandler.PrepareForInsert(ownershipInterest, approvedBy);
            dataSource.InsertEntity(OWNERSHIP_INTEREST_TABLE, ownershipInterest);

            _logger?.LogDebug("Approved division order {OrderId} and created ownership interest {InterestId}", divisionOrderId, ownershipInterest.OWNERSHIP_ID);
        }

        /// <summary>
        /// Approves a division order (synchronous wrapper).
        /// </summary>
        public void ApproveDivisionOrder(string divisionOrderId, string approvedBy)
        {
            ApproveDivisionOrderAsync(divisionOrderId, approvedBy).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a transfer order.
        /// </summary>
        public async Task<TRANSFER_ORDER> CreateTransferOrderAsync(
            string propertyOrLeaseId,
            OWNER_INFORMATION fromOwner,
            OWNER_INFORMATION toOwner,
            decimal interestTransferred,
            DateTime effectiveDate,
            string userId = "system",
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyOrLeaseId))
                throw new ArgumentException("Property or lease ID cannot be null or empty.", nameof(propertyOrLeaseId));

            if (interestTransferred < 0 || interestTransferred > 1)
                throw new ArgumentException("Interest transferred must be between 0 and 1.", nameof(interestTransferred));

            var transferOrder = new TRANSFER_ORDER
            {
                TRANSFER_ORDER_ID = Guid.NewGuid().ToString(),
                PROPERTY_OR_LEASE_ID = propertyOrLeaseId,
                FROM_OWNER_ID = fromOwner?.OWNER_INFORMATION_ID ?? string.Empty,
                TO_OWNER_ID = toOwner?.OWNER_INFORMATION_ID ?? string.Empty,
                INTEREST_TRANSFERRED = interestTransferred,
                EFFECTIVE_DATE = effectiveDate,
                IS_APPROVED = "N"
            };

            // Prepare for insert and save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(transferOrder, userId);
            var result = dataSource.InsertEntity(TRANSFER_ORDER_TABLE, transferOrder);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create transfer order {OrderId}: {Error}", transferOrder.TRANSFER_ORDER_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save transfer order: {errorMessage}");
            }

            _logger?.LogDebug("Created transfer order {OrderId} in database", transferOrder.TRANSFER_ORDER_ID);
            return transferOrder;
        }

        /// <summary>
        /// Creates a transfer order (synchronous wrapper).
        /// </summary>
        public TRANSFER_ORDER CreateTransferOrder(
            string propertyOrLeaseId,
            OWNER_INFORMATION fromOwner,
            OWNER_INFORMATION toOwner,
            decimal interestTransferred,
            DateTime effectiveDate)
        {
            return CreateTransferOrderAsync(propertyOrLeaseId, fromOwner, toOwner, interestTransferred, effectiveDate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Approves a transfer order and updates ownership.
        /// </summary>
        public async Task ApproveTransferOrderAsync(string transferOrderId, string approvedBy, string? connectionName = null)
        {
            var transferOrder = await GetTransferOrderAsync(transferOrderId, connectionName);
            if (transferOrder == null)
                throw new ArgumentException($"Transfer order {transferOrderId} not found.", nameof(transferOrderId));

            transferOrder.IS_APPROVED = "Y";
            transferOrder.APPROVAL_DATE = DateTime.Now;
            transferOrder.APPROVED_BY = approvedBy;

            // Update transfer order in database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForUpdate(transferOrder, approvedBy);
            var updateFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TRANSFER_ORDER_ID", Operator = "=", FilterValue = transferOrderId }
            };
            dataSource.UpdateEntity(TRANSFER_ORDER_TABLE, transferOrder);

            // Update ownership interests
            var fromInterests = await GetOwnershipInterestsAsync(transferOrder.PROPERTY_OR_LEASE_ID, transferOrder.EFFECTIVE_DATE ?? DateTime.MinValue, connName);
            var relevantInterests = fromInterests
                .Where(o => o.OWNER_ID == transferOrder.FROM_OWNER_ID &&
                           (o.EFFECTIVE_END_DATE == null || o.EFFECTIVE_END_DATE >= transferOrder.EFFECTIVE_DATE))
                .ToList();

            foreach (var interest in relevantInterests)
            {
                // End the from owner's interest
                interest.EFFECTIVE_END_DATE = transferOrder.EFFECTIVE_DATE;
                _commonColumnHandler.PrepareForUpdate(interest, approvedBy);
                var interestFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "OWNERSHIP_ID", Operator = "=", FilterValue = interest.OWNERSHIP_ID }
                };
                dataSource.UpdateEntity(OWNERSHIP_INTEREST_TABLE, interest);

                // Create new interest for to owner
                var newInterest = new OWNERSHIP_INTEREST
                {
                    OWNERSHIP_ID = Guid.NewGuid().ToString(),
                    OWNER_ID = transferOrder.TO_OWNER_ID,
                    PROPERTY_OR_LEASE_ID = transferOrder.PROPERTY_OR_LEASE_ID,
                    WORKING_INTEREST = (interest.WORKING_INTEREST ?? 0m) * (transferOrder.INTEREST_TRANSFERRED ?? 0m),
                    NET_REVENUE_INTEREST = (interest.NET_REVENUE_INTEREST ?? 0m) * (transferOrder.INTEREST_TRANSFERRED ?? 0m),
                    EFFECTIVE_START_DATE = transferOrder.EFFECTIVE_DATE
                };

                _commonColumnHandler.PrepareForInsert(newInterest, approvedBy);
                dataSource.InsertEntity(OWNERSHIP_INTEREST_TABLE, newInterest);
            }

            _logger?.LogDebug("Approved transfer order {OrderId} and updated ownership interests", transferOrderId);
        }

        /// <summary>
        /// Approves a transfer order and updates ownership (synchronous wrapper).
        /// </summary>
        public void ApproveTransferOrder(string transferOrderId, string approvedBy)
        {
            ApproveTransferOrderAsync(transferOrderId, approvedBy).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Calculates total working interest for a property or lease.
        /// </summary>
        public decimal CalculateTotalWorkingInterest(string propertyOrLeaseId, DateTime asOfDate)
        {
            return GetOwnershipInterests(propertyOrLeaseId, asOfDate)
                .Sum(o => o.WORKING_INTEREST ?? 0m);
        }

        /// <summary>
        /// Calculates total net revenue interest for a property or lease.
        /// </summary>
        public decimal CalculateTotalNetRevenueInterest(string propertyOrLeaseId, DateTime asOfDate)
        {
            return GetOwnershipInterests(propertyOrLeaseId, asOfDate)
                .Sum(o => o.NET_REVENUE_INTEREST ?? 0m);
        }

        /// <summary>
        /// Gets a division order by ID.
        /// </summary>
        public async Task<DIVISION_ORDER?> GetDivisionOrderAsync(string divisionOrderId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(divisionOrderId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DIVISION_ORDER_ID", Operator = "=", FilterValue = divisionOrderId }
            };

            var results = await dataSource.GetEntityAsync(DIVISION_ORDER_TABLE, filters);
            return results?.FirstOrDefault() as DIVISION_ORDER;
        }

        /// <summary>
        /// Gets a division order by ID (synchronous wrapper).
        /// </summary>
        public DIVISION_ORDER? GetDivisionOrder(string divisionOrderId)
        {
            return GetDivisionOrderAsync(divisionOrderId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a transfer order by ID.
        /// </summary>
        public async Task<TRANSFER_ORDER?> GetTransferOrderAsync(string transferOrderId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(transferOrderId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TRANSFER_ORDER_ID", Operator = "=", FilterValue = transferOrderId }
            };

            var results = await dataSource.GetEntityAsync(TRANSFER_ORDER_TABLE, filters);
            return results?.FirstOrDefault() as TRANSFER_ORDER;
        }

        /// <summary>
        /// Gets a transfer order by ID (synchronous wrapper).
        /// </summary>
        public TRANSFER_ORDER? GetTransferOrder(string transferOrderId)
        {
            return GetTransferOrderAsync(transferOrderId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets ownership interests for a property or lease.
        /// </summary>
        public async Task<IEnumerable<OWNERSHIP_INTEREST>> GetOwnershipInterestsAsync(string propertyOrLeaseId, DateTime asOfDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyOrLeaseId))
                return Enumerable.Empty<OWNERSHIP_INTEREST>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = propertyOrLeaseId },
                new AppFilter { FieldName = "EFFECTIVE_START_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") }
            };

            var results = await dataSource.GetEntityAsync(OWNERSHIP_INTEREST_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<OWNERSHIP_INTEREST>();

            return results.Cast<OWNERSHIP_INTEREST>()
                .Where(o => o != null && o.EFFECTIVE_START_DATE <= asOfDate && (o.EFFECTIVE_END_DATE == null || o.EFFECTIVE_END_DATE >= asOfDate))!;
        }

        /// <summary>
        /// Gets ownership interests for a property or lease (synchronous wrapper).
        /// </summary>
        public IEnumerable<OWNERSHIP_INTEREST> GetOwnershipInterests(string propertyOrLeaseId, DateTime asOfDate)
        {
            return GetOwnershipInterestsAsync(propertyOrLeaseId, asOfDate).GetAwaiter().GetResult();
        }

        #endregion
    }
}