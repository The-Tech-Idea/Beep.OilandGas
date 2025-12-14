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

namespace Beep.OilandGas.ProductionOperations.Services
{
    /// <summary>
    /// Service for managing production operations.
    /// Uses UnitOfWork directly for data access.
    /// </summary>
    public class ProductionManagementService : IProductionManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;

        public ProductionManagementService(IDMEEditor editor, string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _connectionName = connectionName;
        }

        private List<T> ConvertToList<T>(dynamic units) where T : class
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

        private IUnitOfWorkWrapper GetWellUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(WELL), _editor, _connectionName, "WELL", "UWI");
        }

        private IUnitOfWorkWrapper GetFacilityUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(FACILITY), _editor, _connectionName, "FACILITY", "FACILITY_ID");
        }

        public async Task<List<ProductionOperationDto>> GetProductionOperationsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                filters.Add(new AppFilter { FieldName = "UWI", FilterValue = wellUWI, Operator = "=" });
            }

            var units = await pdenUow.Get(filters);
            var pdenList = ConvertToList<PDEN>(units);

            if (startDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE >= startDate.Value).ToList();
            if (endDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE <= endDate.Value).ToList();

            var operations = new List<ProductionOperationDto>();
            var wellUow = GetWellUnitOfWork();

            foreach (var pden in pdenList)
            {
                var well = wellUow.Read(pden.UWI ?? string.Empty) as WELL;
                operations.Add(MapToProductionOperationDto(pden, well));
            }

            return operations;
        }

        public async Task<ProductionOperationDto?> GetProductionOperationAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return null;

            var pdenUow = GetPDENUnitOfWork();
            var pden = pdenUow.Read(operationId) as PDEN;
            if (pden == null || pden.ACTIVE_IND != "Y")
                return null;

            var wellUow = GetWellUnitOfWork();
            var well = wellUow.Read(pden.AREA_ID ?? string.Empty) as WELL;

            return MapToProductionOperationDto(pden, well);
        }

        public async Task<ProductionOperationDto> CreateProductionOperationAsync(CreateProductionOperationDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var pdenUow = GetPDENUnitOfWork();
            var pden = new PDEN
            {
                PDEN_ID = Guid.NewGuid().ToString(),
                PDEN_SUBTYPE = "PRODUCTION",
                ACTIVE_IND = "Y",
                CURRENT_STATUS_DATE = createDto.OperationDate,
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow
            };

            var result = await pdenUow.InsertDoc(pden);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create production operation: {result.Message}");

            await pdenUow.Commit();

            var wellUow = GetWellUnitOfWork();
            var well = wellUow.Read(createDto.WellUWI) as WELL;

            return MapToProductionOperationDto(pden, well);
        }

        public async Task<List<ProductionReportDto>> GetProductionReportsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                filters.Add(new AppFilter { FieldName = "UWI", FilterValue = wellUWI, Operator = "=" });
            }

            var units = await pdenUow.Get(filters);
            var pdenList = ConvertToList<PDEN>(units);

            if (startDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE >= startDate.Value).ToList();
            if (endDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE <= endDate.Value).ToList();

            return pdenList.Select(p => new ProductionReportDto
            {
                ReportId = p.PDEN_ID ?? string.Empty,
                WellUWI = p.UWI ?? string.Empty,
                ReportDate = p.CURRENT_STATUS_DATE,
                Status = p.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<List<WellOperationDto>> GetWellOperationsAsync(string wellUWI)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                return new List<WellOperationDto>();

            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = wellUWI, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var units = await pdenUow.Get(filters);
            var pdenList = ConvertToList<PDEN>(units);

            return pdenList.Select(p => new WellOperationDto
            {
                OperationId = p.PDEN_ID ?? string.Empty,
                WellUWI = wellUWI,
                OperationType = p.PDEN_SUBTYPE ?? "Production",
                OperationDate = p.CURRENT_STATUS_DATE,
                Status = p.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<List<FacilityOperationDto>> GetFacilityOperationsAsync(string facilityId)
        {
            if (string.IsNullOrWhiteSpace(facilityId))
                return new List<FacilityOperationDto>();

            var facilityUow = GetFacilityUnitOfWork();
            var facility = facilityUow.Read(facilityId) as FACILITY;
            if (facility == null)
                return new List<FacilityOperationDto>();

            return new List<FacilityOperationDto>
            {
                new FacilityOperationDto
                {
                    OperationId = facility.FACILITY_ID ?? string.Empty,
                    FacilityId = facilityId,
                    FacilityName = facility.FACILITY_SHORT_NAME ?? string.Empty,
                    OperationDate = facility.ACTIVE_DATE,
                    OperationType = facility.FACILITY_TYPE ?? string.Empty,
                    Status = facility.ACTIVE_IND == "Y" ? "Active" : "Inactive"
                }
            };
        }

        private ProductionOperationDto MapToProductionOperationDto(PDEN pden, WELL? well)
        {
            return new ProductionOperationDto
            {
                OperationId = pden.PDEN_ID ?? string.Empty,
                WellUWI = well?.UWI ?? string.Empty,
                WellName = well?.UWI ?? string.Empty,
                OperationDate = pden.CURRENT_STATUS_DATE,
                Status = pden.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            };
        }
    }
}

