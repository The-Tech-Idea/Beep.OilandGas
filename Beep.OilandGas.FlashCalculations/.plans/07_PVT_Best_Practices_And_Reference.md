# PVT / flash best practices (reference)

## Screening vs laboratory / simulator

| Level | Use |
|-------|-----|
| **Screening** | **`FAST`** solver preset; simplified EOS or ideal K |
| **Facility design** | **`DEFAULT`** / **`STRICT`**; PR/SRK with validated BIPs |
| **Allocation / regulatory** | Full compositional model outside this library if required |

## EOS selection (rule of thumb)

| EOS | When |
|-----|------|
| **PR** | General vapor–liquid for hydrocarbons — common default |
| **SRK** | Legacy compatibility; same α formulations must match reference |
| **IDEAL_K** | Teaching / order-of-magnitude only |

## Specification types

- **PT_SPECIFIED** — matches primary **isothermal flash** path in this library.
- **PH_SPECIFIED** — reserved for extended enthalpy flash — document when implemented.

## Related standards

- API / GPA reports for **Z** and **gas** properties where overlapped with **`GasProperties`**.
