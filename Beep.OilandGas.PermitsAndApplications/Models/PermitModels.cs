using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PermitsAndApplications.Models
{
    /// <summary>
    /// Represents the status of a permit application.
    /// </summary>
    public enum PermitApplicationStatus
    {
        Draft,
        Submitted,
        UnderReview,
        AdditionalInformationRequired,
        Approved,
        Rejected,
        Withdrawn,
        Expired,
        Renewed
    }

    /// <summary>
    /// Represents the type of permit application.
    /// </summary>
    public enum PermitApplicationType
    {
        Drilling,
        Environmental,
        Injection,
        Storage,
        Facility,
        Seismic,
        Groundwater,
        Other
    }

    /// <summary>
    /// Represents a regulatory authority.
    /// </summary>
    public enum RegulatoryAuthority
    {
        // United States - Texas
        RRC,        // Railroad Commission of Texas
        TCEQ,       // Texas Commission on Environmental Quality
        
        // United States - Other States
        AOGCC,      // Alaska Oil and Gas Conservation Commission
        NDIC,       // North Dakota Industrial Commission
        WOGCC,      // Wyoming Oil and Gas Conservation Commission
        COGCC,      // Colorado Oil and Gas Conservation Commission
        OCC,        // Oklahoma Corporation Commission
        LADNR,      // Louisiana Department of Natural Resources
        NMOCD,      // New Mexico Oil Conservation Division
        CEC,        // California Energy Commission
        
        // United States - Federal
        BLM,        // Bureau of Land Management
        USACE,      // U.S. Army Corps of Engineers
        EPA,        // Environmental Protection Agency
        BOEM,       // Bureau of Ocean Energy Management
        BSEE,       // Bureau of Safety and Environmental Enforcement
        
        // Canada
        AER,        // Alberta Energy Regulator
        BCER,       // British Columbia Energy Regulator (formerly BC Oil and Gas Commission)
        SER,        // Saskatchewan Energy and Resources
        NLDET,      // Newfoundland and Labrador Department of Industry, Energy and Technology
        
        // Mexico
        CNH,        // Comisión Nacional de Hidrocarburos (National Hydrocarbons Commission)
        ASEA,       // Agencia de Seguridad, Energía y Ambiente (Safety, Energy and Environment Agency)
        
        // Norway
        NPD,        // Norwegian Petroleum Directorate
        
        // United Kingdom
        NSTA,       // North Sea Transition Authority (formerly Oil and Gas Authority)
        
        // Australia
        NOPSEMA,    // National Offshore Petroleum Safety and Environmental Management Authority
        QLD_DNRME,  // Queensland Department of Natural Resources, Mines and Energy
        WA_DMIRS,   // Western Australia Department of Mines, Industry Regulation and Safety
        NT_DITT,    // Northern Territory Department of Industry, Tourism and Trade
        SA_DMRE,    // South Australia Department for Energy and Mining
        
        // Brazil
        ANP,        // Agência Nacional do Petróleo (National Agency of Petroleum)
        
        // Argentina
        ARG_NEUQUEN, // Neuquén Province
        ARG_MENDOZA, // Mendoza Province
        
        // Nigeria
        DPR,        // Department of Petroleum Resources
        
        // Indonesia
        SKKMigas,   // Special Task Force for Upstream Oil and Gas Business Activities
        
        // Kazakhstan
        KZ_MOE,     // Kazakhstan Ministry of Energy
        
        // Other
        Other
    }

    /// <summary>
    /// Represents a country/jurisdiction.
    /// </summary>
    public enum Country
    {
        UnitedStates,
        Canada,
        Mexico,
        Norway,
        UnitedKingdom,
        Australia,
        Brazil,
        Argentina,
        Nigeria,
        Indonesia,
        Kazakhstan,
        SaudiArabia,
        UnitedArabEmirates,
        Qatar,
        Kuwait,
        Russia,
        China,
        Venezuela,
        Angola,
        Colombia,
        Ecuador,
        Peru,
        Egypt,
        Libya,
        Algeria,
        Iraq,
        Iran,
        Other
    }

    /// <summary>
    /// Represents a state or province within a country.
    /// </summary>
    public enum StateProvince
    {
        // United States
        Texas,
        Louisiana,
        Oklahoma,
        NewMexico,
        NorthDakota,
        Wyoming,
        California,
        Alaska,
        Colorado,
        Pennsylvania,
        WestVirginia,
        Ohio,
        Michigan,
        Illinois,
        Indiana,
        Kansas,
        Montana,
        Utah,
        Arkansas,
        Mississippi,
        Alabama,
        Kentucky,
        Tennessee,
        Virginia,
        Maryland,
        NewYork,
        Nebraska,
        SouthDakota,
        Missouri,
        Georgia,
        Florida,
        Nevada,
        Idaho,
        OtherUS,
        
        // Canada
        Alberta,
        BritishColumbia,
        Saskatchewan,
        Manitoba,
        Ontario,
        Quebec,
        NewfoundlandAndLabrador,
        NewBrunswick,
        NovaScotia,
        PrinceEdwardIsland,
        NorthwestTerritories,
        Yukon,
        Nunavut,
        OtherCanada,
        
        // Mexico
        Campeche,
        Tabasco,
        Veracruz,
        Tamaulipas,
        Chiapas,
        OtherMexico,
        
        // Australia
        Queensland,
        WesternAustralia,
        NorthernTerritory,
        SouthAustralia,
        Victoria,
        NewSouthWales,
        Tasmania,
        OtherAustralia,
        
        // Brazil
        RioDeJaneiro,
        EspiritoSanto,
        Bahia,
        Sergipe,
        Amazonas,
        OtherBrazil,
        
        // Argentina
        Neuquen,
        Mendoza,
        Chubut,
        SantaCruz,
        Salta,
        OtherArgentina,
        
        // Norway (National - no states)
        NorwayNational,
        
        // United Kingdom (National - no states)
        UKNational,
        
        // Nigeria (States)
        Rivers,
        Bayelsa,
        Delta,
        AkwaIbom,
        CrossRiver,
        OtherNigeria,
        
        // Indonesia (Provinces)
        Riau,
        EastKalimantan,
        SouthSumatra,
        Aceh,
        OtherIndonesia,
        
        // Kazakhstan (Regions)
        Atyrau,
        Mangystau,
        WestKazakhstan,
        Aktobe,
        OtherKazakhstan,
        
        // Other
        Other
    }

    /// <summary>
    /// Base class for all permit applications.
    /// </summary>
    public class PermitApplication
    {
        /// <summary>
        /// Unique application identifier.
        /// </summary>
        public string ApplicationId { get; set; } = string.Empty;

        /// <summary>
        /// Type of permit application.
        /// </summary>
        public PermitApplicationType ApplicationType { get; set; }

        /// <summary>
        /// Current status of the application.
        /// </summary>
        public PermitApplicationStatus Status { get; set; }

        /// <summary>
        /// Country/jurisdiction where the application is filed.
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        /// State or province where the application is filed.
        /// </summary>
        public StateProvince StateProvince { get; set; }

        /// <summary>
        /// Regulatory authority handling the application.
        /// </summary>
        public RegulatoryAuthority RegulatoryAuthority { get; set; }

        /// <summary>
        /// Date the application was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date the application was submitted.
        /// </summary>
        public DateTime? SubmittedDate { get; set; }

        /// <summary>
        /// Date the application was received by regulatory authority.
        /// </summary>
        public DateTime? ReceivedDate { get; set; }

        /// <summary>
        /// Date a decision was made.
        /// </summary>
        public DateTime? DecisionDate { get; set; }

        /// <summary>
        /// Date the permit becomes effective.
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Date the permit expires.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Decision made on the application (Approved, Rejected, etc.).
        /// </summary>
        public string? Decision { get; set; }

        /// <summary>
        /// Reference number from regulatory authority.
        /// </summary>
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// Applicant business associate ID.
        /// </summary>
        public string? ApplicantId { get; set; }

        /// <summary>
        /// Operator business associate ID.
        /// </summary>
        public string? OperatorId { get; set; }

        /// <summary>
        /// Related well UWI (if applicable).
        /// </summary>
        public string? RelatedWellUWI { get; set; }

        /// <summary>
        /// Related facility ID (if applicable).
        /// </summary>
        public string? RelatedFacilityId { get; set; }

        /// <summary>
        /// Fees description.
        /// </summary>
        public string? FeesDescription { get; set; }

        /// <summary>
        /// Indicates if fees have been paid.
        /// </summary>
        public bool FeesPaid { get; set; }

        /// <summary>
        /// Remarks or notes.
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// Indicates if submission is complete.
        /// </summary>
        public bool SubmissionComplete { get; set; }

        /// <summary>
        /// Submission description.
        /// </summary>
        public string? SubmissionDescription { get; set; }

        /// <summary>
        /// Attachments/documents associated with the application.
        /// </summary>
        public List<ApplicationAttachment> Attachments { get; set; } = new();

        /// <summary>
        /// Geographic areas covered by the application.
        /// </summary>
        public List<ApplicationArea> Areas { get; set; } = new();

        /// <summary>
        /// Components of the application.
        /// </summary>
        public List<ApplicationComponent> Components { get; set; } = new();
    }

    /// <summary>
    /// Represents a drilling permit application.
    /// </summary>
    public class DrillingPermitApplication : PermitApplication
    {
        /// <summary>
        /// Well UWI for the drilling permit.
        /// </summary>
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Legal description of well location.
        /// </summary>
        public string? LegalDescription { get; set; }

        /// <summary>
        /// Target formation for drilling.
        /// </summary>
        public string? TargetFormation { get; set; }

        /// <summary>
        /// Proposed total depth in feet.
        /// </summary>
        public decimal? ProposedDepth { get; set; }

        /// <summary>
        /// Drilling method (vertical, directional, horizontal).
        /// </summary>
        public string? DrillingMethod { get; set; }

        /// <summary>
        /// Indicates if surface owner has been notified.
        /// </summary>
        public bool SurfaceOwnerNotified { get; set; }

        /// <summary>
        /// Date surface owner was notified.
        /// </summary>
        public DateTime? SurfaceOwnerNotificationDate { get; set; }

        /// <summary>
        /// Environmental impact assessment required.
        /// </summary>
        public bool EnvironmentalAssessmentRequired { get; set; }

        /// <summary>
        /// Environmental assessment document reference.
        /// </summary>
        public string? EnvironmentalAssessmentReference { get; set; }

        /// <summary>
        /// Spacing unit information.
        /// </summary>
        public string? SpacingUnit { get; set; }

        /// <summary>
        /// Permit type (new well, re-entry, etc.).
        /// </summary>
        public string? PermitType { get; set; }
    }

    /// <summary>
    /// Represents an environmental permit application.
    /// </summary>
    public class EnvironmentalPermitApplication : PermitApplication
    {
        /// <summary>
        /// Type of environmental permit (waste hauler, pit, discharge, etc.).
        /// </summary>
        public string? EnvironmentalPermitType { get; set; }

        /// <summary>
        /// Type of waste material.
        /// </summary>
        public string? WasteType { get; set; }

        /// <summary>
        /// Estimated waste volume.
        /// </summary>
        public decimal? WasteVolume { get; set; }

        /// <summary>
        /// Unit of measure for waste volume.
        /// </summary>
        public string? WasteVolumeUnit { get; set; }

        /// <summary>
        /// Disposal or treatment method.
        /// </summary>
        public string? DisposalMethod { get; set; }

        /// <summary>
        /// Environmental impact assessment.
        /// </summary>
        public string? EnvironmentalImpact { get; set; }

        /// <summary>
        /// Monitoring plan description.
        /// </summary>
        public string? MonitoringPlan { get; set; }

        /// <summary>
        /// Indicates if NORM (Naturally Occurring Radioactive Material) is involved.
        /// </summary>
        public bool NORMInvolved { get; set; }

        /// <summary>
        /// Facility location description.
        /// </summary>
        public string? FacilityLocation { get; set; }
    }

    /// <summary>
    /// Represents an injection/storage permit application.
    /// </summary>
    public class InjectionPermitApplication : PermitApplication
    {
        /// <summary>
        /// Type of injection (Enhanced Recovery, Disposal, Storage, etc.).
        /// </summary>
        public string? InjectionType { get; set; }

        /// <summary>
        /// Injection zone formation.
        /// </summary>
        public string? InjectionZone { get; set; }

        /// <summary>
        /// Type of injection fluid.
        /// </summary>
        public string? InjectionFluid { get; set; }

        /// <summary>
        /// Maximum injection pressure in psia.
        /// </summary>
        public decimal? MaximumInjectionPressure { get; set; }

        /// <summary>
        /// Maximum injection rate.
        /// </summary>
        public decimal? MaximumInjectionRate { get; set; }

        /// <summary>
        /// Unit of measure for injection rate.
        /// </summary>
        public string? InjectionRateUnit { get; set; }

        /// <summary>
        /// Mechanical Integrity Test (MIT) results.
        /// </summary>
        public List<MITResult> MITResults { get; set; } = new();

        /// <summary>
        /// Monitoring requirements.
        /// </summary>
        public string? MonitoringRequirements { get; set; }

        /// <summary>
        /// Well UWI for injection well.
        /// </summary>
        public string? InjectionWellUWI { get; set; }

        /// <summary>
        /// Indicates if this is for CO2 storage.
        /// </summary>
        public bool IsCO2Storage { get; set; }

        /// <summary>
        /// Indicates if this is for gas storage.
        /// </summary>
        public bool IsGasStorage { get; set; }

        /// <summary>
        /// Indicates if this is for brine mining/lithium production.
        /// </summary>
        public bool IsBrineMining { get; set; }
    }

    /// <summary>
    /// Represents an application attachment/document.
    /// </summary>
    public class ApplicationAttachment
    {
        public string AttachmentId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public long? FileSize { get; set; }
        public DateTime? UploadDate { get; set; }
        public string? Description { get; set; }
        public string? DocumentType { get; set; }
    }

    /// <summary>
    /// Represents a geographic area covered by an application.
    /// </summary>
    public class ApplicationArea
    {
        public string AreaId { get; set; } = string.Empty;
        public string? AreaName { get; set; }
        public string? AreaType { get; set; }
        public string? LegalDescription { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    /// <summary>
    /// Represents a component of an application.
    /// </summary>
    public class ApplicationComponent
    {
        public string ComponentId { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Value { get; set; }
        public int? SequenceNumber { get; set; }
    }

    /// <summary>
    /// Represents Mechanical Integrity Test (MIT) results.
    /// </summary>
    public class MITResult
    {
        public string MITId { get; set; } = string.Empty;
        public DateTime TestDate { get; set; }
        public string TestType { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public decimal? TestPressure { get; set; }
        public string? TestResult { get; set; }
        public string? Remarks { get; set; }
    }
}

