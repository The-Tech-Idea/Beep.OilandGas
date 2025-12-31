using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_VESSEL: Entity,IPPDMEntity

{

private  System.String SF_SUBTYPEValue; 
 public System.String SF_SUBTYPE
        {  
            get  
            {  
                return this.SF_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SF_SUBTYPEValue, value); }
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
private  System.Decimal VESSEL_CONFIG_OBS_NOValue; 
 public System.Decimal VESSEL_CONFIG_OBS_NO
        {  
            get  
            {  
                return this.VESSEL_CONFIG_OBS_NOValue;  
            }  

          set { SetProperty(ref  VESSEL_CONFIG_OBS_NOValue, value); }
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
private  System.Decimal FATH_AZIMUTHValue; 
 public System.Decimal FATH_AZIMUTH
        {  
            get  
            {  
                return this.FATH_AZIMUTHValue;  
            }  

          set { SetProperty(ref  FATH_AZIMUTHValue, value); }
        } 
private  System.Decimal FATH_OFFSETValue; 
 public System.Decimal FATH_OFFSET
        {  
            get  
            {  
                return this.FATH_OFFSETValue;  
            }  

          set { SetProperty(ref  FATH_OFFSETValue, value); }
        } 
private  System.String MASTER_VESSEL_INDValue; 
 public System.String MASTER_VESSEL_IND
        {  
            get  
            {  
                return this.MASTER_VESSEL_INDValue;  
            }  

          set { SetProperty(ref  MASTER_VESSEL_INDValue, value); }
        } 
private  System.String OFFSET_OUOMValue; 
 public System.String OFFSET_OUOM
        {  
            get  
            {  
                return this.OFFSET_OUOMValue;  
            }  

          set { SetProperty(ref  OFFSET_OUOMValue, value); }
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
private  System.String REFERENCE_POINTValue; 
 public System.String REFERENCE_POINT
        {  
            get  
            {  
                return this.REFERENCE_POINTValue;  
            }  

          set { SetProperty(ref  REFERENCE_POINTValue, value); }
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
private  System.Decimal SHOT_OFFSETValue; 
 public System.Decimal SHOT_OFFSET
        {  
            get  
            {  
                return this.SHOT_OFFSETValue;  
            }  

          set { SetProperty(ref  SHOT_OFFSETValue, value); }
        } 
private  System.String SLAVE_VESSEL_INDValue; 
 public System.String SLAVE_VESSEL_IND
        {  
            get  
            {  
                return this.SLAVE_VESSEL_INDValue;  
            }  

          set { SetProperty(ref  SLAVE_VESSEL_INDValue, value); }
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
private  System.Decimal STREAMER_FAR_OFFSETValue; 
 public System.Decimal STREAMER_FAR_OFFSET
        {  
            get  
            {  
                return this.STREAMER_FAR_OFFSETValue;  
            }  

          set { SetProperty(ref  STREAMER_FAR_OFFSETValue, value); }
        } 
private  System.Decimal STREAMER_NEAR_OFFSETValue; 
 public System.Decimal STREAMER_NEAR_OFFSET
        {  
            get  
            {  
                return this.STREAMER_NEAR_OFFSETValue;  
            }  

          set { SetProperty(ref  STREAMER_NEAR_OFFSETValue, value); }
        } 
private  System.Decimal VESSEL_AZIMUTHValue; 
 public System.Decimal VESSEL_AZIMUTH
        {  
            get  
            {  
                return this.VESSEL_AZIMUTHValue;  
            }  

          set { SetProperty(ref  VESSEL_AZIMUTHValue, value); }
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


    public SEIS_VESSEL () { }

  }
}

