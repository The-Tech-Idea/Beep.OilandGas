using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.DCA;
using Beep.OilandGas.DCA.Services;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.ProductionForecasting.Calculations;
using Beep.OilandGas.ProductionForecasting.Constants;
using Microsoft.Extensions.Logging.Abstractions;

namespace Beep.OilandGas.ProductionForecasting.Services
{
    public partial class ProductionForecastingService
    {
        public async Task<ProductionForecastResult> GenerateForecastAsync(GenerateForecastRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return await GenerateForecastFromRequestAsync(request).ConfigureAwait(false);
        }

        private async Task<ProductionForecastResult> GenerateForecastFromRequestAsync(GenerateForecastRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.WellUWI) && string.IsNullOrWhiteSpace(request.FieldId))
                throw new ArgumentException("Either WellUWI or FieldId must be provided.");
            if (request.ForecastMethod == ForecastType.None)
                throw new ArgumentException("Forecast method cannot be None.", nameof(request));
            if (request.ForecastPeriod < 1)
                throw new ArgumentException("Forecast period must be at least 1.", nameof(request));

            var forecastDurationDays = request.ForecastPeriod * ForecastAlgorithmConstants.DaysPerMonth;
            var timeSteps = Math.Max(request.ForecastPeriod, 12);

            var (qi, di, b, fitNotes) = await ResolveDeclineParametersAsync(request).ConfigureAwait(false);

            var qEcon = request.EconomicLimitOilRate;
            var terminalDi = request.TerminalDeclineDi ?? ForecastAlgorithmConstants.DefaultTerminalDeclineDiPerDay;
            var useModified = request.UseModifiedHyperbolic;

            if (qEcon.HasValue && (qEcon.Value <= 0 || qEcon.Value >= qi))
                throw new ArgumentException("EconomicLimitOilRate must be > 0 and less than qi.");

            var pf = BuildDeclineProductionForecast(
                request.ForecastMethod,
                qi, di, b,
                forecastDurationDays,
                timeSteps,
                qEcon,
                useModified,
                terminalDi);

            var anchor = DateTime.UtcNow.Date;
            var result = ProductionForecastResultMapper.FromDeclineForecast(
                _defaults,
                pf,
                request.WellUWI,
                request.FieldId,
                request.ForecastMethod,
                anchor,
                ProductionForecastingReferenceCodes.RunStatusGenerated);

            if (!string.IsNullOrEmpty(fitNotes))
                result.Notes = string.IsNullOrEmpty(result.Notes) ? fitNotes : $"{result.Notes}; {fitNotes}";

            if (b > ForecastAlgorithmConstants.ArpsBdfSoftMaxB)
                result.Notes = AppendNote(result.Notes, "Decline exponent b is in the upper Arps range; verify boundary-dominated flow assumptions.");

            result.RSquared = null;
            result.RMSE = null;
            return result;
        }

        private static string? AppendNote(string? existing, string note) =>
            string.IsNullOrEmpty(existing) ? note : $"{existing} {note}";

        private async Task<(decimal qi, decimal di, decimal b, string? notes)> ResolveDeclineParametersAsync(GenerateForecastRequest request)
        {
            decimal qi = request.InitialOilRateQi ?? ForecastAlgorithmConstants.DefaultQiOilStbPerDay;
            decimal di = request.InitialDeclineDi ?? ForecastAlgorithmConstants.DefaultDiPerDay;
            decimal b = ProductionForecastResultMapper.ClampDeclineExponentB(
                request.DeclineExponentB ?? ForecastAlgorithmConstants.DefaultHyperbolicB);

            var hasManualQi = request.InitialOilRateQi.HasValue;
            var hasManualDi = request.InitialDeclineDi.HasValue;
            var hasManualB = request.DeclineExponentB.HasValue;

            if (hasManualQi && hasManualDi && hasManualB)
                return (qi, di, b, null);

            if (request.SkipHistoryFit || string.IsNullOrWhiteSpace(request.WellUWI))
                return (qi, di, b, null);

            var history = await ProductionHistoryLoader.TryLoadOilHistoryAsync(
                _editor, _commonColumnHandler, _defaults, _metadata, _connectionName, request.WellUWI!).ConfigureAwait(false);

            if (history == null)
            {
                if (!hasManualQi || !hasManualDi)
                    throw new ArgumentException(
                        $"Insufficient production history for well '{request.WellUWI}' to fit decline parameters. Provide InitialOilRateQi and InitialDeclineDi, or ensure at least {ForecastAlgorithmConstants.MinHistoryPointsForFit} PDEN_VOL_SUMMARY periods with positive oil volumes.");
                return (qi, di, b, null);
            }

            var (rates, dates) = history.Value;
            double qiGuess = Math.Max(rates.Max(), Beep.OilandGas.DCA.Constants.DCAConstants.MinInitialProductionRate + 1e-6);
            double tSpan = (dates[^1] - dates[0]).TotalDays;
            double diGuess = (double)ForecastAlgorithmConstants.DefaultDiPerDay;
            if (tSpan > 1 && rates[0] > 0 && rates[^1] > 0 && rates[^1] < rates[0])
            {
                diGuess = -(Math.Log(rates[^1] / rates[0])) / tSpan;
                diGuess = Math.Clamp(diGuess, 1e-6, 1.5);
            }

            var manager = new DCAManager();
            var managerFit = manager.AnalyzeWithStatistics(rates, dates, qiGuess, diGuess);

            var analysisService = new DCAAnalysisService(NullLogger<DCAAnalysisService>.Instance);
            var asyncFit = await analysisService.AnalyzeDeclineAsync(rates, dates, qiGuess, diGuess).ConfigureAwait(false);
            var fit = asyncFit.RSquared >= managerFit.RSquared ? asyncFit : managerFit;

            var qiF = (decimal)Math.Max(fit.Parameters[0], Beep.OilandGas.DCA.Constants.DCAConstants.MinInitialProductionRate + 1e-6);
            var bRaw = fit.Parameters.Length > 1 ? (decimal)fit.Parameters[1] : ForecastAlgorithmConstants.DefaultHyperbolicB;
            var bF = ProductionForecastResultMapper.ClampDeclineExponentB(bRaw);
            var diF = (decimal)diGuess;

            if (!hasManualQi) qi = qiF;
            if (!hasManualDi) di = diF;
            if (!hasManualB) b = bF;

            return (qi, di, b, $"History fit RSquared={fit.RSquared:F3} RMSE={fit.RMSE:F3}");
        }

        private static PRODUCTION_FORECAST BuildDeclineProductionForecast(
            ForecastType method,
            decimal qi,
            decimal di,
            decimal b,
            decimal forecastDurationDays,
            int timeSteps,
            decimal? economicLimit,
            bool useModifiedHyperbolic,
            decimal terminalDi)
        {
            return method switch
            {
                ForecastType.Exponential => DeclineForecast.GenerateExponentialDeclineForecast(qi, di, forecastDurationDays, timeSteps),
                ForecastType.Harmonic => DeclineForecast.GenerateHarmonicDeclineForecast(qi, di, forecastDurationDays, economicLimit, timeSteps),
                ForecastType.Hyperbolic when useModifiedHyperbolic =>
                    DeclineForecast.GenerateModifiedHyperbolicDeclineForecast(qi, di, b, forecastDurationDays, terminalDi, economicLimit, timeSteps),
                ForecastType.Hyperbolic => DeclineForecast.GenerateHyperbolicDeclineForecast(qi, di, b, forecastDurationDays, economicLimit, timeSteps),
                ForecastType.BoundaryDominated => DeclineForecast.GenerateHarmonicDeclineForecast(qi, di, forecastDurationDays, economicLimit, timeSteps),
                ForecastType.GasWell => DeclineForecast.GenerateAutoSelectDeclineForecast(qi, di, "gas", forecastDurationDays, economicLimit, timeSteps),
                ForecastType.Economic => DeclineForecast.GenerateHarmonicDeclineForecast(qi, di, forecastDurationDays, economicLimit, timeSteps),
                _ => DeclineForecast.GenerateAutoSelectDeclineForecast(qi, di, "conventional", forecastDurationDays, economicLimit, timeSteps)
            };
        }
    }
}
