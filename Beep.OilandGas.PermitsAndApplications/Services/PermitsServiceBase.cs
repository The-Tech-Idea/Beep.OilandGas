using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.PPDM39.Core;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PermitsAndApplications.Services
{
    public abstract class PermitsServiceBase
    {
        protected readonly IDMEEditor Editor;
        protected readonly ICommonColumnHandler CommonColumnHandler;
        protected readonly IPPDM39DefaultsRepository Defaults;
        protected readonly IPPDMMetadataRepository Metadata;
        protected readonly ILogger? Logger;
        protected readonly ICompanyPermitApplicationDataAccess DataAccess;

        protected PermitsServiceBase(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ICompanyPermitApplicationDataAccess dataAccess,
            ILogger? logger)
        {
            Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            CommonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            Defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            Logger = logger;
            DataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
        }

        protected async Task<PPDMGenericRepository> CreateRepositoryAsync<T>(string tableName, string? applicationId = null)
        {
            var scope = await DataAccess.RequireReadScopeAsync(); scope.Require();
            if (typeof(T).Name != tableName) throw new InvalidOperationException("An exact native permit entity/table is required.");
            return new PPDMGenericRepository(Editor, CommonColumnHandler, Defaults, Metadata, typeof(T),
                scope.Database.ConnectionName, tableName, rowAccess: DataAccess.ForRead(typeof(T), tableName, applicationId));
        }

        protected void SetAuditFields(dynamic entity, string userId)
        {
            if (string.IsNullOrEmpty(entity.ROW_CREATED_BY))
            {
                entity.ROW_CREATED_BY = userId;
                entity.ROW_CREATED_DATE = DateTime.UtcNow;
            }

            entity.ROW_CHANGED_BY = userId;
            entity.ROW_CHANGED_DATE = DateTime.UtcNow;
        }
    }
}
