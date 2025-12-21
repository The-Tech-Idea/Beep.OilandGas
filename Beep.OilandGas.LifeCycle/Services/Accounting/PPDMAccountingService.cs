using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Accounting
{
    /// <summary>
    /// Service for Production Accounting operations
    /// Uses PPDMGenericRepository to store and retrieve accounting data from PPDM database
    /// </summary>
    public class PPDMAccountingService : IAccountingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<PPDMAccountingService>? _logger;

        // Cache for repositories
        private PPDMGenericRepository? _productionRepository;
        private PPDMGenericRepository? _accountingAllocationRepository;
        private PPDMGenericRepository? _royaltyCalculationRepository;
        private PPDMGenericRepository? _costAllocationRepository;

        public PPDMAccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<PPDMAccountingService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        #region Repository Helpers

        private async Task<PPDMGenericRepository> GetProductionRepositoryAsync()
        {
            if (_productionRepository == null)
            {
                var metadata = await _metadata.GetTableMetadataAsync("PDEN_VOL_SUMMARY");
                var entityType = typeof(Dictionary<string, object>);
                
                if (metadata != null && !string.IsNullOrEmpty(metadata.EntityTypeName))
                {
                    var resolvedType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                    if (resolvedType != null)
                    {
                        entityType = resolvedType;
                    }
                }

                _productionRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PDEN_VOL_SUMMARY", null);
            }
            return _productionRepository;
        }

        private async Task<PPDMGenericRepository> GetAccountingAllocationRepositoryAsync()
        {
            if (_accountingAllocationRepository == null)
            {
                // Use DTO type from DTOs namespace since PPDM model doesn't exist
                var dtoType = typeof(Beep.OilandGas.PPDM39.Core.DTOs.ACCOUNTING_ALLOCATION);
                _accountingAllocationRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    dtoType, _connectionName, "ACCOUNTING_ALLOCATION", null);
            }
            return _accountingAllocationRepository;
        }

        private async Task<PPDMGenericRepository> GetRoyaltyCalculationRepositoryAsync()
        {
            if (_royaltyCalculationRepository == null)
            {
                // Use DTO type from DTOs namespace since PPDM model doesn't exist
                var dtoType = typeof(ROYALTY_CALCULATION);
                _royaltyCalculationRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    dtoType, _connectionName, "ROYALTY_CALCULATION", null);
            }
            return _royaltyCalculationRepository;
        }

        private async Task<PPDMGenericRepository> GetCostAllocationRepositoryAsync()
        {
            if (_costAllocationRepository == null)
            {
                // Use DTO type from DTOs namespace since PPDM model doesn't exist
                var dtoType = typeof(COST_ALLOCATION);
                _costAllocationRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    dtoType, _connectionName, "COST_ALLOCATION", null);
            }
            return _costAllocationRepository;
        }

        #endregion

        #region Volume Reconciliation

        public async Task<VolumeReconciliationResult> ReconcileVolumesAsync(string fieldId, DateTime startDate, DateTime endDate, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                _logger?.LogInformation("Reconciling volumes for field {FieldId} from {StartDate} to {EndDate}", fieldId, startDate, endDate);

                var productionRepo = await GetProductionRepositoryAsync();
                var allocationRepo = await GetAccountingAllocationRepositoryAsync();

                // Get field production volumes
                var productionFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "PRODUCTION_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "PRODUCTION_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") }
                };
                if (additionalFilters != null)
                    productionFilters.AddRange(additionalFilters);

                var productionData = await productionRepo.GetAsync(productionFilters);
                
                // Get allocated volumes
                var allocationFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "ALLOCATION_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") }
                };
                var allocationData = await allocationRepo.GetAsync(allocationFilters);

                // Calculate totals (placeholder - would need actual field extraction logic)
                decimal fieldOilVolume = 0;
                decimal allocatedOilVolume = 0;
                decimal fieldGasVolume = 0;
                decimal allocatedGasVolume = 0;

                // TODO: Extract actual volumes from production and allocation data
                // This is a simplified calculation - real implementation would need proper field extraction

                var discrepancy = fieldOilVolume - allocatedOilVolume;
                var discrepancyPercentage = fieldOilVolume > 0 ? (discrepancy / fieldOilVolume) * 100 : 0;

                var result = new VolumeReconciliationResult
                {
                    FieldId = fieldId,
                    StartDate = startDate,
                    EndDate = endDate,
                    FieldProductionVolume = fieldOilVolume + fieldGasVolume,
                    AllocatedVolume = allocatedOilVolume + allocatedGasVolume,
                    Discrepancy = discrepancy,
                    DiscrepancyPercentage = discrepancyPercentage,
                    Status = Math.Abs(discrepancyPercentage) < 0.1m ? ReconciliationStatus.Reconciled : ReconciliationStatus.Discrepancies,
                    OilVolume = new VolumeBreakdown
                    {
                        FieldVolume = fieldOilVolume,
                        AllocatedVolume = allocatedOilVolume,
                        Discrepancy = discrepancy,
                        DiscrepancyPercentage = discrepancyPercentage
                    },
                    Issues = new List<ReconciliationIssue>()
                };

                if (result.Status == ReconciliationStatus.Discrepancies)
                {
                    result.Issues.Add(new ReconciliationIssue
                    {
                        IssueType = "DISCREPANCY",
                        Severity = "WARNING",
                        Description = $"Volume discrepancy of {discrepancyPercentage:F2}% detected"
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error reconciling volumes for field {FieldId}", fieldId);
                throw;
            }
        }

        #endregion

        #region Royalty Calculation

        public async Task<RoyaltyCalculationResult> CalculateRoyaltiesAsync(string fieldId, DateTime startDate, DateTime endDate, string? poolId = null, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                _logger?.LogInformation("Calculating royalties for field {FieldId}, pool {PoolId} from {StartDate} to {EndDate}", 
                    fieldId, poolId ?? "ALL", startDate, endDate);

                var productionRepo = await GetProductionRepositoryAsync();

                // Get production data
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "PRODUCTION_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "PRODUCTION_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") }
                };

                if (!string.IsNullOrEmpty(poolId))
                {
                    filters.Add(new AppFilter { FieldName = "POOL_ID", Operator = "=", FilterValue = poolId });
                }

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var productionData = await productionRepo.GetAsync(filters);

                // TODO: Extract actual production volumes and calculate royalties
                // This is a placeholder - real implementation would need proper field extraction and calculation
                decimal grossOilVolume = 0;
                decimal grossGasVolume = 0;
                decimal oilRoyaltyRate = 12.5m; // Default 12.5%
                decimal gasRoyaltyRate = 12.5m;

                var royaltyOilVolume = grossOilVolume * (oilRoyaltyRate / 100);
                var royaltyGasVolume = grossGasVolume * (gasRoyaltyRate / 100);

                var result = new RoyaltyCalculationResult
                {
                    FieldId = fieldId,
                    PoolId = poolId,
                    StartDate = startDate,
                    EndDate = endDate,
                    GrossOilVolume = grossOilVolume,
                    GrossGasVolume = grossGasVolume,
                    OilRoyaltyRate = oilRoyaltyRate,
                    GasRoyaltyRate = gasRoyaltyRate,
                    RoyaltyOilVolume = royaltyOilVolume,
                    RoyaltyGasVolume = royaltyGasVolume,
                    NetOilVolume = grossOilVolume - royaltyOilVolume,
                    NetGasVolume = grossGasVolume - royaltyGasVolume
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating royalties for field {FieldId}", fieldId);
                throw;
            }
        }

        #endregion

        #region Cost Allocation

        public async Task<CostAllocationResult> AllocateCostsAsync(string fieldId, DateTime startDate, DateTime endDate, CostAllocationMethod method, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                _logger?.LogInformation("Allocating costs for field {FieldId} using method {Method} from {StartDate} to {EndDate}", 
                    fieldId, method, startDate, endDate);

                var productionRepo = await GetProductionRepositoryAsync();

                // Get production data for allocation basis
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "PRODUCTION_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "PRODUCTION_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var productionData = await productionRepo.GetAsync(filters);

                // TODO: Implement actual cost allocation based on method
                // Placeholder implementation
                var result = new CostAllocationResult
                {
                    FieldId = fieldId,
                    StartDate = startDate,
                    EndDate = endDate,
                    AllocationMethod = method,
                    AllocationDetails = new List<CostAllocationDetail>()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error allocating costs for field {FieldId}", fieldId);
                throw;
            }
        }

        #endregion

        #region CRUD Operations

        public async Task<List<ACCOUNTING_ALLOCATION>> GetAccountingAllocationsAsync(string? fieldId = null, string? poolId = null, string? wellId = null, DateTime? startDate = null, DateTime? endDate = null, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var repository = await GetAccountingAllocationRepositoryAsync();
                var filters = new List<AppFilter>();

                if (!string.IsNullOrEmpty(fieldId))
                    filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
                if (!string.IsNullOrEmpty(poolId))
                    filters.Add(new AppFilter { FieldName = "POOL_ID", Operator = "=", FilterValue = poolId });
                if (!string.IsNullOrEmpty(wellId))
                    filters.Add(new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId });
                if (startDate.HasValue)
                    filters.Add(new AppFilter { FieldName = "ALLOCATION_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
                if (endDate.HasValue)
                    filters.Add(new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });
                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repository.GetAsync(filters);
                return results.Cast<ACCOUNTING_ALLOCATION>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting accounting allocations");
                throw;
            }
        }

        public async Task<List<ROYALTY_CALCULATION>> GetRoyaltyCalculationsAsync(string? fieldId = null, string? poolId = null, DateTime? startDate = null, DateTime? endDate = null, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var repository = await GetRoyaltyCalculationRepositoryAsync();
                var filters = new List<AppFilter>();

                if (!string.IsNullOrEmpty(fieldId))
                    filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
                if (!string.IsNullOrEmpty(poolId))
                    filters.Add(new AppFilter { FieldName = "POOL_ID", Operator = "=", FilterValue = poolId });
                if (startDate.HasValue)
                    filters.Add(new AppFilter { FieldName = "CALCULATION_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
                if (endDate.HasValue)
                    filters.Add(new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });
                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repository.GetAsync(filters);
                return results.Cast<Beep.OilandGas.PPDM39.Core.DTOs.ROYALTY_CALCULATION>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting royalty calculations");
                throw;
            }
        }

        public async Task<List<Beep.OilandGas.PPDM39.Core.DTOs.COST_ALLOCATION>> GetCostAllocationsAsync(string? fieldId = null, DateTime? startDate = null, DateTime? endDate = null, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var repository = await GetCostAllocationRepositoryAsync();
                var filters = new List<AppFilter>();

                if (!string.IsNullOrEmpty(fieldId))
                    filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
                if (startDate.HasValue)
                    filters.Add(new AppFilter { FieldName = "ALLOCATION_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
                if (endDate.HasValue)
                    filters.Add(new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });
                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repository.GetAsync(filters);
                return results.Cast<COST_ALLOCATION>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting cost allocations");
                throw;
            }
        }

        public async Task<object> SaveAccountingAllocationAsync(object allocationData, string userId)
        {
            try
            {
                var repository = await GetAccountingAllocationRepositoryAsync();
                var result = await repository.InsertAsync(allocationData, userId);
                _logger?.LogInformation("Saved accounting allocation record");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving accounting allocation");
                throw;
            }
        }

        public async Task<object> SaveRoyaltyCalculationAsync(object royaltyData, string userId)
        {
            try
            {
                var repository = await GetRoyaltyCalculationRepositoryAsync();
                var result = await repository.InsertAsync(royaltyData, userId);
                _logger?.LogInformation("Saved royalty calculation record");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving royalty calculation");
                throw;
            }
        }

        public async Task<object> SaveCostAllocationAsync(object costAllocationData, string userId)
        {
            try
            {
                var repository = await GetCostAllocationRepositoryAsync();
                var result = await repository.InsertAsync(costAllocationData, userId);
                _logger?.LogInformation("Saved cost allocation record");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving cost allocation");
                throw;
            }
        }

        #endregion
    }
}
