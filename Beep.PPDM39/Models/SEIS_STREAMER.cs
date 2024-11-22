using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class SEIS_STREAMER: Entity

{

private  System.String STREAMER_IDValue; 
 public System.String STREAMER_ID
        {  
            get  
            {  
                return this.STREAMER_IDValue;  
            }  

          set { SetProperty(ref  STREAMER_IDValue, value); }
        } 
private  System.String ACQTN_DESIGN_IDValue; 
 public System.String ACQTN_DESIGN_ID
        {  
            get  
            {  
                return this.ACQTN_DESIGN_IDValue;  
            }  

          set { SetProperty(ref  ACQTN_DESIGN_IDValue, value); }
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
private  System.String CABLE_MAKEValue; 
 public System.String CABLE_MAKE
        {  
            get  
            {  
                return this.CABLE_MAKEValue;  
            }  

          set { SetProperty(ref  CABLE_MAKEValue, value); }
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
private  System.String SF_SUBTYPEValue; 
 public System.String SF_SUBTYPE
        {  
            get  
            {  
                return this.SF_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SF_SUBTYPEValue, value); }
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
private  System.Decimal STREAMER_LENGTHValue; 
 public System.Decimal STREAMER_LENGTH
        {  
            get  
            {  
                return this.STREAMER_LENGTHValue;  
            }  

          set { SetProperty(ref  STREAMER_LENGTHValue, value); }
        } 
private  System.String STREAMER_LENGTH_OUOMValue; 
 public System.String STREAMER_LENGTH_OUOM
        {  
            get  
            {  
                return this.STREAMER_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  STREAMER_LENGTH_OUOMValue, value); }
        } 
private  System.String STREAMER_POSITIONValue; 
 public System.String STREAMER_POSITION
        {  
            get  
            {  
                return this.STREAMER_POSITIONValue;  
            }  

          set { SetProperty(ref  STREAMER_POSITIONValue, value); }
        } 
private  System.Decimal VESSEL_CONFIG_OBS_NOValue; 
 public System.Decimal VESSEL_CONFIG_OBS_NO
        {  
            get  
            {  
                return this.VESSEL_CONFIG_OBS_NOValue;  
            }  

          set { SetProperty(ref  VESSEL_CONFIG_OBS_NOValue, value); }
        } 
private  System.String VESSEL_IDValue; 
 public System.String VESSEL_ID
        {  
            get  
            {  
                return this.VESSEL_IDValue;  
            }  

          set { SetProperty(ref  VESSEL_IDValue, value); }
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


    public SEIS_STREAMER () { }

  }
}

