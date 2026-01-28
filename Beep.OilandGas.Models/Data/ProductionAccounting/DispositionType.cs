using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum DispositionType
    {
        /// <summary>
        /// Sale to purchaser.
        /// </summary>
        Sale,

        /// <summary>
        /// Transfer to another location.
        /// </summary>
        Transfer,

        /// <summary>
        /// Exchange transaction.
        /// </summary>
        Exchange,

        /// <summary>
        /// Inventory (not disposed).
        /// </summary>
        Inventory,

        /// <summary>
        /// Royalty in kind.
        /// </summary>
        RoyaltyInKind,

        /// <summary>
        /// Working interest in kind.
        /// </summary>
        WorkingInterestInKind
    }
}
