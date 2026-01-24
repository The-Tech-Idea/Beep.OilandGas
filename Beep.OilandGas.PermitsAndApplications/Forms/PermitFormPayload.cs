using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public class PermitFormPayload
    {
        public string FormCode { get; init; } = string.Empty;
        public string FormName { get; init; } = string.Empty;
        public string Authority { get; init; } = string.Empty;
        public string ApplicationType { get; init; } = string.Empty;
        public DateTime GeneratedOnUtc { get; init; } = DateTime.UtcNow;
        public IReadOnlyList<string> FieldOrder { get; init; } = Array.Empty<string>();
        public IDictionary<string, string> Fields { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
