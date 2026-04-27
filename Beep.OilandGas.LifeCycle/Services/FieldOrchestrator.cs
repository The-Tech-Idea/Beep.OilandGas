using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.LifeCycle.Services.Development;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.LifeCycle.Services.Decommissioning;
using Beep.OilandGas.LifeCycle.Services.HSE;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.LifeCycle.Services
{
    /// <summary>
    /// Orchestrator service that manages the complete lifecycle of a single active field
    /// Coordinates all phase services (Exploration, Development, Production, Decommissioning)
    /// </summary>
    public class FieldOrchestrator : IFieldOrchestrator
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly string _connectionName;
        private readonly ILogger<FieldOrchestrator>? _logger;
        private readonly IAccessControlService? _accessControlService;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        private string? _currentFieldId;
        private PPDMGenericRepository? _fieldRepository;

        // Phase services (created on demand when field is set; exploration is always DI-scoped)
        private readonly IFieldExplorationService _fieldExplorationService;
        private IFieldDevelopmentService? _developmentService;
        private IFieldProductionService? _productionService;
        private IFieldDecommissioningService? _decommissioningService;
        private IFieldHSEService? _hseService;
        private PPDMProcessService? _processService;

        public string? CurrentFieldId => _currentFieldId;

        public bool IsFieldActive => !string.IsNullOrEmpty(_currentFieldId);

        public FieldOrchestrator(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            IFieldExplorationService fieldExplorationService,
            string connectionName = "PPDM39",
            ILogger<FieldOrchestrator>? logger = null,
            IAccessControlService? accessControlService = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
            _accessControlService = accessControlService;
            _httpContextAccessor = httpContextAccessor;
            _fieldExplorationService = fieldExplorationService ?? throw new ArgumentNullException(nameof(fieldExplorationService));
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name
                ?? _httpContextAccessor?.HttpContext?.User?.FindFirst("sub")?.Value
                ?? _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<bool> SetActiveFieldAsync(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
            {
                _logger?.LogWarning("Attempted to set active field with null or empty fieldId");
                return false;
            }

            try
            {
                // Validate field exists
                var fieldMetadata = await _metadata.GetTableMetadataAsync("FIELD");
                if (fieldMetadata == null)
                {
                    _logger?.LogError("FIELD table metadata not found");
                    return false;
                }

                _fieldRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FIELD), _connectionName, "FIELD");

                var formattedFieldId = _defaults.FormatIdForTable("FIELD", fieldId);
                var field = await _fieldRepository.GetByIdAsync(formattedFieldId);
                
                if (field == null)
                {
                    _logger?.LogWarning($"Field not found: {fieldId}");
                    return false;
                }

                // Check access control if service is available
                var currentUserId = GetCurrentUserId();
                if (_accessControlService != null && !string.IsNullOrEmpty(currentUserId))
                {
                    var accessCheck = await _accessControlService.CheckAssetAccessAsync(currentUserId, formattedFieldId, "FIELD");
                    if (!accessCheck.HasAccess)
                    {
                        _logger?.LogWarning($"User {currentUserId} does not have access to field: {fieldId}");
                        return false;
                    }
                }

                _currentFieldId = formattedFieldId;
                _logger?.LogInformation($"Active field set to: {fieldId}");

                // Phase services (except injected exploration) are created on demand; reset so they bind to the new field
                _developmentService = null;
                _productionService = null;
                _decommissioningService = null;
                _hseService = null;
                _processService = null;

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error setting active field: {fieldId}");
                return false;
            }
        }

        public async Task<object?> GetCurrentFieldAsync()
        {
            if (string.IsNullOrEmpty(_currentFieldId) || _fieldRepository == null)
                return null;

            try
            {
                return await _fieldRepository.GetByIdAsync(_currentFieldId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting current field: {_currentFieldId}");
                return null;
            }
        }

        public async Task<FieldLifecycleSummary> GetFieldLifecycleSummaryAsync()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            var summary = new FieldLifecycleSummary
            {
                FieldId = _currentFieldId
            };

            try
            {
                // Get field information
                var field = await GetCurrentFieldAsync();
                if (field is FIELD fieldEntity)
                {
                    summary.FieldName = fieldEntity.FIELD_NAME ?? string.Empty;
                }

                // Get exploration data
                var explorationSvc = GetExplorationService();
                var prospects = await explorationSvc.GetProspectsForFieldAsync(_currentFieldId);
                summary.ProspectCount = prospects?.Count ?? 0;

                var surveys = await explorationSvc.GetSeismicSurveysForFieldAsync(_currentFieldId);
                summary.SeismicSurveyCount = surveys?.Count ?? 0;

                var exploratoryWells = await explorationSvc.GetExploratoryWellsForFieldAsync(_currentFieldId);
                summary.ExploratoryWellCount = exploratoryWells?.Count ?? 0;

                // Get development data
                var developmentSvc = GetDevelopmentService();
                var pools = await developmentSvc.GetPoolsForFieldAsync(_currentFieldId);
                summary.PoolCount = pools?.Count ?? 0;

                var facilities = await developmentSvc.GetFacilitiesForFieldAsync(_currentFieldId);
                summary.FacilityCount = facilities?.Count ?? 0;

                var pipelines = await developmentSvc.GetPipelinesForFieldAsync(_currentFieldId);
                summary.PipelineCount = pipelines?.Count ?? 0;

                var developmentWells = await developmentSvc.GetDevelopmentWellsForFieldAsync(_currentFieldId);
                summary.DevelopmentWellCount = developmentWells?.Count ?? 0;

                // Count producing wells from all wells already retrieved
                var allSummaryWells = new List<WELL>();
                if (exploratoryWells != null) allSummaryWells.AddRange(exploratoryWells);
                if (developmentWells != null) allSummaryWells.AddRange(developmentWells);
                summary.ProductionWellCount = allSummaryWells.Count(w =>
                {
                    var s = w.CURRENT_STATUS?.ToUpperInvariant();
                    return s == "PRODUCING" || s == "P" || s == "ACTIVE";
                });

                // Get decommissioning data
                var decommissioningSvc = GetDecommissioningService();
                var abandonedWells = await decommissioningSvc.GetAbandonedWellsForFieldAsync(_currentFieldId);
                summary.AbandonedWellCount = abandonedWells?.Count ?? 0;

                var decommissionedFacilities = await decommissioningSvc.GetDecommissionedFacilitiesForFieldAsync(_currentFieldId);
                summary.DecommissionedFacilityCount = decommissionedFacilities?.Count ?? 0;

                return summary;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting field lifecycle summary for field: {_currentFieldId}");
                throw;
            }
        }

        public async Task<List<WELL>> GetFieldWellsAsync()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            var allWells = new List<WELL>();

            try
            {
                // Get exploratory wells
                var explorationSvc = GetExplorationService();
                var exploratoryWells = await explorationSvc.GetExploratoryWellsForFieldAsync(_currentFieldId);
                if (exploratoryWells != null)
                    allWells.AddRange(exploratoryWells.Cast<WELL>());

                // Get development wells
                var developmentSvc = GetDevelopmentService();
                var developmentWells = await developmentSvc.GetDevelopmentWellsForFieldAsync(_currentFieldId);
                if (developmentWells != null)
                    allWells.AddRange(developmentWells.Cast<WELL>());

                // Filter by access control if service is available
                var currentUserId = GetCurrentUserId();
                if (_accessControlService != null && !string.IsNullOrEmpty(currentUserId))
                {
                    var accessibleAssets = await _accessControlService.GetUserAccessibleAssetsAsync(currentUserId, "WELL");
                    var accessibleWellIds = accessibleAssets.Where(a => a.Active).Select(a => a.AssetId).ToHashSet();
                    allWells = allWells.Where(w => 
                    {
                        var wellId = GetWellId(w);
                        return string.IsNullOrEmpty(wellId) || accessibleWellIds.Contains(wellId);
                    }).ToList();
                }

                return allWells;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting field wells for field: {_currentFieldId}");
                throw;
            }
        }

        private static string? GetWellId(WELL well) => well.WELL_ID;

        public async Task<FieldStatistics> GetFieldStatisticsAsync()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            var stats = new FieldStatistics
            {
                FieldId = _currentFieldId,
                StatisticsAsOfDate = DateTime.UtcNow
            };

            try
            {
                // Get field information
                var field = await GetCurrentFieldAsync();
                if (field is FIELD fieldEntity)
                {
                    stats.FieldName = fieldEntity.FIELD_NAME ?? string.Empty;
                }

                // Get all wells
                var wells = await GetFieldWellsAsync();
                stats.TotalWellCount = wells.Count;
                
                // Count wells by status using CURRENT_STATUS property directly
                var activeWells = 0;
                var inactiveWells = 0;
                foreach (var well in wells)
                {
                    var status = well.CURRENT_STATUS?.ToUpperInvariant();
                    if (status == "ACTIVE" || status == "PRODUCING" || status == "P")
                        activeWells++;
                    else
                        inactiveWells++;
                }
                stats.ActiveWellCount = activeWells;
                stats.InactiveWellCount = inactiveWells;

                // Get production statistics from PDEN_VOL_SUMMARY
                var productionRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY");
                
                var productionFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _currentFieldId }
                };
                var productionData = await productionRepo.GetAsync(productionFilters);

                decimal totalOilProduction = 0;
                decimal totalGasProduction = 0;
                decimal totalWaterProduction = 0;
                DateTime? firstProductionDate = null;
                DateTime? lastProductionDate = null;
                int productionDays = 0;
                var productionDates = new HashSet<DateTime>();

                foreach (var prodRecord in productionData ?? Enumerable.Empty<object>())
                {
                    DateTime? prodDate = null;
                    decimal oilVol = 0;
                    decimal gasVol = 0;
                    decimal waterVol = 0;

                    if (prodRecord is not PDEN_VOL_SUMMARY pden)
                        continue;

                    oilVol = pden.OIL_VOLUME;
                    gasVol = pden.GAS_VOLUME;
                    waterVol = pden.WATER_VOLUME;
                    prodDate = pden.EFFECTIVE_DATE ?? pden.VOLUME_DATE;

                    totalOilProduction += oilVol;
                    totalGasProduction += gasVol;
                    totalWaterProduction += waterVol;

                    if (prodDate.HasValue)
                    {
                        productionDates.Add(prodDate.Value.Date);
                        if (!firstProductionDate.HasValue || prodDate.Value < firstProductionDate.Value)
                            firstProductionDate = prodDate.Value;
                        if (!lastProductionDate.HasValue || prodDate.Value > lastProductionDate.Value)
                            lastProductionDate = prodDate.Value;
                    }
                }

                stats.TotalOilProduction = totalOilProduction;
                stats.TotalGasProduction = totalGasProduction;
                stats.TotalWaterProduction = totalWaterProduction;
                stats.FirstProductionDate = firstProductionDate;
                stats.LastProductionDate = lastProductionDate;

                // Calculate average daily production
                if (productionDates.Any() && firstProductionDate.HasValue && lastProductionDate.HasValue)
                {
                    var daysDiff = (lastProductionDate.Value - firstProductionDate.Value).Days + 1;
                    productionDays = daysDiff;
                    var totalVolume = totalOilProduction + totalGasProduction + totalWaterProduction;
                    stats.AverageDailyProduction = daysDiff > 0 ? totalVolume / daysDiff : 0;
                }

                // RESERVE_ENTITY is a grouping row in PPDM; proved/probable/possible volumes
                // are not scalar columns on that entity in our model. Populate reserve stats
                // from a dedicated reserves service when available.

                // Get facility counts
                var facilityRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY), _connectionName, "FACILITY");
                
                var facilityFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _currentFieldId }
                };
                var facilityData = await facilityRepo.GetAsync(facilityFilters);

                var totalFacilities = facilityData?.Count() ?? 0;
                var activeFacilities = 0;

                foreach (var facilityRecord in facilityData ?? Enumerable.Empty<object>())
                {
                    if (facilityRecord is not FACILITY f)
                        continue;
                    if (f.ACTIVE_IND == "Y")
                        activeFacilities++;
                }

                stats.TotalFacilityCount = totalFacilities;
                stats.ActiveFacilityCount = activeFacilities;

                return stats;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting field statistics for field: {_currentFieldId}");
                throw;
            }
        }

        public async Task<FieldTimeline> GetFieldTimelineAsync()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            var timeline = new FieldTimeline
            {
                FieldId = _currentFieldId,
                Events = new List<FieldTimelineEvent>()
            };

            try
            {
                // Get field information
                var field = await GetCurrentFieldAsync();
                if (field is FIELD fieldEntity)
                {
                    timeline.FieldName = fieldEntity.FIELD_NAME ?? string.Empty;
                }

                // Query PROSPECT table for exploration events
                var prospectRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PROSPECT), _connectionName, "PROSPECT");
                var prospectFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _currentFieldId }
                };
                var prospects = await prospectRepo.GetAsync(prospectFilters);

                foreach (var prospectRecord in prospects ?? Enumerable.Empty<object>())
                {
                    if (prospectRecord is not PROSPECT p)
                        continue;

                    var prospectId = p.PROSPECT_ID;
                    var prospectName = p.PROSPECT_NAME;
                    var prospectDate = p.EVALUATION_DATE ?? p.ROW_CREATED_DATE;
                    var discoveryDate = p.DISCOVERY_DATE;

                    if (prospectDate.HasValue)
                    {
                        timeline.Events.Add(new FieldTimelineEvent
                        {
                            EventId = Guid.NewGuid().ToString(),
                            EventType = "Exploration",
                            EventDescription = $"Prospect created: {prospectName ?? prospectId}",
                            EventDate = prospectDate.Value,
                            EntityType = "PROSPECT",
                            EntityId = prospectId,
                            EntityName = prospectName
                        });
                    }

                    if (discoveryDate.HasValue)
                    {
                        timeline.Events.Add(new FieldTimelineEvent
                        {
                            EventId = Guid.NewGuid().ToString(),
                            EventType = "Exploration",
                            EventDescription = $"Discovery: {prospectName ?? prospectId}",
                            EventDate = discoveryDate.Value,
                            EntityType = "PROSPECT",
                            EntityId = prospectId,
                            EntityName = prospectName
                        });
                    }
                }

                // Query WELL table for development and production events
                var wellRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), _connectionName, "WELL");
                var wellFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _currentFieldId }
                };
                var wells = await wellRepo.GetAsync(wellFilters);

                foreach (var wellRecord in wells ?? Enumerable.Empty<object>())
                {
                    if (wellRecord is not WELL w)
                        continue;

                    var wellId = w.WELL_ID;
                    var wellName = w.WELL_NAME;
                    var spudDate = w.SPUD_DATE;
                    var completionDate = w.COMPLETION_DATE;

                    if (spudDate.HasValue)
                    {
                        timeline.Events.Add(new FieldTimelineEvent
                        {
                            EventId = Guid.NewGuid().ToString(),
                            EventType = "Development",
                            EventDescription = $"Well spudded: {wellName ?? wellId}",
                            EventDate = spudDate.Value,
                            EntityType = "WELL",
                            EntityId = wellId,
                            EntityName = wellName
                        });
                    }

                    if (completionDate.HasValue)
                    {
                        timeline.Events.Add(new FieldTimelineEvent
                        {
                            EventId = Guid.NewGuid().ToString(),
                            EventType = "Development",
                            EventDescription = $"Well completed: {wellName ?? wellId}",
                            EventDate = completionDate.Value,
                            EntityType = "WELL",
                            EntityId = wellId,
                            EntityName = wellName
                        });
                    }

                }

                // Query PDEN_VOL_SUMMARY for monthly production events (aggregate by month)
                var productionRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY");
                var productionFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _currentFieldId }
                };
                var productionData = await productionRepo.GetAsync(productionFilters);

                var monthlyProduction = new Dictionary<(int Year, int Month), decimal>();
                foreach (var prodRecord in productionData ?? Enumerable.Empty<object>())
                {
                    if (prodRecord is not PDEN_VOL_SUMMARY pden)
                        continue;

                    var prodDate = pden.EFFECTIVE_DATE ?? pden.VOLUME_DATE;
                    var volume = pden.OIL_VOLUME + pden.GAS_VOLUME;

                    if (prodDate.HasValue && volume > 0)
                    {
                        var key = (prodDate.Value.Year, prodDate.Value.Month);
                        if (monthlyProduction.ContainsKey(key))
                            monthlyProduction[key] += volume;
                        else
                            monthlyProduction[key] = volume;
                    }
                }

                // Create monthly production events
                foreach (var kvp in monthlyProduction.OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month))
                {
                    var eventDate = new DateTime(kvp.Key.Year, kvp.Key.Month, 1);
                    timeline.Events.Add(new FieldTimelineEvent
                    {
                        EventId = Guid.NewGuid().ToString(),
                        EventType = "Production",
                        EventDescription = $"Monthly production: {kvp.Value:F2} BOE",
                        EventDate = eventDate,
                        EntityType = "PRODUCTION",
                        AdditionalData = new Dictionary<string, object> { { "Volume", kvp.Value } }
                    });
                }

                // Query WELL_ABANDONMENT or WELL_STATUS for decommissioning events
                var wellAbandonmentRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_ABANDONMENT), _connectionName, "WELL_ABANDONMENT");
                var abandonmentFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _currentFieldId }
                };
                var abandonments = await wellAbandonmentRepo.GetAsync(abandonmentFilters);

                foreach (var abandonRecord in abandonments ?? Enumerable.Empty<object>())
                {
                    if (abandonRecord is not WELL_ABANDONMENT a)
                        continue;

                    var wellId = a.UWI;
                    var abandonmentDate = a.ABANDONMENT_DATE;

                    if (abandonmentDate.HasValue && !string.IsNullOrEmpty(wellId))
                    {
                        timeline.Events.Add(new FieldTimelineEvent
                        {
                            EventId = Guid.NewGuid().ToString(),
                            EventType = "Decommissioning",
                            EventDescription = $"Well abandoned: {wellId}",
                            EventDate = abandonmentDate.Value,
                            EntityType = "WELL",
                            EntityId = wellId
                        });
                    }
                }

                // Query FACILITY table for facility events
                var facilityRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY), _connectionName, "FACILITY");
                var facilityFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _currentFieldId }
                };
                var facilities = await facilityRepo.GetAsync(facilityFilters);

                foreach (var facilityRecord in facilities ?? Enumerable.Empty<object>())
                {
                    if (facilityRecord is not FACILITY f)
                        continue;

                    var facilityId = f.FACILITY_ID;
                    var facilityName = f.FACILITY_LONG_NAME;
                    var startDate = f.ACTIVE_DATE ?? f.CONSTRUCTED_DATE ?? f.EFFECTIVE_DATE;
                    var endDate = f.INACTIVE_DATE ?? f.ABANDONED_DATE ?? f.EXPIRY_DATE;

                    if (startDate.HasValue)
                    {
                        timeline.Events.Add(new FieldTimelineEvent
                        {
                            EventId = Guid.NewGuid().ToString(),
                            EventType = "Development",
                            EventDescription = $"Facility started: {facilityName ?? facilityId}",
                            EventDate = startDate.Value,
                            EntityType = "FACILITY",
                            EntityId = facilityId,
                            EntityName = facilityName
                        });
                    }

                    if (endDate.HasValue)
                    {
                        timeline.Events.Add(new FieldTimelineEvent
                        {
                            EventId = Guid.NewGuid().ToString(),
                            EventType = "Decommissioning",
                            EventDescription = $"Facility decommissioned: {facilityName ?? facilityId}",
                            EventDate = endDate.Value,
                            EntityType = "FACILITY",
                            EntityId = facilityId,
                            EntityName = facilityName
                        });
                    }
                }

                // Query POOL table for pool definition events
                var poolRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(POOL), _connectionName, "POOL");
                var poolFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _currentFieldId }
                };
                var pools = await poolRepo.GetAsync(poolFilters);

                foreach (var poolRecord in pools ?? Enumerable.Empty<object>())
                {
                    if (poolRecord is not POOL p)
                        continue;

                    var poolId = p.POOL_ID;
                    var poolName = p.POOL_NAME;
                    var definitionDate = p.DISCOVERY_DATE ?? p.EFFECTIVE_DATE ?? p.CURRENT_STATUS_DATE;

                    if (definitionDate.HasValue)
                    {
                        timeline.Events.Add(new FieldTimelineEvent
                        {
                            EventId = Guid.NewGuid().ToString(),
                            EventType = "Development",
                            EventDescription = $"Pool defined: {poolName ?? poolId}",
                            EventDate = definitionDate.Value,
                            EntityType = "POOL",
                            EntityId = poolId,
                            EntityName = poolName
                        });
                    }
                }

                // Sort events by date and calculate summary statistics
                timeline.Events = timeline.Events.OrderBy(e => e.EventDate).ToList();

                if (timeline.Events.Any())
                {
                    timeline.EarliestEventDate = timeline.Events.Min(e => e.EventDate);
                    timeline.LatestEventDate = timeline.Events.Max(e => e.EventDate);
                    timeline.EventCountsByPhase = timeline.Events
                        .GroupBy(e => e.EventType)
                        .ToDictionary(g => g.Key, g => g.Count());
                }

                return timeline;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting field timeline for field: {_currentFieldId}");
                throw;
            }
        }

        public async Task<FieldDashboard> GetFieldDashboardAsync()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            var dashboard = new FieldDashboard
            {
                FieldId = _currentFieldId,
                DashboardAsOfDate = DateTime.UtcNow,
                PerformanceMetrics = new List<FieldPerformanceMetric>(),
                RecentEvents = new List<FieldTimelineEvent>(),
                Alerts = new List<FieldDashboardAlert>(),
                KPIs = new Dictionary<string, object>()
            };

            try
            {
                // Get field information
                var field = await GetCurrentFieldAsync();
                if (field is FIELD fieldEntity)
                {
                    dashboard.FieldName = fieldEntity.FIELD_NAME ?? string.Empty;
                    dashboard.CurrentLifecyclePhase = fieldEntity.FIELD_TYPE;
                }

                // Get lifecycle summary and statistics in parallel for performance
                var summaryTask = GetFieldLifecycleSummaryAsync();
                var statisticsTask = GetFieldStatisticsAsync();
                await Task.WhenAll(summaryTask, statisticsTask);
                var summary = summaryTask.Result;
                var statistics = statisticsTask.Result;

                // Build performance metrics
                var metrics = new List<FieldPerformanceMetric>();

                // Exploration metrics
                metrics.Add(new FieldPerformanceMetric
                {
                    MetricName = "ProspectCount",
                    MetricLabel = "Prospects",
                    MetricType = "count",
                    CurrentValue = summary.ProspectCount,
                    Phase = "Exploration",
                    AsOfDate = dashboard.DashboardAsOfDate
                });

                metrics.Add(new FieldPerformanceMetric
                {
                    MetricName = "SeismicSurveyCount",
                    MetricLabel = "Seismic Surveys",
                    MetricType = "count",
                    CurrentValue = summary.SeismicSurveyCount,
                    Phase = "Exploration",
                    AsOfDate = dashboard.DashboardAsOfDate
                });

                // Development metrics
                metrics.Add(new FieldPerformanceMetric
                {
                    MetricName = "PoolCount",
                    MetricLabel = "Pools",
                    MetricType = "count",
                    CurrentValue = summary.PoolCount,
                    Phase = "Development",
                    AsOfDate = dashboard.DashboardAsOfDate
                });

                metrics.Add(new FieldPerformanceMetric
                {
                    MetricName = "FacilityCount",
                    MetricLabel = "Facilities",
                    MetricType = "count",
                    CurrentValue = summary.FacilityCount,
                    Phase = "Development",
                    AsOfDate = dashboard.DashboardAsOfDate
                });

                metrics.Add(new FieldPerformanceMetric
                {
                    MetricName = "DevelopmentWellCount",
                    MetricLabel = "Development Wells",
                    MetricType = "count",
                    CurrentValue = summary.DevelopmentWellCount,
                    Phase = "Development",
                    AsOfDate = dashboard.DashboardAsOfDate
                });

                // Production metrics
                metrics.Add(new FieldPerformanceMetric
                {
                    MetricName = "ProductionWellCount",
                    MetricLabel = "Production Wells",
                    MetricType = "count",
                    CurrentValue = summary.ProductionWellCount,
                    Phase = "Production",
                    AsOfDate = dashboard.DashboardAsOfDate
                });

                if (statistics.TotalOilProduction.HasValue)
                {
                    metrics.Add(new FieldPerformanceMetric
                    {
                        MetricName = "TotalOilProduction",
                        MetricLabel = "Total Oil Production",
                        MetricType = "volume",
                        CurrentValue = statistics.TotalOilProduction.Value,
                        Unit = "bbl",
                        Phase = "Production",
                        AsOfDate = dashboard.DashboardAsOfDate
                    });
                }

                if (statistics.AverageDailyProduction.HasValue)
                {
                    metrics.Add(new FieldPerformanceMetric
                    {
                        MetricName = "AverageDailyProduction",
                        MetricLabel = "Avg Daily Production",
                        MetricType = "volume",
                        CurrentValue = statistics.AverageDailyProduction.Value,
                        Unit = "bbl/day",
                        Phase = "Production",
                        AsOfDate = dashboard.DashboardAsOfDate
                    });
                }

                // Decommissioning metrics
                metrics.Add(new FieldPerformanceMetric
                {
                    MetricName = "AbandonedWellCount",
                    MetricLabel = "Abandoned Wells",
                    MetricType = "count",
                    CurrentValue = summary.AbandonedWellCount,
                    Phase = "Decommissioning",
                    AsOfDate = dashboard.DashboardAsOfDate
                });

                // Overall metrics
                metrics.Add(new FieldPerformanceMetric
                {
                    MetricName = "TotalWellCount",
                    MetricLabel = "Total Wells",
                    MetricType = "count",
                    CurrentValue = statistics.TotalWellCount,
                    AsOfDate = dashboard.DashboardAsOfDate
                });

                metrics.Add(new FieldPerformanceMetric
                {
                    MetricName = "ActiveWellCount",
                    MetricLabel = "Active Wells",
                    MetricType = "count",
                    CurrentValue = statistics.ActiveWellCount,
                    AsOfDate = dashboard.DashboardAsOfDate
                });

                dashboard.PerformanceMetrics = metrics;

                // Build phase summaries
                dashboard.ExplorationSummary = new FieldDashboardPhaseSummary
                {
                    PhaseName = "Exploration",
                    EntityCount = summary.ProspectCount + summary.SeismicSurveyCount + summary.ExploratoryWellCount,
                    EntityCountsByType = new Dictionary<string, int>
                    {
                        { "Prospects", summary.ProspectCount },
                        { "Seismic Surveys", summary.SeismicSurveyCount },
                        { "Exploratory Wells", summary.ExploratoryWellCount }
                    },
                    PhaseMetrics = metrics.Where(m => m.Phase == "Exploration").ToList()
                };

                dashboard.DevelopmentSummary = new FieldDashboardPhaseSummary
                {
                    PhaseName = "Development",
                    EntityCount = summary.PoolCount + summary.FacilityCount + summary.PipelineCount + summary.DevelopmentWellCount,
                    EntityCountsByType = new Dictionary<string, int>
                    {
                        { "Pools", summary.PoolCount },
                        { "Facilities", summary.FacilityCount },
                        { "Pipelines", summary.PipelineCount },
                        { "Development Wells", summary.DevelopmentWellCount }
                    },
                    PhaseMetrics = metrics.Where(m => m.Phase == "Development").ToList()
                };

                dashboard.ProductionSummary = new FieldDashboardPhaseSummary
                {
                    PhaseName = "Production",
                    EntityCount = summary.ProductionWellCount,
                    EntityCountsByType = new Dictionary<string, int>
                    {
                        { "Production Wells", summary.ProductionWellCount }
                    },
                    PhaseMetrics = metrics.Where(m => m.Phase == "Production").ToList(),
                    LastActivityDate = summary.LastProductionDate
                };

                dashboard.DecommissioningSummary = new FieldDashboardPhaseSummary
                {
                    PhaseName = "Decommissioning",
                    EntityCount = summary.AbandonedWellCount + summary.DecommissionedFacilityCount,
                    EntityCountsByType = new Dictionary<string, int>
                    {
                        { "Abandoned Wells", summary.AbandonedWellCount },
                        { "Decommissioned Facilities", summary.DecommissionedFacilityCount }
                    },
                    PhaseMetrics = metrics.Where(m => m.Phase == "Decommissioning").ToList()
                };

                // Build KPIs
                dashboard.KPIs = new Dictionary<string, object>
                {
                    { "TotalWells", statistics.TotalWellCount },
                    { "ActiveWells", statistics.ActiveWellCount },
                    { "TotalFacilities", summary.FacilityCount },
                    { "ActiveFacilities", statistics.ActiveFacilityCount },
                };

                if (statistics.TotalOilProduction.HasValue)
                {
                    dashboard.KPIs["TotalOilProduction"] = statistics.TotalOilProduction.Value;
                }

                if (statistics.ProvedReserves.HasValue)
                {
                    dashboard.KPIs["ProvedReserves"] = statistics.ProvedReserves.Value;
                }

                // Get recent timeline events (limit to last 10)
                var timeline = await GetFieldTimelineAsync();
                dashboard.RecentEvents = timeline.Events
                    .OrderByDescending(e => e.EventDate)
                    .Take(10)
                    .ToList();

                // Generate alerts based on business rules
                var alerts = new List<FieldDashboardAlert>();

                // Low Production Alert: Check if current production < 50% of average (last 3 months)
                if (statistics.LastProductionDate.HasValue)
                {
                    var daysSinceLastProduction = (DateTime.UtcNow - statistics.LastProductionDate.Value).Days;
                    if (daysSinceLastProduction <= 90) // Last 3 months
                    {
                        var recentProduction = statistics.TotalOilProduction ?? 0;
                        var avgDailyProduction = statistics.AverageDailyProduction ?? 0;
                        var expectedProduction = avgDailyProduction * 90; // Expected for 3 months
                        if (expectedProduction > 0 && recentProduction < (expectedProduction * 0.5m))
                        {
                            alerts.Add(new FieldDashboardAlert
                            {
                                AlertId = Guid.NewGuid().ToString(),
                                AlertType = "warning",
                                Title = "Low Production",
                                Message = $"Current production ({recentProduction:F2} bbl) is less than 50% of expected average ({expectedProduction * 0.5m:F2} bbl) for the last 3 months.",
                                Phase = "Production",
                                AlertDate = DateTime.UtcNow,
                                IsActive = true
                            });
                        }
                    }
                }

                // Missing Data Alert: Check for missing production data in last 30 days
                if (statistics.LastProductionDate.HasValue)
                {
                    var daysSinceLastProduction = (DateTime.UtcNow - statistics.LastProductionDate.Value).Days;
                    if (daysSinceLastProduction > 30)
                    {
                        alerts.Add(new FieldDashboardAlert
                        {
                            AlertId = Guid.NewGuid().ToString(),
                            AlertType = "error",
                            Title = "Missing Production Data",
                            Message = $"No production data recorded for {daysSinceLastProduction} days. Last production date: {statistics.LastProductionDate.Value:yyyy-MM-dd}",
                            Phase = "Production",
                            AlertDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }
                else if (statistics.FirstProductionDate.HasValue)
                {
                    // Field has production history but no recent data
                    alerts.Add(new FieldDashboardAlert
                    {
                        AlertId = Guid.NewGuid().ToString(),
                        AlertType = "warning",
                        Title = "No Recent Production Data",
                        Message = "No production data found for this field in the current period.",
                        Phase = "Production",
                        AlertDate = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                // Approaching Decommissioning Alert: Check wells/facilities with planned decommissioning dates within 90 days
                var decommissioningSvc = GetDecommissioningService();
                var abandonedWells = await decommissioningSvc.GetAbandonedWellsForFieldAsync(_currentFieldId);
                var decommissionedFacilities = await decommissioningSvc.GetDecommissionedFacilitiesForFieldAsync(_currentFieldId);
                
                // Alert when over 50% of field wells have been abandoned — signals advanced decommissioning stage
                if (abandonedWells.Count > statistics.TotalWellCount * 0.5m)
                {
                    alerts.Add(new FieldDashboardAlert
                    {
                        AlertId = Guid.NewGuid().ToString(),
                        AlertType = "info",
                        Title = "High Abandonment Rate",
                        Message = $"{abandonedWells.Count} out of {statistics.TotalWellCount} wells have been abandoned ({abandonedWells.Count * 100.0m / statistics.TotalWellCount:F1}%).",
                        Phase = "Decommissioning",
                        AlertDate = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                // High Discrepancy Alert: volume reconciliation requires an external accounting
                // service that is not available in the orchestrator. Callers that have access
                // to IProductionAccountingService can compare AllocationVolume vs MeasuredVolume
                // from PDEN_VOL_SUMMARY and emit this alert directly.

                // Reserve Depletion Alert: Check if reserves < 10% of original
                if (statistics.ProvedReserves.HasValue && statistics.TotalOilProduction.HasValue)
                {
                    var remainingReserves = statistics.ProvedReserves.Value;
                    var totalProduced = statistics.TotalOilProduction.Value;
                    var originalReserves = remainingReserves + totalProduced;
                    
                    if (originalReserves > 0)
                    {
                        var depletionPercentage = (totalProduced / originalReserves) * 100;
                        if (depletionPercentage > 90) // Less than 10% remaining
                        {
                            alerts.Add(new FieldDashboardAlert
                            {
                                AlertId = Guid.NewGuid().ToString(),
                                AlertType = "warning",
                                Title = "Reserve Depletion",
                                Message = $"Field reserves are {100 - depletionPercentage:F1}% of original. Consider decommissioning planning.",
                                Phase = "Production",
                                AlertDate = DateTime.UtcNow,
                                IsActive = true
                            });
                        }
                    }
                }

                // Overdue Tasks Alert: Check for WELL_ACTIVITY records > 30 days old with no follow-up
                try
                {
                    var activityMeta = await _metadata.GetTableMetadataAsync("WELL_ACTIVITY");
                    if (activityMeta != null)
                    {
                        var actRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                            typeof(WELL_ACTIVITY), _connectionName, "WELL_ACTIVITY");

                        // Look for active activities older than 30 days that imply pending work (INSP_ or MAINT_ prefix)
                        var cutoff = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
                        var actFilters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                            new AppFilter { FieldName = "EVENT_DATE", Operator = "<=", FilterValue = cutoff }
                        };

                        var oldActivities = await actRepo.GetAsync(actFilters);
                        var overdueActivities = oldActivities
                            .OfType<WELL_ACTIVITY>()
                            .Where(a => a.ACTIVITY_TYPE_ID != null &&
                                (a.ACTIVITY_TYPE_ID.StartsWith("INSP_") || a.ACTIVITY_TYPE_ID.StartsWith("MAINT_") || a.ACTIVITY_TYPE_ID.StartsWith("INSPECT")))
                            .ToList<object>();

                        if (overdueActivities.Count > 0)
                        {
                            alerts.Add(new FieldDashboardAlert
                            {
                                AlertId = Guid.NewGuid().ToString(),
                                AlertType = "warning",
                                Title = "Overdue Tasks",
                                Message = $"{overdueActivities.Count} inspection/maintenance task(s) recorded more than 30 days ago are still marked active. Review and close or reschedule.",
                                Phase = "Operations",
                                AlertDate = DateTime.UtcNow,
                                IsActive = true
                            });
                        }
                    }
                }
                catch (Exception overdueEx)
                {
                    _logger?.LogWarning(overdueEx, "Unable to check overdue tasks for field {FieldId}", _currentFieldId);
                }

                dashboard.Alerts = alerts;

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting field dashboard for field: {_currentFieldId}");
                throw;
            }
        }

        public IFieldExplorationService GetExplorationService()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            return _fieldExplorationService;
        }

        public IFieldDevelopmentService GetDevelopmentService()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            if (_developmentService == null)
            {
                _developmentService = new Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService(
                    _editor, _commonColumnHandler, _defaults, _metadata, _mappingService, _connectionName, null);
            }

            return _developmentService;
        }

        public IFieldProductionService GetProductionService()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            if (_productionService == null)
            {
                // PPDMProductionService now implements IFieldProductionService directly
                _productionService = new Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService(
                    _editor, _commonColumnHandler, _defaults, _metadata, _mappingService, _connectionName);
            }

            return _productionService;
        }

        public IFieldHSEService GetHSEService()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            if (_hseService == null)
            {
                _hseService = new PPDMHSEService(
                    _currentFieldId,
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    _connectionName);
            }

            return _hseService;
        }

        public IFieldDecommissioningService GetDecommissioningService()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            if (_decommissioningService == null)
            {
                _decommissioningService = new Beep.OilandGas.LifeCycle.Services.Decommissioning.PPDMDecommissioningService(
                    _editor, _commonColumnHandler, _defaults, _metadata, _mappingService, _connectionName, null);
            }

            return _decommissioningService;
        }

        public IProcessService GetProcessService()
        {
            if (string.IsNullOrEmpty(_currentFieldId))
            {
                throw new InvalidOperationException("No active field is set");
            }

            if (_processService == null)
            {
                _processService = new PPDMProcessService(
                    _editor, _commonColumnHandler, _defaults, _metadata, _connectionName);
            }

            return _processService;
        }

        public void ClearActiveField()
        {
            _currentFieldId = null;
            _fieldRepository = null;
            _developmentService = null;
            _productionService = null;
            _decommissioningService = null;
            _hseService = null;
            _processService = null;
            _logger?.LogInformation("Active field cleared");
        }

    }
}
