using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CreateInventoryTransactionRequest : ModelEntityBase
    {
        private DateTime TransactionDateValue;

        [Required(ErrorMessage = "TransactionDate is required")]
        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }

        private string TransactionTypeValue = string.Empty;


        [Required(ErrorMessage = "TransactionType is required")]
        public string TransactionType


        {


            get { return this.TransactionTypeValue; }


            set { SetProperty(ref TransactionTypeValue, value); }


        } // IN, OUT, ADJUSTMENT, etc.

        private string ItemIdValue = string.Empty;


        [Required(ErrorMessage = "ItemId is required")]
        public string ItemId


        {


            get { return this.ItemIdValue; }


            set { SetProperty(ref ItemIdValue, value); }


        }

        private decimal QuantityValue;


        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity


        {


            get { return this.QuantityValue; }


            set { SetProperty(ref QuantityValue, value); }


        }

        private decimal? UnitCostValue;


        public decimal? UnitCost


        {


            get { return this.UnitCostValue; }


            set { SetProperty(ref UnitCostValue, value); }


        }
        private string? LocationIdValue;

        public string? LocationId

        {

            get { return this.LocationIdValue; }

            set { SetProperty(ref LocationIdValue, value); }

        }
        private string? ReferenceNumberValue;

        public string? ReferenceNumber

        {

            get { return this.ReferenceNumberValue; }

            set { SetProperty(ref ReferenceNumberValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
