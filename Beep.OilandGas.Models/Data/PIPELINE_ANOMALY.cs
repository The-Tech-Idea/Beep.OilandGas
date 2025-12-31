using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data 
{
public partial class PIPELINE_ANOMALY: Entity,IPPDMEntity

{

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String ANOMALY_IDValue; 
 public System.String ANOMALY_ID
        {  
            get  
            {  
                return this.ANOMALY_IDValue;  
            }  

          set { SetProperty(ref  ANOMALY_IDValue, value); }
        } 
private  System.String ANOMALY_TYPEValue; 
 public System.String ANOMALY_TYPE
        {  
            get  
            {  
                return this.ANOMALY_TYPEValue;  
            }  

          set { SetProperty(ref  ANOMALY_TYPEValue, value); }
        } 
private  System.String INSPECTION_IDValue; 
 public System.String INSPECTION_ID
        {  
            get  
            {  
                return this.INSPECTION_IDValue;  
            }  

          set { SetProperty(ref  INSPECTION_IDValue, value); }
        } 
private  System.Decimal LOCATION_STATION_MILEPOSTValue; 
 public System.Decimal LOCATION_STATION_MILEPOST
        {  
            get  
            {  
                return this.LOCATION_STATION_MILEPOSTValue;  
            }  

          set { SetProperty(ref  LOCATION_STATION_MILEPOSTValue, value); }
        } 
private  System.String LOCATION_STATION_MILEPOST_OUOMValue; 
 public System.String LOCATION_STATION_MILEPOST_OUOM
        {  
            get  
            {  
                return this.LOCATION_STATION_MILEPOST_OUOMValue;  
            }  

          set { SetProperty(ref  LOCATION_STATION_MILEPOST_OUOMValue, value); }
        } 
private  System.String SEVERITYValue; 
 public System.String SEVERITY
        {  
            get  
            {  
                return this.SEVERITYValue;  
            }  

          set { SetProperty(ref  SEVERITYValue, value); }
        } 
private  System.Decimal ANOMALY_LENGTHValue; 
 public System.Decimal ANOMALY_LENGTH
        {  
            get  
            {  
                return this.ANOMALY_LENGTHValue;  
            }  

          set { SetProperty(ref  ANOMALY_LENGTHValue, value); }
        } 
private  System.String ANOMALY_LENGTH_OUOMValue; 
 public System.String ANOMALY_LENGTH_OUOM
        {  
            get  
            {  
                return this.ANOMALY_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  ANOMALY_LENGTH_OUOMValue, value); }
        } 
private  System.Decimal ANOMALY_DEPTHValue; 
 public System.Decimal ANOMALY_DEPTH
        {  
            get  
            {  
                return this.ANOMALY_DEPTHValue;  
            }  

          set { SetProperty(ref  ANOMALY_DEPTHValue, value); }
        } 
private  System.String ANOMALY_DEPTH_OUOMValue; 
 public System.String ANOMALY_DEPTH_OUOM
        {  
            get  
            {  
                return this.ANOMALY_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  ANOMALY_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal ANOMALY_WIDTHValue; 
 public System.Decimal ANOMALY_WIDTH
        {  
            get  
            {  
                return this.ANOMALY_WIDTHValue;  
            }  

          set { SetProperty(ref  ANOMALY_WIDTHValue, value); }
        } 
private  System.String ANOMALY_WIDTH_OUOMValue; 
 public System.String ANOMALY_WIDTH_OUOM
        {  
            get  
            {  
                return this.ANOMALY_WIDTH_OUOMValue;  
            }  

          set { SetProperty(ref  ANOMALY_WIDTH_OUOMValue, value); }
        } 
private  System.String SEGMENT_IDValue; 
 public System.String SEGMENT_ID
        {  
            get  
            {  
                return this.SEGMENT_IDValue;  
            }  

          set { SetProperty(ref  SEGMENT_IDValue, value); }
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


    public PIPELINE_ANOMALY () { }

  }
}

