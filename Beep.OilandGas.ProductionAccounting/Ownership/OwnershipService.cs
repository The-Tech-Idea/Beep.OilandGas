using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Ownership;
using Beep.OilandGas.Models.DTOs.Ownership;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Ownership
{
    /// <summary>
    /// Service for managing ownership, division orders, and transfers.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class OwnershipService : IOwnershipService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<OwnershipService>? _logger;
        private readonly string _connectionName;
        private const string DIVISION_ORDER_TABLE = "DIVISION_ORDER";
        private const string TRANSFER_ORDER_TABLE = "TRANSFER_ORDER";
        private const string OWNERSHIP_INTEREST_TABLE = "OWNERSHIP_INTEREST";

        public OwnershipService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<OwnershipService>? logger = null,
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
            CreateDivisionOrderRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.WorkingInterest < 0 || request.WorkingInterest > 1)
                throw new ArgumentException("Working interest must be between 0 and 1.", nameof(request));
            if (request.NetRevenueInterest < 0 || request.NetRevenueInterest > 1)
                throw new ArgumentException("Net revenue interest must be between 0 and 1.", nameof(request));
            if (request.NetRevenueInterest > request.WorkingInterest)
                throw new ArgumentException("Net revenue interest cannot exceed working interest.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(DIVISION_ORDER), connName, DIVISION_ORDER_TABLE, null);

            var order = new DIVISION_ORDER
            {
                DIVISION_ORDER_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = request.PropertyId,
                OWNER_BA_ID = request.OwnerBaId,
                WORKING_INTEREST = request.WorkingInterest,
                NET_REVENUE_INTEREST = request.NetRevenueInterest,
                ROYALTY_INTEREST = request.RoyaltyInterest,
                OVERRIDING_ROYALTY_INTEREST = request.OverridingRoyaltyInterest,
                EFFECTIVE_DATE = request.EffectiveDate,
                EXPIRATION_DATE = request.ExpirationDate,
                STATUS = "Pending",
                NOTES = request.Notes,
                ACTIVE_IND = "Y"
            };

            if (order is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(order);

            _logger?.LogDebug("Created division order for property {PropertyId}", request.PropertyId);
            return order;
        }

        /// <summary>
        /// Gets a division order by ID.
        /// </summary>
        public async Task<DIVISION_ORDER?> GetDivisionOrderAsync(string orderId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(orderId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(DIVISION_ORDER), connName, DIVISION_ORDER_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DIVISION_ORDER_ID", Operator = "=", FilterValue = orderId }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<DIVISION_ORDER>().FirstOrDefault();
        }

        /// <summary>
        /// Gets division orders by property.
        /// </summary>
        public async Task<List<DIVISION_ORDER>> GetDivisionOrdersByPropertyAsync(
            string propertyId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return new List<DIVISION_ORDER>();

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(DIVISION_ORDER), connName, DIVISION_ORDER_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<DIVISION_ORDER>().OrderByDescending(o => o.EFFECTIVE_DATE).ToList();
        }

        /// <summary>
        /// Creates a transfer order.
        /// </summary>
        public async Task<TRANSFER_ORDER> CreateTransferOrderAsync(
            CreateTransferOrderRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.InterestTransferred < 0 || request.InterestTransferred > 1)
                throw new ArgumentException("Interest transferred must be between 0 and 1.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(TRANSFER_ORDER), connName, TRANSFER_ORDER_TABLE, null);

            var order = new TRANSFER_ORDER
            {
                TRANSFER_ORDER_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = request.PropertyId,
                FROM_OWNER_BA_ID = request.FromOwnerBaId,
                TO_OWNER_BA_ID = request.ToOwnerBaId,
                INTEREST_TRANSFERRED = request.InterestTransferred,
                EFFECTIVE_DATE = request.EffectiveDate,
                IS_APPROVED = "N",
                ACTIVE_IND = "Y"
            };

            if (order is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(order);

            _logger?.LogDebug("Created transfer order for property {PropertyId}", request.PropertyId);
            return order;
        }

        /// <summary>
        /// Gets a transfer order by ID.
        /// </summary>
        public async Task<TRANSFER_ORDER?> GetTransferOrderAsync(string orderId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(orderId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(TRANSFER_ORDER), connName, TRANSFER_ORDER_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TRANSFER_ORDER_ID", Operator = "=", FilterValue = orderId }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<TRANSFER_ORDER>().FirstOrDefault();
        }

        /// <summary>
        /// Registers an ownership interest.
        /// </summary>
        public async Task<OWNERSHIP_INTEREST> RegisterOwnershipInterestAsync(
            CreateOwnershipInterestRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(OWNERSHIP_INTEREST), connName, OWNERSHIP_INTEREST_TABLE, null);

            var interest = new OWNERSHIP_INTEREST
            {
                OWNERSHIP_INTEREST_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = request.PropertyId,
                OWNER_BA_ID = request.OwnerBaId,
                WORKING_INTEREST = request.WorkingInterest,
                NET_REVENUE_INTEREST = request.NetRevenueInterest,
                ROYALTY_INTEREST = request.RoyaltyInterest,
                OVERRIDING_ROYALTY_INTEREST = request.OverridingRoyaltyInterest,
                EFFECTIVE_START_DATE = request.EffectiveStartDate,
                EFFECTIVE_END_DATE = request.EffectiveEndDate,
                DIVISION_ORDER_ID = request.DivisionOrderId,
                ACTIVE_IND = "Y"
            };

            if (interest is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(interest);

            _logger?.LogDebug("Registered ownership interest for property {PropertyId}", request.PropertyId);
            return interest;
        }

        /// <summary>
        /// Gets ownership interests by property.
        /// </summary>
        public async Task<List<OWNERSHIP_INTEREST>> GetOwnershipInterestsByPropertyAsync(
            string propertyId,
            DateTime? asOfDate,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return new List<OWNERSHIP_INTEREST>();

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(OWNERSHIP_INTEREST), connName, OWNERSHIP_INTEREST_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (asOfDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "EFFECTIVE_START_DATE", Operator = "<=", FilterValue = asOfDate.Value.ToString("yyyy-MM-dd") });
            }

            var results = await repo.GetAsync(filters);
            var interests = results.Cast<OWNERSHIP_INTEREST>().ToList();

            // Filter by effective dates
            if (asOfDate.HasValue)
            {
                interests = interests.Where(i =>
                    i.EFFECTIVE_START_DATE <= asOfDate.Value &&
                    (i.EFFECTIVE_END_DATE == null || i.EFFECTIVE_END_DATE >= asOfDate.Value)).ToList();
            }

            return interests.OrderBy(i => i.EFFECTIVE_START_DATE).ToList();
        }

        /// <summary>
        /// Gets ownership tree for a property.
        /// </summary>
        public async Task<OwnershipTree> GetOwnershipTreeAsync(
            string propertyId,
            DateTime? asOfDate,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                throw new ArgumentException("Property ID is required.", nameof(propertyId));

            var connName = connectionName ?? _connectionName;
            var interests = await GetOwnershipInterestsByPropertyAsync(propertyId, asOfDate, connName);

            // Build ownership tree from interests
            var tree = new OwnershipTree
            {
                PropertyOrLeaseId = propertyId
            };

            // Create root node
            tree.Root = new OwnershipTreeNode
            {
                OwnerId = "ROOT",
                OwnerName = "Property",
                InterestPercentage = 100m
            };

            // Add ownership interests as leaf nodes
            foreach (var interest in interests)
            {
                var node = new OwnershipTreeNode
                {
                    OwnerId = interest.OWNER_BA_ID ?? string.Empty,
                    OwnerName = interest.OWNER_BA_ID ?? string.Empty, // Would need to lookup from BUSINESS_ASSOCIATE
                    InterestPercentage = (interest.WORKING_INTEREST ?? 0m) * 100m
                };
                tree.Root.Children.Add(node);
            }

            return tree;
        }

        /// <summary>
        /// Records an ownership change.
        /// </summary>
        public async Task<OwnershipChangeResult> RecordOwnershipChangeAsync(
            OwnershipChangeRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;

            var result = new OwnershipChangeResult
            {
                ChangeId = request.ChangeId,
                PropertyId = request.PropertyId,
                ChangeType = request.ChangeType,
                EffectiveDate = request.EffectiveDate,
                IsApproved = false,
                Status = "Pending"
            };

            // The actual change is recorded in the division order or transfer order
            // This method just tracks the change event
            _logger?.LogDebug("Recorded ownership change {ChangeType} for property {PropertyId}",
                request.ChangeType, request.PropertyId);

            return result;
        }

        /// <summary>
        /// Approves an ownership change.
        /// </summary>
        public async Task<OwnershipApprovalResult> ApproveOwnershipChangeAsync(
            string changeId,
            string changeType,
            string approverId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(changeId))
                throw new ArgumentException("Change ID is required.", nameof(changeId));

            var connName = connectionName ?? _connectionName;

            if (changeType == "DivisionOrder")
            {
                var order = await GetDivisionOrderAsync(changeId, connName);
                if (order == null)
                    throw new InvalidOperationException($"Division order {changeId} not found.");

                order.STATUS = "Approved";
                order.APPROVAL_DATE = DateTime.UtcNow;
                order.APPROVED_BY = approverId;

                if (order is IPPDMEntity ppdmEntity)
                {
                    _commonColumnHandler.PrepareForUpdate(ppdmEntity, approverId);
                }

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(DIVISION_ORDER), connName, DIVISION_ORDER_TABLE, null);

                await repo.UpdateAsync(order);

                // Create ownership interest from approved division order
                var interestRequest = new CreateOwnershipInterestRequest
                {
                    PropertyId = order.PROPERTY_ID ?? string.Empty,
                    OwnerBaId = order.OWNER_BA_ID ?? string.Empty,
                    WorkingInterest = order.WORKING_INTEREST ?? 0m,
                    NetRevenueInterest = order.NET_REVENUE_INTEREST ?? 0m,
                    RoyaltyInterest = order.ROYALTY_INTEREST,
                    OverridingRoyaltyInterest = order.OVERRIDING_ROYALTY_INTEREST,
                    EffectiveStartDate = order.EFFECTIVE_DATE ?? DateTime.UtcNow,
                    EffectiveEndDate = order.EXPIRATION_DATE,
                    DivisionOrderId = order.DIVISION_ORDER_ID ?? string.Empty
                };

                await RegisterOwnershipInterestAsync(interestRequest, approverId, connName);

                return new OwnershipApprovalResult
                {
                    ChangeId = changeId,
                    IsApproved = true,
                    ApproverId = approverId,
                    ApprovalDate = DateTime.UtcNow,
                    Status = "Approved"
                };
            }
            else if (changeType == "TransferOrder")
            {
                var order = await GetTransferOrderAsync(changeId, connName);
                if (order == null)
                    throw new InvalidOperationException($"Transfer order {changeId} not found.");

                order.IS_APPROVED = "Y";
                order.APPROVAL_DATE = DateTime.UtcNow;
                order.APPROVED_BY = approverId;

                if (order is IPPDMEntity ppdmEntity)
                {
                    _commonColumnHandler.PrepareForUpdate(ppdmEntity, approverId);
                }

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(TRANSFER_ORDER), connName, TRANSFER_ORDER_TABLE, null);

                await repo.UpdateAsync(order);

                // Update ownership interests
                var interests = await GetOwnershipInterestsByPropertyAsync(
                    order.PROPERTY_ID ?? string.Empty,
                    order.EFFECTIVE_DATE,
                    connName);

                var fromInterests = interests.Where(i =>
                    i.OWNER_BA_ID == order.FROM_OWNER_BA_ID &&
                    (i.EFFECTIVE_END_DATE == null || i.EFFECTIVE_END_DATE >= order.EFFECTIVE_DATE)).ToList();

                var interestRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(OWNERSHIP_INTEREST), connName, OWNERSHIP_INTEREST_TABLE, null);

                foreach (var interest in fromInterests)
                {
                    // End the from owner's interest
                    interest.EFFECTIVE_END_DATE = order.EFFECTIVE_DATE;

                    if (interest is IPPDMEntity interestPpdmEntity)
                    {
                        _commonColumnHandler.PrepareForUpdate(interestPpdmEntity, approverId);
                    }

                    await interestRepo.UpdateAsync(interest);

                    // Create new interest for to owner
                    var newInterestRequest = new CreateOwnershipInterestRequest
                    {
                        PropertyId = order.PROPERTY_ID ?? string.Empty,
                        OwnerBaId = order.TO_OWNER_BA_ID ?? string.Empty,
                        WorkingInterest = (interest.WORKING_INTEREST ?? 0m) * (order.INTEREST_TRANSFERRED ?? 0m),
                        NetRevenueInterest = (interest.NET_REVENUE_INTEREST ?? 0m) * (order.INTEREST_TRANSFERRED ?? 0m),
                        EffectiveStartDate = order.EFFECTIVE_DATE ?? DateTime.UtcNow
                    };

                    await RegisterOwnershipInterestAsync(newInterestRequest, approverId, connName);
                }

                return new OwnershipApprovalResult
                {
                    ChangeId = changeId,
                    IsApproved = true,
                    ApproverId = approverId,
                    ApprovalDate = DateTime.UtcNow,
                    Status = "Approved"
                };
            }
            else
            {
                throw new ArgumentException($"Unknown change type: {changeType}", nameof(changeType));
            }
        }

        /// <summary>
        /// Gets ownership change history.
        /// </summary>
        public async Task<List<OwnershipChangeHistory>> GetOwnershipChangeHistoryAsync(
            string propertyId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return new List<OwnershipChangeHistory>();

            var connName = connectionName ?? _connectionName;
            var history = new List<OwnershipChangeHistory>();

            // Get division orders
            var divisionOrders = await GetDivisionOrdersByPropertyAsync(propertyId, connName);
            foreach (var order in divisionOrders)
            {
                history.Add(new OwnershipChangeHistory
                {
                    ChangeId = order.DIVISION_ORDER_ID ?? string.Empty,
                    PropertyId = propertyId,
                    ChangeType = "DivisionOrder",
                    ChangeDate = order.EFFECTIVE_DATE ?? DateTime.MinValue,
                    OwnerBaId = order.OWNER_BA_ID ?? string.Empty,
                    InterestAfter = order.WORKING_INTEREST,
                    Status = order.STATUS ?? string.Empty,
                    ApprovedBy = order.APPROVED_BY
                });
            }

            // Get transfer orders
            var transferRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(TRANSFER_ORDER), connName, TRANSFER_ORDER_TABLE, null);

            var transferFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var transferResults = await transferRepo.GetAsync(transferFilters);
            var transferOrders = transferResults.Cast<TRANSFER_ORDER>().ToList();

            foreach (var order in transferOrders)
            {
                history.Add(new OwnershipChangeHistory
                {
                    ChangeId = order.TRANSFER_ORDER_ID ?? string.Empty,
                    PropertyId = propertyId,
                    ChangeType = "TransferOrder",
                    ChangeDate = order.EFFECTIVE_DATE ?? DateTime.MinValue,
                    OwnerBaId = order.TO_OWNER_BA_ID ?? string.Empty,
                    InterestAfter = order.INTEREST_TRANSFERRED,
                    Status = order.IS_APPROVED == "Y" ? "Approved" : "Pending",
                    ApprovedBy = order.APPROVED_BY
                });
            }

            return history.OrderByDescending(h => h.ChangeDate).ToList();
        }
    }
}
