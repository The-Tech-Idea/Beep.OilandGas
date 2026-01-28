using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum LeaseType
    {
        /// <summary>
        /// Fee (private) mineral estate lease.
        /// </summary>
        Fee,

        /// <summary>
        /// Government lease (federal, state, or local).
        /// </summary>
        Government,

        /// <summary>
        /// Net profit interest lease.
        /// </summary>
        NetProfit,

        /// <summary>
        /// Joint interest lease (joint operating agreement).
        /// </summary>
        JointInterest
    }
}
