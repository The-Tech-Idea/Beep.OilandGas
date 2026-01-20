using System;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class GLAccount : ModelEntityBase
    {
        public string GlAccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public string ParentAccountId { get; set; }
        public string NormalBalance { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string Description { get; set; }
        public string ActiveInd { get; set; }
    }

    public class CreateGLAccountRequest : ModelEntityBase
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public string ParentAccountId { get; set; }
        public string NormalBalance { get; set; }
        public decimal? OpeningBalance { get; set; }
        public string Description { get; set; }
    }

    public class GLAccountResponse : GLAccount
    {
    }

    public class UpdateGLAccountRequest : ModelEntityBase
    {
        public string GlAccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public string ParentAccountId { get; set; }
        public string NormalBalance { get; set; }
        public decimal? OpeningBalance { get; set; }
        public string Description { get; set; }
    }

    public class AccountBalanceSummary : ModelEntityBase
    {
        public string GlAccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? DebitTotal { get; set; }
        public decimal? CreditTotal { get; set; }
        public decimal? CurrentBalance { get; set; }
        public decimal? Difference { get; set; }
        public DateTime? AsOfDate { get; set; }
    }

    public class AccountReconciliationResult : ModelEntityBase
    {
        public string ReconciliationId { get; set; } = Guid.NewGuid().ToString();
        public string GlAccountId { get; set; }
        public DateTime ReconciliationDate { get; set; }
        public decimal? BookBalance { get; set; }
        public decimal? ReconciledBalance { get; set; }
        public decimal? Difference { get; set; }
        public bool IsReconciled { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
    }
}





