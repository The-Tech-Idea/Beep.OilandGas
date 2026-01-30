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
            RegulatoryAuthority authority,
            PermitApplicationType applicationType,
            DRILLING_PERMIT_APPLICATION? drillingApplication,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmentalApplication,
            INJECTION_PERMIT_APPLICATION? injectionApplication)
        {
            Application = application;
            Attachments = attachments;
            RequiredForms = requiredForms;
            Requirements = requirements;
            RegulatoryAuthority = authority;
            ApplicationType = applicationType;
            DrillingApplication = drillingApplication;
            EnvironmentalApplication = environmentalApplication;
            InjectionApplication = injectionApplication;
        }

        public PERMIT_APPLICATION Application { get; }
        public IReadOnlyList<APPLICATION_ATTACHMENT> Attachments { get; }
        public IReadOnlyList<REQUIRED_FORM> RequiredForms { get; }
        public JURISDICTION_REQUIREMENTS? Requirements { get; }
        public RegulatoryAuthority RegulatoryAuthority { get; }
        public PermitApplicationType ApplicationType { get; }
        public DRILLING_PERMIT_APPLICATION? DrillingApplication { get; }
        public ENVIRONMENTAL_PERMIT_APPLICATION? EnvironmentalApplication { get; }
        public INJECTION_PERMIT_APPLICATION? InjectionApplication { get; }
    }
}
