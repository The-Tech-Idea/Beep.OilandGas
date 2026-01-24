using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Production Accounting Service - Orchestrator for all production accounting operations.
    /// Coordinates allocation, royalty, measurement, pricing, revenue, GL posting, and period closing.
    /// Master workflow: RUN_TICKET -> Measurement -> Allocation -> Royalty -> Revenue -> GL -> Period Close
    /// </summary>
    public partial class ProductionAccountingService : IProductionAccountingService
    {
        private readonly IAllocationService _allocationService;
        private readonly IRoyaltyService _royaltyService;
        private readonly IJointInterestBillingService _jibService;
        private readonly IImbalanceService _imbalanceService;
        private readonly ISuccessfulEffortsService _seService;
        private readonly IFullCostService _fcService;
        private readonly IAmortizationService _amortizationService;
        private readonly IJournalEntryService _glService;
        private readonly IAccountingService _accountingServices;
        private readonly IRevenueService _revenueService;
        private readonly IMeasurementService _measurementService;
        private readonly IPricingService _pricingService;
        private readonly IInventoryService _inventoryService;
        private readonly IPeriodClosingService _periodClosingService;
        private readonly ITakeOrPayService _takeOrPayService;
        private readonly IProductionTaxService _productionTaxService;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly ILogger<ProductionAccountingService> _logger;
        private const string ConnectionName = "PPDM39";

        public ProductionAccountingService(
            IAllocationService allocationService,
            IRoyaltyService royaltyService,
            IJointInterestBillingService jibService,
            IImbalanceService imbalanceService,
            ISuccessfulEffortsService seService,
            IFullCostService fcService,
            IAmortizationService amortizationService,
            IJournalEntryService glService,
            IRevenueService revenueService,
            IMeasurementService measurementService,
            IPricingService pricingService,
            IInventoryService inventoryService,
            IPeriodClosingService periodClosingService,
            IPPDMMetadataRepository metadata,
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            ILogger<ProductionAccountingService> logger = null,
            IAccountingService accountingServices = null,
            ITakeOrPayService takeOrPayService = null,
            IProductionTaxService productionTaxService = null)
        {
            _allocationService = allocationService ?? throw new ArgumentNullException(nameof(allocationService));
            _royaltyService = royaltyService ?? throw new ArgumentNullException(nameof(royaltyService));
            _jibService = jibService ?? throw new ArgumentNullException(nameof(jibService));
            _imbalanceService = imbalanceService ?? throw new ArgumentNullException(nameof(imbalanceService));
            _seService = seService ?? throw new ArgumentNullException(nameof(seService));
            _fcService = fcService ?? throw new ArgumentNullException(nameof(fcService));
            _amortizationService = amortizationService ?? throw new ArgumentNullException(nameof(amortizationService));
            _accountingServices = accountingServices;
            _glService = glService ?? throw new ArgumentNullException(nameof(glService));
            _revenueService = revenueService ?? throw new ArgumentNullException(nameof(revenueService));
            _measurementService = measurementService ?? throw new ArgumentNullException(nameof(measurementService));
            _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _periodClosingService = periodClosingService ?? throw new ArgumentNullException(nameof(periodClosingService));
            _takeOrPayService = takeOrPayService;
            _productionTaxService = productionTaxService;
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _logger = logger;
        }

        /// <summary>
        /// Processes a complete production cycle from run ticket through GL posting.
        /// Master Workflow:
        /// 1. Record measurement from run ticket
        /// 2. Allocate production to wells/leases/interest owners
        /// 3. Calculate royalties on net revenue
        /// 4. Recognize revenue per ASC 606
        /// 5. Post GL entries (debits = credits)
        /// </summary>
        public async Task<bool> ProcessProductionCycleAsync(
            RUN_TICKET runTicket,
            string userId,
            string connectionName = "PPDM39")
        {
            if (runTicket == null)
                throw new ArgumentNullException(nameof(runTicket));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation(
                "Starting production cycle for run ticket {TicketId} by user {UserId}",
                runTicket.RUN_TICKET_ID, userId);

            try
            {
                // Step 1: Record measurement
                _logger?.LogInformation("Step 1: Recording measurement for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                var measurement = await _measurementService.RecordAsync(runTicket, userId, connectionName);
                if (measurement == null)
                {
                    _logger?.LogError("Failed to record measurement for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                    return false;
                }

                // Step 2: Allocate production
                _logger?.LogInformation("Step 2: Allocating production for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                var allocation = await _allocationService.AllocateAsync(
                    runTicket,
                    "ProRata", // Pro Rata allocation method
                    userId,
                    connectionName);
                if (allocation == null)
                {
                    _logger?.LogError("Failed to allocate production for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                    return false;
                }

                // Get allocation details for next steps
                var allocationDetails = await _allocationService.GetDetailsAsync(
                    allocation.ALLOCATION_RESULT_ID,
                    connectionName);
                if (allocationDetails == null || allocationDetails.Count == 0)
                {
                    _logger?.LogError("No allocation details for allocation {AllocationId}", allocation.ALLOCATION_RESULT_ID);
                    return false;
                }

                // Step 3: Calculate royalties for each allocation detail
                _logger?.LogInformation("Step 3: Calculating royalties for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                var royaltyCalculations = new List<ROYALTY_CALCULATION>();
                foreach (var detail in allocationDetails)
                {
                    var royalty = await _royaltyService.CalculateAsync(
                        detail,
                        userId,
                        connectionName);
                    if (royalty == null)
                    {
                        _logger?.LogError("Failed to calculate royalties for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                        return false;
                    }
                    royaltyCalculations.Add(royalty);
                }

                // Step 4: Recognize revenue
                _logger?.LogInformation("Step 4: Recognizing revenue for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                var revenueAllocations = new List<REVENUE_ALLOCATION>();
                foreach (var detail in allocationDetails)
                {
                    var revenue = await _revenueService.RecognizeRevenueAsync(
                        detail,
                        userId,
                        connectionName);
                    if (revenue == null)
                    {
                        _logger?.LogError("Failed to recognize revenue for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                        return false;
                    }
                    revenueAllocations.Add(revenue);
                }

                if (_takeOrPayService != null)
                {
                    var deliveredVolume = allocation.TOTAL_VOLUME ?? allocation.ALLOCATED_VOLUME ?? 0m;
                    var topUpTransaction = await _takeOrPayService.ApplyTakeOrPayAsync(
                        runTicket,
                        allocation,
                        deliveredVolume,
                        userId,
                        connectionName);

                    if (topUpTransaction != null)
                        await CreateTakeOrPayAllocationsAsync(topUpTransaction, allocationDetails, userId, connectionName);
                }

                if (_productionTaxService != null)
                {
                    var transactionIds = revenueAllocations
                        .Select(r => r.REVENUE_TRANSACTION_ID)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct()
                        .ToList();

                    foreach (var transactionId in transactionIds)
                    {
                        var revenueTransaction = await GetRevenueTransactionAsync(transactionId, connectionName);
                        if (revenueTransaction != null)
                            await _productionTaxService.CalculateProductionTaxesAsync(revenueTransaction, userId, connectionName);
                    }
                }

                // Step 5: Post GL entries
                _logger?.LogInformation("Step 5: Posting GL entries for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                
                // Get GL account from configuration or field settings
                // TODO: Implement GL account lookup from CONFIGURATION table
                // For now, use standard revenue account (aligned with Beep.OilandGas.Accounting conventions)
                string glAccountId = DefaultGlAccounts.Revenue;
                decimal glAmount = revenueAllocations.Sum(r => r.ALLOCATED_AMOUNT ?? 0);
                
                var glEntry = await _glService.CreateEntryAsync(
                    glAccountId,
                    glAmount,
                    $"Production Cycle Entry - Ticket {runTicket.RUN_TICKET_ID}",
                    userId,
                    connectionName);
                if (glEntry == null)
                {
                    _logger?.LogError("Failed to post GL entries for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                    return false;
                }

                await MarkRunTicketProcessedAsync(runTicket, userId, connectionName);

                _logger?.LogInformation(
                    "Production cycle completed successfully for ticket {TicketId}",
                    runTicket.RUN_TICKET_ID);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error processing production cycle for ticket {TicketId}: {ErrorMessage}",
                    runTicket.RUN_TICKET_ID, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to process production cycle for ticket {runTicket.RUN_TICKET_ID}", ex);
            }
        }

        /// <summary>
        /// Gets comprehensive accounting status for a field.
        /// Returns: Total production, revenue, royalties, costs, net income, accounting method, period status.
        /// </summary>
        public async Task<AccountingStatusData> GetAccountingStatusAsync(
            string fieldId,
            DateTime? asOfDate = null,
            string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));

            var date = asOfDate ?? DateTime.UtcNow;

            _logger?.LogInformation(
                "Retrieving accounting status for field {FieldId} as of {Date}",
                fieldId, date.ToShortDateString());

            try
            {
                var status = new AccountingStatusData
                {
                    FieldId = fieldId,
                    AsOfDate = date
                };

                // Get total production from measurements
                status.TotalProduction = await GetTotalProductionAsync(fieldId, date, connectionName);

                // Get total revenue from revenue allocations
                status.TotalRevenue = await GetTotalRevenueAsync(fieldId, date, connectionName);

                // Get total royalties from royalty calculations
                status.TotalRoyalty = await GetTotalRoyaltyAsync(fieldId, date, connectionName);

                // Get total costs from accounting costs
                status.TotalCosts = await GetTotalCostsAsync(fieldId, date, connectionName);

                // Calculate net income
                status.NetIncome = status.TotalRevenue - status.TotalCosts - status.TotalRoyalty;

                // Get accounting method (SE or FC)
                status.AccountingMethod = await GetAccountingMethodAsync(fieldId, connectionName);

                // Get period status
                status.PeriodStatus = await GetPeriodStatusAsync(fieldId, connectionName);

                _logger?.LogInformation(
                    "Accounting status retrieved for field {FieldId}: Production={Production}, Revenue={Revenue}, Royalty={Royalty}, Costs={Costs}, NetIncome={NetIncome}",
                    fieldId, status.TotalProduction, status.TotalRevenue, status.TotalRoyalty, status.TotalCosts, status.NetIncome);

                return status;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error retrieving accounting status for field {FieldId}: {ErrorMessage}",
                    fieldId, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to retrieve accounting status for field {fieldId}", ex);
            }
        }

        /// <summary>
        /// Closes a period - prevents future modifications and reconciles all accounts.
        /// </summary>
        public async Task<bool> ClosePeriodAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (periodEnd == default)
                throw new ArgumentException("periodEnd must be valid", nameof(periodEnd));

            _logger?.LogInformation(
                "Closing accounting period for field {FieldId} as of {PeriodEnd} by user {UserId}",
                fieldId, periodEnd.ToShortDateString(), userId);

            try
            {
                // Delegate to period closing service
                var result = await _periodClosingService.ClosePeriodAsync(
                    fieldId,
                    periodEnd,
                    userId,
                    connectionName);

                if (result)
                {
                    _logger?.LogInformation(
                        "Period closed successfully for field {FieldId}",
                        fieldId);
                }
                else
                {
                    _logger?.LogError(
                        "Period close failed for field {FieldId}",
                        fieldId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error closing period for field {FieldId}: {ErrorMessage}",
                    fieldId, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to close period for field {fieldId}", ex);
            }
        }

        // Private helper methods

        private async Task<decimal> GetTotalProductionAsync(string fieldId, DateTime asOfDate, string cn)
        {
            try
            {
                // Query MEASUREMENT_RECORD for total gross volume up to date
                var metadata = await _metadata.GetTableMetadataAsync("MEASUREMENT_RECORD");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(MEASUREMENT_RECORD);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "MEASUREMENT_RECORD");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "MEASUREMENT_DATETIME", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd HH:mm:ss") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var measurements = await repo.GetAsync(filters);
                var measurementList = measurements?.Cast<MEASUREMENT_RECORD>().ToList() ?? new List<MEASUREMENT_RECORD>();

                var totalProduction = measurementList.Sum(m => m.GROSS_VOLUME ?? 0);
                _logger?.LogInformation(
                    "Calculated total production for property/lease {FieldId}: {TotalProduction} from {Count} records",
                    fieldId, totalProduction, measurementList.Count);

                return totalProduction;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error calculating total production for field {FieldId}", fieldId);
                return 0;
            }
        }

        private async Task<decimal> GetTotalRevenueAsync(string fieldId, DateTime asOfDate, string cn)
        {
            try
            {
                // Query REVENUE_TRANSACTION for total revenue up to date
                var metadata = await _metadata.GetTableMetadataAsync("REVENUE_TRANSACTION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(REVENUE_TRANSACTION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "REVENUE_TRANSACTION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var revenues = await repo.GetAsync(filters);
                var revenueList = revenues?.Cast<REVENUE_TRANSACTION>().ToList() ?? new List<REVENUE_TRANSACTION>();

                var totalRevenue = revenueList.Sum(r => r.NET_REVENUE ?? r.GROSS_REVENUE ?? 0);
                _logger?.LogInformation(
                    "Calculated total revenue for property/lease {FieldId}: ${Total} from {Count} transactions",
                    fieldId, totalRevenue, revenueList.Count);

                return totalRevenue;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error calculating total revenue for field {FieldId}", fieldId);
                return 0;
            }
        }

        private async Task<decimal> GetTotalRoyaltyAsync(string fieldId, DateTime asOfDate, string cn)
        {
            try
            {
                // Query ROYALTY_CALCULATION for total royalties up to date
                var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_CALCULATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ROYALTY_CALCULATION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "ROYALTY_CALCULATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var royalties = await repo.GetAsync(filters);
                var royaltyList = royalties?.Cast<ROYALTY_CALCULATION>().ToList() ?? new List<ROYALTY_CALCULATION>();

                var totalRoyalty = royaltyList.Sum(r => r.ROYALTY_AMOUNT ?? 0);
                _logger?.LogInformation(
                    "Calculated total royalty for property/lease {FieldId}: ${Total} from {Count} calculations",
                    fieldId, totalRoyalty, royaltyList.Count);

                return totalRoyalty;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error calculating total royalty for field {FieldId}", fieldId);
                return 0;
            }
        }

        private async Task<decimal> GetTotalCostsAsync(string fieldId, DateTime asOfDate, string cn)
        {
            try
            {
                // Query ACCOUNTING_COST for total costs up to date
                var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ACCOUNTING_COST);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "ACCOUNTING_COST");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "COST_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var costs = await repo.GetAsync(filters);
                var costList = costs?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();

                var totalCosts = costList.Sum(c => c.AMOUNT);
                _logger?.LogInformation(
                    "Calculated total costs for property/lease {FieldId}: ${Total} from {Count} cost records",
                    fieldId, totalCosts, costList.Count);

                return totalCosts;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error calculating total costs for field {FieldId}", fieldId);
                return 0;
            }
        }

        private async Task<string> GetAccountingMethodAsync(string fieldId, string cn)
        {
            try
            {
                // Check ACCOUNTING_POLICY or company default for method (SE vs FC)
                // For now, default to Successful Efforts which is most common in upstream
                // In future, store accounting method in FIELD or ACCOUNTING_POLICY table
                var metadata = await _metadata.GetTableMetadataAsync("FIELD");
                var fieldEntity = await GetFieldAsync(fieldId, cn);
                
                // TODO: Check FIELD.ACCOUNTING_METHOD or CONFIGURATION table once populated
                string accountingMethod = "SuccessfulEfforts";  // Default: SE is standard
                
                _logger?.LogDebug("Retrieved accounting method for field {FieldId}: {Method}", 
                    fieldId, accountingMethod);
                return accountingMethod;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving accounting method for field {FieldId}", fieldId);
                return "SuccessfulEfforts";  // Safe default
            }
        }

        private async Task<FIELD> GetFieldAsync(string fieldId, string cn)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FIELD");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(FIELD);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "FIELD");

                var field = await repo.GetByIdAsync(fieldId);
                return field as FIELD;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> GetPeriodStatusAsync(string fieldId, string cn)
        {
            try
            {
                // Check if there are any unreconciled transactions for field
                // TODO: Implement proper PERIOD_CLOSE tracking once table is added
                // For now, default to "Open" - proper implementation would:
                // 1. Query ALLOCATION_RESULT for unallocated production
                // 2. Query REVENUE_ALLOCATION for unrecognized revenue
                // 3. Query GL_ENTRY for unbalanced entries
                // 4. Set status to "Closed" when all reconciled

                _logger?.LogDebug("Period status for field {FieldId}: Open (default)", fieldId);
                return "Open";
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving period status for field {FieldId}", fieldId);
                return "Open";  // Safe default
            }
        }

        private async Task MarkRunTicketProcessedAsync(RUN_TICKET runTicket, string userId, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("RUN_TICKET");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(RUN_TICKET);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "RUN_TICKET");

            runTicket.IS_PROCESSED = "Y";
            runTicket.PROCESSED_DATE = DateTime.UtcNow;
            runTicket.ROW_CHANGED_BY = userId;
            runTicket.ROW_CHANGED_DATE = DateTime.UtcNow;

            await repo.UpdateAsync(runTicket, userId);
        }

        private async Task CreateTakeOrPayAllocationsAsync(
            REVENUE_TRANSACTION revenueTransaction,
            List<ALLOCATION_DETAIL> allocationDetails,
            string userId,
            string cn)
        {
            if (revenueTransaction == null || allocationDetails == null || allocationDetails.Count == 0)
                return;

            var totalPercentage = allocationDetails.Sum(d => d.ALLOCATION_PERCENTAGE ?? 0m);
            if (totalPercentage <= 0m)
                totalPercentage = 100m;

            var repo = await CreateRepoAsync<REVENUE_ALLOCATION>("REVENUE_ALLOCATION", cn);
            foreach (var detail in allocationDetails)
            {
                var percentage = detail.ALLOCATION_PERCENTAGE ?? 0m;
                var ratio = percentage / totalPercentage;
                var amount = (revenueTransaction.NET_REVENUE ?? 0m) * ratio;

                var allocation = new REVENUE_ALLOCATION
                {
                    REVENUE_ALLOCATION_ID = Guid.NewGuid().ToString(),
                    REVENUE_TRANSACTION_ID = revenueTransaction.REVENUE_TRANSACTION_ID,
                    AFE_ID = revenueTransaction.AFE_ID,
                    INTEREST_OWNER_BA_ID = detail.ENTITY_ID,
                    INTEREST_PERCENTAGE = percentage,
                    ALLOCATED_AMOUNT = amount,
                    ALLOCATION_METHOD = RevenueAllocationMethod.ProRata,
                    DESCRIPTION = "Take-or-pay allocation",
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                await repo.InsertAsync(allocation, userId);
            }
        }

        private async Task<REVENUE_TRANSACTION?> GetRevenueTransactionAsync(string revenueTransactionId, string cn)
        {
            if (string.IsNullOrWhiteSpace(revenueTransactionId))
                return null;

            var metadata = await _metadata.GetTableMetadataAsync("REVENUE_TRANSACTION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(REVENUE_TRANSACTION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "REVENUE_TRANSACTION");

            var result = await repo.GetByIdAsync(revenueTransactionId);
            return result as REVENUE_TRANSACTION;
        }

        private async Task<PPDMGenericRepository> CreateRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }
    }
}
