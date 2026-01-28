using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_WELL : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
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
private  System.String FLOW_DIRECTIONValue; 
 public System.String FLOW_DIRECTION
        {  
            get  
            {  
                return this.FLOW_DIRECTIONValue;  
            }  

          set { SetProperty(ref  FLOW_DIRECTIONValue, value); }
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

    public PIPELINE_WELL () { }

  }
}
