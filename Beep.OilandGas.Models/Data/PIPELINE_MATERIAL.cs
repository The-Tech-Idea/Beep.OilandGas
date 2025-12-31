using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data 
{
public partial class PIPELINE_MATERIAL: Entity,IPPDMEntity

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
private  System.String MATERIAL_SEQ_NOValue; 
 public System.String MATERIAL_SEQ_NO
        {  
            get  
            {  
                return this.MATERIAL_SEQ_NOValue;  
            }  

          set { SetProperty(ref  MATERIAL_SEQ_NOValue, value); }
        } 
private  System.String MATERIAL_TYPEValue; 
 public System.String MATERIAL_TYPE
        {  
            get  
            {  
                return this.MATERIAL_TYPEValue;  
            }  

          set { SetProperty(ref  MATERIAL_TYPEValue, value); }
        } 
private  System.String MATERIAL_GRADEValue; 
 public System.String MATERIAL_GRADE
        {  
            get  
            {  
                return this.MATERIAL_GRADEValue;  
            }  

          set { SetProperty(ref  MATERIAL_GRADEValue, value); }
        } 
private  System.String MATERIAL_SPECIFICATIONValue; 
 public System.String MATERIAL_SPECIFICATION
        {  
            get  
            {  
                return this.MATERIAL_SPECIFICATIONValue;  
            }  

          set { SetProperty(ref  MATERIAL_SPECIFICATIONValue, value); }
        } 
private  System.String MATERIAL_STANDARDValue; 
 public System.String MATERIAL_STANDARD
        {  
            get  
            {  
                return this.MATERIAL_STANDARDValue;  
            }  

          set { SetProperty(ref  MATERIAL_STANDARDValue, value); }
        } 
private  System.String MANUFACTURERValue; 
 public System.String MANUFACTURER
        {  
            get  
            {  
                return this.MANUFACTURERValue;  
            }  

          set { SetProperty(ref  MANUFACTURERValue, value); }
        } 
private  System.String HEAT_NUMBERValue; 
 public System.String HEAT_NUMBER
        {  
            get  
            {  
                return this.HEAT_NUMBERValue;  
            }  

          set { SetProperty(ref  HEAT_NUMBERValue, value); }
        } 
private  System.String BATCH_NUMBERValue; 
 public System.String BATCH_NUMBER
        {  
            get  
            {  
                return this.BATCH_NUMBERValue;  
            }  

          set { SetProperty(ref  BATCH_NUMBERValue, value); }
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


    public PIPELINE_MATERIAL () { }

  }
}

