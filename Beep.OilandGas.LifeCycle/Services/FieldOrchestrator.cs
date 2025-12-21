using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.LifeCycle.Services.Exploration;
using Beep.OilandGas.LifeCycle.Services.Development;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.LifeCycle.Services.Decommissioning;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

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

        // Phase services (created on demand when field is set)
        private IFieldExplorationService? _explorationService;
        private IFieldDevelopmentService? _developmentService;
        private IFieldProductionService? _productionService;
        private IFieldDecommissioningService? _decommissioningService;

        public string? CurrentFieldId => _currentFieldId;

        public FieldOrchestrator(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
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

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{fieldMetadata.EntityTypeName}");
                if (entityType == null)
                {
                    _logger?.LogError($"Entity type not found for FIELD: {fieldMetadata.EntityTypeName}");
                    return false;
                }

                _fieldRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FIELD");

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

                // Phase services will be created on demand via Get*Service() methods
                // Reset phase services to ensure they use the new field context
                _explorationService = null;
                _developmentService = null;
                _productionService = null;
                _decommissioningService = null;

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
                if (field != null && field is IDictionary<string, object> fieldDict)
                {
                    summary.FieldName = fieldDict.ContainsKey("FIELD_NAME") 
                        ? fieldDict["FIELD_NAME"]?.ToString() ?? string.Empty 
                        : string.Empty;
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

                // Get production data
                var productionSvc = GetProductionService();
                var production = await productionSvc.GetProductionForFieldAsync(_currentFieldId);
                // Calculate totals (simplified - would need to aggregate based on actual data structure)
                summary.ProductionWellCount = production?.Count ?? 0;

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

        private string? GetWellId(WELL well)
        {
            // Extract well ID from WELL object - this would need to match the actual property name
            // For now, return a placeholder
            var prop = well.GetType().GetProperty("WELL_ID") ?? well.GetType().GetProperty("WellId");
            return prop?.GetValue(well)?.ToString();
        }

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
                if (field != null && field is IDictionary<string, object> fieldDict)
                {
                    stats.FieldName = fieldDict.ContainsKey("FIELD_NAME") 
                        ? fieldDict["FIELD_NAME"]?.ToString() ?? string.Empty 
                        : string.Empty;
                }

                // Get all wells
                var wells = await GetFieldWellsAsync();
                stats.TotalWellCount = wells.Count;

                // Get production statistics
                var productionSvc = GetProductionService();
                var production = await productionSvc.GetProductionForFieldAsync(_currentFieldId);
                // TODO: Aggregate production volumes, dates, etc. based on actual data structure

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
                if (field != null && field is IDictionary<string, object> fieldDict)
                {
                    timeline.FieldName = fieldDict.ContainsKey("FIELD_NAME") 
                        ? fieldDict["FIELD_NAME"]?.ToString() ?? string.Empty 
                        : string.Empty;
                }

                // TODO: Query various tables for date fields and create timeline events
                // This would require querying PROSPECT, WELL, PRODUCTION, WELL_ABANDONMENT, etc.
                // and extracting date fields to create timeline events

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
                if (field != null && field is IDictionary<string, object> fieldDict)
                {
                    dashboard.FieldName = fieldDict.ContainsKey("FIELD_NAME") 
                        ? fieldDict["FIELD_NAME"]?.ToString() ?? string.Empty 
                        : string.Empty;
                    
                    // Determine current lifecycle phase based on field data
                    if (fieldDict.ContainsKey("FIELD_TYPE"))
                    {
                        dashboard.CurrentLifecyclePhase = fieldDict["FIELD_TYPE"]?.ToString();
                    }
                }

                // Get lifecycle summary to populate dashboard
                var summary = await GetFieldLifecycleSummaryAsync();
                
                // Get statistics for KPIs
                var statistics = await GetFieldStatisticsAsync();

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

                // TODO: Generate alerts based on business rules
                // For example: low production, missing data, approaching decommissioning dates, etc.

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

            if (_explorationService == null)
            {
                // Logger is optional, pass null for now
                _explorationService = new Beep.OilandGas.LifeCycle.Services.Exploration.PPDMExplorationService(
                    _editor, _commonColumnHandler, _defaults, _metadata, _mappingService, _connectionName, null);
            }

            return _explorationService;
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
    }
}
