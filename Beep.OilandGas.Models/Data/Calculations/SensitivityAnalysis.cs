using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class SensitivityAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Base case NPV
        /// </summary>
        private double BaseNPVValue;

        public double BaseNPV

        {

            get { return this.BaseNPVValue; }

            set { SetProperty(ref BaseNPVValue, value); }

        }

        /// <summary>
        /// Base case IRR
        /// </summary>
        private double BaseIRRValue;

        public double BaseIRR

        {

            get { return this.BaseIRRValue; }

            set { SetProperty(ref BaseIRRValue, value); }

        }

        /// <summary>
        /// List of parameters tested for sensitivity
        /// </summary>
        private List<SensitivityParameter> ParametersValue;

        public List<SensitivityParameter> Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }
    }
}
