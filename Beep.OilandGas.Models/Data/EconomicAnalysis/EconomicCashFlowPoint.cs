using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    public class EconomicCashFlowPoint : ModelEntityBase
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        private decimal _netCashFlow;
        public decimal NetCashFlow
        {
            get => _netCashFlow;
            set => SetProperty(ref _netCashFlow, value);
        }

        private decimal _cumulativeCashFlow;
        public decimal CumulativeCashFlow
        {
            get => _cumulativeCashFlow;
            set => SetProperty(ref _cumulativeCashFlow, value);
        }

        private decimal _discountedCashFlow;
        public decimal DiscountedCashFlow
        {
            get => _discountedCashFlow;
            set => SetProperty(ref _discountedCashFlow, value);
        }

        private decimal _cumulativeDiscountedCashFlow;
        public decimal CumulativeDiscountedCashFlow
        {
            get => _cumulativeDiscountedCashFlow;
            set => SetProperty(ref _cumulativeDiscountedCashFlow, value);
        }
    }
}
