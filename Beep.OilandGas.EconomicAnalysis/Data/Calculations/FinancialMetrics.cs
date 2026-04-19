using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class FinancialMetrics : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the metrics calculation
        /// </summary>
        private string MetricsIdValue;

        public string MetricsId

        {

            get { return this.MetricsIdValue; }

            set { SetProperty(ref MetricsIdValue, value); }

        }

        /// <summary>
        /// Date the metrics were calculated
        /// </summary>
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }

        /// <summary>
        /// Net Present Value
        /// </summary>
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return
        /// </summary>
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Payback period in years
        /// </summary>
        private double PaybackPeriodValue;

        public double PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        }

        /// <summary>
        /// Profitability Index (PV of future cash flows / Initial investment)
        /// </summary>
        private double ProfitabilityIndexValue;

        public double ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }

        /// <summary>
        /// Return on Investment
        /// </summary>
        private double ROIValue;

        public double ROI

        {

            get { return this.ROIValue; }

            set { SetProperty(ref ROIValue, value); }

        }

        /// <summary>
        /// Modified Internal Rate of Return (accounts for financing and reinvestment)
        /// </summary>
        private double ModifiedIRRValue;

        public double ModifiedIRR

        {

            get { return this.ModifiedIRRValue; }

            set { SetProperty(ref ModifiedIRRValue, value); }

        }

        /// <summary>
        /// Equivalent Annual Cost for comparing projects of different lifespans
        /// </summary>
        private double EquivalentAnnualCostValue;

        public double EquivalentAnnualCost

        {

            get { return this.EquivalentAnnualCostValue; }

            set { SetProperty(ref EquivalentAnnualCostValue, value); }

        }

        /// <summary>
        /// Vestigial value (remaining value after cost recovery)
        /// </summary>
        private double VestigialValueValue;

        public double VestigialValue

        {

            get { return this.VestigialValueValue; }

            set { SetProperty(ref VestigialValueValue, value); }

        }
    }
}
