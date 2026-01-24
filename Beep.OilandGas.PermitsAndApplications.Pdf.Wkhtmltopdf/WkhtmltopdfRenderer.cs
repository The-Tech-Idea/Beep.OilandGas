using System;
using System.Diagnostics;
using System.IO;

namespace Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf
{
    public class WkhtmltopdfRenderer
    {
        public void RenderHtmlToPdf(string html, string wkhtmltopdfPath, string outputPdfPath)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new ArgumentException("HTML content is required.", nameof(html));
            if (string.IsNullOrWhiteSpace(wkhtmltopdfPath))
                throw new ArgumentException("wkhtmltopdf path is required.", nameof(wkhtmltopdfPath));
            if (string.IsNullOrWhiteSpace(outputPdfPath))
                throw new ArgumentException("Output PDF path is required.", nameof(outputPdfPath));

            var tempHtmlPath = Path.Combine(Path.GetTempPath(), $"permit-form-{Guid.NewGuid():N}.html");
            File.WriteAllText(tempHtmlPath, html);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPdfPath) ?? ".");

                var startInfo = new ProcessStartInfo
                {
                    FileName = wkhtmltopdfPath,
                    Arguments = $"--quiet \"{tempHtmlPath}\" \"{outputPdfPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                    throw new InvalidOperationException("Failed to start wkhtmltopdf process.");

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    var error = process.StandardError.ReadToEnd();
                    throw new InvalidOperationException($"wkhtmltopdf failed with exit code {process.ExitCode}: {error}");
                }
            }
            finally
            {
                if (File.Exists(tempHtmlPath))
                    File.Delete(tempHtmlPath);
            }
        }
    }
}
