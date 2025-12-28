using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.Core.Interfaces;
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
        }

        /// <summary>
        /// Calculates data quality score for an entity
        /// </summary>
        public async Task<DataQualityScore> CalculateQualityScoreAsync(object entity, string tableName)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            var entityType = entity.GetType();
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fieldScores = new Dictionary<string, double>();
            var issues = new List<DataQualityIssue>();

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

            return new DataQualityScore
            {
                Entity = entity,
                TableName = tableName,
                OverallScore = overallScore,
                FieldScores = fieldScores,
                Issues = issues
            };
        }

        /// <summary>
        /// Calculates data quality metrics for a table
        /// </summary>
        public async Task<DataQualityMetrics> CalculateTableQualityMetricsAsync(string tableName)
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

            return new DataQualityMetrics
            {
                TableName = tableName,
                TotalRecords = totalRecords,
                CompleteRecords = completeRecords,
                IncompleteRecords = incompleteRecords,
                CompletenessScore = completenessScore,
                AccuracyScore = accuracyScore,
                ConsistencyScore = consistencyScore,
                OverallQualityScore = overallScore,
                FieldMetrics = fieldMetrics
            };
        }

        /// <summary>
        /// Finds data quality issues in a table
        /// </summary>
        public async Task<List<DataQualityIssue>> FindQualityIssuesAsync(string tableName, List<string> fieldNames = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            var issues = new List<DataQualityIssue>();
            var metrics = await CalculateTableQualityMetricsAsync(tableName);

            // Find issues in each field
            foreach (var fieldMetric in metrics.FieldMetrics)
            {
                if (fieldNames != null && !fieldNames.Contains(fieldMetric.Key, StringComparer.OrdinalIgnoreCase))
                    continue;

                // Low completeness
                if (fieldMetric.Value.Completeness < 50)
                {
                    issues.Add(new DataQualityIssue
                    {
                        TableName = tableName,
                        FieldName = fieldMetric.Key,
                        IssueType = "CompletenessIssue",
                        IssueDescription = $"Field '{fieldMetric.Key}' has low completeness: {fieldMetric.Value.Completeness:F2}%",
                        Severity = "High"
                    });
                }

                // Many null values
                if (fieldMetric.Value.NullValues > fieldMetric.Value.TotalValues * 0.3)
                {
                    issues.Add(new DataQualityIssue
                    {
                        TableName = tableName,
                        FieldName = fieldMetric.Key,
                        IssueType = "NullValueIssue",
                        IssueDescription = $"Field '{fieldMetric.Key}' has {fieldMetric.Value.NullValues} null values ({fieldMetric.Value.NullValues * 100.0 / fieldMetric.Value.TotalValues:F2}%)",
                        Severity = "Medium"
                    });
                }
            }

            // Overall quality issues
            if (metrics.OverallQualityScore < 70)
            {
                issues.Add(new DataQualityIssue
                {
                    TableName = tableName,
                    IssueType = "OverallQualityIssue",
                    IssueDescription = $"Overall quality score is below threshold: {metrics.OverallQualityScore:F2}%",
                    Severity = "High"
                });
            }

            return issues;
        }

        /// <summary>
        /// Calculates quality score for a single field
        /// </summary>
        private double CalculateFieldQuality(PropertyInfo property, object value, string tableName, out List<DataQualityIssue> issues)
        {
            issues = new List<DataQualityIssue>();
            double score = 100.0;

            // Check for null
            if (value == null)
            {
                score -= 50;
                issues.Add(new DataQualityIssue
                {
                    TableName = tableName,
                    FieldName = property.Name,
                    IssueType = "NullValue",
                    IssueDescription = $"Field '{property.Name}' is null",
                    Severity = "Medium"
                });
            }
            // Check for empty string
            else if (value is string str && string.IsNullOrWhiteSpace(str))
            {
                score -= 30;
                issues.Add(new DataQualityIssue
                {
                    TableName = tableName,
                    FieldName = property.Name,
                    IssueType = "EmptyValue",
                    IssueDescription = $"Field '{property.Name}' is empty",
                    Severity = "Low"
                });
            }
            // Check for default values that might indicate missing data
            else if (value is DateTime dt && dt == DateTime.MinValue)
            {
                score -= 40;
                issues.Add(new DataQualityIssue
                {
                    TableName = tableName,
                    FieldName = property.Name,
                    IssueType = "DefaultValue",
                    IssueDescription = $"Field '{property.Name}' has default DateTime value",
                    Severity = "Medium"
                });
            }

            return Math.Max(0, Math.Min(100, score));
        }
    }
}

