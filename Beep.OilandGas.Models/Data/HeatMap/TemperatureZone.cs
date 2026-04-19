using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class TemperatureZone : ModelEntityBase
    {
        private string _zoneId = string.Empty;
        public string ZoneId { get { return _zoneId; } set { SetProperty(ref _zoneId, value); } }

        private string _locationId = string.Empty;
        public string LocationId { get { return _locationId; } set { SetProperty(ref _locationId, value); } }

        private DateTime _identificationDate;
        public DateTime IdentificationDate { get { return _identificationDate; } set { SetProperty(ref _identificationDate, value); } }

        private decimal _minTemperature;
        public decimal MinTemperature { get { return _minTemperature; } set { SetProperty(ref _minTemperature, value); } }

        private decimal _maxTemperature;
        public decimal MaxTemperature { get { return _maxTemperature; } set { SetProperty(ref _maxTemperature, value); } }

        private decimal _averageTemperature;
        public decimal AverageTemperature { get { return _averageTemperature; } set { SetProperty(ref _averageTemperature, value); } }

        private string _zoneClassification = string.Empty;
        public string ZoneClassification { get { return _zoneClassification; } set { SetProperty(ref _zoneClassification, value); } }

        private decimal _area;
        public decimal Area { get { return _area; } set { SetProperty(ref _area, value); } }

        private int _pointCount;
        public int PointCount { get { return _pointCount; } set { SetProperty(ref _pointCount, value); } }

        private List<decimal> _boundaryCoordinates = new();
        public List<decimal> BoundaryCoordinates { get { return _boundaryCoordinates; } set { SetProperty(ref _boundaryCoordinates, value); } }
    }
}
