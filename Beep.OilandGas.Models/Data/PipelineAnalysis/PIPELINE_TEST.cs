using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_TEST : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String TEST_IDValue; 
 public System.String TEST_ID
        {  
            get  
            {  
                return this.TEST_IDValue;  
            }  

          set { SetProperty(ref  TEST_IDValue, value); }
        } 
private  System.String TEST_TYPEValue; 
 public System.String TEST_TYPE
        {  
            get  
            {  
                return this.TEST_TYPEValue;  
            }  

          set { SetProperty(ref  TEST_TYPEValue, value); }
        } 
private  System.DateTime? TEST_DATEValue; 
 public System.DateTime? TEST_DATE
        {  
            get  
            {  
                return this.TEST_DATEValue;  
            }  

          set { SetProperty(ref  TEST_DATEValue, value); }
        } 
private  System.Decimal TEST_PRESSUREValue; 
 public System.Decimal TEST_PRESSURE
        {  
            get  
            {  
                return this.TEST_PRESSUREValue;  
            }  

          set { SetProperty(ref  TEST_PRESSUREValue, value); }
        } 
private  System.String TEST_PRESSURE_OUOMValue; 
 public System.String TEST_PRESSURE_OUOM
        {  
            get  
            {  
                return this.TEST_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  TEST_PRESSURE_OUOMValue, value); }
        } 
private  System.String TEST_RESULTValue; 
 public System.String TEST_RESULT
        {  
            get  
            {  
                return this.TEST_RESULTValue;  
            }  

          set { SetProperty(ref  TEST_RESULTValue, value); }
        } 
private  System.Decimal TEST_DURATIONValue; 
 public System.Decimal TEST_DURATION
        {  
            get  
            {  
                return this.TEST_DURATIONValue;  
            }  

          set { SetProperty(ref  TEST_DURATIONValue, value); }
        } 
private  System.String TEST_DURATION_OUOMValue; 
 public System.String TEST_DURATION_OUOM
        {  
            get  
            {  
                return this.TEST_DURATION_OUOMValue;  
            }  

          set { SetProperty(ref  TEST_DURATION_OUOMValue, value); }
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
private  System.String TEST_METHODValue; 
 public System.String TEST_METHOD
        {  
            get  
            {  
                return this.TEST_METHODValue;  
            }  

          set { SetProperty(ref  TEST_METHODValue, value); }
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

    public PIPELINE_TEST () { }

  }
}


