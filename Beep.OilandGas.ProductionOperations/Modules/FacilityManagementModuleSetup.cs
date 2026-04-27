using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionOperations.Modules
{
    /// <summary>
    /// Module order 80 — Facility Management.
    /// This module extends PPDM39 facility lifecycle with vertical monitoring tables and reference sets
    /// while continuing to use core PPDM entities (for example <c>FACILITY</c>, <c>WORK_ORDER</c>,
    /// <c>PDEN</c>) owned by the core model / other setup modules.
    /// License and maintenance picklists in PPDM39 live in <c>R_*</c> tables (for example
    /// <see cref="R_LICENSE_STATUS"/>, <see cref="R_FAC_MAINT_STATUS"/>); transactional rows use
    /// <c>FACILITY_LIC_STATUS</c>, <c>FACILITY_MAINT_STATUS</c>, etc. Reference seeding for those
    /// <c>R_*</c> sets can be added alongside facility-management service work (Sprint 1 in the plan).
    /// </summary>
    public sealed class FacilityManagementModuleSetup : ModuleSetupBase
    {
        // SeedScope:
        // - Tables: FACILITY_MEASUREMENT, FACILITY_EQUIPMENT_ACTIVITY
        // - Core: R_FACILITY_MONITORING_CODE (vertical monitoring/action reference sets)
        private static readonly IReadOnlyList<Type> _entityTypes = new[]
        {
            typeof(FACILITY_MEASUREMENT),
            typeof(FACILITY_EQUIPMENT_ACTIVITY),
            typeof(R_FACILITY_MONITORING_CODE)
        };

        public FacilityManagementModuleSetup(ModuleSetupContext context) : base(context) { }

        public override string ModuleId => "FACILITY";
        public override string ModuleName => "Facility Management";
        public override int Order => 80;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override async Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            await SeedEquipmentCatalogAsync(connectionName, userId, result, cancellationToken);
            await SeedFacilityMonitoringReferenceCodesAsync(connectionName, userId, result, cancellationToken);
            result.Success = true;
            return result;
        }

        private async Task SeedEquipmentCatalogAsync(
            string connectionName, string userId, ModuleSetupResult result, CancellationToken ct)
        {
            var repo = GetRepo<CAT_EQUIPMENT>("CAT_EQUIPMENT", connectionName);
            var equipment = new[]
            {
                ("SEP-2PH", "Two-Phase Separator", "Separates gas and liquid phases", "SEPARATOR"),
                ("SEP-3PH", "Three-Phase Separator", "Separates gas, oil, and water", "SEPARATOR"),
                ("SEP-TEST", "Test Separator", "Separator for well testing", "SEPARATOR"),
                ("SEP-PROD", "Production Separator", "Main production separator", "SEPARATOR"),
                ("SEP-SCRUB", "Scrubber", "Gas scrubber for liquid removal", "SEPARATOR"),
                ("SEP-FLARE", "Flare Knockout Drum", "Flare system knockout drum", "SEPARATOR"),
                ("COMP-RECIP", "Reciprocating Compressor", "Reciprocating gas compressor", "COMPRESSOR"),
                ("COMP-SCREW", "Screw Compressor", "Screw-type gas compressor", "COMPRESSOR"),
                ("COMP-CENT", "Centrifugal Compressor", "Centrifugal gas compressor", "COMPRESSOR"),
                ("COMP-BOOST", "Booster Compressor", "Booster compressor station", "COMPRESSOR"),
                ("PUMP-CENT", "Centrifugal Pump", "Centrifugal liquid pump", "PUMP"),
                ("PUMP-RECIP", "Reciprocating Pump", "Reciprocating positive displacement pump", "PUMP"),
                ("PUMP-SCREW", "Screw Pump", "Screw-type positive displacement pump", "PUMP"),
                ("PUMP-PROG", "Progressive Cavity Pump", "Progressive cavity pump", "PUMP"),
                ("PUMP-SUB", "Submersible Pump", "Electric submersible pump", "PUMP"),
                ("PUMP-INJ", "Injection Pump", "Water/gas injection pump", "PUMP"),
                ("HX-SHELL", "Shell and Tube HX", "Shell and tube heat exchanger", "HEAT_EXCHANGER"),
                ("HX-PLATE", "Plate Heat Exchanger", "Plate-type heat exchanger", "HEAT_EXCHANGER"),
                ("HX-AIR", "Air Cooler", "Air-cooled heat exchanger", "HEAT_EXCHANGER"),
                ("HX-FIRE", "Fire Tube Heater", "Fire tube heater/treater", "HEAT_EXCHANGER"),
                ("HX-WATER", "Water Bath Heater", "Water bath heater", "HEAT_EXCHANGER"),
                ("DEHY-GLYCOL", "Glycol Dehydrator", "TEG glycol dehydration unit", "DEHYDRATION"),
                ("DEHY-DESICCANT", "Desiccant Dehydrator", "Solid desiccant dehydration unit", "DEHYDRATION"),
                ("DEHY-MEMBRANE", "Membrane Dehydrator", "Membrane dehydration unit", "DEHYDRATION"),
                ("SWEET-AMINE", "Amine Sweetening Unit", "Amine gas sweetening unit", "SWEETENING"),
                ("SWEET-SCAV", "Scavenger Unit", "H2S scavenger injection system", "SWEETENING"),
                ("SWEET-MEMBRANE", "Membrane Sweetening", "Membrane gas sweetening unit", "SWEETENING"),
                ("METER-ORIFICE", "Orifice Meter", "Orifice plate flow meter", "METERING"),
                ("METER-TURBINE", "Turbine Meter", "Turbine flow meter", "METERING"),
                ("METER-ULTRASONIC", "Ultrasonic Meter", "Ultrasonic flow meter", "METERING"),
                ("METER-CORIOLIS", "Coriolis Meter", "Coriolis mass flow meter", "METERING"),
                ("METER-POSITIVE", "Positive Displacement Meter", "Positive displacement meter", "METERING"),
                ("METER-VORTEX", "Vortex Meter", "Vortex shedding flow meter", "METERING"),
                ("TANK-CRUDE", "Crude Oil Tank", "Crude oil storage tank", "STORAGE"),
                ("TANK-PROD", "Produced Water Tank", "Produced water storage tank", "STORAGE"),
                ("TANK-GLYCOL", "Glycol Storage Tank", "Glycol storage tank", "STORAGE"),
                ("TANK-CHEM", "Chemical Storage Tank", "Chemical storage tank", "STORAGE"),
                ("TANK-FUEL", "Fuel Gas Tank", "Fuel gas storage vessel", "STORAGE"),
                ("GEN-GAS", "Gas Generator", "Gas-fired power generator", "POWER"),
                ("GEN-DIESEL", "Diesel Generator", "Diesel power generator", "POWER"),
                ("GEN-SOLAR", "Solar Generator", "Solar power generator", "POWER"),
                ("GEN-WIND", "Wind Generator", "Wind power generator", "POWER"),
                ("FLARE-GROUND", "Ground Flare", "Ground-level flare system", "FLARE"),
                ("FLARE-ELEVATED", "Elevated Flare", "Elevated flare stack", "FLARE"),
                ("FLARE-ENCLOSED", "Enclosed Flare", "Enclosed ground flare", "FLARE"),
                ("WT-CORRUGATED", "Corrugated Plate Interceptor", "CPI water treatment unit", "WATER_TREATMENT"),
                ("WT-HYDROCYCLONE", "Hydrocyclone", "Hydrocyclone water treatment", "WATER_TREATMENT"),
                ("WT-FILTER", "Filter Unit", "Water filtration unit", "WATER_TREATMENT"),
                ("WT-DAF", "Dissolved Air Flotation", "DAF water treatment unit", "WATER_TREATMENT"),
                ("INST-PRESSURE", "Pressure Transmitter", "Pressure measurement transmitter", "INSTRUMENT"),
                ("INST-TEMP", "Temperature Transmitter", "Temperature measurement transmitter", "INSTRUMENT"),
                ("INST-FLOW", "Flow Transmitter", "Flow measurement transmitter", "INSTRUMENT"),
                ("INST-LEVEL", "Level Transmitter", "Level measurement transmitter", "INSTRUMENT"),
                ("INST-ANALYZER", "Gas Analyzer", "Gas composition analyzer", "INSTRUMENT"),
                ("INST-CONTROL", "Control Valve", "Process control valve", "INSTRUMENT"),
                ("PIPE-SEGMENT", "Pipeline Segment", "Pipeline pipe segment", "PIPELINE"),
                ("PIPE-VALVE", "Pipeline Valve", "Pipeline isolation valve", "PIPELINE"),
                ("PIPE-REGULATOR", "Pressure Regulator", "Pipeline pressure regulator", "PIPELINE"),
                ("PIPE-SCRAPER", "Scraper Trap", "Pipeline scraper/pig trap", "PIPELINE"),
                ("WH-CHRISTMAS", "Christmas Tree", "Wellhead Christmas tree assembly", "WELLHEAD"),
                ("WH-CHOKE", "Wellhead Choke", "Wellhead flow choke", "WELLHEAD"),
                ("WH-SAFETY", "Safety Valve", "Wellhead safety valve", "WELLHEAD"),
                ("WH-MANIFOLD", "Wellhead Manifold", "Wellhead manifold assembly", "WELLHEAD"),
            };

            foreach (var (code, name, desc, category) in equipment)
            {
                ct.ThrowIfCancellationRequested();
                var filter = new AppFilter { FieldName = "CATALOGUE_EQUIP_ID", Operator = "=", FilterValue = code };
                if (await SkipIfExistsAsync(repo, filter)) continue;

                var entity = new CAT_EQUIPMENT
                {
                    CATALOGUE_EQUIP_ID = code,
                    EQUIPMENT_NAME = name,
                    REMARK = desc,
                    CAT_EQUIP_TYPE = category,
                    ACTIVE_IND = "Y"
                };
                await TryInsertAsync(repo, entity, userId, result, $"CatalogEquip:{code}");
            }
        }

        private async Task SeedFacilityMonitoringReferenceCodesAsync(
            string connectionName, string userId, ModuleSetupResult result, CancellationToken ct)
        {
            var repo = GetRepo<R_FACILITY_MONITORING_CODE>("R_FACILITY_MONITORING_CODE", connectionName);
            var rows = new (string Set, string Code, string LongName, string ShortName)[]
            {
                ("EQUIPMENT_ACTIVITY_TYPE", "INSTALL", "Equipment Installed", "INSTALL"),
                ("EQUIPMENT_ACTIVITY_TYPE", "UNINSTALL", "Equipment Uninstalled", "UNINSTALL"),
                ("EQUIPMENT_ACTIVITY_TYPE", "MOVE", "Equipment Moved", "MOVE"),
                ("EQUIPMENT_ACTIVITY_TYPE", "REPLACE", "Equipment Replaced", "REPLACE"),

                ("MEASUREMENT_TYPE", "TANK_LEVEL", "Tank Level", "TANK_LEVEL"),
                ("MEASUREMENT_TYPE", "FLOW_RATE", "Flow Rate", "FLOW_RATE"),
                ("MEASUREMENT_TYPE", "PRESSURE", "Pressure", "PRESSURE"),
                ("MEASUREMENT_TYPE", "TEMPERATURE", "Temperature", "TEMPERATURE"),
                ("MEASUREMENT_TYPE", "VIBRATION", "Vibration", "VIBRATION"),
                ("MEASUREMENT_TYPE", "POWER_DRAW", "Power Draw", "POWER_DRAW"),

                ("MEASUREMENT_QUALITY", "MEASURED", "Directly Measured", "MEASURED"),
                ("MEASUREMENT_QUALITY", "ESTIMATED", "Estimated", "ESTIMATED"),
                ("MEASUREMENT_QUALITY", "IMPUTED", "Imputed", "IMPUTED"),

                ("MEASUREMENT_UOM", "PERCENT", "Percent", "%"),
                ("MEASUREMENT_UOM", "M", "Meter", "M"),
                ("MEASUREMENT_UOM", "FT", "Foot", "FT"),
                ("MEASUREMENT_UOM", "PSI", "Pressure (PSI)", "PSI"),
                ("MEASUREMENT_UOM", "DEG_C", "Degrees Celsius", "C"),
                ("MEASUREMENT_UOM", "DEG_F", "Degrees Fahrenheit", "F"),
            };

            foreach (var (setName, code, longName, shortName) in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    await UpsertFacilityMonitoringReferenceCodeIfMissingAsync(repo, setName, code, longName, shortName, userId);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"MonitoringRef:{setName}:{code}: {ex.Message}");
                }
            }
        }

        private async Task UpsertFacilityMonitoringReferenceCodeIfMissingAsync(
            dynamic repo,
            string referenceSet,
            string referenceCode,
            string longName,
            string shortName,
            string userId)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "REFERENCE_SET", Operator = "=", FilterValue = referenceSet },
                new AppFilter { FieldName = "REFERENCE_CODE", Operator = "=", FilterValue = referenceCode }
            };

            var existing = await repo.GetAsync(filters);
            foreach (var _ in existing) return;

            var row = new R_FACILITY_MONITORING_CODE
            {
                REFERENCE_SET = referenceSet,
                REFERENCE_CODE = referenceCode,
                LONG_NAME = longName,
                SHORT_NAME = shortName,
                ACTIVE_IND = "Y"
            };
            await repo.InsertAsync(row, userId);
        }
    }
}
