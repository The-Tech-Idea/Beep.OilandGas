using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Seismic
{
    /// <summary>
    /// Service for Seismic data management
    /// </summary>
    public class PPDMSeismicService : IPPDMSeismicService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;

        public PPDMSeismicService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
        }

        public async Task<List<object>> GetSeismicSurveysAsync(List<AppFilter> filters = null)
        {
            var surveyMetadata = await _metadata.GetTableMetadataAsync("SEISMIC_SURVEY");
            if (surveyMetadata == null)
                return new List<object>();

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{surveyMetadata.EntityTypeName}");
            if (entityType == null)
                return new List<object>();

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "SEISMIC_SURVEY");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.ToList();
        }

        public async Task<List<object>> GetSeismicAcquisitionAsync(List<AppFilter> filters = null)
        {
            var acquisitionMetadata = await _metadata.GetTableMetadataAsync("SEISMIC_ACQUISITION");
            if (acquisitionMetadata == null)
                return new List<object>();

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{acquisitionMetadata.EntityTypeName}");
            if (entityType == null)
                return new List<object>();

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "SEISMIC_ACQUISITION");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.ToList();
        }

        public async Task<List<object>> GetSeismicProcessingAsync(List<AppFilter> filters = null)
        {
            var processingMetadata = await _metadata.GetTableMetadataAsync("SEISMIC_PROCESSING");
            if (processingMetadata == null)
                return new List<object>();

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{processingMetadata.EntityTypeName}");
            if (entityType == null)
                return new List<object>();

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "SEISMIC_PROCESSING");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.ToList();
        }
    }
}


