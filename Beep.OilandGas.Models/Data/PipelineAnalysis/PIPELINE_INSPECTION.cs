using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_INSPECTION : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
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
private  System.String INSPECTION_TYPEValue; 
 public System.String INSPECTION_TYPE
        {  
            get  
            {  
                return this.INSPECTION_TYPEValue;  
            }  

          set { SetProperty(ref  INSPECTION_TYPEValue, value); }
        } 
private  System.DateTime? INSPECTION_DATEValue; 
 public System.DateTime? INSPECTION_DATE
        {  
            get  
            {  
                return this.INSPECTION_DATEValue;  
            }  

          set { SetProperty(ref  INSPECTION_DATEValue, value); }
        } 
private  System.String INSPECTORValue; 
 public System.String INSPECTOR
        {  
            get  
            {  
                return this.INSPECTORValue;  
            }  

          set { SetProperty(ref  INSPECTORValue, value); }
        } 
private  System.String TOOL_MODELValue; 
 public System.String TOOL_MODEL
        {  
            get  
            {  
                return this.TOOL_MODELValue;  
            }  

          set { SetProperty(ref  TOOL_MODELValue, value); }
        } 
private  System.String TOOL_MANUFACTURERValue; 
 public System.String TOOL_MANUFACTURER
        {  
            get  
            {  
                return this.TOOL_MANUFACTURERValue;  
            }  

          set { SetProperty(ref  TOOL_MANUFACTURERValue, value); }
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
private  System.Decimal STATION_MILEPOSTValue; 
 public System.Decimal STATION_MILEPOST
        {  
            get  
            {  
                return this.STATION_MILEPOSTValue;  
            }  

          set { SetProperty(ref  STATION_MILEPOSTValue, value); }
        } 
private  System.String STATION_MILEPOST_OUOMValue; 
 public System.String STATION_MILEPOST_OUOM
        {  
            get  
            {  
                return this.STATION_MILEPOST_OUOMValue;  
            }  

          set { SetProperty(ref  STATION_MILEPOST_OUOMValue, value); }
        } 
private  System.String INSPECTION_RESULTValue; 
 public System.String INSPECTION_RESULT
        {  
            get  
            {  
                return this.INSPECTION_RESULTValue;  
            }  

          set { SetProperty(ref  INSPECTION_RESULTValue, value); }
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

    public PIPELINE_INSPECTION () { }

  }
}


