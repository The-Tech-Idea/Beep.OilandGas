using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class JournalEntry : ModelEntityBase
    {
        private string JournalEntryIdValue;

        public string JournalEntryId

        {

            get { return this.JournalEntryIdValue; }

            set { SetProperty(ref JournalEntryIdValue, value); }

        }
        private string EntryNumberValue;

        public string EntryNumber

        {

            get { return this.EntryNumberValue; }

            set { SetProperty(ref EntryNumberValue, value); }

        }
        private DateTime? EntryDateValue;

        public DateTime? EntryDate

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
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

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
        private decimal? TotalDebitValue;

        public decimal? TotalDebit

        {

            get { return this.TotalDebitValue; }

            set { SetProperty(ref TotalDebitValue, value); }

        }
        private decimal? TotalCreditValue;

        public decimal? TotalCredit

        {

            get { return this.TotalCreditValue; }

            set { SetProperty(ref TotalCreditValue, value); }

        }
        private List<JournalEntryLine> LinesValue = new List<JournalEntryLine>();

        public List<JournalEntryLine> Lines

        {

            get { return this.LinesValue; }

            set { SetProperty(ref LinesValue, value); }

        }
    }

    public class JournalEntryLine : ModelEntityBase
    {
        private string JournalEntryLineIdValue;

        public string JournalEntryLineId

        {

            get { return this.JournalEntryLineIdValue; }

            set { SetProperty(ref JournalEntryLineIdValue, value); }

        }
        private string JournalEntryIdValue;

        public string JournalEntryId

        {

            get { return this.JournalEntryIdValue; }

            set { SetProperty(ref JournalEntryIdValue, value); }

        }
        private string GlAccountIdValue;

        public string GlAccountId

        {

            get { return this.GlAccountIdValue; }

            set { SetProperty(ref GlAccountIdValue, value); }

        }
        private int? LineNumberValue;

        public int? LineNumber

        {

            get { return this.LineNumberValue; }

            set { SetProperty(ref LineNumberValue, value); }

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
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

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

    public class JournalEntryLineData : ModelEntityBase
    {
        private string GlAccountIdValue;

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
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }



    public class JournalEntryApprovalResult : ModelEntityBase
    {
        private string JournalEntryIdValue;

        public string JournalEntryId

        {

            get { return this.JournalEntryIdValue; }

            set { SetProperty(ref JournalEntryIdValue, value); }

        }
        private bool IsApprovedValue;

        public bool IsApproved

        {

            get { return this.IsApprovedValue; }

            set { SetProperty(ref IsApprovedValue, value); }

        }
        private string ApproverIdValue;

        public string ApproverId

        {

            get { return this.ApproverIdValue; }

            set { SetProperty(ref ApproverIdValue, value); }

        }
        private DateTime ApprovalDateValue = DateTime.UtcNow;

        public DateTime ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}








