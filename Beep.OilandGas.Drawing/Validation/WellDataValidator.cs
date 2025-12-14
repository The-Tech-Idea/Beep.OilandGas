using System;
using Beep.OilandGas.Models;

namespace Beep.OilandGas.Drawing.Validation
{
    /// <summary>
    /// Provides validation for well data in the drawing framework.
    /// This is a simplified version that delegates to Beep.WellSchematics.Validation if available.
    /// </summary>
    public static class WellDataValidator
    {
        /// <summary>
        /// Validates well data structure.
        /// </summary>
        /// <param name="wellData">The well data to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when wellData is null.</exception>
        /// <exception cref="ArgumentException">Thrown when well data is invalid.</exception>
        public static void ValidateWellData(WellData wellData)
        {
            if (wellData == null)
                throw new ArgumentNullException(nameof(wellData), "Well data cannot be null.");

            if (wellData.BoreHoles == null)
                throw new ArgumentException("BoreHoles collection cannot be null.", nameof(wellData));

            if (wellData.BoreHoles.Count == 0)
                throw new ArgumentException("Well data must contain at least one borehole.", nameof(wellData));

            for (int i = 0; i < wellData.BoreHoles.Count; i++)
            {
                var borehole = wellData.BoreHoles[i];
                if (borehole == null)
                    throw new ArgumentException($"Borehole at index {i} cannot be null.", nameof(wellData));

                if (borehole.BottomDepth <= borehole.TopDepth)
                    throw new ArgumentException(
                        $"Borehole at index {i}: BottomDepth ({borehole.BottomDepth}) must be greater than TopDepth ({borehole.TopDepth}).",
                        nameof(wellData));
            }
        }
    }
}

