namespace Beep.OilandGas.Models.Enums
{
    /// <summary>
    /// Status of an accounts receivable invoice.
    /// </summary>
    public enum ReceivableStatus
    {
        /// <summary>
        /// Invoice is open and unpaid.
        /// </summary>
        Open,

        /// <summary>
        /// Invoice has been partially paid.
        /// </summary>
        PartiallyPaid,

        /// <summary>
        /// Invoice has been fully paid.
        /// </summary>
        Paid,

        /// <summary>
        /// Invoice is past due date.
        /// </summary>
        Overdue,

        /// <summary>
        /// Invoice has been written off as uncollectable.
        /// </summary>
        WrittenOff
    }
}

