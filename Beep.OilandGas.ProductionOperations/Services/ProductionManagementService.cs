using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionOperations.Services
{
    /// <summary>
    /// Service for managing production operations.
    /// Stores core PPDM production entities through PPDMGenericRepository.
    /// </summary>
    public partial class ProductionManagementService : IProductionManagementService
    {
        private const string PdenSubtypeFacility = "FACILITY";

        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly Func<CancellationToken, Task<string>>? _resolveConnection;

        public ProductionManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            Func<CancellationToken, Task<string>>? resolveConnection = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
            _resolveConnection = resolveConnection;
        }

        private async Task<PPDMGenericRepository> RepoAsync<T>(string tableName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var connection = _resolveConnection is null ? _connectionName : await _resolveConnection(cancellationToken);
            if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("A PPDM_CORE database binding is required.");
            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                typeof(T),
                connection,
                tableName,
                null);
        }

        public async Task<List<PDEN>> GetProductionOperationsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var repo = await RepoAsync<PDEN>("PDEN", cancellationToken);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                filters.Add(new AppFilter { FieldName = "CURRENT_WELL_STR_NUMBER", FilterValue = wellUWI, Operator = "=" });
            }

            var pdenList = (await repo.GetAsync(filters)).Cast<PDEN>().ToList();

            if (startDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE >= startDate.Value).ToList();
            if (endDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE <= endDate.Value).ToList();

            return pdenList;
        }

        public async Task<PDEN?> GetProductionOperationAsync(string operationId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(operationId))
                return null;

            var repo = await RepoAsync<PDEN>("PDEN", cancellationToken);
            var pden = await repo.GetByIdAsync(operationId) as PDEN;
            if (pden == null || pden.ACTIVE_IND != "Y")
                return null;

            return pden;
        }

        public async Task<PDEN> CreateProductionOperationAsync(CreateProductionOperationRequest createRequest, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (createRequest == null)
                throw new ArgumentNullException(nameof(createRequest));

            var repo = await RepoAsync<PDEN>("PDEN", cancellationToken);
            var operationDate = createRequest.OperationDate ?? DateTime.UtcNow;
            var pden = new PDEN
            {
                PDEN_ID = _defaults.FormatIdForTable("PDEN", Guid.NewGuid().ToString("N")),
                PDEN_SUBTYPE = string.IsNullOrWhiteSpace(createRequest.OperationType) ? "PRODUCTION" : createRequest.OperationType.Trim(),
                ACTIVE_IND = "Y",
                CURRENT_STATUS_DATE = operationDate,
                EFFECTIVE_DATE = operationDate,
                ON_PRODUCTION_DATE = operationDate,
                LAST_PRODUCTION_DATE = operationDate,
                PDEN_STATUS = string.IsNullOrWhiteSpace(createRequest.Status) ? "Planned" : createRequest.Status.Trim(),
                CURRENT_OPERATOR = string.IsNullOrWhiteSpace(createRequest.AssignedTo) ? string.Empty : createRequest.AssignedTo.Trim(),
                CURRENT_WELL_STR_NUMBER = string.IsNullOrWhiteSpace(createRequest.WellUWI) ? string.Empty : createRequest.WellUWI.Trim(),
                REMARK = string.IsNullOrWhiteSpace(createRequest.Remarks) ? string.Empty : createRequest.Remarks.Trim(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow
            };

            if (pden is IPPDMEntity entity)
                _commonColumnHandler.PrepareForInsert(entity, "SYSTEM");
            await repo.InsertAsync(pden, "SYSTEM");

            return pden;
        }

        public async Task<List<PDEN>> GetProductionReportsAsync(string? wellUWI = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var repo = await RepoAsync<PDEN>("PDEN", cancellationToken);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                filters.Add(new AppFilter { FieldName = "CURRENT_WELL_STR_NUMBER", FilterValue = wellUWI, Operator = "=" });
            }

            var pdenList = (await repo.GetAsync(filters)).Cast<PDEN>().ToList();

            if (startDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE >= startDate.Value).ToList();
            if (endDate.HasValue)
                pdenList = pdenList.Where(p => p.CURRENT_STATUS_DATE <= endDate.Value).ToList();

            return pdenList;
        }

        public async Task<List<PDEN>> GetWellOperationsAsync(string wellUWI, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(wellUWI))
                return new List<PDEN>();

            var repo = await RepoAsync<PDEN>("PDEN", cancellationToken);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CURRENT_WELL_STR_NUMBER", FilterValue = wellUWI, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var pdenList = (await repo.GetAsync(filters)).Cast<PDEN>().ToList();

            return pdenList;
        }

        public async Task<List<FACILITY>> GetFacilityOperationsAsync(string facilityId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(facilityId))
                return new List<FACILITY>();

            var repo = await RepoAsync<FACILITY>("FACILITY", cancellationToken);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FACILITY_ID", FilterValue = facilityId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var facilities = (await repo.GetAsync(filters)).Cast<FACILITY>().ToList();
            if (facilities.Count == 0)
                return new List<FACILITY>();

            return facilities;
        }
    }
}
