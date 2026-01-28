using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class CreateJournalEntryRequest : ModelEntityBase
    {
        private string EntryNumberValue;

        public string EntryNumber

        {

            get { return this.EntryNumberValue; }

            set { SetProperty(ref EntryNumberValue, value); }

        }
        private DateTime EntryDateValue;

        public DateTime EntryDate

        {

            get { return this.EntryDateValue; }

            set { SetProperty(ref EntryDateValue, value); }

        }
        private string EntryTypeValue;

        public string EntryType

        {

            get { return this.EntryTypeValue; }

            set { SetProperty(ref EntryTypeValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string ReferenceNumberValue;

        public string ReferenceNumber

        {

            get { return this.ReferenceNumberValue; }

            set { SetProperty(ref ReferenceNumberValue, value); }

        }
        private string SourceModuleValue;

        public string SourceModule

        {

            get { return this.SourceModuleValue; }

            set { SetProperty(ref SourceModuleValue, value); }

        }
        private List<JournalEntryLineData> LinesValue = new List<JournalEntryLineData>();

        public List<JournalEntryLineData> Lines

        {

            get { return this.LinesValue; }

            set { SetProperty(ref LinesValue, value); }

        }
    }
}
