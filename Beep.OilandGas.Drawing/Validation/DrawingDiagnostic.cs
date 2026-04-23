using System;

namespace Beep.OilandGas.Drawing.Validation
{
    /// <summary>
    /// Represents a single validation diagnostic emitted by a scene or normalized model validator.
    /// </summary>
    public sealed class DrawingDiagnostic
    {
        /// <summary>
        /// Gets the stable diagnostic code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the severity of the diagnostic.
        /// </summary>
        public DrawingDiagnosticSeverity Severity { get; }

        /// <summary>
        /// Gets the human-readable diagnostic message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the source path or field that emitted the diagnostic.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets an optional remediation hint.
        /// </summary>
        public string Suggestion { get; }

        public DrawingDiagnostic(
            string code,
            DrawingDiagnosticSeverity severity,
            string message,
            string source = null,
            string suggestion = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Diagnostic code is required.", nameof(code));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Diagnostic message is required.", nameof(message));

            Code = code.Trim();
            Severity = severity;
            Message = message.Trim();
            Source = source?.Trim();
            Suggestion = suggestion?.Trim();
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Source)
                ? $"[{Code}] {Message}"
                : $"[{Code}] {Source}: {Message}";
        }
    }
}