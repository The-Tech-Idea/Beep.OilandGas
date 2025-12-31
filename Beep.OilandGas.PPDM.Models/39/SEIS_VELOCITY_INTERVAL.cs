using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_VELOCITY_INTERVAL: Entity,IPPDMEntity

{

private  System.String INTERVAL_IDValue; 
 public System.String INTERVAL_ID
        {  
            get  
            {  
                return this.INTERVAL_IDValue;  
            }  

          set { SetProperty(ref  INTERVAL_IDValue, value); }
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
private  System.Decimal BASE_DEPTHValue; 
 public System.Decimal BASE_DEPTH
        {  
            get  
            {  
                return this.BASE_DEPTHValue;  
            }  

          set { SetProperty(ref  BASE_DEPTHValue, value); }
        } 
private  System.String BASE_DEPTH_OUOMValue; 
 public System.String BASE_DEPTH_OUOM
        {  
            get  
            {  
                return this.BASE_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  BASE_DEPTH_OUOMValue, value); }
        } 
private  System.String BASE_STRAT_UNIT_IDValue; 
 public System.String BASE_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.BASE_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  BASE_STRAT_UNIT_IDValue, value); }
        } 
private  System.String COMPUTE_METHODValue; 
 public System.String COMPUTE_METHOD
        {  
            get  
            {  
                return this.COMPUTE_METHODValue;  
            }  

          set { SetProperty(ref  COMPUTE_METHODValue, value); }
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
private  System.DateTime? LAST_UPDATEValue; 
 public System.DateTime? LAST_UPDATE
        {  
            get  
            {  
                return this.LAST_UPDATEValue;  
            }  

          set { SetProperty(ref  LAST_UPDATEValue, value); }
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
private  System.Decimal REPORTED_TVDValue; 
 public System.Decimal REPORTED_TVD
        {  
            get  
            {  
                return this.REPORTED_TVDValue;  
            }  

          set { SetProperty(ref  REPORTED_TVDValue, value); }
        } 
private  System.String REPORTED_TVD_OUOMValue; 
 public System.String REPORTED_TVD_OUOM
        {  
            get  
            {  
                return this.REPORTED_TVD_OUOMValue;  
            }  

          set { SetProperty(ref  REPORTED_TVD_OUOMValue, value); }
        } 
private  System.String SEIS_SET_IDValue; 
 public System.String SEIS_SET_ID
        {  
            get  
            {  
                return this.SEIS_SET_IDValue;  
            }  

          set { SetProperty(ref  SEIS_SET_IDValue, value); }
        } 
private  System.String SEIS_SET_SUBTYPEValue; 
 public System.String SEIS_SET_SUBTYPE
        {  
            get  
            {  
                return this.SEIS_SET_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SEIS_SET_SUBTYPEValue, value); }
        } 
private  System.Decimal SEIS_TIME_DATUMValue; 
 public System.Decimal SEIS_TIME_DATUM
        {  
            get  
            {  
                return this.SEIS_TIME_DATUMValue;  
            }  

          set { SetProperty(ref  SEIS_TIME_DATUMValue, value); }
        } 
private  System.String SEIS_TIME_DATUM_OUOMValue; 
 public System.String SEIS_TIME_DATUM_OUOM
        {  
            get  
            {  
                return this.SEIS_TIME_DATUM_OUOMValue;  
            }  

          set { SetProperty(ref  SEIS_TIME_DATUM_OUOMValue, value); }
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
private  System.String STRAT_NAME_SET_IDValue; 
 public System.String STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  STRAT_NAME_SET_IDValue, value); }
        } 
private  System.Decimal TOP_DEPTHValue; 
 public System.Decimal TOP_DEPTH
        {  
            get  
            {  
                return this.TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  TOP_DEPTHValue, value); }
        } 
private  System.String TOP_DEPTH_OUOMValue; 
 public System.String TOP_DEPTH_OUOM
        {  
            get  
            {  
                return this.TOP_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  TOP_DEPTH_OUOMValue, value); }
        } 
private  System.String TOP_STRAT_UNIT_IDValue; 
 public System.String TOP_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.TOP_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  TOP_STRAT_UNIT_IDValue, value); }
        } 
private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
        } 
private  System.String VELOCITY_QUALITYValue; 
 public System.String VELOCITY_QUALITY
        {  
            get  
            {  
                return this.VELOCITY_QUALITYValue;  
            }  

          set { SetProperty(ref  VELOCITY_QUALITYValue, value); }
        } 
private  System.String VELOCITY_TYPEValue; 
 public System.String VELOCITY_TYPE
        {  
            get  
            {  
                return this.VELOCITY_TYPEValue;  
            }  

          set { SetProperty(ref  VELOCITY_TYPEValue, value); }
        } 
private  System.Decimal VELOCITY_VALUEValue; 
 public System.Decimal VELOCITY_VALUE
        {  
            get  
            {  
                return this.VELOCITY_VALUEValue;  
            }  

          set { SetProperty(ref  VELOCITY_VALUEValue, value); }
        } 
private  System.String VELOCITY_VALUE_OUOMValue; 
 public System.String VELOCITY_VALUE_OUOM
        {  
            get  
            {  
                return this.VELOCITY_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  VELOCITY_VALUE_OUOMValue, value); }
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


    public SEIS_VELOCITY_INTERVAL () { }

  }
}

