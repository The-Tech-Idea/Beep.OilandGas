using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_FACILITY : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
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
private  System.Decimal CONNECTION_SEQ_NOValue; 
 public System.Decimal CONNECTION_SEQ_NO
        {  
            get  
            {  
                return this.CONNECTION_SEQ_NOValue;  
            }  

          set { SetProperty(ref  CONNECTION_SEQ_NOValue, value); }
        } 
private  System.String CONNECTION_TYPEValue; 
 public System.String CONNECTION_TYPE
        {  
            get  
            {  
                return this.CONNECTION_TYPEValue;  
            }  

          set { SetProperty(ref  CONNECTION_TYPEValue, value); }
        } 
private  System.DateTime? CONNECTION_DATEValue; 
 public System.DateTime? CONNECTION_DATE
        {  
            get  
            {  
                return this.CONNECTION_DATEValue;  
            }  

          set { SetProperty(ref  CONNECTION_DATEValue, value); }
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

    public PIPELINE_FACILITY () { }

  }
}
