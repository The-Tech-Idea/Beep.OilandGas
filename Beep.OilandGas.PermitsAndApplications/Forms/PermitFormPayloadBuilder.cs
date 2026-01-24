using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public class PermitFormPayloadBuilder
    {
        private readonly PermitFormTemplateRegistry _registry;
        private readonly PermitFormRenderer _renderer;

        public PermitFormPayloadBuilder()
        {
            _registry = new PermitFormTemplateRegistry();
            _renderer = new PermitFormRenderer();
        }

        public PermitFormPayloadBuilder(PermitFormTemplateRegistry registry, PermitFormRenderer renderer)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public IReadOnlyList<PermitFormPayload> BuildPayloads(
            PermitFormRenderContext context,
            string authority,
            string applicationType,
            string? configDirectory = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!string.IsNullOrWhiteSpace(configDirectory))
            {
                _registry.LoadFromDirectory(configDirectory);
            }

            var templates = _registry.GetTemplates(authority, applicationType);
            var payloads = new List<PermitFormPayload>();

            foreach (var template in templates)
            {
                var fields = _renderer.Render(context, template);
                payloads.Add(new PermitFormPayload
                {
                    FormCode = template.FormCode,
                    FormName = template.FormName,
                    Authority = template.Authority,
                    ApplicationType = template.ApplicationType,
                    GeneratedOnUtc = DateTime.UtcNow,
                    FieldOrder = template.FieldKeys,
                    Fields = fields
                });
            }

            return payloads;
        }

        public string BuildJsonPayload(
            PermitFormRenderContext context,
            string authority,
            string applicationType,
            string? configDirectory = null)
        {
            var payloads = BuildPayloads(context, authority, applicationType, configDirectory);
            return JsonSerializer.Serialize(payloads, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}
