using Beep.OilandGas.PPDM39.Core;
﻿using System;
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
    public partial class RoyaltyService : IRoyaltyService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IJournalEntryService _glService;
        private readonly Func<Task<string>> _resolveConnection;
        private readonly ILogger<RoyaltyService> _logger;

        public RoyaltyService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IJournalEntryService glService,
            ILogger<RoyaltyService> logger,
            Func<Task<string>> resolveConnection)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _resolveConnection = resolveConnection ?? throw new ArgumentNullException(nameof(resolveConnection));
            _glService = glService ?? throw new ArgumentNullException(nameof(glService));
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
        public async Task<ROYALTY_CALCULATION> CalculateAsync(string allocationDetailId, string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            var detail = await LoadAllocationDetailAsync(allocationDetailId);
            var repo = await CreateRepositoryAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");
            var existing = (await repo.GetAsync(new List<AppFilter> {
                new() { FieldName = "ALLOCATION_DETAIL_ID", Operator = "=", FilterValue = allocationDetailId }
            })).OfType<ROYALTY_CALCULATION>().ToList();
            if (existing.Count != 0)
            {
                if (existing.Count == 1 && existing[0].ACTIVE_IND == _defaults.GetActiveIndicatorYes() &&
                    existing[0].ROYALTY_STATUS is RoyaltyStatus.Accrued or RoyaltyStatus.Paid)
                    return existing[0];
                throw new RoyaltyException("This allocation already has an incomplete, reversed or ambiguous royalty calculation. Reconcile it before retrying.");
            }
            return await CalculateCoreAsync(detail, userId, await _resolveConnection(), persist: true);
        }

        private async Task<ALLOCATION_DETAIL> LoadAllocationDetailAsync(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            var repo = await CreateRepositoryAsync<ALLOCATION_DETAIL>("ALLOCATION_DETAIL");
            var details = (await repo.GetAsync(new List<AppFilter> {
                new() { FieldName = "ALLOCATION_DETAIL_ID", Operator = "=", FilterValue = id },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            })).OfType<ALLOCATION_DETAIL>().ToList();
            if (details.Count != 1) throw new RoyaltyException("Exactly one active stored allocation detail is required.");
            return details[0];
        }

        public async Task<string> GetAllocationFieldAsync(string allocationDetailId)
        {
            var detail = await LoadAllocationDetailAsync(allocationDetailId);
            var allocation = await GetAllocationResultAsync(detail.ALLOCATION_RESULT_ID, "");
            var ticket = allocation is null ? null : await GetRunTicketAsync(allocation.ALLOCATION_REQUEST_ID, "");
            if (string.IsNullOrWhiteSpace(ticket?.FIELD_ID)) throw new RoyaltyException("The stored allocation must resolve to a source-ticket field.");
            return ticket.FIELD_ID;
        }

        public Task<ROYALTY_CALCULATION> PreviewAsync(ALLOCATION_DETAIL detail, string userId, string connectionName = "PPDM39")
            => CalculateCoreAsync(detail, userId, connectionName, persist: false);

        private async Task<ROYALTY_CALCULATION> CalculateCoreAsync(ALLOCATION_DETAIL detail, string userId, string connectionName, bool persist)
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

                var ALLOCATION_RESULT = await GetAllocationResultAsync(detail.ALLOCATION_RESULT_ID, connectionName);
                if (ALLOCATION_RESULT == null)
                    throw new RoyaltyException($"Allocation result not found: {detail.ALLOCATION_RESULT_ID}");

                var RUN_TICKET = await GetRunTicketAsync(ALLOCATION_RESULT.ALLOCATION_REQUEST_ID, connectionName);
                var leaseId = RUN_TICKET?.LEASE_ID;
                if (string.IsNullOrWhiteSpace(leaseId))
                    throw new RoyaltyException("Lease ID is required for royalty calculation");

                var ROYALTY_INTEREST = await GetRoyaltyInterestAsync(
                    leaseId,
                    detail.ENTITY_ID,
                    RUN_TICKET?.TICKET_DATE_TIME,
                    connectionName);

                if (ROYALTY_INTEREST is null || string.IsNullOrWhiteSpace(ROYALTY_INTEREST.ROYALTY_INTEREST_ID))
                    throw new RoyaltyException("A recorded effective royalty interest is required.");
                var rawRate = ROYALTY_INTEREST.ROYALTY_RATE;
                var royaltyRate = rawRate / 100m; // ROYALTY_RATE is explicitly stored in percentage points.

                if (royaltyRate < 0 || royaltyRate > 1m)
                {
                    _logger?.LogWarning("Royalty rate outside normal range: {Rate}%", royaltyRate * 100);
                    throw new RoyaltyException($"Royalty rate {royaltyRate * 100}% outside acceptable range (0-100%)");
                }
                _logger?.LogDebug("Royalty rate: {Rate}%", royaltyRate * 100);

                // STEP 3: Calculate gross revenue (Volume x Commodity Price) (Volume x Commodity Price)
                // Use the source ticket valuation; no default or substitute market price.
                // Missing recorded prices fail before any financial records are written.
                var priceDate = RUN_TICKET?.TICKET_DATE_TIME ?? throw new RoyaltyException("The source ticket date is required.");
                if (RUN_TICKET?.PRICE_PER_BARREL is not > 0 ||
                    !string.Equals(RUN_TICKET.VOLUME_OUOM, "BBL", StringComparison.OrdinalIgnoreCase))
                    throw new RoyaltyException("A positive recorded ticket price and BBL volume units are required.");
                decimal commodityPrice = RUN_TICKET.PRICE_PER_BARREL.Value;
                decimal grossRevenue = allocatedVolume * commodityPrice;
                _logger?.LogDebug("Gross revenue: {Volume} BBL x ${Price}/BBL = ${GrossRevenue}", 
                    allocatedVolume, commodityPrice, grossRevenue);

                // STEP 4: Calculate deductions from cost records
                // Query ACCOUNTING_COST for lease-specific deductions
                var (dbTransportation, dbAdValorem, dbSeverance) = await GetDeductionsAsync(
                    leaseId,
                    priceDate,
                    connectionName);

                // Use recorded amounts only; never invent percentage deductions.
                decimal transportationCost = dbTransportation;
                decimal adValoremTax = dbAdValorem;
                decimal severanceTax = dbSeverance;
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
                    throw new RoyaltyException("Recorded deductions exceed gross revenue; review the source amounts.");
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
                    ALLOCATION_RESULT_ID = ALLOCATION_RESULT.ALLOCATION_RESULT_ID,
                    ROYALTY_INTEREST_ID = ROYALTY_INTEREST?.ROYALTY_INTEREST_ID,
                    ROYALTY_OWNER_ID = detail.ENTITY_ID,
                    ALLOCATION_DETAIL_ID = detail.ALLOCATION_DETAIL_ID,
                    CALCULATION_DATE = DateTime.UtcNow,
                    GROSS_REVENUE = grossRevenue,
                    GROSS_VOLUME = allocatedVolume,
                    GROSS_VOLUME_OUOM = "BBL",
                    FLUID_TYPE = "OIL",
                    PRICE_PER_UNIT = commodityPrice,
                    PRODUCTION_PERIOD_START = priceDate.Date,
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

                if (!persist)
                {
                    royaltyCalc.ROYALTY_STATUS = "PREVIEW";
                    return royaltyCalc;
                }

                // Save ROYALTY_CALCULATION to database
                var repo = await CreateRepositoryAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");

                // The existing database primary key arbitrates concurrent attempts before any journal call.
                // A correction requires a new allocation detail, not a second obligation for this detail.
                var digest = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
                    "royalty-accrual:" + detail.ALLOCATION_DETAIL_ID));
                royaltyCalc.ROYALTY_CALCULATION_ID = new Guid(digest.AsSpan(0, 16)).ToString();

                await repo.InsertAsync(royaltyCalc, userId);

                if (royaltyAmount > 0m)
                {
                    var accrualDescription = $"Royalty accrual {royaltyCalc.ROYALTY_CALCULATION_ID} for allocation {detail.ALLOCATION_DETAIL_ID}";
                    var accrualEntry = await _glService.CreateReferencedBalancedEntryAsync(
                        DefaultGlAccounts.RoyaltyExpense,
                        DefaultGlAccounts.AccruedRoyalties,
                        royaltyAmount,
                        accrualDescription,
                        userId,
                        royaltyCalc.ROYALTY_CALCULATION_ID);

                    if (accrualEntry is null || string.IsNullOrWhiteSpace(accrualEntry.JOURNAL_ENTRY_ID) || accrualEntry.STATUS != "POSTED")
                        throw new RoyaltyException("The royalty journal was not confirmed posted. Reconcile the reserved calculation before retrying.");
                    royaltyCalc.JOURNAL_ENTRY_ID = accrualEntry.JOURNAL_ENTRY_ID;
                }

                royaltyCalc.ROYALTY_STATUS = RoyaltyStatus.Accrued;
                royaltyCalc.ROW_CHANGED_DATE = DateTime.UtcNow;
                royaltyCalc.ROW_CHANGED_BY = userId;
                await repo.UpdateAsync(royaltyCalc, userId);

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
        public async Task<ROYALTY_CALCULATION?> GetAsync(string royaltyId, string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(royaltyId))
                throw new ArgumentNullException(nameof(royaltyId));

            var repo = await CreateRepositoryAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");

            var result = await repo.GetByIdAsync(royaltyId);
            return result as ROYALTY_CALCULATION;
        }

        /// <summary>
        /// Gets all royalty calculations for an allocation result.
        /// Returns all royalties owed from an allocation.
        /// </summary>
        public async Task<List<ROYALTY_CALCULATION>> GetByAllocationAsync(string allocationId, string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(allocationId))
                throw new ArgumentNullException(nameof(allocationId));

            var repo = await CreateRepositoryAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");

            // Query royalty calculations linked to this allocation
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = allocationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
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
        public async Task<List<ROYALTY_PAYMENT>> GetPaymentsAsync(string royaltyId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(royaltyId);
            var repo = await CreateRepositoryAsync<ROYALTY_PAYMENT>("ROYALTY_PAYMENT");
            return (await repo.GetAsync(new List<AppFilter> {
                new() { FieldName = "ROYALTY_CALCULATION_ID", Operator = "=", FilterValue = royaltyId }
            })).OfType<ROYALTY_PAYMENT>().ToList();
        }

        public async Task<ROYALTY_PAYMENT> RecordPaymentAsync(string royaltyId, Guid requestId, decimal amount, string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            if (requestId == Guid.Empty || amount <= 0 || decimal.Round(amount, 2) != amount)
                throw new RoyaltyException("A request ID and a positive payment amount with at most two decimal places are required.");
            var royalty = await GetAsync(royaltyId) ?? throw new RoyaltyException("Stored royalty calculation not found.");
            if (royalty.ACTIVE_IND != _defaults.GetActiveIndicatorYes() ||
                royalty.ROYALTY_STATUS is not (RoyaltyStatus.Accrued or RoyaltyStatus.Paid) ||
                royalty.ROYALTY_AMOUNT is null or <= 0)
                throw new RoyaltyException("An active accrued royalty obligation is required.");
            var payments = await GetPaymentsAsync(royaltyId);
            var retries = payments.Where(p => p.PAYMENT_REQUEST_ID == requestId.ToString()).ToList();
            if (retries.Count != 0)
            {
                if (retries.Count == 1 && retries[0].STATUS == RoyaltyPaymentStatusCodes.Paid &&
                    retries[0].ACTIVE_IND == _defaults.GetActiveIndicatorYes() &&
                    !string.IsNullOrWhiteSpace(retries[0].JOURNAL_ENTRY_ID) && retries[0].NET_PAYMENT_AMOUNT == amount)
                    return retries[0];
                throw new RoyaltyException("Payment request is changed or incomplete. Reconcile the recorded payment before retrying.");
            }
            if (payments.Any(p => p.STATUS != RoyaltyPaymentStatusCodes.Paid ||
                p.ACTIVE_IND != _defaults.GetActiveIndicatorYes() || p.NET_PAYMENT_AMOUNT <= 0 ||
                string.IsNullOrWhiteSpace(p.JOURNAL_ENTRY_ID)) ||
                payments.Select(p => p.ROYALTY_PAYMENT_ID).Distinct().Count() != payments.Count)
                throw new RoyaltyException("Existing payments require reconciliation before another payment can be recorded.");
            var paid = payments.Sum(p => p.NET_PAYMENT_AMOUNT);
            if (royalty.ROYALTY_STATUS == RoyaltyStatus.Paid || amount > royalty.ROYALTY_AMOUNT.Value - paid)
                throw new RoyaltyException("Payment exceeds the outstanding royalty balance.");

            // Every contender for this next payment position gets the same existing primary key.
            // Retain all reservations: deleting one would invalidate this serialization boundary.
            var digest = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
                "royalty-payment:" + royaltyId + ":" + payments.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            var payment = new ROYALTY_PAYMENT {
                ROYALTY_PAYMENT_ID = new Guid(digest.AsSpan(0, 16)).ToString(),
                ROYALTY_CALCULATION_ID = royaltyId, PAYMENT_REQUEST_ID = requestId.ToString(),
                ROYALTY_INTEREST_ID = royalty.ROYALTY_INTEREST_ID, ROYALTY_OWNER_ID = royalty.ROYALTY_OWNER_ID,
                PROPERTY_OR_LEASE_ID = royalty.PROPERTY_OR_LEASE_ID,
                ROYALTY_AMOUNT = amount, NET_PAYMENT_AMOUNT = amount, PAYMENT_DATE = DateTime.UtcNow,
                STATUS = RoyaltyPaymentStatusCodes.Pending, ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(), ROW_CREATED_DATE = DateTime.UtcNow, ROW_CREATED_BY = userId
            };
            var repo = await CreateRepositoryAsync<ROYALTY_PAYMENT>("ROYALTY_PAYMENT");
            try
            {
                await repo.InsertAsync(payment, userId);
                var journal = await _glService.CreateReferencedBalancedEntryAsync(DefaultGlAccounts.AccruedRoyalties,
                    DefaultGlAccounts.Cash, amount, $"Royalty payment {payment.ROYALTY_PAYMENT_ID} for calculation {royaltyId}",
                    userId, payment.ROYALTY_PAYMENT_ID);
                if (journal is null || journal.STATUS != "POSTED" || string.IsNullOrWhiteSpace(journal.JOURNAL_ENTRY_ID))
                    throw new RoyaltyException("Payment journal was not confirmed posted. Reconcile the reserved payment.");
                payment.JOURNAL_ENTRY_ID = journal.JOURNAL_ENTRY_ID;
                // Keep the reservation pending until both calculation and payment writes succeed.
                royalty.ROYALTY_STATUS = paid + amount == royalty.ROYALTY_AMOUNT.Value ? RoyaltyStatus.Paid : RoyaltyStatus.Accrued;
                await UpdateRoyaltyCalculationAsync(royalty, userId, await _resolveConnection());
                payment.STATUS = RoyaltyPaymentStatusCodes.Paid;
                payment.ROW_CHANGED_BY = userId;
                payment.ROW_CHANGED_DATE = DateTime.UtcNow;
                await repo.UpdateAsync(payment, userId);
                return payment;
            }
            catch (Exception ex)
            {
                throw new RoyaltyException("Payment could not be confirmed. Reconcile its recorded reservation before retrying.", ex);
            }
        }

        /// <summary>
        /// Validates a royalty calculation.
        /// Checks: royalty amount <= gross revenue, rate is reasonable, required fields set, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(ROYALTY_CALCULATION royalty, string connectionName = "PPDM39")
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

        private async Task<ALLOCATION_RESULT?> GetAllocationResultAsync(string allocationResultId, string connectionName)
        {
            var repo = await CreateRepositoryAsync<ALLOCATION_RESULT>("ALLOCATION_RESULT");

            var result = await repo.GetByIdAsync(allocationResultId);
            return result as ALLOCATION_RESULT;
        }

        private async Task<RUN_TICKET?> GetRunTicketAsync(string allocationRequestId, string connectionName)
        {
            var repo = await CreateRepositoryAsync<RUN_TICKET>("RUN_TICKET");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_ID", Operator = "=", FilterValue = allocationRequestId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<RUN_TICKET>().FirstOrDefault();
        }

        private async Task<ROYALTY_INTEREST?> GetRoyaltyInterestAsync(
            string leaseId,
            string ownerId,
            DateTime? asOfDate,
            string connectionName)
        {
            var repo = await CreateRepositoryAsync<ROYALTY_INTEREST>("ROYALTY_INTEREST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "ROYALTY_OWNER_ID", Operator = "=", FilterValue = ownerId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            if (!asOfDate.HasValue) throw new RoyaltyException("The effective date for royalty terms is required.");
            var date = asOfDate.Value.Date;
            var matches = (await repo.GetAsync(filters)).OfType<ROYALTY_INTEREST>()
                .Where(interest => (!interest.EFFECTIVE_START_DATE.HasValue || interest.EFFECTIVE_START_DATE.Value.Date <= date) &&
                    (!interest.EFFECTIVE_END_DATE.HasValue || interest.EFFECTIVE_END_DATE.Value.Date >= date))
                .ToList();
            if (matches.Count != 1)
                throw new RoyaltyException("Exactly one active, effective royalty interest must be recorded for the lease and owner.");
            return matches[0];
        }
        private async Task UpdateRoyaltyCalculationAsync(
            ROYALTY_CALCULATION royalty,
            string userId,
            string connectionName)
        {
            var repo = await CreateRepositoryAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");

            royalty.ROW_CHANGED_DATE = DateTime.UtcNow;
            royalty.ROW_CHANGED_BY = userId;
            await repo.UpdateAsync(royalty, userId);
        }

        private static bool MatchesCostType(string? costType, string canonical, string containsToken)
        {
            if (string.IsNullOrEmpty(costType))
                return false;
            if (string.Equals(costType, canonical, StringComparison.OrdinalIgnoreCase))
                return true;
            return costType.Contains(containsToken, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets deductions (transportation, taxes) from cost records.
        /// </summary>
        private async Task<(decimal transportation, decimal adValorem, decimal severance)> GetDeductionsAsync(
            string leaseId, DateTime periodDate, string connectionName)
        {
            try
            {
                var repo = await CreateRepositoryAsync<ACCOUNTING_COST>("ACCOUNTING_COST");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = leaseId },
                    new AppFilter { FieldName = "COST_DATE", Operator = "<=", FilterValue = periodDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                };

                var costs = await repo.GetAsync(filters);
                var costList = costs?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();

                // Lease-level costs lack an allocation-detail link and approval basis in this contract.
                // Applying the entire lease amount to every owner would duplicate deductions.
                if (costList.Any(c => c.AMOUNT != 0 &&
                    (MatchesCostType(c.COST_TYPE, RoyaltyDeductionCostTypeCodes.Transportation, RoyaltyDeductionCostTypeCodes.TransportContainsToken) ||
                     MatchesCostType(c.COST_TYPE, RoyaltyDeductionCostTypeCodes.AdValorem, RoyaltyDeductionCostTypeCodes.AdValoremContainsToken) ||
                     MatchesCostType(c.COST_TYPE, RoyaltyDeductionCostTypeCodes.Severance, RoyaltyDeductionCostTypeCodes.SeveranceContainsToken))))
                    throw new RoyaltyException("Recorded lease deductions require an approved allocation-specific deduction basis before calculation.");

                // Sum only resolved amounts (currently explicit zero deductions).
                decimal transportation = costList
                    .Where(c => MatchesCostType(c.COST_TYPE, RoyaltyDeductionCostTypeCodes.Transportation, RoyaltyDeductionCostTypeCodes.TransportContainsToken))
                    .Sum(c => c.AMOUNT);

                decimal adValorem = costList
                    .Where(c => MatchesCostType(c.COST_TYPE, RoyaltyDeductionCostTypeCodes.AdValorem, RoyaltyDeductionCostTypeCodes.AdValoremContainsToken))
                    .Sum(c => c.AMOUNT);

                decimal severance = costList
                    .Where(c => MatchesCostType(c.COST_TYPE, RoyaltyDeductionCostTypeCodes.Severance, RoyaltyDeductionCostTypeCodes.SeveranceContainsToken))
                    .Sum(c => c.AMOUNT);

                _logger?.LogDebug(
                    "Retrieved deductions for lease {LeaseId}: Transportation=${Transp}, AdValorem=${AdVal}, Severance=${Sev}",
                    leaseId, transportation, adValorem, severance);

                return (transportation, adValorem, severance);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving deductions for lease {LeaseId}", leaseId);
                throw; // A failed lookup is not evidence of zero deductions.
            }
        }
        private async Task<PPDMGenericRepository> CreateRepositoryAsync<T>(string tableName)
        {
            var connection = await _resolveConnection();
            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException("A PRODUCTION database binding is required.");

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), connection, tableName);
        }
    }
}
