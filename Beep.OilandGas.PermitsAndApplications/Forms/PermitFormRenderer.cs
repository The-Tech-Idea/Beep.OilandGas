using System;
using System.Collections.Generic;
using System.Globalization;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public class PermitFormRenderer
    {
        public IDictionary<string, string> Render(PERMIT_APPLICATION application, PermitFormTemplate template)
        {
            var context = new PermitFormRenderContext(application, null, null, null);
            return Render(context, template);
        }

        public IDictionary<string, string> Render(
            PermitFormRenderContext context,
            PermitFormTemplate template)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in template.FieldKeys)
            {
                if (TryResolveField(context, key, out var value))
                {
                    data[key] = value ?? string.Empty;
                }
            }

            return data;
        }

        private static bool TryResolveField(PermitFormRenderContext context, string key, out string? value) =>
            TheTechIdea.Data.OilGas.PermitApplicationSupportingRecords.TryResolve(context.Application,
                context.DrillingApplication, context.EnvironmentalApplication, context.InjectionApplication, key, out value);
    }
}
