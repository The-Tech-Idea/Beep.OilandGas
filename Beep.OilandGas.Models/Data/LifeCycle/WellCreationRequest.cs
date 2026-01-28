using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class WellCreationRequest : ModelEntityBase
    {
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? WellTypeValue;

        public string? WellType

        {

            get { return this.WellTypeValue; }

            set { SetProperty(ref WellTypeValue, value); }

        }
        private string? WellPurposeValue;

        public string? WellPurpose

        {

            get { return this.WellPurposeValue; }

            set { SetProperty(ref WellPurposeValue, value); }

        }
        private decimal? SurfaceLatitudeValue;

        public decimal? SurfaceLatitude

        {

            get { return this.SurfaceLatitudeValue; }

            set { SetProperty(ref SurfaceLatitudeValue, value); }

        }
        private decimal? SurfaceLongitudeValue;

        public decimal? SurfaceLongitude

        {

            get { return this.SurfaceLongitudeValue; }

            set { SetProperty(ref SurfaceLongitudeValue, value); }

        }
        private decimal? BottomHoleLatitudeValue;

        public decimal? BottomHoleLatitude

        {

            get { return this.BottomHoleLatitudeValue; }

            set { SetProperty(ref BottomHoleLatitudeValue, value); }

        }
        private decimal? BottomHoleLongitudeValue;

        public decimal? BottomHoleLongitude

        {

            get { return this.BottomHoleLongitudeValue; }

            set { SetProperty(ref BottomHoleLongitudeValue, value); }

        }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }
}
