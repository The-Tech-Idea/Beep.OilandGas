using System;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class HEAT_MAP_DATA_POINT : ModelEntityBase
    {
        private double _x;
        public double X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        private double _y;
        public double Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }

        private double _value;
        public double Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        private string _label;
        public string Label
        {
            get { return _label; }
            set { SetProperty(ref _label, value); }
        }

        public string LABEL
        {
            get { return _label; }
            set { SetProperty(ref _label, value); }
        }

        private double _originalX;
        public double OriginalX
        {
            get { return _originalX; }
            set { SetProperty(ref _originalX, value); }
        }

        public double ORIGINAL_X
        {
            get { return _originalX; }
            set { SetProperty(ref _originalX, value); }
        }

        private double _originalY;
        public double OriginalY
        {
            get { return _originalY; }
            set { SetProperty(ref _originalY, value); }
        }

        public double ORIGINAL_Y
        {
            get { return _originalY; }
            set { SetProperty(ref _originalY, value); }
        }

        public HEAT_MAP_DATA_POINT() { }

        public HEAT_MAP_DATA_POINT(double originalX, double originalY, double value, string? label = null)
        {
            _originalX = originalX;
            _originalY = originalY;
            _x = originalX;
            _y = originalY;
            _value = value;
            _label = label;
        }
    }
}
