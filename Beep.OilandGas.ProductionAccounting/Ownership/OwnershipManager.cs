namespace Beep.OilandGas.ProductionAccounting.Ownership
{
    /// <summary>
    /// Manages ownership, division orders, and transfers.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
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
        public async Task<DivisionOrder> CreateDivisionOrderAsync(
            string propertyOrLeaseId,
            OwnerInformation owner,
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

            var divisionOrder = new DivisionOrder
            {
                DivisionOrderId = Guid.NewGuid().ToString(),
                PropertyOrLeaseId = propertyOrLeaseId,
                Owner = owner,
                WorkingInterest = workingInterest,
                NetRevenueInterest = netRevenueInterest,
                EffectiveDate = effectiveDate,
                Status = DivisionOrderStatus.Pending
            };

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var orderData = ConvertDivisionOrderToDictionary(divisionOrder);
            var result = dataSource.InsertEntity(DIVISION_ORDER_TABLE, orderData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create division order {OrderId}: {Error}", divisionOrder.DivisionOrderId, errorMessage);
                throw new InvalidOperationException($"Failed to save division order: {errorMessage}");
            }

            _logger?.LogDebug("Created division order {OrderId} in database", divisionOrder.DivisionOrderId);
            return divisionOrder;
        }

        /// <summary>
        /// Creates a division order (synchronous wrapper).
        /// </summary>
        public DivisionOrder CreateDivisionOrder(
            string propertyOrLeaseId,
            OwnerInformation owner,
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

            divisionOrder.Status = DivisionOrderStatus.Approved;
            divisionOrder.ApprovalDate = DateTime.Now;
            divisionOrder.ApprovedBy = approvedBy;

            // Update division order in database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var orderData = ConvertDivisionOrderToDictionary(divisionOrder);
            var updateFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DIVISION_ORDER_ID", Operator = "=", FilterValue = divisionOrderId }
            };
            dataSource.UpdateEntity(DIVISION_ORDER_TABLE, orderData);

            // Create ownership interest
            var ownershipInterest = new OwnershipInterest
            {
                OwnershipId = Guid.NewGuid().ToString(),
                OwnerId = divisionOrder.Owner.OwnerId,
                PropertyOrLeaseId = divisionOrder.PropertyOrLeaseId,
                WorkingInterest = divisionOrder.WorkingInterest,
                NetRevenueInterest = divisionOrder.NetRevenueInterest,
                RoyaltyInterest = divisionOrder.RoyaltyInterest,
                OverridingRoyaltyInterest = divisionOrder.OverridingRoyaltyInterest,
                EffectiveStartDate = divisionOrder.EffectiveDate,
                EffectiveEndDate = divisionOrder.ExpirationDate,
                DivisionOrderId = divisionOrderId
            };

            dataSource.InsertEntity(OWNERSHIP_INTEREST_TABLE, ownershipInterest);

            _logger?.LogDebug("Approved division order {OrderId} and created ownership interest {InterestId}", divisionOrderId, ownershipInterest.OwnershipId);
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
        public async Task<TransferOrder> CreateTransferOrderAsync(
            string propertyOrLeaseId,
            OwnerInformation fromOwner,
            OwnerInformation toOwner,
            decimal interestTransferred,
            DateTime effectiveDate,
            string userId = "system",
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyOrLeaseId))
                throw new ArgumentException("Property or lease ID cannot be null or empty.", nameof(propertyOrLeaseId));

            if (interestTransferred < 0 || interestTransferred > 1)
                throw new ArgumentException("Interest transferred must be between 0 and 1.", nameof(interestTransferred));

            var transferOrder = new TransferOrder
            {
                TransferOrderId = Guid.NewGuid().ToString(),
                PropertyOrLeaseId = propertyOrLeaseId,
                FromOwner = fromOwner,
                ToOwner = toOwner,
                InterestTransferred = interestTransferred,
                EffectiveDate = effectiveDate,
                IsApproved = false
            };

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var orderData = ConvertTransferOrderToDictionary(transferOrder);
            var result = dataSource.InsertEntity(TRANSFER_ORDER_TABLE, orderData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create transfer order {OrderId}: {Error}", transferOrder.TransferOrderId, errorMessage);
                throw new InvalidOperationException($"Failed to save transfer order: {errorMessage}");
            }

            _logger?.LogDebug("Created transfer order {OrderId} in database", transferOrder.TransferOrderId);
            return transferOrder;
        }

        /// <summary>
        /// Creates a transfer order (synchronous wrapper).
        /// </summary>
        public TransferOrder CreateTransferOrder(
            string propertyOrLeaseId,
            OwnerInformation fromOwner,
            OwnerInformation toOwner,
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

            transferOrder.IsApproved = true;
            transferOrder.ApprovalDate = DateTime.Now;
            transferOrder.ApprovedBy = approvedBy;

            // Update transfer order in database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var orderData = ConvertTransferOrderToDictionary(transferOrder);
            var updateFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TRANSFER_ORDER_ID", Operator = "=", FilterValue = transferOrderId }
            };
            dataSource.UpdateEntity(TRANSFER_ORDER_TABLE, orderData);

            // Update ownership interests
            var fromInterests = await GetOwnershipInterestsAsync(transferOrder.PropertyOrLeaseId, transferOrder.EffectiveDate, connName);
            var relevantInterests = fromInterests
                .Where(o => o.OwnerId == transferOrder.FromOwner.OwnerId &&
                           (o.EffectiveEndDate == null || o.EffectiveEndDate >= transferOrder.EffectiveDate))
                .ToList();

            foreach (var interest in relevantInterests)
            {
                // End the from owner's interest
                interest.EffectiveEndDate = transferOrder.EffectiveDate;
                var interestFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "OWNERSHIP_ID", Operator = "=", FilterValue = interest.OwnershipId }
                };
                dataSource.UpdateEntity(OWNERSHIP_INTEREST_TABLE, interest);

                // Create new interest for to owner
                var newInterest = new OwnershipInterest
                {
                    OwnershipId = Guid.NewGuid().ToString(),
                    OwnerId = transferOrder.ToOwner.OwnerId,
                    PropertyOrLeaseId = transferOrder.PropertyOrLeaseId,
                    WorkingInterest = interest.WorkingInterest * transferOrder.InterestTransferred,
                    NetRevenueInterest = interest.NetRevenueInterest * transferOrder.InterestTransferred,
                    EffectiveStartDate = transferOrder.EffectiveDate
                };

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
                .Sum(o => o.WorkingInterest);
        }

        /// <summary>
        /// Calculates total net revenue interest for a property or lease.
        /// </summary>
        public decimal CalculateTotalNetRevenueInterest(string propertyOrLeaseId, DateTime asOfDate)
        {
            return GetOwnershipInterests(propertyOrLeaseId, asOfDate)
                .Sum(o => o.NetRevenueInterest);
        }

        /// <summary>
        /// Gets a division order by ID.
        /// </summary>
        public async Task<DivisionOrder?> GetDivisionOrderAsync(string divisionOrderId, string? connectionName = null)
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
            var orderData = results?.FirstOrDefault();
            
            if (orderData == null)
                return null;

            return orderData as DivisionOrder;
        }

        /// <summary>
        /// Gets a division order by ID (synchronous wrapper).
        /// </summary>
        public DivisionOrder? GetDivisionOrder(string divisionOrderId)
        {
            return GetDivisionOrderAsync(divisionOrderId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a transfer order by ID.
        /// </summary>
        public async Task<TransferOrder?> GetTransferOrderAsync(string transferOrderId, string? connectionName = null)
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
            var orderData = results?.FirstOrDefault();
            
            if (orderData == null)
                return null;

            return orderData as TransferOrder;
        }

        /// <summary>
        /// Gets a transfer order by ID (synchronous wrapper).
        /// </summary>
        public TransferOrder? GetTransferOrder(string transferOrderId)
        {
            return GetTransferOrderAsync(transferOrderId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets ownership interests for a property or lease.
        /// </summary>
        public async Task<IEnumerable<OwnershipInterest>> GetOwnershipInterestsAsync(string propertyOrLeaseId, DateTime asOfDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyOrLeaseId))
                return Enumerable.Empty<OwnershipInterest>();

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
                return Enumerable.Empty<OwnershipInterest>();

            return results.Cast<OwnershipInterest>()
                .Where(o => o != null && o.EffectiveStartDate <= asOfDate && (o.EffectiveEndDate == null || o.EffectiveEndDate >= asOfDate))!;
        }

        /// <summary>
        /// Gets ownership interests for a property or lease (synchronous wrapper).
        /// </summary>
        public IEnumerable<OwnershipInterest> GetOwnershipInterests(string propertyOrLeaseId, DateTime asOfDate)
        {
            return GetOwnershipInterestsAsync(propertyOrLeaseId, asOfDate).GetAwaiter().GetResult();
        }

        #region Helper Methods - Model to Dictionary Conversion

        private Dictionary<string, object> ConvertDivisionOrderToDictionary(DivisionOrder order)
        {
            return new Dictionary<string, object>
            {
                { "DIVISION_ORDER_ID", order.DivisionOrderId },
                { "PROPERTY_OR_LEASE_ID", order.PropertyOrLeaseId },
                { "OWNER_ID", order.Owner?.OwnerId ?? string.Empty },
                { "OWNER_NAME", order.Owner?.OwnerName ?? string.Empty },
                { "WORKING_INTEREST", order.WorkingInterest },
                { "NET_REVENUE_INTEREST", order.NetRevenueInterest },
                { "ROYALTY_INTEREST", order.RoyaltyInterest ?? (object)DBNull.Value },
                { "OVERRIDING_ROYALTY_INTEREST", order.OverridingRoyaltyInterest ?? (object)DBNull.Value },
                { "EFFECTIVE_DATE", order.EffectiveDate },
                { "EXPIRATION_DATE", order.ExpirationDate ?? (object)DBNull.Value },
                { "STATUS", order.Status.ToString() },
                { "APPROVAL_DATE", order.ApprovalDate ?? (object)DBNull.Value },
                { "APPROVED_BY", order.ApprovedBy ?? string.Empty }
            };
        }

        private DivisionOrder? ConvertDictionaryToDivisionOrder(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("DIVISION_ORDER_ID"))
                return null;

            var order = new DivisionOrder
            {
                DivisionOrderId = dict["DIVISION_ORDER_ID"]?.ToString() ?? string.Empty,
                PropertyOrLeaseId = dict.ContainsKey("PROPERTY_OR_LEASE_ID") ? dict["PROPERTY_OR_LEASE_ID"]?.ToString() ?? string.Empty : string.Empty,
                WorkingInterest = dict.ContainsKey("WORKING_INTEREST") ? Convert.ToDecimal(dict["WORKING_INTEREST"]) : 0m,
                NetRevenueInterest = dict.ContainsKey("NET_REVENUE_INTEREST") ? Convert.ToDecimal(dict["NET_REVENUE_INTEREST"]) : 0m,
                EffectiveDate = dict.ContainsKey("EFFECTIVE_DATE") && dict["EFFECTIVE_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["EFFECTIVE_DATE"])
                    : DateTime.MinValue,
                ExpirationDate = dict.ContainsKey("EXPIRATION_DATE") && dict["EXPIRATION_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["EXPIRATION_DATE"])
                    : null,
                ApprovalDate = dict.ContainsKey("APPROVAL_DATE") && dict["APPROVAL_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["APPROVAL_DATE"])
                    : null,
                ApprovedBy = dict.ContainsKey("APPROVED_BY") ? dict["APPROVED_BY"]?.ToString() : null
            };

            if (dict.ContainsKey("ROYALTY_INTEREST") && dict["ROYALTY_INTEREST"] != DBNull.Value)
                order.RoyaltyInterest = Convert.ToDecimal(dict["ROYALTY_INTEREST"]);
            if (dict.ContainsKey("OVERRIDING_ROYALTY_INTEREST") && dict["OVERRIDING_ROYALTY_INTEREST"] != DBNull.Value)
                order.OverridingRoyaltyInterest = Convert.ToDecimal(dict["OVERRIDING_ROYALTY_INTEREST"]);
            if (dict.ContainsKey("STATUS") && Enum.TryParse<DivisionOrderStatus>(dict["STATUS"]?.ToString(), out var status))
                order.Status = status;

            order.Owner = new OwnerInformation
            {
                OwnerId = dict.ContainsKey("OWNER_ID") ? dict["OWNER_ID"]?.ToString() ?? string.Empty : string.Empty,
                OwnerName = dict.ContainsKey("OWNER_NAME") ? dict["OWNER_NAME"]?.ToString() ?? string.Empty : string.Empty
            };

            return order;
        }

        private Dictionary<string, object> ConvertTransferOrderToDictionary(TransferOrder order)
        {
            return new Dictionary<string, object>
            {
                { "TRANSFER_ORDER_ID", order.TransferOrderId },
                { "PROPERTY_OR_LEASE_ID", order.PropertyOrLeaseId },
                { "FROM_OWNER_ID", order.FromOwner?.OwnerId ?? string.Empty },
                { "TO_OWNER_ID", order.ToOwner?.OwnerId ?? string.Empty },
                { "INTEREST_TRANSFERRED", order.InterestTransferred },
                { "EFFECTIVE_DATE", order.EffectiveDate },
                { "IS_APPROVED", order.IsApproved },
                { "APPROVAL_DATE", order.ApprovalDate ?? (object)DBNull.Value },
                { "APPROVED_BY", order.ApprovedBy ?? string.Empty }
            };
        }

        private TransferOrder? ConvertDictionaryToTransferOrder(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("TRANSFER_ORDER_ID"))
                return null;

            return new TransferOrder
            {
                TransferOrderId = dict["TRANSFER_ORDER_ID"]?.ToString() ?? string.Empty,
                PropertyOrLeaseId = dict.ContainsKey("PROPERTY_OR_LEASE_ID") ? dict["PROPERTY_OR_LEASE_ID"]?.ToString() ?? string.Empty : string.Empty,
                InterestTransferred = dict.ContainsKey("INTEREST_TRANSFERRED") ? Convert.ToDecimal(dict["INTEREST_TRANSFERRED"]) : 0m,
                EffectiveDate = dict.ContainsKey("EFFECTIVE_DATE") && dict["EFFECTIVE_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["EFFECTIVE_DATE"])
                    : DateTime.MinValue,
                IsApproved = dict.ContainsKey("IS_APPROVED") && Convert.ToBoolean(dict["IS_APPROVED"]),
                ApprovalDate = dict.ContainsKey("APPROVAL_DATE") && dict["APPROVAL_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["APPROVAL_DATE"])
                    : null,
                ApprovedBy = dict.ContainsKey("APPROVED_BY") ? dict["APPROVED_BY"]?.ToString() : null,
                FromOwner = new OwnerInformation
                {
                    OwnerId = dict.ContainsKey("FROM_OWNER_ID") ? dict["FROM_OWNER_ID"]?.ToString() ?? string.Empty : string.Empty
                },
                ToOwner = new OwnerInformation
                {
                    OwnerId = dict.ContainsKey("TO_OWNER_ID") ? dict["TO_OWNER_ID"]?.ToString() ?? string.Empty : string.Empty
                }
            };
        }

        private Dictionary<string, object> ConvertOwnershipInterestToDictionary(OwnershipInterest interest)
        {
            return new Dictionary<string, object>
            {
                { "OWNERSHIP_ID", interest.OwnershipId },
                { "OWNER_ID", interest.OwnerId },
                { "PROPERTY_OR_LEASE_ID", interest.PropertyOrLeaseId },
                { "WORKING_INTEREST", interest.WorkingInterest },
                { "NET_REVENUE_INTEREST", interest.NetRevenueInterest },
                { "ROYALTY_INTEREST", interest.RoyaltyInterest ?? (object)DBNull.Value },
                { "OVERRIDING_ROYALTY_INTEREST", interest.OverridingRoyaltyInterest ?? (object)DBNull.Value },
                { "EFFECTIVE_START_DATE", interest.EffectiveStartDate },
                { "EFFECTIVE_END_DATE", interest.EffectiveEndDate ?? (object)DBNull.Value },
                { "DIVISION_ORDER_ID", interest.DivisionOrderId ?? string.Empty }
            };
        }

        private OwnershipInterest? ConvertDictionaryToOwnershipInterest(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("OWNERSHIP_ID"))
                return null;

            var interest = new OwnershipInterest
            {
                OwnershipId = dict["OWNERSHIP_ID"]?.ToString() ?? string.Empty,
                OwnerId = dict.ContainsKey("OWNER_ID") ? dict["OWNER_ID"]?.ToString() ?? string.Empty : string.Empty,
                PropertyOrLeaseId = dict.ContainsKey("PROPERTY_OR_LEASE_ID") ? dict["PROPERTY_OR_LEASE_ID"]?.ToString() ?? string.Empty : string.Empty,
                WorkingInterest = dict.ContainsKey("WORKING_INTEREST") ? Convert.ToDecimal(dict["WORKING_INTEREST"]) : 0m,
                NetRevenueInterest = dict.ContainsKey("NET_REVENUE_INTEREST") ? Convert.ToDecimal(dict["NET_REVENUE_INTEREST"]) : 0m,
                EffectiveStartDate = dict.ContainsKey("EFFECTIVE_START_DATE") && dict["EFFECTIVE_START_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["EFFECTIVE_START_DATE"])
                    : DateTime.MinValue,
                EffectiveEndDate = dict.ContainsKey("EFFECTIVE_END_DATE") && dict["EFFECTIVE_END_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["EFFECTIVE_END_DATE"])
                    : null,
                DivisionOrderId = dict.ContainsKey("DIVISION_ORDER_ID") ? dict["DIVISION_ORDER_ID"]?.ToString() : null
            };

            if (dict.ContainsKey("ROYALTY_INTEREST") && dict["ROYALTY_INTEREST"] != DBNull.Value)
                interest.RoyaltyInterest = Convert.ToDecimal(dict["ROYALTY_INTEREST"]);
            if (dict.ContainsKey("OVERRIDING_ROYALTY_INTEREST") && dict["OVERRIDING_ROYALTY_INTEREST"] != DBNull.Value)
                interest.OverridingRoyaltyInterest = Convert.ToDecimal(dict["OVERRIDING_ROYALTY_INTEREST"]);

            return interest;
        }

        #endregion
    }
}
