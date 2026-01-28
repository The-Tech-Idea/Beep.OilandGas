using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
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
}
