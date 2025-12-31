using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class ANL_OIL_VISCOSITY: Entity,IPPDMEntity

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
private  System.Decimal VISCOSITY_OBS_NOValue; 
 public System.Decimal VISCOSITY_OBS_NO
        {  
            get  
            {  
                return this.VISCOSITY_OBS_NOValue;  
            }  

          set { SetProperty(ref  VISCOSITY_OBS_NOValue, value); }
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
private  System.Decimal OIL_TEMPERATUREValue; 
 public System.Decimal OIL_TEMPERATURE
        {  
            get  
            {  
                return this.OIL_TEMPERATUREValue;  
            }  

          set { SetProperty(ref  OIL_TEMPERATUREValue, value); }
        } 
private  System.String OIL_TEMPERATURE_OUOMValue; 
 public System.String OIL_TEMPERATURE_OUOM
        {  
            get  
            {  
                return this.OIL_TEMPERATURE_OUOMValue;  
            }  

          set { SetProperty(ref  OIL_TEMPERATURE_OUOMValue, value); }
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
private  System.Decimal OIL_VISCOSITYValue; 
 public System.Decimal OIL_VISCOSITY
        {  
            get  
            {  
                return this.OIL_VISCOSITYValue;  
            }  

          set { SetProperty(ref  OIL_VISCOSITYValue, value); }
        } 
private  System.String OIL_VISCOSITY_OUOMValue; 
 public System.String OIL_VISCOSITY_OUOM
        {  
            get  
            {  
                return this.OIL_VISCOSITY_OUOMValue;  
            }  

          set { SetProperty(ref  OIL_VISCOSITY_OUOMValue, value); }
        } 
private  System.String OIL_VISCOSITY_UOMValue; 
 public System.String OIL_VISCOSITY_UOM
        {  
            get  
            {  
                return this.OIL_VISCOSITY_UOMValue;  
            }  

          set { SetProperty(ref  OIL_VISCOSITY_UOMValue, value); }
        } 
private  System.Decimal POUR_POINT_TEMPERATUREValue; 
 public System.Decimal POUR_POINT_TEMPERATURE
        {  
            get  
            {  
                return this.POUR_POINT_TEMPERATUREValue;  
            }  

          set { SetProperty(ref  POUR_POINT_TEMPERATUREValue, value); }
        } 
private  System.String POUR_POINT_TEMPERATURE_OUOMValue; 
 public System.String POUR_POINT_TEMPERATURE_OUOM
        {  
            get  
            {  
                return this.POUR_POINT_TEMPERATURE_OUOMValue;  
            }  

          set { SetProperty(ref  POUR_POINT_TEMPERATURE_OUOMValue, value); }
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
private  System.String PREFERRED_INDValue; 
 public System.String PREFERRED_IND
        {  
            get  
            {  
                return this.PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  PREFERRED_INDValue, value); }
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
private  System.String SOURCEValue; 
 public System.String SOURCE
        {  
            get  
            {  
                return this.SOURCEValue;  
            }  

          set { SetProperty(ref  SOURCEValue, value); }
        } 
private  System.Decimal STEP_SEQ_NOValue; 
 public System.Decimal STEP_SEQ_NO
        {  
            get  
            {  
                return this.STEP_SEQ_NOValue;  
            }  

          set { SetProperty(ref  STEP_SEQ_NOValue, value); }
        } 
private  System.Decimal VISCOSITY_PRESSValue; 
 public System.Decimal VISCOSITY_PRESS
        {  
            get  
            {  
                return this.VISCOSITY_PRESSValue;  
            }  

          set { SetProperty(ref  VISCOSITY_PRESSValue, value); }
        } 
private  System.String VISCOSITY_PRESS_OUOMValue; 
 public System.String VISCOSITY_PRESS_OUOM
        {  
            get  
            {  
                return this.VISCOSITY_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  VISCOSITY_PRESS_OUOMValue, value); }
        } 
private  System.Decimal VISCOSITY_TEMPValue; 
 public System.Decimal VISCOSITY_TEMP
        {  
            get  
            {  
                return this.VISCOSITY_TEMPValue;  
            }  

          set { SetProperty(ref  VISCOSITY_TEMPValue, value); }
        } 
private  System.String VISCOSITY_TEMP_OUOMValue; 
 public System.String VISCOSITY_TEMP_OUOM
        {  
            get  
            {  
                return this.VISCOSITY_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  VISCOSITY_TEMP_OUOMValue, value); }
        } 
private  System.Decimal VISCOSITY_VALUEValue; 
 public System.Decimal VISCOSITY_VALUE
        {  
            get  
            {  
                return this.VISCOSITY_VALUEValue;  
            }  

          set { SetProperty(ref  VISCOSITY_VALUEValue, value); }
        } 
private  System.String VISCOSITY_VALUE_OUOMValue; 
 public System.String VISCOSITY_VALUE_OUOM
        {  
            get  
            {  
                return this.VISCOSITY_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  VISCOSITY_VALUE_OUOMValue, value); }
        } 
private  System.String VISCOSITY_VALUE_UOMValue; 
 public System.String VISCOSITY_VALUE_UOM
        {  
            get  
            {  
                return this.VISCOSITY_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  VISCOSITY_VALUE_UOMValue, value); }
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


    public ANL_OIL_VISCOSITY () { }

  }
}

