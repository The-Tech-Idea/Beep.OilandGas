using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_NODE_STRAT_UNIT: Entity,IPPDMEntity

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
private  System.String STRAT_NAME_SET_IDValue; 
 public System.String STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String STRAT_UNIT_IDValue; 
 public System.String STRAT_UNIT_ID
        {  
            get  
            {  
                return this.STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  STRAT_UNIT_IDValue, value); }
        } 
private  System.String INTERP_IDValue; 
 public System.String INTERP_ID
        {  
            get  
            {  
                return this.INTERP_IDValue;  
            }  

          set { SetProperty(ref  INTERP_IDValue, value); }
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
private  System.Decimal BASE_OFFSET_TVDValue; 
 public System.Decimal BASE_OFFSET_TVD
        {  
            get  
            {  
                return this.BASE_OFFSET_TVDValue;  
            }  

          set { SetProperty(ref  BASE_OFFSET_TVDValue, value); }
        } 
private  System.String BASE_OFFSET_TVD_OUOMValue; 
 public System.String BASE_OFFSET_TVD_OUOM
        {  
            get  
            {  
                return this.BASE_OFFSET_TVD_OUOMValue;  
            }  

          set { SetProperty(ref  BASE_OFFSET_TVD_OUOMValue, value); }
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
private  System.String INTERPRETER_BA_IDValue; 
 public System.String INTERPRETER_BA_ID
        {  
            get  
            {  
                return this.INTERPRETER_BA_IDValue;  
            }  

          set { SetProperty(ref  INTERPRETER_BA_IDValue, value); }
        } 
private  System.String LOCATION_TYPEValue; 
 public System.String LOCATION_TYPE
        {  
            get  
            {  
                return this.LOCATION_TYPEValue;  
            }  

          set { SetProperty(ref  LOCATION_TYPEValue, value); }
        } 
private  System.String NODE_IDValue; 
 public System.String NODE_ID
        {  
            get  
            {  
                return this.NODE_IDValue;  
            }  

          set { SetProperty(ref  NODE_IDValue, value); }
        } 
private  System.String NODE_POSITIONValue; 
 public System.String NODE_POSITION
        {  
            get  
            {  
                return this.NODE_POSITIONValue;  
            }  

          set { SetProperty(ref  NODE_POSITIONValue, value); }
        } 
private  System.DateTime? PICK_DATEValue; 
 public System.DateTime? PICK_DATE
        {  
            get  
            {  
                return this.PICK_DATEValue;  
            }  

          set { SetProperty(ref  PICK_DATEValue, value); }
        } 
private  System.Decimal PICK_DEPTHValue; 
 public System.Decimal PICK_DEPTH
        {  
            get  
            {  
                return this.PICK_DEPTHValue;  
            }  

          set { SetProperty(ref  PICK_DEPTHValue, value); }
        } 
private  System.String PICK_DEPTH_OUOMValue; 
 public System.String PICK_DEPTH_OUOM
        {  
            get  
            {  
                return this.PICK_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  PICK_DEPTH_OUOMValue, value); }
        } 
private  System.String PICK_LOCATIONValue; 
 public System.String PICK_LOCATION
        {  
            get  
            {  
                return this.PICK_LOCATIONValue;  
            }  

          set { SetProperty(ref  PICK_LOCATIONValue, value); }
        } 
private  System.String PICK_METHODValue; 
 public System.String PICK_METHOD
        {  
            get  
            {  
                return this.PICK_METHODValue;  
            }  

          set { SetProperty(ref  PICK_METHODValue, value); }
        } 
private  System.String PICK_METHOD_DESCValue; 
 public System.String PICK_METHOD_DESC
        {  
            get  
            {  
                return this.PICK_METHOD_DESCValue;  
            }  

          set { SetProperty(ref  PICK_METHOD_DESCValue, value); }
        } 
private  System.String PICK_QUALITYValue; 
 public System.String PICK_QUALITY
        {  
            get  
            {  
                return this.PICK_QUALITYValue;  
            }  

          set { SetProperty(ref  PICK_QUALITYValue, value); }
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
private  System.String PREFERRED_INDValue; 
 public System.String PREFERRED_IND
        {  
            get  
            {  
                return this.PREFERRED_INDValue;  
            }  

          set { SetProperty(ref  PREFERRED_INDValue, value); }
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
private  System.Decimal TOP_OFFSET_TVDValue; 
 public System.Decimal TOP_OFFSET_TVD
        {  
            get  
            {  
                return this.TOP_OFFSET_TVDValue;  
            }  

          set { SetProperty(ref  TOP_OFFSET_TVDValue, value); }
        } 
private  System.String TOP_OFFSET_TVD_OUOMValue; 
 public System.String TOP_OFFSET_TVD_OUOM
        {  
            get  
            {  
                return this.TOP_OFFSET_TVD_OUOMValue;  
            }  

          set { SetProperty(ref  TOP_OFFSET_TVD_OUOMValue, value); }
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


    public WELL_NODE_STRAT_UNIT () { }

  }
}

