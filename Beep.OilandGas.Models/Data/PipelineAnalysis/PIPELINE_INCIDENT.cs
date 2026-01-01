using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_INCIDENT: Entity,IPPDMEntity

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
private  System.String INCIDENT_IDValue; 
 public System.String INCIDENT_ID
        {  
            get  
            {  
                return this.INCIDENT_IDValue;  
            }  

          set { SetProperty(ref  INCIDENT_IDValue, value); }
        } 
private  System.DateTime? INCIDENT_DATEValue; 
 public System.DateTime? INCIDENT_DATE
        {  
            get  
            {  
                return this.INCIDENT_DATEValue;  
            }  

          set { SetProperty(ref  INCIDENT_DATEValue, value); }
        } 
private  System.String INCIDENT_TYPEValue; 
 public System.String INCIDENT_TYPE
        {  
            get  
            {  
                return this.INCIDENT_TYPEValue;  
            }  

          set { SetProperty(ref  INCIDENT_TYPEValue, value); }
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
private  System.String CAUSEValue; 
 public System.String CAUSE
        {  
            get  
            {  
                return this.CAUSEValue;  
            }  

          set { SetProperty(ref  CAUSEValue, value); }
        } 
private  System.Decimal VOLUME_RELEASEDValue; 
 public System.Decimal VOLUME_RELEASED
        {  
            get  
            {  
                return this.VOLUME_RELEASEDValue;  
            }  

          set { SetProperty(ref  VOLUME_RELEASEDValue, value); }
        } 
private  System.String VOLUME_RELEASED_OUOMValue; 
 public System.String VOLUME_RELEASED_OUOM
        {  
            get  
            {  
                return this.VOLUME_RELEASED_OUOMValue;  
            }  

          set { SetProperty(ref  VOLUME_RELEASED_OUOMValue, value); }
        } 
private  System.String ENVIRONMENTAL_IMPACTValue; 
 public System.String ENVIRONMENTAL_IMPACT
        {  
            get  
            {  
                return this.ENVIRONMENTAL_IMPACTValue;  
            }  

          set { SetProperty(ref  ENVIRONMENTAL_IMPACTValue, value); }
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


    public PIPELINE_INCIDENT () { }

  }
}

