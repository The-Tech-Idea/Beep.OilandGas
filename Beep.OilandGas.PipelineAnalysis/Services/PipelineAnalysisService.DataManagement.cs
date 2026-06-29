using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PipelineAnalysis.Services
{
    /// <summary>
    /// Partial class: Data Management Methods (4 methods)
    /// </summary>
    public partial class PipelineAnalysisService
    {
        public async Task SaveAnalysisResultsAsync(PipelineAnalysisResult results, string userId)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving analysis results {AnalysisId} for pipeline {PipelineId}",
                results.AnalysisId, results.PipelineId);

            try
            {
                if (string.IsNullOrWhiteSpace(results.AnalysisId))
                {
                    results.AnalysisId = _defaults.FormatIdForTable("PIPELINE_ANALYSIS", Guid.NewGuid().ToString());
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PIPELINE_ANALYSIS_RESULT), _connectionName, "PIPELINE_ANALYSIS_RESULT");
                await repo.InsertAsync(results, userId);

                _logger?.LogInformation("Analysis results saved successfully: {AnalysisId}", results.AnalysisId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving analysis results for {PipelineId}", results.PipelineId);
                throw;
            }
        }

        public async Task<List<PipelineAnalysisResult>> GetAnalysisHistoryAsync(string pipelineId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));

            _logger?.LogInformation("Retrieving analysis history for {PipelineId} from {StartDate} to {EndDate}",
                pipelineId, startDate, endDate);

            try
            {
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PIPELINE_ANALYSIS_RESULT), _connectionName, "PIPELINE_ANALYSIS_RESULT");

                var filters = new List<AppFilter>
                {
                    new() { FieldName = "PIPELINE_ID", Operator = "=", FilterValue = pipelineId },
                    new() { FieldName = "ANALYSIS_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                    new() { FieldName = "ANALYSIS_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") },
                    new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                };

                var entities = await repo.GetAsync(filters);
                var history = entities.Cast<PIPELINE_ANALYSIS_RESULT>()
                    .Select(e => new PipelineAnalysisResult
                    {
                        AnalysisId = e.ANALYSIS_ID,
                        PipelineId = e.PIPELINE_ID,
                        AnalysisDate = e.ANALYSIS_DATE.GetValueOrDefault(),
                        FlowRate = e.FLOW_RATE.GetValueOrDefault(),
                        InletPressure = e.INLET_PRESSURE.GetValueOrDefault(),
                        OutletPressure = e.OUTLET_PRESSURE.GetValueOrDefault(),
                        PressureDrop = e.PRESSURE_DROP.GetValueOrDefault(),
                        Velocity = e.VELOCITY.GetValueOrDefault(),
                        FlowRegime = e.FLOW_REGIME,
                        Status = e.ANALYSIS_STATUS
                    }).ToList();

                _logger?.LogInformation("Analysis history retrieved: {Count} records found", history.Count);
                return history;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving analysis history for {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task UpdatePipelineConfigurationAsync(PipelineConfiguration config, string userId)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Updating pipeline configuration for {PipelineId}", config.PipelineId);

            try
            {
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PIPELINE), _connectionName, "PIPELINE");

                var filter = new List<AppFilter>
                {
                    new() { FieldName = "PIPELINE_ID", Operator = "=", FilterValue = config.PipelineId }
                };
                var entities = await repo.GetAsync(filter);
                var entity = entities.Cast<PIPELINE>().FirstOrDefault();

                if (entity != null)
                {
                    entity.DIAMETER = config.Diameter;
                    entity.WALL_THICKNESS = config.WallThickness;
                    entity.LENGTH = config.Length;
                    entity.MATERIAL = config.Material;
                    entity.DESIGN_PRESSURE = config.DesignPressure;
                    entity.DESIGN_TEMPERATURE = config.DesignTemperature;
                    entity.LAST_INSPECTION_DATE = config.LastInspectionDate;
                    entity.MAX_ALLOWABLE_WORKING_PRESSURE = config.MaxAllowableWorkingPressure;
                    await repo.UpdateAsync(entity, userId);
                }

                _logger?.LogInformation("Pipeline configuration updated successfully for {PipelineId}", config.PipelineId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating pipeline configuration for {PipelineId}", config.PipelineId);
                throw;
            }
        }

        public async Task<PipelineConfiguration?> GetPipelineConfigurationAsync(string pipelineId)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));

            _logger?.LogInformation("Retrieving pipeline configuration for {PipelineId}", pipelineId);

            try
            {
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PIPELINE), _connectionName, "PIPELINE");

                var filter = new List<AppFilter>
                {
                    new() { FieldName = "PIPELINE_ID", Operator = "=", FilterValue = pipelineId },
                    new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                };
                var entities = await repo.GetAsync(filter);
                var entity = entities.Cast<PIPELINE>().FirstOrDefault();

                if (entity == null)
                {
                    _logger?.LogInformation("No pipeline configuration found for {PipelineId}", pipelineId);
                    return null;
                }

                var config = new PipelineConfiguration
                {
                    PipelineId = entity.PIPELINE_ID,
                    Diameter = entity.DIAMETER.GetValueOrDefault(),
                    WallThickness = entity.WALL_THICKNESS.GetValueOrDefault(),
                    Length = entity.LENGTH.GetValueOrDefault(),
                    Material = entity.MATERIAL,
                    DesignPressure = entity.DESIGN_PRESSURE.GetValueOrDefault(),
                    DesignTemperature = entity.DESIGN_TEMPERATURE.GetValueOrDefault(),
                    LastInspectionDate = entity.LAST_INSPECTION_DATE.GetValueOrDefault(),
                    MaxAllowableWorkingPressure = entity.MAX_ALLOWABLE_WORKING_PRESSURE.GetValueOrDefault()
                };

                _logger?.LogInformation("Pipeline configuration retrieved for {PipelineId}", pipelineId);
                return config;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving pipeline configuration for {PipelineId}", pipelineId);
                throw;
            }
        }
    }
}
