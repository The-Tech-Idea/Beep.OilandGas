using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_ANOMALY : ModelEntityBase {

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
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String REMARKValue; 

private  System.String SOURCEValue; 

    public PIPELINE_ANOMALY () { }

  }
}


