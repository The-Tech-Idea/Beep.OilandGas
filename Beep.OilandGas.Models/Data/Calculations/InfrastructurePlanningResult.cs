using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class InfrastructurePlanningResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime PlanningDateValue;

        public DateTime PlanningDate

        {

            get { return this.PlanningDateValue; }

            set { SetProperty(ref PlanningDateValue, value); }

        }
        private double PeakProductionValue;

        public double PeakProduction

        {

            get { return this.PeakProductionValue; }

            set { SetProperty(ref PeakProductionValue, value); }

        }
        private string ProductTypeValue;

        public string ProductType

        {

            get { return this.ProductTypeValue; }

            set { SetProperty(ref ProductTypeValue, value); }

        }
        private double DistanceToMarketValue;

        public double DistanceToMarket

        {

            get { return this.DistanceToMarketValue; }

            set { SetProperty(ref DistanceToMarketValue, value); }

        }
        private ProcessingFacilitySpecification ProcessingFacilityValue;

        public ProcessingFacilitySpecification ProcessingFacility

        {

            get { return this.ProcessingFacilityValue; }

            set { SetProperty(ref ProcessingFacilityValue, value); }

        }
        private StorageRequirementsResult StorageRequirementsValue;

        public StorageRequirementsResult StorageRequirements

        {

            get { return this.StorageRequirementsValue; }

            set { SetProperty(ref StorageRequirementsValue, value); }

        }
        private string TransportationMethodValue;

        public string TransportationMethod

        {

            get { return this.TransportationMethodValue; }

            set { SetProperty(ref TransportationMethodValue, value); }

        }
        private PipelineSpecification PipelineSpecificationsValue;

        public PipelineSpecification PipelineSpecifications

        {

            get { return this.PipelineSpecificationsValue; }

            set { SetProperty(ref PipelineSpecificationsValue, value); }

        }
        private double PowerRequirementsValue;

        public double PowerRequirements

        {

            get { return this.PowerRequirementsValue; }

            set { SetProperty(ref PowerRequirementsValue, value); }

        }
        private string WaterHandlingValue;

        public string WaterHandling

        {

            get { return this.WaterHandlingValue; }

            set { SetProperty(ref WaterHandlingValue, value); }

        }
        private List<string> SafetySystemsRequiredValue;

        public List<string> SafetySystemsRequired

        {

            get { return this.SafetySystemsRequiredValue; }

            set { SetProperty(ref SafetySystemsRequiredValue, value); }

        }
        private List<string> EnvironmentalControlsRequiredValue;

        public List<string> EnvironmentalControlsRequired

        {

            get { return this.EnvironmentalControlsRequiredValue; }

            set { SetProperty(ref EnvironmentalControlsRequiredValue, value); }

        }
    }
}
