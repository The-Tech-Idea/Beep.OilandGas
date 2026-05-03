using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Results;

namespace Beep.OilandGas.DCA.DataImportExport
{
    /// <summary>
    /// Provides methods for exporting DCA results to various formats.
    /// </summary>
    public static class DataExporter
    {
        /// <summary>
        /// Exports fit results to CSV format.
        /// </summary>
        /// <param name="result">The DCA fit result to export.</param>
        /// <param name="filePath">Path to the output CSV file.</param>
        /// <param name="includeStatistics">Whether to include statistical metrics in the export.</param>
        /// <exception cref="ArgumentNullException">Thrown when result or filePath is null.</exception>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when file path is invalid.</exception>
        public static void ExportToCsv(DCAFitResult result, string filePath, bool includeStatistics = true)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write header
                    writer.WriteLine("Index,Observed,Predicted,Residual");

                    // Write data rows
                    for (int i = 0; i < result.ObservedValues.Length; i++)
                    {
                        writer.WriteLine($"{i},{result.ObservedValues[i].ToString(CultureInfo.InvariantCulture)}," +
                                       $"{result.PredictedValues[i].ToString(CultureInfo.InvariantCulture)}," +
                                       $"{result.Residuals[i].ToString(CultureInfo.InvariantCulture)}");
                    }

                    // Write statistics if requested
                    if (includeStatistics)
                    {
                        writer.WriteLine();
                        writer.WriteLine("Statistics");
                        writer.WriteLine($"R²,{result.RSquared.ToString(CultureInfo.InvariantCulture)}");
                        writer.WriteLine($"Adjusted R²,{result.AdjustedRSquared.ToString(CultureInfo.InvariantCulture)}");
                        writer.WriteLine($"RMSE,{result.RMSE.ToString(CultureInfo.InvariantCulture)}");
                        writer.WriteLine($"MAE,{result.MAE.ToString(CultureInfo.InvariantCulture)}");
                        writer.WriteLine($"AIC,{result.AIC.ToString(CultureInfo.InvariantCulture)}");
                        writer.WriteLine($"BIC,{result.BIC.ToString(CultureInfo.InvariantCulture)}");
                        writer.WriteLine($"Converged,{result.Converged}");
                        writer.WriteLine($"Iterations,{result.Iterations}");

                        writer.WriteLine();
                        writer.WriteLine("Parameters");
                        for (int i = 0; i < result.Parameters.Length; i++)
                        {
                            var ci = result.ConfidenceIntervals[i];
                            writer.WriteLine($"Parameter {i},{result.Parameters[i].ToString(CultureInfo.InvariantCulture)}," +
                                           $"{ci.lowerBound.ToString(CultureInfo.InvariantCulture)}," +
                                           $"{ci.upperBound.ToString(CultureInfo.InvariantCulture)}");
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is ArgumentNullException || ex is Exceptions.InvalidDataException))
            {
                throw new Exceptions.InvalidDataException($"Failed to export to CSV: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exports fit results to JSON format.
        /// </summary>
        /// <param name="result">The DCA fit result to export.</param>
        /// <param name="filePath">Path to the output JSON file.</param>
        /// <exception cref="ArgumentNullException">Thrown when result or filePath is null.</exception>
        public static void ExportToJson(DCAFitResult result, string filePath)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"  \"rSquared\": {result.RSquared.ToString(CultureInfo.InvariantCulture)},");
            json.AppendLine($"  \"adjustedRSquared\": {result.AdjustedRSquared.ToString(CultureInfo.InvariantCulture)},");
            json.AppendLine($"  \"rmse\": {result.RMSE.ToString(CultureInfo.InvariantCulture)},");
            json.AppendLine($"  \"mae\": {result.MAE.ToString(CultureInfo.InvariantCulture)},");
            json.AppendLine($"  \"aic\": {result.AIC.ToString(CultureInfo.InvariantCulture)},");
            json.AppendLine($"  \"bic\": {result.BIC.ToString(CultureInfo.InvariantCulture)},");
            json.AppendLine($"  \"converged\": {result.Converged.ToString().ToLower()},");
            json.AppendLine($"  \"iterations\": {result.Iterations},");
            json.AppendLine($"  \"parameters\": [{string.Join(", ", result.Parameters.Select(p => p.ToString(CultureInfo.InvariantCulture)))}],");
            json.AppendLine($"  \"observedValues\": [{string.Join(", ", result.ObservedValues.Select(v => v.ToString(CultureInfo.InvariantCulture)))}],");
            json.AppendLine($"  \"predictedValues\": [{string.Join(", ", result.PredictedValues.Select(v => v.ToString(CultureInfo.InvariantCulture)))}],");
            json.AppendLine($"  \"residuals\": [{string.Join(", ", result.Residuals.Select(r => r.ToString(CultureInfo.InvariantCulture)))}]");
            json.AppendLine("}");

            try
            {
                File.WriteAllText(filePath, json.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exceptions.InvalidDataException($"Failed to export to JSON: {ex.Message}", ex);
            }
        }
    }
}

