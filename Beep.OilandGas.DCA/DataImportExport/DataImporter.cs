using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Validation;

namespace Beep.OilandGas.DCA.DataImportExport
{
    /// <summary>
    /// Provides methods for importing production data from various formats.
    /// </summary>
    public static class DataImporter
    {
        /// <summary>
        /// Imports production data from a CSV file.
        /// Expected format: Date,ProductionRate (or Time,ProductionRate)
        /// </summary>
        /// <param name="filePath">Path to the CSV file.</param>
        /// <param name="hasHeader">Whether the CSV file has a header row.</param>
        /// <param name="dateColumnIndex">Index of the date column (0-based).</param>
        /// <param name="productionColumnIndex">Index of the production rate column (0-based).</param>
        /// <returns>Tuple containing (productionData, timeData).</returns>
        /// <exception cref="ArgumentNullException">Thrown when filePath is null.</exception>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when file cannot be read or data is invalid.</exception>
        public static (List<double> productionData, List<DateTime> timeData) ImportFromCsv(
            string filePath,
            bool hasHeader = true,
            int dateColumnIndex = 0,
            int productionColumnIndex = 1)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
            {
                throw new Exceptions.InvalidDataException($"File not found: {filePath}");
            }

            var productionData = new List<double>();
            var timeData = new List<DateTime>();

            try
            {
                var lines = File.ReadAllLines(filePath);
                int startIndex = hasHeader ? 1 : 0;

                for (int i = startIndex; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    var columns = lines[i].Split(',');
                    if (columns.Length <= Math.Max(dateColumnIndex, productionColumnIndex))
                    {
                        throw new Exceptions.InvalidDataException(
                            $"Line {i + 1}: Insufficient columns. Expected at least {Math.Max(dateColumnIndex, productionColumnIndex) + 1} columns.");
                    }

                    // Parse date
                    if (!DateTime.TryParse(columns[dateColumnIndex].Trim(), out DateTime date))
                    {
                        throw new Exceptions.InvalidDataException(
                            $"Line {i + 1}: Invalid date format: {columns[dateColumnIndex]}");
                    }

                    // Parse production rate
                    if (!double.TryParse(columns[productionColumnIndex].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double production))
                    {
                        throw new Exceptions.InvalidDataException(
                            $"Line {i + 1}: Invalid production rate format: {columns[productionColumnIndex]}");
                    }

                    if (production < 0)
                    {
                        throw new Exceptions.InvalidDataException(
                            $"Line {i + 1}: Production rate cannot be negative: {production}");
                    }

                    timeData.Add(date);
                    productionData.Add(production);
                }

                // Validate imported data
                DataValidator.ValidateProductionData(productionData, "imported production data");
                DataValidator.ValidateTimeData(timeData, productionData.Count, "imported time data");

                return (productionData, timeData);
            }
            catch (Exceptions.InvalidDataException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exceptions.InvalidDataException($"Failed to import CSV: {ex.Message}", ex);
            }
        }
    }
}

