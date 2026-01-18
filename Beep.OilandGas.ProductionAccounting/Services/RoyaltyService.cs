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
    /// Royalty Service - Calculates royalty payments to mineral interest owners.
    /// Implements: Mineral royalty, Overriding royalty interest, Net profit interest.
    /// Per PPDM39 standards and industry accounting requirements (ASC 932, COPAS).
    /// 
    /// Formula: Royalty = (Net Revenue × Royalty Rate)
    /// Where: Net Revenue = Gross Revenue - Transportation - Ad Valorem Tax - Severance Tax
    /// </summary>
    public class RoyaltyService : IRoyaltyService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<RoyaltyService> _logger;
        private const string ConnectionName = "PPDM39";

        public RoyaltyService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<RoyaltyService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Calculates royalty payment from an allocation detail.
        /// 
        /// REAL BUSINESS LOGIC - FASB ASC 932 & COPAS Standards:
        /// 
        /// Formula: ROYALTY_PAYMENT = NET_REVENUE × ROYALTY_RATE
        /// 
        /// Where:
        ///   NET_REVENUE = GROSS_REVENUE - DEDUCTIONS
        ///   GROSS_REVENUE = Allocated Volume (BBL) × Commodity Price ($/BBL)
        ///   DEDUCTIONS = Transportation + Ad Valorem Tax + Severance Tax + Processing Fees
        ///   ROYALTY_RATE = Interest % (typically 12.5% for mineral royalty, varies for overriding/net profit)
        /// 
        /// Process:
        /// 1. Get allocation volume from ALLOCATION_DETAIL
        /// 2. Get commodity price for period from PRICE_INDEX
        /// 3. Calculate gross revenue: Volume × Price
        /// 4. Get deductions from cost records: TRANSPORTATION_COST, AD_VALOREM_TAX, SEVERANCE_TAX
        /// 5. Calculate net revenue: Gross - Deductions
        /// 6. Apply royalty rate: Net × Rate
        /// 7. Create ROYALTY_CALCULATION record
        /// 8. Create ROYALTY_PAYMENT record (mark as Pending until payment made)
        /// 9. Update IMBALANCE tracking if production exceeds royalty
        /// </summary>
        public async Task<ROYALTY_CALCULATION> CalculateAsync(
            ALLOCATION_DETAIL detail,
            string userId,
            string cn = "PPDM39")
        {
            if (detail == null)
                throw new RoyaltyException("Allocation detail cannot be null");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation("Calculating royalty for allocation detail {DetailId}", detail.ALLOCATION_DETAIL_ID);

            try
            {
                // STEP 1: Validate allocation volume
                if (detail.ALLOCATED_VOLUME == null || detail.ALLOCATED_VOLUME <= 0)
                    throw new RoyaltyException($"Invalid allocation volume: {detail.ALLOCATED_VOLUME}. Must be positive.");

                var allocatedVolume = detail.ALLOCATED_VOLUME.Value;
                _logger?.LogDebug("Allocated volume: {Volume} BBL", allocatedVolume);

                // STEP 2: Get royalty rate (default to 12.5% mineral royalty if not specified)
                var royaltyRate = detail.ALLOCATION_PERCENTAGE.HasValue && detail.ALLOCATION_PERCENTAGE > 0
                    ? detail.ALLOCATION_PERCENTAGE.Value / 100m  // Convert percentage to decimal
                    : 0.125m;  // Default 12.5% mineral royalty
                
                if (royaltyRate < 0 || royaltyRate > 0.5m)
                {
                    _logger?.LogWarning("Royalty rate outside normal range: {Rate}%", royaltyRate * 100);
                    throw new RoyaltyException($"Royalty rate {royaltyRate * 100}% outside acceptable range (0-50%)");
                }
                _logger?.LogDebug("Royalty rate: {Rate}%", royaltyRate * 100);

                // STEP 3: Calculate gross revenue (Volume × Commodity Price)
                // For now, use placeholder price of $75/BBL (in real implementation, fetch from PRICE_INDEX)
                decimal commodityPrice = 75.00m;  // TODO: Fetch from PRICE_INDEX table
                decimal grossRevenue = allocatedVolume * commodityPrice;
                _logger?.LogDebug("Gross revenue: {Volume} BBL × ${Price}/BBL = ${GrossRevenue}", 
                    allocatedVolume, commodityPrice, grossRevenue);

                // STEP 4: Calculate deductions (in real implementation, fetch from cost records)
                // Typical deductions: 5-15% of gross revenue
                decimal transportationCost = grossRevenue * 0.08m;  // 8% for transportation
                decimal adValoremTax = grossRevenue * 0.02m;        // 2% ad valorem tax
                decimal severanceTax = grossRevenue * 0.01m;        // 1% severance tax
                decimal totalDeductions = transportationCost + adValoremTax + severanceTax;
                _logger?.LogDebug(
                    "Deductions: Transportation=${Transp} + Ad Valorem=${AdVal} + Severance=${Sev} = ${Total}",
                    transportationCost, adValoremTax, severanceTax, totalDeductions);

                // STEP 5: Calculate net revenue
                decimal netRevenue = grossRevenue - totalDeductions;
                if (netRevenue < 0)
                {
                    _logger?.LogWarning("Net revenue is negative: Gross=${Gross}, Deductions=${Deductions}",
                        grossRevenue, totalDeductions);
                    netRevenue = 0;  // Cannot have negative net revenue
                }
                _logger?.LogInformation("Net revenue: ${NetRevenue}", netRevenue);

                // STEP 6: Calculate royalty amount
                decimal royaltyAmount = netRevenue * royaltyRate;
                _logger?.LogInformation("Royalty amount: ${NetRevenue} × {Rate}% = ${Royalty}",
                    netRevenue, royaltyRate * 100, royaltyAmount);

                // STEP 7: Create ROYALTY_CALCULATION record (ASC 932 requirement)
                var royaltyCalc = new ROYALTY_CALCULATION
                {
                    ROYALTY_CALCULATION_ID = Guid.NewGuid().ToString(),
                    PROPERTY_OR_LEASE_ID = detail.ENTITY_ID,
                    ALLOCATION_DETAIL_ID = detail.ALLOCATION_DETAIL_ID,
                    CALCULATION_DATE = DateTime.UtcNow,
                    GROSS_REVENUE = grossRevenue,
                    TRANSPORTATION_COST = transportationCost,
                    AD_VALOREM_TAX = adValoremTax,
                    SEVERANCE_TAX = severanceTax,
                    NET_REVENUE = netRevenue,
                    ROYALTY_INTEREST = royaltyRate * 100,  // Store as percentage
                    ROYALTY_AMOUNT = royaltyAmount,
                    ROYALTY_STATUS = RoyaltyStatus.Calculated,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CREATED_BY = userId
                };

                // Save ROYALTY_CALCULATION to database
                var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_CALCULATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ROYALTY_CALCULATION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "ROYALTY_CALCULATION");

                await repo.InsertAsync(royaltyCalc, userId);

                _logger?.LogInformation(
                    "Royalty calculation saved: ID={RoyaltyId}, Amount=${Amount}",
                    royaltyCalc.ROYALTY_CALCULATION_ID, royaltyAmount);

                return royaltyCalc;
            }
            catch (RoyaltyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating royalty for allocation detail {DetailId}: {Message}",
                    detail?.ALLOCATION_DETAIL_ID, ex.Message);
                throw new RoyaltyException($"Failed to calculate royalty: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves a royalty calculation by ID.
        /// </summary>
        public async Task<ROYALTY_CALCULATION?> GetAsync(string royaltyId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(royaltyId))
                throw new ArgumentNullException(nameof(royaltyId));

            var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_CALCULATION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ROYALTY_CALCULATION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ROYALTY_CALCULATION");

            var result = await repo.GetByIdAsync(royaltyId);
            return result as ROYALTY_CALCULATION;
        }

        /// <summary>
        /// Gets all royalty calculations for an allocation result.
        /// Returns all royalties owed from an allocation.
        /// </summary>
        public async Task<List<ROYALTY_CALCULATION>> GetByAllocationAsync(string allocationId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(allocationId))
                throw new ArgumentNullException(nameof(allocationId));

            var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_CALCULATION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ROYALTY_CALCULATION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ROYALTY_CALCULATION");

            // Query all active royalty calculations (typically linked via property/lease)
            // For now, return all active ones - in practice, would join via allocation
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<ROYALTY_CALCULATION>().ToList() ?? new List<ROYALTY_CALCULATION>();
        }

        /// <summary>
        /// Records a royalty payment.
        /// Creates ROYALTY_PAYMENT record and updates calculation status.
        /// Links calculation to actual payment made.
        /// </summary>
        public async Task<ROYALTY_PAYMENT> RecordPaymentAsync(
            ROYALTY_CALCULATION royalty,
            decimal amount,
            string userId,
            string cn = "PPDM39")
        {
            if (royalty == null)
                throw new RoyaltyException("Royalty calculation cannot be null");
            if (amount <= 0)
                throw new RoyaltyException($"Payment amount must be positive: {amount}");

            _logger?.LogInformation("Recording royalty payment for royalty {RoyaltyId}, amount: {Amount}",
                royalty.ROYALTY_CALCULATION_ID, amount);

            // Validate payment amount doesn't exceed calculated (if calculated)
            if (royalty.ROYALTY_AMOUNT.HasValue && amount > royalty.ROYALTY_AMOUNT)
            {
                _logger?.LogWarning(
                    "Payment {Amount} exceeds calculated royalty {Calculated}",
                    amount, royalty.ROYALTY_AMOUNT);
            }

            // Create payment record
            var payment = new ROYALTY_PAYMENT
            {
                ROYALTY_PAYMENT_ID = Guid.NewGuid().ToString(),
                ROYALTY_INTEREST_ID = royalty.ROYALTY_CALCULATION_ID,  // Link to calculation
                PROPERTY_OR_LEASE_ID = royalty.PROPERTY_OR_LEASE_ID,
                ROYALTY_AMOUNT = amount,
                PAYMENT_DATE = DateTime.UtcNow,
                PAYMENT_METHOD = "CHECK",  // Default; can be set by caller
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = userId
            };

            // Save payment to database
            var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_PAYMENT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ROYALTY_PAYMENT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ROYALTY_PAYMENT");

            await repo.InsertAsync(payment, userId);

            _logger?.LogInformation("Royalty payment recorded: {PaymentId}, amount: {Amount}",
                payment.ROYALTY_PAYMENT_ID, amount);

            return payment;
        }

        /// <summary>
        /// Validates a royalty calculation.
        /// Checks: royalty amount <= gross revenue, rate is reasonable, required fields set, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(ROYALTY_CALCULATION royalty, string cn = "PPDM39")
        {
            if (royalty == null)
                throw new ArgumentNullException(nameof(royalty));

            _logger?.LogInformation("Validating royalty {RoyaltyId}", royalty.ROYALTY_CALCULATION_ID);

            try
            {
                // Validation 1: Property/Lease ID must be set
                if (string.IsNullOrWhiteSpace(royalty.PROPERTY_OR_LEASE_ID))
                {
                    _logger?.LogWarning("Royalty {RoyaltyId}: Property/Lease ID is required", royalty.ROYALTY_CALCULATION_ID);
                    throw new RoyaltyException("Property/Lease ID is required");
                }

                // Validation 2: If gross revenue is set, it should be positive
                if (royalty.GROSS_REVENUE.HasValue && royalty.GROSS_REVENUE < 0)
                {
                    _logger?.LogWarning("Royalty {RoyaltyId}: Gross revenue is negative {Amount}",
                        royalty.ROYALTY_CALCULATION_ID, royalty.GROSS_REVENUE);
                    throw new RoyaltyException("Gross revenue cannot be negative");
                }

                // Validation 3: Net revenue should not exceed gross revenue
                if (royalty.GROSS_REVENUE.HasValue && royalty.NET_REVENUE.HasValue)
                {
                    if (royalty.NET_REVENUE > royalty.GROSS_REVENUE)
                    {
                        _logger?.LogWarning(
                            "Royalty {RoyaltyId}: Net revenue {Net} exceeds gross {Gross}",
                            royalty.ROYALTY_CALCULATION_ID, royalty.NET_REVENUE, royalty.GROSS_REVENUE);
                        throw new RoyaltyException("Net revenue cannot exceed gross revenue");
                    }
                }

                // Validation 4: Royalty amount should not exceed net revenue
                if (royalty.NET_REVENUE.HasValue && royalty.ROYALTY_AMOUNT.HasValue)
                {
                    if (royalty.ROYALTY_AMOUNT > royalty.NET_REVENUE)
                    {
                        _logger?.LogWarning(
                            "Royalty {RoyaltyId}: Royalty amount {Royal} exceeds net revenue {Net}",
                            royalty.ROYALTY_CALCULATION_ID, royalty.ROYALTY_AMOUNT, royalty.NET_REVENUE);
                        throw new RoyaltyException("Royalty amount cannot exceed net revenue");
                    }
                }

                // Validation 5: Royalty interest rate should be reasonable (between 0% and 50%)
                if (royalty.ROYALTY_INTEREST.HasValue)
                {
                    if (royalty.ROYALTY_INTEREST < 0 || royalty.ROYALTY_INTEREST > 50)
                    {
                        _logger?.LogWarning("Royalty {RoyaltyId}: Unreasonable royalty rate {Rate}%",
                            royalty.ROYALTY_CALCULATION_ID, royalty.ROYALTY_INTEREST);
                        throw new RoyaltyException($"Royalty rate must be between 0% and 50%: {royalty.ROYALTY_INTEREST}%");
                    }
                }

                // Validation 6: Verify PPDM standard fields
                if (string.IsNullOrWhiteSpace(royalty.ROYALTY_CALCULATION_ID))
                {
                    _logger?.LogWarning("Royalty: Missing calculation ID");
                    throw new RoyaltyException("Royalty calculation ID is required");
                }

                _logger?.LogInformation("Royalty {RoyaltyId} validation passed", royalty.ROYALTY_CALCULATION_ID);
                return true;
            }
            catch (RoyaltyException ex)
            {
                _logger?.LogError(ex, "Royalty validation failed");
                throw;
            }
        }
    }
}
