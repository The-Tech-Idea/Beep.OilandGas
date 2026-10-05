using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdeaWeb.Diagnostics;
using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.LifeCycle.Services.DataMapping;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> EntityPropertyCache =
            new ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>();
        private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> ResultPropertyCache =
            new ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>();

        private Type? FindEntityTypeByName(string? entityTypeName)
        {
            if (string.IsNullOrWhiteSpace(entityTypeName))
            {
                return null;
            }

            var ppdmAssembly = typeof(ANL_ANALYSIS_REPORT).Assembly;
            var modelAssembly = typeof(ModelEntityBase).Assembly;

            return FindEntityTypeInAssembly(ppdmAssembly, entityTypeName)
                ?? FindEntityTypeInAssembly(modelAssembly, entityTypeName);
        }

        private Type? FindEntityTypeInAssembly(Assembly assembly, string entityTypeName)
        {
            var ppdmQualified = assembly.GetType($"Beep.OilandGas.PPDM39.Models.{entityTypeName}", false, true);
            if (ppdmQualified != null && typeof(IPPDMEntity).IsAssignableFrom(ppdmQualified))
            {
                return ppdmQualified;
            }

            var modelQualified = assembly.GetType($"Beep.OilandGas.Models.Data.{entityTypeName}", false, true);
            if (modelQualified != null && typeof(IPPDMEntity).IsAssignableFrom(modelQualified))
            {
                return modelQualified;
            }

            var normalizedTarget = NormalizePropertyName(entityTypeName);
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // No question answers "which types load" without loading them. The types that did load are still
                // searched; the ones that did not are a deployment fault the operator needs to see.
                _failures.ReportHandled(ex, $"listing the entity types in {assembly.GetName().Name}",
                    "only the types that load are searched for a calculation result table's entity type",
                    FailureSeverity.Degraded);
                types = ex.Types.Where(type => type != null).ToArray()!;
            }

            return types.FirstOrDefault(type =>
                type != null &&
                typeof(IPPDMEntity).IsAssignableFrom(type) &&
                NormalizePropertyName(type.Name).Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase));
        }

        private Type ResolvePPDMEntityType(string? entityTypeName, string tableName)
        {
            var resolvedType = FindEntityTypeByName(entityTypeName)
                ?? FindEntityTypeByName(tableName);

            if (resolvedType == null)
            {
                throw new InvalidOperationException(
                    $"Entity type not found for table {tableName} (entity type {entityTypeName ?? "unknown"}).");
            }

            return resolvedType;
        }

        private static string NormalizePropertyName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var sanitized = name.Where(char.IsLetterOrDigit).ToArray();
            return new string(sanitized).ToUpperInvariant();
        }

        private static IReadOnlyDictionary<string, PropertyInfo> GetEntityPropertyMap(Type entityType)
        {
            return EntityPropertyCache.GetOrAdd(entityType, type =>
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(prop => prop.CanWrite)
                    .ToDictionary(prop => NormalizePropertyName(prop.Name), prop => prop));
        }

        private static IReadOnlyDictionary<string, PropertyInfo> GetReadablePropertyMap(Type resultType)
        {
            return ResultPropertyCache.GetOrAdd(resultType, type =>
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(prop => prop.CanRead)
                    .ToDictionary(prop => NormalizePropertyName(prop.Name), prop => prop));
        }

        private static bool TryResolveResultPropertyAlias(
            string normalizedEntityProperty,
            IReadOnlyDictionary<string, PropertyInfo> resultPropertyMap,
            out PropertyInfo? resultProperty)
        {
            resultProperty = null;

            if (normalizedEntityProperty.EndsWith("ANALYSISID", StringComparison.OrdinalIgnoreCase) ||
                normalizedEntityProperty.EndsWith("CALCULATIONID", StringComparison.OrdinalIgnoreCase) ||
                normalizedEntityProperty.EndsWith("RESULTID", StringComparison.OrdinalIgnoreCase))
            {
                if (resultPropertyMap.TryGetValue("CALCULATIONID", out resultProperty) ||
                    resultPropertyMap.TryGetValue("ANALYSISID", out resultProperty) ||
                    resultPropertyMap.TryGetValue("RESULTID", out resultProperty))
                {
                    return true;
                }
            }

            if (normalizedEntityProperty.Equals("WELLTESTID", StringComparison.OrdinalIgnoreCase))
            {
                if (resultPropertyMap.TryGetValue("CALCULATIONID", out resultProperty) ||
                    resultPropertyMap.TryGetValue("ANALYSISID", out resultProperty))
                {
                    return true;
                }
            }

            if (normalizedEntityProperty.EndsWith("ANALYSISDATE", StringComparison.OrdinalIgnoreCase) ||
                normalizedEntityProperty.EndsWith("CALCULATIONDATE", StringComparison.OrdinalIgnoreCase))
            {
                if (resultPropertyMap.TryGetValue("CALCULATIONDATE", out resultProperty) ||
                    resultPropertyMap.TryGetValue("ANALYSISDATE", out resultProperty))
                {
                    return true;
                }
            }

            return false;
        }

        private object CreateEntityFromResult(PPDMGenericRepository repository, object result)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var entityType = repository.EntityType;
            if (entityType.IsAssignableFrom(result.GetType()))
            {
                return result;
            }

            var entity = Activator.CreateInstance(entityType)
                ?? throw new InvalidOperationException($"Unable to create entity of type {entityType.Name}.");

            var propertyMap = GetEntityPropertyMap(entityType);
            var resultPropertyMap = GetReadablePropertyMap(result.GetType());

            foreach (var entityProperty in propertyMap)
            {
                if (!resultPropertyMap.TryGetValue(entityProperty.Key, out var resultProperty))
                {
                    if (!TryResolveResultPropertyAlias(entityProperty.Key, resultPropertyMap, out resultProperty))
                    {
                        continue;
                    }
                }

                var value = resultProperty!.GetValue(result);
                TrySetPropertyValue(entity, entityProperty.Value, value);
            }

            return entity;
        }

        private bool TrySetPropertyValue(object entity, PropertyInfo property, object? value)
        {
            try
            {
                if (value == null || value == DBNull.Value)
                {
                    property.SetValue(entity, null);
                    return true;
                }

                if (value is JsonElement jsonElement)
                {
                    value = jsonElement.ValueKind == JsonValueKind.String
                        ? jsonElement.GetString()
                        : jsonElement.ToString();
                }

                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (value is Enum enumValue && targetType == typeof(string))
                {
                    value = enumValue.ToString();
                }
                else if (targetType.IsEnum && value is string enumText)
                {
                    if (!Enum.TryParse(targetType, enumText, ignoreCase: true, out var member))
                    {
                        _logger?.LogWarning("{Value} is not a {EnumType}; {PropertyName} on {EntityType} is left unset",
                            enumText, targetType.Name, property.Name, entity.GetType().Name);
                        return false;
                    }

                    value = member;
                }
                else if (targetType.IsEnum && value != null && !targetType.IsInstanceOfType(value))
                {
                    value = Enum.ToObject(targetType, value);
                }

                if (!targetType.IsInstanceOfType(value))
                {
                    value = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                }

                property.SetValue(entity, value);
                return true;
            }
            // Convert.ChangeType has no question to ask first: these are its refusals of a value, and the setter's own.
            catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException
                                           or TargetInvocationException)
            {
                _failures.ReportHandled(ex, $"storing {property.Name} on a {entity.GetType().Name} calculation result row",
                    $"the stored row leaves {property.Name} empty; the calculation result returned to the caller is complete",
                    FailureSeverity.Degraded);
                return false;
            }
        }

        private async Task InsertAnalysisResultAsync(
            PPDMGenericRepository repository,
            object result,
            string? userId)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            var entity = CreateEntityFromResult(repository, result);
            var effectiveUserId = string.IsNullOrWhiteSpace(userId) ? "SYSTEM" : userId;
            await repository.InsertAsync(entity, effectiveUserId);
        }

        private async Task<PPDMGenericRepository> CreateAnalysisResultRepositoryAsync(
            string primaryTableName,
            string fallbackTableName = "ANL_ANALYSIS_REPORT")
        {
            if (string.IsNullOrWhiteSpace(primaryTableName))
            {
                throw new ArgumentException("Primary table name must be provided", nameof(primaryTableName));
            }

            var metadata = await _metadata.GetTableMetadataAsync(primaryTableName);
            if (metadata == null && !string.IsNullOrWhiteSpace(fallbackTableName))
            {
                metadata = await _metadata.GetTableMetadataAsync(fallbackTableName);
            }

            if (metadata == null)
            {
                throw new InvalidOperationException($"{primaryTableName} or {fallbackTableName} table not found");
            }

            var tableName = string.IsNullOrWhiteSpace(metadata.TableName) ? primaryTableName : metadata.TableName;
            var entityType = ResolvePPDMEntityType(metadata.EntityTypeName, tableName);

            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                entityType,
                _connectionName,
                tableName,
                null);
        }

        private double? ConvertToDouble(object? value)
        {
            // Asked rather than caught: a value that is not a number is answered as no value.
            return value switch
            {
                null => null,
                double d => d,
                float f => f,
                decimal m => (double)m,
                int i => i,
                long l => l,
                short s => s,
                _ => double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : null
            };
        }

        private DateTime? GetDateValue(object? entity, string propertyName)
        {
            if (entity == null || string.IsNullOrEmpty(propertyName))
                return null;

            var prop = entity.GetType().GetProperty(propertyName);
            if (prop == null)
                return null;

            var value = prop.GetValue(entity);
            if (value is DateTime dt) return dt;
            if (DateTime.TryParse(value?.ToString(), out var parsed)) return parsed;

            return null;
        }

        private decimal? GetPropertyValue(object? entity, string propertyName)
        {
            if (entity == null || string.IsNullOrEmpty(propertyName))
                return null;

            var prop = entity.GetType().GetProperty(propertyName);
            if (prop == null)
                return null;

            return ConvertToDecimal(prop.GetValue(entity));
        }

        private decimal? GetPropertyValueMultiple(object? entity, params string[] propertyNames)
        {
            if (entity == null)
                return null;

            foreach (var propName in propertyNames)
            {
                var value = GetPropertyValue(entity, propName);
                if (value.HasValue)
                    return value;
            }
            return null;
        }

        private decimal? ConvertToDecimal(object? value)
        {
            if (value == null) return null;
            if (value is decimal dec) return dec;
            if (value is double d) return (decimal)d;
            if (value is float f) return (decimal)f;
            if (value is int i) return i;
            if (value is long l) return l;
            if (decimal.TryParse(value.ToString(), out var parsed)) return parsed;
            return null;
        }

        /// <summary>
        /// What the calculation history records for a run that did not complete: a refusal's own sentence, or that the run
        /// did not complete. Never the exception's text — the run's failure goes on to the API's handler, which reports it
        /// and gives the person its reference.
        /// </summary>
        private static string FailedRunMessage(Exception failure, string calculation) =>
            failure is RefusalException refusal
                ? refusal.Sentence
                : $"The {calculation} did not complete; the failure was reported.";

        /// <summary>
        /// Writes a failed run's row to the calculation history before the run's own failure goes on to the caller.
        /// </summary>
        private async Task RecordFailedRunAsync(string calculation, Func<Task> store)
        {
            try
            {
                await store();
            }
            // Whatever stops the history row being written, the run's own failure must still reach the caller, which
            // rethrows it after this: the history row is reported missing, and nothing is answered in its place.
            catch (Exception storeFailure) when (storeFailure is not OperationCanceledException)
            {
                _failures.ReportHandled(storeFailure, $"recording a failed {calculation} run in the calculation history",
                    $"the failed run is missing from the {calculation} history; the run's own failure still reaches the caller",
                    FailureSeverity.Degraded);
            }
        }

        /// <summary>
        /// Saves a completed run's result to the calculation history. The run's result is still the caller's answer when
        /// saving fails: the failure is reported, and the result is returned unsaved.
        /// </summary>
        private async Task SaveCompletedRunAsync(string calculation, Func<Task> store)
        {
            try
            {
                await store();
            }
            // The calculation itself succeeded; whatever stops the save, the result is still returned, and the missing
            // history row is reported so the operator sees it.
            catch (Exception storeFailure) when (storeFailure is not OperationCanceledException)
            {
                _failures.ReportHandled(storeFailure, $"saving a completed {calculation} run in the calculation history",
                    $"the {calculation} result is returned to the caller but is not saved in the calculation history",
                    FailureSeverity.Degraded);
            }
        }
    }
}
