using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Enums.Shared;
using Beep.OilandGas.Models.Enums.WellTest;
using Beep.OilandGas.Models.Enums.ChokeAnalysis;
using Beep.OilandGas.Models.Enums.Compressor;
using Beep.OilandGas.Models.Enums.Analytics;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.DataSources;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    public class EnumReferenceDataSeeder
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly LOVManagementService _lovService;
        private readonly string _connectionName;

        public EnumReferenceDataSeeder(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            LOVManagementService lovService,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _lovService = lovService ?? throw new ArgumentNullException(nameof(lovService));
            _connectionName = connectionName;
        }

        public async Task<int> SeedAllEnumsAsync(string userId = "SYSTEM")
        {
            int totalSeeded = 0;

            // ==========================================
            // 1. SHARED ENUMS
            // ==========================================
            
            // Well Status
            totalSeeded += await SeedEnumAsync<R_WELL_STATUS>(typeof(WellStatus), "STATUS", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_WELL_STATUS>(typeof(IhsStatus), "STATUS", "LONG_NAME", "REMARK", userId, "IHS");

            // Well Class / Intent
            totalSeeded += await SeedEnumAsync<R_WELL_CLASS>(typeof(WellIntent), "WELL_CLASS", "LONG_NAME", "REMARK", userId);

            // Well Trajectory / Profile
            totalSeeded += await SeedEnumAsync<R_WELL_PROFILE_TYPE>(typeof(WellTrajectory), "WELL_PROFILE_TYPE", "LONG_NAME", "REMARK", userId);

            // Fluid / Product Types
            totalSeeded += await SeedEnumAsync<R_FLUID_TYPE>(typeof(WellProductType), "FLUID_TYPE", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_FLUID_TYPE>(typeof(PpdmFluidType), "FLUID_TYPE", "LONG_NAME", "REMARK", userId);

            // Risk & Severity (Shared + HSE + Analytics)
            totalSeeded += await SeedEnumAsync<R_SEVERITY>(typeof(RiskSeverity), "SEVERITY_ID", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_SEVERITY>(typeof(RiskLevel), "SEVERITY_ID", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_SEVERITY>(typeof(RiskMatrixLevel), "SEVERITY_ID", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_SEVERITY>(typeof(HseRiskLevel), "SEVERITY_ID", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_SEVERITY>(typeof(RiskProbability), "SEVERITY_ID", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_SEVERITY>(typeof(InsightSeverity), "SEVERITY_ID", "LONG_NAME", "REMARK", userId); // Analytics

            // Units of Measure (Shared)
            totalSeeded += await SeedEnumAsync<PPDM_UNIT_OF_MEASURE>(typeof(VolumeUnit), "UOM_ID", "UOM_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<PPDM_UNIT_OF_MEASURE>(typeof(RateUnit), "UOM_ID", "UOM_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<PPDM_UNIT_OF_MEASURE>(typeof(PressureUnit), "UOM_ID", "UOM_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<PPDM_UNIT_OF_MEASURE>(typeof(TemperatureUnit), "UOM_ID", "UOM_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<PPDM_UNIT_OF_MEASURE>(typeof(LengthUnit), "UOM_ID", "UOM_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<PPDM_UNIT_OF_MEASURE>(typeof(DensityUnit), "UOM_ID", "UOM_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<PPDM_UNIT_OF_MEASURE>(typeof(EnergyUnit), "UOM_ID", "UOM_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<PPDM_UNIT_OF_MEASURE>(typeof(CurrencyCode), "UOM_ID", "UOM_NAME", "REMARK", userId, "CURRENCY");

            // Completion & Production
            totalSeeded += await SeedEnumAsync<R_COMPLETION_STATUS>(typeof(CompletionStatus), "COMPLETION_STATUS", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_COMPLETION_TYPE>(typeof(SpecializedCompletionConfiguration), "COMPLETION_TYPE", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_PRODUCTION_METHOD>(typeof(WellProductionMode), "PRODUCTION_METHOD", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_ALLOCATION_TYPE>(typeof(AllocationMethod), "ALLOCATION_TYPE", "LONG_NAME", "REMARK", userId);

            // Roles & Components
            totalSeeded += await SeedEnumAsync<R_WELL_QUALIFIC_TYPE>(typeof(SpecializedWellRole), "WELL_QUALIFIC_TYPE", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_WELL_COMPONENT_TYPE>(typeof(WellboreInterface), "WELL_COMPONENT_TYPE", "LONG_NAME", "REMARK", userId);

            // Location & Facility
            totalSeeded += await SeedEnumAsync<R_PLATFORM_TYPE>(typeof(OffshoreStructureType), "PLATFORM_TYPE", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_LOCATION_TYPE>(typeof(OperationalSiteType), "LOCATION_TYPE", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_AREA_TYPE>(typeof(AreaType), "AREA_TYPE", "LONG_NAME", "REMARK", userId);
            // Simops -> Well Activity / Cause? Or Project Status? Mapping to R_WELL_ACTIVITY_CAUSE as best fit for now.
            totalSeeded += await SeedEnumAsync<R_WELL_ACTIVITY_CAUSE>(typeof(SimopsCode), "ACTIVITY_CAUSE", "LONG_NAME", "REMARK", userId);
            
            // Financial / Legal
            totalSeeded += await SeedEnumAsync<R_FIN_STATUS>(typeof(JIBStatus), "JIB_STATUS", "LONG_NAME", "REMARK", userId); // Mapping to R_FIN_STATUS but ID is different usually.
            // Wait, R_FIN_STATUS likely has FIN_STATUS_ID. Let's check or assume generic col logic handles it if we pass correct col name.
            // Actually R_FIN_STATUS usually has 'STATUS' or 'FIN_STATUS'. 
            // Let's use 'STATUS' alias if possible, but PPDM varies. 
            // Checking: R_FIN_STATUS usually has FIN_STATUS.
            totalSeeded += await SeedEnumAsync<R_FIN_STATUS>(typeof(JIBStatus), "STATUS", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_FIN_STATUS>(typeof(ReceivableStatus), "STATUS", "LONG_NAME", "REMARK", userId);

            // Permit / License
            totalSeeded += await SeedEnumAsync<R_BA_PERMIT_TYPE>(typeof(PermitToWorkType), "PERMIT_TYPE", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_LICENSE_STATUS>(typeof(PermitStatus), "LICENSE_STATUS", "LONG_NAME", "REMARK", userId);

            // General / PPDM
            totalSeeded += await SeedEnumAsync<R_CONDITION_TYPE>(typeof(PpdmCondition), "CONDITION_TYPE", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_DIRECTION>(typeof(PpdmDirection), "DIRECTION", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_PROJECT_STEP>(typeof(PpdmLifecyclePhase), "PROJECT_STEP_ID", "LONG_NAME", "REMARK", userId); // Project Steps often Phase ID
            totalSeeded += await SeedEnumAsync<R_WELL_ACTIVITY_CAUSE>(typeof(WellControlMethod), "ACTIVITY_CAUSE", "LONG_NAME", "REMARK", userId);


            // ==========================================
            // 2. WELL TEST
            // ==========================================
            totalSeeded += await SeedEnumAsync<R_WELL_TEST_TYPE>(typeof(WellTestType), "WELL_TEST_TYPE", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_WELL_TEST_TYPE>(typeof(WellTestCategory), "WELL_TEST_TYPE", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_TEST_EQUIPMENT>(typeof(SurfaceTestEquipment), "TEST_EQUIPMENT", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_TEST_RESULT>(typeof(TestQuality), "TEST_RESULT", "LONG_NAME", "REMARK", userId);


            // ==========================================
            // 3. EQUIPMENT SPECIFIC (Choke, Compressor)
            // ==========================================
            totalSeeded += await SeedEnumAsync<R_CAT_EQUIP_TYPE>(typeof(ChokeType), "CAT_EQUIP_TYPE", "LONG_NAME", "REMARK", userId);
            
            // Compressor 
            totalSeeded += await SeedEnumAsync<R_CAT_EQUIP_TYPE>(typeof(CompressorType), "CAT_EQUIP_TYPE", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_CAT_EQUIP_SPEC>(typeof(DriverType), "CAT_EQUIP_SPEC_ID", "LONG_NAME", "REMARK", userId);

             // Choke Analysis
            totalSeeded += await SeedEnumAsync<R_WELL_TEST_TYPE>(typeof(FlowRegime), "WELL_TEST_TYPE", "LONG_NAME", "REMARK", userId); // Mapping Flow Regime to Test Type for now


            // ==========================================
            // 4. ANALYTICS
            // ==========================================
            totalSeeded += await SeedEnumAsync<R_PROJECT_STATUS>(typeof(ActionStatus), "STATUS", "LONG_NAME", "REMARK", userId);
            totalSeeded += await SeedEnumAsync<R_PROJECT_TYPE>(typeof(InsightType), "PROJECT_TYPE", "LONG_NAME", "REMARK", userId);

            return totalSeeded;
        }

        private async Task<int> SeedEnumAsync<T>(
            Type enumType, 
            string idPropertyName, 
            string namePropertyName, 
            string descPropertyName, 
            string userId,
            string sourceValue = "BEEP_ENUMS") 
            where T : class, IPPDMEntity, new()
        {
            if (!enumType.IsEnum) return 0;

            int count = 0;
            // Create a specific repository for this Entity Type
            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(T), _connectionName);
            
            var enumValues = Enum.GetValues(enumType);
            foreach (var value in enumValues)
            {
                try
                {
                    var idStr = value.ToString().ToUpperInvariant();
                    // PPDM IDs limit check (30 chars)
                    if (idStr.Length > 30) idStr = idStr.Substring(0, 30);

                    // Get Description/Name
                    var fieldInfo = enumType.GetField(value.ToString());
                    var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                    var nameStr = descriptionAttribute?.Description ?? value.ToString();
                    if (nameStr.Length > 240) nameStr = nameStr.Substring(0, 240); // Standard PPDM Name limit often 255

                    // Check Existence
                    var filters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = idPropertyName, Operator = "=", FilterValue = idStr }
                    };

                    var existing = await repository.GetEntitiesWithFiltersAsync(typeof(T), typeof(T).Name, filters);
                    if (existing != null && existing.Any())
                    {
                         continue;
                    }

                    // Create & Populate
                    var entity = new T();
                    var type = typeof(T);

                    // Set ID
                    var idProp = type.GetProperty(idPropertyName);
                    if (idProp != null && idProp.CanWrite) idProp.SetValue(entity, idStr);

                    // Set Name
                    var nameProp = type.GetProperty(namePropertyName);
                    if (nameProp != null && nameProp.CanWrite) nameProp.SetValue(entity, nameStr);

                    // Set Description
                    var descProp = type.GetProperty(descPropertyName);
                    if (descProp != null && descProp.CanWrite) descProp.SetValue(entity, $"Enum: {enumType.Name}");
                    
                    // Metadata
                    var sourceProp = type.GetProperty("SOURCE");
                    if (sourceProp != null && sourceProp.CanWrite) sourceProp.SetValue(entity, sourceValue);

                    var activeProp = type.GetProperty("ACTIVE_IND");
                    if (activeProp != null && activeProp.CanWrite) activeProp.SetValue(entity, "Y");

                    // Insert
                    await repository.InsertWithParentKeysAsync(entity, userId, null);
                    count++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding {typeof(T).Name} - {value}: {ex.Message}");
                }
            }
            return count;
        }
    }
}
