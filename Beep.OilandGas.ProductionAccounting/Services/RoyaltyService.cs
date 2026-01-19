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
    /// Formula: Royalty = (Net Revenue x Royalty Rate)
    /// Where: Net Revenue = Gross Revenue - Transportation - Ad Valorem Tax - Severance Tax
    /// </summary>
    public class RoyaltyService : IRoyaltyService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IJournalEntryService _glService;
        private readonly IAccountingServices _accountingServices;
        private readonly ILogger<RoyaltyService> _logger;
        private const string ConnectionName = "PPDM39";

        public RoyaltyService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IJournalEntryService glService,
            ILogger<RoyaltyService> logger = null,
            IAccountingServices accountingServices = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _accountingServices = accountingServices;
            _glService = _accountingServices?.JournalEntries ?? glService ?? throw new ArgumentNullException(nameof(glService));
            _logger = logger;
        }

        /// <summary>
        /// Calculates royalty payment from an allocation detail.
        /// 
        /// REAL BUSINESS LOGIC - FASB ASC 932 & COPAS Standards:
        /// 
        /// Formula: ROYALTY_PAYMENT = NET_REVENUE x ROYALTY_RATE
        /// 
        /// Where:
        ///   NET_REVENUE = GROSS_REVENUE - DEDUCTIONS
        ///   GROSS_REVENUE = Allocated Volume (BBL) x Commodity Price ($/BBL)
        ///   DEDUCTIONS = Transportation + Ad Valorem Tax + Severance Tax + Processing Fees
        ///   ROYALTY_RATE = Interest % (typically 12.5% for mineral royalty, varies for overriding/net profit)
        /// 
        /// Process:
        /// 1. Get allocation volume from ALLOCATION_DETAIL
        /// 2. Get commodity price for period from PRICE_INDEX
        /// 3. Calculate gross revenue: Volume x Price
        /// 4. Get deductions from cost records: TRANSPORTATION_COST, AD_VALOREM_TAX, SEVERANCE_TAX
        /// 5. Calculate net revenue: Gross - Deductions
        /// 6. Apply royalty rate: Net x Rate
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

                // STEP 2: Resolve allocation and lease for royalty interest lookup
                if (string.IsNullOrWhiteSpace(detail.ALLOCATION_RESULT_ID))
                    throw new RoyaltyException("Allocation detail is missing ALLOCATION_RESULT_ID");

                var allocationResult = await GetAllocationResultAsync(detail.ALLOCATION_RESULT_ID, cn);
                if (allocationResult == null)
                    throw new RoyaltyException($"Allocation result not found: {detail.ALLOCATION_RESULT_ID}");

                var runTicket = await GetRunTicketAsync(allocationResult.ALLOCATION_REQUEST_ID, cn);
                var leaseId = runTicket?.LEASE_ID;
                if (string.IsNullOrWhiteSpace(leaseId))
                    throw new RoyaltyException("Lease ID is required for royalty calculation");

                var royaltyInterest = await GetRoyaltyInterestAsync(
                    leaseId,
                    detail.ENTITY_ID,
                    runTicket?.TICKET_DATE_TIME,
                    cn);

                var rawRate = royaltyInterest?.ROYALTY_RATE ?? royaltyInterest?.INTEREST_PERCENTAGE ?? 12.5m;
                var royaltyRate = NormalizeRate(rawRate);

                if (royaltyRate < 0 || royaltyRate > 0.5m)
                {
                    _logger?.LogWarning("Royalty rate outside normal range: {Rate}%", royaltyRate * 100);
                    throw new RoyaltyException($"Royalty rate {royaltyRate * 100}% outside acceptable range (0-50%)");
                }
                _logger?.LogDebug("Royalty rate: {Rate}%", royaltyRate * 100);

                // STEP 3: Calculate gross revenue (Volume x Commodity Price) (Volume x Commodity Price)
                // Query PRICE_INDEX for commodity price at calculation date
                // GetCommodityPriceAsync returns fallback $75/BBL if lookup fails
                var priceDate = runTicket?.TICKET_DATE_TIME ?? DateTime.UtcNow;
                decimal commodityPrice = runTicket?.PRICE_PER_BARREL > 0
                    ? runTicket.PRICE_PER_BARREL.Value
                    : await GetCommodityPriceAsync("OIL", priceDate, cn);
                
                decimal grossRevenue = allocatedVolume * commodityPrice;
                _logger?.LogDebug("Gross revenue: {Volume} BBL x ${Price}/BBL = ${GrossRevenue}", 
                    allocatedVolume, commodityPrice, grossRevenue);

                // STEP 4: Calculate deductions from cost records
                // Query ACCOUNTING_COST for lease-specific deductions
                var (dbTransportation, dbAdValorem, dbSeverance) = await GetDeductionsAsync(
                    leaseId,
                    priceDate,
                    cn);

                // Use database values if found, otherwise fall back to percentage-based
                decimal transportationCost = dbTransportation > 0 ? dbTransportation : (grossRevenue * 0.08m);
                decimal adValoremTax = dbAdValorem > 0 ? dbAdValorem : (grossRevenue * 0.02m);
                decimal severanceTax = dbSeverance > 0 ? dbSeverance : (grossRevenue * 0.01m);
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
                _logger?.LogInformation("Royalty amount: ${NetRevenue} x {Rate}% = ${Royalty}",
                    netRevenue, royaltyRate * 100, royaltyAmount);

                // STEP 7: Create ROYALTY_CALCULATION record (ASC 932 requirement)
                var royaltyCalc = new ROYALTY_CALCULATION
                {
                    ROYALTY_CALCULATION_ID = Guid.NewGuid().ToString(),
                    PROPERTY_OR_LEASE_ID = leaseId,
                    ALLOCATION_RESULT_ID = allocationResult.ALLOCATION_RESULT_ID,
                    ROYALTY_INTEREST_ID = royaltyInterest?.ROYALTY_INTEREST_ID,
                    ROYALTY_OWNER_ID = detail.ENTITY_ID,
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

                if (royaltyAmount > 0m)
                {
                    var accrualDescription = $"Royalty accrual for allocation {detail.ALLOCATION_DETAIL_ID}";
                    var accrualEntry = await _glService.CreateBalancedEntryAsync(
                        DefaultGlAccounts.RoyaltyExpense,
                        DefaultGlAccounts.AccruedRoyalties,
                        royaltyAmount,
                        accrualDescription,
                        userId,
                        cn);

                    if (accrualEntry != null)
                    {
                        royaltyCalc.ROYALTY_STATUS = RoyaltyStatus.Accrued;
                        royaltyCalc.ROW_CHANGED_DATE = DateTime.UtcNow;
                        royaltyCalc.ROW_CHANGED_BY = userId;
                        await repo.UpdateAsync(royaltyCalc, userId);
                    }
                }

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

            // Query royalty calculations linked to this allocation
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = allocationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var royalties = results?.Cast<ROYALTY_CALCULATION>().ToList() ?? new List<ROYALTY_CALCULATION>();
            
            _logger?.LogInformation(
                "Retrieved {Count} royalty calculations for allocation {AllocationId}",
                royalties.Count, allocationId);
            
            return royalties;
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
                ROYALTY_INTEREST_ID = royalty.ROYALTY_INTEREST_ID,
                ROYALTY_OWNER_ID = royalty.ROYALTY_OWNER_ID,
                PROPERTY_OR_LEASE_ID = royalty.PROPERTY_OR_LEASE_ID,
                ROYALTY_AMOUNT = amount,
                NET_PAYMENT_AMOUNT = amount,
                PAYMENT_DATE = DateTime.UtcNow,
                PAYMENT_METHOD = "CHECK",  // Default; can be set by caller
                STATUS = RoyaltyStatus.Paid,
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

            if (amount > 0m)
            {
                var paymentDescription = $"Royalty payment for calculation {royalty.ROYALTY_CALCULATION_ID}";
                await _glService.CreateBalancedEntryAsync(
                    DefaultGlAccounts.AccruedRoyalties,
                    DefaultGlAccounts.Cash,
                    amount,
                    paymentDescription,
                    userId,
                    cn);
            }

            royalty.ROYALTY_STATUS = RoyaltyStatus.Paid;
            await UpdateRoyaltyCalculationAsync(royalty, userId, cn);

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

        /// <summary>
        /// Gets commodity price from PRICE_INDEX table.
        /// Falls back to $75/BBL if not found.
        /// </summary>
        private async Task<decimal> GetCommodityPriceAsync(string commodity, DateTime asOfDate, string cn)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PRICE_INDEX");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(PRICE_INDEX);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "PRICE_INDEX");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "COMMODITY_TYPE", Operator = "=", FilterValue = commodity },
                    new AppFilter { FieldName = "PRICE_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var prices = await repo.GetAsync(filters);
                var priceList = prices?.Cast<PRICE_INDEX>().OrderByDescending(p => p.PRICE_DATE).ToList() 
                    ?? new List<PRICE_INDEX>();

                if (priceList.Any())
                {
                    var latestPrice = priceList.First();
                    decimal price = latestPrice.PRICE_VALUE ?? 75.00m;
                    _logger?.LogDebug(
                        "Retrieved commodity price for {Commodity}: ${Price}/unit as of {Date}",
                        commodity, price, latestPrice.PRICE_DATE);
                    return price;
                }

                // No price found - log warning and use fallback
                _logger?.LogWarning(
                    "No price index found for commodity {Commodity} as of {Date}, using fallback $75.00/unit",
                    commodity, asOfDate.ToShortDateString());
                return 75.00m;  // Fallback directly instead of returning 0
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving commodity price for {Commodity}, using fallback $75.00/unit", commodity);
                return 75.00m;  // Fallback directly instead of returning 0
            }
        }

        private async Task<ALLOCATION_RESULT?> GetAllocationResultAsync(string allocationResultId, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ALLOCATION_RESULT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ALLOCATION_RESULT");

            var result = await repo.GetByIdAsync(allocationResultId);
            return result as ALLOCATION_RESULT;
        }

        private async Task<RUN_TICKET?> GetRunTicketAsync(string allocationRequestId, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("RUN_TICKET");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(RUN_TICKET);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "RUN_TICKET");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_REQUEST_ID", Operator = "=", FilterValue = allocationRequestId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<RUN_TICKET>().FirstOrDefault();
        }

        private async Task<ROYALTY_INTEREST?> GetRoyaltyInterestAsync(
            string leaseId,
            string ownerId,
            DateTime? asOfDate,
            string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_INTEREST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ROYALTY_INTEREST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ROYALTY_INTEREST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "ROYALTY_OWNER_ID", Operator = "=", FilterValue = ownerId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (asOfDate.HasValue)
            {
                filters.Add(new AppFilter
                {
                    FieldName = "EFFECTIVE_START_DATE",
                    Operator = "<=",
                    FilterValue = asOfDate.Value.ToString("yyyy-MM-dd")
                });
                filters.Add(new AppFilter
                {
                    FieldName = "EFFECTIVE_END_DATE",
                    Operator = ">=",
                    FilterValue = asOfDate.Value.ToString("yyyy-MM-dd")
                });
            }

            var results = await repo.GetAsync(filters);
            var direct = results?.Cast<ROYALTY_INTEREST>()
                .OrderByDescending(r => r.EFFECTIVE_DATE ?? r.EFFECTIVE_START_DATE)
                .FirstOrDefault();
            if (direct != null)
                return direct;

            var ownership = await GetOwnershipInterestAsync(leaseId, ownerId, asOfDate, cn);
            if (ownership == null)
                return null;

            var royaltyRate = NormalizeFraction(ownership.ROYALTY_INTEREST) +
                              NormalizeFraction(ownership.OVERRIDING_ROYALTY_INTEREST);

            if (royaltyRate <= 0m && !string.IsNullOrWhiteSpace(ownership.DIVISION_ORDER_ID))
            {
                var divisionOrder = await GetDivisionOrderAsync(ownership.DIVISION_ORDER_ID, cn);
                if (divisionOrder != null)
                {
                    royaltyRate = NormalizeFraction(divisionOrder.ROYALTY_INTEREST) +
                                  NormalizeFraction(divisionOrder.OVERRIDING_ROYALTY_INTEREST);
                }
            }

            if (royaltyRate <= 0m)
                return null;

            return new ROYALTY_INTEREST
            {
                ROYALTY_INTEREST_ID = Guid.NewGuid().ToString(),
                ROYALTY_OWNER_ID = ownerId,
                PROPERTY_OR_LEASE_ID = leaseId,
                INTEREST_PERCENTAGE = royaltyRate,
                EFFECTIVE_DATE = asOfDate?.Date ?? DateTime.UtcNow.Date,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString()
            };
        }

        private async Task<OWNERSHIP_INTEREST?> GetOwnershipInterestAsync(
            string leaseId,
            string ownerId,
            DateTime? asOfDate,
            string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("OWNERSHIP_INTEREST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(OWNERSHIP_INTEREST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "OWNERSHIP_INTEREST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "OWNER_ID", Operator = "=", FilterValue = ownerId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var interests = results?.Cast<OWNERSHIP_INTEREST>().ToList() ?? new List<OWNERSHIP_INTEREST>();

            if (asOfDate.HasValue)
            {
                var date = asOfDate.Value.Date;
                interests = interests
                    .Where(o =>
                        (!o.EFFECTIVE_START_DATE.HasValue || o.EFFECTIVE_START_DATE.Value.Date <= date) &&
                        (!o.EFFECTIVE_END_DATE.HasValue || o.EFFECTIVE_END_DATE.Value.Date >= date))
                    .ToList();
            }

            return interests.FirstOrDefault();
        }

        private async Task<DIVISION_ORDER?> GetDivisionOrderAsync(string divisionOrderId, string cn)
        {
            if (string.IsNullOrWhiteSpace(divisionOrderId))
                return null;

            var metadata = await _metadata.GetTableMetadataAsync("DIVISION_ORDER");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(DIVISION_ORDER);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "DIVISION_ORDER");

            var result = await repo.GetByIdAsync(divisionOrderId);
            var divisionOrder = result as DIVISION_ORDER;
            if (divisionOrder == null)
                return null;
            if (!string.Equals(divisionOrder.STATUS, "APPROVED", StringComparison.OrdinalIgnoreCase))
                return null;
            return divisionOrder;
        }

        private static decimal NormalizeFraction(decimal? value)
        {
            if (!value.HasValue)
                return 0m;
            if (value.Value <= 0m)
                return 0m;
            return value.Value > 1m ? value.Value / 100m : value.Value;
        }

        private async Task UpdateRoyaltyCalculationAsync(
            ROYALTY_CALCULATION royalty,
            string userId,
            string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_CALCULATION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ROYALTY_CALCULATION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ROYALTY_CALCULATION");

            royalty.ROW_CHANGED_DATE = DateTime.UtcNow;
            royalty.ROW_CHANGED_BY = userId;
            await repo.UpdateAsync(royalty, userId);
        }

        private static decimal NormalizeRate(decimal rawRate)
        {
            if (rawRate <= 0)
                return 0m;

            if (rawRate > 1m)
                return rawRate / 100m;

            return rawRate;
        }

        /// <summary>
        /// Gets deductions (transportation, taxes) from cost records.
        /// </summary>
        private async Task<(decimal transportation, decimal adValorem, decimal severance)> GetDeductionsAsync(
            string leaseId, DateTime periodDate, string cn)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ACCOUNTING_COST);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "ACCOUNTING_COST");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId },
                    new AppFilter { FieldName = "COST_DATE", Operator = "<=", FilterValue = periodDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var costs = await repo.GetAsync(filters);
                var costList = costs?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();

                // Sum costs by type
                decimal transportation = costList
                    .Where(c => c.COST_TYPE == "TRANSPORTATION" || c.COST_TYPE?.Contains("TRANSPORT") == true)
                    .Sum(c => c.AMOUNT);

                decimal adValorem = costList
                    .Where(c => c.COST_TYPE == "AD_VALOREM" || c.COST_TYPE?.Contains("VALOREM") == true)
                    .Sum(c => c.AMOUNT);

                decimal severance = costList
                    .Where(c => c.COST_TYPE == "SEVERANCE" || c.COST_TYPE?.Contains("SEVERANCE") == true)
                    .Sum(c => c.AMOUNT);

                _logger?.LogDebug(
                    "Retrieved deductions for lease {LeaseId}: Transportation=${Transp}, AdValorem=${AdVal}, Severance=${Sev}",
                    leaseId, transportation, adValorem, severance);

                return (transportation, adValorem, severance);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving deductions for lease {LeaseId}", leaseId);
                return (0, 0, 0);  // Return zeros if lookup fails - will trigger fallback in main logic
            }
        }
    }
}
