using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChemicalEORAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string ChemicalTypeValue;

        public string ChemicalType

        {

            get { return this.ChemicalTypeValue; }

            set { SetProperty(ref ChemicalTypeValue, value); }

        }
        private double ReservoirTemperatureValue;

        public double ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private double SalinityValue;

        public double Salinity

        {

            get { return this.SalinityValue; }

            set { SetProperty(ref SalinityValue, value); }

        }
        private double CrudePaveViscosityValue;

        public double CrudePaveViscosity

        {

            get { return this.CrudePaveViscosityValue; }

            set { SetProperty(ref CrudePaveViscosityValue, value); }

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
        private double InterfacialTensionReductionValue;

        public double InterfacialTensionReduction

        {

            get { return this.InterfacialTensionReductionValue; }

            set { SetProperty(ref InterfacialTensionReductionValue, value); }

        }
        private double OilRecoveryIncrementValue;

        public double OilRecoveryIncrement

        {

            get { return this.OilRecoveryIncrementValue; }

            set { SetProperty(ref OilRecoveryIncrementValue, value); }

        }
        private double CostPerBarrelRecoveredValue;

        public double CostPerBarrelRecovered

        {

            get { return this.CostPerBarrelRecoveredValue; }

            set { SetProperty(ref CostPerBarrelRecoveredValue, value); }

        }
        private List<string> EnvironmentalConcernsValue = new();

        public List<string> EnvironmentalConcerns

        {

            get { return this.EnvironmentalConcernsValue; }

            set { SetProperty(ref EnvironmentalConcernsValue, value); }

        }
        private ChemicalParameters ChemicalParametersValue;

        public ChemicalParameters ChemicalParameters

        {

            get { return this.ChemicalParametersValue; }

            set { SetProperty(ref ChemicalParametersValue, value); }

        }
    }
}
