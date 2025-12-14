# Oil and Gas Industry File Formats

## Overview

This document provides a comprehensive overview of file formats used in the oil and gas industry, categorized by data type and usage.

## Currently Implemented Formats

### ✅ Log Data Formats
- **LAS (Log ASCII Standard)** v2.0/v3.0 - ✅ Implemented (`LasLogLoader`)
- **CSV Log Files** - ✅ Implemented (`CsvLogLoader`)
- **Database Logs** - ✅ Implemented (`DatabaseLogLoader`)

### ✅ Reservoir/Geological Formats
- **RESQML v2.2** - ✅ Implemented (`ResqmlReservoirLoader`)
- **PPDM38/PPDM39** - ✅ Partially Implemented (`Ppdm38LayerLoader`, `Ppdm38SchematicLoader`)

### ✅ Well Schematic Formats
- **PPDM38 Database** - ✅ Implemented (`Ppdm38SchematicLoader`)
- **SeaBed** - ✅ Implemented (`SeaBedSchematicLoader`)
- **File-based (JSON/XML)** - ✅ Implemented (`FileSchematicLoader`)

### ✅ Data Acquisition
- **WITSML DataWorkOrder v1.0** - ✅ Implemented (`DataWorkOrderLoader`)

### ✅ Standards Integration
- **PWLS v3.0** - ✅ Implemented (`PwlsMnemonicMapper`)

---

## Additional Important Formats

### 1. Log Data Formats

#### DLIS (Digital Log Interchange Standard) / RP66
- **Standard**: API RP66 (Recommended Practice 66)
- **Type**: Binary format
- **Usage**: Industry-standard binary format for well log data
- **Advantages**: 
  - More efficient than LAS (binary)
  - Supports complex data structures
  - Better for large datasets
  - Standard format for LWD (Logging While Drilling) data
- **File Extensions**: `.dlis`, `.lis`
- **Priority**: **HIGH** - Very common in industry, especially for LWD data

#### WITSML Log
- **Standard**: WITSML v1.4/v2.1
- **Type**: XML format
- **Usage**: Well log data in WITSML format
- **Advantages**:
  - Integrated with WITSML ecosystem
  - Supports real-time data
  - Rich metadata
- **File Extensions**: `.xml`, `.witsml`
- **Priority**: **HIGH** - Part of WITSML standard we're already using

#### ASCII Well Data (Various Formats)
- **Types**: 
  - Generic ASCII with headers
  - Tab-delimited
  - Space-delimited
  - Custom formats
- **Usage**: Legacy data, custom formats
- **Priority**: **MEDIUM** - Useful for legacy data migration

#### WellCAD Format
- **Standard**: Altus Geoscience WellCAD
- **Type**: Binary/XML
- **Usage**: WellCAD software export format
- **Priority**: **LOW** - Proprietary format

#### Kingdom/OpenWorks Formats
- **Standards**: IHS Markit Kingdom, Landmark OpenWorks
- **Type**: Proprietary binary/ASCII
- **Usage**: Legacy data from major software platforms
- **Priority**: **LOW** - Proprietary, less common

---

### 2. Seismic Data Formats

#### SEG-Y (SEG Y)
- **Standard**: Society of Exploration Geophysicists
- **Type**: Binary format
- **Usage**: 2D/3D seismic reflection data
- **Advantages**:
  - Industry standard for seismic data
  - Supports large 3D volumes
  - Widely supported
- **File Extensions**: `.sgy`, `.segy`
- **Priority**: **MEDIUM** - Important for geophysical analysis

#### SEG-2
- **Standard**: Society of Exploration Geophysicists
- **Type**: Binary format
- **Usage**: 2D seismic data (older format)
- **Priority**: **LOW** - Legacy format, SEG-Y preferred

#### SEG-D
- **Standard**: Society of Exploration Geophysicists
- **Type**: Binary format
- **Usage**: Raw seismic acquisition data
- **Priority**: **LOW** - Specialized use case

---

### 3. Production Data Formats

#### PRODML (Production Markup Language)
- **Standard**: Energistics PRODML v2.0+
- **Type**: XML format
- **Usage**: Production data, well tests, flow rates
- **Advantages**:
  - Industry standard (Energistics)
  - Integrated with WITSML/RESQML
  - Comprehensive production data model
- **File Extensions**: `.xml`, `.prodml`
- **Priority**: **HIGH** - Important for production analysis

#### ASCII Production Data
- **Types**: Various CSV/text formats
- **Usage**: Production reports, daily production
- **Priority**: **MEDIUM** - Common but less standardized

#### Excel Production Reports
- **Type**: Microsoft Excel
- **Usage**: Production reports, well summaries
- **Priority**: **MEDIUM** - Very common in industry

---

### 4. Well Data Formats

#### WITSML (Full Implementation)
- **Standard**: WITSML v1.4, v2.0, v2.1
- **Type**: XML format
- **Usage**: Comprehensive well data (we have DataWorkOrder, but need full WITSML)
- **Components**:
  - Well
  - Wellbore
  - Log
  - Trajectory
  - MudLog
  - CementJob
  - BhaRun
  - And many more...
- **Priority**: **HIGH** - Industry standard, we have partial support

#### PPDM (Full Implementation)
- **Standard**: PPDM v3.8, v3.9
- **Type**: Database schema
- **Usage**: Comprehensive petroleum data model
- **Priority**: **MEDIUM** - We have partial support, could expand

---

### 5. Reservoir/Geological Formats

#### RESQML (Additional Versions)
- **Standard**: RESQML v2.0, v2.1, v2.2, v2.3
- **Type**: XML/EPC format
- **Usage**: Reservoir models (we have v2.2)
- **Priority**: **MEDIUM** - Could add v2.3 support

#### Petrel Formats
- **Standard**: Schlumberger Petrel
- **Type**: Proprietary binary
- **Usage**: Petrel software export
- **Priority**: **LOW** - Proprietary

#### RMS Formats
- **Standard**: Emerson RMS
- **Type**: Proprietary
- **Usage**: RMS software export
- **Priority**: **LOW** - Proprietary

---

### 6. Directional Survey Formats

#### ASCII Survey Formats
- **Types**: Various text formats
- **Usage**: Well trajectory data
- **Common Formats**:
  - MD, Inc, Azim
  - MD, TVD, N/S, E/W
  - Custom formats
- **Priority**: **MEDIUM** - Important for deviated wells

#### WITSML Trajectory
- **Standard**: WITSML
- **Type**: XML
- **Usage**: Well trajectory in WITSML format
- **Priority**: **HIGH** - Part of WITSML standard

---

### 7. Core Data Formats

#### ASCII Core Data
- **Types**: Various text formats
- **Usage**: Core analysis data (porosity, permeability, saturation)
- **Priority**: **MEDIUM** - Important for reservoir characterization

#### Excel Core Reports
- **Type**: Microsoft Excel
- **Usage**: Core analysis reports
- **Priority**: **MEDIUM** - Common format

---

### 8. Pressure/Test Data Formats

#### ASCII Test Data
- **Types**: Various formats
- **Usage**: Well test data, pressure data
- **Priority**: **MEDIUM** - Important for reservoir analysis

#### WITSML WellTest
- **Standard**: WITSML
- **Type**: XML
- **Usage**: Well test data in WITSML format
- **Priority**: **HIGH** - Part of WITSML standard

---

### 9. Drilling Data Formats

#### WITSML MudLog
- **Standard**: WITSML
- **Type**: XML
- **Usage**: Mud logging data
- **Priority**: **HIGH** - Part of WITSML standard

#### WITSML BhaRun
- **Standard**: WITSML
- **Type**: XML
- **Usage**: BHA (Bottom Hole Assembly) data
- **Priority**: **MEDIUM** - Part of WITSML standard

#### WITSML CementJob
- **Standard**: WITSML
- **Type**: XML
- **Usage**: Cement job data
- **Priority**: **MEDIUM** - Part of WITSML standard

---

### 10. Spatial/Positioning Formats

#### IOGP P1/P2/P6 Formats
- **Standard**: International Association of Oil & Gas Producers
- **Type**: ASCII/XML
- **Usage**: Geophysical positioning data
- **Priority**: **LOW** - Specialized use case

#### SSDM (Seabed Survey Data Model)
- **Standard**: Industry standard
- **Type**: Esri ArcGIS geodatabase
- **Usage**: Seabed survey data
- **Priority**: **LOW** - Specialized use case

---

### 11. Business/Transaction Formats

#### PIDX (Petroleum Industry Data Exchange)
- **Standard**: PIDX
- **Type**: XML
- **Usage**: Electronic business transactions
- **Priority**: **LOW** - Business/transaction focused, not technical data

#### PDEF (Pipeline Data Exchange Format)
- **Standard**: Industry standard
- **Type**: XML/JSON
- **Usage**: Subsea pipeline facility data
- **Priority**: **LOW** - Specialized use case

---

## Recommended Implementation Priority

### Priority 1: High Value, High Usage
1. **DLIS/RP66 Loader** - Binary log format, very common, especially for LWD
2. **WITSML Full Implementation** - Complete WITSML support (Log, Trajectory, MudLog, WellTest, etc.)
3. **PRODML Loader** - Production data standard (Energistics)

### Priority 2: Medium Value, Common Usage
4. **ASCII Survey Loader** - Well trajectory data (various formats)
5. **SEG-Y Loader** - Seismic data (if seismic analysis is needed)
6. **Excel Loader** - Production reports, core data (very common)
7. **WITSML Trajectory** - Well trajectory in WITSML format

### Priority 3: Lower Priority
8. **ASCII Core Data Loader** - Core analysis data
9. **ASCII Test Data Loader** - Well test data
10. **RESQML v2.3** - Latest RESQML version

---

## Format Comparison

| Format | Type | Standard | Common Use | Priority |
|--------|------|----------|------------|----------|
| **LAS** | ASCII | CWLS | Well logs | ✅ Implemented |
| **DLIS/RP66** | Binary | API RP66 | Well logs (LWD) | 🔴 High |
| **WITSML** | XML | Energistics | Comprehensive well data | 🟡 Partial |
| **RESQML** | XML/EPC | Energistics | Reservoir models | ✅ Implemented |
| **PRODML** | XML | Energistics | Production data | 🔴 High |
| **PPDM** | Database | PPDM | Comprehensive data model | 🟡 Partial |
| **SEG-Y** | Binary | SEG | Seismic data | 🟢 Medium |
| **CSV** | ASCII | Generic | Various data | ✅ Implemented |
| **Excel** | Binary | Microsoft | Reports, summaries | 🟢 Medium |
| **PWLS** | Standard | Energistics | Mnemonic mapping | ✅ Implemented |

---

## Implementation Recommendations

### Immediate Next Steps (High Priority)

1. **DLIS/RP66 Log Loader**
   - Binary format parser
   - Support for LWD data
   - Integration with existing log loader interface

2. **WITSML Full Implementation**
   - WITSML Log loader
   - WITSML Trajectory loader
   - WITSML MudLog loader
   - WITSML WellTest loader
   - WITSML Well/Wellbore loader

3. **PRODML Loader**
   - Production data loader
   - Well test data
   - Flow rate data
   - Integration with DCA analysis

### Medium-Term (Medium Priority)

4. **ASCII Survey Loader**
   - Multiple format support
   - MD/Inc/Azim format
   - MD/TVD/N/S/E/W format
   - Custom format support

5. **Excel Loader**
   - Production reports
   - Core data
   - Well summaries
   - Flexible sheet parsing

6. **SEG-Y Loader** (if seismic analysis needed)
   - Seismic data loading
   - 2D/3D support
   - Header parsing

---

## Format Details

### DLIS (Digital Log Interchange Standard)

**Structure**:
- Binary format with logical file structure
- Contains multiple logical files
- Each logical file has multiple logical records
- Supports complex data types and structures

**Key Features**:
- More efficient than LAS (binary)
- Supports image data
- Better metadata support
- Standard for LWD data

**Implementation Complexity**: High (binary parsing required)

### WITSML (Full Implementation)

**Key Objects**:
- `well` - Well information
- `wellbore` - Wellbore information
- `log` - Well log data
- `trajectory` - Well trajectory
- `mudLog` - Mud logging data
- `wellTest` - Well test data
- `cementJob` - Cement job data
- `bhaRun` - BHA run data
- `rig` - Rig information
- And many more...

**Implementation Complexity**: Medium-High (many object types)

### PRODML

**Key Objects**:
- `ProductionOperation` - Production operations
- `WellTest` - Well test data
- `Flow` - Flow data
- `Fluid` - Fluid properties
- `Facility` - Production facilities

**Implementation Complexity**: Medium

---

## Summary

There are **many** additional file formats used in the oil and gas industry. The most important ones to implement next would be:

1. **DLIS/RP66** - For binary log data (especially LWD)
2. **WITSML Full** - Complete WITSML support
3. **PRODML** - Production data standard

These three formats would significantly expand the framework's capabilities and industry compatibility.

