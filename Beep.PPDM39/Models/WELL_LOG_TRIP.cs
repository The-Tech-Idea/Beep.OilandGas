using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class WELL_LOG_TRIP: Entity

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
private  System.String JOB_IDValue; 
 public System.String JOB_ID
        {  
            get  
            {  
                return this.JOB_IDValue;  
            }  

          set { SetProperty(ref  JOB_IDValue, value); }
        } 
private  System.Decimal TRIP_OBS_NOValue; 
 public System.Decimal TRIP_OBS_NO
        {  
            get  
            {  
                return this.TRIP_OBS_NOValue;  
            }  

          set { SetProperty(ref  TRIP_OBS_NOValue, value); }
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
private  System.DateTime EFFECTIVE_DATEValue; 
 public System.DateTime EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime EXPIRY_DATEValue; 
 public System.DateTime EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.String LOGGING_SERVICE_TYPEValue; 
 public System.String LOGGING_SERVICE_TYPE
        {  
            get  
            {  
                return this.LOGGING_SERVICE_TYPEValue;  
            }  

          set { SetProperty(ref  LOGGING_SERVICE_TYPEValue, value); }
        } 
private  System.Decimal MAX_DEPTHValue; 
 public System.Decimal MAX_DEPTH
        {  
            get  
            {  
                return this.MAX_DEPTHValue;  
            }  

          set { SetProperty(ref  MAX_DEPTHValue, value); }
        } 
private  System.String MAX_DEPTH_OUOMValue; 
 public System.String MAX_DEPTH_OUOM
        {  
            get  
            {  
                return this.MAX_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  MAX_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal MAX_TEMPERATUREValue; 
 public System.Decimal MAX_TEMPERATURE
        {  
            get  
            {  
                return this.MAX_TEMPERATUREValue;  
            }  

          set { SetProperty(ref  MAX_TEMPERATUREValue, value); }
        } 
private  System.String MAX_TEMPERATURE_OUOMValue; 
 public System.String MAX_TEMPERATURE_OUOM
        {  
            get  
            {  
                return this.MAX_TEMPERATURE_OUOMValue;  
            }  

          set { SetProperty(ref  MAX_TEMPERATURE_OUOMValue, value); }
        } 
private  System.String MUD_SAMPLE_IDValue; 
 public System.String MUD_SAMPLE_ID
        {  
            get  
            {  
                return this.MUD_SAMPLE_IDValue;  
            }  

          set { SetProperty(ref  MUD_SAMPLE_IDValue, value); }
        } 
private  System.String MUD_SAMPLE_TYPEValue; 
 public System.String MUD_SAMPLE_TYPE
        {  
            get  
            {  
                return this.MUD_SAMPLE_TYPEValue;  
            }  

          set { SetProperty(ref  MUD_SAMPLE_TYPEValue, value); }
        } 
private  System.String MUD_SOURCEValue; 
 public System.String MUD_SOURCE
        {  
            get  
            {  
                return this.MUD_SOURCEValue;  
            }  

          set { SetProperty(ref  MUD_SOURCEValue, value); }
        } 
private  System.String OBSERVERValue; 
 public System.String OBSERVER
        {  
            get  
            {  
                return this.OBSERVERValue;  
            }  

          set { SetProperty(ref  OBSERVERValue, value); }
        } 
private  System.DateTime ON_BOTTOM_DATEValue; 
 public System.DateTime ON_BOTTOM_DATE
        {  
            get  
            {  
                return this.ON_BOTTOM_DATEValue;  
            }  

          set { SetProperty(ref  ON_BOTTOM_DATEValue, value); }
        } 
private  System.DateTime ON_BOTTOM_TIMEValue; 
 public System.DateTime ON_BOTTOM_TIME
        {  
            get  
            {  
                return this.ON_BOTTOM_TIMEValue;  
            }  

          set { SetProperty(ref  ON_BOTTOM_TIMEValue, value); }
        } 
private  System.String ON_BOTTOM_TIMEZONEValue; 
 public System.String ON_BOTTOM_TIMEZONE
        {  
            get  
            {  
                return this.ON_BOTTOM_TIMEZONEValue;  
            }  

          set { SetProperty(ref  ON_BOTTOM_TIMEZONEValue, value); }
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
private  System.Decimal REPORT_APDValue; 
 public System.Decimal REPORT_APD
        {  
            get  
            {  
                return this.REPORT_APDValue;  
            }  

          set { SetProperty(ref  REPORT_APDValue, value); }
        } 
private  System.String REPORT_LOG_DATUMValue; 
 public System.String REPORT_LOG_DATUM
        {  
            get  
            {  
                return this.REPORT_LOG_DATUMValue;  
            }  

          set { SetProperty(ref  REPORT_LOG_DATUMValue, value); }
        } 
private  System.Decimal REPORT_LOG_DATUM_ELEVValue; 
 public System.Decimal REPORT_LOG_DATUM_ELEV
        {  
            get  
            {  
                return this.REPORT_LOG_DATUM_ELEVValue;  
            }  

          set { SetProperty(ref  REPORT_LOG_DATUM_ELEVValue, value); }
        } 
private  System.String REPORT_LOG_DATUM_ELEV_OUOMValue; 
 public System.String REPORT_LOG_DATUM_ELEV_OUOM
        {  
            get  
            {  
                return this.REPORT_LOG_DATUM_ELEV_OUOMValue;  
            }  

          set { SetProperty(ref  REPORT_LOG_DATUM_ELEV_OUOMValue, value); }
        } 
private  System.String REPORT_LOG_RUNValue; 
 public System.String REPORT_LOG_RUN
        {  
            get  
            {  
                return this.REPORT_LOG_RUNValue;  
            }  

          set { SetProperty(ref  REPORT_LOG_RUNValue, value); }
        } 
private  System.String REPORT_PERM_DATUMValue; 
 public System.String REPORT_PERM_DATUM
        {  
            get  
            {  
                return this.REPORT_PERM_DATUMValue;  
            }  

          set { SetProperty(ref  REPORT_PERM_DATUMValue, value); }
        } 
private  System.Decimal REPORT_PERM_DATUM_ELEVValue; 
 public System.Decimal REPORT_PERM_DATUM_ELEV
        {  
            get  
            {  
                return this.REPORT_PERM_DATUM_ELEVValue;  
            }  

          set { SetProperty(ref  REPORT_PERM_DATUM_ELEVValue, value); }
        } 
private  System.String REPORT_PERM_DATUM_ELEV_OUOMValue; 
 public System.String REPORT_PERM_DATUM_ELEV_OUOM
        {  
            get  
            {  
                return this.REPORT_PERM_DATUM_ELEV_OUOMValue;  
            }  

          set { SetProperty(ref  REPORT_PERM_DATUM_ELEV_OUOMValue, value); }
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
private  System.DateTime TRIP_DATEValue; 
 public System.DateTime TRIP_DATE
        {  
            get  
            {  
                return this.TRIP_DATEValue;  
            }  

          set { SetProperty(ref  TRIP_DATEValue, value); }
        } 
private  System.Decimal TUBING_BOTTOM_DEPTHValue; 
 public System.Decimal TUBING_BOTTOM_DEPTH
        {  
            get  
            {  
                return this.TUBING_BOTTOM_DEPTHValue;  
            }  

          set { SetProperty(ref  TUBING_BOTTOM_DEPTHValue, value); }
        } 
private  System.String TUBING_BOTTOM_DEPTH_OUOMValue; 
 public System.String TUBING_BOTTOM_DEPTH_OUOM
        {  
            get  
            {  
                return this.TUBING_BOTTOM_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  TUBING_BOTTOM_DEPTH_OUOMValue, value); }
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
private  System.DateTime ROW_CHANGED_DATEValue; 
 public System.DateTime ROW_CHANGED_DATE
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
private  System.DateTime ROW_CREATED_DATEValue; 
 public System.DateTime ROW_CREATED_DATE
        {  
            get  
            {  
                return this.ROW_CREATED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_DATEValue, value); }
        } 
private  System.DateTime ROW_EFFECTIVE_DATEValue; 
 public System.DateTime ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime ROW_EXPIRY_DATEValue; 
 public System.DateTime ROW_EXPIRY_DATE
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


    public WELL_LOG_TRIP () { }

  }
}

