using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Models.Data;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using ROYALTY_CALCULATION = Beep.OilandGas.Models.DTOs.ROYALTY_CALCULATION;


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
        private PPDMGenericRepository? _accountingCostRepository;
        private PPDMGenericRepository? _accountingAmortizationRepository;
        private PPDMGenericRepository? _accountingMethodRepository;
        private PPDMGenericRepository? _accountingImpairmentRepository;

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
                // Use PPDM class directly
                var entityType = typeof(Beep.OilandGas.PPDM39.Models.PDEN_VOL_SUMMARY);
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
                var dtoType = typeof(Beep.OilandGas.Models.DTOs.ACCOUNTING_ALLOCATION);
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
                // Use DTO type from DTOs namespace
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

        private async Task<PPDMGenericRepository> GetAccountingCostRepositoryAsync()
        {
            if (_accountingCostRepository == null)
            {
                // Use PPDM class directly
                var entityType = typeof(Beep.OilandGas.PPDM39.Models.ACCOUNTING_COST);
                _accountingCostRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "ACCOUNTING_COST", null);
            }
            return _accountingCostRepository;
        }

        private async Task<PPDMGenericRepository> GetAccountingAmortizationRepositoryAsync()
        {
            if (_accountingAmortizationRepository == null)
            {
                // Use PPDM class directly
                var entityType = typeof(Beep.OilandGas.PPDM39.Models.ACCOUNTING_AMORTIZATION);
                _accountingAmortizationRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "ACCOUNTING_AMORTIZATION", null);
            }
            return _accountingAmortizationRepository;
        }

        private async Task<PPDMGenericRepository> GetAccountingMethodRepositoryAsync()
        {
            if (_accountingMethodRepository == null)
            {
                // Use Data class (moved from PPDM39.Models)
                var entityType = typeof(ACCOUNTING_METHOD);
                _accountingMethodRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "ACCOUNTING_METHOD", null);
            }
            return _accountingMethodRepository;
        }

        private async Task<PPDMGenericRepository> GetAccountingImpairmentRepositoryAsync()
        {
            if (_accountingImpairmentRepository == null)
            {
                // Use DTO type from DTOs namespace since PPDM model doesn't exist
                var dtoType = typeof(Beep.OilandGas.Models.DTOs.ACCOUNTING_IMPAIRMENT);
                _accountingImpairmentRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    dtoType, _connectionName, "ACCOUNTING_IMPAIRMENT", null);
            }
            return _accountingImpairmentRepository;
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

                // Extract volumes from production data (PDEN_VOL_SUMMARY)
                decimal fieldOilVolume = 0;
                decimal fieldGasVolume = 0;
                decimal fieldWaterVolume = 0;
                decimal fieldCondensateVolume = 0;

                foreach (var prodRecord in productionData ?? Enumerable.Empty<object>())
                {
                    if (prodRecord is Beep.OilandGas.PPDM39.Models.PDEN_VOL_SUMMARY pden)
                    {
                        fieldOilVolume += pden.OIL_VOLUME;
                        fieldGasVolume += pden.GAS_VOLUME;
                        fieldWaterVolume += pden.WATER_VOLUME;
                        // CONDENSATE_VOLUME may not exist, check if property exists
                        var condensateProp = prodRecord.GetType().GetProperty("CONDENSATE_VOLUME");
                        if (condensateProp != null && condensateProp.GetValue(prodRecord) is decimal condensate)
                        {
                            fieldCondensateVolume += condensate;
                        }
                    }
                    else
                    {
                        // Use Entity properties via reflection (works with any Entity type)
                        fieldOilVolume += GetDecimalValue(prodRecord, "OIL_VOLUME");
                        fieldGasVolume += GetDecimalValue(prodRecord, "GAS_VOLUME");
                        fieldWaterVolume += GetDecimalValue(prodRecord, "WATER_VOLUME");
                        fieldCondensateVolume += GetDecimalValue(prodRecord, "CONDENSATE_VOLUME");
                    }
                }

                // Extract allocated volumes from allocation data (ACCOUNTING_ALLOCATION)
                decimal allocatedOilVolume = 0;
                decimal allocatedGasVolume = 0;
                decimal allocatedWaterVolume = 0;
                decimal allocatedCondensateVolume = 0;
                var allocationDetails = new List<AllocationDetail>();

                foreach (var allocRecord in allocationData ?? Enumerable.Empty<object>())
                {
                    string? productType = null;
                    decimal? allocatedVolume = null;
                    string? entityId = null;
                    string? entityType = null;
                    string? entityName = null;
                    decimal? allocationPercentage = null;

                    if (allocRecord is ACCOUNTING_ALLOCATION alloc)
                    {
                        productType = alloc.PRODUCT_TYPE;
                        allocatedVolume = alloc.ALLOCATED_VOLUME ?? 0;
                        allocationPercentage = alloc.ALLOCATION_PERCENTAGE;
                        entityId = alloc.WELL_ID ?? alloc.POOL_ID ?? alloc.FACILITY_ID;
                        entityType = alloc.WELL_ID != null ? "WELL" : (alloc.POOL_ID != null ? "POOL" : "FACILITY");
                    }
                    else
                    {
                        // Use Entity properties via reflection (works with any Entity type)
                        productType = GetStringValue(allocRecord, "PRODUCT_TYPE");
                        allocatedVolume = GetDecimalValue(allocRecord, "ALLOCATED_VOLUME");
                        allocationPercentage = GetDecimalValue(allocRecord, "ALLOCATION_PERCENTAGE");
                        entityId = GetStringValue(allocRecord, "WELL_ID") ?? GetStringValue(allocRecord, "POOL_ID") ?? GetStringValue(allocRecord, "FACILITY_ID");
                        entityType = GetStringValue(allocRecord, "WELL_ID") != null ? "WELL" : (GetStringValue(allocRecord, "POOL_ID") != null ? "POOL" : "FACILITY");
                    }

                    if (allocatedVolume.HasValue && !string.IsNullOrEmpty(productType))
                    {
                        switch (productType?.ToUpper())
                        {
                            case "OIL":
                                allocatedOilVolume += allocatedVolume.Value;
                                break;
                            case "GAS":
                                allocatedGasVolume += allocatedVolume.Value;
                                break;
                            case "WATER":
                                allocatedWaterVolume += allocatedVolume.Value;
                                break;
                            case "CONDENSATE":
                                allocatedCondensateVolume += allocatedVolume.Value;
                                break;
                        }

                        if (!string.IsNullOrEmpty(entityId))
                        {
                            allocationDetails.Add(new AllocationDetail
                            {
                                EntityId = entityId,
                                EntityType = entityType,
                                AllocatedVolume = allocatedVolume,
                                AllocationPercentage = allocationPercentage
                            });
                        }
                    }
                }

                // Calculate discrepancies and percentages
                var oilDiscrepancy = fieldOilVolume - allocatedOilVolume;
                var oilDiscrepancyPercentage = fieldOilVolume > 0 ? (oilDiscrepancy / fieldOilVolume) * 100 : 0;

                var gasDiscrepancy = fieldGasVolume - allocatedGasVolume;
                var gasDiscrepancyPercentage = fieldGasVolume > 0 ? (gasDiscrepancy / fieldGasVolume) * 100 : 0;

                var waterDiscrepancy = fieldWaterVolume - allocatedWaterVolume;
                var waterDiscrepancyPercentage = fieldWaterVolume > 0 ? (waterDiscrepancy / fieldWaterVolume) * 100 : 0;

                var condensateDiscrepancy = fieldCondensateVolume - allocatedCondensateVolume;
                var condensateDiscrepancyPercentage = fieldCondensateVolume > 0 ? (condensateDiscrepancy / fieldCondensateVolume) * 100 : 0;

                var totalFieldVolume = fieldOilVolume + fieldGasVolume + fieldWaterVolume + fieldCondensateVolume;
                var totalAllocatedVolume = allocatedOilVolume + allocatedGasVolume + allocatedWaterVolume + allocatedCondensateVolume;
                var totalDiscrepancy = totalFieldVolume - totalAllocatedVolume;
                var totalDiscrepancyPercentage = totalFieldVolume > 0 ? (totalDiscrepancy / totalFieldVolume) * 100 : 0;

                // Determine overall status
                var hasDiscrepancies = Math.Abs(oilDiscrepancyPercentage) >= 0.1m || 
                                      Math.Abs(gasDiscrepancyPercentage) >= 0.1m ||
                                      Math.Abs(waterDiscrepancyPercentage) >= 0.1m ||
                                      Math.Abs(condensateDiscrepancyPercentage) >= 0.1m;

                var result = new VolumeReconciliationResult
                {
                    FieldId = fieldId,
                    StartDate = startDate,
                    EndDate = endDate,
                    FieldProductionVolume = totalFieldVolume,
                    AllocatedVolume = totalAllocatedVolume,
                    Discrepancy = totalDiscrepancy,
                    DiscrepancyPercentage = totalDiscrepancyPercentage,
                    Status = hasDiscrepancies ? ReconciliationStatus.Discrepancies : ReconciliationStatus.Reconciled,
                    OilVolume = new VolumeBreakdown
                    {
                        FieldVolume = fieldOilVolume,
                        AllocatedVolume = allocatedOilVolume,
                        Discrepancy = oilDiscrepancy,
                        DiscrepancyPercentage = oilDiscrepancyPercentage,
                        AllocationDetails = allocationDetails.Where(d => d.EntityType == "WELL" || d.EntityType == "POOL" || d.EntityType == "FACILITY").ToList()
                    },
                    GasVolume = new VolumeBreakdown
                    {
                        FieldVolume = fieldGasVolume,
                        AllocatedVolume = allocatedGasVolume,
                        Discrepancy = gasDiscrepancy,
                        DiscrepancyPercentage = gasDiscrepancyPercentage
                    },
                    WaterVolume = new VolumeBreakdown
                    {
                        FieldVolume = fieldWaterVolume,
                        AllocatedVolume = allocatedWaterVolume,
                        Discrepancy = waterDiscrepancy,
                        DiscrepancyPercentage = waterDiscrepancyPercentage
                    },
                    CondensateVolume = new VolumeBreakdown
                    {
                        FieldVolume = fieldCondensateVolume,
                        AllocatedVolume = allocatedCondensateVolume,
                        Discrepancy = condensateDiscrepancy,
                        DiscrepancyPercentage = condensateDiscrepancyPercentage
                    },
                    Issues = new List<ReconciliationIssue>()
                };

                // Add discrepancy issues
                if (Math.Abs(oilDiscrepancyPercentage) >= 0.1m)
                {
                    result.Issues.Add(new ReconciliationIssue
                    {
                        IssueType = "DISCREPANCY",
                        Severity = Math.Abs(oilDiscrepancyPercentage) >= 5m ? "ERROR" : "WARNING",
                        Description = $"Oil volume discrepancy of {oilDiscrepancyPercentage:F2}% detected ({oilDiscrepancy:F2} units)"
                    });
                }

                if (Math.Abs(gasDiscrepancyPercentage) >= 0.1m)
                {
                    result.Issues.Add(new ReconciliationIssue
                    {
                        IssueType = "DISCREPANCY",
                        Severity = Math.Abs(gasDiscrepancyPercentage) >= 5m ? "ERROR" : "WARNING",
                        Description = $"Gas volume discrepancy of {gasDiscrepancyPercentage:F2}% detected ({gasDiscrepancy:F2} units)"
                    });
                }

                // Check for missing data
                if (productionData == null || !productionData.Any())
                {
                    result.Issues.Add(new ReconciliationIssue
                    {
                        IssueType = "MISSING_DATA",
                        Severity = "ERROR",
                        Description = "No production data found for the specified date range"
                    });
                }

                if (allocationData == null || !allocationData.Any())
                {
                    result.Issues.Add(new ReconciliationIssue
                    {
                        IssueType = "MISSING_DATA",
                        Severity = "WARNING",
                        Description = "No allocation data found for the specified date range"
                    });
                }

                // Check for invalid allocations (percentages don't sum to 100%)
                var totalAllocationPercentage = allocationDetails.Sum(d => d.AllocationPercentage ?? 0);
                if (allocationDetails.Any() && Math.Abs(totalAllocationPercentage - 100m) > 1m)
                {
                    result.Issues.Add(new ReconciliationIssue
                    {
                        IssueType = "INVALID_ALLOCATION",
                        Severity = "WARNING",
                        Description = $"Allocation percentages sum to {totalAllocationPercentage:F2}% (expected 100%)"
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

                // Extract production volumes from PDEN_VOL_SUMMARY
                decimal grossOilVolume = 0;
                decimal grossGasVolume = 0;
                decimal grossWaterVolume = 0;

                foreach (var prodRecord in productionData ?? Enumerable.Empty<object>())
                {
                    if (prodRecord is Beep.OilandGas.PPDM39.Models.PDEN_VOL_SUMMARY pden)
                    {
                        grossOilVolume += pden.OIL_VOLUME;
                        grossGasVolume += pden.GAS_VOLUME;
                        grossWaterVolume += pden.WATER_VOLUME;
                    }
                    else
                    {
                        // Use Entity properties via reflection (works with any Entity type)
                        grossOilVolume += GetDecimalValue(prodRecord, "OIL_VOLUME");
                        grossGasVolume += GetDecimalValue(prodRecord, "GAS_VOLUME");
                        grossWaterVolume += GetDecimalValue(prodRecord, "WATER_VOLUME");
                    }
                }

                // Query for royalty rates from ROYALTY_CALCULATION table or use defaults
                var royaltyRepo = await GetRoyaltyCalculationRepositoryAsync();
                decimal oilRoyaltyRate = 12.5m; // Default 12.5%
                decimal gasRoyaltyRate = 12.5m;
                decimal? oilPrice = null;
                decimal? gasPrice = null;
                string? royaltyOwnerId = null;
                string? royaltyType = null;

                var royaltyFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId }
                };
                if (!string.IsNullOrEmpty(poolId))
                {
                    royaltyFilters.Add(new AppFilter { FieldName = "POOL_ID", Operator = "=", FilterValue = poolId });
                }

                var existingRoyaltyData = await royaltyRepo.GetAsync(royaltyFilters);
                if (existingRoyaltyData != null && existingRoyaltyData.Any())
                {
                    var latestRoyalty = existingRoyaltyData.OrderByDescending(r =>
                    {
                        if (r is ROYALTY_CALCULATION rc && rc.CALCULATION_DATE.HasValue)
                            return rc.CALCULATION_DATE.Value;
                        // Use reflection to get CALCULATION_DATE property
                        var dateProp = r.GetType().GetProperty("CALCULATION_DATE");
                        if (dateProp?.GetValue(r) is DateTime dt)
                            return dt;
                        return DateTime.MinValue;
                    }).First();

                    if (latestRoyalty is ROYALTY_CALCULATION rc)
                    {
                        oilRoyaltyRate = rc.OIL_ROYALTY_RATE ?? 12.5m;
                        gasRoyaltyRate = rc.GAS_ROYALTY_RATE ?? 12.5m;
                        oilPrice = rc.OIL_PRICE;
                        gasPrice = rc.GAS_PRICE;
                        royaltyOwnerId = rc.ROYALTY_OWNER_ID;
                        royaltyType = rc.ROYALTY_TYPE;
                    }
                    else
                    {
                        // Use Entity properties via reflection (works with any Entity type)
                        oilRoyaltyRate = GetDecimalValue(latestRoyalty, "OIL_ROYALTY_RATE");
                        if (oilRoyaltyRate == 0) oilRoyaltyRate = 12.5m;
                        gasRoyaltyRate = GetDecimalValue(latestRoyalty, "GAS_ROYALTY_RATE");
                        if (gasRoyaltyRate == 0) gasRoyaltyRate = 12.5m;
                        var oilPriceVal = GetDecimalValue(latestRoyalty, "OIL_PRICE");
                        if (oilPriceVal > 0) oilPrice = oilPriceVal;
                        var gasPriceVal = GetDecimalValue(latestRoyalty, "GAS_PRICE");
                        if (gasPriceVal > 0) gasPrice = gasPriceVal;
                        royaltyOwnerId = GetStringValue(latestRoyalty, "ROYALTY_OWNER_ID");
                        royaltyType = GetStringValue(latestRoyalty, "ROYALTY_TYPE");
                    }
                }

                // Calculate royalty volumes
                var royaltyOilVolume = grossOilVolume * (oilRoyaltyRate / 100);
                var royaltyGasVolume = grossGasVolume * (gasRoyaltyRate / 100);

                // Calculate net volumes
                var netOilVolume = grossOilVolume - royaltyOilVolume;
                var netGasVolume = grossGasVolume - royaltyGasVolume;

                // Calculate royalty values if prices are available
                decimal? royaltyOilValue = null;
                decimal? royaltyGasValue = null;
                decimal? totalRoyaltyValue = null;

                if (oilPrice.HasValue && oilPrice.Value > 0)
                {
                    royaltyOilValue = royaltyOilVolume * oilPrice.Value;
                }

                if (gasPrice.HasValue && gasPrice.Value > 0)
                {
                    royaltyGasValue = royaltyGasVolume * gasPrice.Value;
                }

                if (royaltyOilValue.HasValue || royaltyGasValue.HasValue)
                {
                    totalRoyaltyValue = (royaltyOilValue ?? 0) + (royaltyGasValue ?? 0);
                }

                var result = new RoyaltyCalculationResult
                {
                    FieldId = fieldId,
                    PoolId = poolId,
                    StartDate = startDate,
                    EndDate = endDate,
                    GrossOilVolume = grossOilVolume,
                    GrossGasVolume = grossGasVolume,
                    GrossWaterVolume = grossWaterVolume,
                    OilRoyaltyRate = oilRoyaltyRate,
                    GasRoyaltyRate = gasRoyaltyRate,
                    RoyaltyOilVolume = royaltyOilVolume,
                    RoyaltyGasVolume = royaltyGasVolume,
                    NetOilVolume = netOilVolume,
                    NetGasVolume = netGasVolume,
                    OilPrice = oilPrice,
                    GasPrice = gasPrice,
                    RoyaltyOilValue = royaltyOilValue,
                    RoyaltyGasValue = royaltyGasValue,
                    TotalRoyaltyValue = totalRoyaltyValue,
                    RoyaltyOwnerId = royaltyOwnerId,
                    RoyaltyType = royaltyType
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

                // Get costs from ACCOUNTING_COST table
                var costRepo = await GetAccountingCostRepositoryAsync();
                var costFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "COST_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "COST_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") }
                };
                var costData = await costRepo.GetAsync(costFilters);

                // Calculate total costs
                decimal totalOperatingCosts = 0;
                decimal totalCapitalCosts = 0;
                decimal totalCosts = 0;

                foreach (var costRecord in costData ?? Enumerable.Empty<object>())
                {
                    string? costType = null;
                    decimal? amount = null;

                    if (costRecord is Beep.OilandGas.PPDM39.Models.ACCOUNTING_COST cost)
                    {
                        costType = cost.COST_TYPE;
                        amount = cost.AMOUNT;
                    }
                    else
                    {
                        // Use Entity properties via reflection (works with any Entity type)
                        costType = GetStringValue(costRecord, "COST_TYPE");
                        amount = GetDecimalValue(costRecord, "AMOUNT");
                    }

                    if (amount.HasValue)
                    {
                        totalCosts += amount.Value;
                        if (costType?.ToUpper() == "OPERATING" || costType?.ToUpper().Contains("OPERATING") == true)
                        {
                            totalOperatingCosts += amount.Value;
                        }
                        else
                        {
                            totalCapitalCosts += amount.Value;
                        }
                    }
                }

                // Get allocation details based on method
                var allocationDetails = new List<CostAllocationDetail>();

                if (method == CostAllocationMethod.Manual)
                {
                    // Use existing COST_ALLOCATION records
                    var costAllocRepo = await GetCostAllocationRepositoryAsync();
                    var allocFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                        new AppFilter { FieldName = "ALLOCATION_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                        new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") }
                    };
                    var existingAllocations = await costAllocRepo.GetAsync(allocFilters);

                    foreach (var allocRecord in existingAllocations ?? Enumerable.Empty<object>())
                    {
                        if (allocRecord is COST_ALLOCATION ca)
                        {
                            allocationDetails.Add(new CostAllocationDetail
                            {
                                EntityId = fieldId,
                                EntityType = "FIELD",
                                AllocatedOperatingCost = ca.TOTAL_OPERATING_COSTS,
                                AllocatedCapitalCost = ca.TOTAL_CAPITAL_COSTS,
                                TotalAllocatedCost = ca.TOTAL_COSTS,
                                AllocationBasisType = ca.ALLOCATION_METHOD
                            });
                        }
                    }
                }
                else
                {
                    // Get entities (wells, pools, facilities) for allocation
                    var entities = new List<(string EntityId, string EntityType, string? EntityName, decimal BasisValue)>();

                    if (method == CostAllocationMethod.VolumeBased || method == CostAllocationMethod.RevenueBased)
                    {
                        // Group production data by entity (WELL, POOL, or FACILITY)
                        var entityVolumes = new Dictionary<string, (string Type, string? Name, decimal Volume, decimal? Revenue)>();

                        foreach (var prodRecord in productionData ?? Enumerable.Empty<object>())
                        {
                            string? wellId = null;
                            string? poolId = null;
                            string? facilityId = null;
                            decimal volume = 0;
                            decimal? revenue = null;

                            if (prodRecord is Beep.OilandGas.PPDM39.Models.PDEN_VOL_SUMMARY pden)
                            {
                                wellId = pden.WELL_ID;
                                poolId = pden.POOL_ID;
                                facilityId = pden.FACILITY_ID;
                                volume = pden.OIL_VOLUME + pden.GAS_VOLUME + pden.WATER_VOLUME;
                            }
                            else
                            {
                                // Use Entity properties via reflection (works with any Entity type)
                                wellId = GetStringValue(prodRecord, "WELL_ID");
                                poolId = GetStringValue(prodRecord, "POOL_ID");
                                facilityId = GetStringValue(prodRecord, "FACILITY_ID");
                                volume = GetDecimalValue(prodRecord, "OIL_VOLUME") + 
                                        GetDecimalValue(prodRecord, "GAS_VOLUME") + 
                                        GetDecimalValue(prodRecord, "WATER_VOLUME");
                                revenue = GetDecimalValue(prodRecord, "REVENUE");
                            }

                            string? entityId = wellId ?? poolId ?? facilityId;
                            string entityType = wellId != null ? "WELL" : (poolId != null ? "POOL" : "FACILITY");

                            if (!string.IsNullOrEmpty(entityId))
                            {
                                var key = $"{entityType}:{entityId}";
                                if (entityVolumes.ContainsKey(key))
                                {
                                    var existing = entityVolumes[key];
                                    entityVolumes[key] = (existing.Type, existing.Name, existing.Volume + volume, revenue ?? existing.Revenue);
                                }
                                else
                                {
                                    entityVolumes[key] = (entityType, null, volume, revenue);
                                }
                            }
                        }

                        foreach (var kvp in entityVolumes)
                        {
                            var parts = kvp.Key.Split(':');
                            entities.Add((parts[1], kvp.Value.Type, kvp.Value.Name, 
                                method == CostAllocationMethod.VolumeBased ? kvp.Value.Volume : (kvp.Value.Revenue ?? 0)));
                        }
                    }
                    else if (method == CostAllocationMethod.EqualShare)
                    {
                        // Get all wells, pools, and facilities for the field
                        var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                            typeof(Beep.OilandGas.PPDM39.Models.WELL), _connectionName, "WELL");
                        var wells = await wellRepo.GetAsync(new List<AppFilter> { new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId } });
                        foreach (var well in wells ?? Enumerable.Empty<object>())
                        {
                            string? wellId = null;
                            if (well is Beep.OilandGas.PPDM39.Models.WELL w) wellId = w.WELL_ID;
                            else wellId = GetStringValue(well, "WELL_ID");
                            if (!string.IsNullOrEmpty(wellId)) entities.Add((wellId, "WELL", null, 1));
                        }

                        var poolRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                            typeof(Beep.OilandGas.PPDM39.Models.POOL), _connectionName, "POOL");
                        var pools = await poolRepo.GetAsync(new List<AppFilter> { new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId } });
                        foreach (var pool in pools ?? Enumerable.Empty<object>())
                        {
                            string? poolId = null;
                            if (pool is Beep.OilandGas.PPDM39.Models.POOL p) poolId = p.POOL_ID;
                            else poolId = GetStringValue(pool, "POOL_ID");
                            if (!string.IsNullOrEmpty(poolId)) entities.Add((poolId, "POOL", null, 1));
                        }
                    }
                    else if (method == CostAllocationMethod.FixedPercentage)
                    {
                        // Get fixed percentages from COST_ALLOCATION records
                        var costAllocRepo = await GetCostAllocationRepositoryAsync();
                        var allocFilters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId }
                        };
                        var fixedAllocations = await costAllocRepo.GetAsync(allocFilters);
                        
                        // Extract fixed percentages from allocation records
                        // Note: This assumes COST_ALLOCATION has entity-level percentage data
                        // If not available, fall back to equal share
                        if (fixedAllocations != null && fixedAllocations.Any())
                        {
                            foreach (var allocRecord in fixedAllocations)
                            {
                                if (allocRecord is COST_ALLOCATION ca && ca.TOTAL_COSTS.HasValue)
                                {
                                    // For fixed percentage, we need entity-level data
                                    // This is a simplified implementation
                                    entities.Add((fieldId, "FIELD", null, ca.TOTAL_COSTS.Value));
                                }
                            }
                        }
                        else
                        {
                            // Fallback to equal share if no fixed percentages found
                            // Get all wells, pools, and facilities for the field
                            var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                                typeof(Beep.OilandGas.PPDM39.Models.WELL), _connectionName, "WELL");
                            var wells = await wellRepo.GetAsync(new List<AppFilter> { new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId } });
                            foreach (var well in wells ?? Enumerable.Empty<object>())
                            {
                                string? wellId = null;
                                if (well is Beep.OilandGas.PPDM39.Models.WELL w) wellId = w.WELL_ID;
                                else wellId = GetStringValue(well, "WELL_ID");
                                if (!string.IsNullOrEmpty(wellId)) entities.Add((wellId, "WELL", null, 1));
                            }

                            var poolRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                                typeof(Beep.OilandGas.PPDM39.Models.POOL), _connectionName, "POOL");
                            var pools = await poolRepo.GetAsync(new List<AppFilter> { new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId } });
                            foreach (var pool in pools ?? Enumerable.Empty<object>())
                            {
                                string? poolId = null;
                                if (pool is Beep.OilandGas.PPDM39.Models.POOL p) poolId = p.POOL_ID;
                                else poolId = GetStringValue(pool, "POOL_ID");
                                if (!string.IsNullOrEmpty(poolId)) entities.Add((poolId, "POOL", null, 1));
                            }
                        }
                    }

                    // Calculate allocation percentages and amounts
                    var totalBasis = entities.Sum(e => e.BasisValue);
                    if (totalBasis > 0)
                    {
                        foreach (var entity in entities)
                        {
                            var allocationPercentage = (entity.BasisValue / totalBasis) * 100;
                            var allocatedOperatingCost = totalOperatingCosts * (allocationPercentage / 100);
                            var allocatedCapitalCost = totalCapitalCosts * (allocationPercentage / 100);
                            var totalAllocatedCost = allocatedOperatingCost + allocatedCapitalCost;

                            allocationDetails.Add(new CostAllocationDetail
                            {
                                EntityId = entity.EntityId,
                                EntityType = entity.EntityType,
                                EntityName = entity.EntityName,
                                AllocatedOperatingCost = allocatedOperatingCost,
                                AllocatedCapitalCost = allocatedCapitalCost,
                                TotalAllocatedCost = totalAllocatedCost,
                                AllocationBasisValue = entity.BasisValue,
                                AllocationBasisType = method == CostAllocationMethod.VolumeBased ? "VOLUME" : 
                                                     (method == CostAllocationMethod.RevenueBased ? "REVENUE" : "EQUAL"),
                                AllocationPercentage = allocationPercentage
                            });
                        }
                    }
                }

                var result = new CostAllocationResult
                {
                    FieldId = fieldId,
                    StartDate = startDate,
                    EndDate = endDate,
                    AllocationMethod = method,
                    TotalOperatingCosts = totalOperatingCosts,
                    TotalCapitalCosts = totalCapitalCosts,
                    TotalCosts = totalCosts,
                    AllocationDetails = allocationDetails
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
                return results.Cast<Beep.OilandGas.Models.DTOs.ROYALTY_CALCULATION>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting royalty calculations");
                throw;
            }
        }

        public async Task<List<Beep.OilandGas.Models.DTOs.COST_ALLOCATION>> GetCostAllocationsAsync(string? fieldId = null, DateTime? startDate = null, DateTime? endDate = null, List<AppFilter>? additionalFilters = null)
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

        #region Accounting Library Integration

        /// <summary>
        /// Calculates amortization using units-of-production method
        /// </summary>
        public async Task<decimal> CalculateAmortizationAsync(
            string propertyId,
            decimal netCapitalizedCosts,
            decimal totalProvedReservesBOE,
            decimal productionBOE,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Calculating amortization for property {PropertyId}", propertyId);

                // Use AccountingManager to calculate amortization
                var amortization = AccountingManager.CalculateAmortization(
                    netCapitalizedCosts,
                    totalProvedReservesBOE,
                    productionBOE);

                // Store amortization record
                var repository = await GetAccountingAmortizationRepositoryAsync();
                var amortizationRecord = new Beep.OilandGas.PPDM39.Models.ACCOUNTING_AMORTIZATION
                {
                    ACCOUNTING_AMORTIZATION_ID = Guid.NewGuid().ToString(),
                    PROPERTY_ID = propertyId,
                    PERIOD_START_DATE = DateTime.UtcNow.Date,
                    PERIOD_END_DATE = DateTime.UtcNow.Date,
                    CAPITALIZED_COST = netCapitalizedCosts,
                    PRODUCTION_BOE = productionBOE,
                    TOTAL_RESERVES_BOE = totalProvedReservesBOE,
                    AMORTIZATION_AMOUNT = amortization
                };

                await repository.InsertAsync(amortizationRecord, userId);
                _logger?.LogInformation("Calculated and stored amortization: {Amortization} for property {PropertyId}", 
                    amortization, propertyId);

                return amortization;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating amortization for property {PropertyId}", propertyId);
                throw;
            }
        }

        /// <summary>
        /// Converts production data to BOE (Barrels of Oil Equivalent)
        /// </summary>
        public decimal ConvertProductionToBOE(decimal oilProduction, decimal gasProduction)
        {
            try
            {
                var productionData = new ProductionData
                {
                    OilProduction = oilProduction,
                    GasProduction = gasProduction
                };

                return AccountingManager.ConvertProductionToBOE(productionData);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error converting production to BOE");
                throw;
            }
        }

        /// <summary>
        /// Converts reserves data to BOE (Barrels of Oil Equivalent)
        /// </summary>
        public decimal ConvertReservesToBOE(decimal oilReserves, decimal gasReserves)
        {
            try
            {
                var reserves = new ProvedReserves
                {
                    ProvedDevelopedOilReserves = oilReserves,
                    ProvedDevelopedGasReserves = gasReserves
                };

                return AccountingManager.ConvertReservesToBOE(reserves);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error converting reserves to BOE");
                throw;
            }
        }

        /// <summary>
        /// Records exploration costs using Successful Efforts accounting
        /// </summary>
        public async Task RecordExplorationCostsAsync(
            string propertyId,
            decimal geologicalGeophysicalCosts,
            decimal exploratoryDrillingCosts,
            bool isDryHole,
            bool foundProvedReserves,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Recording exploration costs for property {PropertyId}", propertyId);

                // Get accounting method for property/field
                var accountingMethod = await GetAccountingMethodForPropertyAsync(propertyId);
                var isSuccessfulEfforts = accountingMethod == "SUCCESSFUL_EFFORTS";

                if (isSuccessfulEfforts)
                {
                    var accounting = AccountingManager.CreateSuccessfulEffortsAccounting();
                    var explorationCosts = new ExplorationCosts
                    {
                        PropertyId = propertyId,
                        GeologicalGeophysicalCosts = geologicalGeophysicalCosts,
                        ExploratoryDrillingCosts = exploratoryDrillingCosts,
                        IsDryHole = isDryHole,
                        FoundProvedReserves = foundProvedReserves
                    };

                    if (isDryHole)
                    {
                        accounting.RecordDryHole(explorationCosts);
                    }
                    else
                    {
                        accounting.RecordExplorationCosts(explorationCosts);
                    }
                }
                else
                {
                    // Full Cost accounting - all costs capitalized
                    var accounting = AccountingManager.CreateFullCostAccounting();
                    var explorationCosts = new ExplorationCosts
                    {
                        PropertyId = propertyId,
                        GeologicalGeophysicalCosts = geologicalGeophysicalCosts,
                        ExploratoryDrillingCosts = exploratoryDrillingCosts,
                        IsDryHole = isDryHole,
                        FoundProvedReserves = foundProvedReserves
                    };
                    // Use propertyId as costCenterId for Full Cost accounting
                    accounting.RecordExplorationCosts(propertyId, explorationCosts);
                }

                // Store cost record in PPDM
                var repository = await GetAccountingCostRepositoryAsync();
                var costRecord = new Beep.OilandGas.PPDM39.Models.ACCOUNTING_COST
                {
                    ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                    PROPERTY_ID = propertyId,
                    COST_TYPE = "EXPLORATION",
                    COST_CATEGORY = isDryHole ? "DRILLING" : "G&G",
                    AMOUNT = geologicalGeophysicalCosts + exploratoryDrillingCosts,
                    COST_DATE = DateTime.UtcNow,
                    IS_CAPITALIZED = (!isDryHole || !isSuccessfulEfforts) ? "Y" : "N",
                    IS_EXPENSED = (isDryHole && isSuccessfulEfforts) ? "Y" : "N",
                    DRY_HOLE_FLAG = isDryHole ? "Y" : "N"
                };

                await repository.InsertAsync(costRecord, userId);
                _logger?.LogInformation("Recorded exploration costs for property {PropertyId}", propertyId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording exploration costs for property {PropertyId}", propertyId);
                throw;
            }
        }

        /// <summary>
        /// Records development costs
        /// </summary>
        public async Task RecordDevelopmentCostsAsync(
            string propertyId,
            string? wellId,
            string? fieldId,
            decimal developmentCosts,
            string costCategory,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Recording development costs for property {PropertyId}", propertyId);

                // Get accounting method
                var accountingMethod = await GetAccountingMethodForPropertyAsync(propertyId);
                var isSuccessfulEfforts = accountingMethod == "SUCCESSFUL_EFFORTS";

                if (isSuccessfulEfforts)
                {
                    var accounting = AccountingManager.CreateSuccessfulEffortsAccounting();
                    var costs = new DevelopmentCosts
                    {
                        PropertyId = propertyId,
                        DevelopmentWellDrillingCosts = developmentCosts,
                        DevelopmentWellEquipment = 0,
                        SupportEquipmentAndFacilities = 0,
                        ServiceWellCosts = 0,
                        CostDate = DateTime.UtcNow
                    };
                    accounting.RecordDevelopmentCosts(costs);
                }
                else
                {
                    var accounting = AccountingManager.CreateFullCostAccounting();
                    var costs = new DevelopmentCosts
                    {
                        PropertyId = propertyId,
                        DevelopmentWellDrillingCosts = developmentCosts,
                        DevelopmentWellEquipment = 0,
                        SupportEquipmentAndFacilities = 0,
                        ServiceWellCosts = 0,
                        CostDate = DateTime.UtcNow
                    };
                    // Use propertyId as costCenterId for Full Cost accounting
                    accounting.RecordDevelopmentCosts(propertyId, costs);
                }

                // Store cost record
                var repository = await GetAccountingCostRepositoryAsync();
                var costRecord = new Beep.OilandGas.PPDM39.Models.ACCOUNTING_COST
                {
                    ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                    PROPERTY_ID = propertyId,
                    WELL_ID = wellId ?? string.Empty,
                    FIELD_ID = fieldId ?? string.Empty,
                    COST_TYPE = "DEVELOPMENT",
                    COST_CATEGORY = costCategory,
                    AMOUNT = developmentCosts,
                    COST_DATE = DateTime.UtcNow,
                    IS_CAPITALIZED = "Y", // Development costs always capitalized
                    IS_EXPENSED = "N"
                };

                await repository.InsertAsync(costRecord, userId);
                _logger?.LogInformation("Recorded development costs for property {PropertyId}", propertyId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording development costs for property {PropertyId}", propertyId);
                throw;
            }
        }

        /// <summary>
        /// Records property acquisition
        /// </summary>
        public async Task RecordPropertyAcquisitionAsync(
            string propertyId,
            decimal acquisitionCost,
            DateTime acquisitionDate,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Recording property acquisition for property {PropertyId}", propertyId);

                var accounting = AccountingManager.CreateSuccessfulEffortsAccounting();
                var property = new UnprovedProperty
                {
                    PropertyId = propertyId,
                    AcquisitionCost = acquisitionCost,
                    AcquisitionDate = acquisitionDate
                };
                accounting.RecordAcquisition(property);

                // Store cost record
                var repository = await GetAccountingCostRepositoryAsync();
                var costRecord = new Beep.OilandGas.PPDM39.Models.ACCOUNTING_COST
                {
                    ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                    PROPERTY_ID = propertyId,
                    COST_TYPE = "ACQUISITION",
                    COST_CATEGORY = "ACQUISITION",
                    AMOUNT = acquisitionCost,
                    COST_DATE = acquisitionDate,
                    IS_CAPITALIZED = "Y",
                    IS_EXPENSED = "N"
                };

                await repository.InsertAsync(costRecord, userId);
                _logger?.LogInformation("Recorded property acquisition for property {PropertyId}", propertyId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording property acquisition for property {PropertyId}", propertyId);
                throw;
            }
        }

        /// <summary>
        /// Records impairment for an unproved property
        /// </summary>
        public async Task RecordImpairmentAsync(
            string propertyId,
            decimal impairmentAmount,
            string reason,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Recording impairment for property {PropertyId}", propertyId);

                var accounting = AccountingManager.CreateSuccessfulEffortsAccounting();
                accounting.RecordImpairment(propertyId, impairmentAmount);

                // Store impairment record
                var repository = await GetAccountingImpairmentRepositoryAsync();
                var impairmentRecord = new Beep.OilandGas.Models.DTOs.ACCOUNTING_IMPAIRMENT
                {
                    ACCOUNTING_IMPAIRMENT_ID = Guid.NewGuid().ToString(),
                    PROPERTY_ID = propertyId,
                    IMPAIRMENT_DATE = DateTime.UtcNow,
                    IMPAIRMENT_AMOUNT = impairmentAmount,
                    REASON = reason
                };

                await repository.InsertAsync(impairmentRecord, userId);
                _logger?.LogInformation("Recorded impairment for property {PropertyId}", propertyId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording impairment for property {PropertyId}", propertyId);
                throw;
            }
        }

        /// <summary>
        /// Gets or sets the accounting method for a field
        /// </summary>
        public async Task<string> GetAccountingMethodForPropertyAsync(string propertyId)
        {
            try
            {
                // Try to get from ACCOUNTING_METHOD table
                var repository = await GetAccountingMethodRepositoryAsync();
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = propertyId }
                };

                var results = await repository.GetAsync(filters);
                var resultsList = results?.ToList() ?? new List<object>();
                if (resultsList.Count > 0)
                {
                    var method = resultsList[0] as ACCOUNTING_METHOD;
                    if (method != null && !string.IsNullOrEmpty(method.METHOD_TYPE))
                    {
                        return method.METHOD_TYPE;
                    }
                }

                // Default to Successful Efforts
                return "SUCCESSFUL_EFFORTS";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting accounting method for property {PropertyId}", propertyId);
                return "SUCCESSFUL_EFFORTS"; // Default
            }
        }

        /// <summary>
        /// Sets the accounting method for a field
        /// </summary>
        public async Task SetAccountingMethodAsync(
            string fieldId,
            string methodType, // "SUCCESSFUL_EFFORTS" or "FULL_COST"
            DateTime effectiveDate,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Setting accounting method {MethodType} for field {FieldId}", methodType, fieldId);

                if (methodType != "SUCCESSFUL_EFFORTS" && methodType != "FULL_COST")
                {
                    throw new ArgumentException("Method type must be SUCCESSFUL_EFFORTS or FULL_COST", nameof(methodType));
                }

                var repository = await GetAccountingMethodRepositoryAsync();
                var methodRecord = new ACCOUNTING_METHOD
                {
                    ACCOUNTING_METHOD_ID = Guid.NewGuid().ToString(),
                    FIELD_ID = fieldId,
                    METHOD_TYPE = methodType,
                    EFFECTIVE_DATE = effectiveDate
                };

                await repository.InsertAsync(methodRecord, userId);
                _logger?.LogInformation("Set accounting method {MethodType} for field {FieldId}", methodType, fieldId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error setting accounting method for field {FieldId}", fieldId);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets decimal value from Entity object or Dictionary (for backward compatibility)
        /// </summary>
        private decimal GetDecimalValue(object? obj, string propertyName)
        {
            if (obj == null) return 0;
            
            // Try Entity object first (using reflection)
            var prop = obj.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                var value = prop.GetValue(obj);
                if (value is decimal d) return d;
                // handle nullable decimal stored as object
                if (value is null) { }
                else if (value is decimal) return (decimal)value;
                else if (value is double db) return (decimal)db;
                else if (value is int i) return i;
                else if (value is long l) return l;
                else if (decimal.TryParse(value?.ToString(), out var parsed)) return parsed;
            }
            
            // Fallback to Dictionary for backward compatibility (will be removed when all services are updated)
            if (obj is IDictionary<string, object> dict && dict.TryGetValue(propertyName, out var dictValue))
            {
                if (dictValue is decimal) return (decimal)dictValue;
                if (dictValue is double db) return (decimal)db;
                if (dictValue is int i) return i;
                if (dictValue is long l) return l;
                if (decimal.TryParse(dictValue?.ToString(), out var parsed)) return parsed;
            }
            
            return 0;
        }

        /// <summary>
        /// Gets string value from Entity object or Dictionary (for backward compatibility)
        /// </summary>
        private string? GetStringValue(object? obj, string propertyName)
        {
            if (obj == null) return null;
            
            // Try Entity object first (using reflection)
            var prop = obj.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                return prop.GetValue(obj)?.ToString();
            }
            
            // Fallback to Dictionary for backward compatibility (will be removed when all services are updated)
            if (obj is IDictionary<string, object> dict && dict.TryGetValue(propertyName, out var dictValue))
            {
                return dictValue?.ToString();
            }
            
            return null;
        }

        #endregion
    }
}
