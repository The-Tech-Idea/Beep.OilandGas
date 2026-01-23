using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class GLAccount : ModelEntityBase
    {
        private string GlAccountIdValue;

        public string GlAccountId

        {

            get { return this.GlAccountIdValue; }

            set { SetProperty(ref GlAccountIdValue, value); }

        }
        private string AccountNumberValue;

        public string AccountNumber

        {

            get { return this.AccountNumberValue; }

            set { SetProperty(ref AccountNumberValue, value); }

        }
        private string AccountNameValue;

        public string AccountName

        {

            get { return this.AccountNameValue; }

            set { SetProperty(ref AccountNameValue, value); }

        }
        private string AccountTypeValue;

        public string AccountType

        {

            get { return this.AccountTypeValue; }

            set { SetProperty(ref AccountTypeValue, value); }

        }
        private string ParentAccountIdValue;

        public string ParentAccountId

        {

            get { return this.ParentAccountIdValue; }

            set { SetProperty(ref ParentAccountIdValue, value); }

        }
        private string NormalBalanceValue;

        public string NormalBalance

        {

            get { return this.NormalBalanceValue; }

            set { SetProperty(ref NormalBalanceValue, value); }

        }
        private decimal? OpeningBalanceValue;

        public decimal? OpeningBalance

        {

            get { return this.OpeningBalanceValue; }

            set { SetProperty(ref OpeningBalanceValue, value); }

        }
        private decimal? CurrentBalanceValue;

        public decimal? CurrentBalance

        {

            get { return this.CurrentBalanceValue; }

            set { SetProperty(ref CurrentBalanceValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string ActiveIndValue;

        public string ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }
    }

    public class CreateGLAccountRequest : ModelEntityBase
    {
        private string AccountNumberValue;

        public string AccountNumber

        {

            get { return this.AccountNumberValue; }

            set { SetProperty(ref AccountNumberValue, value); }

        }
        private string AccountNameValue;

        public string AccountName

        {

            get { return this.AccountNameValue; }

            set { SetProperty(ref AccountNameValue, value); }

        }
        private string AccountTypeValue;

        public string AccountType

        {

            get { return this.AccountTypeValue; }

            set { SetProperty(ref AccountTypeValue, value); }

        }
        private string ParentAccountIdValue;

        public string ParentAccountId

        {

            get { return this.ParentAccountIdValue; }

            set { SetProperty(ref ParentAccountIdValue, value); }

        }
        private string NormalBalanceValue;

        public string NormalBalance

        {

            get { return this.NormalBalanceValue; }

            set { SetProperty(ref NormalBalanceValue, value); }

        }
        private decimal? OpeningBalanceValue;

        public decimal? OpeningBalance

        {

            get { return this.OpeningBalanceValue; }

            set { SetProperty(ref OpeningBalanceValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class GLAccountResponse : GLAccount
    {
    }

    public class UpdateGLAccountRequest : ModelEntityBase
    {
        private string GlAccountIdValue;

        public string GlAccountId

        {

            get { return this.GlAccountIdValue; }

            set { SetProperty(ref GlAccountIdValue, value); }

        }
        private string AccountNumberValue;

        public string AccountNumber

        {

            get { return this.AccountNumberValue; }

            set { SetProperty(ref AccountNumberValue, value); }

        }
        private string AccountNameValue;

        public string AccountName

        {

            get { return this.AccountNameValue; }

            set { SetProperty(ref AccountNameValue, value); }

        }
        private string AccountTypeValue;

        public string AccountType

        {

            get { return this.AccountTypeValue; }

            set { SetProperty(ref AccountTypeValue, value); }

        }
        private string ParentAccountIdValue;

        public string ParentAccountId

        {

            get { return this.ParentAccountIdValue; }

            set { SetProperty(ref ParentAccountIdValue, value); }

        }
        private string NormalBalanceValue;

        public string NormalBalance

        {

            get { return this.NormalBalanceValue; }

            set { SetProperty(ref NormalBalanceValue, value); }

        }
        private decimal? OpeningBalanceValue;

        public decimal? OpeningBalance

        {

            get { return this.OpeningBalanceValue; }

            set { SetProperty(ref OpeningBalanceValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class AccountBalanceSummary : ModelEntityBase
    {
        private string GlAccountIdValue;

        public string GlAccountId

        {

            get { return this.GlAccountIdValue; }

            set { SetProperty(ref GlAccountIdValue, value); }

        }
        private string AccountNumberValue;

        public string AccountNumber

        {

            get { return this.AccountNumberValue; }

            set { SetProperty(ref AccountNumberValue, value); }

        }
        private string AccountNameValue;

        public string AccountName

        {

            get { return this.AccountNameValue; }

            set { SetProperty(ref AccountNameValue, value); }

        }
        private decimal? OpeningBalanceValue;

        public decimal? OpeningBalance

        {

            get { return this.OpeningBalanceValue; }

            set { SetProperty(ref OpeningBalanceValue, value); }

        }
        private decimal? DebitTotalValue;

        public decimal? DebitTotal

        {

            get { return this.DebitTotalValue; }

            set { SetProperty(ref DebitTotalValue, value); }

        }
        private decimal? CreditTotalValue;

        public decimal? CreditTotal

        {

            get { return this.CreditTotalValue; }

            set { SetProperty(ref CreditTotalValue, value); }

        }
        private decimal? CurrentBalanceValue;

        public decimal? CurrentBalance

        {

            get { return this.CurrentBalanceValue; }

            set { SetProperty(ref CurrentBalanceValue, value); }

        }
        private decimal? DifferenceValue;

        public decimal? Difference

        {

            get { return this.DifferenceValue; }

            set { SetProperty(ref DifferenceValue, value); }

        }
        private DateTime? AsOfDateValue;

        public DateTime? AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
    }

    public class AccountReconciliationResult : ModelEntityBase
    {
        private string ReconciliationIdValue = Guid.NewGuid().ToString();

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private string GlAccountIdValue;

        public string GlAccountId

        {

            get { return this.GlAccountIdValue; }

            set { SetProperty(ref GlAccountIdValue, value); }

        }
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private decimal? BookBalanceValue;

        public decimal? BookBalance

        {

            get { return this.BookBalanceValue; }

            set { SetProperty(ref BookBalanceValue, value); }

        }
        private decimal? ReconciledBalanceValue;

        public decimal? ReconciledBalance

        {

            get { return this.ReconciledBalanceValue; }

            set { SetProperty(ref ReconciledBalanceValue, value); }

        }
        private decimal? DifferenceValue;

        public decimal? Difference

        {

            get { return this.DifferenceValue; }

            set { SetProperty(ref DifferenceValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string UserIdValue;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}








