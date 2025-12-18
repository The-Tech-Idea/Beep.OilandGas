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

namespace Beep.OilandGas.Decommissioning.Services
{
    /// <summary>
    /// Service for managing well plugging operations.
    /// Uses UnitOfWork directly for data access.
    /// </summary>
    public class WellPluggingService : IWellPluggingService
    {
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;

        public WellPluggingService(IDMEEditor editor, string connectionName = "PPDM39")
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

        private IUnitOfWorkWrapper GetWellPlugbackUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(WELL_PLUGBACK), _editor, _connectionName, "WELL_PLUGBACK", "UWI");
        }

        private IUnitOfWorkWrapper GetWellUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(WELL), _editor, _connectionName, "WELL", "UWI");
        }

        private IUnitOfWorkWrapper GetFacilityUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(FACILITY), _editor, _connectionName, "FACILITY", "FACILITY_ID");
        }

        public async Task<List<WellPluggingDto>> GetWellPluggingOperationsAsync(string? wellUWI = null)
        {
            var plugbackUow = GetWellPlugbackUnitOfWork();
            List<WELL_PLUGBACK> plugbacks;

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", FilterValue = wellUWI, Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                var units = await plugbackUow.Get(filters);
                plugbacks = ConvertToList<WELL_PLUGBACK>(units);
            }
            else
            {
                var units = await plugbackUow.Get();
                List<WELL_PLUGBACK> allPlugbacks = ConvertToList<WELL_PLUGBACK>(units);
                plugbacks = allPlugbacks.Where(p => p.ACTIVE_IND == "Y").ToList();
            }

            var wellUow = GetWellUnitOfWork();
            var pluggings = new List<WellPluggingDto>();

            foreach (var plugback in plugbacks)
            {
                var well = wellUow.Read(plugback.UWI ?? string.Empty) as WELL;
                pluggings.Add(MapToWellPluggingDto(plugback, well));
            }

            return pluggings;
        }

        public async Task<WellPluggingDto?> GetWellPluggingOperationAsync(string pluggingId)
        {
            if (string.IsNullOrWhiteSpace(pluggingId))
                return null;

            var plugbackUow = GetWellPlugbackUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = pluggingId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var units = await plugbackUow.Get(filters);
            var plugbacks = ConvertToList<WELL_PLUGBACK>(units);
            var plugback = plugbacks.FirstOrDefault();

            if (plugback == null)
                return null;

            var wellUow = GetWellUnitOfWork();
            var well = wellUow.Read(pluggingId) as WELL;

            return MapToWellPluggingDto(plugback, well);
        }

        public async Task<WellPluggingDto> CreateWellPluggingOperationAsync(CreateWellPluggingDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var plugbackUow = GetWellPlugbackUnitOfWork();
            var plugback = new WELL_PLUGBACK
            {
                UWI = createDto.WellUWI,
                PLUGBACK_OBS_NO = 1,
                ACTIVE_IND = "Y",
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow
            };

            var result = await plugbackUow.InsertDoc(plugback);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create well plugging operation: {result.Message}");

            await plugbackUow.Commit();

            var wellUow = GetWellUnitOfWork();
            var well = wellUow.Read(createDto.WellUWI) as WELL;

            return MapToWellPluggingDto(plugback, well);
        }

        public async Task<WellPluggingDto> VerifyWellPluggingAsync(string pluggingId, string verifiedBy, bool passed)
        {
            if (string.IsNullOrWhiteSpace(pluggingId))
                throw new ArgumentException("Plugging ID cannot be null or empty.", nameof(pluggingId));

            var plugging = await GetWellPluggingOperationAsync(pluggingId);
            if (plugging == null)
                throw new KeyNotFoundException($"Well plugging operation with ID {pluggingId} not found.");

            plugging.VerifiedBy = verifiedBy;
            plugging.VerificationDate = DateTime.UtcNow;
            plugging.VerificationPassed = passed;
            plugging.Status = passed ? "Verified" : "Failed";

            return plugging;
        }

        public async Task<List<FacilityDecommissioningDto>> GetFacilityDecommissioningOperationsAsync(string? facilityId = null)
        {
            var facilityUow = GetFacilityUnitOfWork();
            List<FACILITY> facilities;

            if (!string.IsNullOrWhiteSpace(facilityId))
            {
                var facility = facilityUow.Read(facilityId) as FACILITY;
                facilities = facility != null && !string.IsNullOrEmpty(facility.ABANDONED_DATE.ToString()) 
                    ? new List<FACILITY> { facility } 
                    : new List<FACILITY>();
            }
            else
            {
                var units = await facilityUow.Get();
                List<FACILITY> allFacilities = ConvertToList<FACILITY>(units);
                facilities = allFacilities.Where(f => !string.IsNullOrEmpty(f.ABANDONED_DATE.ToString())).ToList();
            }

            return facilities.Select(f => new FacilityDecommissioningDto
            {
                DecommissioningId = f.FACILITY_ID ?? string.Empty,
                FacilityId = f.FACILITY_ID ?? string.Empty,
                FacilityName = f.FACILITY_SHORT_NAME ?? string.Empty,
                CompletionDate = f.ABANDONED_DATE,
                Status = f.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<List<SiteRestorationDto>> GetSiteRestorationOperationsAsync(string? siteId = null)
        {
            // Note: Site restoration might be stored in FACILITY or a separate entity
            // For now, returning empty list as placeholder
            await Task.CompletedTask;
            return new List<SiteRestorationDto>();
        }

        public async Task<List<AbandonmentDto>> GetAbandonmentOperationsAsync(string? wellUWI = null)
        {
            var wellUow = GetWellUnitOfWork();
            List<WELL> wells;

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                var well = wellUow.Read(wellUWI) as WELL;
                wells = well != null && !string.IsNullOrEmpty(well.ABANDONMENT_DATE.ToString())
                    ? new List<WELL> { well }
                    : new List<WELL>();
            }
            else
            {
                var units = await wellUow.Get();
                List<WELL> allWells = ConvertToList<WELL>(units);
                wells = allWells.Where(w => !string.IsNullOrEmpty(w.ABANDONMENT_DATE.ToString())).ToList();
            }

            return wells.Select(w => new AbandonmentDto
            {
                AbandonmentId = w.UWI ?? string.Empty,
                WellUWI = w.UWI ?? string.Empty,
                WellName = w.UWI ?? string.Empty,
                AbandonmentDate = w.ABANDONMENT_DATE,
                Status = w.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        private WellPluggingDto MapToWellPluggingDto(WELL_PLUGBACK plugback, WELL? well)
        {
            return new WellPluggingDto
            {
                PluggingId = plugback.UWI ?? string.Empty,
                WellUWI = plugback.UWI ?? string.Empty,
                WellName = well?.UWI ?? string.Empty,
                PlugDepth = plugback.BASE_DEPTH,
                Status = plugback.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            };
        }
    }
}

