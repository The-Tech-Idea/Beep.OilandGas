using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class WELL_PRESSURE_AOF: Entity

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
private  System.Decimal PRESSURE_OBS_NOValue; 
 public System.Decimal PRESSURE_OBS_NO
        {  
            get  
            {  
                return this.PRESSURE_OBS_NOValue;  
            }  

          set { SetProperty(ref  PRESSURE_OBS_NOValue, value); }
        } 
private  System.Decimal AOF_OBS_NOValue; 
 public System.Decimal AOF_OBS_NO
        {  
            get  
            {  
                return this.AOF_OBS_NOValue;  
            }  

          set { SetProperty(ref  AOF_OBS_NOValue, value); }
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
private  System.String ANALYSIS_TYPEValue; 
 public System.String ANALYSIS_TYPE
        {  
            get  
            {  
                return this.ANALYSIS_TYPEValue;  
            }  

          set { SetProperty(ref  ANALYSIS_TYPEValue, value); }
        } 
private  System.String AOF_CALCULATE_METHODValue; 
 public System.String AOF_CALCULATE_METHOD
        {  
            get  
            {  
                return this.AOF_CALCULATE_METHODValue;  
            }  

          set { SetProperty(ref  AOF_CALCULATE_METHODValue, value); }
        } 
private  System.Decimal AOF_POTENTIALValue; 
 public System.Decimal AOF_POTENTIAL
        {  
            get  
            {  
                return this.AOF_POTENTIALValue;  
            }  

          set { SetProperty(ref  AOF_POTENTIALValue, value); }
        } 
private  System.String AOF_POTENTIAL_OUOMValue; 
 public System.String AOF_POTENTIAL_OUOM
        {  
            get  
            {  
                return this.AOF_POTENTIAL_OUOMValue;  
            }  

          set { SetProperty(ref  AOF_POTENTIAL_OUOMValue, value); }
        } 
private  System.Decimal AOF_SLOPEValue; 
 public System.Decimal AOF_SLOPE
        {  
            get  
            {  
                return this.AOF_SLOPEValue;  
            }  

          set { SetProperty(ref  AOF_SLOPEValue, value); }
        } 
private  System.String BOTTOM_HOLE_PRESSURE_METHODValue; 
 public System.String BOTTOM_HOLE_PRESSURE_METHOD
        {  
            get  
            {  
                return this.BOTTOM_HOLE_PRESSURE_METHODValue;  
            }  

          set { SetProperty(ref  BOTTOM_HOLE_PRESSURE_METHODValue, value); }
        } 
private  System.Decimal CAOF_RATEValue; 
 public System.Decimal CAOF_RATE
        {  
            get  
            {  
                return this.CAOF_RATEValue;  
            }  

          set { SetProperty(ref  CAOF_RATEValue, value); }
        } 
private  System.String CAOF_RATE_OUOMValue; 
 public System.String CAOF_RATE_OUOM
        {  
            get  
            {  
                return this.CAOF_RATE_OUOMValue;  
            }  

          set { SetProperty(ref  CAOF_RATE_OUOMValue, value); }
        } 
private  System.String CHOKE_SIZE_DESCValue; 
 public System.String CHOKE_SIZE_DESC
        {  
            get  
            {  
                return this.CHOKE_SIZE_DESCValue;  
            }  

          set { SetProperty(ref  CHOKE_SIZE_DESCValue, value); }
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
private  System.Decimal FLOW_PERIOD_DURATIONValue; 
 public System.Decimal FLOW_PERIOD_DURATION
        {  
            get  
            {  
                return this.FLOW_PERIOD_DURATIONValue;  
            }  

          set { SetProperty(ref  FLOW_PERIOD_DURATIONValue, value); }
        } 
private  System.String FLOW_PERIOD_DURATION_OUOMValue; 
 public System.String FLOW_PERIOD_DURATION_OUOM
        {  
            get  
            {  
                return this.FLOW_PERIOD_DURATION_OUOMValue;  
            }  

          set { SetProperty(ref  FLOW_PERIOD_DURATION_OUOMValue, value); }
        } 
private  System.Decimal FLOW_PRESSUREValue; 
 public System.Decimal FLOW_PRESSURE
        {  
            get  
            {  
                return this.FLOW_PRESSUREValue;  
            }  

          set { SetProperty(ref  FLOW_PRESSUREValue, value); }
        } 
private  System.String FLOW_PRESSURE_OUOMValue; 
 public System.String FLOW_PRESSURE_OUOM
        {  
            get  
            {  
                return this.FLOW_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  FLOW_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal FLOW_RATEValue; 
 public System.Decimal FLOW_RATE
        {  
            get  
            {  
                return this.FLOW_RATEValue;  
            }  

          set { SetProperty(ref  FLOW_RATEValue, value); }
        } 
private  System.String FLOW_RATE_OUOMValue; 
 public System.String FLOW_RATE_OUOM
        {  
            get  
            {  
                return this.FLOW_RATE_OUOMValue;  
            }  

          set { SetProperty(ref  FLOW_RATE_OUOMValue, value); }
        } 
private  System.Decimal FOUR_POINT_CAOF_RATEValue; 
 public System.Decimal FOUR_POINT_CAOF_RATE
        {  
            get  
            {  
                return this.FOUR_POINT_CAOF_RATEValue;  
            }  

          set { SetProperty(ref  FOUR_POINT_CAOF_RATEValue, value); }
        } 
private  System.String FOUR_POINT_CAOF_RATE_OUOMValue; 
 public System.String FOUR_POINT_CAOF_RATE_OUOM
        {  
            get  
            {  
                return this.FOUR_POINT_CAOF_RATE_OUOMValue;  
            }  

          set { SetProperty(ref  FOUR_POINT_CAOF_RATE_OUOMValue, value); }
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
private  System.Decimal RESERVOIR_PRESSUREValue; 
 public System.Decimal RESERVOIR_PRESSURE
        {  
            get  
            {  
                return this.RESERVOIR_PRESSUREValue;  
            }  

          set { SetProperty(ref  RESERVOIR_PRESSUREValue, value); }
        } 
private  System.String RESERVOIR_PRESSURE_OUOMValue; 
 public System.String RESERVOIR_PRESSURE_OUOM
        {  
            get  
            {  
                return this.RESERVOIR_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  RESERVOIR_PRESSURE_OUOMValue, value); }
        } 
private  System.String SHUTIN_PRESSURE_TYPEValue; 
 public System.String SHUTIN_PRESSURE_TYPE
        {  
            get  
            {  
                return this.SHUTIN_PRESSURE_TYPEValue;  
            }  

          set { SetProperty(ref  SHUTIN_PRESSURE_TYPEValue, value); }
        } 
private  System.DateTime TEST_DATEValue; 
 public System.DateTime TEST_DATE
        {  
            get  
            {  
                return this.TEST_DATEValue;  
            }  

          set { SetProperty(ref  TEST_DATEValue, value); }
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


    public WELL_PRESSURE_AOF () { }

  }
}

