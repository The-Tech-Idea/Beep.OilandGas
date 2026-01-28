using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class AllocationEntityRequest : ModelEntityBase
    {
        private string EntityIdValue = string.Empty;

        [Required]
        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string? EntityNameValue;

        public string? EntityName

        {

            get { return this.EntityNameValue; }

            set { SetProperty(ref EntityNameValue, value); }

        }
        private decimal? WorkingInterestValue;

        public decimal? WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal? NetRevenueInterestValue;

        public decimal? NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private decimal? ProductionHistoryValue;

        public decimal? ProductionHistory

        {

            get { return this.ProductionHistoryValue; }

            set { SetProperty(ref ProductionHistoryValue, value); }

        }
    }
}
