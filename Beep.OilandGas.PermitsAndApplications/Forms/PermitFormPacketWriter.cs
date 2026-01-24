using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public class PermitFormPacketWriter
    {
        private readonly PermitFormHtmlRenderer _htmlRenderer = new();

        public IReadOnlyList<FormPacketFile> WritePackets(
            IEnumerable<PermitFormPayload> payloads,
            string outputDirectory,
            string applicationId,
            string authority,
            string applicationType)
        {
            if (payloads == null)
                throw new ArgumentNullException(nameof(payloads));
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentException("Output directory is required.", nameof(outputDirectory));
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentException("Application ID is required.", nameof(applicationId));

            var storagePath = Path.Combine(outputDirectory, applicationId);
            Directory.CreateDirectory(storagePath);

            var files = new List<FormPacketFile>();
            foreach (var payload in payloads)
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                var baseName = $"{applicationId}_{authority}_{applicationType}_{payload.FormCode}_{timestamp}";

                var jsonPath = Path.Combine(storagePath, $"{baseName}.json");
                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonPath, json);
                files.Add(new FormPacketFile(jsonPath, "application/json", payload.FormCode, "Form packet JSON"));

                var htmlPath = Path.Combine(storagePath, $"{baseName}.html");
                var html = _htmlRenderer.Render(payload);
                File.WriteAllText(htmlPath, html);
                files.Add(new FormPacketFile(htmlPath, "text/html", payload.FormCode, "PDF-ready HTML packet"));
            }

            return files;
        }
    }

    public record FormPacketFile(string Path, string ContentType, string FormCode, string Description);
}
