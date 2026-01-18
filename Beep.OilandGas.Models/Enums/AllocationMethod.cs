namespace Beep.OilandGas.Models.Enums
{
    /// <summary>
    /// Enumeration of production allocation methods used in allocation calculations.
    /// </summary>
    public enum AllocationMethod
    {
        /// <summary>
        /// Pro Rata allocation based on percentage ownership.
        /// </summary>
        ProRata = 0,

        /// <summary>
        /// Equation-based allocation using custom formulas.
        /// </summary>
        Equation = 1,

        /// <summary>
        /// Volumetric allocation based on volumes.
        /// </summary>
        Volumetric = 2,

        /// <summary>
        /// Yield allocation based on well yield/performance.
        /// </summary>
        Yield = 3,

        /// <summary>
        /// Custom allocation method specified by user.
        /// </summary>
        Custom = 4
    }
}
