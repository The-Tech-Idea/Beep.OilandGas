using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class ThermalAnomaly : ModelEntityBase
    {
        private string _anomalyId = string.Empty;
        public string AnomalyId { get { return _anomalyId; } set { SetProperty(ref _anomalyId, value); } }

        private string _locationId = string.Empty;
        public string LocationId { get { return _locationId; } set { SetProperty(ref _locationId, value); } }

        private DateTime _detectionDate;
        public DateTime DetectionDate { get { return _detectionDate; } set { SetProperty(ref _detectionDate, value); } }

        private decimal _anomalyTemperature;
        public decimal AnomalyTemperature { get { return _anomalyTemperature; } set { SetProperty(ref _anomalyTemperature, value); } }

        private decimal _expectedTemperature;
        public decimal ExpectedTemperature { get { return _expectedTemperature; } set { SetProperty(ref _expectedTemperature, value); } }

        private decimal _temperatureDeviation;
        public decimal TemperatureDeviation { get { return _temperatureDeviation; } set { SetProperty(ref _temperatureDeviation, value); } }

        private decimal _deviationPercent;
        public decimal DeviationPercent { get { return _deviationPercent; } set { SetProperty(ref _deviationPercent, value); } }

        private string _anomalyType = string.Empty;
        public string AnomalyType { get { return _anomalyType; } set { SetProperty(ref _anomalyType, value); } }

        private string _severity = string.Empty;
        public string Severity { get { return _severity; } set { SetProperty(ref _severity, value); } }

        private decimal _x;
        public decimal X { get { return _x; } set { SetProperty(ref _x, value); } }

        private decimal _y;
        public decimal Y { get { return _y; } set { SetProperty(ref _y, value); } }

        private string _description = string.Empty;
        public string Description { get { return _description; } set { SetProperty(ref _description, value); } }

        private List<string> _recommendedActions = new();
        public List<string> RecommendedActions { get { return _recommendedActions; } set { SetProperty(ref _recommendedActions, value); } }
    }
}
