using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class ENTITLEMENT: Entity,IPPDMEntity

{

private  System.String ENTITLEMENT_IDValue; 
 public System.String ENTITLEMENT_ID
        {  
            get  
            {  
                return this.ENTITLEMENT_IDValue;  
            }  

          set { SetProperty(ref  ENTITLEMENT_IDValue, value); }
        } 
private  System.String ACCESS_CONDITIONValue; 
 public System.String ACCESS_CONDITION
        {  
            get  
            {  
                return this.ACCESS_CONDITIONValue;  
            }  

          set { SetProperty(ref  ACCESS_CONDITIONValue, value); }
        } 
private  System.String ACCESS_COND_CODEValue; 
 public System.String ACCESS_COND_CODE
        {  
            get  
            {  
                return this.ACCESS_COND_CODEValue;  
            }  

          set { SetProperty(ref  ACCESS_COND_CODEValue, value); }
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
private  System.String DESCRIPTIONValue; 
 public System.String DESCRIPTION
        {  
            get  
            {  
                return this.DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  DESCRIPTIONValue, value); }
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
private  System.String ENTITLEMENT_NAMEValue; 
 public System.String ENTITLEMENT_NAME
        {  
            get  
            {  
                return this.ENTITLEMENT_NAMEValue;  
            }  

          set { SetProperty(ref  ENTITLEMENT_NAMEValue, value); }
        } 
private  System.String ENTITLEMENT_TYPEValue; 
 public System.String ENTITLEMENT_TYPE
        {  
            get  
            {  
                return this.ENTITLEMENT_TYPEValue;  
            }  

          set { SetProperty(ref  ENTITLEMENT_TYPEValue, value); }
        } 
private  System.String EXPIRY_ACTIONValue; 
 public System.String EXPIRY_ACTION
        {  
            get  
            {  
                return this.EXPIRY_ACTIONValue;  
            }  

          set { SetProperty(ref  EXPIRY_ACTIONValue, value); }
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
private  System.String PPDM_GUIDValue; 
 public System.String PPDM_GUID
        {  
            get  
            {  
                return this.PPDM_GUIDValue;  
            }  

          set { SetProperty(ref  PPDM_GUIDValue, value); }
        } 
private  System.Decimal PRIMARY_TERMValue; 
 public System.Decimal PRIMARY_TERM
        {  
            get  
            {  
                return this.PRIMARY_TERMValue;  
            }  

          set { SetProperty(ref  PRIMARY_TERMValue, value); }
        } 
private  System.String PRIMARY_TERM_OUOMValue; 
 public System.String PRIMARY_TERM_OUOM
        {  
            get  
            {  
                return this.PRIMARY_TERM_OUOMValue;  
            }  

          set { SetProperty(ref  PRIMARY_TERM_OUOMValue, value); }
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
private  System.String SECURITY_DESCValue; 
 public System.String SECURITY_DESC
        {  
            get  
            {  
                return this.SECURITY_DESCValue;  
            }  

          set { SetProperty(ref  SECURITY_DESCValue, value); }
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
private  System.String USE_CONDITIONValue; 
 public System.String USE_CONDITION
        {  
            get  
            {  
                return this.USE_CONDITIONValue;  
            }  

          set { SetProperty(ref  USE_CONDITIONValue, value); }
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


    public ENTITLEMENT () { }

  }
}

