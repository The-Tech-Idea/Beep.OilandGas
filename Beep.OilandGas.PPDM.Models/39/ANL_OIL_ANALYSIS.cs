using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class ANL_OIL_ANALYSIS: Entity,IPPDMEntity

{

private  System.String ANALYSIS_IDValue; 
 public System.String ANALYSIS_ID
        {  
            get  
            {  
                return this.ANALYSIS_IDValue;  
            }  

          set { SetProperty(ref  ANALYSIS_IDValue, value); }
        } 
private  System.String ANL_SOURCEValue; 
 public System.String ANL_SOURCE
        {  
            get  
            {  
                return this.ANL_SOURCEValue;  
            }  

          set { SetProperty(ref  ANL_SOURCEValue, value); }
        } 
private  System.Decimal OIL_ANALYSIS_OBS_NOValue; 
 public System.Decimal OIL_ANALYSIS_OBS_NO
        {  
            get  
            {  
                return this.OIL_ANALYSIS_OBS_NOValue;  
            }  

          set { SetProperty(ref  OIL_ANALYSIS_OBS_NOValue, value); }
        } 
private  System.Decimal ABSOLUTE_GRAVITYValue; 
 public System.Decimal ABSOLUTE_GRAVITY
        {  
            get  
            {  
                return this.ABSOLUTE_GRAVITYValue;  
            }  

          set { SetProperty(ref  ABSOLUTE_GRAVITYValue, value); }
        } 
private  System.String ABSOLUTE_GRAVITY_OUOMValue; 
 public System.String ABSOLUTE_GRAVITY_OUOM
        {  
            get  
            {  
                return this.ABSOLUTE_GRAVITY_OUOMValue;  
            }  

          set { SetProperty(ref  ABSOLUTE_GRAVITY_OUOMValue, value); }
        } 
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
        } 
private  System.Decimal ASPHALTENE_CONTENTValue; 
 public System.Decimal ASPHALTENE_CONTENT
        {  
            get  
            {  
                return this.ASPHALTENE_CONTENTValue;  
            }  

          set { SetProperty(ref  ASPHALTENE_CONTENTValue, value); }
        } 
private  System.String ASPHALTENE_CONTENT_OUOMValue; 
 public System.String ASPHALTENE_CONTENT_OUOM
        {  
            get  
            {  
                return this.ASPHALTENE_CONTENT_OUOMValue;  
            }  

          set { SetProperty(ref  ASPHALTENE_CONTENT_OUOMValue, value); }
        } 
private  System.String ASPH_CONTENT_PREFERRED_INDValue; 
 public System.String ASPH_CONTENT_PREFERRED_IND
        {  
            get  
            {  
                return this.ASPH_CONTENT_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  ASPH_CONTENT_PREFERRED_INDValue, value); }
        } 
private  System.String ASPH_CONTENT_PROBLEM_INDValue; 
 public System.String ASPH_CONTENT_PROBLEM_IND
        {  
            get  
            {  
                return this.ASPH_CONTENT_PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  ASPH_CONTENT_PROBLEM_INDValue, value); }
        } 
private  System.Decimal ASPH_CONTENT_STEP_SEQ_NOValue; 
 public System.Decimal ASPH_CONTENT_STEP_SEQ_NO
        {  
            get  
            {  
                return this.ASPH_CONTENT_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  ASPH_CONTENT_STEP_SEQ_NOValue, value); }
        } 
private  System.String CHARACTERIZE_FACTORValue; 
 public System.String CHARACTERIZE_FACTOR
        {  
            get  
            {  
                return this.CHARACTERIZE_FACTORValue;  
            }  

          set { SetProperty(ref  CHARACTERIZE_FACTORValue, value); }
        } 
private  System.String CHARACTERIZE_FACTOR_OUOMValue; 
 public System.String CHARACTERIZE_FACTOR_OUOM
        {  
            get  
            {  
                return this.CHARACTERIZE_FACTOR_OUOMValue;  
            }  

          set { SetProperty(ref  CHARACTERIZE_FACTOR_OUOMValue, value); }
        } 
private  System.String CLOUD_POINT_PREFERRED_INDValue; 
 public System.String CLOUD_POINT_PREFERRED_IND
        {  
            get  
            {  
                return this.CLOUD_POINT_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  CLOUD_POINT_PREFERRED_INDValue, value); }
        } 
private  System.String CLOUD_POINT_PROBLEM_INDValue; 
 public System.String CLOUD_POINT_PROBLEM_IND
        {  
            get  
            {  
                return this.CLOUD_POINT_PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  CLOUD_POINT_PROBLEM_INDValue, value); }
        } 
private  System.Decimal CLOUD_POINT_STEP_SEQ_NOValue; 
 public System.Decimal CLOUD_POINT_STEP_SEQ_NO
        {  
            get  
            {  
                return this.CLOUD_POINT_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  CLOUD_POINT_STEP_SEQ_NOValue, value); }
        } 
private  System.Decimal CLOUD_POINT_TEMPValue; 
 public System.Decimal CLOUD_POINT_TEMP
        {  
            get  
            {  
                return this.CLOUD_POINT_TEMPValue;  
            }  

          set { SetProperty(ref  CLOUD_POINT_TEMPValue, value); }
        } 
private  System.String CLOUD_POINT_TEMP_OUOMValue; 
 public System.String CLOUD_POINT_TEMP_OUOM
        {  
            get  
            {  
                return this.CLOUD_POINT_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  CLOUD_POINT_TEMP_OUOMValue, value); }
        } 
private  System.String DEW_POINT_PREFERRED_INDValue; 
 public System.String DEW_POINT_PREFERRED_IND
        {  
            get  
            {  
                return this.DEW_POINT_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  DEW_POINT_PREFERRED_INDValue, value); }
        } 
private  System.String DEW_POINT_PROBLEM_INDValue; 
 public System.String DEW_POINT_PROBLEM_IND
        {  
            get  
            {  
                return this.DEW_POINT_PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  DEW_POINT_PROBLEM_INDValue, value); }
        } 
private  System.Decimal DEW_POINT_STEP_SEQ_NOValue; 
 public System.Decimal DEW_POINT_STEP_SEQ_NO
        {  
            get  
            {  
                return this.DEW_POINT_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  DEW_POINT_STEP_SEQ_NOValue, value); }
        } 
private  System.Decimal DEW_POINT_TEMPValue; 
 public System.Decimal DEW_POINT_TEMP
        {  
            get  
            {  
                return this.DEW_POINT_TEMPValue;  
            }  

          set { SetProperty(ref  DEW_POINT_TEMPValue, value); }
        } 
private  System.String DEW_POINT_TEMP_OUOMValue; 
 public System.String DEW_POINT_TEMP_OUOM
        {  
            get  
            {  
                return this.DEW_POINT_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  DEW_POINT_TEMP_OUOMValue, value); }
        } 
private  System.String DISTILLATION_BASE_TYPEValue; 
 public System.String DISTILLATION_BASE_TYPE
        {  
            get  
            {  
                return this.DISTILLATION_BASE_TYPEValue;  
            }  

          set { SetProperty(ref  DISTILLATION_BASE_TYPEValue, value); }
        } 
private  System.String DISTILLATION_PREFERRED_INDValue; 
 public System.String DISTILLATION_PREFERRED_IND
        {  
            get  
            {  
                return this.DISTILLATION_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  DISTILLATION_PREFERRED_INDValue, value); }
        } 
private  System.String DISTILLATION_PROBLEM_INDValue; 
 public System.String DISTILLATION_PROBLEM_IND
        {  
            get  
            {  
                return this.DISTILLATION_PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  DISTILLATION_PROBLEM_INDValue, value); }
        } 
private  System.Decimal DISTILLATION_STEP_SEQ_NOValue; 
 public System.Decimal DISTILLATION_STEP_SEQ_NO
        {  
            get  
            {  
                return this.DISTILLATION_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  DISTILLATION_STEP_SEQ_NOValue, value); }
        } 
private  System.DateTime? EFFECTIVE_DATEValue; 
 public System.DateTime? EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime? EXPIRY_DATEValue; 
 public System.DateTime? EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.String FLASH_POINT_PREFERRED_INDValue; 
 public System.String FLASH_POINT_PREFERRED_IND
        {  
            get  
            {  
                return this.FLASH_POINT_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  FLASH_POINT_PREFERRED_INDValue, value); }
        } 
private  System.String FLASH_POINT_PROBLEM_INDValue; 
 public System.String FLASH_POINT_PROBLEM_IND
        {  
            get  
            {  
                return this.FLASH_POINT_PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  FLASH_POINT_PROBLEM_INDValue, value); }
        } 
private  System.Decimal FLASH_POINT_STEP_SEQ_NOValue; 
 public System.Decimal FLASH_POINT_STEP_SEQ_NO
        {  
            get  
            {  
                return this.FLASH_POINT_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  FLASH_POINT_STEP_SEQ_NOValue, value); }
        } 
private  System.Decimal FLASH_POINT_TEMPValue; 
 public System.Decimal FLASH_POINT_TEMP
        {  
            get  
            {  
                return this.FLASH_POINT_TEMPValue;  
            }  

          set { SetProperty(ref  FLASH_POINT_TEMPValue, value); }
        } 
private  System.String FLASH_POINT_TEMP_OUOMValue; 
 public System.String FLASH_POINT_TEMP_OUOM
        {  
            get  
            {  
                return this.FLASH_POINT_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  FLASH_POINT_TEMP_OUOMValue, value); }
        } 
private  System.Decimal GORValue; 
 public System.Decimal GOR
        {  
            get  
            {  
                return this.GORValue;  
            }  

          set { SetProperty(ref  GORValue, value); }
        } 
private  System.String GOR_OUOMValue; 
 public System.String GOR_OUOM
        {  
            get  
            {  
                return this.GOR_OUOMValue;  
            }  

          set { SetProperty(ref  GOR_OUOMValue, value); }
        } 
private  System.Decimal MEASURED_PRESSUREValue; 
 public System.Decimal MEASURED_PRESSURE
        {  
            get  
            {  
                return this.MEASURED_PRESSUREValue;  
            }  

          set { SetProperty(ref  MEASURED_PRESSUREValue, value); }
        } 
private  System.String MEASURED_PRESSURE_OUOMValue; 
 public System.String MEASURED_PRESSURE_OUOM
        {  
            get  
            {  
                return this.MEASURED_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  MEASURED_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal MEASURED_VOLUMEValue; 
 public System.Decimal MEASURED_VOLUME
        {  
            get  
            {  
                return this.MEASURED_VOLUMEValue;  
            }  

          set { SetProperty(ref  MEASURED_VOLUMEValue, value); }
        } 
private  System.String MEASURED_VOLUME_OUOMValue; 
 public System.String MEASURED_VOLUME_OUOM
        {  
            get  
            {  
                return this.MEASURED_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  MEASURED_VOLUME_OUOMValue, value); }
        } 
private  System.String METHOD_TYPEValue; 
 public System.String METHOD_TYPE
        {  
            get  
            {  
                return this.METHOD_TYPEValue;  
            }  

          set { SetProperty(ref  METHOD_TYPEValue, value); }
        } 
private  System.Decimal OIL_ANALYSIS_TEMPValue; 
 public System.Decimal OIL_ANALYSIS_TEMP
        {  
            get  
            {  
                return this.OIL_ANALYSIS_TEMPValue;  
            }  

          set { SetProperty(ref  OIL_ANALYSIS_TEMPValue, value); }
        } 
private  System.String OIL_ANALYSIS_TEMP_OUOMValue; 
 public System.String OIL_ANALYSIS_TEMP_OUOM
        {  
            get  
            {  
                return this.OIL_ANALYSIS_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  OIL_ANALYSIS_TEMP_OUOMValue, value); }
        } 
private  System.String OIL_COLORValue; 
 public System.String OIL_COLOR
        {  
            get  
            {  
                return this.OIL_COLORValue;  
            }  

          set { SetProperty(ref  OIL_COLORValue, value); }
        } 
private  System.String OIL_COLOR_PREFERRED_INDValue; 
 public System.String OIL_COLOR_PREFERRED_IND
        {  
            get  
            {  
                return this.OIL_COLOR_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  OIL_COLOR_PREFERRED_INDValue, value); }
        } 
private  System.String OIL_COLOR_PROBLEM_INDValue; 
 public System.String OIL_COLOR_PROBLEM_IND
        {  
            get  
            {  
                return this.OIL_COLOR_PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  OIL_COLOR_PROBLEM_INDValue, value); }
        } 
private  System.Decimal OIL_COLOR_STEP_SEQ_NOValue; 
 public System.Decimal OIL_COLOR_STEP_SEQ_NO
        {  
            get  
            {  
                return this.OIL_COLOR_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  OIL_COLOR_STEP_SEQ_NOValue, value); }
        } 
private  System.Decimal OIL_DENSITYValue; 
 public System.Decimal OIL_DENSITY
        {  
            get  
            {  
                return this.OIL_DENSITYValue;  
            }  

          set { SetProperty(ref  OIL_DENSITYValue, value); }
        } 
private  System.String OIL_DENSITY_OUOMValue; 
 public System.String OIL_DENSITY_OUOM
        {  
            get  
            {  
                return this.OIL_DENSITY_OUOMValue;  
            }  

          set { SetProperty(ref  OIL_DENSITY_OUOMValue, value); }
        } 
private  System.String OIL_DENSITY_PREFERRED_INDValue; 
 public System.String OIL_DENSITY_PREFERRED_IND
        {  
            get  
            {  
                return this.OIL_DENSITY_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  OIL_DENSITY_PREFERRED_INDValue, value); }
        } 
private  System.String OIL_DENSITY_PROBLEM_INDValue; 
 public System.String OIL_DENSITY_PROBLEM_IND
        {  
            get  
            {  
                return this.OIL_DENSITY_PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  OIL_DENSITY_PROBLEM_INDValue, value); }
        } 
private  System.Decimal OIL_DENSITY_STEP_SEQ_NOValue; 
 public System.Decimal OIL_DENSITY_STEP_SEQ_NO
        {  
            get  
            {  
                return this.OIL_DENSITY_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  OIL_DENSITY_STEP_SEQ_NOValue, value); }
        } 
private  System.Decimal OIL_GRAVITYValue; 
 public System.Decimal OIL_GRAVITY
        {  
            get  
            {  
                return this.OIL_GRAVITYValue;  
            }  

          set { SetProperty(ref  OIL_GRAVITYValue, value); }
        } 
private  System.String OIL_GRAVITY_OUOMValue; 
 public System.String OIL_GRAVITY_OUOM
        {  
            get  
            {  
                return this.OIL_GRAVITY_OUOMValue;  
            }  

          set { SetProperty(ref  OIL_GRAVITY_OUOMValue, value); }
        } 
private  System.String OIL_GRAVITY_PREFERRED_INDValue; 
 public System.String OIL_GRAVITY_PREFERRED_IND
        {  
            get  
            {  
                return this.OIL_GRAVITY_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  OIL_GRAVITY_PREFERRED_INDValue, value); }
        } 
private  System.Decimal OIL_GRAVITY_STEP_SEQ_NOValue; 
 public System.Decimal OIL_GRAVITY_STEP_SEQ_NO
        {  
            get  
            {  
                return this.OIL_GRAVITY_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  OIL_GRAVITY_STEP_SEQ_NOValue, value); }
        } 
private  System.Decimal OIL_GRAVITY_TEMPValue; 
 public System.Decimal OIL_GRAVITY_TEMP
        {  
            get  
            {  
                return this.OIL_GRAVITY_TEMPValue;  
            }  

          set { SetProperty(ref  OIL_GRAVITY_TEMPValue, value); }
        } 
private  System.String OIL_GRAVITY_TEMP_OUOMValue; 
 public System.String OIL_GRAVITY_TEMP_OUOM
        {  
            get  
            {  
                return this.OIL_GRAVITY_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  OIL_GRAVITY_TEMP_OUOMValue, value); }
        } 
private  System.String OIL_GRAVITY_TEMP_PREFER_INDValue; 
 public System.String OIL_GRAVITY_TEMP_PREFER_IND
        {  
            get  
            {  
                return this.OIL_GRAVITY_TEMP_PREFER_INDValue;  
            }  

          set { SetProperty(ref  OIL_GRAVITY_TEMP_PREFER_INDValue, value); }
        } 
private  System.String OIL_GRAVITY_TEMP_PROB_INDValue; 
 public System.String OIL_GRAVITY_TEMP_PROB_IND
        {  
            get  
            {  
                return this.OIL_GRAVITY_TEMP_PROB_INDValue;  
            }  

          set { SetProperty(ref  OIL_GRAVITY_TEMP_PROB_INDValue, value); }
        } 
private  System.Decimal OIL_GRAVITY_TEMP_STEP_SEQValue; 
 public System.Decimal OIL_GRAVITY_TEMP_STEP_SEQ
        {  
            get  
            {  
                return this.OIL_GRAVITY_TEMP_STEP_SEQValue;  
            }  

          set { SetProperty(ref  OIL_GRAVITY_TEMP_STEP_SEQValue, value); }
        } 
private  System.String OIL_TYPEValue; 
 public System.String OIL_TYPE
        {  
            get  
            {  
                return this.OIL_TYPEValue;  
            }  

          set { SetProperty(ref  OIL_TYPEValue, value); }
        } 
private  System.String PPDM_GUIDValue; 
 public System.String PPDM_GUID
        {  
            get  
            {  
                return this.PPDM_GUIDValue;  
            }  

          set { SetProperty(ref  PPDM_GUIDValue, value); }
        } 
private  System.Decimal RELATIVE_DENSITYValue; 
 public System.Decimal RELATIVE_DENSITY
        {  
            get  
            {  
                return this.RELATIVE_DENSITYValue;  
            }  

          set { SetProperty(ref  RELATIVE_DENSITYValue, value); }
        } 
private  System.String RELATIVE_DENSITY_OUOMValue; 
 public System.String RELATIVE_DENSITY_OUOM
        {  
            get  
            {  
                return this.RELATIVE_DENSITY_OUOMValue;  
            }  

          set { SetProperty(ref  RELATIVE_DENSITY_OUOMValue, value); }
        } 
private  System.Decimal RELATIVE_STD_DENSITYValue; 
 public System.Decimal RELATIVE_STD_DENSITY
        {  
            get  
            {  
                return this.RELATIVE_STD_DENSITYValue;  
            }  

          set { SetProperty(ref  RELATIVE_STD_DENSITYValue, value); }
        } 
private  System.String RELATIVE_STD_DENSITY_OUOMValue; 
 public System.String RELATIVE_STD_DENSITY_OUOM
        {  
            get  
            {  
                return this.RELATIVE_STD_DENSITY_OUOMValue;  
            }  

          set { SetProperty(ref  RELATIVE_STD_DENSITY_OUOMValue, value); }
        } 
private  System.String REMARKValue; 
 public System.String REMARK
        {  
            get  
            {  
                return this.REMARKValue;  
            }  

          set { SetProperty(ref  REMARKValue, value); }
        } 
private  System.Decimal RESERVOIR_PRESSValue; 
 public System.Decimal RESERVOIR_PRESS
        {  
            get  
            {  
                return this.RESERVOIR_PRESSValue;  
            }  

          set { SetProperty(ref  RESERVOIR_PRESSValue, value); }
        } 
private  System.String RESERVOIR_PRESS_OUOMValue; 
 public System.String RESERVOIR_PRESS_OUOM
        {  
            get  
            {  
                return this.RESERVOIR_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  RESERVOIR_PRESS_OUOMValue, value); }
        } 
private  System.Decimal RESERVOIR_TEMPValue; 
 public System.Decimal RESERVOIR_TEMP
        {  
            get  
            {  
                return this.RESERVOIR_TEMPValue;  
            }  

          set { SetProperty(ref  RESERVOIR_TEMPValue, value); }
        } 
private  System.String RESERVOIR_TEMP_OUOMValue; 
 public System.String RESERVOIR_TEMP_OUOM
        {  
            get  
            {  
                return this.RESERVOIR_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  RESERVOIR_TEMP_OUOMValue, value); }
        } 
private  System.Decimal SEDIMENT_CONTENTValue; 
 public System.Decimal SEDIMENT_CONTENT
        {  
            get  
            {  
                return this.SEDIMENT_CONTENTValue;  
            }  

          set { SetProperty(ref  SEDIMENT_CONTENTValue, value); }
        } 
private  System.String SEDIMENT_CONTENT_OUOMValue; 
 public System.String SEDIMENT_CONTENT_OUOM
        {  
            get  
            {  
                return this.SEDIMENT_CONTENT_OUOMValue;  
            }  

          set { SetProperty(ref  SEDIMENT_CONTENT_OUOMValue, value); }
        } 
private  System.String SEDIMENT_PREFERRED_INDValue; 
 public System.String SEDIMENT_PREFERRED_IND
        {  
            get  
            {  
                return this.SEDIMENT_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  SEDIMENT_PREFERRED_INDValue, value); }
        } 
private  System.String SEDIMENT_PROBLEM_INDValue; 
 public System.String SEDIMENT_PROBLEM_IND
        {  
            get  
            {  
                return this.SEDIMENT_PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  SEDIMENT_PROBLEM_INDValue, value); }
        } 
private  System.Decimal SEDIMENT_STEP_SEQ_NOValue; 
 public System.Decimal SEDIMENT_STEP_SEQ_NO
        {  
            get  
            {  
                return this.SEDIMENT_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  SEDIMENT_STEP_SEQ_NOValue, value); }
        } 
private  System.Decimal SHRINKAGE_FACTORValue; 
 public System.Decimal SHRINKAGE_FACTOR
        {  
            get  
            {  
                return this.SHRINKAGE_FACTORValue;  
            }  

          set { SetProperty(ref  SHRINKAGE_FACTORValue, value); }
        } 
private  System.String SHRINK_FACTOR_PREFERRED_INDValue; 
 public System.String SHRINK_FACTOR_PREFERRED_IND
        {  
            get  
            {  
                return this.SHRINK_FACTOR_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  SHRINK_FACTOR_PREFERRED_INDValue, value); }
        } 
private  System.String SHRINK_FACTOR_PROBLEM_INDValue; 
 public System.String SHRINK_FACTOR_PROBLEM_IND
        {  
            get  
            {  
                return this.SHRINK_FACTOR_PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  SHRINK_FACTOR_PROBLEM_INDValue, value); }
        } 
private  System.Decimal SHRINK_FACTOR_STEP_SEQ_NOValue; 
 public System.Decimal SHRINK_FACTOR_STEP_SEQ_NO
        {  
            get  
            {  
                return this.SHRINK_FACTOR_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  SHRINK_FACTOR_STEP_SEQ_NOValue, value); }
        } 
private  System.String SOURCEValue; 
 public System.String SOURCE
        {  
            get  
            {  
                return this.SOURCEValue;  
            }  

          set { SetProperty(ref  SOURCEValue, value); }
        } 
private  System.Decimal SULPHUR_CONTENTValue; 
 public System.Decimal SULPHUR_CONTENT
        {  
            get  
            {  
                return this.SULPHUR_CONTENTValue;  
            }  

          set { SetProperty(ref  SULPHUR_CONTENTValue, value); }
        } 
private  System.Decimal VAPOUR_PRESSValue; 
 public System.Decimal VAPOUR_PRESS
        {  
            get  
            {  
                return this.VAPOUR_PRESSValue;  
            }  

          set { SetProperty(ref  VAPOUR_PRESSValue, value); }
        } 
private  System.String VAPOUR_PRESS_OUOMValue; 
 public System.String VAPOUR_PRESS_OUOM
        {  
            get  
            {  
                return this.VAPOUR_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  VAPOUR_PRESS_OUOMValue, value); }
        } 
private  System.Decimal VAPOUR_PRESS_TEMPValue; 
 public System.Decimal VAPOUR_PRESS_TEMP
        {  
            get  
            {  
                return this.VAPOUR_PRESS_TEMPValue;  
            }  

          set { SetProperty(ref  VAPOUR_PRESS_TEMPValue, value); }
        } 
private  System.String VAPOUR_PRESS_TEMP_OUOMValue; 
 public System.String VAPOUR_PRESS_TEMP_OUOM
        {  
            get  
            {  
                return this.VAPOUR_PRESS_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  VAPOUR_PRESS_TEMP_OUOMValue, value); }
        } 
private  System.Decimal WATER_CONTENTValue; 
 public System.Decimal WATER_CONTENT
        {  
            get  
            {  
                return this.WATER_CONTENTValue;  
            }  

          set { SetProperty(ref  WATER_CONTENTValue, value); }
        } 
private  System.String WATER_CONTENT_OUOMValue; 
 public System.String WATER_CONTENT_OUOM
        {  
            get  
            {  
                return this.WATER_CONTENT_OUOMValue;  
            }  

          set { SetProperty(ref  WATER_CONTENT_OUOMValue, value); }
        } 
private  System.Decimal WAX_CONTENTValue; 
 public System.Decimal WAX_CONTENT
        {  
            get  
            {  
                return this.WAX_CONTENTValue;  
            }  

          set { SetProperty(ref  WAX_CONTENTValue, value); }
        } 
private  System.String WAX_CONTENT_OUOMValue; 
 public System.String WAX_CONTENT_OUOM
        {  
            get  
            {  
                return this.WAX_CONTENT_OUOMValue;  
            }  

          set { SetProperty(ref  WAX_CONTENT_OUOMValue, value); }
        } 
private  System.String WAX_CONTENT_PREFERRED_INDValue; 
 public System.String WAX_CONTENT_PREFERRED_IND
        {  
            get  
            {  
                return this.WAX_CONTENT_PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  WAX_CONTENT_PREFERRED_INDValue, value); }
        } 
private  System.String WAX_CONTENT_PROBLEM_INDValue; 
 public System.String WAX_CONTENT_PROBLEM_IND
        {  
            get  
            {  
                return this.WAX_CONTENT_PROBLEM_INDValue;  
            }  

          set { SetProperty(ref  WAX_CONTENT_PROBLEM_INDValue, value); }
        } 
private  System.Decimal WAX_CONTENT_STEP_SEQ_NOValue; 
 public System.Decimal WAX_CONTENT_STEP_SEQ_NO
        {  
            get  
            {  
                return this.WAX_CONTENT_STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  WAX_CONTENT_STEP_SEQ_NOValue, value); }
        } 
private  System.String ROW_CHANGED_BYValue; 
 public System.String ROW_CHANGED_BY
        {  
            get  
            {  
                return this.ROW_CHANGED_BYValue;  
            }  

          set { SetProperty(ref  ROW_CHANGED_BYValue, value); }
        } 
private  System.DateTime? ROW_CHANGED_DATEValue; 
 public System.DateTime? ROW_CHANGED_DATE
        {  
            get  
            {  
                return this.ROW_CHANGED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CHANGED_DATEValue, value); }
        } 
private  System.String ROW_CREATED_BYValue; 
 public System.String ROW_CREATED_BY
        {  
            get  
            {  
                return this.ROW_CREATED_BYValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_BYValue, value); }
        } 
private  System.DateTime? ROW_CREATED_DATEValue; 
 public System.DateTime? ROW_CREATED_DATE
        {  
            get  
            {  
                return this.ROW_CREATED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_DATEValue, value); }
        } 
private  System.DateTime? ROW_EFFECTIVE_DATEValue; 
 public System.DateTime? ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime? ROW_EXPIRY_DATEValue; 
 public System.DateTime? ROW_EXPIRY_DATE
        {  
            get  
            {  
                return this.ROW_EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EXPIRY_DATEValue, value); }
        } 
private  System.String ROW_QUALITYValue; 
 public System.String ROW_QUALITY
        {  
            get  
            {  
                return this.ROW_QUALITYValue;  
            }  

          set { SetProperty(ref  ROW_QUALITYValue, value); }
        } 


    public ANL_OIL_ANALYSIS () { }

  }
}

