using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    /// <summary>
    /// Helper for mapping jurisdictions and regulatory authorities.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01: every switch here names every member of its enum, so a member added later is a build error
    /// rather than a silent "Other". Naming them showed what the catch-alls had hidden: Saskatchewan's regulator was in no
    /// country, the Northern Territory's and South Australia's regulators were outside Australia, and the Australian state
    /// regulators, Norway's and the United Kingdom's were in no state or national jurisdiction.
    /// </remarks>
    public static class JurisdictionHelper
    {
        public static RegulatoryAuthority GetDefaultRegulatoryAuthority(Country country, StateProvince stateProvince)
        {
            return country switch
            {
                // Where the state or province has its own regulator, it is the default; elsewhere in these countries
                // no regulator is known here.
                Country.UnitedStates or Country.Canada or Country.Argentina => StateRegulatorIn(country, stateProvince),
                Country.Mexico => RegulatoryAuthority.CNH,
                Country.Norway => RegulatoryAuthority.NPD,
                Country.UnitedKingdom => RegulatoryAuthority.NSTA,
                Country.Australia => RegulatoryAuthority.NOPSEMA,
                Country.Brazil => RegulatoryAuthority.ANP,
                Country.Nigeria => RegulatoryAuthority.DPR,
                Country.Indonesia => RegulatoryAuthority.SKKMigas,
                Country.Kazakhstan => RegulatoryAuthority.KZ_MOE,
                Country.SaudiArabia or Country.UnitedArabEmirates or Country.Qatar or Country.Kuwait or Country.Russia
                    or Country.China or Country.Venezuela or Country.Angola or Country.Colombia or Country.Ecuador
                    or Country.Peru or Country.Egypt or Country.Libya or Country.Algeria or Country.Iraq or Country.Iran
                    or Country.Other => RegulatoryAuthority.Other
            };
        }

        public static Country GetCountry(RegulatoryAuthority authority)
        {
            return authority switch
            {
                RegulatoryAuthority.RRC or RegulatoryAuthority.TCEQ or RegulatoryAuthority.AOGCC or RegulatoryAuthority.NDIC
                    or RegulatoryAuthority.WOGCC or RegulatoryAuthority.COGCC or RegulatoryAuthority.OCC
                    or RegulatoryAuthority.LADNR or RegulatoryAuthority.NMOCD or RegulatoryAuthority.CEC
                    or RegulatoryAuthority.BLM or RegulatoryAuthority.USACE or RegulatoryAuthority.EPA
                    or RegulatoryAuthority.BOEM or RegulatoryAuthority.BSEE => Country.UnitedStates,
                RegulatoryAuthority.AER or RegulatoryAuthority.BCER or RegulatoryAuthority.SER
                    or RegulatoryAuthority.NLDET => Country.Canada,
                RegulatoryAuthority.CNH or RegulatoryAuthority.ASEA => Country.Mexico,
                RegulatoryAuthority.NPD => Country.Norway,
                RegulatoryAuthority.NSTA => Country.UnitedKingdom,
                RegulatoryAuthority.NOPSEMA or RegulatoryAuthority.QLD_DNRME or RegulatoryAuthority.WA_DMIRS
                    or RegulatoryAuthority.NT_DITT or RegulatoryAuthority.SA_DMRE => Country.Australia,
                RegulatoryAuthority.ANP => Country.Brazil,
                RegulatoryAuthority.ARG_NEUQUEN or RegulatoryAuthority.ARG_MENDOZA => Country.Argentina,
                RegulatoryAuthority.DPR => Country.Nigeria,
                RegulatoryAuthority.SKKMigas => Country.Indonesia,
                RegulatoryAuthority.KZ_MOE => Country.Kazakhstan,
                RegulatoryAuthority.Other => Country.Other
            };
        }

        public static StateProvince GetStateProvince(RegulatoryAuthority authority)
        {
            return authority switch
            {
                RegulatoryAuthority.RRC or RegulatoryAuthority.TCEQ => StateProvince.Texas,
                RegulatoryAuthority.LADNR => StateProvince.Louisiana,
                RegulatoryAuthority.OCC => StateProvince.Oklahoma,
                RegulatoryAuthority.NMOCD => StateProvince.NewMexico,
                RegulatoryAuthority.NDIC => StateProvince.NorthDakota,
                RegulatoryAuthority.WOGCC => StateProvince.Wyoming,
                RegulatoryAuthority.CEC => StateProvince.California,
                RegulatoryAuthority.AOGCC => StateProvince.Alaska,
                RegulatoryAuthority.COGCC => StateProvince.Colorado,
                RegulatoryAuthority.AER => StateProvince.Alberta,
                RegulatoryAuthority.BCER => StateProvince.BritishColumbia,
                RegulatoryAuthority.SER => StateProvince.Saskatchewan,
                RegulatoryAuthority.NLDET => StateProvince.NewfoundlandAndLabrador,
                RegulatoryAuthority.QLD_DNRME => StateProvince.Queensland,
                RegulatoryAuthority.WA_DMIRS => StateProvince.WesternAustralia,
                RegulatoryAuthority.NT_DITT => StateProvince.NorthernTerritory,
                RegulatoryAuthority.SA_DMRE => StateProvince.SouthAustralia,
                RegulatoryAuthority.ARG_NEUQUEN => StateProvince.Neuquen,
                RegulatoryAuthority.ARG_MENDOZA => StateProvince.Mendoza,
                RegulatoryAuthority.NPD => StateProvince.NorwayNational,
                RegulatoryAuthority.NSTA => StateProvince.UKNational,
                // Federal and national regulators answer for no one state or province.
                RegulatoryAuthority.BLM or RegulatoryAuthority.USACE or RegulatoryAuthority.EPA
                    or RegulatoryAuthority.BOEM or RegulatoryAuthority.BSEE or RegulatoryAuthority.CNH
                    or RegulatoryAuthority.ASEA or RegulatoryAuthority.NOPSEMA or RegulatoryAuthority.ANP
                    or RegulatoryAuthority.DPR or RegulatoryAuthority.SKKMigas or RegulatoryAuthority.KZ_MOE
                    or RegulatoryAuthority.Other => StateProvince.Other
            };
        }

        public static bool IsValidJurisdiction(Country country, StateProvince stateProvince)
        {
            if (country == Country.UnitedStates)
            {
                return stateProvince == StateProvince.Texas ||
                       stateProvince == StateProvince.Louisiana ||
                       stateProvince == StateProvince.Oklahoma ||
                       stateProvince == StateProvince.NewMexico ||
                       stateProvince == StateProvince.NorthDakota ||
                       stateProvince == StateProvince.Wyoming ||
                       stateProvince == StateProvince.California ||
                       stateProvince == StateProvince.Alaska ||
                       stateProvince == StateProvince.Colorado ||
                       stateProvince == StateProvince.Other;
            }

            if (country == Country.Canada)
            {
                return stateProvince == StateProvince.Alberta ||
                       stateProvince == StateProvince.BritishColumbia ||
                       stateProvince == StateProvince.Saskatchewan ||
                       stateProvince == StateProvince.NewfoundlandAndLabrador ||
                       stateProvince == StateProvince.Other;
            }

            return true;
        }

        /// <summary>The state or province's own regulator, when it is one of <paramref name="country"/>'s.</summary>
        private static RegulatoryAuthority StateRegulatorIn(Country country, StateProvince stateProvince)
        {
            var regulator = StateRegulator(stateProvince);
            return GetCountry(regulator) == country ? regulator : RegulatoryAuthority.Other;
        }

        private static RegulatoryAuthority StateRegulator(StateProvince stateProvince)
        {
            return stateProvince switch
            {
                StateProvince.Texas => RegulatoryAuthority.RRC,
                StateProvince.Louisiana => RegulatoryAuthority.LADNR,
                StateProvince.Oklahoma => RegulatoryAuthority.OCC,
                StateProvince.NewMexico => RegulatoryAuthority.NMOCD,
                StateProvince.NorthDakota => RegulatoryAuthority.NDIC,
                StateProvince.Wyoming => RegulatoryAuthority.WOGCC,
                StateProvince.California => RegulatoryAuthority.CEC,
                StateProvince.Alaska => RegulatoryAuthority.AOGCC,
                StateProvince.Colorado => RegulatoryAuthority.COGCC,
                StateProvince.Alberta => RegulatoryAuthority.AER,
                StateProvince.BritishColumbia => RegulatoryAuthority.BCER,
                StateProvince.Saskatchewan => RegulatoryAuthority.SER,
                StateProvince.NewfoundlandAndLabrador => RegulatoryAuthority.NLDET,
                StateProvince.Queensland => RegulatoryAuthority.QLD_DNRME,
                StateProvince.WesternAustralia => RegulatoryAuthority.WA_DMIRS,
                StateProvince.NorthernTerritory => RegulatoryAuthority.NT_DITT,
                StateProvince.SouthAustralia => RegulatoryAuthority.SA_DMRE,
                StateProvince.Neuquen => RegulatoryAuthority.ARG_NEUQUEN,
                StateProvince.Mendoza => RegulatoryAuthority.ARG_MENDOZA,
                // No state or provincial regulator is catalogued for these.
                StateProvince.Pennsylvania or StateProvince.WestVirginia or StateProvince.Ohio or StateProvince.Michigan
                    or StateProvince.Illinois or StateProvince.Indiana or StateProvince.Kansas or StateProvince.Montana
                    or StateProvince.Utah or StateProvince.Arkansas or StateProvince.Mississippi or StateProvince.Alabama
                    or StateProvince.Kentucky or StateProvince.Tennessee or StateProvince.Virginia
                    or StateProvince.Maryland or StateProvince.NewYork or StateProvince.Nebraska
                    or StateProvince.SouthDakota or StateProvince.Missouri or StateProvince.Georgia
                    or StateProvince.Florida or StateProvince.Nevada or StateProvince.Idaho or StateProvince.OtherUS
                    or StateProvince.Manitoba or StateProvince.Ontario or StateProvince.Quebec
                    or StateProvince.NewBrunswick or StateProvince.NovaScotia or StateProvince.PrinceEdwardIsland
                    or StateProvince.NorthwestTerritories or StateProvince.Yukon or StateProvince.Nunavut
                    or StateProvince.OtherCanada
                    or StateProvince.Campeche or StateProvince.Tabasco or StateProvince.Veracruz
                    or StateProvince.Tamaulipas or StateProvince.Chiapas or StateProvince.OtherMexico
                    or StateProvince.Victoria or StateProvince.NewSouthWales or StateProvince.Tasmania
                    or StateProvince.OtherAustralia
                    or StateProvince.RioDeJaneiro or StateProvince.EspiritoSanto or StateProvince.Bahia
                    or StateProvince.Sergipe or StateProvince.Amazonas or StateProvince.OtherBrazil
                    or StateProvince.Chubut or StateProvince.SantaCruz or StateProvince.Salta
                    or StateProvince.OtherArgentina
                    or StateProvince.NorwayNational or StateProvince.UKNational
                    or StateProvince.Rivers or StateProvince.Bayelsa or StateProvince.Delta or StateProvince.AkwaIbom
                    or StateProvince.CrossRiver or StateProvince.OtherNigeria
                    or StateProvince.Riau or StateProvince.EastKalimantan or StateProvince.SouthSumatra
                    or StateProvince.Aceh or StateProvince.OtherIndonesia
                    or StateProvince.Atyrau or StateProvince.Mangystau or StateProvince.WestKazakhstan
                    or StateProvince.Aktobe or StateProvince.OtherKazakhstan
                    or StateProvince.Other => RegulatoryAuthority.Other
            };
        }
    }
}
