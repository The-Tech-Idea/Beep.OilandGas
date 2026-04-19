using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum CrudeOilType
    {
        /// <summary>
        /// Light crude oil (API gravity > 31.1°)
        /// </summary>
        Light,

        /// <summary>
        /// Medium crude oil (API gravity 22.3° - 31.1°)
        /// </summary>
        Medium,

        /// <summary>
        /// Heavy crude oil (API gravity 10° - 22.3°)
        /// </summary>
        Heavy,

        /// <summary>
        /// Extra heavy crude oil (API gravity < 10°)
        /// </summary>
        ExtraHeavy,

        /// <summary>
        /// Condensate (very light, API gravity > 45°)
        /// </summary>
        Condensate
    }
}
