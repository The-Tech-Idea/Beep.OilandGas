
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum TaxWithholdingType
    {
        /// <summary>
        /// Invalid tax ID withholding.
        /// </summary>
        InvalidTaxId,

        /// <summary>
        /// Out of state withholding.
        /// </summary>
        OutOfState,

        /// <summary>
        /// Alien withholding.
        /// </summary>
        Alien,

        /// <summary>
        /// Backup withholding.
        /// </summary>
        BackupWithholding
    }
}
