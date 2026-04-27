using System;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using ProspectRecord = Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    /// <summary>
    /// Service for prospect identification operations.
    /// Uses PPDMGenericRepository for PPDM-backed prospect persistence and keeps
    /// richer exploration models as aggregate projections rather than storage entities.
    /// Unit tests (mapping + deterministic analysis): <c>Beep.OilandGas.ProspectIdentification.Tests</c>.
    /// </summary>
    public partial class ProspectIdentificationService
        : IProspectIdentificationService,
          IProspectTechnicalMaturationService,
          IProspectRiskEconomicAnalysisService,
          IProspectPortfolioOptimizationService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<ProspectIdentificationService>? _logger;

        public ProspectIdentificationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<ProspectIdentificationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        private PPDMGenericRepository CreateProspectRepository()
        {
            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                typeof(ProspectRecord),
                _connectionName,
                "PROSPECT",
                null);
        }

        internal static string ResolveFieldId(ProspectRecord entity)
        {
            return !string.IsNullOrWhiteSpace(entity.PRIMARY_FIELD_ID)
                ? entity.PRIMARY_FIELD_ID
                : entity.FIELD_ID ?? string.Empty;
        }

        internal static string ResolveStatus(ProspectRecord entity)
        {
            return !string.IsNullOrWhiteSpace(entity.PROSPECT_STATUS)
                ? entity.PROSPECT_STATUS
                : entity.STATUS;
        }

        internal static string ResolveRecommendation(ProspectRecord? prospect, decimal riskScore)
        {
            if (prospect == null)
                return "Further evaluation recommended";

            if (!string.IsNullOrWhiteSpace(prospect.PROSPECT_STATUS))
            {
                return prospect.PROSPECT_STATUS.ToUpperInvariant() switch
                {
                    "APPROVED" or "COMMITTED" => "Recommend drilling",
                    "REJECTED" or "ABANDONED" => "Do not drill",
                    _ => "Further evaluation recommended"
                };
            }

            return riskScore switch
            {
                >= 0.7m => "Detailed study required",
                >= 0.4m => "Further evaluation recommended",
                _ => "Recommend drilling"
            };
        }
    }
}
