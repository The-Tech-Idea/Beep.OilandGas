using TheTechIdea.Data.OilGas;
using Beep.OilandGas.PermitsAndApplications.Exceptions;
using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PermitsAndApplications.Services
{
    /// <summary>
    /// Domain-level permit application service.
    /// </summary>
    public partial class PermitApplicationLifecycleService : PermitsServiceBase, IPermitApplicationLifecycleService
    {
        private readonly ILogger<PermitApplicationLifecycleService>? _logger;

        public PermitApplicationLifecycleService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ICompanyPermitApplicationDataAccess dataAccess,
            ILogger<PermitApplicationLifecycleService>? logger = null)
            : base(editor, commonColumnHandler, defaults, metadata, dataAccess, logger)
        {
            _logger = logger;
        }

        public async Task<PERMIT_APPLICATION?> GetByIdAsync(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new InvalidApplicationException("The permit application ID is required.");

            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var result = await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
            return result;
        }

        public async Task<PermitApplicationSourceReview> ReviewSourceAsync(string applicationId)
        {
            var scope = await DataAccess.RequireReadScopeAsync();
            async Task<PermitApplicationSourceReview> Read()
            {
                var application = await GetByIdAsync(applicationId) ?? throw new PermitNotFoundException("The reviewed permit is unavailable.", applicationId);
                var repository = await CreateRepositoryAsync<PERMIT_STATUS_HISTORY>("PERMIT_STATUS_HISTORY", applicationId);
                var history = (await repository.GetAsync(new List<AppFilter>())).ToArray();
                if (history.Any(row => row is not PERMIT_STATUS_HISTORY)) throw new InvalidOperationException("The recorded permit history has an unexpected native type.");
                return PermitApplicationSourceReview.Capture(scope, application, history.Cast<PERMIT_STATUS_HISTORY>().ToArray(), await ReadSupportingAsync(application));
            }
            var original = await Read();
            original.RequireCurrent(await Read());
            if (scope != await DataAccess.RequireReadScopeAsync()) throw new InvalidOperationException("The permit review authority changed.");
            return original;
        }

        private async Task<PermitApplicationSupportingSnapshot> ReadSupportingAsync(PERMIT_APPLICATION parent)
        {
            var scope=await DataAccess.RequireReadScopeAsync();var source=OpenReviewedSource(scope);
            async Task<T?> Read<T>() where T : class
            {
                var type=typeof(T);var table=type.Name;
                var metadata=await Metadata.GetTableMetadataAsync(table);
                if(metadata is null || metadata.EntityTypeName!=type.FullName || metadata.PrimaryKeyColumn!="PERMIT_APPLICATION_ID")
                    throw new NativeSchemaCompatibilityException("Exact canonical supporting metadata is required before reviewing a permit.");
                var shape=source.GetEntityStructure(table,true);
                if(shape?.Fields is null || !shape.Fields.Where(x=>x.IsKey).Select(x=>x.FieldName).SequenceEqual(["PERMIT_APPLICATION_ID"]) ||
                    type.GetProperties().Where(x=>x.CanRead && x.CanWrite).Any(property=>
                        shape.Fields.Count(column=>column.FieldName==property.Name && column.Fieldtype==(Nullable.GetUnderlyingType(property.PropertyType)??property.PropertyType).FullName)!=1))
                    throw new NativeSchemaCompatibilityException("Install every canonical supporting column/key using Beep Migration before reviewing a permit; unavailable evidence cannot mean absent.");
                var repository = await CreateRepositoryAsync<T>(table,parent.PERMIT_APPLICATION_ID);
                var row=await repository.GetByIdAsync(parent.PERMIT_APPLICATION_ID);
                if(row is not null && row is not T || source.ErrorObject?.Flag==TheTechIdea.Beep.ConfigUtil.Errors.Failed ||
                    !ReferenceEquals(source,Editor.GetDataSource(scope.Database.ConnectionName)) || await DataAccess.RequireReadScopeAsync()!=scope)
                    throw new InvalidOperationException("The supporting permit read failed or its current source changed; absence is unconfirmed.");
                return row as T;
            }
            return PermitApplicationSupportingSnapshot.Capture(parent,await Read<DRILLING_PERMIT_APPLICATION>(),
                await Read<ENVIRONMENTAL_PERMIT_APPLICATION>(),await Read<INJECTION_PERMIT_APPLICATION>());
        }

        public async Task<IReadOnlyList<PERMIT_APPLICATION>> GetApplicationsAsync()
        {
            var scope=await DataAccess.RequireReadScopeAsync();
            var source=OpenReviewedSource(scope);
            if(source.ErrorObject?.Flag==TheTechIdea.Beep.ConfigUtil.Errors.Failed)throw new InvalidOperationException("The permit population provider reported a failure.");
            var shape=source.GetEntityStructure(new TheTechIdea.Beep.DataBase.EntityStructure{EntityName=nameof(PERMIT_APPLICATION),DatasourceEntityName=nameof(PERMIT_APPLICATION)},true);
            var required=new[]{"PERMIT_APPLICATION_ID","OWNER_TENANT_ID","OWNER_COMPANY_ID","FIELD_ID","ACTIVE_IND","APPLICATION_TYPE","REGULATORY_AUTHORITY","COUNTRY","STATE_PROVINCE","APPLICANT_ID","STATUS","CREATED_DATE","SUBMITTED_BY_USER_ID","SUBMITTED_DATE","DECISION_DATE","DECISION"};
            if(shape?.Fields is null || required.Any(name=>shape.Fields.Count(column=>column.FieldName==name)!=1))
                throw new NativeSchemaCompatibilityException("Install the canonical permit population columns using Beep Migration before reading role work queues.");
            var repository=await CreateRepositoryAsync<PERMIT_APPLICATION>(nameof(PERMIT_APPLICATION));
            var rows=(await repository.GetAsync(new List<AppFilter>())).ToArray();
            if(!ReferenceEquals(source,Editor.GetDataSource(scope.Database.ConnectionName)) || source.ErrorObject?.Flag==TheTechIdea.Beep.ConfigUtil.Errors.Failed || await DataAccess.RequireReadScopeAsync()!=scope)
                throw new InvalidOperationException("The permit population provider failed or its reviewed source changed.");
            if(rows.Any(row=>row is not PERMIT_APPLICATION))throw new InvalidOperationException("The permit population contains an unexpected native row.");
            return rows.Cast<PERMIT_APPLICATION>().ToArray();
        }
        public async Task<IReadOnlyList<PERMIT_APPLICATION>> GetByStatusAsync(PermitApplicationStatus status)
        {
            if(!Enum.IsDefined(status))throw new InvalidOperationException("An explicit supported permit status is required.");
            return (await GetApplicationsAsync()).Where(row=>row.STATUS==status).ToArray();
        }
    }
}
