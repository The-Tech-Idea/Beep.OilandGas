using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_DRILL_PERIOD_CREW: Entity,IPPDMEntity

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
private  System.Decimal PERIOD_OBS_NOValue; 
 public System.Decimal PERIOD_OBS_NO
        {  
            get  
            {  
                return this.PERIOD_OBS_NOValue;  
            }  

          set { SetProperty(ref  PERIOD_OBS_NOValue, value); }
        } 
private  System.String CREW_COMPANY_BA_IDValue; 
 public System.String CREW_COMPANY_BA_ID
        {  
            get  
            {  
                return this.CREW_COMPANY_BA_IDValue;  
            }  

          set { SetProperty(ref  CREW_COMPANY_BA_IDValue, value); }
        } 
private  System.Decimal DETAIL_OBS_NOValue; 
 public System.Decimal DETAIL_OBS_NO
        {  
            get  
            {  
                return this.DETAIL_OBS_NOValue;  
            }  

          set { SetProperty(ref  DETAIL_OBS_NOValue, value); }
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
private  System.Decimal BOOKED_TIME_ELAPSEDValue; 
 public System.Decimal BOOKED_TIME_ELAPSED
        {  
            get  
            {  
                return this.BOOKED_TIME_ELAPSEDValue;  
            }  

          set { SetProperty(ref  BOOKED_TIME_ELAPSEDValue, value); }
        } 
private  System.String BOOKED_TIME_ELAPSED_UOMValue; 
 public System.String BOOKED_TIME_ELAPSED_UOM
        {  
            get  
            {  
                return this.BOOKED_TIME_ELAPSED_UOMValue;  
            }  

          set { SetProperty(ref  BOOKED_TIME_ELAPSED_UOMValue, value); }
        } 
private  System.String CREW_MEMBER_BA_IDValue; 
 public System.String CREW_MEMBER_BA_ID
        {  
            get  
            {  
                return this.CREW_MEMBER_BA_IDValue;  
            }  

          set { SetProperty(ref  CREW_MEMBER_BA_IDValue, value); }
        } 
private  System.String CREW_MEMBER_NAMEValue; 
 public System.String CREW_MEMBER_NAME
        {  
            get  
            {  
                return this.CREW_MEMBER_NAMEValue;  
            }  

          set { SetProperty(ref  CREW_MEMBER_NAMEValue, value); }
        } 
private  System.String CREW_MEMBER_NUMValue; 
 public System.String CREW_MEMBER_NUM
        {  
            get  
            {  
                return this.CREW_MEMBER_NUMValue;  
            }  

          set { SetProperty(ref  CREW_MEMBER_NUMValue, value); }
        } 
private  System.String CREW_MEMBER_RECORD_INDValue; 
 public System.String CREW_MEMBER_RECORD_IND
        {  
            get  
            {  
                return this.CREW_MEMBER_RECORD_INDValue;  
            }  

          set { SetProperty(ref  CREW_MEMBER_RECORD_INDValue, value); }
        } 
private  System.String CREW_POSITIONValue; 
 public System.String CREW_POSITION
        {  
            get  
            {  
                return this.CREW_POSITIONValue;  
            }  

          set { SetProperty(ref  CREW_POSITIONValue, value); }
        } 
private  System.String CREW_RECORD_INDValue; 
 public System.String CREW_RECORD_IND
        {  
            get  
            {  
                return this.CREW_RECORD_INDValue;  
            }  

          set { SetProperty(ref  CREW_RECORD_INDValue, value); }
        } 
private  System.String CREW_REFERENCE_NUMValue; 
 public System.String CREW_REFERENCE_NUM
        {  
            get  
            {  
                return this.CREW_REFERENCE_NUMValue;  
            }  

          set { SetProperty(ref  CREW_REFERENCE_NUMValue, value); }
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
private  System.String INJURY_INDValue; 
 public System.String INJURY_IND
        {  
            get  
            {  
                return this.INJURY_INDValue;  
            }  

          set { SetProperty(ref  INJURY_INDValue, value); }
        } 
private  System.String INJURY_INITIAL_INDValue; 
 public System.String INJURY_INITIAL_IND
        {  
            get  
            {  
                return this.INJURY_INITIAL_INDValue;  
            }  

          set { SetProperty(ref  INJURY_INITIAL_INDValue, value); }
        } 
private  System.String NO_INJURY_INDValue; 
 public System.String NO_INJURY_IND
        {  
            get  
            {  
                return this.NO_INJURY_INDValue;  
            }  

          set { SetProperty(ref  NO_INJURY_INDValue, value); }
        } 
private  System.Decimal OVERHEAD_COSTValue; 
 public System.Decimal OVERHEAD_COST
        {  
            get  
            {  
                return this.OVERHEAD_COSTValue;  
            }  

          set { SetProperty(ref  OVERHEAD_COSTValue, value); }
        } 
private  System.String OVERHEAD_COST_UOMValue; 
 public System.String OVERHEAD_COST_UOM
        {  
            get  
            {  
                return this.OVERHEAD_COST_UOMValue;  
            }  

          set { SetProperty(ref  OVERHEAD_COST_UOMValue, value); }
        } 
private  System.String OVERHEAD_TYPEValue; 
 public System.String OVERHEAD_TYPE
        {  
            get  
            {  
                return this.OVERHEAD_TYPEValue;  
            }  

          set { SetProperty(ref  OVERHEAD_TYPEValue, value); }
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
private  System.String SF_SUBTYPEValue; 
 public System.String SF_SUBTYPE
        {  
            get  
            {  
                return this.SF_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SF_SUBTYPEValue, value); }
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
private  System.String SUBSISTANCE_INDValue; 
 public System.String SUBSISTANCE_IND
        {  
            get  
            {  
                return this.SUBSISTANCE_INDValue;  
            }  

          set { SetProperty(ref  SUBSISTANCE_INDValue, value); }
        } 
private  System.String SUPPORT_FACILITY_IDValue; 
 public System.String SUPPORT_FACILITY_ID
        {  
            get  
            {  
                return this.SUPPORT_FACILITY_IDValue;  
            }  

          set { SetProperty(ref  SUPPORT_FACILITY_IDValue, value); }
        } 
private  System.Decimal TOTAL_CREW_COUNTValue; 
 public System.Decimal TOTAL_CREW_COUNT
        {  
            get  
            {  
                return this.TOTAL_CREW_COUNTValue;  
            }  

          set { SetProperty(ref  TOTAL_CREW_COUNTValue, value); }
        } 
private  System.Decimal TOTAL_INJURY_COUNTValue; 
 public System.Decimal TOTAL_INJURY_COUNT
        {  
            get  
            {  
                return this.TOTAL_INJURY_COUNTValue;  
            }  

          set { SetProperty(ref  TOTAL_INJURY_COUNTValue, value); }
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


    public WELL_DRILL_PERIOD_CREW () { }

  }
}

