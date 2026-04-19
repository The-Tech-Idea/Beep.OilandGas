using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class ThermalComparisonResult : ModelEntityBase
    {
        private string _comparisonId = string.Empty;
        public string ComparisonId { get { return _comparisonId; } set { SetProperty(ref _comparisonId, value); } }

        private string _locationId = string.Empty;
        public string LocationId { get { return _locationId; } set { SetProperty(ref _locationId, value); } }

        private DateTime _comparisonDate;
        public DateTime ComparisonDate { get { return _comparisonDate; } set { SetProperty(ref _comparisonDate, value); } }

        private DateTime _baselineDate;
        public DateTime BaselineDate { get { return _baselineDate; } set { SetProperty(ref _baselineDate, value); } }

        private DateTime _currentDate;
        public DateTime CurrentDate { get { return _currentDate; } set { SetProperty(ref _currentDate, value); } }

        private decimal _baselineAverageTemperature;
        public decimal BaselineAverageTemperature { get { return _baselineAverageTemperature; } set { SetProperty(ref _baselineAverageTemperature, value); } }

        private decimal _currentAverageTemperature;
        public decimal CurrentAverageTemperature { get { return _currentAverageTemperature; } set { SetProperty(ref _currentAverageTemperature, value); } }

        private decimal _temperatureChange;
        public decimal TemperatureChange { get { return _temperatureChange; } set { SetProperty(ref _temperatureChange, value); } }

        private decimal _percentChange;
        public decimal PercentChange { get { return _percentChange; } set { SetProperty(ref _percentChange, value); } }

        private decimal _baselineStdDev;
        public decimal BaselineStdDev { get { return _baselineStdDev; } set { SetProperty(ref _baselineStdDev, value); } }

        private decimal _currentStdDev;
        public decimal CurrentStdDev { get { return _currentStdDev; } set { SetProperty(ref _currentStdDev, value); } }

        private string _significantChange = string.Empty;
        public string SignificantChange { get { return _significantChange; } set { SetProperty(ref _significantChange, value); } }

        private List<string> _changePatterns = new();
        public List<string> ChangePatterns { get { return _changePatterns; } set { SetProperty(ref _changePatterns, value); } }
    }
}
