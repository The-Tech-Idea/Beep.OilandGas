using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class STRAT_COL_UNIT_AGE: Entity,IPPDMEntity

{

private  System.String STRAT_COLUMN_IDValue; 
 public System.String STRAT_COLUMN_ID
        {  
            get  
            {  
                return this.STRAT_COLUMN_IDValue;  
            }  

          set { SetProperty(ref  STRAT_COLUMN_IDValue, value); }
        } 
private  System.String STRAT_COLUMN_SOURCEValue; 
 public System.String STRAT_COLUMN_SOURCE
        {  
            get  
            {  
                return this.STRAT_COLUMN_SOURCEValue;  
            }  

          set { SetProperty(ref  STRAT_COLUMN_SOURCEValue, value); }
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
private  System.String AGE_SOURCEValue; 
 public System.String AGE_SOURCE
        {  
            get  
            {  
                return this.AGE_SOURCEValue;  
            }  

          set { SetProperty(ref  AGE_SOURCEValue, value); }
        } 
private  System.String AGE_IDValue; 
 public System.String AGE_ID
        {  
            get  
            {  
                return this.AGE_IDValue;  
            }  

          set { SetProperty(ref  AGE_IDValue, value); }
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
private  System.Decimal AVERAGE_AGEValue; 
 public System.Decimal AVERAGE_AGE
        {  
            get  
            {  
                return this.AVERAGE_AGEValue;  
            }  

          set { SetProperty(ref  AVERAGE_AGEValue, value); }
        } 
private  System.Decimal AVERAGE_AGE_ERROR_MINUSValue; 
 public System.Decimal AVERAGE_AGE_ERROR_MINUS
        {  
            get  
            {  
                return this.AVERAGE_AGE_ERROR_MINUSValue;  
            }  

          set { SetProperty(ref  AVERAGE_AGE_ERROR_MINUSValue, value); }
        } 
private  System.Decimal AVERAGE_AGE_ERROR_PLUSValue; 
 public System.Decimal AVERAGE_AGE_ERROR_PLUS
        {  
            get  
            {  
                return this.AVERAGE_AGE_ERROR_PLUSValue;  
            }  

          set { SetProperty(ref  AVERAGE_AGE_ERROR_PLUSValue, value); }
        } 
private  System.String AVERAGE_REL_STRAT_NAME_SETValue; 
 public System.String AVERAGE_REL_STRAT_NAME_SET
        {  
            get  
            {  
                return this.AVERAGE_REL_STRAT_NAME_SETValue;  
            }  

          set { SetProperty(ref  AVERAGE_REL_STRAT_NAME_SETValue, value); }
        } 
private  System.String AVERAGE_REL_STRAT_UNIT_IDValue; 
 public System.String AVERAGE_REL_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.AVERAGE_REL_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  AVERAGE_REL_STRAT_UNIT_IDValue, value); }
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
private  System.Decimal LOWER_MAX_AGEValue; 
 public System.Decimal LOWER_MAX_AGE
        {  
            get  
            {  
                return this.LOWER_MAX_AGEValue;  
            }  

          set { SetProperty(ref  LOWER_MAX_AGEValue, value); }
        } 
private  System.Decimal LOWER_MAX_AGE_ERROR_MINUSValue; 
 public System.Decimal LOWER_MAX_AGE_ERROR_MINUS
        {  
            get  
            {  
                return this.LOWER_MAX_AGE_ERROR_MINUSValue;  
            }  

          set { SetProperty(ref  LOWER_MAX_AGE_ERROR_MINUSValue, value); }
        } 
private  System.Decimal LOWER_MAX_AGE_ERROR_PLUSValue; 
 public System.Decimal LOWER_MAX_AGE_ERROR_PLUS
        {  
            get  
            {  
                return this.LOWER_MAX_AGE_ERROR_PLUSValue;  
            }  

          set { SetProperty(ref  LOWER_MAX_AGE_ERROR_PLUSValue, value); }
        } 
private  System.Decimal LOWER_MIN_AGEValue; 
 public System.Decimal LOWER_MIN_AGE
        {  
            get  
            {  
                return this.LOWER_MIN_AGEValue;  
            }  

          set { SetProperty(ref  LOWER_MIN_AGEValue, value); }
        } 
private  System.Decimal LOWER_MIN_AGE_ERROR_MINUSValue; 
 public System.Decimal LOWER_MIN_AGE_ERROR_MINUS
        {  
            get  
            {  
                return this.LOWER_MIN_AGE_ERROR_MINUSValue;  
            }  

          set { SetProperty(ref  LOWER_MIN_AGE_ERROR_MINUSValue, value); }
        } 
private  System.Decimal LOWER_MIN_AGE_ERROR_PLUSValue; 
 public System.Decimal LOWER_MIN_AGE_ERROR_PLUS
        {  
            get  
            {  
                return this.LOWER_MIN_AGE_ERROR_PLUSValue;  
            }  

          set { SetProperty(ref  LOWER_MIN_AGE_ERROR_PLUSValue, value); }
        } 
private  System.String LOWER_REL_STRAT_NAME_SETValue; 
 public System.String LOWER_REL_STRAT_NAME_SET
        {  
            get  
            {  
                return this.LOWER_REL_STRAT_NAME_SETValue;  
            }  

          set { SetProperty(ref  LOWER_REL_STRAT_NAME_SETValue, value); }
        } 
private  System.String LOWER_REL_STRAT_UNIT_IDValue; 
 public System.String LOWER_REL_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.LOWER_REL_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  LOWER_REL_STRAT_UNIT_IDValue, value); }
        } 
private  System.Decimal MAX_AGEValue; 
 public System.Decimal MAX_AGE
        {  
            get  
            {  
                return this.MAX_AGEValue;  
            }  

          set { SetProperty(ref  MAX_AGEValue, value); }
        } 
private  System.Decimal MAX_AGE_ERROR_MINUSValue; 
 public System.Decimal MAX_AGE_ERROR_MINUS
        {  
            get  
            {  
                return this.MAX_AGE_ERROR_MINUSValue;  
            }  

          set { SetProperty(ref  MAX_AGE_ERROR_MINUSValue, value); }
        } 
private  System.Decimal MAX_AGE_ERROR_PLUSValue; 
 public System.Decimal MAX_AGE_ERROR_PLUS
        {  
            get  
            {  
                return this.MAX_AGE_ERROR_PLUSValue;  
            }  

          set { SetProperty(ref  MAX_AGE_ERROR_PLUSValue, value); }
        } 
private  System.Decimal MIN_AGEValue; 
 public System.Decimal MIN_AGE
        {  
            get  
            {  
                return this.MIN_AGEValue;  
            }  

          set { SetProperty(ref  MIN_AGEValue, value); }
        } 
private  System.Decimal MIN_AGE_ERROR_MINUSValue; 
 public System.Decimal MIN_AGE_ERROR_MINUS
        {  
            get  
            {  
                return this.MIN_AGE_ERROR_MINUSValue;  
            }  

          set { SetProperty(ref  MIN_AGE_ERROR_MINUSValue, value); }
        } 
private  System.Decimal MIN_AGE_ERROR_PLUSValue; 
 public System.Decimal MIN_AGE_ERROR_PLUS
        {  
            get  
            {  
                return this.MIN_AGE_ERROR_PLUSValue;  
            }  

          set { SetProperty(ref  MIN_AGE_ERROR_PLUSValue, value); }
        } 
private  System.String POST_QUALIFIERValue; 
 public System.String POST_QUALIFIER
        {  
            get  
            {  
                return this.POST_QUALIFIERValue;  
            }  

          set { SetProperty(ref  POST_QUALIFIERValue, value); }
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
private  System.String PRE_QUALIFIERValue; 
 public System.String PRE_QUALIFIER
        {  
            get  
            {  
                return this.PRE_QUALIFIERValue;  
            }  

          set { SetProperty(ref  PRE_QUALIFIERValue, value); }
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
private  System.String SOURCE_DOCUMENT_IDValue; 
 public System.String SOURCE_DOCUMENT_ID
        {  
            get  
            {  
                return this.SOURCE_DOCUMENT_IDValue;  
            }  

          set { SetProperty(ref  SOURCE_DOCUMENT_IDValue, value); }
        } 
private  System.String STRAT_AGE_METHODValue; 
 public System.String STRAT_AGE_METHOD
        {  
            get  
            {  
                return this.STRAT_AGE_METHODValue;  
            }  

          set { SetProperty(ref  STRAT_AGE_METHODValue, value); }
        } 
private  System.Decimal UPPER_MAX_AGEValue; 
 public System.Decimal UPPER_MAX_AGE
        {  
            get  
            {  
                return this.UPPER_MAX_AGEValue;  
            }  

          set { SetProperty(ref  UPPER_MAX_AGEValue, value); }
        } 
private  System.Decimal UPPER_MAX_AGE_ERROR_MINUSValue; 
 public System.Decimal UPPER_MAX_AGE_ERROR_MINUS
        {  
            get  
            {  
                return this.UPPER_MAX_AGE_ERROR_MINUSValue;  
            }  

          set { SetProperty(ref  UPPER_MAX_AGE_ERROR_MINUSValue, value); }
        } 
private  System.Decimal UPPER_MAX_AGE_ERROR_PLUSValue; 
 public System.Decimal UPPER_MAX_AGE_ERROR_PLUS
        {  
            get  
            {  
                return this.UPPER_MAX_AGE_ERROR_PLUSValue;  
            }  

          set { SetProperty(ref  UPPER_MAX_AGE_ERROR_PLUSValue, value); }
        } 
private  System.Decimal UPPER_MIN_AGEValue; 
 public System.Decimal UPPER_MIN_AGE
        {  
            get  
            {  
                return this.UPPER_MIN_AGEValue;  
            }  

          set { SetProperty(ref  UPPER_MIN_AGEValue, value); }
        } 
private  System.Decimal UPPER_MIN_AGE_ERROR_MINUSValue; 
 public System.Decimal UPPER_MIN_AGE_ERROR_MINUS
        {  
            get  
            {  
                return this.UPPER_MIN_AGE_ERROR_MINUSValue;  
            }  

          set { SetProperty(ref  UPPER_MIN_AGE_ERROR_MINUSValue, value); }
        } 
private  System.Decimal UPPER_MIN_AGE_ERROR_PLUSValue; 
 public System.Decimal UPPER_MIN_AGE_ERROR_PLUS
        {  
            get  
            {  
                return this.UPPER_MIN_AGE_ERROR_PLUSValue;  
            }  

          set { SetProperty(ref  UPPER_MIN_AGE_ERROR_PLUSValue, value); }
        } 
private  System.String UPPER_REL_STRAT_NAME_SETValue; 
 public System.String UPPER_REL_STRAT_NAME_SET
        {  
            get  
            {  
                return this.UPPER_REL_STRAT_NAME_SETValue;  
            }  

          set { SetProperty(ref  UPPER_REL_STRAT_NAME_SETValue, value); }
        } 
private  System.String UPPER_REL_STRAT_UNIT_IDValue; 
 public System.String UPPER_REL_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.UPPER_REL_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  UPPER_REL_STRAT_UNIT_IDValue, value); }
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


    public STRAT_COL_UNIT_AGE () { }

  }
}

