using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PROJECT: Entity,IPPDMEntity

{

private  System.String PROJECT_IDValue; 
 public System.String PROJECT_ID
        {  
            get  
            {  
                return this.PROJECT_IDValue;  
            }  

          set { SetProperty(ref  PROJECT_IDValue, value); }
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
private  System.DateTime? COMPLETE_DATEValue; 
 public System.DateTime? COMPLETE_DATE
        {  
            get  
            {  
                return this.COMPLETE_DATEValue;  
            }  

          set { SetProperty(ref  COMPLETE_DATEValue, value); }
        } 
private  System.String CONFIDENTIAL_INDValue; 
 public System.String CONFIDENTIAL_IND
        {  
            get  
            {  
                return this.CONFIDENTIAL_INDValue;  
            }  

          set { SetProperty(ref  CONFIDENTIAL_INDValue, value); }
        } 
private  System.DateTime? CONFIDENTIAL_RELEASE_DATEValue; 
 public System.DateTime? CONFIDENTIAL_RELEASE_DATE
        {  
            get  
            {  
                return this.CONFIDENTIAL_RELEASE_DATEValue;  
            }  

          set { SetProperty(ref  CONFIDENTIAL_RELEASE_DATEValue, value); }
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
private  System.String FIELD_STATION_INDValue; 
 public System.String FIELD_STATION_IND
        {  
            get  
            {  
                return this.FIELD_STATION_INDValue;  
            }  

          set { SetProperty(ref  FIELD_STATION_INDValue, value); }
        } 
private  System.String LAND_RIGHT_INDValue; 
 public System.String LAND_RIGHT_IND
        {  
            get  
            {  
                return this.LAND_RIGHT_INDValue;  
            }  

          set { SetProperty(ref  LAND_RIGHT_INDValue, value); }
        } 
private  System.String PDEN_INDValue; 
 public System.String PDEN_IND
        {  
            get  
            {  
                return this.PDEN_INDValue;  
            }  

          set { SetProperty(ref  PDEN_INDValue, value); }
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
private  System.String PROJECT_NAMEValue; 
 public System.String PROJECT_NAME
        {  
            get  
            {  
                return this.PROJECT_NAMEValue;  
            }  

          set { SetProperty(ref  PROJECT_NAMEValue, value); }
        } 
private  System.String PROJECT_NUMValue; 
 public System.String PROJECT_NUM
        {  
            get  
            {  
                return this.PROJECT_NUMValue;  
            }  

          set { SetProperty(ref  PROJECT_NUMValue, value); }
        } 
private  System.String PROJECT_TYPEValue; 
 public System.String PROJECT_TYPE
        {  
            get  
            {  
                return this.PROJECT_TYPEValue;  
            }  

          set { SetProperty(ref  PROJECT_TYPEValue, value); }
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
private  System.String SEIS_SET_INDValue; 
 public System.String SEIS_SET_IND
        {  
            get  
            {  
                return this.SEIS_SET_INDValue;  
            }  

          set { SetProperty(ref  SEIS_SET_INDValue, value); }
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
private  System.DateTime? START_DATEValue; 
 public System.DateTime? START_DATE
        {  
            get  
            {  
                return this.START_DATEValue;  
            }  

          set { SetProperty(ref  START_DATEValue, value); }
        } 
private  System.String STRAT_COLUMN_INDValue; 
 public System.String STRAT_COLUMN_IND
        {  
            get  
            {  
                return this.STRAT_COLUMN_INDValue;  
            }  

          set { SetProperty(ref  STRAT_COLUMN_INDValue, value); }
        } 
private  System.String STRAT_INTERPRETATION_INDValue; 
 public System.String STRAT_INTERPRETATION_IND
        {  
            get  
            {  
                return this.STRAT_INTERPRETATION_INDValue;  
            }  

          set { SetProperty(ref  STRAT_INTERPRETATION_INDValue, value); }
        } 
private  System.String WELL_INDValue; 
 public System.String WELL_IND
        {  
            get  
            {  
                return this.WELL_INDValue;  
            }  

          set { SetProperty(ref  WELL_INDValue, value); }
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


    public PROJECT () { }

  }
}

