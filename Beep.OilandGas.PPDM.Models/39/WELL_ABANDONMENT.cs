using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_ABANDONMENT: Entity,IPPDMEntity

{

private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
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
private  System.String ABANDONMENT_IDValue; 
 public System.String ABANDONMENT_ID
        {  
            get  
            {  
                return this.ABANDONMENT_IDValue;  
            }  

          set { SetProperty(ref  ABANDONMENT_IDValue, value); }
        } 
private  System.String FIELD_IDValue; 
 public System.String FIELD_ID
        {  
            get  
            {  
                return this.FIELD_IDValue;  
            }  

          set { SetProperty(ref  FIELD_IDValue, value); }
        } 
private  System.DateTime? ABANDONMENT_DATEValue; 
 public System.DateTime? ABANDONMENT_DATE
        {  
            get  
            {  
                return this.ABANDONMENT_DATEValue;  
            }  

          set { SetProperty(ref  ABANDONMENT_DATEValue, value); }
        } 
private  System.String ABANDONMENT_TYPEValue; 
 public System.String ABANDONMENT_TYPE
        {  
            get  
            {  
                return this.ABANDONMENT_TYPEValue;  
            }  

          set { SetProperty(ref  ABANDONMENT_TYPEValue, value); }
        } 
private  System.String ABANDONMENT_REASONValue; 
 public System.String ABANDONMENT_REASON
        {  
            get  
            {  
                return this.ABANDONMENT_REASONValue;  
            }  

          set { SetProperty(ref  ABANDONMENT_REASONValue, value); }
        } 
private  System.DateTime? PLUG_DATEValue; 
 public System.DateTime? PLUG_DATE
        {  
            get  
            {  
                return this.PLUG_DATEValue;  
            }  

          set { SetProperty(ref  PLUG_DATEValue, value); }
        } 
private  System.String STATUSValue; 
 public System.String STATUS
        {  
            get  
            {  
                return this.STATUSValue;  
            }  

          set { SetProperty(ref  STATUSValue, value); }
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
private  System.DateTime? ROW_EFFECTIVE_DATEValue; 
 public System.DateTime? ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }        } 
private  System.DateTime? ROW_EXPIRY_DATEValue; 
 public System.DateTime? ROW_EXPIRY_DATE
        {  
            get  
            {  
                return this.ROW_EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EXPIRY_DATEValue, value); }        } 
private  System.String ROW_QUALITYValue; 
 public System.String ROW_QUALITY
        {  
            get  
            {  
                return this.ROW_QUALITYValue;  
            }  

          set { SetProperty(ref  ROW_QUALITYValue, value); }
        } 

    public WELL_ABANDONMENT () { }
}
}
