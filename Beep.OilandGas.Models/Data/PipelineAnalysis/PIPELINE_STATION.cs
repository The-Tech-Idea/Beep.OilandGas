using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_STATION : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String STATION_IDValue; 
 public System.String STATION_ID
        {  
            get  
            {  
                return this.STATION_IDValue;  
            }  

          set { SetProperty(ref  STATION_IDValue, value); }
        } 
private  System.String STATION_NAMEValue; 
 public System.String STATION_NAME
        {  
            get  
            {  
                return this.STATION_NAMEValue;  
            }  

          set { SetProperty(ref  STATION_NAMEValue, value); }
        } 
private  System.String STATION_SHORT_NAMEValue; 
 public System.String STATION_SHORT_NAME
        {  
            get  
            {  
                return this.STATION_SHORT_NAMEValue;  
            }  

          set { SetProperty(ref  STATION_SHORT_NAMEValue, value); }
        } 
private  System.String STATION_TYPEValue; 
 public System.String STATION_TYPE
        {  
            get  
            {  
                return this.STATION_TYPEValue;  
            }  

          set { SetProperty(ref  STATION_TYPEValue, value); }
        } 
private  System.String FACILITY_IDValue; 
 public System.String FACILITY_ID
        {  
            get  
            {  
                return this.FACILITY_IDValue;  
            }  

          set { SetProperty(ref  FACILITY_IDValue, value); }
        } 
private  System.String FACILITY_TYPEValue; 
 public System.String FACILITY_TYPE
        {  
            get  
            {  
                return this.FACILITY_TYPEValue;  
            }  

          set { SetProperty(ref  FACILITY_TYPEValue, value); }
        } 
private  System.String STATION_FUNCTIONValue; 
 public System.String STATION_FUNCTION
        {  
            get  
            {  
                return this.STATION_FUNCTIONValue;  
            }  

          set { SetProperty(ref  STATION_FUNCTIONValue, value); }
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

    public PIPELINE_STATION () { }

  }
}


