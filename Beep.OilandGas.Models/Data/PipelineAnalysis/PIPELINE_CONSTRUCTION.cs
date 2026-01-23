using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_CONSTRUCTION : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String CONSTRUCTION_PHASE_NOValue; 
 public System.String CONSTRUCTION_PHASE_NO
        {  
            get  
            {  
                return this.CONSTRUCTION_PHASE_NOValue;  
            }  

          set { SetProperty(ref  CONSTRUCTION_PHASE_NOValue, value); }
        } 
private  System.String PHASE_TYPEValue; 
 public System.String PHASE_TYPE
        {  
            get  
            {  
                return this.PHASE_TYPEValue;  
            }  

          set { SetProperty(ref  PHASE_TYPEValue, value); }
        } 
private  System.String PHASE_NAMEValue; 
 public System.String PHASE_NAME
        {  
            get  
            {  
                return this.PHASE_NAMEValue;  
            }  

          set { SetProperty(ref  PHASE_NAMEValue, value); }
        } 
private  System.DateTime? START_DATEValue; 
 public System.DateTime? START_DATE
        {  
            get  
            {  
                return this.START_DATEValue;  
            }  

          set { SetProperty(ref  START_DATEValue, value); }
        } 
private  System.DateTime? END_DATEValue; 
 public System.DateTime? END_DATE
        {  
            get  
            {  
                return this.END_DATEValue;  
            }  

          set { SetProperty(ref  END_DATEValue, value); }
        } 
private  System.String CONTRACTORValue; 
 public System.String CONTRACTOR
        {  
            get  
            {  
                return this.CONTRACTORValue;  
            }  

          set { SetProperty(ref  CONTRACTORValue, value); }
        } 
private  System.String INSPECTION_RECORDSValue; 
 public System.String INSPECTION_RECORDS
        {  
            get  
            {  
                return this.INSPECTION_RECORDSValue;  
            }  

          set { SetProperty(ref  INSPECTION_RECORDSValue, value); }
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

    public PIPELINE_CONSTRUCTION () { }

  }
}


