namespace Beep.OilandGas.Models.PermitsAndApplications
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
}
