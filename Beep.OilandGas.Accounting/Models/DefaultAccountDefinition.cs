namespace Beep.OilandGas.Accounting.Models
{
    public class DefaultAccountDefinition
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; } // ASSET, LIABILITY, EQUITY, REVENUE, EXPENSE
        public string NormalBalance { get; set; } // DEBIT, CREDIT
        public string Description { get; set; }

        public DefaultAccountDefinition(string number, string name, string type, string balance, string description)
        {
            AccountNumber = number;
            AccountName = name;
            AccountType = type;
            NormalBalance = balance;
            Description = description;
        }
    }
}
