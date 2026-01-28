using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ThermalRecoveryAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string ThermalMethodValue;

        public string ThermalMethod

        {

            get { return this.ThermalMethodValue; }

            set { SetProperty(ref ThermalMethodValue, value); }

        }
        private double ReservoirTemperatureValue;

        public double ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private double CrudePaveViscosityValue;

        public double CrudePaveViscosity

        {

            get { return this.CrudePaveViscosityValue; }

            set { SetProperty(ref CrudePaveViscosityValue, value); }

        }
        private double OilSaturationValue;

        public double OilSaturation

        {

            get { return this.OilSaturationValue; }

            set { SetProperty(ref OilSaturationValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string SuitabilityValue;

        public string Suitability

        {

            get { return this.SuitabilityValue; }

            set { SetProperty(ref SuitabilityValue, value); }

        }
        private double ViscosityReductionValue;

        public double ViscosityReduction

        {

            get { return this.ViscosityReductionValue; }

            set { SetProperty(ref ViscosityReductionValue, value); }

        }
        private double MobilityImprovementValue;

        public double MobilityImprovement

        {

            get { return this.MobilityImprovementValue; }

            set { SetProperty(ref MobilityImprovementValue, value); }

        }
        private double EnergyRequirementValue;

        public double EnergyRequirement

        {

            get { return this.EnergyRequirementValue; }

            set { SetProperty(ref EnergyRequirementValue, value); }

        }
        private double ProjectedRecoveryFactorValue;

        public double ProjectedRecoveryFactor

        {

            get { return this.ProjectedRecoveryFactorValue; }

            set { SetProperty(ref ProjectedRecoveryFactorValue, value); }

        }
        private double OperatingCostPerBarrelValue;

        public double OperatingCostPerBarrel

        {

            get { return this.OperatingCostPerBarrelValue; }

            set { SetProperty(ref OperatingCostPerBarrelValue, value); }

        }
        private string EnvironmentalImpactValue;

        public string EnvironmentalImpact

        {

            get { return this.EnvironmentalImpactValue; }

            set { SetProperty(ref EnvironmentalImpactValue, value); }

        }
    }
}
