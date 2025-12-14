# Supported Jurisdictions

## Overview

The `Beep.OilandGas.PermitsAndApplications` library supports multiple jurisdictions worldwide. This document lists all supported countries, states/provinces, and their regulatory authorities.

## United States

### States with Dedicated Regulatory Authorities

| State | Regulatory Authority | Abbreviation |
|-------|---------------------|--------------|
| Texas | Railroad Commission of Texas | RRC |
| Texas | Texas Commission on Environmental Quality | TCEQ |
| Alaska | Alaska Oil and Gas Conservation Commission | AOGCC |
| North Dakota | North Dakota Industrial Commission | NDIC |
| Wyoming | Wyoming Oil and Gas Conservation Commission | WOGCC |
| Colorado | Colorado Oil and Gas Conservation Commission | COGCC |
| Oklahoma | Oklahoma Corporation Commission | OCC |
| Louisiana | Louisiana Department of Natural Resources | LADNR |
| New Mexico | New Mexico Oil Conservation Division | NMOCD |
| California | California Energy Commission | CEC |

### Other US States
- Pennsylvania, West Virginia, Ohio, Michigan, Illinois, Indiana, Kansas, Montana, Utah, Arkansas, Mississippi, Alabama, Kentucky, Tennessee, Virginia, Maryland, New York, Nebraska, South Dakota, Missouri, Georgia, Florida, Nevada, Idaho

### Federal Authorities
- **BLM** - Bureau of Land Management
- **USACE** - U.S. Army Corps of Engineers
- **EPA** - Environmental Protection Agency
- **BOEM** - Bureau of Ocean Energy Management
- **BSEE** - Bureau of Safety and Environmental Enforcement

## Canada

| Province/Territory | Regulatory Authority | Abbreviation |
|-------------------|---------------------|--------------|
| Alberta | Alberta Energy Regulator | AER |
| British Columbia | British Columbia Energy Regulator | BCER |
| Saskatchewan | Saskatchewan Energy and Resources | SER |
| Newfoundland and Labrador | Department of Industry, Energy and Technology | NLDET |

### Other Canadian Provinces/Territories
- Manitoba, Ontario, Quebec, New Brunswick, Nova Scotia, Prince Edward Island, Northwest Territories, Yukon, Nunavut

## Mexico

| Regulatory Authority | Abbreviation |
|---------------------|--------------|
| Comisión Nacional de Hidrocarburos (National Hydrocarbons Commission) | CNH |
| Agencia de Seguridad, Energía y Ambiente (Safety, Energy and Environment Agency) | ASEA |

### Mexican States
- Campeche, Tabasco, Veracruz, Tamaulipas, Chiapas

## Norway

| Regulatory Authority | Abbreviation |
|---------------------|--------------|
| Norwegian Petroleum Directorate | NPD |

**Note**: Norway operates at the national level (no state/province divisions).

## United Kingdom

| Regulatory Authority | Abbreviation |
|---------------------|--------------|
| North Sea Transition Authority (formerly Oil and Gas Authority) | NSTA |

**Note**: UK operates at the national level for offshore oil and gas (North Sea).

## Australia

| State/Territory | Regulatory Authority | Abbreviation |
|----------------|---------------------|--------------|
| National (Offshore) | National Offshore Petroleum Safety and Environmental Management Authority | NOPSEMA |
| Queensland | Department of Natural Resources, Mines and Energy | QLD_DNRME |
| Western Australia | Department of Mines, Industry Regulation and Safety | WA_DMIRS |
| Northern Territory | Department of Industry, Tourism and Trade | NT_DITT |
| South Australia | Department for Energy and Mining | SA_DMRE |

### Other Australian States
- Victoria, New South Wales, Tasmania

## Brazil

| Regulatory Authority | Abbreviation |
|---------------------|--------------|
| Agência Nacional do Petróleo (National Agency of Petroleum) | ANP |

### Brazilian States
- Rio de Janeiro, Espírito Santo, Bahia, Sergipe, Amazonas

## Argentina

| Province | Regulatory Authority | Abbreviation |
|----------|---------------------|--------------|
| Neuquén | Neuquén Province Government | ARG_NEUQUEN |
| Mendoza | Mendoza Province Government | ARG_MENDOZA |

### Other Argentine Provinces
- Chubut, Santa Cruz, Salta

## Nigeria

| Regulatory Authority | Abbreviation |
|---------------------|--------------|
| Department of Petroleum Resources | DPR |

### Nigerian States
- Rivers, Bayelsa, Delta, Akwa Ibom, Cross River

## Indonesia

| Regulatory Authority | Abbreviation |
|---------------------|--------------|
| Special Task Force for Upstream Oil and Gas Business Activities | SKKMigas |

### Indonesian Provinces
- Riau, East Kalimantan, South Sumatra, Aceh

## Kazakhstan

| Regulatory Authority | Abbreviation |
|---------------------|--------------|
| Kazakhstan Ministry of Energy | KZ_MOE |

### Kazakh Regions
- Atyrau, Mangystau, West Kazakhstan, Aktobe

## Additional Countries (Available for Future Expansion)

The system also includes enum values for other major oil and gas producing countries:
- Saudi Arabia
- United Arab Emirates
- Qatar
- Kuwait
- Russia
- China
- Venezuela
- Angola
- Colombia
- Ecuador
- Peru
- Egypt
- Libya
- Algeria
- Iraq
- Iran

## Adding New Jurisdictions

To add support for a new jurisdiction:

1. **Add Country** to `Country` enum
2. **Add State/Province** to `StateProvince` enum (if applicable)
3. **Add Regulatory Authority** to `RegulatoryAuthority` enum
4. **Add Constant** to `PermitConstants.cs`
5. **Update Mappers**:
   - `ApplicationMapper.MapRegulatoryAuthority()`
   - `ApplicationMapper.MapRegulatoryAuthorityToString()`
6. **Update JurisdictionHelper**:
   - `GetDefaultRegulatoryAuthority()`
   - `GetCountry()`
   - `GetStateProvince()`
   - `IsValidJurisdiction()`

## References

- [RRC Texas Forms](https://www.rrc.texas.gov/oil-and-gas/oil-and-gas-forms/)
- [AER Alberta Forms](https://www.aer.ca/applications-and-notices/application-processes/aer-forms/)
- [Alaska AOGCC](https://en.wikipedia.org/wiki/Alaska_Oil_and_Gas_Conservation_Commission)
- [BCER British Columbia](https://en.wikipedia.org/wiki/British_Columbia_Energy_Regulator)
- [Norwegian Petroleum Directorate](https://www.npd.no/)
- [UK NSTA](https://www.nstauthority.co.uk/)

