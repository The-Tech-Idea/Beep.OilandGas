using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Drawing.Validation
{
    /// <summary>
    /// Aggregates validation diagnostics for drawing scenes and normalized data models.
    /// </summary>
    public sealed class DrawingValidationReport
    {
        private readonly List<DrawingDiagnostic> diagnostics = new();

        /// <summary>
        /// Gets all diagnostics in emission order.
        /// </summary>
        public IReadOnlyList<DrawingDiagnostic> Diagnostics => diagnostics.AsReadOnly();

        /// <summary>
        /// Gets whether the report contains any error diagnostics.
        /// </summary>
        public bool HasErrors => diagnostics.Any(diagnostic => diagnostic.Severity == DrawingDiagnosticSeverity.Error);

        /// <summary>
        /// Gets whether the report contains any warning diagnostics.
        /// </summary>
        public bool HasWarnings => diagnostics.Any(diagnostic => diagnostic.Severity == DrawingDiagnosticSeverity.Warning);

        /// <summary>
        /// Adds a diagnostic to the report.
        /// </summary>
        public void Add(DrawingDiagnostic diagnostic)
        {
            if (diagnostic == null)
                throw new ArgumentNullException(nameof(diagnostic));

            diagnostics.Add(diagnostic);
        }

        public void AddError(string code, string message, string source = null, string suggestion = null)
        {
            Add(new DrawingDiagnostic(code, DrawingDiagnosticSeverity.Error, message, source, suggestion));
        }

        public void AddWarning(string code, string message, string source = null, string suggestion = null)
        {
            Add(new DrawingDiagnostic(code, DrawingDiagnosticSeverity.Warning, message, source, suggestion));
        }

        public void AddInfo(string code, string message, string source = null, string suggestion = null)
        {
            Add(new DrawingDiagnostic(code, DrawingDiagnosticSeverity.Info, message, source, suggestion));
        }

        /// <summary>
        /// Merges another report into this one.
        /// </summary>
        public void Merge(DrawingValidationReport other)
        {
            if (other == null)
                return;

            diagnostics.AddRange(other.diagnostics);
        }

        /// <summary>
        /// Builds a compact summary message for logging or exception text.
        /// </summary>
        public string BuildSummary(int maxDiagnostics = 3)
        {
            if (diagnostics.Count == 0)
                return "No diagnostics.";

            var summaryItems = diagnostics
                .Take(Math.Max(1, maxDiagnostics))
                .Select(diagnostic => diagnostic.ToString());

            var summary = string.Join("; ", summaryItems);
            if (diagnostics.Count > maxDiagnostics)
            {
                summary += $"; plus {diagnostics.Count - maxDiagnostics} more";
            }

            return summary;
        }
    }
}