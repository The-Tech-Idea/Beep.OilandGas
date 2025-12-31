using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PPDM_METRIC: Entity,IPPDMEntity

{

private  System.String METRIC_IDValue; 
 public System.String METRIC_ID
        {  
            get  
            {  
                return this.METRIC_IDValue;  
            }  

          set { SetProperty(ref  METRIC_IDValue, value); }
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
private  System.DateTime? END_DATEValue; 
 public System.DateTime? END_DATE
        {  
            get  
            {  
                return this.END_DATEValue;  
            }  

          set { SetProperty(ref  END_DATEValue, value); }
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
private  System.String METRIC_NAMEValue; 
 public System.String METRIC_NAME
        {  
            get  
            {  
                return this.METRIC_NAMEValue;  
            }  

          set { SetProperty(ref  METRIC_NAMEValue, value); }
        } 
private  System.String METRIC_PROCEDUREValue; 
 public System.String METRIC_PROCEDURE
        {  
            get  
            {  
                return this.METRIC_PROCEDUREValue;  
            }  

          set { SetProperty(ref  METRIC_PROCEDUREValue, value); }
        } 
private  System.String METRIC_TYPEValue; 
 public System.String METRIC_TYPE
        {  
            get  
            {  
                return this.METRIC_TYPEValue;  
            }  

          set { SetProperty(ref  METRIC_TYPEValue, value); }
        } 
private  System.String ORGANIZATION_IDValue; 
 public System.String ORGANIZATION_ID
        {  
            get  
            {  
                return this.ORGANIZATION_IDValue;  
            }  

          set { SetProperty(ref  ORGANIZATION_IDValue, value); }
        } 
private  System.Decimal ORGANIZATION_SEQ_NOValue; 
 public System.Decimal ORGANIZATION_SEQ_NO
        {  
            get  
            {  
                return this.ORGANIZATION_SEQ_NOValue;  
            }  

          set { SetProperty(ref  ORGANIZATION_SEQ_NOValue, value); }
        } 
private  System.String OWNER_BA_IDValue; 
 public System.String OWNER_BA_ID
        {  
            get  
            {  
                return this.OWNER_BA_IDValue;  
            }  

          set { SetProperty(ref  OWNER_BA_IDValue, value); }
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
private  System.String PROCEDURE_IDValue; 
 public System.String PROCEDURE_ID
        {  
            get  
            {  
                return this.PROCEDURE_IDValue;  
            }  

          set { SetProperty(ref  PROCEDURE_IDValue, value); }
        } 
private  System.String PROCEDURE_SYSTEM_IDValue; 
 public System.String PROCEDURE_SYSTEM_ID
        {  
            get  
            {  
                return this.PROCEDURE_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  PROCEDURE_SYSTEM_IDValue, value); }
        } 
private  System.Decimal PROJECTED_FINAL_COUNTValue; 
 public System.Decimal PROJECTED_FINAL_COUNT
        {  
            get  
            {  
                return this.PROJECTED_FINAL_COUNTValue;  
            }  

          set { SetProperty(ref  PROJECTED_FINAL_COUNTValue, value); }
        } 
private  System.String PURPOSE_DESCValue; 
 public System.String PURPOSE_DESC
        {  
            get  
            {  
                return this.PURPOSE_DESCValue;  
            }  

          set { SetProperty(ref  PURPOSE_DESCValue, value); }
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
private  System.DateTime? START_DATEValue; 
 public System.DateTime? START_DATE
        {  
            get  
            {  
                return this.START_DATEValue;  
            }  

          set { SetProperty(ref  START_DATEValue, value); }
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


    public PPDM_METRIC () { }

  }
}

