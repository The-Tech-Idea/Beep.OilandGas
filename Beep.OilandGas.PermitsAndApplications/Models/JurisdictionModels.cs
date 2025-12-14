using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PermitsAndApplications.Models
{
    /// <summary>
    /// Represents jurisdiction-specific permit requirements and regulations.
    /// </summary>
    public class JurisdictionRequirements
    {
        /// <summary>
        /// Country/jurisdiction.
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        /// State or province.
        /// </summary>
        public StateProvince StateProvince { get; set; }

        /// <summary>
        /// Regulatory authority.
        /// </summary>
        public RegulatoryAuthority RegulatoryAuthority { get; set; }

        /// <summary>
        /// List of required forms for this jurisdiction.
        /// </summary>
        public List<RequiredForm> RequiredForms { get; set; } = new();

        /// <summary>
        /// List of applicable regulations.
        /// </summary>
        public List<string> ApplicableRegulations { get; set; } = new();

        /// <summary>
        /// Jurisdiction-specific requirements description.
        /// </summary>
        public string? RequirementsDescription { get; set; }
    }

    /// <summary>
    /// Represents a required form for a jurisdiction.
    /// </summary>
    public class RequiredForm
    {
        /// <summary>
        /// Form identifier or number.
        /// </summary>
        public string FormId { get; set; } = string.Empty;

        /// <summary>
        /// Form name or title.
        /// </summary>
        public string FormName { get; set; } = string.Empty;

        /// <summary>
        /// Form description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// URL to form (if available online).
        /// </summary>
        public string? FormUrl { get; set; }

        /// <summary>
        /// Indicates if online filing is available.
        /// </summary>
        public bool OnlineFilingAvailable { get; set; }

        /// <summary>
        /// Form type (PDF, Word, Online, etc.).
        /// </summary>
        public string? FormType { get; set; }

        /// <summary>
        /// Applicable permit types for this form.
        /// </summary>
        public List<PermitApplicationType> ApplicablePermitTypes { get; set; } = new();
    }

    /// <summary>
    /// Helper class for jurisdiction mapping and validation.
    /// </summary>
    public static class JurisdictionHelper
    {
        /// <summary>
        /// Gets the default regulatory authority for a country and state/province.
        /// </summary>
        public static RegulatoryAuthority GetDefaultRegulatoryAuthority(Country country, StateProvince stateProvince)
        {
            return (country, stateProvince) switch
            {
                // United States
                (Country.UnitedStates, StateProvince.Texas) => RegulatoryAuthority.RRC,
                (Country.UnitedStates, StateProvince.Alaska) => RegulatoryAuthority.AOGCC,
                (Country.UnitedStates, StateProvince.NorthDakota) => RegulatoryAuthority.NDIC,
                (Country.UnitedStates, StateProvince.Wyoming) => RegulatoryAuthority.WOGCC,
                (Country.UnitedStates, StateProvince.Colorado) => RegulatoryAuthority.COGCC,
                (Country.UnitedStates, StateProvince.Oklahoma) => RegulatoryAuthority.OCC,
                (Country.UnitedStates, StateProvince.Louisiana) => RegulatoryAuthority.LADNR,
                (Country.UnitedStates, StateProvince.NewMexico) => RegulatoryAuthority.NMOCD,
                (Country.UnitedStates, StateProvince.California) => RegulatoryAuthority.CEC,
                
                // Canada
                (Country.Canada, StateProvince.Alberta) => RegulatoryAuthority.AER,
                (Country.Canada, StateProvince.BritishColumbia) => RegulatoryAuthority.BCER,
                (Country.Canada, StateProvince.Saskatchewan) => RegulatoryAuthority.SER,
                (Country.Canada, StateProvince.NewfoundlandAndLabrador) => RegulatoryAuthority.NLDET,
                
                // Mexico
                (Country.Mexico, _) => RegulatoryAuthority.CNH,
                
                // Norway
                (Country.Norway, _) => RegulatoryAuthority.NPD,
                
                // United Kingdom
                (Country.UnitedKingdom, _) => RegulatoryAuthority.NSTA,
                
                // Australia
                (Country.Australia, StateProvince.Queensland) => RegulatoryAuthority.QLD_DNRME,
                (Country.Australia, StateProvince.WesternAustralia) => RegulatoryAuthority.WA_DMIRS,
                (Country.Australia, _) => RegulatoryAuthority.NOPSEMA,
                
                // Brazil
                (Country.Brazil, _) => RegulatoryAuthority.ANP,
                
                // Argentina
                (Country.Argentina, StateProvince.Neuquen) => RegulatoryAuthority.ARG_NEUQUEN,
                (Country.Argentina, StateProvince.Mendoza) => RegulatoryAuthority.ARG_MENDOZA,
                
                // Nigeria
                (Country.Nigeria, _) => RegulatoryAuthority.DPR,
                
                // Indonesia
                (Country.Indonesia, _) => RegulatoryAuthority.SKKMigas,
                
                // Kazakhstan
                (Country.Kazakhstan, _) => RegulatoryAuthority.KZ_MOE,
                
                _ => RegulatoryAuthority.Other
            };
        }

        /// <summary>
        /// Gets the country for a regulatory authority.
        /// </summary>
        public static Country GetCountry(RegulatoryAuthority authority)
        {
            return authority switch
            {
                // United States
                RegulatoryAuthority.RRC or RegulatoryAuthority.TCEQ or RegulatoryAuthority.AOGCC
                or RegulatoryAuthority.NDIC or RegulatoryAuthority.WOGCC or RegulatoryAuthority.COGCC
                or RegulatoryAuthority.OCC or RegulatoryAuthority.LADNR or RegulatoryAuthority.NMOCD
                or RegulatoryAuthority.CEC or RegulatoryAuthority.BLM or RegulatoryAuthority.USACE
                or RegulatoryAuthority.EPA or RegulatoryAuthority.BOEM or RegulatoryAuthority.BSEE => Country.UnitedStates,
                
                // Canada
                RegulatoryAuthority.AER or RegulatoryAuthority.BCER or RegulatoryAuthority.SER
                or RegulatoryAuthority.NLDET => Country.Canada,
                
                // Mexico
                RegulatoryAuthority.CNH or RegulatoryAuthority.ASEA => Country.Mexico,
                
                // Norway
                RegulatoryAuthority.NPD => Country.Norway,
                
                // United Kingdom
                RegulatoryAuthority.NSTA => Country.UnitedKingdom,
                
                // Australia
                RegulatoryAuthority.NOPSEMA or RegulatoryAuthority.QLD_DNRME or RegulatoryAuthority.WA_DMIRS
                or RegulatoryAuthority.NT_DITT or RegulatoryAuthority.SA_DMRE => Country.Australia,
                
                // Brazil
                RegulatoryAuthority.ANP => Country.Brazil,
                
                // Argentina
                RegulatoryAuthority.ARG_NEUQUEN or RegulatoryAuthority.ARG_MENDOZA => Country.Argentina,
                
                // Nigeria
                RegulatoryAuthority.DPR => Country.Nigeria,
                
                // Indonesia
                RegulatoryAuthority.SKKMigas => Country.Indonesia,
                
                // Kazakhstan
                RegulatoryAuthority.KZ_MOE => Country.Kazakhstan,
                
                _ => Country.Other
            };
        }

        /// <summary>
        /// Gets the state/province for a regulatory authority.
        /// </summary>
        public static StateProvince GetStateProvince(RegulatoryAuthority authority)
        {
            return authority switch
            {
                // United States
                RegulatoryAuthority.RRC or RegulatoryAuthority.TCEQ => StateProvince.Texas,
                RegulatoryAuthority.AOGCC => StateProvince.Alaska,
                RegulatoryAuthority.NDIC => StateProvince.NorthDakota,
                RegulatoryAuthority.WOGCC => StateProvince.Wyoming,
                RegulatoryAuthority.COGCC => StateProvince.Colorado,
                RegulatoryAuthority.OCC => StateProvince.Oklahoma,
                RegulatoryAuthority.LADNR => StateProvince.Louisiana,
                RegulatoryAuthority.NMOCD => StateProvince.NewMexico,
                RegulatoryAuthority.CEC => StateProvince.California,
                RegulatoryAuthority.BLM or RegulatoryAuthority.USACE or RegulatoryAuthority.EPA
                or RegulatoryAuthority.BOEM or RegulatoryAuthority.BSEE => StateProvince.OtherUS,
                
                // Canada
                RegulatoryAuthority.AER => StateProvince.Alberta,
                RegulatoryAuthority.BCER => StateProvince.BritishColumbia,
                RegulatoryAuthority.SER => StateProvince.Saskatchewan,
                RegulatoryAuthority.NLDET => StateProvince.NewfoundlandAndLabrador,
                
                // Mexico
                RegulatoryAuthority.CNH or RegulatoryAuthority.ASEA => StateProvince.OtherMexico,
                
                // Norway
                RegulatoryAuthority.NPD => StateProvince.NorwayNational,
                
                // United Kingdom
                RegulatoryAuthority.NSTA => StateProvince.UKNational,
                
                // Australia
                RegulatoryAuthority.QLD_DNRME => StateProvince.Queensland,
                RegulatoryAuthority.WA_DMIRS => StateProvince.WesternAustralia,
                RegulatoryAuthority.NOPSEMA or RegulatoryAuthority.NT_DITT or RegulatoryAuthority.SA_DMRE => StateProvince.OtherAustralia,
                
                // Brazil
                RegulatoryAuthority.ANP => StateProvince.OtherBrazil,
                
                // Argentina
                RegulatoryAuthority.ARG_NEUQUEN => StateProvince.Neuquen,
                RegulatoryAuthority.ARG_MENDOZA => StateProvince.Mendoza,
                
                // Nigeria
                RegulatoryAuthority.DPR => StateProvince.OtherNigeria,
                
                // Indonesia
                RegulatoryAuthority.SKKMigas => StateProvince.OtherIndonesia,
                
                // Kazakhstan
                RegulatoryAuthority.KZ_MOE => StateProvince.OtherKazakhstan,
                
                _ => StateProvince.Other
            };
        }

        /// <summary>
        /// Validates that country and state/province are compatible.
        /// </summary>
        public static bool IsValidJurisdiction(Country country, StateProvince stateProvince)
        {
            return country switch
            {
                Country.UnitedStates => stateProvince switch
                {
                    StateProvince.Texas or StateProvince.Louisiana or StateProvince.Oklahoma
                    or StateProvince.NewMexico or StateProvince.NorthDakota or StateProvince.Wyoming
                    or StateProvince.California or StateProvince.Alaska or StateProvince.Colorado
                    or StateProvince.Pennsylvania or StateProvince.WestVirginia or StateProvince.Ohio
                    or StateProvince.Michigan or StateProvince.Illinois or StateProvince.Indiana
                    or StateProvince.Kansas or StateProvince.Montana or StateProvince.Utah
                    or StateProvince.Arkansas or StateProvince.Mississippi or StateProvince.Alabama
                    or StateProvince.Kentucky or StateProvince.Tennessee or StateProvince.Virginia
                    or StateProvince.Maryland or StateProvince.NewYork or StateProvince.OtherUS => true,
                    _ => false
                },
                Country.Canada => stateProvince switch
                {
                    StateProvince.Alberta or StateProvince.BritishColumbia or StateProvince.Saskatchewan
                    or StateProvince.Manitoba or StateProvince.Ontario or StateProvince.Quebec
                    or StateProvince.NewfoundlandAndLabrador or StateProvince.NewBrunswick
                    or StateProvince.NovaScotia or StateProvince.PrinceEdwardIsland
                    or StateProvince.NorthwestTerritories or StateProvince.Yukon
                    or StateProvince.Nunavut or StateProvince.OtherCanada => true,
                    _ => false
                },
                Country.Mexico => stateProvince switch
                {
                    StateProvince.Campeche or StateProvince.Tabasco or StateProvince.Veracruz
                    or StateProvince.Tamaulipas or StateProvince.Chiapas or StateProvince.OtherMexico => true,
                    _ => false
                },
                Country.Norway => stateProvince == StateProvince.NorwayNational,
                Country.UnitedKingdom => stateProvince == StateProvince.UKNational,
                Country.Australia => stateProvince switch
                {
                    StateProvince.Queensland or StateProvince.WesternAustralia or StateProvince.NorthernTerritory
                    or StateProvince.SouthAustralia or StateProvince.Victoria or StateProvince.NewSouthWales
                    or StateProvince.Tasmania or StateProvince.OtherAustralia => true,
                    _ => false
                },
                Country.Brazil => stateProvince switch
                {
                    StateProvince.RioDeJaneiro or StateProvince.EspiritoSanto or StateProvince.Bahia
                    or StateProvince.Sergipe or StateProvince.Amazonas or StateProvince.OtherBrazil => true,
                    _ => false
                },
                Country.Argentina => stateProvince switch
                {
                    StateProvince.Neuquen or StateProvince.Mendoza or StateProvince.Chubut
                    or StateProvince.SantaCruz or StateProvince.Salta or StateProvince.OtherArgentina => true,
                    _ => false
                },
                Country.Nigeria => stateProvince switch
                {
                    StateProvince.Rivers or StateProvince.Bayelsa or StateProvince.Delta
                    or StateProvince.AkwaIbom or StateProvince.CrossRiver or StateProvince.OtherNigeria => true,
                    _ => false
                },
                Country.Indonesia => stateProvince switch
                {
                    StateProvince.Riau or StateProvince.EastKalimantan or StateProvince.SouthSumatra
                    or StateProvince.Aceh or StateProvince.OtherIndonesia => true,
                    _ => false
                },
                Country.Kazakhstan => stateProvince switch
                {
                    StateProvince.Atyrau or StateProvince.Mangystau or StateProvince.WestKazakhstan
                    or StateProvince.Aktobe or StateProvince.OtherKazakhstan => true,
                    _ => false
                },
                _ => true // Other countries allow any state/province
            };
        }
    }
}

