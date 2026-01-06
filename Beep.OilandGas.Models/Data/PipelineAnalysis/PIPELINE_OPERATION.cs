using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_OPERATION: Entity,IPPDMEntity

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


    public PIPELINE_OPERATION () { }

  }
}




