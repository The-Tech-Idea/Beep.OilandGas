using System;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class ThermalAnalysisResult : ModelEntityBase
    {
        private string _analysisId = string.Empty;
        public string AnalysisId { get { return _analysisId; } set { SetProperty(ref _analysisId, value); } }

        private string _locationId = string.Empty;
        public string LocationId { get { return _locationId; } set { SetProperty(ref _locationId, value); } }

        private DateTime _analysisDate;
        public DateTime AnalysisDate { get { return _analysisDate; } set { SetProperty(ref _analysisDate, value); } }

        private decimal _averageTemperature;
        public decimal AverageTemperature { get { return _averageTemperature; } set { SetProperty(ref _averageTemperature, value); } }

        private decimal _maximumTemperature;
        public decimal MaximumTemperature { get { return _maximumTemperature; } set { SetProperty(ref _maximumTemperature, value); } }

        private decimal _minimumTemperature;
        public decimal MinimumTemperature { get { return _minimumTemperature; } set { SetProperty(ref _minimumTemperature, value); } }

        private decimal _temperatureGradient;
        public decimal TemperatureGradient { get { return _temperatureGradient; } set { SetProperty(ref _temperatureGradient, value); } }

        private decimal _standardDeviation;
        public decimal StandardDeviation { get { return _standardDeviation; } set { SetProperty(ref _standardDeviation, value); } }

        private string _thermalPattern = string.Empty;
        public string ThermalPattern { get { return _thermalPattern; } set { SetProperty(ref _thermalPattern, value); } }

        private int _dataPointCount;
        public int DataPointCount { get { return _dataPointCount; } set { SetProperty(ref _dataPointCount, value); } }

        private decimal _temperatureRange;
        public decimal TemperatureRange { get { return _temperatureRange; } set { SetProperty(ref _temperatureRange, value); } }
    }
}
