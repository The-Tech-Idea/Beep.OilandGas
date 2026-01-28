using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class RodStringDesign : ModelEntityBase
    {
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string RodSizeValue = string.Empty;

        public string RodSize

        {

            get { return this.RodSizeValue; }

            set { SetProperty(ref RodSizeValue, value); }

        }
        private string RodGradeValue = string.Empty;

        public string RodGrade

        {

            get { return this.RodGradeValue; }

            set { SetProperty(ref RodGradeValue, value); }

        }
        private int RodSectionsValue;

        public int RodSections

        {

            get { return this.RodSectionsValue; }

            set { SetProperty(ref RodSectionsValue, value); }

        }
        private decimal TotalRodLengthValue;

        public decimal TotalRodLength

        {

            get { return this.TotalRodLengthValue; }

            set { SetProperty(ref TotalRodLengthValue, value); }

        }
        private decimal SafetyFactorValue;

        public decimal SafetyFactor

        {

            get { return this.SafetyFactorValue; }

            set { SetProperty(ref SafetyFactorValue, value); }

        }
        private bool IsAdequateValue;

        public bool IsAdequate

        {

            get { return this.IsAdequateValue; }

            set { SetProperty(ref IsAdequateValue, value); }

        }
    }
}
