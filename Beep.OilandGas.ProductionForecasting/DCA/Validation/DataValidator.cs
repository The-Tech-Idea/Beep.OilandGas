using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;

namespace Beep.OilandGas.DCA.Validation
{
    /// <summary>
    /// Provides validation methods for DCA input data.
    /// </summary>
    public static class DataValidator
    {
        /// <summary>
        /// Validates production data array.
        /// </summary>
        /// <param name="productionData">The production data to validate.</param>
        /// <param name="parameterName">The name of the parameter for error messages.</param>
        /// <exception cref="ArgumentNullException">Thrown when productionData is null.</exception>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when productionData is empty or contains invalid values.</exception>
        public static void ValidateProductionData(IEnumerable<double> productionData, string parameterName = "productionData")
        {
            if (productionData == null)
            {
                throw new ArgumentNullException(parameterName, "Production data cannot be null.");
            }

            var dataList = productionData.ToList();
            
            if (dataList.Count < DCAConstants.MinDataPoints)
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} must contain at least {DCAConstants.MinDataPoints} data points. " +
                    $"Provided: {dataList.Count}.");
            }

            if (dataList.Any(q => q < 0))
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} contains negative values. Production rates must be non-negative.");
            }

            if (dataList.All(q => Math.Abs(q - dataList[0]) < DCAConstants.Epsilon))
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} contains constant values. Decline curve analysis requires varying production rates.");
            }
        }

        /// <summary>
        /// Validates time data array and ensures it matches production data length.
        /// </summary>
        /// <param name="timeData">The time data to validate.</param>
        /// <param name="productionDataCount">The count of production data points.</param>
        /// <param name="parameterName">The name of the parameter for error messages.</param>
        /// <exception cref="ArgumentNullException">Thrown when timeData is null.</exception>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when timeData is invalid or doesn't match production data length.</exception>
        public static void ValidateTimeData(IEnumerable<DateTime> timeData, int productionDataCount, string parameterName = "timeData")
        {
            if (timeData == null)
            {
                throw new ArgumentNullException(parameterName, "Time data cannot be null.");
            }

            var timeList = timeData.ToList();

            if (timeList.Count != productionDataCount)
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} count ({timeList.Count}) must match production data count ({productionDataCount}).");
            }

            if (timeList.Count < DCAConstants.MinDataPoints)
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} must contain at least {DCAConstants.MinDataPoints} data points.");
            }

            // Check for chronological order
            for (int i = 1; i < timeList.Count; i++)
            {
                if (timeList[i] <= timeList[i - 1])
                {
                    throw new Exceptions.InvalidDataException(
                        $"{parameterName} must be in chronological order. " +
                        $"Time at index {i} ({timeList[i]}) is not greater than time at index {i - 1} ({timeList[i - 1]}).");
            }
            }
        }

        /// <summary>
        /// Validates initial production rate parameter.
        /// </summary>
        /// <param name="qi">The initial production rate to validate.</param>
        /// <param name="parameterName">The name of the parameter for error messages.</param>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when qi is invalid.</exception>
        public static void ValidateInitialProductionRate(double qi, string parameterName = "qi")
        {
            if (qi <= DCAConstants.MinInitialProductionRate)
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} must be greater than {DCAConstants.MinInitialProductionRate}. " +
                    $"Provided: {qi}.");
            }

            if (double.IsNaN(qi) || double.IsInfinity(qi))
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} must be a finite number. Provided: {qi}.");
            }
        }

        /// <summary>
        /// Validates initial decline rate parameter.
        /// </summary>
        /// <param name="di">The initial decline rate to validate.</param>
        /// <param name="parameterName">The name of the parameter for error messages.</param>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when di is invalid.</exception>
        public static void ValidateInitialDeclineRate(double di, string parameterName = "di")
        {
            if (di <= DCAConstants.MinInitialDeclineRate)
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} must be greater than {DCAConstants.MinInitialDeclineRate}. " +
                    $"Provided: {di}.");
            }

            if (double.IsNaN(di) || double.IsInfinity(di))
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} must be a finite number. Provided: {di}.");
            }
        }

        /// <summary>
        /// Validates decline exponent parameter for hyperbolic decline.
        /// </summary>
        /// <param name="b">The decline exponent to validate.</param>
        /// <param name="parameterName">The name of the parameter for error messages.</param>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when b is invalid.</exception>
        public static void ValidateDeclineExponent(double b, string parameterName = "b")
        {
            if (b < DCAConstants.MinDeclineExponent || b > DCAConstants.MaxDeclineExponent)
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} must be between {DCAConstants.MinDeclineExponent} and {DCAConstants.MaxDeclineExponent}. " +
                    $"Provided: {b}.");
            }

            if (double.IsNaN(b) || double.IsInfinity(b))
            {
                throw new Exceptions.InvalidDataException(
                    $"{parameterName} must be a finite number. Provided: {b}.");
            }
        }

        /// <summary>
        /// Validates that production and time data arrays have matching lengths.
        /// </summary>
        /// <param name="productionData">The production data.</param>
        /// <param name="timeData">The time data.</param>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when arrays have mismatched lengths.</exception>
        public static void ValidateDataLengthsMatch(IEnumerable<double> productionData, IEnumerable<DateTime> timeData)
        {
            var prodList = productionData?.ToList() ?? new List<double>();
            var timeList = timeData?.ToList() ?? new List<DateTime>();

            if (prodList.Count != timeList.Count)
            {
                throw new Exceptions.InvalidDataException(
                    $"Production data count ({prodList.Count}) must match time data count ({timeList.Count}).");
            }
        }
    }
}

