using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// IAS 36 impairment testing — compares carrying amount to recoverable amount (higher of VIU and FVLCTS).
    /// Persists <c>IMPAIRMENT_RECORD</c> with <c>IMPAIRMENT_TYPE</c> = <see cref="ImpairmentRecordTypeCodes.Ias36"/> and
    /// <c>REASON</c> from <see cref="ImpairmentEvaluationReasonCodes"/> (seed <c>IMPAIRMENT_RECORD_TYPE</c> / <c>IMPAIRMENT_EVALUATION_REASON</c>).
    /// CGU id is persisted in <c>COST_CENTER_ID</c>.
    /// </summary>
    public class ImpairmentTestingService : IImpairmentTestingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ImpairmentTestingService> _logger;

        public ImpairmentTestingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ImpairmentTestingService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<IMPAIRMENT_RECORD> EvaluateImpairmentAsync(
            string cguId,
            decimal carryingAmount,
            decimal valueInUse,
            decimal fairValueLessCosts,
            DateTime testDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(cguId))
                throw new ArgumentNullException(nameof(cguId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var recoverableAmount = Math.Max(valueInUse, fairValueLessCosts);
            var impairment = Math.Max(0m, carryingAmount - recoverableAmount);

            var record = new IMPAIRMENT_RECORD
            {
                IMPAIRMENT_RECORD_ID = Guid.NewGuid().ToString(),
                COST_CENTER_ID = cguId,
                IMPAIRMENT_DATE = testDate,
                IMPAIRMENT_AMOUNT = impairment,
                IMPAIRMENT_TYPE = ImpairmentRecordTypeCodes.Ias36,
                REASON = impairment > 0m
                    ? ImpairmentEvaluationReasonCodes.RecoverableBelowCarrying
                    : ImpairmentEvaluationReasonCodes.NoImpairment,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var metadata = await _metadata.GetTableMetadataAsync("IMPAIRMENT_RECORD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(IMPAIRMENT_RECORD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "IMPAIRMENT_RECORD");

            await repo.InsertAsync(record, userId);

            _logger?.LogInformation(
                "Impairment test completed for CGU {CguId}: impairment={Impairment}",
                cguId, impairment);

            return record;
        }
    }
}
