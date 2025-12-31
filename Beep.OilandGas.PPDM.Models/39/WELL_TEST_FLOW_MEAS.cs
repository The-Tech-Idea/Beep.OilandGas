using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_TEST_FLOW_MEAS: Entity,IPPDMEntity

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
private  System.String SOURCEValue; 
 public System.String SOURCE
        {  
            get  
            {  
                return this.SOURCEValue;  
            }  

          set { SetProperty(ref  SOURCEValue, value); }
        } 
private  System.String TEST_TYPEValue; 
 public System.String TEST_TYPE
        {  
            get  
            {  
                return this.TEST_TYPEValue;  
            }  

          set { SetProperty(ref  TEST_TYPEValue, value); }
        } 
private  System.String RUN_NUMValue; 
 public System.String RUN_NUM
        {  
            get  
            {  
                return this.RUN_NUMValue;  
            }  

          set { SetProperty(ref  RUN_NUMValue, value); }
        } 
private  System.String TEST_NUMValue; 
 public System.String TEST_NUM
        {  
            get  
            {  
                return this.TEST_NUMValue;  
            }  

          set { SetProperty(ref  TEST_NUMValue, value); }
        } 
private  System.Decimal MEASUREMENT_OBS_NOValue; 
 public System.Decimal MEASUREMENT_OBS_NO
        {  
            get  
            {  
                return this.MEASUREMENT_OBS_NOValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_OBS_NOValue, value); }
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
private  System.Decimal FLOW_DURATIONValue; 
 public System.Decimal FLOW_DURATION
        {  
            get  
            {  
                return this.FLOW_DURATIONValue;  
            }  

          set { SetProperty(ref  FLOW_DURATIONValue, value); }
        } 
private  System.String FLOW_DURATION_OUOMValue; 
 public System.String FLOW_DURATION_OUOM
        {  
            get  
            {  
                return this.FLOW_DURATION_OUOMValue;  
            }  

          set { SetProperty(ref  FLOW_DURATION_OUOMValue, value); }
        } 
private  System.String FLUID_TYPEValue; 
 public System.String FLUID_TYPE
        {  
            get  
            {  
                return this.FLUID_TYPEValue;  
            }  

          set { SetProperty(ref  FLUID_TYPEValue, value); }
        } 
private  System.Decimal MEASUREMENT_PRESSUREValue; 
 public System.Decimal MEASUREMENT_PRESSURE
        {  
            get  
            {  
                return this.MEASUREMENT_PRESSUREValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_PRESSUREValue, value); }
        } 
private  System.String MEASUREMENT_PRESSURE_OUOMValue; 
 public System.String MEASUREMENT_PRESSURE_OUOM
        {  
            get  
            {  
                return this.MEASUREMENT_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal MEASUREMENT_TIME_ELAPSEDValue; 
 public System.Decimal MEASUREMENT_TIME_ELAPSED
        {  
            get  
            {  
                return this.MEASUREMENT_TIME_ELAPSEDValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_TIME_ELAPSEDValue, value); }
        } 
private  System.String MEASUREMENT_TIME_ELAPSED_OUOMValue; 
 public System.String MEASUREMENT_TIME_ELAPSED_OUOM
        {  
            get  
            {  
                return this.MEASUREMENT_TIME_ELAPSED_OUOMValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_TIME_ELAPSED_OUOMValue, value); }
        } 
private  System.Decimal MEASUREMENT_VOLUMEValue; 
 public System.Decimal MEASUREMENT_VOLUME
        {  
            get  
            {  
                return this.MEASUREMENT_VOLUMEValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_VOLUMEValue, value); }
        } 
private  System.String MEASUREMENT_VOLUME_OUOMValue; 
 public System.String MEASUREMENT_VOLUME_OUOM
        {  
            get  
            {  
                return this.MEASUREMENT_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_VOLUME_OUOMValue, value); }
        } 
private  System.String MEASUREMENT_VOLUME_UOMValue; 
 public System.String MEASUREMENT_VOLUME_UOM
        {  
            get  
            {  
                return this.MEASUREMENT_VOLUME_UOMValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_VOLUME_UOMValue, value); }
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
private  System.String PERIOD_TYPEValue; 
 public System.String PERIOD_TYPE
        {  
            get  
            {  
                return this.PERIOD_TYPEValue;  
            }  

          set { SetProperty(ref  PERIOD_TYPEValue, value); }
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
private  System.String REMARKValue; 
 public System.String REMARK
        {  
            get  
            {  
                return this.REMARKValue;  
            }  

          set { SetProperty(ref  REMARKValue, value); }
        } 
private  System.Decimal SURFACE_CHOKE_DIAMETERValue; 
 public System.Decimal SURFACE_CHOKE_DIAMETER
        {  
            get  
            {  
                return this.SURFACE_CHOKE_DIAMETERValue;  
            }  

          set { SetProperty(ref  SURFACE_CHOKE_DIAMETERValue, value); }
        } 
private  System.String SURFACE_CHOKE_DIAMETER_OUOMValue; 
 public System.String SURFACE_CHOKE_DIAMETER_OUOM
        {  
            get  
            {  
                return this.SURFACE_CHOKE_DIAMETER_OUOMValue;  
            }  

          set { SetProperty(ref  SURFACE_CHOKE_DIAMETER_OUOMValue, value); }
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


    public WELL_TEST_FLOW_MEAS () { }

  }
}

