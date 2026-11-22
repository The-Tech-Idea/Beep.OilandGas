using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The run columns of a supplier payment (ACC-05): the run it belongs to, and the state it reached.
    /// </summary>
    /// <remarks>
    /// <c>AP_PAYMENT</c> is one amount against one invoice - which is what an allocation of a run is - so the run header
    /// was all it lacked. <c>PAYMENT_STATUS</c> already exists and is left to mean what the owner's service gives it; the
    /// run's own state lives on the header.
    /// </remarks>
    public partial class AP_PAYMENT
    {
        /// <summary>The run that settles this invoice, or null for a payment recorded on its own.</summary>
        [MaxLength(64)]
        public string? RUN_ID
        {
            get => RunValue;
            set => SetProperty(ref RunValue, value);
        }

        private string? RunValue;
    }
}
