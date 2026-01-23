using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services
{
    public partial class FieldOrchestrator
    {
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
                KPIs = new List<FieldKpi>()
            };

            try
            {
                // Get field information
                var field = await GetCurrentFieldAsync();
                if (field != null)
                {
                    dashboard.FieldName = field.FIELD_NAME ?? string.Empty;
                    dashboard.CurrentLifecyclePhase = field.FIELD_TYPE;
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
                    EntityCountsByType = new List<FieldEntityCount>
                    {
                        new FieldEntityCount { EntityType = "Prospects", Count = summary.ProspectCount },
                        new FieldEntityCount { EntityType = "Seismic Surveys", Count = summary.SeismicSurveyCount },
                        new FieldEntityCount { EntityType = "Exploratory Wells", Count = summary.ExploratoryWellCount }
                    },
                    PhaseMetrics = metrics.Where(m => m.Phase == "Exploration").ToList()
                };

                dashboard.DevelopmentSummary = new FieldDashboardPhaseSummary
                {
                    PhaseName = "Development",
                    EntityCount = summary.PoolCount + summary.FacilityCount + summary.PipelineCount + summary.DevelopmentWellCount,
                    EntityCountsByType = new List<FieldEntityCount>
                    {
                        new FieldEntityCount { EntityType = "Pools", Count = summary.PoolCount },
                        new FieldEntityCount { EntityType = "Facilities", Count = summary.FacilityCount },
                        new FieldEntityCount { EntityType = "Pipelines", Count = summary.PipelineCount },
                        new FieldEntityCount { EntityType = "Development Wells", Count = summary.DevelopmentWellCount }
                    },
                    PhaseMetrics = metrics.Where(m => m.Phase == "Development").ToList()
                };

                dashboard.ProductionSummary = new FieldDashboardPhaseSummary
                {
                    PhaseName = "Production",
                    EntityCount = summary.ProductionWellCount,
                    EntityCountsByType = new List<FieldEntityCount>
                    {
                        new FieldEntityCount { EntityType = "Production Wells", Count = summary.ProductionWellCount }
                    },
                    PhaseMetrics = metrics.Where(m => m.Phase == "Production").ToList(),
                    LastActivityDate = summary.LastProductionDate
                };

                dashboard.DecommissioningSummary = new FieldDashboardPhaseSummary
                {
                    PhaseName = "Decommissioning",
                    EntityCount = summary.AbandonedWellCount + summary.DecommissionedFacilityCount,
                    EntityCountsByType = new List<FieldEntityCount>
                    {
                        new FieldEntityCount { EntityType = "Abandoned Wells", Count = summary.AbandonedWellCount },
                        new FieldEntityCount { EntityType = "Decommissioned Facilities", Count = summary.DecommissionedFacilityCount }
                    },
                    PhaseMetrics = metrics.Where(m => m.Phase == "Decommissioning").ToList()
                };

                // Build KPIs
                dashboard.KPIs = new List<FieldKpi>
                {
                    new FieldKpi { Name = "TotalWells", Value = statistics.TotalWellCount, AsOfDate = dashboard.DashboardAsOfDate },
                    new FieldKpi { Name = "ActiveWells", Value = statistics.ActiveWellCount, AsOfDate = dashboard.DashboardAsOfDate },
                    new FieldKpi { Name = "TotalFacilities", Value = summary.FacilityCount, AsOfDate = dashboard.DashboardAsOfDate },
                    new FieldKpi { Name = "ActiveFacilities", Value = statistics.ActiveFacilityCount, AsOfDate = dashboard.DashboardAsOfDate }
                };

                if (statistics.TotalOilProduction.HasValue)
                {
                    dashboard.KPIs.Add(new FieldKpi
                    {
                        Name = "TotalOilProduction",
                        Value = statistics.TotalOilProduction.Value,
                        Unit = "bbl",
                        AsOfDate = dashboard.DashboardAsOfDate
                    });
                }

                if (statistics.ProvedReserves.HasValue)
                {
                    dashboard.KPIs.Add(new FieldKpi
                    {
                        Name = "ProvedReserves",
                        Value = statistics.ProvedReserves.Value,
                        AsOfDate = dashboard.DashboardAsOfDate
                    });
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

                // Check for wells/facilities approaching decommissioning (would need planned dates in data model)
                // For now, check if there are many abandoned wells/facilities
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

                // High Discrepancy Alert: Check volume reconciliation discrepancies > 5%
                // This would require calling volume reconciliation service
                // For now, skip as it requires additional service dependency

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

                // Overdue Tasks Alert: Check for incomplete process steps older than 30 days
                // This would require process service integration
                // For now, skip as it requires additional service dependency

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
