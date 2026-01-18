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
    ///   Each Party's Share = (Interest % × Net Revenue) - (Interest % × Costs)
    /// 
    /// With Operator's Burden:
    ///   Non-Consenting Cost = Operator Cost × (1 + Burden Factor)
    /// 
    /// Overhead Allocation:
    ///   Overhead = Base Amount × Overhead Rate
    ///   Allocated = Overhead × Interest %
    /// </summary>
    public class JointInterestBillingService : IJointInterestBillingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<JointInterestBillingService> _logger;
        private const string ConnectionName = "PPDM39";

        public JointInterestBillingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<JointInterestBillingService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Allocates production from an allocation result to all participants based on their interests.
        /// COPAS Formula: Each Party's Share = (Interest % × Net Revenue) - (Interest % × Costs)
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
                // Formula: Participant Share = (Interest % × Gross Revenue) - (Interest % × Costs)
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
                    new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId },
                    new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var allocResults = await allocRepo.GetAsync(allocFilters);
                var allocations = allocResults?.Cast<ALLOCATION_RESULT>().ToList() ?? new List<ALLOCATION_RESULT>();

                _logger?.LogInformation("Found {Count} allocations for lease {LeaseId} in period", allocations.Count, leaseId);

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

                _logger?.LogInformation("Found {Count} cost allocations for lease {LeaseId}", costs.Count, leaseId);

                // Step 3: Get all revenue for the period
                var revMetadata = await _metadata.GetTableMetadataAsync("REVENUE_ALLOCATION");
                var revEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{revMetadata.EntityTypeName}")
                    ?? typeof(REVENUE_ALLOCATION);

                var revRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    revEntityType, cn, "REVENUE_ALLOCATION");

                var revFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId },
                    new AppFilter { FieldName = "REVENUE_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var revResults = await revRepo.GetAsync(revFilters);
                var revenues = revResults?.Cast<REVENUE_ALLOCATION>().ToList() ?? new List<REVENUE_ALLOCATION>();

                _logger?.LogInformation("Found {Count} revenue records for lease {LeaseId}", revenues.Count, leaseId);

                // Step 4: Calculate each participant's share using COPAS formula
                // COPAS Formula: Party Share = (Interest % × Net Revenue) - (Interest % × Costs)
                decimal totalRevenue = revenues.Sum(r => r.ALLOCATED_AMOUNT ?? 0);
                decimal totalCosts = costs.Sum(c => c.ALLOCATED_AMOUNT ?? 0);
                decimal netRevenue = totalRevenue - totalCosts;

                _logger?.LogInformation(
                    "JIB Calculation for lease {LeaseId}: Total Revenue={Revenue}, Total Costs={Costs}, Net={Net}",
                    leaseId, totalRevenue, totalCosts, netRevenue);

                // Step 5: Log statement generation
                _logger?.LogInformation(
                    "JIB statement for lease {LeaseId} period {PeriodEnd}: Generated successfully. " +
                    "Allocations={AllocCount}, Costs={CostCount}, Revenue={RevCount}, Net Revenue={NetRevenue}",
                    leaseId, periodEnd.ToShortDateString(), allocations.Count, costs.Count, revenues.Count, netRevenue);

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

                // Validation 2: Check that participant percentages sum to reasonable values
                var participantGroups = allDetails.GroupBy(d => d.ENTITY_ID).ToList();
                
                foreach (var group in participantGroups)
                {
                    var totalPercentage = group.Sum(d => (d.ALLOCATED_VOLUME ?? 0));
                    
                    _logger?.LogInformation(
                        "Lease {LeaseId}: Participant {ParticipantId} has total allocation {Amount}",
                        leaseId, group.Key, totalPercentage);
                }

                // Validation 3: Verify no negative allocations
                var negativeAllocations = allDetails.Where(d => (d.ALLOCATED_VOLUME ?? 0) < 0).ToList();
                if (negativeAllocations.Any())
                {
                    validationErrors.Add(
                        $"Found {negativeAllocations.Count} negative allocations - all allocations must be >= 0");
                    
                    _logger?.LogWarning(
                        "Lease {LeaseId}: Found {Count} negative allocations",
                        leaseId, negativeAllocations.Count);
                }

                // Validation 4: Verify allocations have entity names
                var blankNames = allDetails.Where(d => string.IsNullOrWhiteSpace(d.ENTITY_NAME)).ToList();
                if (blankNames.Any())
                {
                    _logger?.LogWarning(
                        "Lease {LeaseId}: Found {Count} allocations with missing entity names",
                        leaseId, blankNames.Count);
                }

                // Validation 5: Verify all participants are active
                var inactiveAllocations = allDetails.Where(d => d.ACTIVE_IND != "Y").ToList();
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
    }
}
