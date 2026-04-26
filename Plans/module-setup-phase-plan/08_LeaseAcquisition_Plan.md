# 08 — LeaseAcquisition: Best-Practice Audit & Revision Plan

## SP-A — Domain Audit Findings

### O&G Standards Applied
| Standard | Scope |
|---|---|
| PPDM 3.9 `LAND_PARCEL` / `CONTRACT` / `BUSINESS_ASSOCIATE` | Lease entity model |
| AAPL Model JOA 610 | Working interest / NRI conventions |
| BLM 43 CFR Part 3100 | Federal onshore lease (GOVERNMENT_LEASE) |
| BOEM 30 CFR Part 550 | Federal offshore lease class |
| Texas OGRC Statewide Rule 1 | State lease identification |

### Audit Findings

1. **Orphaned `EFFECTIVE_DATEValue` backing field (no getter/setter)** — `FEE_MINERAL_LEASE`, `GOVERNMENT_LEASE`, and `NET_PROFIT_LEASE` each declared a private `System.DateTime? EFFECTIVE_DATEValue` field without a corresponding public property. `ModelEntityBase` already provides `EFFECTIVE_DATE`; removed dead fields.

2. **Orphaned `REMARKValue` / `SOURCEValue` backing fields** — All three sub-lease classes had orphaned private `string REMARKValue` and `string SOURCEValue` fields in a `// Standard PPDM columns` comment block. `ModelEntityBase` already defines `REMARK` and `SOURCE`; removed dead fields.

3. **Orphaned `EXPIRY_DATEValue` in `NET_PROFIT_LEASE`** — Same pattern; removed.

4. **`LEASE_ACQUISITION` missing core O&G land identifier fields** — Added:
   - `LEASE_TYPE` (string: "FEE_MINERAL" / "FEDERAL" / "STATE" / "NET_PROFIT")
   - `GROSS_ACREAGE`, `NET_ACREAGE`, `ACREAGE_OUOM` (default `"ac"`)
   - `BONUS_AMOUNT`, `BONUS_AMOUNT_OUOM` (default `"USD"`) — lease signing bonus
   - `ROYALTY_RATE` — landowner royalty fraction
   - `DELAY_RENTAL_AMOUNT`, `RENTALS_DUE_DATE` — annual delay rental per AAPL JOA
   - `PRIMARY_TERM_MONTHS` — primary lease term
   - `HELD_BY_PRODUCTION_IND` (string Y/N, default `"N"`)
   - `LESSOR_BA_ID`, `LESSEE_BA_ID` — PPDM 3.9 BA references
   - `ACQUISITION_COST_OUOM` (default `"USD"`) — currency for `ACQUISITION_COST`

5. **`FEE_MINERAL_LEASE` missing fields** — Added:
   - `LESSEE_BA_ID` — company holding the lease
   - `COUNTY_ID`, `STATE_ID`, `COUNTRY_ID` — standard jurisdiction identifiers
   - `GROSS_ACREAGE`, `NET_ACREAGE`, `ACREAGE_OUOM`
   - `BONUS_AMOUNT`, `BONUS_AMOUNT_OUOM`
   - `DELAY_RENTAL_AMOUNT` — annual delay rental
   - `ACQUISITION_DATE`, `ACQUISITION_STATUS` — lease acquisition lifecycle

6. **`GOVERNMENT_LEASE` missing fields** — Added:
   - `LEASE_NAME` — human-readable lease identifier
   - `GROSS_ACREAGE`, `NET_ACREAGE`, `ACREAGE_OUOM`
   - `BONUS_AMOUNT`, `BONUS_AMOUNT_OUOM`
   - `HELD_BY_PRODUCTION_IND`
   - `PRIMARY_TERM_MONTHS`
   - `FEDERAL_LEASE_CLASS` — "ONSHORE_BLM" / "OFFSHORE_BOEM" / "INDIAN"
   - `COUNTY_ID`, `STATE_ID`

7. **`NET_PROFIT_LEASE` missing fields** — Added:
   - `COST_BASIS`, `COST_BASIS_OUOM` — capitalized cost base for NPI calculation
   - `HELD_BY_PRODUCTION_IND`

---

## SP-B — File Changes

| File | Change |
|---|---|
| `Modules/ProductionAccountingModuleSetup.cs` | Fixed constructor name (`ProductionModule` → `ProductionAccountingModuleSetup`) |
| `Data/Lease/Tables/LEASE_ACQUISITION.cs` | Added 14 O&G standard fields (lease type, acreage, bonus, royalty, rentals, HBP, BA IDs, cost UOM) |
| `Data/Lease/Tables/FEE_MINERAL_LEASE.cs` | Removed orphaned `EFFECTIVE_DATEValue`/`REMARKValue`/`SOURCEValue`; added 9 fields |
| `Data/Lease/Tables/GOVERNMENT_LEASE.cs` | Removed orphaned `EFFECTIVE_DATEValue`/`REMARKValue`/`SOURCEValue`; added 10 fields |
| `Data/Lease/Tables/NET_PROFIT_LEASE.cs` | Removed orphaned `EFFECTIVE_DATEValue`/`EXPIRY_DATEValue`/`REMARKValue`/`SOURCEValue`; added 3 NPI fields |

---

## SP-C — EntityTypes Registry

`Modules/LeaseAcquisitionModule.cs` — **unchanged** — registers 4 table types:

```
LEASE_ACQUISITION, FEE_MINERAL_LEASE, GOVERNMENT_LEASE, NET_PROFIT_LEASE
```

Order 45 — before Exploration (50), after Security (40).

---

## Build Result

```
dotnet build Beep.OilandGas.LeaseAcquisition/Beep.OilandGas.LeaseAcquisition.csproj -v q
→ 0 Error(s)   0 Warning(s)   ✓
```
