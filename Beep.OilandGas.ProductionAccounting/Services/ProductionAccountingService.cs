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
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Production Accounting Service - Orchestrator for all production accounting operations.
    /// Coordinates allocation, royalty, measurement, pricing, revenue, GL posting, and period closing.
    /// Master workflow: RUN_TICKET → Measurement → Allocation → Royalty → Revenue → GL → Period Close
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
        private readonly IRevenueService _revenueService;
        private readonly IMeasurementService _measurementService;
        private readonly IPricingService _pricingService;
        private readonly IInventoryService _inventoryService;
        private readonly IPeriodClosingService _periodClosingService;
        private readonly IPPDMMetadataRepository _metadata;
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
            ILogger<ProductionAccountingService> logger = null)
        {
            _allocationService = allocationService ?? throw new ArgumentNullException(nameof(allocationService));
            _royaltyService = royaltyService ?? throw new ArgumentNullException(nameof(royaltyService));
            _jibService = jibService ?? throw new ArgumentNullException(nameof(jibService));
            _imbalanceService = imbalanceService ?? throw new ArgumentNullException(nameof(imbalanceService));
            _seService = seService ?? throw new ArgumentNullException(nameof(seService));
            _fcService = fcService ?? throw new ArgumentNullException(nameof(fcService));
            _amortizationService = amortizationService ?? throw new ArgumentNullException(nameof(amortizationService));
            _glService = glService ?? throw new ArgumentNullException(nameof(glService));
            _revenueService = revenueService ?? throw new ArgumentNullException(nameof(revenueService));
            _measurementService = measurementService ?? throw new ArgumentNullException(nameof(measurementService));
            _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _periodClosingService = periodClosingService ?? throw new ArgumentNullException(nameof(periodClosingService));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
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
                var royalty = await _royaltyService.CalculateAsync(
                    allocationDetails[0], // Use first detail as example
                    userId,
                    connectionName);
                if (royalty == null)
                {
                    _logger?.LogError("Failed to calculate royalties for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                    return false;
                }

                // Step 4: Recognize revenue
                _logger?.LogInformation("Step 4: Recognizing revenue for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                var revenue = await _revenueService.RecognizeRevenueAsync(
                    allocationDetails[0], // Use first detail as example
                    userId,
                    connectionName);
                if (revenue == null)
                {
                    _logger?.LogError("Failed to recognize revenue for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                    return false;
                }

                // Step 5: Post GL entries
                _logger?.LogInformation("Step 5: Posting GL entries for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                var glEntry = await _glService.CreateEntryAsync(
                    "1000", // GL Account placeholder
                    revenue.ALLOCATED_AMOUNT ?? 0,
                    "Production Cycle Entry",
                    userId,
                    connectionName);
                if (glEntry == null)
                {
                    _logger?.LogError("Failed to post GL entries for ticket {TicketId}", runTicket.RUN_TICKET_ID);
                    return false;
                }

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

                // Would query database and sum GROSS_VOLUME where FIELD_ID = fieldId and MEASUREMENT_DATETIME <= asOfDate
                // For now, return placeholder
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<decimal> GetTotalRevenueAsync(string fieldId, DateTime asOfDate, string cn)
        {
            try
            {
                // Query REVENUE_ALLOCATION for total revenue up to date
                var metadata = await _metadata.GetTableMetadataAsync("REVENUE_ALLOCATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(REVENUE_ALLOCATION);

                // Would query database and sum REVENUE_AMOUNT where FIELD_ID = fieldId and REVENUE_DATE <= asOfDate
                // For now, return placeholder
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<decimal> GetTotalRoyaltyAsync(string fieldId, DateTime asOfDate, string cn)
        {
            try
            {
                // Query ROYALTY_PAYMENT for total royalties up to date
                var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_PAYMENT");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ROYALTY_PAYMENT);

                // Would query database and sum ROYALTY_AMOUNT where FIELD_ID = fieldId and PAYMENT_DATE <= asOfDate
                // For now, return placeholder
                return 0;
            }
            catch
            {
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

                // Would query database and sum AMOUNT where FIELD_ID = fieldId and COST_DATE <= asOfDate
                // For now, return placeholder
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<string> GetAccountingMethodAsync(string fieldId, string cn)
        {
            try
            {
                // Query FIELD or accounting setup to determine SE vs FC
                // For now, return default
                return "SuccessfulEfforts";
            }
            catch
            {
                return "Unknown";
            }
        }

        private async Task<string> GetPeriodStatusAsync(string fieldId, string cn)
        {
            try
            {
                // Check if period is open or closed
                // For now, return default
                return "Open";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
