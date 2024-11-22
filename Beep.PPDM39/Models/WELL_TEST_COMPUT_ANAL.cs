using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class WELL_TEST_COMPUT_ANAL: Entity

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
private  System.String TEST_TYPEValue; 
 public System.String TEST_TYPE
        {  
            get  
            {  
                return this.TEST_TYPEValue;  
            }  

          set { SetProperty(ref  TEST_TYPEValue, value); }
        } 
private  System.String RUN_NUMValue; 
 public System.String RUN_NUM
        {  
            get  
            {  
                return this.RUN_NUMValue;  
            }  

          set { SetProperty(ref  RUN_NUMValue, value); }
        } 
private  System.String TEST_NUMValue; 
 public System.String TEST_NUM
        {  
            get  
            {  
                return this.TEST_NUMValue;  
            }  

          set { SetProperty(ref  TEST_NUMValue, value); }
        } 
private  System.Decimal REPORT_NOValue; 
 public System.Decimal REPORT_NO
        {  
            get  
            {  
                return this.REPORT_NOValue;  
            }  

          set { SetProperty(ref  REPORT_NOValue, value); }
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
private  System.String ANALYSIS_COMPANYValue; 
 public System.String ANALYSIS_COMPANY
        {  
            get  
            {  
                return this.ANALYSIS_COMPANYValue;  
            }  

          set { SetProperty(ref  ANALYSIS_COMPANYValue, value); }
        } 
private  System.Decimal COMPUTED_PERMEABILITYValue; 
 public System.Decimal COMPUTED_PERMEABILITY
        {  
            get  
            {  
                return this.COMPUTED_PERMEABILITYValue;  
            }  

          set { SetProperty(ref  COMPUTED_PERMEABILITYValue, value); }
        } 
private  System.String COMPUTED_PERMEABILITY_OUOMValue; 
 public System.String COMPUTED_PERMEABILITY_OUOM
        {  
            get  
            {  
                return this.COMPUTED_PERMEABILITY_OUOMValue;  
            }  

          set { SetProperty(ref  COMPUTED_PERMEABILITY_OUOMValue, value); }
        } 
private  System.Decimal CONFIDENCE_LIMITValue; 
 public System.Decimal CONFIDENCE_LIMIT
        {  
            get  
            {  
                return this.CONFIDENCE_LIMITValue;  
            }  

          set { SetProperty(ref  CONFIDENCE_LIMITValue, value); }
        } 
private  System.String CONFIDENCE_LIMIT_OUOMValue; 
 public System.String CONFIDENCE_LIMIT_OUOM
        {  
            get  
            {  
                return this.CONFIDENCE_LIMIT_OUOMValue;  
            }  

          set { SetProperty(ref  CONFIDENCE_LIMIT_OUOMValue, value); }
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
private  System.Decimal EST_DAMAGE_RATIOValue; 
 public System.Decimal EST_DAMAGE_RATIO
        {  
            get  
            {  
                return this.EST_DAMAGE_RATIOValue;  
            }  

          set { SetProperty(ref  EST_DAMAGE_RATIOValue, value); }
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
private  System.Decimal EXTRAP_PRESSURE_PERCENTValue; 
 public System.Decimal EXTRAP_PRESSURE_PERCENT
        {  
            get  
            {  
                return this.EXTRAP_PRESSURE_PERCENTValue;  
            }  

          set { SetProperty(ref  EXTRAP_PRESSURE_PERCENTValue, value); }
        } 
private  System.Decimal FINAL_RESERVOIR_PRESSUREValue; 
 public System.Decimal FINAL_RESERVOIR_PRESSURE
        {  
            get  
            {  
                return this.FINAL_RESERVOIR_PRESSUREValue;  
            }  

          set { SetProperty(ref  FINAL_RESERVOIR_PRESSUREValue, value); }
        } 
private  System.String FINAL_RESERVOIR_PRESS_OUOMValue; 
 public System.String FINAL_RESERVOIR_PRESS_OUOM
        {  
            get  
            {  
                return this.FINAL_RESERVOIR_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  FINAL_RESERVOIR_PRESS_OUOMValue, value); }
        } 
private  System.Decimal GAUGE_DEPTHValue; 
 public System.Decimal GAUGE_DEPTH
        {  
            get  
            {  
                return this.GAUGE_DEPTHValue;  
            }  

          set { SetProperty(ref  GAUGE_DEPTHValue, value); }
        } 
private  System.String GAUGE_DEPTH_OUOMValue; 
 public System.String GAUGE_DEPTH_OUOM
        {  
            get  
            {  
                return this.GAUGE_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  GAUGE_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal INVESTIGATION_RADIUSValue; 
 public System.Decimal INVESTIGATION_RADIUS
        {  
            get  
            {  
                return this.INVESTIGATION_RADIUSValue;  
            }  

          set { SetProperty(ref  INVESTIGATION_RADIUSValue, value); }
        } 
private  System.String INVESTIGATION_RADIUS_OUOMValue; 
 public System.String INVESTIGATION_RADIUS_OUOM
        {  
            get  
            {  
                return this.INVESTIGATION_RADIUS_OUOMValue;  
            }  

          set { SetProperty(ref  INVESTIGATION_RADIUS_OUOMValue, value); }
        } 
private  System.Decimal MAX_RESERVOIR_PRESSUREValue; 
 public System.Decimal MAX_RESERVOIR_PRESSURE
        {  
            get  
            {  
                return this.MAX_RESERVOIR_PRESSUREValue;  
            }  

          set { SetProperty(ref  MAX_RESERVOIR_PRESSUREValue, value); }
        } 
private  System.String MAX_RESERVOIR_PRESSURE_OUOMValue; 
 public System.String MAX_RESERVOIR_PRESSURE_OUOM
        {  
            get  
            {  
                return this.MAX_RESERVOIR_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  MAX_RESERVOIR_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal POTMTRC_SURF_HEIGHTValue; 
 public System.Decimal POTMTRC_SURF_HEIGHT
        {  
            get  
            {  
                return this.POTMTRC_SURF_HEIGHTValue;  
            }  

          set { SetProperty(ref  POTMTRC_SURF_HEIGHTValue, value); }
        } 
private  System.String POTMTRC_SURF_HEIGHT_OUOMValue; 
 public System.String POTMTRC_SURF_HEIGHT_OUOM
        {  
            get  
            {  
                return this.POTMTRC_SURF_HEIGHT_OUOMValue;  
            }  

          set { SetProperty(ref  POTMTRC_SURF_HEIGHT_OUOMValue, value); }
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
private  System.Decimal PRODUCTION_INDEX_RATEValue; 
 public System.Decimal PRODUCTION_INDEX_RATE
        {  
            get  
            {  
                return this.PRODUCTION_INDEX_RATEValue;  
            }  

          set { SetProperty(ref  PRODUCTION_INDEX_RATEValue, value); }
        } 
private  System.String PRODUCTION_INDEX_RATE_OUOMValue; 
 public System.String PRODUCTION_INDEX_RATE_OUOM
        {  
            get  
            {  
                return this.PRODUCTION_INDEX_RATE_OUOMValue;  
            }  

          set { SetProperty(ref  PRODUCTION_INDEX_RATE_OUOMValue, value); }
        } 
private  System.String RECORDER_IDValue; 
 public System.String RECORDER_ID
        {  
            get  
            {  
                return this.RECORDER_IDValue;  
            }  

          set { SetProperty(ref  RECORDER_IDValue, value); }
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


    public WELL_TEST_COMPUT_ANAL () { }

  }
}

