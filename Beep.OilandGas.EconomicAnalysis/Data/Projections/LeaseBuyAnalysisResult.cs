using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class LeaseBuyAnalysisResult : ModelEntityBase
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
        /// Cost to purchase the asset
        /// </summary>
        private double AssetCostValue;

        public double AssetCost

        {

            get { return this.AssetCostValue; }

            set { SetProperty(ref AssetCostValue, value); }

        }

        /// <summary>
        /// Duration of lease in years
        /// </summary>
        private int LeaseTermValue;

        public int LeaseTerm

        {

            get { return this.LeaseTermValue; }

            set { SetProperty(ref LeaseTermValue, value); }

        }

        /// <summary>
        /// Residual value of asset at end of lease term
        /// </summary>
        private double SalvageValueValue;

        public double SalvageValue

        {

            get { return this.SalvageValueValue; }

            set { SetProperty(ref SalvageValueValue, value); }

        }

        /// <summary>
        /// Net Present Value of buying option
        /// </summary>
        private double BuyNPVValue;

        public double BuyNPV

        {

            get { return this.BuyNPVValue; }

            set { SetProperty(ref BuyNPVValue, value); }

        }

        /// <summary>
        /// Net Present Value of leasing option
        /// </summary>
        private double LeaseNPVValue;

        public double LeaseNPV

        {

            get { return this.LeaseNPVValue; }

            set { SetProperty(ref LeaseNPVValue, value); }

        }

        /// <summary>
        /// Net advantage of leasing vs buying (positive favors leasing)
        /// </summary>
        private double NetAdvantageOfLeasingValue;

        public double NetAdvantageOfLeasing

        {

            get { return this.NetAdvantageOfLeasingValue; }

            set { SetProperty(ref NetAdvantageOfLeasingValue, value); }

        }

        /// <summary>
        /// Recommendation: "Lease" or "Buy"
        /// </summary>
        private string RecommendationValue;

        public string Recommendation

        {

            get { return this.RecommendationValue; }

            set { SetProperty(ref RecommendationValue, value); }

        }
    }
}
