using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data 
{
public partial class PIPELINE_TEST: Entity,IPPDMEntity

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


    public PIPELINE_TEST () { }

  }
}

