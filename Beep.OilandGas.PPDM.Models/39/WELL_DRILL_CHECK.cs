using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_DRILL_CHECK: Entity,IPPDMEntity

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
private  System.Decimal DRILL_CHECK_OBS_NOValue; 
 public System.Decimal DRILL_CHECK_OBS_NO
        {  
            get  
            {  
                return this.DRILL_CHECK_OBS_NOValue;  
            }  

          set { SetProperty(ref  DRILL_CHECK_OBS_NOValue, value); }
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
private  System.String CHECK_SET_IDValue; 
 public System.String CHECK_SET_ID
        {  
            get  
            {  
                return this.CHECK_SET_IDValue;  
            }  

          set { SetProperty(ref  CHECK_SET_IDValue, value); }
        } 
private  System.String CHECK_TYPEValue; 
 public System.String CHECK_TYPE
        {  
            get  
            {  
                return this.CHECK_TYPEValue;  
            }  

          set { SetProperty(ref  CHECK_TYPEValue, value); }
        } 
private  System.String CONTRACTOR_NAMEValue; 
 public System.String CONTRACTOR_NAME
        {  
            get  
            {  
                return this.CONTRACTOR_NAMEValue;  
            }  

          set { SetProperty(ref  CONTRACTOR_NAMEValue, value); }
        } 
private  System.String CONTRACTOR_REP_BA_IDValue; 
 public System.String CONTRACTOR_REP_BA_ID
        {  
            get  
            {  
                return this.CONTRACTOR_REP_BA_IDValue;  
            }  

          set { SetProperty(ref  CONTRACTOR_REP_BA_IDValue, value); }
        } 
private  System.String DEFICIENT_INDValue; 
 public System.String DEFICIENT_IND
        {  
            get  
            {  
                return this.DEFICIENT_INDValue;  
            }  

          set { SetProperty(ref  DEFICIENT_INDValue, value); }
        } 
private  System.Decimal DEFICIENT_PERIODValue; 
 public System.Decimal DEFICIENT_PERIOD
        {  
            get  
            {  
                return this.DEFICIENT_PERIODValue;  
            }  

          set { SetProperty(ref  DEFICIENT_PERIODValue, value); }
        } 
private  System.String DEFICIENT_PERIOD_OUOMValue; 
 public System.String DEFICIENT_PERIOD_OUOM
        {  
            get  
            {  
                return this.DEFICIENT_PERIOD_OUOMValue;  
            }  

          set { SetProperty(ref  DEFICIENT_PERIOD_OUOMValue, value); }
        } 
private  System.String DEFICIENT_PERIOD_UOMValue; 
 public System.String DEFICIENT_PERIOD_UOM
        {  
            get  
            {  
                return this.DEFICIENT_PERIOD_UOMValue;  
            }  

          set { SetProperty(ref  DEFICIENT_PERIOD_UOMValue, value); }
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
private  System.String OPERATOR_NAMEValue; 
 public System.String OPERATOR_NAME
        {  
            get  
            {  
                return this.OPERATOR_NAMEValue;  
            }  

          set { SetProperty(ref  OPERATOR_NAMEValue, value); }
        } 
private  System.String OPERATOR_REP_BA_IDValue; 
 public System.String OPERATOR_REP_BA_ID
        {  
            get  
            {  
                return this.OPERATOR_REP_BA_IDValue;  
            }  

          set { SetProperty(ref  OPERATOR_REP_BA_IDValue, value); }
        } 
private  System.String PASSED_INDValue; 
 public System.String PASSED_IND
        {  
            get  
            {  
                return this.PASSED_INDValue;  
            }  

          set { SetProperty(ref  PASSED_INDValue, value); }
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
private  System.DateTime? RECORDED_TIMEValue; 
 public System.DateTime? RECORDED_TIME
        {  
            get  
            {  
                return this.RECORDED_TIMEValue;  
            }  

          set { SetProperty(ref  RECORDED_TIMEValue, value); }
        } 
private  System.String RECORDED_TIMEZONEValue; 
 public System.String RECORDED_TIMEZONE
        {  
            get  
            {  
                return this.RECORDED_TIMEZONEValue;  
            }  

          set { SetProperty(ref  RECORDED_TIMEZONEValue, value); }
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


    public WELL_DRILL_CHECK () { }

  }
}

