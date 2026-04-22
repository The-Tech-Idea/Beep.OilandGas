using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.ConfigUtil;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.EnhancedRecovery.Services
{
    /// <summary>
    /// Service for managing enhanced recovery operations.
    /// Uses UnitOfWork directly for data access.
    /// </summary>
    public partial class EnhancedRecoveryService : IEnhancedRecoveryService, Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService
    {
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<EnhancedRecoveryService> _logger;

        public EnhancedRecoveryService(IDMEEditor editor, string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _connectionName = connectionName;
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<EnhancedRecoveryService>.Instance;
        }

        private List<T> ConvertToList<T>(object units) where T : class
        {
            var result = new List<T>();
            if (units == null) return result;
            
            if (units is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is T entity)
                    {
                        result.Add(entity);
                    }
                }
            }
            return result;
        }

        private IUnitOfWorkWrapper GetPDENUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(PDEN), _editor, _connectionName, "PDEN", "PDEN_ID");
        }

        private IUnitOfWorkWrapper GetFieldUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(FIELD), _editor, _connectionName, "FIELD", "FIELD_ID");
        }

        private IUnitOfWorkWrapper GetWellUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(WELL), _editor, _connectionName, "WELL", "UWI");
        }

        private IUnitOfWorkWrapper GetPDENFlowMeasurementUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(
                typeof(PDEN_FLOW_MEASUREMENT),
                _editor,
                _connectionName,
                "PDEN_FLOW_MEASUREMENT",
                "PDEN_ID,PDEN_SUBTYPE,PDEN_SOURCE,PRODUCT_TYPE,AMENDMENT_SEQ_NO,PERIOD_TYPE,MEASUREMENT_OBS_NO");
        }

        private static string ResolveEnhancedRecoveryType(PDEN pden)
        {
            return !string.IsNullOrWhiteSpace(pden.ENHANCED_RECOVERY_TYPE)
                ? pden.ENHANCED_RECOVERY_TYPE
                : pden.PDEN_SUBTYPE ?? string.Empty;
        }

        private static bool IsInjectionOperation(PDEN pden)
        {
            return string.Equals(ResolveEnhancedRecoveryType(pden), "INJECTION", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEnhancedRecoveryOperation(PDEN pden)
        {
            var recoveryType = ResolveEnhancedRecoveryType(pden);
            return !string.IsNullOrWhiteSpace(recoveryType)
                && !string.Equals(recoveryType, "INJECTION", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveOperationStatus(PDEN pden)
        {
            if (!string.IsNullOrWhiteSpace(pden.PDEN_STATUS))
                return pden.PDEN_STATUS;

            return string.Equals(pden.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase)
                ? "Active"
                : "Inactive";
        }

        private async Task<List<PDEN_FLOW_MEASUREMENT>> GetFlowMeasurementsAsync(PDEN pden, bool activeOnly = true)
        {
            var measurementUow = GetPDENFlowMeasurementUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PDEN_ID", FilterValue = pden.PDEN_ID ?? string.Empty, Operator = "=" }
            };

            if (activeOnly)
            {
                filters.Add(new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" });
            }

            var units = await measurementUow.Get(filters);
            List<PDEN_FLOW_MEASUREMENT> measurements = ConvertToList<PDEN_FLOW_MEASUREMENT>(units);

            return measurements
                .Where(m => string.Equals(m.PDEN_SUBTYPE, pden.PDEN_SUBTYPE, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(m => m.MEASUREMENT_DATE ?? m.ROW_CHANGED_DATE ?? m.ROW_CREATED_DATE ?? DateTime.MinValue)
                .ThenByDescending(m => m.MEASUREMENT_OBS_NO)
                .ToList();
        }

        private async Task<PDEN_FLOW_MEASUREMENT?> GetLatestFlowMeasurementAsync(PDEN pden)
        {
            var measurements = await GetFlowMeasurementsAsync(pden);

            return measurements
                .Where(m => string.IsNullOrWhiteSpace(m.MEASUREMENT_TYPE)
                    || string.Equals(m.MEASUREMENT_TYPE, "INJECTION_RATE", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault()
                ?? measurements.FirstOrDefault();
        }

        private async Task UpsertFlowMeasurementAsync(PDEN pden, decimal flowRate, string? flowRateUnit, string productType = "WATER")
        {
            if (string.IsNullOrWhiteSpace(pden.PDEN_ID))
                throw new InvalidOperationException("Cannot persist a flow measurement without a PDEN ID.");

            var measurementUow = GetPDENFlowMeasurementUnitOfWork();
            var now = DateTime.UtcNow;
            var normalizedUnit = string.IsNullOrWhiteSpace(flowRateUnit) ? "BBL/D" : flowRateUnit;
            var existingMeasurements = await GetFlowMeasurementsAsync(pden, activeOnly: false);
            var existing = existingMeasurements
                .Where(m => string.IsNullOrWhiteSpace(m.MEASUREMENT_TYPE)
                    || string.Equals(m.MEASUREMENT_TYPE, "INJECTION_RATE", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (existing != null)
            {
                existing.ACTIVE_IND = "Y";
                existing.MEASUREMENT_DATE = now;
                existing.MEASUREMENT_DATE_DESC = "RECORDED";
                existing.MEASUREMENT_TYPE = "INJECTION_RATE";
                existing.FLOW_RATE = flowRate;
                existing.FLOW_RATE_OUOM = normalizedUnit;
                existing.PRODUCT_TYPE = string.IsNullOrWhiteSpace(existing.PRODUCT_TYPE) ? productType : existing.PRODUCT_TYPE;
                existing.ROW_CHANGED_BY = "SYSTEM";
                existing.ROW_CHANGED_DATE = now;

                var updateResult = await measurementUow.UpdateDoc(existing);
                if (updateResult.Flag != Errors.Ok)
                    throw new InvalidOperationException($"Failed to update injection flow measurement: {updateResult.Message}");

                await measurementUow.Commit();
                return;
            }

            var measurement = new PDEN_FLOW_MEASUREMENT
            {
                PDEN_ID = pden.PDEN_ID ?? string.Empty,
                PDEN_SUBTYPE = pden.PDEN_SUBTYPE ?? string.Empty,
                PDEN_SOURCE = string.IsNullOrWhiteSpace(pden.SOURCE) ? "ENHANCED_RECOVERY" : pden.SOURCE,
                PRODUCT_TYPE = productType,
                AMENDMENT_SEQ_NO = 0,
                PERIOD_TYPE = "INSTANT",
                MEASUREMENT_OBS_NO = existingMeasurements.Count == 0
                    ? 1
                    : existingMeasurements.Max(m => m.MEASUREMENT_OBS_NO) + 1,
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = now,
                MEASUREMENT_DATE = now,
                MEASUREMENT_DATE_DESC = "RECORDED",
                MEASUREMENT_TYPE = "INJECTION_RATE",
                FLOW_RATE = flowRate,
                FLOW_RATE_OUOM = normalizedUnit,
                ROW_CREATED_BY = "SYSTEM",
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = "SYSTEM",
                ROW_CHANGED_DATE = now,
                ROW_EFFECTIVE_DATE = now,
                ROW_QUALITY = "GOOD"
            };

            var insertResult = await measurementUow.InsertDoc(measurement);
            if (insertResult.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create injection flow measurement: {insertResult.Message}");

            await measurementUow.Commit();
        }

        private async Task<EnhancedRecoveryOperation> MapEnhancedRecoveryOperationAsync(PDEN pden)
        {
            var latestMeasurement = await GetLatestFlowMeasurementAsync(pden);

            return new EnhancedRecoveryOperation
            {
                OperationId = pden.PDEN_ID ?? string.Empty,
                FieldId = pden.FIELD_ID ?? pden.AREA_ID ?? string.Empty,
                EORType = ResolveEnhancedRecoveryType(pden),
                StartDate = pden.EFFECTIVE_DATE ?? pden.CURRENT_STATUS_DATE,
                Status = ResolveOperationStatus(pden),
                InjectionRate = latestMeasurement?.FLOW_RATE,
                InjectionRateUnit = latestMeasurement?.FLOW_RATE_OUOM,
                Remarks = pden.REMARK
            };
        }

        private async Task<InjectionOperation> MapInjectionOperationAsync(PDEN pden)
        {
            var latestMeasurement = await GetLatestFlowMeasurementAsync(pden);

            return new InjectionOperation
            {
                OperationId = pden.PDEN_ID ?? string.Empty,
                WellUWI = pden.CURRENT_WELL_STR_NUMBER ?? string.Empty,
                InjectionType = ResolveEnhancedRecoveryType(pden),
                OperationDate = pden.LAST_INJECTION_DATE
                    ?? pden.ON_INJECTION_DATE
                    ?? pden.CURRENT_STATUS_DATE
                    ?? pden.EFFECTIVE_DATE
                    ?? DateTime.UtcNow,
                InjectionRate = latestMeasurement?.FLOW_RATE,
                InjectionRateUnit = latestMeasurement?.FLOW_RATE_OUOM,
                Status = ResolveOperationStatus(pden),
                Remarks = pden.REMARK
            };
        }

        private async Task<PDEN> CreateInjectionOperationAsync(string injectionWellId, decimal injectionRate)
        {
            var wellUow = GetWellUnitOfWork();
            var well = wellUow.Read(injectionWellId) as WELL;
            if (well == null)
                throw new InvalidOperationException($"Injection well {injectionWellId} was not found.");

            var now = DateTime.UtcNow;
            var fieldId = well.ASSIGNED_FIELD ?? string.Empty;
            var pdenUow = GetPDENUnitOfWork();
            var pden = new PDEN
            {
                PDEN_ID = Guid.NewGuid().ToString(),
                PDEN_SUBTYPE = "INJECTION",
                ENHANCED_RECOVERY_TYPE = "INJECTION",
                SOURCE = "ENHANCED_RECOVERY",
                ACTIVE_IND = "Y",
                AREA_ID = fieldId,
                AREA_TYPE = string.IsNullOrWhiteSpace(fieldId) ? string.Empty : "FIELD",
                FIELD_ID = fieldId,
                CURRENT_WELL_STR_NUMBER = well.UWI ?? injectionWellId,
                CURRENT_STATUS_DATE = now,
                EFFECTIVE_DATE = now,
                ON_INJECTION_DATE = now,
                LAST_INJECTION_DATE = now,
                PDEN_STATUS = "ACTIVE",
                PDEN_STATUS_TYPE = "STATUS",
                PDEN_SHORT_NAME = $"Injection {injectionWellId}",
                PDEN_LONG_NAME = $"Injection operation for {injectionWellId}",
                REMARK = $"Managed injection for {injectionWellId}",
                ROW_CREATED_BY = "SYSTEM",
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = "SYSTEM",
                ROW_CHANGED_DATE = now,
                ROW_EFFECTIVE_DATE = now,
                ROW_QUALITY = "GOOD"
            };

            var insertResult = await pdenUow.InsertDoc(pden);
            if (insertResult.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create injection operation: {insertResult.Message}");

            await pdenUow.Commit();
            await UpsertFlowMeasurementAsync(pden, injectionRate, "BBL/D");

            return pden;
        }

        public async Task<List<EnhancedRecoveryOperation>> GetEnhancedRecoveryOperationsAsync(string? fieldId = null)
        {
            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
            {
                filters.Add(new AppFilter { FieldName = "AREA_ID", FilterValue = fieldId, Operator = "=" });
            }

            var units = await pdenUow.Get(filters);
            List<PDEN> pdenList = ConvertToList<PDEN>(units);
            var operations = new List<EnhancedRecoveryOperation>();

            foreach (var pden in pdenList.Where(IsEnhancedRecoveryOperation).OrderByDescending(p => p.CURRENT_STATUS_DATE ?? p.EFFECTIVE_DATE ?? DateTime.MinValue))
            {
                operations.Add(await MapEnhancedRecoveryOperationAsync(pden));
            }

            return operations;
        }

        public async Task<EnhancedRecoveryOperation?> GetEnhancedRecoveryOperationAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return null;

            var pdenUow = GetPDENUnitOfWork();
            var pden = pdenUow.Read(operationId) as PDEN;
            if (pden == null || pden.ACTIVE_IND != "Y" || !IsEnhancedRecoveryOperation(pden))
                return null;

            return await MapEnhancedRecoveryOperationAsync(pden);
        }

        public async Task<EnhancedRecoveryOperation> CreateEnhancedRecoveryOperationAsync(CreateEnhancedRecoveryOperation createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var now = DateTime.UtcNow;
            var recoveryType = string.IsNullOrWhiteSpace(createDto.EORType) ? "EOR" : createDto.EORType;
            var startDate = createDto.PlannedStartDate ?? now;
            var pdenUow = GetPDENUnitOfWork();
            var pden = new PDEN
            {
                PDEN_ID = Guid.NewGuid().ToString(),
                PDEN_SUBTYPE = recoveryType,
                ENHANCED_RECOVERY_TYPE = recoveryType,
                SOURCE = "ENHANCED_RECOVERY",
                ACTIVE_IND = "Y",
                AREA_ID = createDto.FieldId,
                AREA_TYPE = string.IsNullOrWhiteSpace(createDto.FieldId) ? string.Empty : "FIELD",
                FIELD_ID = createDto.FieldId,
                CURRENT_STATUS_DATE = startDate,
                EFFECTIVE_DATE = startDate,
                PDEN_STATUS = "ACTIVE",
                PDEN_STATUS_TYPE = "STATUS",
                PDEN_SHORT_NAME = $"{recoveryType} {createDto.FieldId}".Trim(),
                PDEN_LONG_NAME = $"{recoveryType} operation {createDto.FieldId}".Trim(),
                ROW_CREATED_BY = "SYSTEM",
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = "SYSTEM",
                ROW_CHANGED_DATE = now,
                ROW_EFFECTIVE_DATE = now,
                ROW_QUALITY = "GOOD"
            };

            var result = await pdenUow.InsertDoc(pden);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create enhanced recovery operation: {result.Message}");

            await pdenUow.Commit();

            if (string.Equals(recoveryType, "INJECTION", StringComparison.OrdinalIgnoreCase)
                && createDto.PlannedInjectionRate.HasValue)
            {
                pden.ON_INJECTION_DATE = startDate;
                pden.LAST_INJECTION_DATE = startDate;
                var updateResult = await pdenUow.UpdateDoc(pden);
                if (updateResult.Flag != Errors.Ok)
                    throw new InvalidOperationException($"Failed to update injection operation dates: {updateResult.Message}");

                await pdenUow.Commit();
                await UpsertFlowMeasurementAsync(pden, createDto.PlannedInjectionRate.Value, createDto.InjectionRateUnit);
            }

            return await MapEnhancedRecoveryOperationAsync(pden);
        }

        public async Task<List<InjectionOperation>> GetInjectionOperationsAsync(string? wellUWI = null)
        {
            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                filters.Add(new AppFilter { FieldName = "CURRENT_WELL_STR_NUMBER", FilterValue = wellUWI, Operator = "=" });
            }

            var units = await pdenUow.Get(filters);
            List<PDEN> pdenList = ConvertToList<PDEN>(units);
            var operations = new List<InjectionOperation>();

            foreach (var pden in pdenList.Where(IsInjectionOperation).OrderByDescending(p => p.LAST_INJECTION_DATE ?? p.CURRENT_STATUS_DATE ?? DateTime.MinValue))
            {
                operations.Add(await MapInjectionOperationAsync(pden));
            }

            return operations;
        }

        public async Task<List<WaterFlooding>> GetWaterFloodingOperationsAsync(string? fieldId = null)
        {
            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PDEN_SUBTYPE", FilterValue = "WATER_FLOOD", Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
            {
                filters.Add(new AppFilter { FieldName = "AREA_ID", FilterValue = fieldId, Operator = "=" });
            }

            var units = await pdenUow.Get(filters);
            List<PDEN> pdenList = ConvertToList<PDEN>(units);

            return pdenList.Select(p => new WaterFlooding
            {
                OperationId = p.PDEN_ID ?? string.Empty,
                FieldId = p.AREA_ID ?? string.Empty,
                StartDate = p.CURRENT_STATUS_DATE,
                Status = p.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<List<GasInjection>> GetGasInjectionOperationsAsync(string? fieldId = null)
        {
            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PDEN_SUBTYPE", FilterValue = "GAS_INJECTION", Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
            {
                filters.Add(new AppFilter { FieldName = "AREA_ID", FilterValue = fieldId, Operator = "=" });
            }

            var units = await pdenUow.Get(filters);
            List<PDEN> pdenList = ConvertToList<PDEN>(units);

            return pdenList.Select(p => new GasInjection
            {
                OperationId = p.PDEN_ID ?? string.Empty,
                FieldId = p.AREA_ID ?? string.Empty,
                GasType = p.PDEN_SUBTYPE ?? "GAS_INJECTION",
                StartDate = p.CURRENT_STATUS_DATE,
                Status = p.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }
    }
}

