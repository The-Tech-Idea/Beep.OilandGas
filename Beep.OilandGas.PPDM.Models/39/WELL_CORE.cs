using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_CORE: Entity,IPPDMEntity

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
private  System.String CORE_IDValue; 
 public System.String CORE_ID
        {  
            get  
            {  
                return this.CORE_IDValue;  
            }  

          set { SetProperty(ref  CORE_IDValue, value); }
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
private  System.String CONTRACTORValue; 
 public System.String CONTRACTOR
        {  
            get  
            {  
                return this.CONTRACTORValue;  
            }  

          set { SetProperty(ref  CONTRACTORValue, value); }
        } 
private  System.Decimal CORE_BARREL_SIZEValue; 
 public System.Decimal CORE_BARREL_SIZE
        {  
            get  
            {  
                return this.CORE_BARREL_SIZEValue;  
            }  

          set { SetProperty(ref  CORE_BARREL_SIZEValue, value); }
        } 
private  System.String CORE_BARREL_SIZE_OUOMValue; 
 public System.String CORE_BARREL_SIZE_OUOM
        {  
            get  
            {  
                return this.CORE_BARREL_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  CORE_BARREL_SIZE_OUOMValue, value); }
        } 
private  System.Decimal CORE_DIAMETERValue; 
 public System.Decimal CORE_DIAMETER
        {  
            get  
            {  
                return this.CORE_DIAMETERValue;  
            }  

          set { SetProperty(ref  CORE_DIAMETERValue, value); }
        } 
private  System.String CORE_DIAMETER_OUOMValue; 
 public System.String CORE_DIAMETER_OUOM
        {  
            get  
            {  
                return this.CORE_DIAMETER_OUOMValue;  
            }  

          set { SetProperty(ref  CORE_DIAMETER_OUOMValue, value); }
        } 
private  System.String CORE_HANDLING_TYPEValue; 
 public System.String CORE_HANDLING_TYPE
        {  
            get  
            {  
                return this.CORE_HANDLING_TYPEValue;  
            }  

          set { SetProperty(ref  CORE_HANDLING_TYPEValue, value); }
        } 
private  System.String CORE_ORIENTED_INDValue; 
 public System.String CORE_ORIENTED_IND
        {  
            get  
            {  
                return this.CORE_ORIENTED_INDValue;  
            }  

          set { SetProperty(ref  CORE_ORIENTED_INDValue, value); }
        } 
private  System.String CORE_SHOW_TYPEValue; 
 public System.String CORE_SHOW_TYPE
        {  
            get  
            {  
                return this.CORE_SHOW_TYPEValue;  
            }  

          set { SetProperty(ref  CORE_SHOW_TYPEValue, value); }
        } 
private  System.String CORE_TYPEValue; 
 public System.String CORE_TYPE
        {  
            get  
            {  
                return this.CORE_TYPEValue;  
            }  

          set { SetProperty(ref  CORE_TYPEValue, value); }
        } 
private  System.String CORING_FLUIDValue; 
 public System.String CORING_FLUID
        {  
            get  
            {  
                return this.CORING_FLUIDValue;  
            }  

          set { SetProperty(ref  CORING_FLUIDValue, value); }
        } 
private  System.String DIGIT_AVAIL_INDValue; 
 public System.String DIGIT_AVAIL_IND
        {  
            get  
            {  
                return this.DIGIT_AVAIL_INDValue;  
            }  

          set { SetProperty(ref  DIGIT_AVAIL_INDValue, value); }
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
private  System.String GAMMA_CORRELATION_INDValue; 
 public System.String GAMMA_CORRELATION_IND
        {  
            get  
            {  
                return this.GAMMA_CORRELATION_INDValue;  
            }  

          set { SetProperty(ref  GAMMA_CORRELATION_INDValue, value); }
        } 
private  System.Decimal OPERATION_SEQ_NOValue; 
 public System.Decimal OPERATION_SEQ_NO
        {  
            get  
            {  
                return this.OPERATION_SEQ_NOValue;  
            }  

          set { SetProperty(ref  OPERATION_SEQ_NOValue, value); }
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
private  System.String PRIMARY_CORE_STRAT_UNIT_IDValue; 
 public System.String PRIMARY_CORE_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.PRIMARY_CORE_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  PRIMARY_CORE_STRAT_UNIT_IDValue, value); }
        } 
private  System.Decimal RECOVERED_AMOUNTValue; 
 public System.Decimal RECOVERED_AMOUNT
        {  
            get  
            {  
                return this.RECOVERED_AMOUNTValue;  
            }  

          set { SetProperty(ref  RECOVERED_AMOUNTValue, value); }
        } 
private  System.String RECOVERED_AMOUNT_OUOMValue; 
 public System.String RECOVERED_AMOUNT_OUOM
        {  
            get  
            {  
                return this.RECOVERED_AMOUNT_OUOMValue;  
            }  

          set { SetProperty(ref  RECOVERED_AMOUNT_OUOMValue, value); }
        } 
private  System.String RECOVERED_AMOUNT_UOMValue; 
 public System.String RECOVERED_AMOUNT_UOM
        {  
            get  
            {  
                return this.RECOVERED_AMOUNT_UOMValue;  
            }  

          set { SetProperty(ref  RECOVERED_AMOUNT_UOMValue, value); }
        } 
private  System.DateTime? RECOVERY_DATEValue; 
 public System.DateTime? RECOVERY_DATE
        {  
            get  
            {  
                return this.RECOVERY_DATEValue;  
            }  

          set { SetProperty(ref  RECOVERY_DATEValue, value); }
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
private  System.String REPORTED_CORE_NUMValue; 
 public System.String REPORTED_CORE_NUM
        {  
            get  
            {  
                return this.REPORTED_CORE_NUMValue;  
            }  

          set { SetProperty(ref  REPORTED_CORE_NUMValue, value); }
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
private  System.Decimal SHOT_RECOVERED_COUNTValue; 
 public System.Decimal SHOT_RECOVERED_COUNT
        {  
            get  
            {  
                return this.SHOT_RECOVERED_COUNTValue;  
            }  

          set { SetProperty(ref  SHOT_RECOVERED_COUNTValue, value); }
        } 
private  System.String SIDEWALL_INDValue; 
 public System.String SIDEWALL_IND
        {  
            get  
            {  
                return this.SIDEWALL_INDValue;  
            }  

          set { SetProperty(ref  SIDEWALL_INDValue, value); }
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
private  System.Decimal TOTAL_SHOT_COUNTValue; 
 public System.Decimal TOTAL_SHOT_COUNT
        {  
            get  
            {  
                return this.TOTAL_SHOT_COUNTValue;  
            }  

          set { SetProperty(ref  TOTAL_SHOT_COUNTValue, value); }
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


    public WELL_CORE () { }

  }
}

