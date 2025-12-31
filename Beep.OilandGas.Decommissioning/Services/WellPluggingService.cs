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
using Beep.OilandGas.Models.Data;

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

        public async Task<List<WELL_PLUGBACK>> GetWellPluggingOperationsAsync(string? wellUWI = null)
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

            return plugbacks;
        }

        public async Task<WELL_PLUGBACK?> GetWellPluggingOperationAsync(string pluggingId)
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

            return plugback;
        }

        public async Task<WELL_PLUGBACK> CreateWellPluggingOperationAsync(CreateWellPluggingRequest createRequest)
        {
            if (createRequest == null)
                throw new ArgumentNullException(nameof(createRequest));

            var plugbackUow = GetWellPlugbackUnitOfWork();
            var plugback = new WELL_PLUGBACK
            {
                UWI = createRequest.WellUWI,
                PLUGBACK_OBS_NO = 1,
                ACTIVE_IND = "Y",
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow
            };

            var result = await plugbackUow.InsertDoc(plugback);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create well plugging operation: {result.Message}");

            await plugbackUow.Commit();

            return plugback;
        }

        public async Task<WELL_PLUGBACK> VerifyWellPluggingAsync(string pluggingId, string verifiedBy, bool passed)
        {
            if (string.IsNullOrWhiteSpace(pluggingId))
                throw new ArgumentException("Plugging ID cannot be null or empty.", nameof(pluggingId));

            var plugging = await GetWellPluggingOperationAsync(pluggingId);
            if (plugging == null)
                throw new KeyNotFoundException($"Well plugging operation with ID {pluggingId} not found.");

            // Note: WELL_PLUGBACK entity doesn't have verification fields
            // This would need to be stored in a separate table or added to the entity
            // For now, just return the plugging entity
            return plugging;
        }

        public async Task<List<FacilityDecommissioningResponse>> GetFacilityDecommissioningOperationsAsync(string? facilityId = null)
        {
            var facilityUow = GetFacilityUnitOfWork();
            List<FACILITY> facilities;

            if (!string.IsNullOrWhiteSpace(facilityId))
            {
                var facility = facilityUow.Read(facilityId) as FACILITY;
                facilities = facility != null && facility.ABANDONED_DATE != default(DateTime) 
                    ? new List<FACILITY> { facility } 
                    : new List<FACILITY>();
            }
            else
            {
                var units = await facilityUow.Get();
                List<FACILITY> allFacilities = ConvertToList<FACILITY>(units);
                facilities = allFacilities.Where(f => f.ABANDONED_DATE != default(DateTime)).ToList();
            }

            return facilities.Select(f => new FacilityDecommissioningResponse
            {
                DecommissioningId = f.FACILITY_ID ?? string.Empty,
                FacilityId = f.FACILITY_ID ?? string.Empty,
                DecommissioningEndDate = f.ABANDONED_DATE,
                Status = f.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<List<EnvironmentalRestorationResponse>> GetSiteRestorationOperationsAsync(string? siteId = null)
        {
            // Note: Site restoration might be stored in FACILITY or a separate entity
            // For now, returning empty list as placeholder
            await Task.CompletedTask;
            return new List<EnvironmentalRestorationResponse>();
        }

        public async Task<List<WellAbandonmentResponse>> GetAbandonmentOperationsAsync(string? wellUWI = null)
        {
            var wellUow = GetWellUnitOfWork();
            List<WELL> wells;

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                var well = wellUow.Read(wellUWI) as WELL;
                wells = well != null && well.ABANDONMENT_DATE != default(DateTime)
                    ? new List<WELL> { well }
                    : new List<WELL>();
            }
            else
            {
                var units = await wellUow.Get();
                List<WELL> allWells = ConvertToList<WELL>(units);
                wells = allWells.Where(w => w.ABANDONMENT_DATE != default(DateTime)).ToList();
            }

            return wells.Select(w => new WellAbandonmentResponse
            {
                AbandonmentId = w.UWI ?? string.Empty,
                WellId = w.UWI ?? string.Empty,
                AbandonmentEndDate = w.ABANDONMENT_DATE,
                Status = w.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }
    }
}

