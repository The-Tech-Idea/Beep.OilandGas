using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    /// <summary>
    /// Helper for mapping jurisdictions and regulatory authorities.
    /// </summary>
    public static class JurisdictionHelper
    {
        public static RegulatoryAuthority GetDefaultRegulatoryAuthority(Country country, StateProvince stateProvince)
        {
            return country switch
            {
                Country.UnitedStates => stateProvince switch
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
                    _ => RegulatoryAuthority.Other
                },
                Country.Canada => stateProvince switch
                {
                    StateProvince.Alberta => RegulatoryAuthority.AER,
                    StateProvince.BritishColumbia => RegulatoryAuthority.BCER,
                    StateProvince.Saskatchewan => RegulatoryAuthority.SER,
                    StateProvince.NewfoundlandAndLabrador => RegulatoryAuthority.NLDET,
                    _ => RegulatoryAuthority.Other
                },
                Country.Mexico => RegulatoryAuthority.CNH,
                Country.Norway => RegulatoryAuthority.NPD,
                Country.UnitedKingdom => RegulatoryAuthority.NSTA,
                Country.Australia => RegulatoryAuthority.NOPSEMA,
                Country.Brazil => RegulatoryAuthority.ANP,
                Country.Argentina => stateProvince switch
                {
                    StateProvince.Neuquen => RegulatoryAuthority.ARG_NEUQUEN,
                    StateProvince.Mendoza => RegulatoryAuthority.ARG_MENDOZA,
                    _ => RegulatoryAuthority.Other
                },
                Country.Nigeria => RegulatoryAuthority.DPR,
                Country.Indonesia => RegulatoryAuthority.SKKMigas,
                Country.Kazakhstan => RegulatoryAuthority.KZ_MOE,
                _ => RegulatoryAuthority.Other
            };
        }

        public static Country GetCountry(RegulatoryAuthority authority)
        {
            return authority switch
            {
                RegulatoryAuthority.CNH or RegulatoryAuthority.ASEA => Country.Mexico,
                RegulatoryAuthority.AER or RegulatoryAuthority.BCER or RegulatoryAuthority.NLDET => Country.Canada,
                RegulatoryAuthority.NPD => Country.Norway,
                RegulatoryAuthority.NSTA => Country.UnitedKingdom,
                RegulatoryAuthority.NOPSEMA or RegulatoryAuthority.QLD_DNRME or RegulatoryAuthority.WA_DMIRS => Country.Australia,
                RegulatoryAuthority.ANP => Country.Brazil,
                RegulatoryAuthority.ARG_NEUQUEN or RegulatoryAuthority.ARG_MENDOZA => Country.Argentina,
                RegulatoryAuthority.DPR => Country.Nigeria,
                RegulatoryAuthority.SKKMigas => Country.Indonesia,
                RegulatoryAuthority.KZ_MOE => Country.Kazakhstan,
                _ when authority == RegulatoryAuthority.RRC || authority == RegulatoryAuthority.TCEQ ||
                       authority == RegulatoryAuthority.AOGCC || authority == RegulatoryAuthority.NDIC ||
                       authority == RegulatoryAuthority.WOGCC || authority == RegulatoryAuthority.COGCC ||
                       authority == RegulatoryAuthority.OCC || authority == RegulatoryAuthority.LADNR ||
                       authority == RegulatoryAuthority.NMOCD || authority == RegulatoryAuthority.CEC ||
                       authority == RegulatoryAuthority.BLM || authority == RegulatoryAuthority.USACE ||
                       authority == RegulatoryAuthority.EPA || authority == RegulatoryAuthority.BOEM ||
                       authority == RegulatoryAuthority.BSEE => Country.UnitedStates,
                _ => Country.Other
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
                RegulatoryAuthority.ARG_NEUQUEN => StateProvince.Neuquen,
                RegulatoryAuthority.ARG_MENDOZA => StateProvince.Mendoza,
                _ => StateProvince.Other
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
    }
}
