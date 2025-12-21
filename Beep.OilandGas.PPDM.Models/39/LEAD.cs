using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class LEAD: Entity

{

private  System.String LEAD_IDValue; 
 public System.String LEAD_ID
        {  
            get  
            {  
                return this.LEAD_IDValue;  
            }  

          set { SetProperty(ref  LEAD_IDValue, value); }
        } 
private  System.String LEAD_NAMEValue; 
 public System.String LEAD_NAME
        {  
            get  
            {  
                return this.LEAD_NAMEValue;  
            }  

          set { SetProperty(ref  LEAD_NAMEValue, value); }
        } 
private  System.String LEAD_SHORT_NAMEValue; 
 public System.String LEAD_SHORT_NAME
        {  
            get  
            {  
                return this.LEAD_SHORT_NAMEValue;  
            }  

          set { SetProperty(ref  LEAD_SHORT_NAMEValue, value); }
        } 
private  System.String FIELD_IDValue; 
 public System.String FIELD_ID
        {  
            get  
            {  
                return this.FIELD_IDValue;  
            }  

          set { SetProperty(ref  FIELD_IDValue, value); }
        } 
private  System.String PLAY_IDValue; 
 public System.String PLAY_ID
        {  
            get  
            {  
                return this.PLAY_IDValue;  
            }  

          set { SetProperty(ref  PLAY_IDValue, value); }
        } 
private  System.String LEAD_STATUSValue; 
 public System.String LEAD_STATUS
        {  
            get  
            {  
                return this.LEAD_STATUSValue;  
            }  

          set { SetProperty(ref  LEAD_STATUSValue, value); }
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
private  System.String LEAD_SOURCEValue; 
 public System.String LEAD_SOURCE
        {  
            get  
            {  
                return this.LEAD_SOURCEValue;  
            }  

          set { SetProperty(ref  LEAD_SOURCEValue, value); }
        } 
private  System.DateTime? INITIAL_ASSESSMENT_DATEValue; 
 public System.DateTime? INITIAL_ASSESSMENT_DATE
        {  
            get  
            {  
                return this.INITIAL_ASSESSMENT_DATEValue;  
            }  

          set { SetProperty(ref  INITIAL_ASSESSMENT_DATEValue, value); }
        } 
private  System.DateTime? PROMOTED_TO_PROSPECT_DATEValue; 
 public System.DateTime? PROMOTED_TO_PROSPECT_DATE
        {  
            get  
            {  
                return this.PROMOTED_TO_PROSPECT_DATEValue;  
            }  

          set { SetProperty(ref  PROMOTED_TO_PROSPECT_DATEValue, value); }
        } 
private  System.DateTime? REJECTED_DATEValue; 
 public System.DateTime? REJECTED_DATE
        {  
            get  
            {  
                return this.REJECTED_DATEValue;  
            }  

          set { SetProperty(ref  REJECTED_DATEValue, value); }
        } 
private  System.String REJECTION_REASONValue; 
 public System.String REJECTION_REASON
        {  
            get  
            {  
                return this.REJECTION_REASONValue;  
            }  

          set { SetProperty(ref  REJECTION_REASONValue, value); }
        } 
private  System.Decimal? LATITUDEValue; 
 public System.Decimal? LATITUDE
        {  
            get  
            {  
                return this.LATITUDEValue;  
            }  

          set { SetProperty(ref  LATITUDEValue, value); }
        } 
private  System.Decimal? LONGITUDEValue; 
 public System.Decimal? LONGITUDE
        {  
            get  
            {  
                return this.LONGITUDEValue;  
            }  

          set { SetProperty(ref  LONGITUDEValue, value); }
        } 
private  System.Decimal? ELEVATIONValue; 
 public System.Decimal? ELEVATION
        {  
            get  
            {  
                return this.ELEVATIONValue;  
            }  

          set { SetProperty(ref  ELEVATIONValue, value); }
        } 
private  System.String ELEVATION_OUOMValue; 
 public System.String ELEVATION_OUOM
        {  
            get  
            {  
                return this.ELEVATION_OUOMValue;  
            }  

          set { SetProperty(ref  ELEVATION_OUOMValue, value); }
        } 
private  System.String LOCATION_DESCRIPTIONValue; 
 public System.String LOCATION_DESCRIPTION
        {  
            get  
            {  
                return this.LOCATION_DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  LOCATION_DESCRIPTIONValue, value); }
        } 
private  System.String INITIAL_RISK_ASSESSMENTValue; 
 public System.String INITIAL_RISK_ASSESSMENT
        {  
            get  
            {  
                return this.INITIAL_RISK_ASSESSMENTValue;  
            }  

          set { SetProperty(ref  INITIAL_RISK_ASSESSMENTValue, value); }
        } 
private  System.Decimal? INITIAL_VOLUME_ESTIMATE_OILValue; 
 public System.Decimal? INITIAL_VOLUME_ESTIMATE_OIL
        {  
            get  
            {  
                return this.INITIAL_VOLUME_ESTIMATE_OILValue;  
            }  

          set { SetProperty(ref  INITIAL_VOLUME_ESTIMATE_OILValue, value); }
        } 
private  System.Decimal? INITIAL_VOLUME_ESTIMATE_GASValue; 
 public System.Decimal? INITIAL_VOLUME_ESTIMATE_GAS
        {  
            get  
            {  
                return this.INITIAL_VOLUME_ESTIMATE_GASValue;  
            }  

          set { SetProperty(ref  INITIAL_VOLUME_ESTIMATE_GASValue, value); }
        } 
private  System.String INITIAL_VOLUME_ESTIMATE_OUOMValue; 
 public System.String INITIAL_VOLUME_ESTIMATE_OUOM
        {  
            get  
            {  
                return this.INITIAL_VOLUME_ESTIMATE_OUOMValue;  
            }  

          set { SetProperty(ref  INITIAL_VOLUME_ESTIMATE_OUOMValue, value); }
        } 
private  System.String PROMOTED_TO_PROSPECT_IDValue; 
 public System.String PROMOTED_TO_PROSPECT_ID
        {  
            get  
            {  
                return this.PROMOTED_TO_PROSPECT_IDValue;  
            }  

          set { SetProperty(ref  PROMOTED_TO_PROSPECT_IDValue, value); }
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


    public LEAD () { }

  }
}
