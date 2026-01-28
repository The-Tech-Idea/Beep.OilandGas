using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class FacilityDesignResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime DesignDateValue;

        public DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }
        private double ProductionRateValue;

        public double ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }
        private string ProductTypeValue;

        public string ProductType

        {

            get { return this.ProductTypeValue; }

            set { SetProperty(ref ProductTypeValue, value); }

        }
        private List<ProcessingEquipment> SeparatorSpecificationsValue;

        public List<ProcessingEquipment> SeparatorSpecifications

        {

            get { return this.SeparatorSpecificationsValue; }

            set { SetProperty(ref SeparatorSpecificationsValue, value); }

        }
        private List<ProcessingEquipment> CompressorRequirementsValue;

        public List<ProcessingEquipment> CompressorRequirements

        {

            get { return this.CompressorRequirementsValue; }

            set { SetProperty(ref CompressorRequirementsValue, value); }

        }
        private List<ProcessingEquipment> PumpingRequirementsValue;

        public List<ProcessingEquipment> PumpingRequirements

        {

            get { return this.PumpingRequirementsValue; }

            set { SetProperty(ref PumpingRequirementsValue, value); }

        }
        private List<ProcessingEquipment> HeatExchangerSpecificationsValue;

        public List<ProcessingEquipment> HeatExchangerSpecifications

        {

            get { return this.HeatExchangerSpecificationsValue; }

            set { SetProperty(ref HeatExchangerSpecificationsValue, value); }

        }
        private List<string> ControlSystemsSpecificationValue;

        public List<string> ControlSystemsSpecification

        {

            get { return this.ControlSystemsSpecificationValue; }

            set { SetProperty(ref ControlSystemsSpecificationValue, value); }

        }
        private List<string> LaySgDownRequirementsValue;

        public List<string> LaySgDownRequirements

        {

            get { return this.LaySgDownRequirementsValue; }

            set { SetProperty(ref LaySgDownRequirementsValue, value); }

        }
        private List<string> UtilityRequirementsValue;

        public List<string> UtilityRequirements

        {

            get { return this.UtilityRequirementsValue; }

            set { SetProperty(ref UtilityRequirementsValue, value); }

        }
        private double CostEstimateValue;

        public double CostEstimate

        {

            get { return this.CostEstimateValue; }

            set { SetProperty(ref CostEstimateValue, value); }

        }
    }
}
