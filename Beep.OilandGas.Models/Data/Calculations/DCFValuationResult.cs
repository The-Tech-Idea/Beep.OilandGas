using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DCFValuationResult : ModelEntityBase
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
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Terminal growth rate beyond explicit forecast period
        /// </summary>
        private double TerminalGrowthRateValue;

        public double TerminalGrowthRate

        {

            get { return this.TerminalGrowthRateValue; }

            set { SetProperty(ref TerminalGrowthRateValue, value); }

        }

        /// <summary>
        /// Weighted Average Cost of Capital (discount rate)
        /// </summary>
        private double WACCValue;

        public double WACC

        {

            get { return this.WACCValue; }

            set { SetProperty(ref WACCValue, value); }

        }

        /// <summary>
        /// Present value components for each year of the forecast period
        /// </summary>
        private List<PVComponent> PresentValueComponentsValue;

        public List<PVComponent> PresentValueComponents

        {

            get { return this.PresentValueComponentsValue; }

            set { SetProperty(ref PresentValueComponentsValue, value); }

        }

        /// <summary>
        /// PV of explicit forecast period cash flows
        /// </summary>
        private double ExplicitPeriodValueValue;

        public double ExplicitPeriodValue

        {

            get { return this.ExplicitPeriodValueValue; }

            set { SetProperty(ref ExplicitPeriodValueValue, value); }

        }

        /// <summary>
        /// Terminal value of cash flows beyond forecast period
        /// </summary>
        private double TerminalValueValue;

        public double TerminalValue

        {

            get { return this.TerminalValueValue; }

            set { SetProperty(ref TerminalValueValue, value); }

        }

        /// <summary>
        /// Present value of terminal value
        /// </summary>
        private double PVTerminalValueValue;

        public double PVTerminalValue

        {

            get { return this.PVTerminalValueValue; }

            set { SetProperty(ref PVTerminalValueValue, value); }

        }

        /// <summary>
        /// Total enterprise value (sum of explicit period and terminal values)
        /// </summary>
        private double EnterpriseValueValue;

        public double EnterpriseValue

        {

            get { return this.EnterpriseValueValue; }

            set { SetProperty(ref EnterpriseValueValue, value); }

        }

        /// <summary>
        /// Percentage of enterprise value attributable to terminal value
        /// </summary>
        private double TerminalValuePercentageValue;

        public double TerminalValuePercentage

        {

            get { return this.TerminalValuePercentageValue; }

            set { SetProperty(ref TerminalValuePercentageValue, value); }

        }
    }
}
