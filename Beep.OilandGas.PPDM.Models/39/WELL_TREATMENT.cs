using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_TREATMENT: Entity

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
private  System.String TREATMENT_TYPEValue; 
 public System.String TREATMENT_TYPE
        {  
            get  
            {  
                return this.TREATMENT_TYPEValue;  
            }  

          set { SetProperty(ref  TREATMENT_TYPEValue, value); }
        } 
private  System.Decimal TREATMENT_OBS_NOValue; 
 public System.Decimal TREATMENT_OBS_NO
        {  
            get  
            {  
                return this.TREATMENT_OBS_NOValue;  
            }  

          set { SetProperty(ref  TREATMENT_OBS_NOValue, value); }
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
private  System.String ADDITIVE_TYPEValue; 
 public System.String ADDITIVE_TYPE
        {  
            get  
            {  
                return this.ADDITIVE_TYPEValue;  
            }  

          set { SetProperty(ref  ADDITIVE_TYPEValue, value); }
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
private  System.Decimal COMPLETION_OBS_NOValue; 
 public System.Decimal COMPLETION_OBS_NO
        {  
            get  
            {  
                return this.COMPLETION_OBS_NOValue;  
            }  

          set { SetProperty(ref  COMPLETION_OBS_NOValue, value); }
        } 
private  System.String COMPLETION_SOURCEValue; 
 public System.String COMPLETION_SOURCE
        {  
            get  
            {  
                return this.COMPLETION_SOURCEValue;  
            }  

          set { SetProperty(ref  COMPLETION_SOURCEValue, value); }
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
private  System.Decimal FORM_BREAK_PRESSUREValue; 
 public System.Decimal FORM_BREAK_PRESSURE
        {  
            get  
            {  
                return this.FORM_BREAK_PRESSUREValue;  
            }  

          set { SetProperty(ref  FORM_BREAK_PRESSUREValue, value); }
        } 
private  System.String FORM_BREAK_PRESSURE_OUOMValue; 
 public System.String FORM_BREAK_PRESSURE_OUOM
        {  
            get  
            {  
                return this.FORM_BREAK_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  FORM_BREAK_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal INJECTION_RATEValue; 
 public System.Decimal INJECTION_RATE
        {  
            get  
            {  
                return this.INJECTION_RATEValue;  
            }  

          set { SetProperty(ref  INJECTION_RATEValue, value); }
        } 
private  System.String INJECTION_RATE_OUOMValue; 
 public System.String INJECTION_RATE_OUOM
        {  
            get  
            {  
                return this.INJECTION_RATE_OUOMValue;  
            }  

          set { SetProperty(ref  INJECTION_RATE_OUOMValue, value); }
        } 
private  System.Decimal INSTANT_SI_PRESSUREValue; 
 public System.Decimal INSTANT_SI_PRESSURE
        {  
            get  
            {  
                return this.INSTANT_SI_PRESSUREValue;  
            }  

          set { SetProperty(ref  INSTANT_SI_PRESSUREValue, value); }
        } 
private  System.String INSTANT_SI_PRESSURE_OUOMValue; 
 public System.String INSTANT_SI_PRESSURE_OUOM
        {  
            get  
            {  
                return this.INSTANT_SI_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  INSTANT_SI_PRESSURE_OUOMValue, value); }
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
private  System.Decimal PROPPANT_AGENT_AMOUNTValue; 
 public System.Decimal PROPPANT_AGENT_AMOUNT
        {  
            get  
            {  
                return this.PROPPANT_AGENT_AMOUNTValue;  
            }  

          set { SetProperty(ref  PROPPANT_AGENT_AMOUNTValue, value); }
        } 
private  System.String PROPPANT_AGENT_AMOUNT_OUOMValue; 
 public System.String PROPPANT_AGENT_AMOUNT_OUOM
        {  
            get  
            {  
                return this.PROPPANT_AGENT_AMOUNT_OUOMValue;  
            }  

          set { SetProperty(ref  PROPPANT_AGENT_AMOUNT_OUOMValue, value); }
        } 
private  System.String PROPPANT_AGENT_AMOUNT_UOMValue; 
 public System.String PROPPANT_AGENT_AMOUNT_UOM
        {  
            get  
            {  
                return this.PROPPANT_AGENT_AMOUNT_UOMValue;  
            }  

          set { SetProperty(ref  PROPPANT_AGENT_AMOUNT_UOMValue, value); }
        } 
private  System.String PROPPANT_AGENT_MESH_SIZEValue; 
 public System.String PROPPANT_AGENT_MESH_SIZE
        {  
            get  
            {  
                return this.PROPPANT_AGENT_MESH_SIZEValue;  
            }  

          set { SetProperty(ref  PROPPANT_AGENT_MESH_SIZEValue, value); }
        } 
private  System.String PROPPANT_AGENT_TYPEValue; 
 public System.String PROPPANT_AGENT_TYPE
        {  
            get  
            {  
                return this.PROPPANT_AGENT_TYPEValue;  
            }  

          set { SetProperty(ref  PROPPANT_AGENT_TYPEValue, value); }
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
private  System.String RUN_NUMValue; 
 public System.String RUN_NUM
        {  
            get  
            {  
                return this.RUN_NUMValue;  
            }  

          set { SetProperty(ref  RUN_NUMValue, value); }
        } 
private  System.Decimal STAGE_NOValue; 
 public System.Decimal STAGE_NO
        {  
            get  
            {  
                return this.STAGE_NOValue;  
            }  

          set { SetProperty(ref  STAGE_NOValue, value); }
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
private  System.String TEST_NUMValue; 
 public System.String TEST_NUM
        {  
            get  
            {  
                return this.TEST_NUMValue;  
            }  

          set { SetProperty(ref  TEST_NUMValue, value); }
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
private  System.Decimal TREATMENT_AMOUNTValue; 
 public System.Decimal TREATMENT_AMOUNT
        {  
            get  
            {  
                return this.TREATMENT_AMOUNTValue;  
            }  

          set { SetProperty(ref  TREATMENT_AMOUNTValue, value); }
        } 
private  System.String TREATMENT_AMOUNT_OUOMValue; 
 public System.String TREATMENT_AMOUNT_OUOM
        {  
            get  
            {  
                return this.TREATMENT_AMOUNT_OUOMValue;  
            }  

          set { SetProperty(ref  TREATMENT_AMOUNT_OUOMValue, value); }
        } 
private  System.String TREATMENT_AMOUNT_UOMValue; 
 public System.String TREATMENT_AMOUNT_UOM
        {  
            get  
            {  
                return this.TREATMENT_AMOUNT_UOMValue;  
            }  

          set { SetProperty(ref  TREATMENT_AMOUNT_UOMValue, value); }
        } 
private  System.String TREATMENT_BA_IDValue; 
 public System.String TREATMENT_BA_ID
        {  
            get  
            {  
                return this.TREATMENT_BA_IDValue;  
            }  

          set { SetProperty(ref  TREATMENT_BA_IDValue, value); }
        } 
private  System.DateTime TREATMENT_DATEValue; 
 public System.DateTime TREATMENT_DATE
        {  
            get  
            {  
                return this.TREATMENT_DATEValue;  
            }  

          set { SetProperty(ref  TREATMENT_DATEValue, value); }
        } 
private  System.String TREATMENT_FLUID_TYPEValue; 
 public System.String TREATMENT_FLUID_TYPE
        {  
            get  
            {  
                return this.TREATMENT_FLUID_TYPEValue;  
            }  

          set { SetProperty(ref  TREATMENT_FLUID_TYPEValue, value); }
        } 
private  System.Decimal TREATMENT_PRESSUREValue; 
 public System.Decimal TREATMENT_PRESSURE
        {  
            get  
            {  
                return this.TREATMENT_PRESSUREValue;  
            }  

          set { SetProperty(ref  TREATMENT_PRESSUREValue, value); }
        } 
private  System.String TREATMENT_PRESSURE_OUOMValue; 
 public System.String TREATMENT_PRESSURE_OUOM
        {  
            get  
            {  
                return this.TREATMENT_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  TREATMENT_PRESSURE_OUOMValue, value); }
        } 
private  System.String TREATMENT_STATUSValue; 
 public System.String TREATMENT_STATUS
        {  
            get  
            {  
                return this.TREATMENT_STATUSValue;  
            }  

          set { SetProperty(ref  TREATMENT_STATUSValue, value); }
        } 
private  System.String TREATMENT_STATUS_TYPEValue; 
 public System.String TREATMENT_STATUS_TYPE
        {  
            get  
            {  
                return this.TREATMENT_STATUS_TYPEValue;  
            }  

          set { SetProperty(ref  TREATMENT_STATUS_TYPEValue, value); }
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


    public WELL_TREATMENT () { }

  }
}

