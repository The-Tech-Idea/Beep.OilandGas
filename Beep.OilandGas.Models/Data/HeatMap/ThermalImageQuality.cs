using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class ThermalImageQuality : ModelEntityBase
    {
        private string _qualityId = string.Empty;
        public string QualityId { get { return _qualityId; } set { SetProperty(ref _qualityId, value); } }

        private string _imageId = string.Empty;
        public string ImageId { get { return _imageId; } set { SetProperty(ref _imageId, value); } }

        private DateTime _assessmentDate;
        public DateTime AssessmentDate { get { return _assessmentDate; } set { SetProperty(ref _assessmentDate, value); } }

        private decimal _clarity;
        public decimal Clarity { get { return _clarity; } set { SetProperty(ref _clarity, value); } }

        private decimal _noiseLevel;
        public decimal NoiseLevel { get { return _noiseLevel; } set { SetProperty(ref _noiseLevel, value); } }

        private decimal _contrast;
        public decimal Contrast { get { return _contrast; } set { SetProperty(ref _contrast, value); } }

        private decimal _overallQualityScore;
        public decimal OverallQualityScore { get { return _overallQualityScore; } set { SetProperty(ref _overallQualityScore, value); } }

        private string _qualityRating = string.Empty;
        public string QualityRating { get { return _qualityRating; } set { SetProperty(ref _qualityRating, value); } }

        private List<string> _qualityIssues = new();
        public List<string> QualityIssues { get { return _qualityIssues; } set { SetProperty(ref _qualityIssues, value); } }

        private List<string> _recommendedImprovements = new();
        public List<string> RecommendedImprovements { get { return _recommendedImprovements; } set { SetProperty(ref _recommendedImprovements, value); } }
    }
}
