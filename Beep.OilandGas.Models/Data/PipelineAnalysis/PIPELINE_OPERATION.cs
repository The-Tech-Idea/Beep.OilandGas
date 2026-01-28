using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_OPERATION : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String OPERATION_IDValue; 
 public System.String OPERATION_ID
        {  
            get  
            {  
                return this.OPERATION_IDValue;  
            }  

          set { SetProperty(ref  OPERATION_IDValue, value); }
        } 
private  System.DateTime? OPERATION_DATEValue; 
 public System.DateTime? OPERATION_DATE
        {  
            get  
            {  
                return this.OPERATION_DATEValue;  
            }  

          set { SetProperty(ref  OPERATION_DATEValue, value); }
        } 
private  System.Decimal FLOW_RATEValue; 
 public System.Decimal FLOW_RATE
        {  
            get  
            {  
                return this.FLOW_RATEValue;  
            }  

          set { SetProperty(ref  FLOW_RATEValue, value); }
        } 
private  System.String FLOW_RATE_OUOMValue; 
 public System.String FLOW_RATE_OUOM
        {  
            get  
            {  
                return this.FLOW_RATE_OUOMValue;  
            }  

          set { SetProperty(ref  FLOW_RATE_OUOMValue, value); }
        } 
private  System.Decimal PRESSUREValue; 
 public System.Decimal PRESSURE
        {  
            get  
            {  
                return this.PRESSUREValue;  
            }  

          set { SetProperty(ref  PRESSUREValue, value); }
        } 
private  System.String PRESSURE_OUOMValue; 
 public System.String PRESSURE_OUOM
        {  
            get  
            {  
                return this.PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal TEMPERATUREValue; 
 public System.Decimal TEMPERATURE
        {  
            get  
            {  
                return this.TEMPERATUREValue;  
            }  

          set { SetProperty(ref  TEMPERATUREValue, value); }
        } 
private  System.String TEMPERATURE_OUOMValue; 
 public System.String TEMPERATURE_OUOM
        {  
            get  
            {  
                return this.TEMPERATURE_OUOMValue;  
            }  

          set { SetProperty(ref  TEMPERATURE_OUOMValue, value); }
        } 
private  System.Decimal THROUGHPUTValue; 
 public System.Decimal THROUGHPUT
        {  
            get  
            {  
                return this.THROUGHPUTValue;  
            }  

          set { SetProperty(ref  THROUGHPUTValue, value); }
        } 
private  System.String THROUGHPUT_OUOMValue; 
 public System.String THROUGHPUT_OUOM
        {  
            get  
            {  
                return this.THROUGHPUT_OUOMValue;  
            }  

          set { SetProperty(ref  THROUGHPUT_OUOMValue, value); }
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

    public PIPELINE_OPERATION () { }

  }
}
