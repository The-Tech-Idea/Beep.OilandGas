using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.EnhancedRecovery.Services
{
    /// <summary>
    /// Service for managing enhanced recovery operations.
    /// Uses UnitOfWork directly for data access.
    /// </summary>
    public partial class EnhancedRecoveryService : IEnhancedRecoveryService
    {
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;

        public EnhancedRecoveryService(IDMEEditor editor, string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _connectionName = connectionName;
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

        public async Task<List<EnhancedRecoveryOperationDto>> GetEnhancedRecoveryOperationsAsync(string? fieldId = null)
        {
            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PDEN_SUBTYPE", FilterValue = "EOR", Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
            {
                filters.Add(new AppFilter { FieldName = "AREA_ID", FilterValue = fieldId, Operator = "=" });
            }

            var units = await pdenUow.Get(filters);
            List<PDEN> pdenList = ConvertToList<PDEN>(units);

            return pdenList.Select(p => new EnhancedRecoveryOperationDto
            {
                OperationId = p.PDEN_ID ?? string.Empty,
                FieldId = p.AREA_ID ?? string.Empty,
                EORType = p.PDEN_SUBTYPE ?? "EOR",
                StartDate = p.CURRENT_STATUS_DATE,
                Status = p.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<EnhancedRecoveryOperationDto?> GetEnhancedRecoveryOperationAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return null;

            var pdenUow = GetPDENUnitOfWork();
            var pden = pdenUow.Read(operationId) as PDEN;
            if (pden == null || pden.ACTIVE_IND != "Y")
                return null;

            return new EnhancedRecoveryOperationDto
            {
                OperationId = pden.PDEN_ID ?? string.Empty,
                FieldId = pden.AREA_ID ?? string.Empty,
                EORType = pden.PDEN_SUBTYPE ?? "EOR",
                StartDate = pden.CURRENT_STATUS_DATE,
                Status = pden.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            };
        }

        public async Task<EnhancedRecoveryOperationDto> CreateEnhancedRecoveryOperationAsync(CreateEnhancedRecoveryOperationDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var pdenUow = GetPDENUnitOfWork();
            var pden = new PDEN
            {
                PDEN_ID = Guid.NewGuid().ToString(),
                PDEN_SUBTYPE = createDto.EORType,
                ACTIVE_IND = "Y",
                AREA_ID = createDto.FieldId,
                CURRENT_STATUS_DATE = createDto.PlannedStartDate ?? DateTime.UtcNow,
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow
            };

            var result = await pdenUow.InsertDoc(pden);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create enhanced recovery operation: {result.Message}");

            await pdenUow.Commit();

            return new EnhancedRecoveryOperationDto
            {
                OperationId = pden.PDEN_ID ?? string.Empty,
                FieldId = createDto.FieldId,
                EORType = createDto.EORType,
                StartDate = createDto.PlannedStartDate,
                Status = "Planned",
                InjectionRate = createDto.PlannedInjectionRate,
                InjectionRateUnit = createDto.InjectionRateUnit
            };
        }

        public async Task<List<InjectionOperationDto>> GetInjectionOperationsAsync(string? wellUWI = null)
        {
            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PDEN_SUBTYPE", FilterValue = "INJECTION", Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                filters.Add(new AppFilter { FieldName = "UWI", FilterValue = wellUWI, Operator = "=" });
            }

            var units = await pdenUow.Get(filters);
            List<PDEN> pdenList = ConvertToList<PDEN>(units);

            return pdenList.Select(p => new InjectionOperationDto
            {
                OperationId = p.PDEN_ID ?? string.Empty,
                WellUWI = string.Empty, // PDEN doesn't have UWI - injection operations may be field-level
                InjectionType = p.PDEN_SUBTYPE ?? "INJECTION",
                OperationDate = p.CURRENT_STATUS_DATE.Value,
                Status = p.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<List<WaterFloodingDto>> GetWaterFloodingOperationsAsync(string? fieldId = null)
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

            return pdenList.Select(p => new WaterFloodingDto
            {
                OperationId = p.PDEN_ID ?? string.Empty,
                FieldId = p.AREA_ID ?? string.Empty,
                StartDate = p.CURRENT_STATUS_DATE,
                Status = p.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<List<GasInjectionDto>> GetGasInjectionOperationsAsync(string? fieldId = null)
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

            return pdenList.Select(p => new GasInjectionDto
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

