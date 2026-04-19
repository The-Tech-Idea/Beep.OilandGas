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
    }
}
