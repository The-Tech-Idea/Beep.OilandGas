using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class INT_SET_XREF: Entity,IPPDMEntity

{

private  System.String INTEREST_SET_IDValue; 
 public System.String INTEREST_SET_ID
        {  
            get  
            {  
                return this.INTEREST_SET_IDValue;  
            }  

          set { SetProperty(ref  INTEREST_SET_IDValue, value); }
        } 
private  System.Decimal INTEREST_SET_SEQ_NOValue; 
 public System.Decimal INTEREST_SET_SEQ_NO
        {  
            get  
            {  
                return this.INTEREST_SET_SEQ_NOValue;  
            }  

          set { SetProperty(ref  INTEREST_SET_SEQ_NOValue, value); }
        } 
private  System.String INTEREST_SET_ID_2Value; 
 public System.String INTEREST_SET_ID_2
        {  
            get  
            {  
                return this.INTEREST_SET_ID_2Value;  
            }  

          set { SetProperty(ref  INTEREST_SET_ID_2Value, value); }
        } 
private  System.Decimal INTEREST_SET_SEQ_NO_2Value; 
 public System.Decimal INTEREST_SET_SEQ_NO_2
        {  
            get  
            {  
                return this.INTEREST_SET_SEQ_NO_2Value;  
            }  

          set { SetProperty(ref  INTEREST_SET_SEQ_NO_2Value, value); }
        } 
private  System.Decimal INT_SET_XREF_OBS_NOValue; 
 public System.Decimal INT_SET_XREF_OBS_NO
        {  
            get  
            {  
                return this.INT_SET_XREF_OBS_NOValue;  
            }  

          set { SetProperty(ref  INT_SET_XREF_OBS_NOValue, value); }
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
private  System.String CONTRACT_IDValue; 
 public System.String CONTRACT_ID
        {  
            get  
            {  
                return this.CONTRACT_IDValue;  
            }  

          set { SetProperty(ref  CONTRACT_IDValue, value); }
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
private  System.String INT_SET_XREF_TYPEValue; 
 public System.String INT_SET_XREF_TYPE
        {  
            get  
            {  
                return this.INT_SET_XREF_TYPEValue;  
            }  

          set { SetProperty(ref  INT_SET_XREF_TYPEValue, value); }
        } 
private  System.String PARTNER_BA_IDValue; 
 public System.String PARTNER_BA_ID
        {  
            get  
            {  
                return this.PARTNER_BA_IDValue;  
            }  

          set { SetProperty(ref  PARTNER_BA_IDValue, value); }
        } 
private  System.String PARTNER_BA_ID_2Value; 
 public System.String PARTNER_BA_ID_2
        {  
            get  
            {  
                return this.PARTNER_BA_ID_2Value;  
            }  

          set { SetProperty(ref  PARTNER_BA_ID_2Value, value); }
        } 
private  System.Decimal PARTNER_OBS_NOValue; 
 public System.Decimal PARTNER_OBS_NO
        {  
            get  
            {  
                return this.PARTNER_OBS_NOValue;  
            }  

          set { SetProperty(ref  PARTNER_OBS_NOValue, value); }
        } 
private  System.Decimal PARTNER_OBS_NO_2Value; 
 public System.Decimal PARTNER_OBS_NO_2
        {  
            get  
            {  
                return this.PARTNER_OBS_NO_2Value;  
            }  

          set { SetProperty(ref  PARTNER_OBS_NO_2Value, value); }
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
private  System.String PROVISION_IDValue; 
 public System.String PROVISION_ID
        {  
            get  
            {  
                return this.PROVISION_IDValue;  
            }  

          set { SetProperty(ref  PROVISION_IDValue, value); }
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


    public INT_SET_XREF () { }

  }
}

