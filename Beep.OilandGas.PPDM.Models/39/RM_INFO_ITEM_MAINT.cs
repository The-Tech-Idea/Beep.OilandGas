using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class RM_INFO_ITEM_MAINT: Entity,IPPDMEntity

{

private  System.String INFO_ITEM_SUBTYPEValue; 
 public System.String INFO_ITEM_SUBTYPE
        {  
            get  
            {  
                return this.INFO_ITEM_SUBTYPEValue;  
            }  

          set { SetProperty(ref  INFO_ITEM_SUBTYPEValue, value); }
        } 
private  System.String INFORMATION_ITEM_IDValue; 
 public System.String INFORMATION_ITEM_ID
        {  
            get  
            {  
                return this.INFORMATION_ITEM_IDValue;  
            }  

          set { SetProperty(ref  INFORMATION_ITEM_IDValue, value); }
        } 
private  System.String MAINTAIN_IDValue; 
 public System.String MAINTAIN_ID
        {  
            get  
            {  
                return this.MAINTAIN_IDValue;  
            }  

          set { SetProperty(ref  MAINTAIN_IDValue, value); }
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
private  System.String INSTRUCTIONValue; 
 public System.String INSTRUCTION
        {  
            get  
            {  
                return this.INSTRUCTIONValue;  
            }  

          set { SetProperty(ref  INSTRUCTIONValue, value); }
        } 
private  System.String MAINT_BA_IDValue; 
 public System.String MAINT_BA_ID
        {  
            get  
            {  
                return this.MAINT_BA_IDValue;  
            }  

          set { SetProperty(ref  MAINT_BA_IDValue, value); }
        } 
private  System.DateTime? MAINT_COMPLETE_DATEValue; 
 public System.DateTime? MAINT_COMPLETE_DATE
        {  
            get  
            {  
                return this.MAINT_COMPLETE_DATEValue;  
            }  

          set { SetProperty(ref  MAINT_COMPLETE_DATEValue, value); }
        } 
private  System.DateTime? MAINT_DUE_DATEValue; 
 public System.DateTime? MAINT_DUE_DATE
        {  
            get  
            {  
                return this.MAINT_DUE_DATEValue;  
            }  

          set { SetProperty(ref  MAINT_DUE_DATEValue, value); }
        } 
private  System.Decimal MAINT_PERIODValue; 
 public System.Decimal MAINT_PERIOD
        {  
            get  
            {  
                return this.MAINT_PERIODValue;  
            }  

          set { SetProperty(ref  MAINT_PERIODValue, value); }
        } 
private  System.String MAINT_PERIOD_TYPEValue; 
 public System.String MAINT_PERIOD_TYPE
        {  
            get  
            {  
                return this.MAINT_PERIOD_TYPEValue;  
            }  

          set { SetProperty(ref  MAINT_PERIOD_TYPEValue, value); }
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
private  System.String SCHEDULED_INDValue; 
 public System.String SCHEDULED_IND
        {  
            get  
            {  
                return this.SCHEDULED_INDValue;  
            }  

          set { SetProperty(ref  SCHEDULED_INDValue, value); }
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


    public RM_INFO_ITEM_MAINT () { }

  }
}

