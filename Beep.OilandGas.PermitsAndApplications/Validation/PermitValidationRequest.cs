using System.Collections.Generic;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Validation
{
    public class PermitValidationRequest
    {
        public PermitValidationRequest(
            PERMIT_APPLICATION application,
            IReadOnlyList<APPLICATION_ATTACHMENT> attachments,
            IReadOnlyList<REQUIRED_FORM> requiredForms,
            JURISDICTION_REQUIREMENTS? requirements,
            string normalizedAuthority,
            string normalizedApplicationType,
            DRILLING_PERMIT_APPLICATION? drillingApplication,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmentalApplication,
            INJECTION_PERMIT_APPLICATION? injectionApplication)
        {
            Application = application;
            Attachments = attachments;
            RequiredForms = requiredForms;
            Requirements = requirements;
            NormalizedAuthority = normalizedAuthority;
            NormalizedApplicationType = normalizedApplicationType;
            DrillingApplication = drillingApplication;
            EnvironmentalApplication = environmentalApplication;
            InjectionApplication = injectionApplication;
        }

        public PERMIT_APPLICATION Application { get; }
        public IReadOnlyList<APPLICATION_ATTACHMENT> Attachments { get; }
        public IReadOnlyList<REQUIRED_FORM> RequiredForms { get; }
        public JURISDICTION_REQUIREMENTS? Requirements { get; }
        public string NormalizedAuthority { get; }
        public string NormalizedApplicationType { get; }
        public DRILLING_PERMIT_APPLICATION? DrillingApplication { get; }
        public ENVIRONMENTAL_PERMIT_APPLICATION? EnvironmentalApplication { get; }
        public INJECTION_PERMIT_APPLICATION? InjectionApplication { get; }
    }
}
