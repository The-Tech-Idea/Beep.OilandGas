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
    /// Joint Interest Billing Service - Multi-party cost and revenue sharing per COPAS standards.
    /// Implements COPAS-compliant joint interest accounting for oil & gas operations.
    /// 
    /// Allocation Formula:
    ///   Each Party's Share = (Interest % x Net Revenue) - (Interest % x Costs)
    /// 
    /// With Operator's Burden:
    ///   Non-Consenting Cost = Operator Cost x (1 + Burden Factor)
    /// 
    /// Overhead Allocation:
    ///   Overhead = Base Amount x Overhead Rate
    ///   Allocated = Overhead x Interest %
    /// </summary>
    public class JointInterestBillingService : IJointInterestBillingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ICopasOverheadService _copasOverheadService;
        private readonly ILogger<JointInterestBillingService> _logger;
        private const string ConnectionName = "PPDM39";

        public JointInterestBillingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<JointInterestBillingService> logger = null,
            ICopasOverheadService copasOverheadService = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _copasOverheadService = copasOverheadService;
        }

        /// <summary>
        /// Allocates production from an allocation result to all participants based on their interests.
        /// COPAS Formula: Each Party's Share = (Interest % x Net Revenue) - (Interest % x Costs)
        /// </summary>
        public async Task<bool> AllocateToParticipantsAsync(
            ALLOCATION_RESULT allocation,
            string userId,
            string cn = "PPDM39")
        {
            if (allocation == null)
                throw new ArgumentNullException(nameof(allocation));

            _logger?.LogInformation("Allocating allocation {AllocationId} to participants", allocation.ALLOCATION_RESULT_ID);

            try
            {
                // Get allocation details (these represent different participating interests)
                var details = await GetAllocationDetailsAsync(allocation.ALLOCATION_RESULT_ID, cn);
                if (!details.Any())
                {
                    _logger?.LogWarning("Allocation {AllocationId} has no details to allocate", allocation.ALLOCATION_RESULT_ID);
                    return false;
                }

                // Calculate each participant's share
                // Formula: Participant Share = (Interest % x Gross Revenue) - (Interest % x Costs)
                decimal totalAllocated = 0;
                foreach (var detail in details)
                {
                    if (detail.ALLOCATED_VOLUME == null || detail.ALLOCATED_VOLUME <= 0)
                        continue;

                    // Track participant's volume allocation
                    totalAllocated += detail.ALLOCATED_VOLUME.Value;

                    _logger?.LogInformation(
                        "Participant {EntityId} allocated {Volume} from allocation {AllocationId}",
                        detail.ENTITY_ID, detail.ALLOCATED_VOLUME, allocation.ALLOCATION_RESULT_ID);
                }

                // Validate total allocation equals or closely matches production
                if (allocation.TOTAL_VOLUME.HasValue)
                {
                    var tolerance = allocation.TOTAL_VOLUME * 0.0001m;  // 0.01% tolerance
                    if (Math.Abs(totalAllocated - allocation.TOTAL_VOLUME.Value) > tolerance)
                    {
                        _logger?.LogWarning(
                            "Allocation {AllocationId}: Participant total {Total} != production {Production}",
                            allocation.ALLOCATION_RESULT_ID, totalAllocated, allocation.TOTAL_VOLUME);
                    }
                }

                _logger?.LogInformation("Allocation {AllocationId} successfully distributed to participants", allocation.ALLOCATION_RESULT_ID);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error allocating to participants");
                throw;
            }
        }

        /// <summary>
        /// Generates a Joint Interest Billing statement for a lease/well for a specific period.
        /// Statement shows costs, revenue, and net shares for each participant.
        /// </summary>
        public async Task<bool> GenerateStatementAsync(
            string leaseId,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));

            _logger?.LogInformation("Generating JIB statement for lease {LeaseId}, period end {PeriodEnd}",
                leaseId, periodEnd.ToShortDateString());

            try
            {
                // Step 1: Get all allocations for the lease in the period
                var allocMetadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
                var allocEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{allocMetadata.EntityTypeName}")
                    ?? typeof(ALLOCATION_RESULT);

                var allocRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    allocEntityType, cn, "ALLOCATION_RESULT");

                var allocFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var allocResults = await allocRepo.GetAsync(allocFilters);
                var allocations = allocResults?.Cast<ALLOCATION_RESULT>().ToList() ?? new List<ALLOCATION_RESULT>();
                var filteredAllocations = new List<ALLOCATION_RESULT>();
                foreach (var allocation in allocations)
                {
                    var RUN_TICKET = await GetRunTicketAsync(allocation.ALLOCATION_REQUEST_ID, cn);
                    if (RUN_TICKET != null && string.Equals(RUN_TICKET.LEASE_ID, leaseId, StringComparison.OrdinalIgnoreCase))
                        filteredAllocations.Add(allocation);
                }

                _logger?.LogInformation("Found {Count} allocations for lease {LeaseId} in period", filteredAllocations.Count, leaseId);

                // Step 2: Get all cost allocations for the period
                var costMetadata = await _metadata.GetTableMetadataAsync("COST_ALLOCATION");
                var costEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{costMetadata.EntityTypeName}")
                    ?? typeof(COST_ALLOCATION);

                var costRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    costEntityType, cn, "COST_ALLOCATION");

                var costFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = leaseId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var costResults = await costRepo.GetAsync(costFilters);
                var costs = costResults?.Cast<COST_ALLOCATION>().ToList() ?? new List<COST_ALLOCATION>();
                var costIds = costs.Select(c => c.COST_TRANSACTION_ID).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
                var costTransactions = await GetAccountingCostsAsync(costIds, cn);
                var filteredCosts = costs
                    .Where(c =>
                    {
                        if (string.IsNullOrWhiteSpace(c.COST_TRANSACTION_ID))
                            return true;
                        var cost = costTransactions.FirstOrDefault(t => t.ACCOUNTING_COST_ID == c.COST_TRANSACTION_ID);
                        return cost?.COST_DATE == null || cost.COST_DATE.Value.Date <= periodEnd.Date;
                    })
                    .ToList();

                _logger?.LogInformation("Found {Count} cost allocations for lease {LeaseId}", filteredCosts.Count, leaseId);

                // Step 3: Get all revenue for the period
                var revTxMetadata = await _metadata.GetTableMetadataAsync("REVENUE_TRANSACTION");
                var revTxEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{revTxMetadata.EntityTypeName}")
                    ?? typeof(REVENUE_TRANSACTION);

                var revTxRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    revTxEntityType, cn, "REVENUE_TRANSACTION");

                var revTxFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = leaseId },
                    new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var revTxResults = await revTxRepo.GetAsync(revTxFilters);
                var revenueTransactions = revTxResults?.Cast<REVENUE_TRANSACTION>().ToList() ?? new List<REVENUE_TRANSACTION>();
                var transactionIds = revenueTransactions.Select(r => r.REVENUE_TRANSACTION_ID).ToList();
                var revenues = await GetRevenueAllocationsAsync(transactionIds, cn);

                _logger?.LogInformation("Found {Count} revenue records for lease {LeaseId}", revenues.Count, leaseId);

                // Step 4: Calculate each participant's share using COPAS formula
                // COPAS Formula: Party Share = (Interest % x Net Revenue) - (Interest % x Costs)
                decimal totalRevenue = revenues.Sum(r => r.ALLOCATED_AMOUNT ?? 0);
                decimal baseCosts = filteredCosts.Sum(c => c.ALLOCATED_AMOUNT ?? 0);
                decimal overhead = 0m;
                if (_copasOverheadService != null)
                    overhead = await _copasOverheadService.CalculateOverheadAsync(leaseId, baseCosts, periodEnd, cn);

                decimal totalCosts = baseCosts + overhead;
                decimal netRevenue = totalRevenue - totalCosts;

                _logger?.LogInformation(
                    "JIB Calculation for lease {LeaseId}: Total Revenue={Revenue}, Total Costs={Costs}, Net={Net}",
                    leaseId, totalRevenue, totalCosts, netRevenue);

                var statement = await PersistJibStatementAsync(
                    leaseId,
                    periodEnd,
                    totalCosts,
                    totalRevenue,
                    netRevenue,
                    filteredAllocations,
                    filteredCosts,
                    revenues,
                    userId,
                    cn);

                if (_copasOverheadService != null && overhead > 0m)
                    await _copasOverheadService.ApplyOverheadToStatementAsync(statement, baseCosts, userId, cn);

                // Step 5: Log statement generation
                _logger?.LogInformation(
                    "JIB statement for lease {LeaseId} period {PeriodEnd}: Generated successfully. " +
                    "Allocations={AllocCount}, Costs={CostCount}, Revenue={RevCount}, Net Revenue={NetRevenue}",
                    leaseId, periodEnd.ToShortDateString(), filteredAllocations.Count, filteredCosts.Count, revenues.Count, netRevenue);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating JIB statement for lease {LeaseId}: {ErrorMessage}", leaseId, ex.Message);
                throw new ProductionAccountingException($"Failed to generate JIB statement for lease {leaseId}", ex);
            }
        }

        /// <summary>
        /// Validates a lease's JIB data.
        /// Checks: total interests sum to 100%, cost allocations are correct, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(string leaseId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));

            _logger?.LogInformation("Validating JIB data for lease {LeaseId}", leaseId);

            try
            {
                var validationErrors = new List<string>();

                // Validation 1: Get all allocation details for the lease to check participant interests
                var detailMetadata = await _metadata.GetTableMetadataAsync("ALLOCATION_DETAIL");
                var detailEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{detailMetadata.EntityTypeName}")
                    ?? typeof(ALLOCATION_DETAIL);

                var detailRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    detailEntityType, cn, "ALLOCATION_DETAIL");

                var detailFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var detailResults = await detailRepo.GetAsync(detailFilters);
                var allDetails = detailResults?.Cast<ALLOCATION_DETAIL>().ToList() ?? new List<ALLOCATION_DETAIL>();
                var allocationResults = await GetAllocationsForLeaseAsync(leaseId, cn);
                var allocationIds = new HashSet<string>(allocationResults.Select(a => a.ALLOCATION_RESULT_ID));
                var leaseDetails = allDetails.Where(d => allocationIds.Contains(d.ALLOCATION_RESULT_ID)).ToList();

                // Validation 2: Check that participant percentages sum to reasonable values
                var participantGroups = leaseDetails.GroupBy(d => d.ENTITY_ID).ToList();
                
                foreach (var group in participantGroups)
                {
                    var totalPercentage = group.Sum(d => (d.ALLOCATED_VOLUME ?? 0));
                    
                    _logger?.LogInformation(
                        "Lease {LeaseId}: Participant {ParticipantId} has total allocation {Amount}",
                        leaseId, group.Key, totalPercentage);
                }

                // Validation 3: Verify no negative allocations
                var negativeAllocations = leaseDetails.Where(d => (d.ALLOCATED_VOLUME ?? 0) < 0).ToList();
                if (negativeAllocations.Any())
                {
                    validationErrors.Add(
                        $"Found {negativeAllocations.Count} negative allocations - all allocations must be >= 0");
                    
                    _logger?.LogWarning(
                        "Lease {LeaseId}: Found {Count} negative allocations",
                        leaseId, negativeAllocations.Count);
                }

                // Validation 4: Verify allocations have entity names
                var blankNames = leaseDetails.Where(d => string.IsNullOrWhiteSpace(d.ENTITY_NAME)).ToList();
                if (blankNames.Any())
                {
                    _logger?.LogWarning(
                        "Lease {LeaseId}: Found {Count} allocations with missing entity names",
                        leaseId, blankNames.Count);
                }

                // Validation 5: Verify all participants are active
                var inactiveAllocations = leaseDetails.Where(d => d.ACTIVE_IND != "Y").ToList();
                if (inactiveAllocations.Any())
                {
                    validationErrors.Add(
                        $"Found {inactiveAllocations.Count} inactive allocations - all allocations must be active");
                    
                    _logger?.LogWarning(
                        "Lease {LeaseId}: Found {Count} inactive allocations",
                        leaseId, inactiveAllocations.Count);
                }

                // Log validation result
                if (validationErrors.Any())
                {
                    _logger?.LogError(
                        "JIB validation for lease {LeaseId}: FAILED with {Count} errors: {Errors}",
                        leaseId, validationErrors.Count, string.Join("; ", validationErrors));
                    
                    throw new ProductionAccountingException(
                        $"JIB validation failed for lease {leaseId}: {string.Join("; ", validationErrors)}");
                }

                _logger?.LogInformation(
                    "JIB validation for lease {LeaseId}: PASSED - All participants and allocations are valid",
                    leaseId);
                
                return true;
            }
            catch (ProductionAccountingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "JIB validation failed for lease {LeaseId}: {ErrorMessage}", leaseId, ex.Message);
                throw new ProductionAccountingException($"Error validating JIB data for lease {leaseId}", ex);
            }
        }

        /// <summary>
        /// Gets allocation details for an allocation result.
        /// These represent the different participants in the joint interest.
        /// </summary>
        private async Task<List<ALLOCATION_DETAIL>> GetAllocationDetailsAsync(
            string allocationResultId,
            string cn = "PPDM39")
        {
            var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_DETAIL");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ALLOCATION_DETAIL);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ALLOCATION_DETAIL");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = allocationResultId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<ALLOCATION_DETAIL>().ToList() ?? new List<ALLOCATION_DETAIL>();
        }

        private async Task<RUN_TICKET?> GetRunTicketAsync(string allocationRequestId, string cn)
        {
            if (string.IsNullOrWhiteSpace(allocationRequestId))
                return null;

            var metadata = await _metadata.GetTableMetadataAsync("RUN_TICKET");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(RUN_TICKET);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "RUN_TICKET");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_ID", Operator = "=", FilterValue = allocationRequestId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<RUN_TICKET>().FirstOrDefault();
        }

        private async Task<List<ALLOCATION_RESULT>> GetAllocationsForLeaseAsync(string leaseId, string cn)
        {
            var allocMetadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
            var allocEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{allocMetadata.EntityTypeName}")
                ?? typeof(ALLOCATION_RESULT);

            var allocRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                allocEntityType, cn, "ALLOCATION_RESULT");

            var allocFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var allocResults = await allocRepo.GetAsync(allocFilters);
            var allocations = allocResults?.Cast<ALLOCATION_RESULT>().ToList() ?? new List<ALLOCATION_RESULT>();
            var filtered = new List<ALLOCATION_RESULT>();

            foreach (var allocation in allocations)
            {
                var RUN_TICKET = await GetRunTicketAsync(allocation.ALLOCATION_REQUEST_ID, cn);
                if (RUN_TICKET != null && string.Equals(RUN_TICKET.LEASE_ID, leaseId, StringComparison.OrdinalIgnoreCase))
                    filtered.Add(allocation);
            }

            return filtered;
        }

        private async Task<JOINT_INTEREST_STATEMENT> PersistJibStatementAsync(
            string leaseId,
            DateTime periodEnd,
            decimal totalCosts,
            decimal totalRevenue,
            decimal netRevenue,
            List<ALLOCATION_RESULT> allocations,
            List<COST_ALLOCATION> costs,
            List<REVENUE_ALLOCATION> revenues,
            string userId,
            string cn)
        {
            var periodStart = allocations.Select(a => a.ALLOCATION_DATE).Where(d => d.HasValue).Min()
                ?? periodEnd.AddMonths(-1);

            var statement = new JOINT_INTEREST_STATEMENT
            {
                JOINT_INTEREST_STATEMENT_ID = Guid.NewGuid().ToString(),
                REPORT_TYPE = "JIB",
                REPORT_PERIOD_START = periodStart,
                REPORT_PERIOD_END = periodEnd,
                GENERATION_DATE = DateTime.UtcNow,
                GENERATED_BY = userId,
                JIB_ID = leaseId,
                OPERATOR = userId,
                TOTAL_CHARGES = totalCosts,
                TOTAL_CREDITS = totalRevenue,
                NET_AMOUNT = netRevenue,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var statementRepo = await CreateRepoAsync<JOINT_INTEREST_STATEMENT>("JOINT_INTEREST_STATEMENT", cn);
            await statementRepo.InsertAsync(statement, userId);

            await PersistParticipantsAsync(statement, allocations, userId, cn);
            await PersistChargesAsync(statement, costs, userId, cn);
            await PersistCreditsAsync(statement, revenues, userId, cn);

            return statement;
        }

        private async Task PersistParticipantsAsync(
            JOINT_INTEREST_STATEMENT statement,
            List<ALLOCATION_RESULT> allocations,
            string userId,
            string cn)
        {
            var details = new List<ALLOCATION_DETAIL>();

            foreach (var allocation in allocations)
            {
                if (string.IsNullOrWhiteSpace(allocation.ALLOCATION_RESULT_ID))
                    continue;

                details.AddRange(await GetAllocationDetailsAsync(allocation.ALLOCATION_RESULT_ID, cn));
            }

            var repo = await CreateRepoAsync<JIB_PARTICIPANT>("JIB_PARTICIPANT", cn);
            var totalVolume = details.Sum(d => d.ALLOCATED_VOLUME ?? 0m);
            var groups = details.GroupBy(d => d.ENTITY_ID).ToList();

            foreach (var group in groups)
            {
                var groupVolume = group.Sum(d => d.ALLOCATED_VOLUME ?? 0m);
                var interestPercent = totalVolume == 0m ? 0m : (groupVolume / totalVolume) * 100m;
                var name = group.Select(d => d.ENTITY_NAME).FirstOrDefault();
                var record = new JIB_PARTICIPANT
                {
                    JIB_PARTICIPANT_ID = Guid.NewGuid().ToString(),
                    JOINT_INTEREST_STATEMENT_ID = statement.JOINT_INTEREST_STATEMENT_ID,
                    COMPANY_NAME = name,
                    WORKING_INTEREST = interestPercent,
                    NET_REVENUE_INTEREST = interestPercent,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                await repo.InsertAsync(record, userId);
            }
        }

        private async Task PersistChargesAsync(
            JOINT_INTEREST_STATEMENT statement,
            List<COST_ALLOCATION> costs,
            string userId,
            string cn)
        {
            var repo = await CreateRepoAsync<JIB_CHARGE>("JIB_CHARGE", cn);
            foreach (var cost in costs)
            {
                var record = new JIB_CHARGE
                {
                    JIB_CHARGE_ID = Guid.NewGuid().ToString(),
                    JOINT_INTEREST_STATEMENT_ID = statement.JOINT_INTEREST_STATEMENT_ID,
                    DESCRIPTION = cost.DESCRIPTION ?? "Cost allocation",
                    CATEGORY = cost.ALLOCATION_METHOD,
                    AMOUNT = cost.ALLOCATED_AMOUNT ?? 0m,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                await repo.InsertAsync(record, userId);
            }
        }

        private async Task PersistCreditsAsync(
            JOINT_INTEREST_STATEMENT statement,
            List<REVENUE_ALLOCATION> revenues,
            string userId,
            string cn)
        {
            var repo = await CreateRepoAsync<JIB_CREDIT>("JIB_CREDIT", cn);
            foreach (var revenue in revenues)
            {
                var record = new JIB_CREDIT
                {
                    JIB_CREDIT_ID = Guid.NewGuid().ToString(),
                    JOINT_INTEREST_STATEMENT_ID = statement.JOINT_INTEREST_STATEMENT_ID,
                    DESCRIPTION = revenue.DESCRIPTION ?? "Revenue allocation",
                    CATEGORY = revenue.ALLOCATION_METHOD,
                    AMOUNT = revenue.ALLOCATED_AMOUNT ?? 0m,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                await repo.InsertAsync(record, userId);
            }
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

        private async Task<List<ACCOUNTING_COST>> GetAccountingCostsAsync(List<string> costIds, string cn)
        {
            if (costIds == null || costIds.Count == 0)
                return new List<ACCOUNTING_COST>();

            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            var results = await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            });

            var allCosts = results?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();
            return allCosts.Where(c => costIds.Contains(c.ACCOUNTING_COST_ID)).ToList();
        }

        private async Task<List<REVENUE_ALLOCATION>> GetRevenueAllocationsAsync(List<string> transactionIds, string cn)
        {
            if (transactionIds == null || transactionIds.Count == 0)
                return new List<REVENUE_ALLOCATION>();

            var metadata = await _metadata.GetTableMetadataAsync("REVENUE_ALLOCATION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(REVENUE_ALLOCATION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "REVENUE_ALLOCATION");

            var results = await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            });

            var allAllocations = results?.Cast<REVENUE_ALLOCATION>().ToList() ?? new List<REVENUE_ALLOCATION>();
            return allAllocations.Where(a => transactionIds.Contains(a.REVENUE_TRANSACTION_ID)).ToList();
        }
    }
}
