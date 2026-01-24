using System.Collections.Generic;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public class PermitFormTemplate
    {
        public PermitFormTemplate(
            string formCode,
            string formName,
            string authority,
            string applicationType,
            IReadOnlyList<string> fieldKeys)
        {
            FormCode = formCode;
            FormName = formName;
            Authority = authority;
            ApplicationType = applicationType;
            FieldKeys = fieldKeys ?? new List<string>();
        }

        public string FormCode { get; }
        public string FormName { get; }
        public string Authority { get; }
        public string ApplicationType { get; }
        public IReadOnlyList<string> FieldKeys { get; }
    }
}
