using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_DRILL_PERIOD_EQUIP: Entity,IPPDMEntity

{

private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
        } 
private  System.Decimal PERIOD_OBS_NOValue; 
 public System.Decimal PERIOD_OBS_NO
        {  
            get  
            {  
                return this.PERIOD_OBS_NOValue;  
            }  

          set { SetProperty(ref  PERIOD_OBS_NOValue, value); }
        } 
private  System.String EQUIPMENT_IDValue; 
 public System.String EQUIPMENT_ID
        {  
            get  
            {  
                return this.EQUIPMENT_IDValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_IDValue, value); }
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
private  System.Decimal AVG_BOILER_PHValue; 
 public System.Decimal AVG_BOILER_PH
        {  
            get  
            {  
                return this.AVG_BOILER_PHValue;  
            }  

          set { SetProperty(ref  AVG_BOILER_PHValue, value); }
        } 
private  System.Decimal AVG_BOILER_STACK_TEMPValue; 
 public System.Decimal AVG_BOILER_STACK_TEMP
        {  
            get  
            {  
                return this.AVG_BOILER_STACK_TEMPValue;  
            }  

          set { SetProperty(ref  AVG_BOILER_STACK_TEMPValue, value); }
        } 
private  System.String AVG_BOILER_STACK_TEMP_OUOMValue; 
 public System.String AVG_BOILER_STACK_TEMP_OUOM
        {  
            get  
            {  
                return this.AVG_BOILER_STACK_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  AVG_BOILER_STACK_TEMP_OUOMValue, value); }
        } 
private  System.Decimal AVG_PUMP_PRESSUREValue; 
 public System.Decimal AVG_PUMP_PRESSURE
        {  
            get  
            {  
                return this.AVG_PUMP_PRESSUREValue;  
            }  

          set { SetProperty(ref  AVG_PUMP_PRESSUREValue, value); }
        } 
private  System.String AVG_PUMP_PRESSURE_OUOMValue; 
 public System.String AVG_PUMP_PRESSURE_OUOM
        {  
            get  
            {  
                return this.AVG_PUMP_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  AVG_PUMP_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal BOOKED_TIME_ELAPSEDValue; 
 public System.Decimal BOOKED_TIME_ELAPSED
        {  
            get  
            {  
                return this.BOOKED_TIME_ELAPSEDValue;  
            }  

          set { SetProperty(ref  BOOKED_TIME_ELAPSEDValue, value); }
        } 
private  System.String BOOKED_TIME_ELAPSED_OUOMValue; 
 public System.String BOOKED_TIME_ELAPSED_OUOM
        {  
            get  
            {  
                return this.BOOKED_TIME_ELAPSED_OUOMValue;  
            }  

          set { SetProperty(ref  BOOKED_TIME_ELAPSED_OUOMValue, value); }
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
private  System.Decimal EQUIPMENT_OBS_NOValue; 
 public System.Decimal EQUIPMENT_OBS_NO
        {  
            get  
            {  
                return this.EQUIPMENT_OBS_NOValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_OBS_NOValue, value); }
        } 
private  System.String EQUIP_REF_NUMValue; 
 public System.String EQUIP_REF_NUM
        {  
            get  
            {  
                return this.EQUIP_REF_NUMValue;  
            }  

          set { SetProperty(ref  EQUIP_REF_NUMValue, value); }
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
private  System.Decimal INTAKE_DENSITYValue; 
 public System.Decimal INTAKE_DENSITY
        {  
            get  
            {  
                return this.INTAKE_DENSITYValue;  
            }  

          set { SetProperty(ref  INTAKE_DENSITYValue, value); }
        } 
private  System.String INTAKE_DENSITY_OUOMValue; 
 public System.String INTAKE_DENSITY_OUOM
        {  
            get  
            {  
                return this.INTAKE_DENSITY_OUOMValue;  
            }  

          set { SetProperty(ref  INTAKE_DENSITY_OUOMValue, value); }
        } 
private  System.Decimal OVERFLOW_DENSITYValue; 
 public System.Decimal OVERFLOW_DENSITY
        {  
            get  
            {  
                return this.OVERFLOW_DENSITYValue;  
            }  

          set { SetProperty(ref  OVERFLOW_DENSITYValue, value); }
        } 
private  System.String OVERFLOW_DENSITY_OUOMValue; 
 public System.String OVERFLOW_DENSITY_OUOM
        {  
            get  
            {  
                return this.OVERFLOW_DENSITY_OUOMValue;  
            }  

          set { SetProperty(ref  OVERFLOW_DENSITY_OUOMValue, value); }
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
private  System.Decimal PUMP_LINER_INSIDE_DIAMValue; 
 public System.Decimal PUMP_LINER_INSIDE_DIAM
        {  
            get  
            {  
                return this.PUMP_LINER_INSIDE_DIAMValue;  
            }  

          set { SetProperty(ref  PUMP_LINER_INSIDE_DIAMValue, value); }
        } 
private  System.String PUMP_LINER_INSIDE_DIAM_OUOMValue; 
 public System.String PUMP_LINER_INSIDE_DIAM_OUOM
        {  
            get  
            {  
                return this.PUMP_LINER_INSIDE_DIAM_OUOMValue;  
            }  

          set { SetProperty(ref  PUMP_LINER_INSIDE_DIAM_OUOMValue, value); }
        } 
private  System.Decimal PUMP_STROKEValue; 
 public System.Decimal PUMP_STROKE
        {  
            get  
            {  
                return this.PUMP_STROKEValue;  
            }  

          set { SetProperty(ref  PUMP_STROKEValue, value); }
        } 
private  System.String PUMP_STROKE_OUOMValue; 
 public System.String PUMP_STROKE_OUOM
        {  
            get  
            {  
                return this.PUMP_STROKE_OUOMValue;  
            }  

          set { SetProperty(ref  PUMP_STROKE_OUOMValue, value); }
        } 
private  System.Decimal REDUCED_PUMP_DEPTHValue; 
 public System.Decimal REDUCED_PUMP_DEPTH
        {  
            get  
            {  
                return this.REDUCED_PUMP_DEPTHValue;  
            }  

          set { SetProperty(ref  REDUCED_PUMP_DEPTHValue, value); }
        } 
private  System.String REDUCED_PUMP_DEPTH_OUOMValue; 
 public System.String REDUCED_PUMP_DEPTH_OUOM
        {  
            get  
            {  
                return this.REDUCED_PUMP_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  REDUCED_PUMP_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal REDUCED_PUMP_PRESSValue; 
 public System.Decimal REDUCED_PUMP_PRESS
        {  
            get  
            {  
                return this.REDUCED_PUMP_PRESSValue;  
            }  

          set { SetProperty(ref  REDUCED_PUMP_PRESSValue, value); }
        } 
private  System.String REDUCED_PUMP_PRESS_OUOMValue; 
 public System.String REDUCED_PUMP_PRESS_OUOM
        {  
            get  
            {  
                return this.REDUCED_PUMP_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  REDUCED_PUMP_PRESS_OUOMValue, value); }
        } 
private  System.Decimal REDUCED_PUMP_STROKEValue; 
 public System.Decimal REDUCED_PUMP_STROKE
        {  
            get  
            {  
                return this.REDUCED_PUMP_STROKEValue;  
            }  

          set { SetProperty(ref  REDUCED_PUMP_STROKEValue, value); }
        } 
private  System.String REDUCED_PUMP_STROKE_OUOMValue; 
 public System.String REDUCED_PUMP_STROKE_OUOM
        {  
            get  
            {  
                return this.REDUCED_PUMP_STROKE_OUOMValue;  
            }  

          set { SetProperty(ref  REDUCED_PUMP_STROKE_OUOMValue, value); }
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
private  System.Decimal TUBING_OBS_NOValue; 
 public System.Decimal TUBING_OBS_NO
        {  
            get  
            {  
                return this.TUBING_OBS_NOValue;  
            }  

          set { SetProperty(ref  TUBING_OBS_NOValue, value); }
        } 
private  System.String TUBING_SOURCEValue; 
 public System.String TUBING_SOURCE
        {  
            get  
            {  
                return this.TUBING_SOURCEValue;  
            }  

          set { SetProperty(ref  TUBING_SOURCEValue, value); }
        } 
private  System.String TUBING_TYPEValue; 
 public System.String TUBING_TYPE
        {  
            get  
            {  
                return this.TUBING_TYPEValue;  
            }  

          set { SetProperty(ref  TUBING_TYPEValue, value); }
        } 
private  System.Decimal UNDERFLOW_DENSITYValue; 
 public System.Decimal UNDERFLOW_DENSITY
        {  
            get  
            {  
                return this.UNDERFLOW_DENSITYValue;  
            }  

          set { SetProperty(ref  UNDERFLOW_DENSITYValue, value); }
        } 
private  System.String UNDERFLOW_DENSITY_OUOMValue; 
 public System.String UNDERFLOW_DENSITY_OUOM
        {  
            get  
            {  
                return this.UNDERFLOW_DENSITY_OUOMValue;  
            }  

          set { SetProperty(ref  UNDERFLOW_DENSITY_OUOMValue, value); }
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


    public WELL_DRILL_PERIOD_EQUIP () { }

  }
}

