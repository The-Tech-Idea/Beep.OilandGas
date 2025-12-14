using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_PRESSURE: Entity

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
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
        } 
private  System.String ASSIGNED_FIELDValue; 
 public System.String ASSIGNED_FIELD
        {  
            get  
            {  
                return this.ASSIGNED_FIELDValue;  
            }  

          set { SetProperty(ref  ASSIGNED_FIELDValue, value); }
        } 
private  System.Decimal BASE_DEPTHValue; 
 public System.Decimal BASE_DEPTH
        {  
            get  
            {  
                return this.BASE_DEPTHValue;  
            }  

          set { SetProperty(ref  BASE_DEPTHValue, value); }
        } 
private  System.String BASE_DEPTH_OUOMValue; 
 public System.String BASE_DEPTH_OUOM
        {  
            get  
            {  
                return this.BASE_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  BASE_DEPTH_OUOMValue, value); }
        } 
private  System.String BASE_STRAT_UNIT_IDValue; 
 public System.String BASE_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.BASE_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  BASE_STRAT_UNIT_IDValue, value); }
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
private  System.Decimal EVENT_OBS_NOValue; 
 public System.Decimal EVENT_OBS_NO
        {  
            get  
            {  
                return this.EVENT_OBS_NOValue;  
            }  

          set { SetProperty(ref  EVENT_OBS_NOValue, value); }
        } 
private  System.String EVENT_SOURCEValue; 
 public System.String EVENT_SOURCE
        {  
            get  
            {  
                return this.EVENT_SOURCEValue;  
            }  

          set { SetProperty(ref  EVENT_SOURCEValue, value); }
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
private  System.Decimal FLOW_CASING_PRESSUREValue; 
 public System.Decimal FLOW_CASING_PRESSURE
        {  
            get  
            {  
                return this.FLOW_CASING_PRESSUREValue;  
            }  

          set { SetProperty(ref  FLOW_CASING_PRESSUREValue, value); }
        } 
private  System.String FLOW_CASING_PRESSURE_OUOMValue; 
 public System.String FLOW_CASING_PRESSURE_OUOM
        {  
            get  
            {  
                return this.FLOW_CASING_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  FLOW_CASING_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal FLOW_TUBING_PRESSUREValue; 
 public System.Decimal FLOW_TUBING_PRESSURE
        {  
            get  
            {  
                return this.FLOW_TUBING_PRESSUREValue;  
            }  

          set { SetProperty(ref  FLOW_TUBING_PRESSUREValue, value); }
        } 
private  System.String FLOW_TUBING_PRESSURE_OUOMValue; 
 public System.String FLOW_TUBING_PRESSURE_OUOM
        {  
            get  
            {  
                return this.FLOW_TUBING_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  FLOW_TUBING_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal INIT_RESERVOIR_PRESSUREValue; 
 public System.Decimal INIT_RESERVOIR_PRESSURE
        {  
            get  
            {  
                return this.INIT_RESERVOIR_PRESSUREValue;  
            }  

          set { SetProperty(ref  INIT_RESERVOIR_PRESSUREValue, value); }
        } 
private  System.String INIT_RESERVOIR_PRESS_OUOMValue; 
 public System.String INIT_RESERVOIR_PRESS_OUOM
        {  
            get  
            {  
                return this.INIT_RESERVOIR_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  INIT_RESERVOIR_PRESS_OUOMValue, value); }
        } 
private  System.String POOL_DATUMValue; 
 public System.String POOL_DATUM
        {  
            get  
            {  
                return this.POOL_DATUMValue;  
            }  

          set { SetProperty(ref  POOL_DATUMValue, value); }
        } 
private  System.Decimal POOL_DATUM_DEPTHValue; 
 public System.Decimal POOL_DATUM_DEPTH
        {  
            get  
            {  
                return this.POOL_DATUM_DEPTHValue;  
            }  

          set { SetProperty(ref  POOL_DATUM_DEPTHValue, value); }
        } 
private  System.String POOL_DATUM_DEPTH_OUOMValue; 
 public System.String POOL_DATUM_DEPTH_OUOM
        {  
            get  
            {  
                return this.POOL_DATUM_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  POOL_DATUM_DEPTH_OUOMValue, value); }
        } 
private  System.String POOL_IDValue; 
 public System.String POOL_ID
        {  
            get  
            {  
                return this.POOL_IDValue;  
            }  

          set { SetProperty(ref  POOL_IDValue, value); }
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
private  System.String PROD_STRING_IDValue; 
 public System.String PROD_STRING_ID
        {  
            get  
            {  
                return this.PROD_STRING_IDValue;  
            }  

          set { SetProperty(ref  PROD_STRING_IDValue, value); }
        } 
private  System.String PROD_STRING_SOURCEValue; 
 public System.String PROD_STRING_SOURCE
        {  
            get  
            {  
                return this.PROD_STRING_SOURCEValue;  
            }  

          set { SetProperty(ref  PROD_STRING_SOURCEValue, value); }
        } 
private  System.Decimal PR_STR_FORM_OBS_NOValue; 
 public System.Decimal PR_STR_FORM_OBS_NO
        {  
            get  
            {  
                return this.PR_STR_FORM_OBS_NOValue;  
            }  

          set { SetProperty(ref  PR_STR_FORM_OBS_NOValue, value); }
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
private  System.Decimal SHUTIN_CASING_PRESSUREValue; 
 public System.Decimal SHUTIN_CASING_PRESSURE
        {  
            get  
            {  
                return this.SHUTIN_CASING_PRESSUREValue;  
            }  

          set { SetProperty(ref  SHUTIN_CASING_PRESSUREValue, value); }
        } 
private  System.String SHUTIN_CASING_PRESSURE_OUOMValue; 
 public System.String SHUTIN_CASING_PRESSURE_OUOM
        {  
            get  
            {  
                return this.SHUTIN_CASING_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  SHUTIN_CASING_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal SHUTIN_TUBING_PRESSUREValue; 
 public System.Decimal SHUTIN_TUBING_PRESSURE
        {  
            get  
            {  
                return this.SHUTIN_TUBING_PRESSUREValue;  
            }  

          set { SetProperty(ref  SHUTIN_TUBING_PRESSUREValue, value); }
        } 
private  System.String SHUTIN_TUBING_PRESSURE_OUOMValue; 
 public System.String SHUTIN_TUBING_PRESSURE_OUOM
        {  
            get  
            {  
                return this.SHUTIN_TUBING_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  SHUTIN_TUBING_PRESSURE_OUOMValue, value); }
        } 
private  System.String STRAT_NAME_SET_IDValue; 
 public System.String STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  STRAT_NAME_SET_IDValue, value); }
        } 
private  System.Decimal TOP_DEPTHValue; 
 public System.Decimal TOP_DEPTH
        {  
            get  
            {  
                return this.TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  TOP_DEPTHValue, value); }
        } 
private  System.String TOP_DEPTH_OUOMValue; 
 public System.String TOP_DEPTH_OUOM
        {  
            get  
            {  
                return this.TOP_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  TOP_DEPTH_OUOMValue, value); }
        } 
private  System.String TOP_STRAT_UNIT_IDValue; 
 public System.String TOP_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.TOP_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  TOP_STRAT_UNIT_IDValue, value); }
        } 
private  System.Decimal WELL_DATUM_DEPTHValue; 
 public System.Decimal WELL_DATUM_DEPTH
        {  
            get  
            {  
                return this.WELL_DATUM_DEPTHValue;  
            }  

          set { SetProperty(ref  WELL_DATUM_DEPTHValue, value); }
        } 
private  System.String WELL_DATUM_OUOMValue; 
 public System.String WELL_DATUM_OUOM
        {  
            get  
            {  
                return this.WELL_DATUM_OUOMValue;  
            }  

          set { SetProperty(ref  WELL_DATUM_OUOMValue, value); }
        } 
private  System.String WELL_TEST_NUMValue; 
 public System.String WELL_TEST_NUM
        {  
            get  
            {  
                return this.WELL_TEST_NUMValue;  
            }  

          set { SetProperty(ref  WELL_TEST_NUMValue, value); }
        } 
private  System.String WELL_TEST_RUN_NUMValue; 
 public System.String WELL_TEST_RUN_NUM
        {  
            get  
            {  
                return this.WELL_TEST_RUN_NUMValue;  
            }  

          set { SetProperty(ref  WELL_TEST_RUN_NUMValue, value); }
        } 
private  System.String WELL_TEST_SOURCEValue; 
 public System.String WELL_TEST_SOURCE
        {  
            get  
            {  
                return this.WELL_TEST_SOURCEValue;  
            }  

          set { SetProperty(ref  WELL_TEST_SOURCEValue, value); }
        } 
private  System.String WELL_TEST_TYPEValue; 
 public System.String WELL_TEST_TYPE
        {  
            get  
            {  
                return this.WELL_TEST_TYPEValue;  
            }  

          set { SetProperty(ref  WELL_TEST_TYPEValue, value); }
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


    public WELL_PRESSURE () { }

  }
}

