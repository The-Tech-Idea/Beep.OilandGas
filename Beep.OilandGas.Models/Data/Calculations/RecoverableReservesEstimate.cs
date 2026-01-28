using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class RecoverableReservesEstimate : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime EstimationDateValue;

        public DateTime EstimationDate

        {

            get { return this.EstimationDateValue; }

            set { SetProperty(ref EstimationDateValue, value); }

        }
        private double InitialReservesValue;

        public double InitialReserves

        {

            get { return this.InitialReservesValue; }

            set { SetProperty(ref InitialReservesValue, value); }

        }
        private double RecoveryFactorValue;

        public double RecoveryFactor

        {

            get { return this.RecoveryFactorValue; }

            set { SetProperty(ref RecoveryFactorValue, value); }

        }
        private double RecoverableReservesValue;

        public double RecoverableReserves

        {

            get { return this.RecoverableReservesValue; }

            set { SetProperty(ref RecoverableReservesValue, value); }

        }
        private double CumulativeProductionValue;

        public double CumulativeProduction

        {

            get { return this.CumulativeProductionValue; }

            set { SetProperty(ref CumulativeProductionValue, value); }

        }
        private double RemainingReservesValue;

        public double RemainingReserves

        {

            get { return this.RemainingReservesValue; }

            set { SetProperty(ref RemainingReservesValue, value); }

        }
        private string ProductionTrendValue;

        public string ProductionTrend

        {

            get { return this.ProductionTrendValue; }

            set { SetProperty(ref ProductionTrendValue, value); }

        }
        private double ProductionDeclineValue;

        public double ProductionDecline

        {

            get { return this.ProductionDeclineValue; }

            set { SetProperty(ref ProductionDeclineValue, value); }

        }
        private double P90EstimateValue;

        public double P90Estimate

        {

            get { return this.P90EstimateValue; }

            set { SetProperty(ref P90EstimateValue, value); }

        }  // Conservative
        private double P50EstimateValue;

        public double P50Estimate

        {

            get { return this.P50EstimateValue; }

            set { SetProperty(ref P50EstimateValue, value); }

        }  // Most likely
        private double P10EstimateValue;

        public double P10Estimate

        {

            get { return this.P10EstimateValue; }

            set { SetProperty(ref P10EstimateValue, value); }

        }  // Optimistic
        private UncertaintyRange UncertaintyRangeValue;

        public UncertaintyRange UncertaintyRange

        {

            get { return this.UncertaintyRangeValue; }

            set { SetProperty(ref UncertaintyRangeValue, value); }

        }
    }
}
