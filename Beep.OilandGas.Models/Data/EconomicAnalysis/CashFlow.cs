
namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    public class CashFlow : ModelEntityBase
    {
        /// <summary>
        /// Time period (0, 1, 2, ...)
        /// </summary>
        private int PeriodValue;

        public int Period

        {

            get { return this.PeriodValue; }

            set { SetProperty(ref PeriodValue, value); }

        }

        /// <summary>
        /// Cash flow amount (positive for inflows, negative for outflows)
        /// </summary>
        private double AmountValue;

        public double Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }

        /// <summary>
        /// Optional description of the cash flow
        /// </summary>
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
