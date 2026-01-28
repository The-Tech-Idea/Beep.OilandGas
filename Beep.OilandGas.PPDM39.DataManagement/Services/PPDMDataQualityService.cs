using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for measuring and reporting on data quality metrics
    /// Provides simple and efficient quality assessment for oil and gas data
    /// </summary>
    public class PPDMDataQualityService : IPPDMDataQualityService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly PPDMGenericRepository _qualityScoreRepository;
        private readonly PPDMGenericRepository _qualityMetricsRepository;
        private readonly PPDMGenericRepository _qualityIssueRepository;

        public PPDMDataQualityService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;

            // Create repositories for quality entities
            _qualityScoreRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(DATA_QUALITY_SCORE), _connectionName, "DATA_QUALITY_SCORE");
            
            _qualityMetricsRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(DATA_QUALITY_METRICS), _connectionName, "DATA_QUALITY_METRICS");
            
            _qualityIssueRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(DATA_QUALITY_ISSUE), _connectionName, "DATA_QUALITY_ISSUE");
        }

        /// <summary>
        /// Calculates data quality score for an entity
        /// </summary>
        public async Task<DATA_QUALITY_SCORE> CalculateQualityScoreAsync(object entity, string tableName)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            var entityType = entity.GetType();
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fieldScores = new Dictionary<string, double>();
            var issues = new List<DATA_QUALITY_ISSUE>();

            double totalScore = 0;
            int fieldCount = 0;

            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                var fieldScore = CalculateFieldQuality(property, value, tableName, out var fieldIssues);
                
                fieldScores[property.Name] = fieldScore;
                issues.AddRange(fieldIssues);
                
                totalScore += fieldScore;
                fieldCount++;
            }

            var overallScore = fieldCount > 0 ? totalScore / fieldCount : 0;

            var qualityScore = new DATA_QUALITY_SCORE
            {
                ENTITY = entity,
                TABLE_NAME = tableName,
                OVERALL_SCORE = overallScore,
                FIELD_SCORES_JSON = System.Text.Json.JsonSerializer.Serialize(fieldScores),
                ISSUES_JSON = System.Text.Json.JsonSerializer.Serialize(issues)
            };

            // Persist quality score to database
            await SaveQualityScoreAsync(qualityScore, entity, tableName);

            return qualityScore;
        }

        /// <summary>
        /// Saves quality score to database
        /// </summary>
        private async Task SaveQualityScoreAsync(DATA_QUALITY_SCORE score, object entity, string tableName)
        {
            // Get entity ID
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null) return;

            var entityType = entity.GetType();
            var pkColumn = metadata.PrimaryKeyColumn.Split(',').FirstOrDefault()?.Trim();
            var pkProperty = entityType.GetProperty(pkColumn, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pkProperty == null) return;

            var entityId = pkProperty.GetValue(entity)?.ToString() ?? string.Empty;

            var qualityScoreEntity = new DATA_QUALITY_SCORE
            {
                QUALITY_SCORE_ID = Guid.NewGuid().ToString(),
                TABLE_NAME = tableName,
                ENTITY_ID = entityId,
                OVERALL_SCORE = score.OVERALL_SCORE,
                SCORE_DATE = DateTime.UtcNow,
                FIELD_SCORES_JSON = System.Text.Json.JsonSerializer.Serialize(score.FIELD_SCORES_JSON),
                ACTIVE_IND = _defaults?.GetActiveIndicatorYes() ?? "Y",
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = "SYSTEM"
            };

            await _qualityScoreRepository.InsertAsync(qualityScoreEntity, "SYSTEM");

            // Save quality issues
            foreach (var issue in score.ISSUES)
            {
                await SaveQualityIssueAsync(issue, entityId);
            }
        }

        /// <summary>
        /// Saves quality issue to database
        /// </summary>
        private async Task SaveQualityIssueAsync(DATA_QUALITY_ISSUE issue, string entityId)
        {
            var issueEntity = new DATA_QUALITY_ISSUE
            {
                QUALITY_ISSUE_ID = Guid.NewGuid().ToString(),
                TABLE_NAME = issue.TABLE_NAME,
                FIELD_NAME = issue.FIELD_NAME,
                ENTITY_ID = entityId,
                ISSUE_TYPE = issue.ISSUE_TYPE,
                ISSUE_DESCRIPTION = issue.ISSUE_DESCRIPTION,
                SEVERITY = issue.SEVERITY,
                ISSUE_DATE = DateTime.UtcNow,
                RESOLVED_IND = _defaults?.GetActiveIndicatorNo() ?? "N",
                ACTIVE_IND = _defaults?.GetActiveIndicatorYes() ?? "Y",
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = "SYSTEM"
            };

            await _qualityIssueRepository.InsertAsync(issueEntity, "SYSTEM");
        }

        /// <summary>
        /// Calculates data quality metrics for a table
        /// </summary>
        public async Task<DATA_QUALITY_METRICS> CalculateTableQualityMetricsAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            // Get metadata
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
                throw new InvalidOperationException($"Metadata not found for table: {tableName}");

            // Get entity type
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
            if (entityType == null)
                throw new InvalidOperationException($"Entity type not found: {metadata.EntityTypeName}");

            // Create repository to query data
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, tableName);

            // Get all active records
            var allRecords = await repo.GetActiveAsync();
            var records = allRecords.ToList();

            var totalRecords = records.Count;
            var completeRecords = 0;
            var incompleteRecords = 0;
            var fieldMetrics = new Dictionary<string, FieldQualityMetrics>();

            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var pkColumns = metadata.PrimaryKeyColumn.Split(',').Select(c => c.Trim()).ToList();

            // Initialize field metrics
            foreach (var property in properties)
            {
                if (!pkColumns.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                {
                    fieldMetrics[property.Name] = new FieldQualityMetrics
                    {
                        FieldName = property.Name,
                        TotalValues = totalRecords,
                        NullValues = 0,
                        EmptyValues = 0,
                        ValidValues = 0
                    };
                }
            }

            // Analyze each record
            foreach (var record in records)
            {
                bool isComplete = true;

                foreach (var property in properties)
                {
                    if (pkColumns.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                        continue;

                    var value = property.GetValue(record);
                    var fieldMetric = fieldMetrics[property.Name];

                    if (value == null)
                    {
                        fieldMetric.NullValues++;
                        isComplete = false;
                    }
                    else if (value is string str && string.IsNullOrWhiteSpace(str))
                    {
                        fieldMetric.EmptyValues++;
                        isComplete = false;
                    }
                    else
                    {
                        fieldMetric.ValidValues++;
                    }
                }

                if (isComplete)
                    completeRecords++;
                else
                    incompleteRecords++;
            }

            // Calculate completeness for each field
            foreach (var fieldMetric in fieldMetrics.Values)
            {
                if (fieldMetric.TotalValues > 0)
                {
                    fieldMetric.Completeness = (double)fieldMetric.ValidValues / fieldMetric.TotalValues * 100;
                }
            }

            // Calculate overall scores
            var completenessScore = totalRecords > 0 ? (double)completeRecords / totalRecords * 100 : 0;
            var accuracyScore = 100.0; // Would need validation rules to calculate
            var consistencyScore = 100.0; // Would need cross-field validation
            var overallScore = (completenessScore + accuracyScore + consistencyScore) / 3;

            var metrics = new DATA_QUALITY_METRICS
            {
                TABLE_NAME = tableName,
                TOTAL_RECORDS = totalRecords,
                COMPLETE_RECORDS = completeRecords,
                INCOMPLETE_RECORDS = incompleteRecords,
                COMPLETENESS_SCORE = completenessScore,
                ACCURACY_SCORE = accuracyScore,
                CONSISTENCY_SCORE = consistencyScore,
                OVERALL_QUALITY_SCORE = overallScore,
                FIELD_METRICS = fieldMetrics
            };

            // Persist metrics to database
            await SaveQualityMetricsAsync(metrics);

            return metrics;
        }

        /// <summary>
        /// Saves quality metrics to database
        /// </summary>
        private async Task SaveQualityMetricsAsync(DATA_QUALITY_METRICS metrics)
        {
            var metricsEntity = new DATA_QUALITY_METRICS
            {
                METRICS_ID = Guid.NewGuid().ToString(),
                TABLE_NAME = metrics.TABLE_NAME,
                TOTAL_RECORDS = metrics.TOTAL_RECORDS,
                COMPLETE_RECORDS = metrics.COMPLETE_RECORDS,
                INCOMPLETE_RECORDS = metrics.INCOMPLETE_RECORDS,
                COMPLETENESS_SCORE = metrics.COMPLETENESS_SCORE,
                ACCURACY_SCORE = metrics.ACCURACY_SCORE,
                CONSISTENCY_SCORE = metrics.CONSISTENCY_SCORE,
                OVERALL_QUALITY_SCORE = metrics.OVERALL_QUALITY_SCORE,
                METRICS_DATE = DateTime.UtcNow,
                FIELD_METRICS_JSON = System.Text.Json.JsonSerializer.Serialize(metrics.FIELD_METRICS),
                ACTIVE_IND = _defaults?.GetActiveIndicatorYes() ?? "Y",
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = "SYSTEM"
            };

            await _qualityMetricsRepository.InsertAsync(metricsEntity, "SYSTEM");
        }

        /// <summary>
        /// Finds data quality issues in a table
        /// </summary>
        public async Task<List<DATA_QUALITY_ISSUE>> FindQualityIssuesAsync(string tableName, List<string> fieldNames = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            var issues = new List<DATA_QUALITY_ISSUE>();
            var metrics = await CalculateTableQualityMetricsAsync(tableName);

            // Find issues in each field
            foreach (var fieldMetric in metrics.FIELD_METRICS)
            {
                if (fieldNames != null && !fieldNames.Contains(fieldMetric.Key, StringComparer.OrdinalIgnoreCase))
                    continue;

                // Low completeness
                if (fieldMetric.Value.Completeness < 50)
                {
                    issues.Add(new DATA_QUALITY_ISSUE
                    {
                        TABLE_NAME = tableName,
                        FIELD_NAME = fieldMetric.Key,
                        ISSUE_TYPE = "CompletenessIssue",
                        ISSUE_DESCRIPTION = $"Field '{fieldMetric.Key}' has low completeness: {fieldMetric.Value.Completeness:F2}%",
                        SEVERITY = "High"
                    });
                }

                // Many null values
                if (fieldMetric.Value.NullValues > fieldMetric.Value.TotalValues * 0.3)
                {
                    issues.Add(new DATA_QUALITY_ISSUE
                    {
                        TABLE_NAME = tableName,
                        FIELD_NAME = fieldMetric.Key,
                        ISSUE_TYPE = "NullValueIssue",
                        ISSUE_DESCRIPTION = $"Field '{fieldMetric.Key}' has {fieldMetric.Value.NullValues} null values ({fieldMetric.Value.NullValues * 100.0 / fieldMetric.Value.TotalValues:F2}%)",
                        SEVERITY = "Medium"
                    });
                }
            }

            // Overall quality issues
            if (metrics.OVERALL_QUALITY_SCORE < 70)
            {
                issues.Add(new DATA_QUALITY_ISSUE
                {
                    TABLE_NAME = tableName,
                    ISSUE_TYPE = "OverallQualityIssue",
                    ISSUE_DESCRIPTION = $"Overall quality score is below threshold: {metrics.OVERALL_QUALITY_SCORE:F2}%",
                    SEVERITY = "High"
                });
            }

            return issues;
        }

        /// <summary>
        /// Calculates quality score for a single field
        /// </summary>
        private double CalculateFieldQuality(PropertyInfo property, object value, string tableName, out List<DATA_QUALITY_ISSUE> issues)
        {
            issues = new List<DATA_QUALITY_ISSUE>();
            double score = 100.0;

            // Check for null
            if (value == null)
            {
                score -= 50;
                issues.Add(new DATA_QUALITY_ISSUE
                {
                    TABLE_NAME = tableName,
                    FIELD_NAME = property.Name,
                    ISSUE_TYPE = "NullValue",
                    ISSUE_DESCRIPTION = $"Field '{property.Name}' is null",
                    SEVERITY = "Medium"
                });
            }
            // Check for empty string
            else if (value is string str && string.IsNullOrWhiteSpace(str))
            {
                score -= 30;
                issues.Add(new DATA_QUALITY_ISSUE
                {
                    TABLE_NAME = tableName,
                    FIELD_NAME = property.Name,
                    ISSUE_TYPE = "EmptyValue",
                    ISSUE_DESCRIPTION = $"Field '{property.Name}' is empty",
                    SEVERITY = "Low"
                });
            }
            // Check for default values that might indicate missing data
            else if (value is DateTime dt && dt == DateTime.MinValue)
            {
                score -= 40;
                issues.Add(new DATA_QUALITY_ISSUE
                {
                    TABLE_NAME   = tableName,
                    FIELD_NAME = property.Name,
                    ISSUE_TYPE = "DefaultValue",
                    ISSUE_DESCRIPTION = $"Field '{property.Name}' has default DateTime value",
                    SEVERITY = "Medium"
                });
            }

            return Math.Max(0, Math.Min(100, score));
        }
    }
}

