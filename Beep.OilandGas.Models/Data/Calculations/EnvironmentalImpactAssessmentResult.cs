using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EnvironmentalImpactAssessmentResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime AssessmentDateValue;

        public DateTime AssessmentDate

        {

            get { return this.AssessmentDateValue; }

            set { SetProperty(ref AssessmentDateValue, value); }

        }
        private string LocationValue;

        public string Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
        private string EnvironmentalSensitivityValue;

        public string EnvironmentalSensitivity

        {

            get { return this.EnvironmentalSensitivityValue; }

            set { SetProperty(ref EnvironmentalSensitivityValue, value); }

        }
        private string AirQualityImpactValue;

        public string AirQualityImpact

        {

            get { return this.AirQualityImpactValue; }

            set { SetProperty(ref AirQualityImpactValue, value); }

        }
        private string WaterQualityImpactValue;

        public string WaterQualityImpact

        {

            get { return this.WaterQualityImpactValue; }

            set { SetProperty(ref WaterQualityImpactValue, value); }

        }
        private string SoilAndLandImpactValue;

        public string SoilAndLandImpact

        {

            get { return this.SoilAndLandImpactValue; }

            set { SetProperty(ref SoilAndLandImpactValue, value); }

        }
        private string BiodiversityImpactValue;

        public string BiodiversityImpact

        {

            get { return this.BiodiversityImpactValue; }

            set { SetProperty(ref BiodiversityImpactValue, value); }

        }
        private double GHGEmissionsValue;

        public double GHGEmissions

        {

            get { return this.GHGEmissionsValue; }

            set { SetProperty(ref GHGEmissionsValue, value); }

        }
        private List<string> MitigationMeasuresValue;

        public List<string> MitigationMeasures

        {

            get { return this.MitigationMeasuresValue; }

            set { SetProperty(ref MitigationMeasuresValue, value); }

        }
        private List<string> ComplianceRequirementsValue;

        public List<string> ComplianceRequirements

        {

            get { return this.ComplianceRequirementsValue; }

            set { SetProperty(ref ComplianceRequirementsValue, value); }

        }
        private double CostOfMitigationValue;

        public double CostOfMitigation

        {

            get { return this.CostOfMitigationValue; }

            set { SetProperty(ref CostOfMitigationValue, value); }

        }
    }
}
