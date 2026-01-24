using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public class PermitFormRenderContext
    {
        public PermitFormRenderContext(
            PERMIT_APPLICATION application,
            DRILLING_PERMIT_APPLICATION? drillingApplication,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmentalApplication,
            INJECTION_PERMIT_APPLICATION? injectionApplication)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            DrillingApplication = drillingApplication;
            EnvironmentalApplication = environmentalApplication;
            InjectionApplication = injectionApplication;
        }

        public PERMIT_APPLICATION Application { get; }
        public DRILLING_PERMIT_APPLICATION? DrillingApplication { get; }
        public ENVIRONMENTAL_PERMIT_APPLICATION? EnvironmentalApplication { get; }
        public INJECTION_PERMIT_APPLICATION? InjectionApplication { get; }
    }
}
