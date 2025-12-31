using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_TEST_FLOW: Entity,IPPDMEntity

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
private  System.String PERIOD_TYPEValue; 
 public System.String PERIOD_TYPE
        {  
            get  
            {  
                return this.PERIOD_TYPEValue;  
            }  

          set { SetProperty(ref  PERIOD_TYPEValue, value); }
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
private  System.String FLUID_TYPEValue; 
 public System.String FLUID_TYPE
        {  
            get  
            {  
                return this.FLUID_TYPEValue;  
            }  

          set { SetProperty(ref  FLUID_TYPEValue, value); }
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
private  System.String BASIC_SEDIMENT_INDValue; 
 public System.String BASIC_SEDIMENT_IND
        {  
            get  
            {  
                return this.BASIC_SEDIMENT_INDValue;  
            }  

          set { SetProperty(ref  BASIC_SEDIMENT_INDValue, value); }
        } 
private  System.String BLOW_DESCValue; 
 public System.String BLOW_DESC
        {  
            get  
            {  
                return this.BLOW_DESCValue;  
            }  

          set { SetProperty(ref  BLOW_DESCValue, value); }
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
private  System.Decimal GAS_RISER_DIAMETERValue; 
 public System.Decimal GAS_RISER_DIAMETER
        {  
            get  
            {  
                return this.GAS_RISER_DIAMETERValue;  
            }  

          set { SetProperty(ref  GAS_RISER_DIAMETERValue, value); }
        } 
private  System.String GAS_RISER_DIAMETER_OUOMValue; 
 public System.String GAS_RISER_DIAMETER_OUOM
        {  
            get  
            {  
                return this.GAS_RISER_DIAMETER_OUOMValue;  
            }  

          set { SetProperty(ref  GAS_RISER_DIAMETER_OUOMValue, value); }
        } 
private  System.Decimal MAX_FLUID_RATEValue; 
 public System.Decimal MAX_FLUID_RATE
        {  
            get  
            {  
                return this.MAX_FLUID_RATEValue;  
            }  

          set { SetProperty(ref  MAX_FLUID_RATEValue, value); }
        } 
private  System.String MAX_FLUID_RATE_OUOMValue; 
 public System.String MAX_FLUID_RATE_OUOM
        {  
            get  
            {  
                return this.MAX_FLUID_RATE_OUOMValue;  
            }  

          set { SetProperty(ref  MAX_FLUID_RATE_OUOMValue, value); }
        } 
private  System.String MAX_FLUID_RATE_UOMValue; 
 public System.String MAX_FLUID_RATE_UOM
        {  
            get  
            {  
                return this.MAX_FLUID_RATE_UOMValue;  
            }  

          set { SetProperty(ref  MAX_FLUID_RATE_UOMValue, value); }
        } 
private  System.Decimal MAX_SURFACE_PRESSUREValue; 
 public System.Decimal MAX_SURFACE_PRESSURE
        {  
            get  
            {  
                return this.MAX_SURFACE_PRESSUREValue;  
            }  

          set { SetProperty(ref  MAX_SURFACE_PRESSUREValue, value); }
        } 
private  System.String MAX_SURFACE_PRESSURE_OUOMValue; 
 public System.String MAX_SURFACE_PRESSURE_OUOM
        {  
            get  
            {  
                return this.MAX_SURFACE_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  MAX_SURFACE_PRESSURE_OUOMValue, value); }
        } 
private  System.String MEASUREMENT_TECHNIQUEValue; 
 public System.String MEASUREMENT_TECHNIQUE
        {  
            get  
            {  
                return this.MEASUREMENT_TECHNIQUEValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_TECHNIQUEValue, value); }
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
private  System.Decimal SHAKEOUT_PERCENTValue; 
 public System.Decimal SHAKEOUT_PERCENT
        {  
            get  
            {  
                return this.SHAKEOUT_PERCENTValue;  
            }  

          set { SetProperty(ref  SHAKEOUT_PERCENTValue, value); }
        } 
private  System.Decimal TTS_TIME_ELAPSEDValue; 
 public System.Decimal TTS_TIME_ELAPSED
        {  
            get  
            {  
                return this.TTS_TIME_ELAPSEDValue;  
            }  

          set { SetProperty(ref  TTS_TIME_ELAPSEDValue, value); }
        } 
private  System.String TTS_TIME_ELAPSED_OUOMValue; 
 public System.String TTS_TIME_ELAPSED_OUOM
        {  
            get  
            {  
                return this.TTS_TIME_ELAPSED_OUOMValue;  
            }  

          set { SetProperty(ref  TTS_TIME_ELAPSED_OUOMValue, value); }
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


    public WELL_TEST_FLOW () { }

  }
}

