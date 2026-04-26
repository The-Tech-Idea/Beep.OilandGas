using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProjectMetrics : ModelEntityBase
    {
        /// <summary>
        /// Name or identifier of the project
        /// </summary>
        private string ProjectNameValue;

        public string ProjectName

        {

            get { return this.ProjectNameValue; }

            set { SetProperty(ref ProjectNameValue, value); }

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
        /// Profitability Index
        /// </summary>
        private double ProfitabilityIndexValue;

        public double ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }

        /// <summary>
        /// Rank among compared projects (1 = best)
        /// </summary>
        private int RankValue;

        public int Rank

        {

            get { return this.RankValue; }

            set { SetProperty(ref RankValue, value); }

        }

        /// <summary>
        /// Scoring points based on ranking
        /// </summary>
        private double ScoreValue;

        public double Score

        {

            get { return this.ScoreValue; }

            set { SetProperty(ref ScoreValue, value); }

        }

        // ── Best-practice additions (SPE PRMS §6.2 / SPEE guidelines) ─────────────

        /// <summary>Discount rate used for NPV — must be reported alongside NPV per SPE PRMS §6.2</summary>
        private double DISCOUNTRATEValue;
        public double DISCOUNT_RATE
        {
            get { return this.DISCOUNTRATEValue; }
            set { SetProperty(ref DISCOUNTRATEValue, value); }
        }

        /// <summary>NPV at 10% discount rate — standard O&G benchmark (SPEE guideline §4)</summary>
        private double NPV10Value;
        public double NPV10
        {
            get { return this.NPV10Value; }
            set { SetProperty(ref NPV10Value, value); }
        }

        /// <summary>Date the economic evaluation was performed</summary>
        private DateTime EVALUATION_DATEValue;
        public DateTime EVALUATION_DATE
        {
            get { return this.EVALUATION_DATEValue; }
            set { SetProperty(ref EVALUATION_DATEValue, value); }
        }

        /// <summary>ISO 4217 currency code for monetary metrics (e.g. USD, GBP)</summary>
        private string CURRENCYValue = "USD";
        public string CURRENCY
        {
            get { return this.CURRENCYValue; }
            set { SetProperty(ref CURRENCYValue, value); }
        }
    }
}
