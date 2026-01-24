using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public class JurisdictionConfig
    {
        public string Authority { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public List<JurisdictionTemplateConfig> Templates { get; set; } = new();

        public IReadOnlyList<PermitFormTemplate> ToTemplates()
        {
            var templates = new List<PermitFormTemplate>();
            foreach (var template in Templates)
            {
                templates.Add(new PermitFormTemplate(
                    template.FormCode ?? string.Empty,
                    template.FormName ?? string.Empty,
                    Authority ?? string.Empty,
                    template.ApplicationType ?? string.Empty,
                    template.FieldKeys ?? new List<string>()));
            }

            return templates;
        }
    }

    public class JurisdictionTemplateConfig
    {
        public string? FormCode { get; set; }
        public string? FormName { get; set; }
        public string? ApplicationType { get; set; }
        public List<string>? FieldKeys { get; set; }
    }
}
