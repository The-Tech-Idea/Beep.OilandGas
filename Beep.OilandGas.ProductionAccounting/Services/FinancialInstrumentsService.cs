using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// IFRS 9 financial instruments and hedge accounting service.
    /// </summary>
    public class FinancialInstrumentsService : IFinancialInstrumentsService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<FinancialInstrumentsService> _logger;

        public FinancialInstrumentsService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<FinancialInstrumentsService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<FINANCIAL_INSTRUMENT> UpdateFairValueAsync(
            FINANCIAL_INSTRUMENT instrument,
            decimal fairValue,
            DateTime valuationDate,
            string userId,
            string cn = "PPDM39")
        {
            if (instrument == null)
                throw new ArgumentNullException(nameof(instrument));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            instrument.INSTRUMENT_ID ??= Guid.NewGuid().ToString();
            instrument.FAIR_VALUE = fairValue;
            instrument.VALUATION_DATE = valuationDate;
            instrument.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            instrument.PPDM_GUID ??= Guid.NewGuid().ToString();
            instrument.ROW_CHANGED_BY = userId;
            instrument.ROW_CHANGED_DATE = DateTime.UtcNow;

            var metadata = await _metadata.GetTableMetadataAsync("FINANCIAL_INSTRUMENT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(FINANCIAL_INSTRUMENT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "FINANCIAL_INSTRUMENT");

            var existing = await repo.GetByIdAsync(instrument.INSTRUMENT_ID) as FINANCIAL_INSTRUMENT;
            if (existing == null)
                await repo.InsertAsync(instrument, userId);
            else
                await repo.UpdateAsync(instrument, userId);

            _logger?.LogInformation(
                "Updated fair value for instrument {InstrumentId} to {FairValue}",
                instrument.INSTRUMENT_ID, fairValue);

            return instrument;
        }

        public async Task<HEDGE_MEASUREMENT> MeasureHedgeAsync(
            HEDGE_RELATIONSHIP hedge,
            decimal hedgedItemChange,
            decimal hedgingInstrumentChange,
            DateTime measurementDate,
            string userId,
            string cn = "PPDM39")
        {
            if (hedge == null)
                throw new ArgumentNullException(nameof(hedge));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var hedgeRatio = hedge.HEDGE_RATIO ?? 1m;
            var adjustedHedgedChange = hedgedItemChange * hedgeRatio;
            var effectivePortion = Math.Sign(hedgingInstrumentChange) *
                Math.Min(Math.Abs(adjustedHedgedChange), Math.Abs(hedgingInstrumentChange));
            var ineffectivePortion = hedgingInstrumentChange - effectivePortion;

            var measurement = new HEDGE_MEASUREMENT
            {
                HEDGE_MEASUREMENT_ID = Guid.NewGuid().ToString(),
                HEDGE_RELATIONSHIP_ID = hedge.HEDGE_RELATIONSHIP_ID,
                MEASUREMENT_DATE = measurementDate,
                EFFECTIVE_PORTION = effectivePortion,
                INEFFECTIVE_PORTION = ineffectivePortion,
                TOTAL_CHANGE = hedgingInstrumentChange,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var metadata = await _metadata.GetTableMetadataAsync("HEDGE_MEASUREMENT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(HEDGE_MEASUREMENT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "HEDGE_MEASUREMENT");

            await repo.InsertAsync(measurement, userId);

            _logger?.LogInformation(
                "Hedge measurement recorded for relationship {HedgeId}: effective={Effective}, ineffective={Ineffective}",
                hedge.HEDGE_RELATIONSHIP_ID, effectivePortion, ineffectivePortion);

            return measurement;
        }
    }
}
