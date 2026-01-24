using System;
using System.Net;
using System.Text;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public class PermitFormHtmlRenderer
    {
        public string Render(PermitFormPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var builder = new StringBuilder();
            builder.AppendLine("<!doctype html>");
            builder.AppendLine("<html lang=\"en\">");
            builder.AppendLine("<head>");
            builder.AppendLine("<meta charset=\"utf-8\" />");
            builder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            builder.AppendLine($"<title>{WebUtility.HtmlEncode(payload.FormName)}</title>");
            builder.AppendLine("<style>");
            builder.AppendLine("body{font-family:Arial,Helvetica,sans-serif;margin:32px;color:#1a1a1a;}");
            builder.AppendLine("h1{font-size:20px;margin-bottom:4px;}");
            builder.AppendLine(".meta{font-size:12px;color:#555;margin-bottom:16px;}");
            builder.AppendLine("table{width:100%;border-collapse:collapse;}");
            builder.AppendLine("th,td{border:1px solid #ddd;padding:8px;text-align:left;font-size:12px;}");
            builder.AppendLine("th{background:#f3f3f3;}");
            builder.AppendLine("</style>");
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            builder.AppendLine($"<h1>{WebUtility.HtmlEncode(payload.FormName)}</h1>");
            builder.AppendLine("<div class=\"meta\">");
            builder.AppendLine($"Form Code: {WebUtility.HtmlEncode(payload.FormCode)}<br/>");
            builder.AppendLine($"Authority: {WebUtility.HtmlEncode(payload.Authority)}<br/>");
            builder.AppendLine($"Application Type: {WebUtility.HtmlEncode(payload.ApplicationType)}<br/>");
            builder.AppendLine($"Generated: {payload.GeneratedOnUtc:yyyy-MM-dd HH:mm:ss} UTC");
            builder.AppendLine("</div>");
            builder.AppendLine("<table>");
            builder.AppendLine("<thead><tr><th>Field</th><th>Value</th></tr></thead>");
            builder.AppendLine("<tbody>");

            foreach (var field in payload.FieldOrder)
            {
                payload.Fields.TryGetValue(field, out var value);
                builder.AppendLine("<tr>");
                builder.AppendLine($"<td>{WebUtility.HtmlEncode(field)}</td>");
                builder.AppendLine($"<td>{WebUtility.HtmlEncode(value ?? string.Empty)}</td>");
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("</tbody>");
            builder.AppendLine("</table>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");

            return builder.ToString();
        }
    }
}
