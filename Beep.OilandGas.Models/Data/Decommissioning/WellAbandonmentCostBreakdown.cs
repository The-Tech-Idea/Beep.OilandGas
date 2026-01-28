using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellAbandonmentCostBreakdown : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? CostCurrencyValue;

        public string? CostCurrency

        {

            get { return this.CostCurrencyValue; }

            set { SetProperty(ref CostCurrencyValue, value); }

        }
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
