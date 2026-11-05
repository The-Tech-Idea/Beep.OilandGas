using System.Collections.Generic;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;

namespace Beep.OilandGas.PermitsAndApplications.Validation
{
    public class PermitValidationRequest
    {
        public PermitValidationRequest(
            PERMIT_APPLICATION application,
            IReadOnlyList<APPLICATION_ATTACHMENT> attachments,
            IReadOnlyList<REQUIRED_FORM> requiredForms,
            JURISDICTION_REQUIREMENTS? requirements,
            DRILLING_PERMIT_APPLICATION? drillingApplication,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmentalApplication,
            INJECTION_PERMIT_APPLICATION? injectionApplication)
        {
            TheTechIdea.Data.OilGas.PermitApplicationSupportingRecords.RequireParent(application, drillingApplication, environmentalApplication, injectionApplication);
            Application = application;
            Attachments = attachments;
            RequiredForms = requiredForms;
            Requirements = requirements;
            DrillingApplication = drillingApplication;
            EnvironmentalApplication = environmentalApplication;
            InjectionApplication = injectionApplication;
        }

        public PERMIT_APPLICATION Application { get; }
        public IReadOnlyList<APPLICATION_ATTACHMENT> Attachments { get; }
        public IReadOnlyList<REQUIRED_FORM> RequiredForms { get; }
        public JURISDICTION_REQUIREMENTS? Requirements { get; }
        public RegulatoryAuthority RegulatoryAuthority => Application.REGULATORY_AUTHORITY;
        public PermitApplicationType ApplicationType => Application.APPLICATION_TYPE;
        public DRILLING_PERMIT_APPLICATION? DrillingApplication { get; }
        public ENVIRONMENTAL_PERMIT_APPLICATION? EnvironmentalApplication { get; }
        public INJECTION_PERMIT_APPLICATION? InjectionApplication { get; }
    }
}
