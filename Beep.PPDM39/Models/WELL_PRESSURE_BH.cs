using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class WELL_PRESSURE_BH: Entity

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
private  System.Decimal PRESSURE_OBS_NOValue; 
 public System.Decimal PRESSURE_OBS_NO
        {  
            get  
            {  
                return this.PRESSURE_OBS_NOValue;  
            }  

          set { SetProperty(ref  PRESSURE_OBS_NOValue, value); }
        } 
private  System.Decimal BHP_OBS_NOValue; 
 public System.Decimal BHP_OBS_NO
        {  
            get  
            {  
                return this.BHP_OBS_NOValue;  
            }  

          set { SetProperty(ref  BHP_OBS_NOValue, value); }
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
private  System.Decimal BHFPValue; 
 public System.Decimal BHFP
        {  
            get  
            {  
                return this.BHFPValue;  
            }  

          set { SetProperty(ref  BHFPValue, value); }
        } 
private  System.String BHFP_OUOMValue; 
 public System.String BHFP_OUOM
        {  
            get  
            {  
                return this.BHFP_OUOMValue;  
            }  

          set { SetProperty(ref  BHFP_OUOMValue, value); }
        } 
private  System.String BHP_METHODValue; 
 public System.String BHP_METHOD
        {  
            get  
            {  
                return this.BHP_METHODValue;  
            }  

          set { SetProperty(ref  BHP_METHODValue, value); }
        } 
private  System.String BH_TEST_CODEValue; 
 public System.String BH_TEST_CODE
        {  
            get  
            {  
                return this.BH_TEST_CODEValue;  
            }  

          set { SetProperty(ref  BH_TEST_CODEValue, value); }
        } 
private  System.Decimal DATUM_PRESSUREValue; 
 public System.Decimal DATUM_PRESSURE
        {  
            get  
            {  
                return this.DATUM_PRESSUREValue;  
            }  

          set { SetProperty(ref  DATUM_PRESSUREValue, value); }
        } 
private  System.String DATUM_PRESSURE_OUOMValue; 
 public System.String DATUM_PRESSURE_OUOM
        {  
            get  
            {  
                return this.DATUM_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  DATUM_PRESSURE_OUOMValue, value); }
        } 
private  System.DateTime EFFECTIVE_DATEValue; 
 public System.DateTime EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime EXPIRY_DATEValue; 
 public System.DateTime EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.Decimal PACKER_DEPTHValue; 
 public System.Decimal PACKER_DEPTH
        {  
            get  
            {  
                return this.PACKER_DEPTHValue;  
            }  

          set { SetProperty(ref  PACKER_DEPTHValue, value); }
        } 
private  System.String PACKER_DEPTH_OUOMValue; 
 public System.String PACKER_DEPTH_OUOM
        {  
            get  
            {  
                return this.PACKER_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  PACKER_DEPTH_OUOMValue, value); }
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
private  System.Decimal PRESSURE_GRADIENTValue; 
 public System.Decimal PRESSURE_GRADIENT
        {  
            get  
            {  
                return this.PRESSURE_GRADIENTValue;  
            }  

          set { SetProperty(ref  PRESSURE_GRADIENTValue, value); }
        } 
private  System.String PRESSURE_GRADIENT_OUOMValue; 
 public System.String PRESSURE_GRADIENT_OUOM
        {  
            get  
            {  
                return this.PRESSURE_GRADIENT_OUOMValue;  
            }  

          set { SetProperty(ref  PRESSURE_GRADIENT_OUOMValue, value); }
        } 
private  System.Decimal RECORDER_DATUMValue; 
 public System.Decimal RECORDER_DATUM
        {  
            get  
            {  
                return this.RECORDER_DATUMValue;  
            }  

          set { SetProperty(ref  RECORDER_DATUMValue, value); }
        } 
private  System.String RECORDER_DATUM_OUOMValue; 
 public System.String RECORDER_DATUM_OUOM
        {  
            get  
            {  
                return this.RECORDER_DATUM_OUOMValue;  
            }  

          set { SetProperty(ref  RECORDER_DATUM_OUOMValue, value); }
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
private  System.Decimal REPORTED_RUN_TVDValue; 
 public System.Decimal REPORTED_RUN_TVD
        {  
            get  
            {  
                return this.REPORTED_RUN_TVDValue;  
            }  

          set { SetProperty(ref  REPORTED_RUN_TVDValue, value); }
        } 
private  System.String REPORTED_RUN_TVD_OUOMValue; 
 public System.String REPORTED_RUN_TVD_OUOM
        {  
            get  
            {  
                return this.REPORTED_RUN_TVD_OUOMValue;  
            }  

          set { SetProperty(ref  REPORTED_RUN_TVD_OUOMValue, value); }
        } 
private  System.Decimal RUN_DEPTHValue; 
 public System.Decimal RUN_DEPTH
        {  
            get  
            {  
                return this.RUN_DEPTHValue;  
            }  

          set { SetProperty(ref  RUN_DEPTHValue, value); }
        } 
private  System.String RUN_DEPTH_OUOMValue; 
 public System.String RUN_DEPTH_OUOM
        {  
            get  
            {  
                return this.RUN_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  RUN_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal RUN_DEPTH_TEMPERATUREValue; 
 public System.Decimal RUN_DEPTH_TEMPERATURE
        {  
            get  
            {  
                return this.RUN_DEPTH_TEMPERATUREValue;  
            }  

          set { SetProperty(ref  RUN_DEPTH_TEMPERATUREValue, value); }
        } 
private  System.String RUN_DEPTH_TEMPERATURE_OUOMValue; 
 public System.String RUN_DEPTH_TEMPERATURE_OUOM
        {  
            get  
            {  
                return this.RUN_DEPTH_TEMPERATURE_OUOMValue;  
            }  

          set { SetProperty(ref  RUN_DEPTH_TEMPERATURE_OUOMValue, value); }
        } 
private  System.Decimal SHUTIN_PERIODValue; 
 public System.Decimal SHUTIN_PERIOD
        {  
            get  
            {  
                return this.SHUTIN_PERIODValue;  
            }  

          set { SetProperty(ref  SHUTIN_PERIODValue, value); }
        } 
private  System.String SHUTIN_PERIOD_OUOMValue; 
 public System.String SHUTIN_PERIOD_OUOM
        {  
            get  
            {  
                return this.SHUTIN_PERIOD_OUOMValue;  
            }  

          set { SetProperty(ref  SHUTIN_PERIOD_OUOMValue, value); }
        } 
private  System.DateTime SURVEY_DATEValue; 
 public System.DateTime SURVEY_DATE
        {  
            get  
            {  
                return this.SURVEY_DATEValue;  
            }  

          set { SetProperty(ref  SURVEY_DATEValue, value); }
        } 
private  System.Decimal TEST_DURATIONValue; 
 public System.Decimal TEST_DURATION
        {  
            get  
            {  
                return this.TEST_DURATIONValue;  
            }  

          set { SetProperty(ref  TEST_DURATIONValue, value); }
        } 
private  System.String TEST_DURATION_OUOMValue; 
 public System.String TEST_DURATION_OUOM
        {  
            get  
            {  
                return this.TEST_DURATION_OUOMValue;  
            }  

          set { SetProperty(ref  TEST_DURATION_OUOMValue, value); }
        } 
private  System.String TUBING_SIZE_DESCValue; 
 public System.String TUBING_SIZE_DESC
        {  
            get  
            {  
                return this.TUBING_SIZE_DESCValue;  
            }  

          set { SetProperty(ref  TUBING_SIZE_DESCValue, value); }
        } 
private  System.Decimal WELL_HEAD_PRESSUREValue; 
 public System.Decimal WELL_HEAD_PRESSURE
        {  
            get  
            {  
                return this.WELL_HEAD_PRESSUREValue;  
            }  

          set { SetProperty(ref  WELL_HEAD_PRESSUREValue, value); }
        } 
private  System.String WELL_HEAD_PRESSURE_OUOMValue; 
 public System.String WELL_HEAD_PRESSURE_OUOM
        {  
            get  
            {  
                return this.WELL_HEAD_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  WELL_HEAD_PRESSURE_OUOMValue, value); }
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
private  System.DateTime ROW_CHANGED_DATEValue; 
 public System.DateTime ROW_CHANGED_DATE
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
private  System.DateTime ROW_CREATED_DATEValue; 
 public System.DateTime ROW_CREATED_DATE
        {  
            get  
            {  
                return this.ROW_CREATED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_DATEValue, value); }
        } 
private  System.DateTime ROW_EFFECTIVE_DATEValue; 
 public System.DateTime ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime ROW_EXPIRY_DATEValue; 
 public System.DateTime ROW_EXPIRY_DATE
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


    public WELL_PRESSURE_BH () { }

  }
}

