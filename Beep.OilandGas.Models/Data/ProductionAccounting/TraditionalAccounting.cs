using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Request to create a journal entry
    /// </summary>
    public class CreateJournalEntryRequest : ModelEntityBase
    {
        private string? EntryNumberValue;

        public string? EntryNumber

        {

            get { return this.EntryNumberValue; }

            set { SetProperty(ref EntryNumberValue, value); }

        }

        private DateTime? EntryDateValue;


        [Required(ErrorMessage = "EntryDate is required")]
        public DateTime? EntryDate


        {


            get { return this.EntryDateValue; }


            set { SetProperty(ref EntryDateValue, value); }


        }

        private string? EntryTypeValue;


        public string? EntryType


        {


            get { return this.EntryTypeValue; }


            set { SetProperty(ref EntryTypeValue, value); }


        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }

        private List<JournalEntryLineRequest> LinesValue = new();


        [Required(ErrorMessage = "At least one line is required")]
        [MinLength(1, ErrorMessage = "At least one journal entry line is required")]
        public List<JournalEntryLineRequest> Lines


        {


            get { return this.LinesValue; }


            set { SetProperty(ref LinesValue, value); }


        }
    }

    /// <summary>
    /// Request for a journal entry line
    /// </summary>
    public class JournalEntryLineRequest : ModelEntityBase
    {
        private string GlAccountIdValue = string.Empty;

        [Required(ErrorMessage = "GlAccountId is required")]
        public string GlAccountId

        {

            get { return this.GlAccountIdValue; }

            set { SetProperty(ref GlAccountIdValue, value); }

        }

        private decimal? DebitAmountValue;


        public decimal? DebitAmount


        {


            get { return this.DebitAmountValue; }


            set { SetProperty(ref DebitAmountValue, value); }


        }
        private decimal? CreditAmountValue;

        public decimal? CreditAmount

        {

            get { return this.CreditAmountValue; }

            set { SetProperty(ref CreditAmountValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    /// <summary>
    /// Request to create an inventory transaction
    /// </summary>
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







