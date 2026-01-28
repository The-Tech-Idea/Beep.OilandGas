using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProductionScheduleOptimizationResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime OptimizationDateValue;

        public DateTime OptimizationDate

        {

            get { return this.OptimizationDateValue; }

            set { SetProperty(ref OptimizationDateValue, value); }

        }
        private double ProductionCapacityValue;

        public double ProductionCapacity

        {

            get { return this.ProductionCapacityValue; }

            set { SetProperty(ref ProductionCapacityValue, value); }

        }
        private double MarketPriceValue;

        public double MarketPrice

        {

            get { return this.MarketPriceValue; }

            set { SetProperty(ref MarketPriceValue, value); }

        }
        private double OperatingCostPerUnitValue;

        public double OperatingCostPerUnit

        {

            get { return this.OperatingCostPerUnitValue; }

            set { SetProperty(ref OperatingCostPerUnitValue, value); }

        }
        private List<ProductionPhase> ProductionPhasesValue;

        public List<ProductionPhase> ProductionPhases

        {

            get { return this.ProductionPhasesValue; }

            set { SetProperty(ref ProductionPhasesValue, value); }

        }
        private List<double> MonthlyScheduleValue;

        public List<double> MonthlySchedule

        {

            get { return this.MonthlyScheduleValue; }

            set { SetProperty(ref MonthlyScheduleValue, value); }

        }
        private double BreakevenValue;

        public double Breakeven

        {

            get { return this.BreakevenValue; }

            set { SetProperty(ref BreakevenValue, value); }

        }
        private double MarginValue;

        public double Margin

        {

            get { return this.MarginValue; }

            set { SetProperty(ref MarginValue, value); }

        }
        private List<double> MonthlyRevenueValue;

        public List<double> MonthlyRevenue

        {

            get { return this.MonthlyRevenueValue; }

            set { SetProperty(ref MonthlyRevenueValue, value); }

        }
        private List<double> MonthlyCashFlowValue;

        public List<double> MonthlyCashFlow

        {

            get { return this.MonthlyCashFlowValue; }

            set { SetProperty(ref MonthlyCashFlowValue, value); }

        }
        private double TotalProjectRevenueValue;

        public double TotalProjectRevenue

        {

            get { return this.TotalProjectRevenueValue; }

            set { SetProperty(ref TotalProjectRevenueValue, value); }

        }
        private double TotalProjectCostValue;

        public double TotalProjectCost

        {

            get { return this.TotalProjectCostValue; }

            set { SetProperty(ref TotalProjectCostValue, value); }

        }
        private double NetCashFlowValue;

        public double NetCashFlow

        {

            get { return this.NetCashFlowValue; }

            set { SetProperty(ref NetCashFlowValue, value); }

        }
    }
}
