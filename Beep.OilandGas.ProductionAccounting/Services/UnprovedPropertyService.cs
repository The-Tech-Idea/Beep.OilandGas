using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Unproved property accounting - acquisition, impairment testing, and reclassification to proved.
    /// </summary>
    public class UnprovedPropertyService : IUnprovedPropertyService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IReserveAccountingService _reserveAccountingService;
        private readonly ILogger<UnprovedPropertyService> _logger;
        private const string ConnectionName = "PPDM39";

        public UnprovedPropertyService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<UnprovedPropertyService> logger = null,
            IReserveAccountingService reserveAccountingService = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _reserveAccountingService = reserveAccountingService;
            _logger = logger;
        }

        public async Task<ACCOUNTING_COST> RecordUnprovedAcquisitionAsync(
            string propertyId,
            decimal cost,
            DateTime acquisitionDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (cost <= 0m)
                throw new AccountingException("Acquisition cost must be positive.");

            var record = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = propertyId,
                AMOUNT = cost,
                COST_DATE = acquisitionDate,
                COST_TYPE = CostTypes.Acquisition,
                COST_CATEGORY = CostCategories.Lease,
                IS_CAPITALIZED = "Y",
                IS_EXPENSED = "N",
                DESCRIPTION = "Unproved property acquisition",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(record, userId);

            _logger?.LogInformation(
                "Recorded unproved acquisition for property {PropertyId} amount {Amount}",
                propertyId, cost);

            return record;
        }

        public async Task<LEASEHOLD_CARRYING_GROUP> CreateCarryingGroupAsync(
            string propertyId,
            string groupName,
            DateTime effectiveDate,
            DateTime? expiryDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentNullException(nameof(groupName));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var group = new LEASEHOLD_CARRYING_GROUP
            {
                LEASEHOLD_CARRYING_GROUP_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = propertyId,
                GROUP_NAME = groupName,
                EFFECTIVE_DATE = effectiveDate,
                EXPIRY_DATE = expiryDate,
                STATUS = "ACTIVE",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<LEASEHOLD_CARRYING_GROUP>("LEASEHOLD_CARRYING_GROUP", cn);
            await repo.InsertAsync(group, userId);
            return group;
        }

        public async Task<LEASE_OPTION> RecordLeaseOptionAsync(
            string leaseId,
            DateTime optionDate,
            DateTime optionExpiryDate,
            decimal bonusAmount,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var option = new LEASE_OPTION
            {
                LEASE_OPTION_ID = Guid.NewGuid().ToString(),
                LEASE_ID = leaseId,
                OPTION_DATE = optionDate,
                OPTION_EXPIRY_DATE = optionExpiryDate,
                BONUS_AMOUNT = bonusAmount,
                STATUS = "ACTIVE",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<LEASE_OPTION>("LEASE_OPTION", cn);
            await repo.InsertAsync(option, userId);

            if (bonusAmount > 0m)
            {
                await RecordLeaseRelatedCostAsync(
                    leaseId,
                    bonusAmount,
                    optionDate,
                    CostCategories.LeaseOption,
                    "Lease option bonus",
                    userId,
                    cn);
            }

            return option;
        }

        public async Task<DELAY_RENTAL> RecordDelayRentalAsync(
            string leaseId,
            DateTime rentalDate,
            decimal amount,
            DateTime? nextDueDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var rental = new DELAY_RENTAL
            {
                DELAY_RENTAL_ID = Guid.NewGuid().ToString(),
                LEASE_ID = leaseId,
                RENTAL_DATE = rentalDate,
                AMOUNT = amount,
                NEXT_DUE_DATE = nextDueDate,
                STATUS = "PAID",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<DELAY_RENTAL>("DELAY_RENTAL", cn);
            await repo.InsertAsync(rental, userId);

            if (amount > 0m)
            {
                await RecordLeaseRelatedCostAsync(
                    leaseId,
                    amount,
                    rentalDate,
                    CostCategories.DelayRental,
                    "Delay rental expense",
                    userId,
                    cn);
            }

            return rental;
        }

        public async Task<List<LEASE_EXPIRY_EVENT>> EvaluateExpiriesAsync(
            DateTime asOfDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var events = new List<LEASE_EXPIRY_EVENT>();
            var leaseRepo = await CreateRepoAsync<LEASE_CONTRACT>("LEASE_CONTRACT", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var leases = await leaseRepo.GetAsync(filters);
            var leaseList = leases?.Cast<LEASE_CONTRACT>().ToList() ?? new List<LEASE_CONTRACT>();
            var expired = leaseList.Where(l => l.EXPIRY_DATE.HasValue && l.EXPIRY_DATE.Value.Date <= asOfDate.Date).ToList();

            foreach (var lease in expired)
            {
                var evt = new LEASE_EXPIRY_EVENT
                {
                    LEASE_EXPIRY_EVENT_ID = Guid.NewGuid().ToString(),
                    LEASE_ID = lease.LEASE_ID,
                    EXPIRY_DATE = lease.EXPIRY_DATE,
                    ACTION_TAKEN = "WRITE_OFF",
                    NOTES = "Lease expired; unproved costs written off",
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var eventRepo = await CreateRepoAsync<LEASE_EXPIRY_EVENT>("LEASE_EXPIRY_EVENT", cn);
                await eventRepo.InsertAsync(evt, userId);
                events.Add(evt);

                await WriteOffUnprovedCostsAsync(lease.LEASE_ID, asOfDate, userId, cn);
            }

            return events;
        }

        public async Task<ACCOUNTING_COST?> TestImpairmentAsync(
            string propertyId,
            DateTime asOfDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var unprovedCosts = await GetUnprovedCostsAsync(propertyId, cn);
            var carryingAmount = unprovedCosts.Sum(c => c.AMOUNT);
            if (carryingAmount <= 0m)
            {
                _logger?.LogInformation("No unproved costs found for property {PropertyId}", propertyId);
                return null;
            }

            var pv = 0m;
            if (_reserveAccountingService != null)
            {
                pv = await _reserveAccountingService.CalculatePresentValueAsync(propertyId, asOfDate, cn);
            }

            if (pv <= 0m || pv >= carryingAmount)
            {
                _logger?.LogInformation(
                    "Impairment test passed for property {PropertyId}: carrying {Carrying} pv {PV}",
                    propertyId, carryingAmount, pv);
                return null;
            }

            var impairmentAmount = carryingAmount - pv;
            var impairment = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = propertyId,
                AMOUNT = impairmentAmount,
                COST_DATE = asOfDate,
                COST_TYPE = "IMPAIRMENT",
                COST_CATEGORY = CostCategories.Lease,
                IS_CAPITALIZED = "N",
                IS_EXPENSED = "Y",
                DESCRIPTION = $"Unproved property impairment (PV {pv:0.##})",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(impairment, userId);

            _logger?.LogWarning(
                "Impairment recorded for property {PropertyId}: write-down {Amount}",
                propertyId, impairmentAmount);

            return impairment;
        }

        public async Task<bool> ReclassifyToProvedAsync(
            string propertyId,
            DateTime effectiveDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "COST_TYPE", Operator = "=", FilterValue = CostTypes.Acquisition },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var costs = results?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();
            if (!costs.Any())
                return false;

            foreach (var cost in costs)
            {
                cost.COST_TYPE = CostTypes.Development;
                cost.IS_CAPITALIZED = "Y";
                cost.IS_EXPENSED = "N";
                cost.ROW_CHANGED_BY = userId;
                cost.ROW_CHANGED_DATE = DateTime.UtcNow;
                cost.REMARK = "Reclassified to proved property";

                await repo.UpdateAsync(cost, userId);
            }

            _logger?.LogInformation(
                "Reclassified {Count} unproved costs to proved for property {PropertyId}",
                costs.Count, propertyId);

            return true;
        }

        private async Task<List<ACCOUNTING_COST>> GetUnprovedCostsAsync(string propertyId, string cn)
        {
            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "COST_TYPE", Operator = "=", FilterValue = CostTypes.Acquisition },
                new AppFilter { FieldName = "IS_CAPITALIZED", Operator = "=", FilterValue = "Y" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();
        }

        private async Task RecordLeaseRelatedCostAsync(
            string leaseId,
            decimal amount,
            DateTime costDate,
            string category,
            string description,
            string userId,
            string cn)
        {
            var record = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = leaseId,
                AMOUNT = amount,
                COST_DATE = costDate,
                COST_TYPE = CostTypes.Acquisition,
                COST_CATEGORY = category,
                IS_CAPITALIZED = "N",
                IS_EXPENSED = "Y",
                DESCRIPTION = description,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(record, userId);
        }

        private async Task WriteOffUnprovedCostsAsync(
            string propertyId,
            DateTime asOfDate,
            string userId,
            string cn)
        {
            var unproved = await GetUnprovedCostsAsync(propertyId, cn);
            var carrying = unproved.Sum(c => c.AMOUNT);
            if (carrying <= 0m)
                return;

            var writeOff = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = propertyId,
                AMOUNT = carrying,
                COST_DATE = asOfDate,
                COST_TYPE = "EXPIRY_WRITE_OFF",
                COST_CATEGORY = CostCategories.Lease,
                IS_CAPITALIZED = "N",
                IS_EXPENSED = "Y",
                DESCRIPTION = "Unproved lease expiry write-off",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(writeOff, userId);
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
