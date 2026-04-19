using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class TemperatureGradientAnalysis : ModelEntityBase
    {
        private string _gradientId = string.Empty;
        public string GradientId { get { return _gradientId; } set { SetProperty(ref _gradientId, value); } }

        private string _locationId = string.Empty;
        public string LocationId { get { return _locationId; } set { SetProperty(ref _locationId, value); } }

        private DateTime _analysisDate;
        public DateTime AnalysisDate { get { return _analysisDate; } set { SetProperty(ref _analysisDate, value); } }

        private decimal _averageGradient;
        public decimal AverageGradient { get { return _averageGradient; } set { SetProperty(ref _averageGradient, value); } }

        private decimal _maxGradient;
        public decimal MaxGradient { get { return _maxGradient; } set { SetProperty(ref _maxGradient, value); } }

        private decimal _minGradient;
        public decimal MinGradient { get { return _minGradient; } set { SetProperty(ref _minGradient, value); } }

        private decimal _horizontalGradient;
        public decimal HorizontalGradient { get { return _horizontalGradient; } set { SetProperty(ref _horizontalGradient, value); } }

        private decimal _verticalGradient;
        public decimal VerticalGradient { get { return _verticalGradient; } set { SetProperty(ref _verticalGradient, value); } }

        private string _gradientPattern = string.Empty;
        public string GradientPattern { get { return _gradientPattern; } set { SetProperty(ref _gradientPattern, value); } }

        private List<GradientPoint> _gradientPoints = new();
        public List<GradientPoint> GradientPoints { get { return _gradientPoints; } set { SetProperty(ref _gradientPoints, value); } }
    }
}
