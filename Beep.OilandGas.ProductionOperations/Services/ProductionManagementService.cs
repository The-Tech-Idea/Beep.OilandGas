using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private IUnitOfWorkWrapper GetWellUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(WELL), _editor, _connectionName, "WELL", "UWI");
        }

        private IUnitOfWorkWrapper GetFacilityUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(FACILITY), _editor, _connectionName, "FACILITY", "FACILITY_ID");
        }

        public async Task<List<PDEN>> GetProductionOperationsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null)
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
            List<PDEN> pdenList = ConvertToList<PDEN>(units);

            if (startDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE >= startDate.Value).ToList();
            if (endDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE <= endDate.Value).ToList();

            return pdenList;
        }

        public async Task<PDEN?> GetProductionOperationAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return null;

            var pdenUow = GetPDENUnitOfWork();
            var pden = pdenUow.Read(operationId) as PDEN;
            if (pden == null || pden.ACTIVE_IND != "Y")
                return null;

            return pden;
        }

        public async Task<PDEN> CreateProductionOperationAsync(CreateProductionOperationRequest createRequest)
        {
            if (createRequest == null)
                throw new ArgumentNullException(nameof(createRequest));

            var pdenUow = GetPDENUnitOfWork();
            var pden = new PDEN
            {
                PDEN_ID = Guid.NewGuid().ToString(),
                PDEN_SUBTYPE = "PRODUCTION",
                ACTIVE_IND = "Y",
                CURRENT_STATUS_DATE = createRequest.OperationDate ?? DateTime.UtcNow,
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow
            };

            var result = await pdenUow.InsertDoc(pden);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create production operation: {result.Message}");

            await pdenUow.Commit();

            return pden;
        }

        public async Task<List<PDEN>> GetProductionReportsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null)
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
            List<PDEN> pdenList = ConvertToList<PDEN>(units);

            if (startDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE >= startDate.Value).ToList();
            if (endDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE <= endDate.Value).ToList();

            return pdenList;
        }

        public async Task<List<PDEN>> GetWellOperationsAsync(string wellUWI)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                return new List<PDEN>();

            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = wellUWI, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var units = await pdenUow.Get(filters);
            List<PDEN> pdenList = ConvertToList<PDEN>(units);

            return pdenList;
        }

        public async Task<List<FACILITY>> GetFacilityOperationsAsync(string facilityId)
        {
            if (string.IsNullOrWhiteSpace(facilityId))
                return new List<FACILITY>();

            var facilityUow = GetFacilityUnitOfWork();
            var facility = facilityUow.Read(facilityId) as FACILITY;
            if (facility == null)
                return new List<FACILITY>();

            return new List<FACILITY> { facility };
        }
    }
}

