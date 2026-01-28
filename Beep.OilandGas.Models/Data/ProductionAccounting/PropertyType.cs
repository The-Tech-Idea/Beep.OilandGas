using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum PropertyType
    {
        /// <summary>
        /// Oil and gas lease.
        /// </summary>
        Lease,

        /// <summary>
        /// Concession.
        /// </summary>
        Concession,

        /// <summary>
        /// Fee interest.
        /// </summary>
        FeeInterest,

        /// <summary>
        /// Royalty interest.
        /// </summary>
        RoyaltyInterest,

        /// <summary>
        /// Production payment.
        /// </summary>
        ProductionPayment
    }
}
