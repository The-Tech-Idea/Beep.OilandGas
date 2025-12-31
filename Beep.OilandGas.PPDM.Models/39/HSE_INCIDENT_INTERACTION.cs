using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class HSE_INCIDENT_INTERACTION: Entity,IPPDMEntity

{

private  System.String INCIDENT_IDValue; 
 public System.String INCIDENT_ID
        {  
            get  
            {  
                return this.INCIDENT_IDValue;  
            }  

          set { SetProperty(ref  INCIDENT_IDValue, value); }
        } 
private  System.Decimal INTERACTION_OBS_NOValue; 
 public System.Decimal INTERACTION_OBS_NO
        {  
            get  
            {  
                return this.INTERACTION_OBS_NOValue;  
            }  

          set { SetProperty(ref  INTERACTION_OBS_NOValue, value); }
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
private  System.Decimal CAUSE_OBS_NOValue; 
 public System.Decimal CAUSE_OBS_NO
        {  
            get  
            {  
                return this.CAUSE_OBS_NOValue;  
            }  

          set { SetProperty(ref  CAUSE_OBS_NOValue, value); }
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
private  System.Decimal DETAIL_OBS_NOValue; 
 public System.Decimal DETAIL_OBS_NO
        {  
            get  
            {  
                return this.DETAIL_OBS_NOValue;  
            }  

          set { SetProperty(ref  DETAIL_OBS_NOValue, value); }
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
private  System.Decimal EQUIP_OBS_NOValue; 
 public System.Decimal EQUIP_OBS_NO
        {  
            get  
            {  
                return this.EQUIP_OBS_NOValue;  
            }  

          set { SetProperty(ref  EQUIP_OBS_NOValue, value); }
        } 
private  System.Decimal EQUIP_ROLE_OBS_NOValue; 
 public System.Decimal EQUIP_ROLE_OBS_NO
        {  
            get  
            {  
                return this.EQUIP_ROLE_OBS_NOValue;  
            }  

          set { SetProperty(ref  EQUIP_ROLE_OBS_NOValue, value); }
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
private  System.String INCIDENT_SUBSTANCEValue; 
 public System.String INCIDENT_SUBSTANCE
        {  
            get  
            {  
                return this.INCIDENT_SUBSTANCEValue;  
            }  

          set { SetProperty(ref  INCIDENT_SUBSTANCEValue, value); }
        } 
private  System.String INTERACTION_TYPEValue; 
 public System.String INTERACTION_TYPE
        {  
            get  
            {  
                return this.INTERACTION_TYPEValue;  
            }  

          set { SetProperty(ref  INTERACTION_TYPEValue, value); }
        } 
private  System.Decimal PARTY_OBS_NOValue; 
 public System.Decimal PARTY_OBS_NO
        {  
            get  
            {  
                return this.PARTY_OBS_NOValue;  
            }  

          set { SetProperty(ref  PARTY_OBS_NOValue, value); }
        } 
private  System.Decimal PARTY_ROLE_OBS_NOValue; 
 public System.Decimal PARTY_ROLE_OBS_NO
        {  
            get  
            {  
                return this.PARTY_ROLE_OBS_NOValue;  
            }  

          set { SetProperty(ref  PARTY_ROLE_OBS_NOValue, value); }
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
private  System.Decimal RESPONSE_OBS_NOValue; 
 public System.Decimal RESPONSE_OBS_NO
        {  
            get  
            {  
                return this.RESPONSE_OBS_NOValue;  
            }  

          set { SetProperty(ref  RESPONSE_OBS_NOValue, value); }
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
private  System.Decimal SUBSTANCE_SEQ_NOValue; 
 public System.Decimal SUBSTANCE_SEQ_NO
        {  
            get  
            {  
                return this.SUBSTANCE_SEQ_NOValue;  
            }  

          set { SetProperty(ref  SUBSTANCE_SEQ_NOValue, value); }
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


    public HSE_INCIDENT_INTERACTION () { }

  }
}

