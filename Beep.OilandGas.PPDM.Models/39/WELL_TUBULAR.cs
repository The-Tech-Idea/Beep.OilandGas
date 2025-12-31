using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_TUBULAR: Entity,IPPDMEntity

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
private  System.String TUBING_TYPEValue; 
 public System.String TUBING_TYPE
        {  
            get  
            {  
                return this.TUBING_TYPEValue;  
            }  

          set { SetProperty(ref  TUBING_TYPEValue, value); }
        } 
private  System.Decimal TUBING_OBS_NOValue; 
 public System.Decimal TUBING_OBS_NO
        {  
            get  
            {  
                return this.TUBING_OBS_NOValue;  
            }  

          set { SetProperty(ref  TUBING_OBS_NOValue, value); }
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
private  System.String CAT_EQUIP_IDValue; 
 public System.String CAT_EQUIP_ID
        {  
            get  
            {  
                return this.CAT_EQUIP_IDValue;  
            }  

          set { SetProperty(ref  CAT_EQUIP_IDValue, value); }
        } 
private  System.String COLLAR_TYPEValue; 
 public System.String COLLAR_TYPE
        {  
            get  
            {  
                return this.COLLAR_TYPEValue;  
            }  

          set { SetProperty(ref  COLLAR_TYPEValue, value); }
        } 
private  System.String COUPLING_TYPEValue; 
 public System.String COUPLING_TYPE
        {  
            get  
            {  
                return this.COUPLING_TYPEValue;  
            }  

          set { SetProperty(ref  COUPLING_TYPEValue, value); }
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
private  System.String EQUIPMENT_IDValue; 
 public System.String EQUIPMENT_ID
        {  
            get  
            {  
                return this.EQUIPMENT_IDValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_IDValue, value); }
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
private  System.Decimal HOLE_SIZEValue; 
 public System.Decimal HOLE_SIZE
        {  
            get  
            {  
                return this.HOLE_SIZEValue;  
            }  

          set { SetProperty(ref  HOLE_SIZEValue, value); }
        } 
private  System.String HOLE_SIZE_OUOMValue; 
 public System.String HOLE_SIZE_OUOM
        {  
            get  
            {  
                return this.HOLE_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  HOLE_SIZE_OUOMValue, value); }
        } 
private  System.Decimal HUNG_TOP_DEPTHValue; 
 public System.Decimal HUNG_TOP_DEPTH
        {  
            get  
            {  
                return this.HUNG_TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  HUNG_TOP_DEPTHValue, value); }
        } 
private  System.String HUNG_TOP_DEPTH_OUOMValue; 
 public System.String HUNG_TOP_DEPTH_OUOM
        {  
            get  
            {  
                return this.HUNG_TOP_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  HUNG_TOP_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal INSIDE_DIAMETERValue; 
 public System.Decimal INSIDE_DIAMETER
        {  
            get  
            {  
                return this.INSIDE_DIAMETERValue;  
            }  

          set { SetProperty(ref  INSIDE_DIAMETERValue, value); }
        } 
private  System.String INSIDE_DIAMETER_OUOMValue; 
 public System.String INSIDE_DIAMETER_OUOM
        {  
            get  
            {  
                return this.INSIDE_DIAMETER_OUOMValue;  
            }  

          set { SetProperty(ref  INSIDE_DIAMETER_OUOMValue, value); }
        } 
private  System.Decimal JOINT_COUNTValue; 
 public System.Decimal JOINT_COUNT
        {  
            get  
            {  
                return this.JOINT_COUNTValue;  
            }  

          set { SetProperty(ref  JOINT_COUNTValue, value); }
        } 
private  System.Decimal LEFT_IN_HOLE_LENGTHValue; 
 public System.Decimal LEFT_IN_HOLE_LENGTH
        {  
            get  
            {  
                return this.LEFT_IN_HOLE_LENGTHValue;  
            }  

          set { SetProperty(ref  LEFT_IN_HOLE_LENGTHValue, value); }
        } 
private  System.String LEFT_IN_HOLE_LENGTH_OUOMValue; 
 public System.String LEFT_IN_HOLE_LENGTH_OUOM
        {  
            get  
            {  
                return this.LEFT_IN_HOLE_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  LEFT_IN_HOLE_LENGTH_OUOMValue, value); }
        } 
private  System.String LINER_TYPEValue; 
 public System.String LINER_TYPE
        {  
            get  
            {  
                return this.LINER_TYPEValue;  
            }  

          set { SetProperty(ref  LINER_TYPEValue, value); }
        } 
private  System.String MANUFACTURER_BA_IDValue; 
 public System.String MANUFACTURER_BA_ID
        {  
            get  
            {  
                return this.MANUFACTURER_BA_IDValue;  
            }  

          set { SetProperty(ref  MANUFACTURER_BA_IDValue, value); }
        } 
private  System.String MIXED_STRING_INDValue; 
 public System.String MIXED_STRING_IND
        {  
            get  
            {  
                return this.MIXED_STRING_INDValue;  
            }  

          set { SetProperty(ref  MIXED_STRING_INDValue, value); }
        } 
private  System.DateTime? OBSERVATION_DATEValue; 
 public System.DateTime? OBSERVATION_DATE
        {  
            get  
            {  
                return this.OBSERVATION_DATEValue;  
            }  

          set { SetProperty(ref  OBSERVATION_DATEValue, value); }
        } 
private  System.Decimal OUTSIDE_DIAMETERValue; 
 public System.Decimal OUTSIDE_DIAMETER
        {  
            get  
            {  
                return this.OUTSIDE_DIAMETERValue;  
            }  

          set { SetProperty(ref  OUTSIDE_DIAMETERValue, value); }
        } 
private  System.String OUTSIDE_DIAMETER_DESCValue; 
 public System.String OUTSIDE_DIAMETER_DESC
        {  
            get  
            {  
                return this.OUTSIDE_DIAMETER_DESCValue;  
            }  

          set { SetProperty(ref  OUTSIDE_DIAMETER_DESCValue, value); }
        } 
private  System.String OUTSIDE_DIAMETER_OUOMValue; 
 public System.String OUTSIDE_DIAMETER_OUOM
        {  
            get  
            {  
                return this.OUTSIDE_DIAMETER_OUOMValue;  
            }  

          set { SetProperty(ref  OUTSIDE_DIAMETER_OUOMValue, value); }
        } 
private  System.Decimal PACKER_SET_DEPTHValue; 
 public System.Decimal PACKER_SET_DEPTH
        {  
            get  
            {  
                return this.PACKER_SET_DEPTHValue;  
            }  

          set { SetProperty(ref  PACKER_SET_DEPTHValue, value); }
        } 
private  System.String PACKER_SET_DEPTH_OUOMValue; 
 public System.String PACKER_SET_DEPTH_OUOM
        {  
            get  
            {  
                return this.PACKER_SET_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  PACKER_SET_DEPTH_OUOMValue, value); }
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
private  System.String PPDM_GUIDValue; 
 public System.String PPDM_GUID
        {  
            get  
            {  
                return this.PPDM_GUIDValue;  
            }  

          set { SetProperty(ref  PPDM_GUIDValue, value); }
        } 
private  System.Decimal PULLED_LENGTHValue; 
 public System.Decimal PULLED_LENGTH
        {  
            get  
            {  
                return this.PULLED_LENGTHValue;  
            }  

          set { SetProperty(ref  PULLED_LENGTHValue, value); }
        } 
private  System.String PULLED_LENGTH_OUOMValue; 
 public System.String PULLED_LENGTH_OUOM
        {  
            get  
            {  
                return this.PULLED_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  PULLED_LENGTH_OUOMValue, value); }
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
private  System.Decimal REPORTED_BASE_TVDValue; 
 public System.Decimal REPORTED_BASE_TVD
        {  
            get  
            {  
                return this.REPORTED_BASE_TVDValue;  
            }  

          set { SetProperty(ref  REPORTED_BASE_TVDValue, value); }
        } 
private  System.String REPORTED_BASE_TVD_OUOMValue; 
 public System.String REPORTED_BASE_TVD_OUOM
        {  
            get  
            {  
                return this.REPORTED_BASE_TVD_OUOMValue;  
            }  

          set { SetProperty(ref  REPORTED_BASE_TVD_OUOMValue, value); }
        } 
private  System.Decimal REPORTED_TOP_TVDValue; 
 public System.Decimal REPORTED_TOP_TVD
        {  
            get  
            {  
                return this.REPORTED_TOP_TVDValue;  
            }  

          set { SetProperty(ref  REPORTED_TOP_TVDValue, value); }
        } 
private  System.String REPORTED_TOP_TVD_OUOMValue; 
 public System.String REPORTED_TOP_TVD_OUOM
        {  
            get  
            {  
                return this.REPORTED_TOP_TVD_OUOMValue;  
            }  

          set { SetProperty(ref  REPORTED_TOP_TVD_OUOMValue, value); }
        } 
private  System.Decimal SEA_FLOOR_PENETRATIONValue; 
 public System.Decimal SEA_FLOOR_PENETRATION
        {  
            get  
            {  
                return this.SEA_FLOOR_PENETRATIONValue;  
            }  

          set { SetProperty(ref  SEA_FLOOR_PENETRATIONValue, value); }
        } 
private  System.String SEA_FLOOR_PENETRATION_OUOMValue; 
 public System.String SEA_FLOOR_PENETRATION_OUOM
        {  
            get  
            {  
                return this.SEA_FLOOR_PENETRATION_OUOMValue;  
            }  

          set { SetProperty(ref  SEA_FLOOR_PENETRATION_OUOMValue, value); }
        } 
private  System.Decimal SHOE_DEPTHValue; 
 public System.Decimal SHOE_DEPTH
        {  
            get  
            {  
                return this.SHOE_DEPTHValue;  
            }  

          set { SetProperty(ref  SHOE_DEPTHValue, value); }
        } 
private  System.String SHOE_DEPTH_OUOMValue; 
 public System.String SHOE_DEPTH_OUOM
        {  
            get  
            {  
                return this.SHOE_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  SHOE_DEPTH_OUOMValue, value); }
        } 
private  System.String STEEL_SPECValue; 
 public System.String STEEL_SPEC
        {  
            get  
            {  
                return this.STEEL_SPECValue;  
            }  

          set { SetProperty(ref  STEEL_SPECValue, value); }
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
private  System.String TOP_STRAT_UNIT_IDValue; 
 public System.String TOP_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.TOP_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  TOP_STRAT_UNIT_IDValue, value); }
        } 
private  System.Decimal TORQUEValue; 
 public System.Decimal TORQUE
        {  
            get  
            {  
                return this.TORQUEValue;  
            }  

          set { SetProperty(ref  TORQUEValue, value); }
        } 
private  System.String TORQUE_OUOMValue; 
 public System.String TORQUE_OUOM
        {  
            get  
            {  
                return this.TORQUE_OUOMValue;  
            }  

          set { SetProperty(ref  TORQUE_OUOMValue, value); }
        } 
private  System.Decimal TUBING_DENSITYValue; 
 public System.Decimal TUBING_DENSITY
        {  
            get  
            {  
                return this.TUBING_DENSITYValue;  
            }  

          set { SetProperty(ref  TUBING_DENSITYValue, value); }
        } 
private  System.String TUBING_DENSITY_OUOMValue; 
 public System.String TUBING_DENSITY_OUOM
        {  
            get  
            {  
                return this.TUBING_DENSITY_OUOMValue;  
            }  

          set { SetProperty(ref  TUBING_DENSITY_OUOMValue, value); }
        } 
private  System.String TUBING_GRADEValue; 
 public System.String TUBING_GRADE
        {  
            get  
            {  
                return this.TUBING_GRADEValue;  
            }  

          set { SetProperty(ref  TUBING_GRADEValue, value); }
        } 
private  System.Decimal TUBING_STRENGTHValue; 
 public System.Decimal TUBING_STRENGTH
        {  
            get  
            {  
                return this.TUBING_STRENGTHValue;  
            }  

          set { SetProperty(ref  TUBING_STRENGTHValue, value); }
        } 
private  System.String TUBING_STRENGTH_OUOMValue; 
 public System.String TUBING_STRENGTH_OUOM
        {  
            get  
            {  
                return this.TUBING_STRENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  TUBING_STRENGTH_OUOMValue, value); }
        } 
private  System.Decimal TUBING_WEIGHTValue; 
 public System.Decimal TUBING_WEIGHT
        {  
            get  
            {  
                return this.TUBING_WEIGHTValue;  
            }  

          set { SetProperty(ref  TUBING_WEIGHTValue, value); }
        } 
private  System.String TUBING_WEIGHT_OUOMValue; 
 public System.String TUBING_WEIGHT_OUOM
        {  
            get  
            {  
                return this.TUBING_WEIGHT_OUOMValue;  
            }  

          set { SetProperty(ref  TUBING_WEIGHT_OUOMValue, value); }
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


    public WELL_TUBULAR () { }

  }
}

