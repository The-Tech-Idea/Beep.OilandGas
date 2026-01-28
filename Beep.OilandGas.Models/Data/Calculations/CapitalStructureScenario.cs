using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CapitalStructureScenario : ModelEntityBase
    {
        /// <summary>
        /// Debt as percentage of total capital (0.0 to 1.0)
        /// </summary>
        private double DebtRatioValue;

        public double DebtRatio

        {

            get { return this.DebtRatioValue; }

            set { SetProperty(ref DebtRatioValue, value); }

        }

        /// <summary>
        /// Equity as percentage of total capital (0.0 to 1.0)
        /// </summary>
        private double EquityRatioValue;

        public double EquityRatio

        {

            get { return this.EquityRatioValue; }

            set { SetProperty(ref EquityRatioValue, value); }

        }

        /// <summary>
        /// Total market value of debt
        /// </summary>
        private double DebtValueValue;

        public double DebtValue

        {

            get { return this.DebtValueValue; }

            set { SetProperty(ref DebtValueValue, value); }

        }

        /// <summary>
        /// Total market value of equity
        /// </summary>
        private double EquityValueValue;

        public double EquityValue

        {

            get { return this.EquityValueValue; }

            set { SetProperty(ref EquityValueValue, value); }

        }

        /// <summary>
        /// Tax benefit from interest deductions
        /// </summary>
        private double TaxShieldValue;

        public double TaxShield

        {

            get { return this.TaxShieldValue; }

            set { SetProperty(ref TaxShieldValue, value); }

        }

        /// <summary>
        /// Levered firm value with this capital structure
        /// </summary>
        private double LeveredValueValue;

        public double LeveredValue

        {

            get { return this.LeveredValueValue; }

            set { SetProperty(ref LeveredValueValue, value); }

        }

        /// <summary>
        /// Weighted Average Cost of Capital for this structure
        /// </summary>
        private double WACCValue;

        public double WACC

        {

            get { return this.WACCValue; }

            set { SetProperty(ref WACCValue, value); }

        }

        /// <summary>
        /// Financial risk classification: Low, Medium, or High
        /// </summary>
        private string FinancialRiskValue;

        public string FinancialRisk

        {

            get { return this.FinancialRiskValue; }

            set { SetProperty(ref FinancialRiskValue, value); }

        }
    }
}
