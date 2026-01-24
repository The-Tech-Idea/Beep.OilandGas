using System;
using System.Collections.Generic;
using System.IO;
using Beep.OilandGas.PermitsAndApplications.Forms;

namespace Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf
{
    public class WkhtmltopdfPacketWriter
    {
        private readonly PermitFormHtmlRenderer _htmlRenderer = new();
        private readonly WkhtmltopdfRenderer _renderer = new();

        public IReadOnlyList<FormPacketFile> WritePackets(
            IEnumerable<PermitFormPayload> payloads,
            string outputDirectory,
            string applicationId,
            string authority,
            string applicationType,
            string wkhtmltopdfPath)
        {
            if (payloads == null)
                throw new ArgumentNullException(nameof(payloads));
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentException("Output directory is required.", nameof(outputDirectory));
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentException("Application ID is required.", nameof(applicationId));
            if (string.IsNullOrWhiteSpace(wkhtmltopdfPath))
                throw new ArgumentException("wkhtmltopdf path is required.", nameof(wkhtmltopdfPath));

            var storagePath = Path.Combine(outputDirectory, applicationId);
            Directory.CreateDirectory(storagePath);

            var files = new List<FormPacketFile>();
            foreach (var payload in payloads)
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                var baseName = $"{applicationId}_{authority}_{applicationType}_{payload.FormCode}_{timestamp}";
                var htmlPath = Path.Combine(storagePath, $"{baseName}.html");
                var pdfPath = Path.Combine(storagePath, $"{baseName}.pdf");

                var html = _htmlRenderer.Render(payload);
                File.WriteAllText(htmlPath, html);
                files.Add(new FormPacketFile(htmlPath, "text/html", payload.FormCode, "HTML packet"));

                _renderer.RenderHtmlToPdf(html, wkhtmltopdfPath, pdfPath);
                files.Add(new FormPacketFile(pdfPath, "application/pdf", payload.FormCode, "PDF packet"));
            }

            return files;
        }
    }
}
