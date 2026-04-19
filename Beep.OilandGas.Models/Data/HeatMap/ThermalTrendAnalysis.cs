using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class ThermalTrendAnalysis : ModelEntityBase
    {
        private string _trendId = string.Empty;
        public string TrendId { get { return _trendId; } set { SetProperty(ref _trendId, value); } }

        private string _locationId = string.Empty;
        public string LocationId { get { return _locationId; } set { SetProperty(ref _locationId, value); } }

        private DateTime _analysisDate;
        public DateTime AnalysisDate { get { return _analysisDate; } set { SetProperty(ref _analysisDate, value); } }

        private int _monthsAnalyzed;
        public int MonthsAnalyzed { get { return _monthsAnalyzed; } set { SetProperty(ref _monthsAnalyzed, value); } }

        private decimal _temperatureTrend;
        public decimal TemperatureTrend { get { return _temperatureTrend; } set { SetProperty(ref _temperatureTrend, value); } }

        private decimal _trendSlope;
        public decimal TrendSlope { get { return _trendSlope; } set { SetProperty(ref _trendSlope, value); } }

        private string _trendDirection = string.Empty;
        public string TrendDirection { get { return _trendDirection; } set { SetProperty(ref _trendDirection, value); } }

        private decimal _percentChange;
        public decimal PercentChange { get { return _percentChange; } set { SetProperty(ref _percentChange, value); } }

        private List<decimal> _historicalTemperatures = new();
        public List<decimal> HistoricalTemperatures { get { return _historicalTemperatures; } set { SetProperty(ref _historicalTemperatures, value); } }

        private decimal _predictedTemperature;
        public decimal PredictedTemperature { get { return _predictedTemperature; } set { SetProperty(ref _predictedTemperature, value); } }

        private int _predictionMonths;
        public int PredictionMonths { get { return _predictionMonths; } set { SetProperty(ref _predictionMonths, value); } }

        private decimal _rSquared;
        public decimal RSquared { get { return _rSquared; } set { SetProperty(ref _rSquared, value); } }
    }
}
