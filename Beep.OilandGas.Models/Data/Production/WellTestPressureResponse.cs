using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellTestPressureResponse : ModelEntityBase
    {
        private string PressureIdValue = string.Empty;

        public string PressureId

        {

            get { return this.PressureIdValue; }

            set { SetProperty(ref PressureIdValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        
        // Pressure measurement
        private DateTime? PressureDateValue;

        public DateTime? PressureDate

        {

            get { return this.PressureDateValue; }

            set { SetProperty(ref PressureDateValue, value); }

        }
        private decimal? StaticPressureValue;

        public decimal? StaticPressure

        {

            get { return this.StaticPressureValue; }

            set { SetProperty(ref StaticPressureValue, value); }

        }
        private decimal? FlowingPressureValue;

        public decimal? FlowingPressure

        {

            get { return this.FlowingPressureValue; }

            set { SetProperty(ref FlowingPressureValue, value); }

        }
        private decimal? BottomHolePressureValue;

        public decimal? BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }
        private decimal? WellheadPressureValue;

        public decimal? WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        }
        private string? PressureOuomValue;

        public string? PressureOuom

        {

            get { return this.PressureOuomValue; }

            set { SetProperty(ref PressureOuomValue, value); }

        }
        
        // Pressure type
        private string? PressureTypeValue;

        public string? PressureType

        {

            get { return this.PressureTypeValue; }

            set { SetProperty(ref PressureTypeValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
        private string? PreferredIndValue;

        public string? PreferredInd

        {

            get { return this.PreferredIndValue; }

            set { SetProperty(ref PreferredIndValue, value); }

        }
        
        // Audit fields
        private DateTime? CreateDateValue;

        public DateTime? CreateDate

        {

            get { return this.CreateDateValue; }

            set { SetProperty(ref CreateDateValue, value); }

        }
        private string? CreateUserValue;

        public string? CreateUser

        {

            get { return this.CreateUserValue; }

            set { SetProperty(ref CreateUserValue, value); }

        }
        private DateTime? UpdateDateValue;

        public DateTime? UpdateDate

        {

            get { return this.UpdateDateValue; }

            set { SetProperty(ref UpdateDateValue, value); }

        }
        private string? UpdateUserValue;

        public string? UpdateUser

        {

            get { return this.UpdateUserValue; }

            set { SetProperty(ref UpdateUserValue, value); }

        }
    }
}
