using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class REST_CONTACT: Entity,IPPDMEntity

{

private  System.String RESTRICTION_IDValue; 
 public System.String RESTRICTION_ID
        {  
            get  
            {  
                return this.RESTRICTION_IDValue;  
            }  

          set { SetProperty(ref  RESTRICTION_IDValue, value); }
        } 
private  System.Decimal RESTRICTION_VERSIONValue; 
 public System.Decimal RESTRICTION_VERSION
        {  
            get  
            {  
                return this.RESTRICTION_VERSIONValue;  
            }  

          set { SetProperty(ref  RESTRICTION_VERSIONValue, value); }
        } 
private  System.String BUSINESS_ASSOCIATE_IDValue; 
 public System.String BUSINESS_ASSOCIATE_ID
        {  
            get  
            {  
                return this.BUSINESS_ASSOCIATE_IDValue;  
            }  

          set { SetProperty(ref  BUSINESS_ASSOCIATE_IDValue, value); }
        } 
private  System.Decimal CONTACT_OBS_NOValue; 
 public System.Decimal CONTACT_OBS_NO
        {  
            get  
            {  
                return this.CONTACT_OBS_NOValue;  
            }  

          set { SetProperty(ref  CONTACT_OBS_NOValue, value); }
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
private  System.Decimal ADDRESS_OBS_NOValue; 
 public System.Decimal ADDRESS_OBS_NO
        {  
            get  
            {  
                return this.ADDRESS_OBS_NOValue;  
            }  

          set { SetProperty(ref  ADDRESS_OBS_NOValue, value); }
        } 
private  System.String ADDRESS_SOURCEValue; 
 public System.String ADDRESS_SOURCE
        {  
            get  
            {  
                return this.ADDRESS_SOURCEValue;  
            }  

          set { SetProperty(ref  ADDRESS_SOURCEValue, value); }
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
private  System.String PHONE_NUMValue; 
 public System.String PHONE_NUM
        {  
            get  
            {  
                return this.PHONE_NUMValue;  
            }  

          set { SetProperty(ref  PHONE_NUMValue, value); }
        } 
private  System.String PHONE_NUM_IDValue; 
 public System.String PHONE_NUM_ID
        {  
            get  
            {  
                return this.PHONE_NUM_IDValue;  
            }  

          set { SetProperty(ref  PHONE_NUM_IDValue, value); }
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
private  System.String PRIMARY_CONTACT_INDValue; 
 public System.String PRIMARY_CONTACT_IND
        {  
            get  
            {  
                return this.PRIMARY_CONTACT_INDValue;  
            }  

          set { SetProperty(ref  PRIMARY_CONTACT_INDValue, value); }
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


    public REST_CONTACT () { }

  }
}

