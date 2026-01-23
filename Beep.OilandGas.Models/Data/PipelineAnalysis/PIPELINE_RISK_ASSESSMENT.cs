using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_RISK_ASSESSMENT : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String RISK_ASSESSMENT_IDValue; 
 public System.String RISK_ASSESSMENT_ID
        {  
            get  
            {  
                return this.RISK_ASSESSMENT_IDValue;  
            }  

          set { SetProperty(ref  RISK_ASSESSMENT_IDValue, value); }
        } 
private  System.DateTime? ASSESSMENT_DATEValue; 
 public System.DateTime? ASSESSMENT_DATE
        {  
            get  
            {  
                return this.ASSESSMENT_DATEValue;  
            }  

          set { SetProperty(ref  ASSESSMENT_DATEValue, value); }
        } 
private  System.Decimal RISK_SCOREValue; 
 public System.Decimal RISK_SCORE
        {  
            get  
            {  
                return this.RISK_SCOREValue;  
            }  

          set { SetProperty(ref  RISK_SCOREValue, value); }
        } 
private  System.String ASSESSMENT_METHODValue; 
 public System.String ASSESSMENT_METHOD
        {  
            get  
            {  
                return this.ASSESSMENT_METHODValue;  
            }  

          set { SetProperty(ref  ASSESSMENT_METHODValue, value); }
        } 
private  System.String RISK_FACTORSValue; 
 public System.String RISK_FACTORS
        {  
            get  
            {  
                return this.RISK_FACTORSValue;  
            }  

          set { SetProperty(ref  RISK_FACTORSValue, value); }
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

    public PIPELINE_RISK_ASSESSMENT () { }

  }
}


