namespace Beep.OilandGas.PermitsAndApplications.Constants
{
    /// <summary>
    /// Constants for permit and application management.
    /// </summary>
    public static class PermitConstants
    {
        // United States - Texas
        /// <summary>
        /// RRC (Railroad Commission of Texas) identifier.
        /// </summary>
        public const string RRC_AUTHORITY = "RRC";

        /// <summary>
        /// TCEQ (Texas Commission on Environmental Quality) identifier.
        /// </summary>
        public const string TCEQ_AUTHORITY = "TCEQ";

        // United States - Other States
        /// <summary>
        /// AOGCC (Alaska Oil and Gas Conservation Commission) identifier.
        /// </summary>
        public const string AOGCC_AUTHORITY = "AOGCC";

        /// <summary>
        /// NDIC (North Dakota Industrial Commission) identifier.
        /// </summary>
        public const string NDIC_AUTHORITY = "NDIC";

        /// <summary>
        /// WOGCC (Wyoming Oil and Gas Conservation Commission) identifier.
        /// </summary>
        public const string WOGCC_AUTHORITY = "WOGCC";

        /// <summary>
        /// COGCC (Colorado Oil and Gas Conservation Commission) identifier.
        /// </summary>
        public const string COGCC_AUTHORITY = "COGCC";

        /// <summary>
        /// OCC (Oklahoma Corporation Commission) identifier.
        /// </summary>
        public const string OCC_AUTHORITY = "OCC";

        /// <summary>
        /// LADNR (Louisiana Department of Natural Resources) identifier.
        /// </summary>
        public const string LADNR_AUTHORITY = "LADNR";

        /// <summary>
        /// NMOCD (New Mexico Oil Conservation Division) identifier.
        /// </summary>
        public const string NMOCD_AUTHORITY = "NMOCD";

        /// <summary>
        /// CEC (California Energy Commission) identifier.
        /// </summary>
        public const string CEC_AUTHORITY = "CEC";

        // United States - Federal
        /// <summary>
        /// BLM (Bureau of Land Management) identifier.
        /// </summary>
        public const string BLM_AUTHORITY = "BLM";

        /// <summary>
        /// USACE (U.S. Army Corps of Engineers) identifier.
        /// </summary>
        public const string USACE_AUTHORITY = "USACE";

        /// <summary>
        /// EPA (Environmental Protection Agency) identifier.
        /// </summary>
        public const string EPA_AUTHORITY = "EPA";

        /// <summary>
        /// BOEM (Bureau of Ocean Energy Management) identifier.
        /// </summary>
        public const string BOEM_AUTHORITY = "BOEM";

        /// <summary>
        /// BSEE (Bureau of Safety and Environmental Enforcement) identifier.
        /// </summary>
        public const string BSEE_AUTHORITY = "BSEE";

        // Canada
        /// <summary>
        /// AER (Alberta Energy Regulator) identifier.
        /// </summary>
        public const string AER_AUTHORITY = "AER";

        /// <summary>
        /// BCER (British Columbia Energy Regulator) identifier.
        /// </summary>
        public const string BCER_AUTHORITY = "BCER";

        /// <summary>
        /// SER (Saskatchewan Energy and Resources) identifier.
        /// </summary>
        public const string SER_AUTHORITY = "SER";

        /// <summary>
        /// NLDET (Newfoundland and Labrador Department of Industry, Energy and Technology) identifier.
        /// </summary>
        public const string NLDET_AUTHORITY = "NLDET";

        // Mexico
        /// <summary>
        /// CNH (Comisión Nacional de Hidrocarburos) identifier.
        /// </summary>
        public const string CNH_AUTHORITY = "CNH";

        /// <summary>
        /// ASEA (Agencia de Seguridad, Energía y Ambiente) identifier.
        /// </summary>
        public const string ASEA_AUTHORITY = "ASEA";

        // Norway
        /// <summary>
        /// NPD (Norwegian Petroleum Directorate) identifier.
        /// </summary>
        public const string NPD_AUTHORITY = "NPD";

        // United Kingdom
        /// <summary>
        /// NSTA (North Sea Transition Authority) identifier.
        /// </summary>
        public const string NSTA_AUTHORITY = "NSTA";

        // Australia
        /// <summary>
        /// NOPSEMA (National Offshore Petroleum Safety and Environmental Management Authority) identifier.
        /// </summary>
        public const string NOPSEMA_AUTHORITY = "NOPSEMA";

        /// <summary>
        /// QLD_DNRME (Queensland Department of Natural Resources, Mines and Energy) identifier.
        /// </summary>
        public const string QLD_DNRME_AUTHORITY = "QLD_DNRME";

        /// <summary>
        /// WA_DMIRS (Western Australia Department of Mines, Industry Regulation and Safety) identifier.
        /// </summary>
        public const string WA_DMIRS_AUTHORITY = "WA_DMIRS";

        // Brazil
        /// <summary>
        /// ANP (Agência Nacional do Petróleo) identifier.
        /// </summary>
        public const string ANP_AUTHORITY = "ANP";

        // Argentina
        /// <summary>
        /// ARG_NEUQUEN (Neuquén Province) identifier.
        /// </summary>
        public const string ARG_NEUQUEN_AUTHORITY = "ARG_NEUQUEN";

        /// <summary>
        /// ARG_MENDOZA (Mendoza Province) identifier.
        /// </summary>
        public const string ARG_MENDOZA_AUTHORITY = "ARG_MENDOZA";

        // Nigeria
        /// <summary>
        /// DPR (Department of Petroleum Resources) identifier.
        /// </summary>
        public const string DPR_AUTHORITY = "DPR";

        // Indonesia
        /// <summary>
        /// SKKMigas (Special Task Force for Upstream Oil and Gas Business Activities) identifier.
        /// </summary>
        public const string SKKMigas_AUTHORITY = "SKKMigas";

        // Kazakhstan
        /// <summary>
        /// KZ_MOE (Kazakhstan Ministry of Energy) identifier.
        /// </summary>
        public const string KZ_MOE_AUTHORITY = "KZ_MOE";

        /// <summary>
        /// Application status values.
        /// </summary>
        public static class ApplicationStatus
        {
            public const string DRAFT = "DRAFT";
            public const string SUBMITTED = "SUBMITTED";
            public const string UNDER_REVIEW = "UNDER_REVIEW";
            public const string ADDITIONAL_INFO_REQUIRED = "ADDITIONAL_INFO_REQUIRED";
            public const string APPROVED = "APPROVED";
            public const string REJECTED = "REJECTED";
            public const string WITHDRAWN = "WITHDRAWN";
            public const string EXPIRED = "EXPIRED";
            public const string RENEWED = "RENEWED";
        }

        /// <summary>
        /// Application type values.
        /// </summary>
        public static class ApplicationType
        {
            public const string DRILLING = "DRILLING";
            public const string ENVIRONMENTAL = "ENVIRONMENTAL";
            public const string INJECTION = "INJECTION";
            public const string STORAGE = "STORAGE";
            public const string FACILITY = "FACILITY";
            public const string SEISMIC = "SEISMIC";
            public const string GROUNDWATER = "GROUNDWATER";
        }

        /// <summary>
        /// Environmental permit types.
        /// </summary>
        public static class EnvironmentalPermitType
        {
            public const string WASTE_HAULER = "WASTE_HAULER";
            public const string PIT = "PIT";
            public const string DISCHARGE = "DISCHARGE";
            public const string RECYCLING = "RECYCLING";
            public const string LANDFARMING = "LANDFARMING";
            public const string RECLAMATION_PLANT = "RECLAMATION_PLANT";
            public const string COMMERCIAL_WASTE_SEPARATION = "COMMERCIAL_WASTE_SEPARATION";
            public const string COMMERCIAL_SURFACE_WASTE = "COMMERCIAL_SURFACE_WASTE";
            public const string HAZARDOUS_WASTE = "HAZARDOUS_WASTE";
            public const string NORM = "NORM";
        }

        /// <summary>
        /// Injection permit types.
        /// </summary>
        public static class InjectionPermitType
        {
            public const string ENHANCED_RECOVERY = "ENHANCED_RECOVERY";
            public const string WASTE_DISPOSAL = "WASTE_DISPOSAL";
            public const string GAS_STORAGE = "GAS_STORAGE";
            public const string CAVERN_STORAGE = "CAVERN_STORAGE";
            public const string CO2_STORAGE = "CO2_STORAGE";
            public const string BRINE_MINING = "BRINE_MINING";
            public const string GEOTHERMAL = "GEOTHERMAL";
            public const string TCEQ_CLASS_I = "TCEQ_CLASS_I";
        }

        /// <summary>
        /// Drilling permit types.
        /// </summary>
        public static class DrillingPermitType
        {
            public const string NEW_WELL = "NEW_WELL";
            public const string RE_ENTRY = "RE_ENTRY";
            public const string REAPPLICATION = "REAPPLICATION";
            public const string HORIZONTAL = "HORIZONTAL";
        }

        /// <summary>
        /// Default permit validity period in days.
        /// </summary>
        public const int DEFAULT_PERMIT_VALIDITY_DAYS = 365;

        /// <summary>
        /// Days before expiry to send renewal reminder.
        /// </summary>
        public const int RENEWAL_REMINDER_DAYS = 90;
    }
}

