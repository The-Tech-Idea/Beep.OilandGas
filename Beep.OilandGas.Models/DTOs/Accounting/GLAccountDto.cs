namespace Beep.OilandGas.Models.DTOs.Accounting
{
    public class GLAccountDto
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

    public class CreateGLAccountRequest
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public string ParentAccountId { get; set; }
        public string NormalBalance { get; set; }
        public decimal? OpeningBalance { get; set; }
        public string Description { get; set; }
    }

    public class GLAccountResponse : GLAccountDto
    {
    }
}

