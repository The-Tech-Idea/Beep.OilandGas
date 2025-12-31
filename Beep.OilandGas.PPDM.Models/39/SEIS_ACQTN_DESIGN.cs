using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_ACQTN_DESIGN: Entity,IPPDMEntity

{

private  System.String ACQTN_DESIGN_IDValue; 
 public System.String ACQTN_DESIGN_ID
        {  
            get  
            {  
                return this.ACQTN_DESIGN_IDValue;  
            }  

          set { SetProperty(ref  ACQTN_DESIGN_IDValue, value); }
        } 
private  System.DateTime? ACQTN_COMPLETED_DATEValue; 
 public System.DateTime? ACQTN_COMPLETED_DATE
        {  
            get  
            {  
                return this.ACQTN_COMPLETED_DATEValue;  
            }  

          set { SetProperty(ref  ACQTN_COMPLETED_DATEValue, value); }
        } 
private  System.String ACQTN_COMPLETED_DATE_DESCValue; 
 public System.String ACQTN_COMPLETED_DATE_DESC
        {  
            get  
            {  
                return this.ACQTN_COMPLETED_DATE_DESCValue;  
            }  

          set { SetProperty(ref  ACQTN_COMPLETED_DATE_DESCValue, value); }
        } 
private  System.String ACQTN_DIMENSIONValue; 
 public System.String ACQTN_DIMENSION
        {  
            get  
            {  
                return this.ACQTN_DIMENSIONValue;  
            }  

          set { SetProperty(ref  ACQTN_DIMENSIONValue, value); }
        } 
private  System.String ACQTN_DIRECTIONValue; 
 public System.String ACQTN_DIRECTION
        {  
            get  
            {  
                return this.ACQTN_DIRECTIONValue;  
            }  

          set { SetProperty(ref  ACQTN_DIRECTIONValue, value); }
        } 
private  System.Decimal ACQTN_INLINE_BIN_SIZEValue; 
 public System.Decimal ACQTN_INLINE_BIN_SIZE
        {  
            get  
            {  
                return this.ACQTN_INLINE_BIN_SIZEValue;  
            }  

          set { SetProperty(ref  ACQTN_INLINE_BIN_SIZEValue, value); }
        } 
private  System.String ACQTN_INLINE_BIN_SIZE_OUOMValue; 
 public System.String ACQTN_INLINE_BIN_SIZE_OUOM
        {  
            get  
            {  
                return this.ACQTN_INLINE_BIN_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  ACQTN_INLINE_BIN_SIZE_OUOMValue, value); }
        } 
private  System.String ACQTN_REMARKValue; 
 public System.String ACQTN_REMARK
        {  
            get  
            {  
                return this.ACQTN_REMARKValue;  
            }  

          set { SetProperty(ref  ACQTN_REMARKValue, value); }
        } 
private  System.Decimal ACQTN_SHOTPT_INTERVALValue; 
 public System.Decimal ACQTN_SHOTPT_INTERVAL
        {  
            get  
            {  
                return this.ACQTN_SHOTPT_INTERVALValue;  
            }  

          set { SetProperty(ref  ACQTN_SHOTPT_INTERVALValue, value); }
        } 
private  System.String ACQTN_SHOTPT_INTERVAL_OUOMValue; 
 public System.String ACQTN_SHOTPT_INTERVAL_OUOM
        {  
            get  
            {  
                return this.ACQTN_SHOTPT_INTERVAL_OUOMValue;  
            }  

          set { SetProperty(ref  ACQTN_SHOTPT_INTERVAL_OUOMValue, value); }
        } 
private  System.Decimal ACQTN_SHOT_LINE_SPACINGValue; 
 public System.Decimal ACQTN_SHOT_LINE_SPACING
        {  
            get  
            {  
                return this.ACQTN_SHOT_LINE_SPACINGValue;  
            }  

          set { SetProperty(ref  ACQTN_SHOT_LINE_SPACINGValue, value); }
        } 
private  System.String ACQTN_SHOT_LINE_SPACING_OUOMValue; 
 public System.String ACQTN_SHOT_LINE_SPACING_OUOM
        {  
            get  
            {  
                return this.ACQTN_SHOT_LINE_SPACING_OUOMValue;  
            }  

          set { SetProperty(ref  ACQTN_SHOT_LINE_SPACING_OUOMValue, value); }
        } 
private  System.Decimal ACQTN_SHOT_TIME_INTVLValue; 
 public System.Decimal ACQTN_SHOT_TIME_INTVL
        {  
            get  
            {  
                return this.ACQTN_SHOT_TIME_INTVLValue;  
            }  

          set { SetProperty(ref  ACQTN_SHOT_TIME_INTVLValue, value); }
        } 
private  System.String ACQTN_SHOT_TIME_INTVL_OUOMValue; 
 public System.String ACQTN_SHOT_TIME_INTVL_OUOM
        {  
            get  
            {  
                return this.ACQTN_SHOT_TIME_INTVL_OUOMValue;  
            }  

          set { SetProperty(ref  ACQTN_SHOT_TIME_INTVL_OUOMValue, value); }
        } 
private  System.DateTime? ACQTN_START_DATEValue; 
 public System.DateTime? ACQTN_START_DATE
        {  
            get  
            {  
                return this.ACQTN_START_DATEValue;  
            }  

          set { SetProperty(ref  ACQTN_START_DATEValue, value); }
        } 
private  System.String ACQTN_START_DATE_DESCValue; 
 public System.String ACQTN_START_DATE_DESC
        {  
            get  
            {  
                return this.ACQTN_START_DATE_DESCValue;  
            }  

          set { SetProperty(ref  ACQTN_START_DATE_DESCValue, value); }
        } 
private  System.Decimal ACQTN_XLINE_BIN_SIZEValue; 
 public System.Decimal ACQTN_XLINE_BIN_SIZE
        {  
            get  
            {  
                return this.ACQTN_XLINE_BIN_SIZEValue;  
            }  

          set { SetProperty(ref  ACQTN_XLINE_BIN_SIZEValue, value); }
        } 
private  System.String ACQTN_XLINE_BIN_SIZE_OUOMValue; 
 public System.String ACQTN_XLINE_BIN_SIZE_OUOM
        {  
            get  
            {  
                return this.ACQTN_XLINE_BIN_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  ACQTN_XLINE_BIN_SIZE_OUOMValue, value); }
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
private  System.String ACTUAL_INDValue; 
 public System.String ACTUAL_IND
        {  
            get  
            {  
                return this.ACTUAL_INDValue;  
            }  

          set { SetProperty(ref  ACTUAL_INDValue, value); }
        } 
private  System.Decimal CDP_COVERAGEValue; 
 public System.Decimal CDP_COVERAGE
        {  
            get  
            {  
                return this.CDP_COVERAGEValue;  
            }  

          set { SetProperty(ref  CDP_COVERAGEValue, value); }
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
private  System.Decimal ENERGY_CHARGE_SIZEValue; 
 public System.Decimal ENERGY_CHARGE_SIZE
        {  
            get  
            {  
                return this.ENERGY_CHARGE_SIZEValue;  
            }  

          set { SetProperty(ref  ENERGY_CHARGE_SIZEValue, value); }
        } 
private  System.String ENERGY_CHARGE_SIZE_OUOMValue; 
 public System.String ENERGY_CHARGE_SIZE_OUOM
        {  
            get  
            {  
                return this.ENERGY_CHARGE_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  ENERGY_CHARGE_SIZE_OUOMValue, value); }
        } 
private  System.Decimal ENERGY_OPRG_PSRValue; 
 public System.Decimal ENERGY_OPRG_PSR
        {  
            get  
            {  
                return this.ENERGY_OPRG_PSRValue;  
            }  

          set { SetProperty(ref  ENERGY_OPRG_PSRValue, value); }
        } 
private  System.String ENERGY_OPRG_PSR_OUOMValue; 
 public System.String ENERGY_OPRG_PSR_OUOM
        {  
            get  
            {  
                return this.ENERGY_OPRG_PSR_OUOMValue;  
            }  

          set { SetProperty(ref  ENERGY_OPRG_PSR_OUOMValue, value); }
        } 
private  System.Decimal ENERGY_OPRG_VOLUMEValue; 
 public System.Decimal ENERGY_OPRG_VOLUME
        {  
            get  
            {  
                return this.ENERGY_OPRG_VOLUMEValue;  
            }  

          set { SetProperty(ref  ENERGY_OPRG_VOLUMEValue, value); }
        } 
private  System.String ENERGY_OPRG_VOLUME_OUOMValue; 
 public System.String ENERGY_OPRG_VOLUME_OUOM
        {  
            get  
            {  
                return this.ENERGY_OPRG_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  ENERGY_OPRG_VOLUME_OUOMValue, value); }
        } 
private  System.Decimal ENERGY_SHOT_DEPTHValue; 
 public System.Decimal ENERGY_SHOT_DEPTH
        {  
            get  
            {  
                return this.ENERGY_SHOT_DEPTHValue;  
            }  

          set { SetProperty(ref  ENERGY_SHOT_DEPTHValue, value); }
        } 
private  System.String ENERGY_SHOT_DEPTH_OUOMValue; 
 public System.String ENERGY_SHOT_DEPTH_OUOM
        {  
            get  
            {  
                return this.ENERGY_SHOT_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  ENERGY_SHOT_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal ENERGY_SRC_ARRAY_SPCValue; 
 public System.Decimal ENERGY_SRC_ARRAY_SPC
        {  
            get  
            {  
                return this.ENERGY_SRC_ARRAY_SPCValue;  
            }  

          set { SetProperty(ref  ENERGY_SRC_ARRAY_SPCValue, value); }
        } 
private  System.String ENERGY_SRC_ARRAY_SPC_OUOMValue; 
 public System.String ENERGY_SRC_ARRAY_SPC_OUOM
        {  
            get  
            {  
                return this.ENERGY_SRC_ARRAY_SPC_OUOMValue;  
            }  

          set { SetProperty(ref  ENERGY_SRC_ARRAY_SPC_OUOMValue, value); }
        } 
private  System.String ENERGY_SRC_ARRAY_TYPEValue; 
 public System.String ENERGY_SRC_ARRAY_TYPE
        {  
            get  
            {  
                return this.ENERGY_SRC_ARRAY_TYPEValue;  
            }  

          set { SetProperty(ref  ENERGY_SRC_ARRAY_TYPEValue, value); }
        } 
private  System.String ENERGY_SRC_MAKEValue; 
 public System.String ENERGY_SRC_MAKE
        {  
            get  
            {  
                return this.ENERGY_SRC_MAKEValue;  
            }  

          set { SetProperty(ref  ENERGY_SRC_MAKEValue, value); }
        } 
private  System.Decimal ENERGY_SRC_PER_SHOTValue; 
 public System.Decimal ENERGY_SRC_PER_SHOT
        {  
            get  
            {  
                return this.ENERGY_SRC_PER_SHOTValue;  
            }  

          set { SetProperty(ref  ENERGY_SRC_PER_SHOTValue, value); }
        } 
private  System.Decimal ENERGY_SWEEP_DURATIONValue; 
 public System.Decimal ENERGY_SWEEP_DURATION
        {  
            get  
            {  
                return this.ENERGY_SWEEP_DURATIONValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_DURATIONValue, value); }
        } 
private  System.String ENERGY_SWEEP_DURATION_OUOMValue; 
 public System.String ENERGY_SWEEP_DURATION_OUOM
        {  
            get  
            {  
                return this.ENERGY_SWEEP_DURATION_OUOMValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_DURATION_OUOMValue, value); }
        } 
private  System.Decimal ENERGY_SWEEP_END_FREQValue; 
 public System.Decimal ENERGY_SWEEP_END_FREQ
        {  
            get  
            {  
                return this.ENERGY_SWEEP_END_FREQValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_END_FREQValue, value); }
        } 
private  System.String ENERGY_SWEEP_FREQ_OUOMValue; 
 public System.String ENERGY_SWEEP_FREQ_OUOM
        {  
            get  
            {  
                return this.ENERGY_SWEEP_FREQ_OUOMValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_FREQ_OUOMValue, value); }
        } 
private  System.Decimal ENERGY_SWEEP_MVUP_DISTValue; 
 public System.Decimal ENERGY_SWEEP_MVUP_DIST
        {  
            get  
            {  
                return this.ENERGY_SWEEP_MVUP_DISTValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_MVUP_DISTValue, value); }
        } 
private  System.String ENERGY_SWEEP_MVUP_DIST_OUOMValue; 
 public System.String ENERGY_SWEEP_MVUP_DIST_OUOM
        {  
            get  
            {  
                return this.ENERGY_SWEEP_MVUP_DIST_OUOMValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_MVUP_DIST_OUOMValue, value); }
        } 
private  System.Decimal ENERGY_SWEEP_NOValue; 
 public System.Decimal ENERGY_SWEEP_NO
        {  
            get  
            {  
                return this.ENERGY_SWEEP_NOValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_NOValue, value); }
        } 
private  System.Decimal ENERGY_SWEEP_ST_FREQValue; 
 public System.Decimal ENERGY_SWEEP_ST_FREQ
        {  
            get  
            {  
                return this.ENERGY_SWEEP_ST_FREQValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_ST_FREQValue, value); }
        } 
private  System.Decimal ENERGY_SWEEP_TAPERValue; 
 public System.Decimal ENERGY_SWEEP_TAPER
        {  
            get  
            {  
                return this.ENERGY_SWEEP_TAPERValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_TAPERValue, value); }
        } 
private  System.String ENERGY_SWEEP_TAPER_OUOMValue; 
 public System.String ENERGY_SWEEP_TAPER_OUOM
        {  
            get  
            {  
                return this.ENERGY_SWEEP_TAPER_OUOMValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_TAPER_OUOMValue, value); }
        } 
private  System.String ENERGY_SWEEP_TYPEValue; 
 public System.String ENERGY_SWEEP_TYPE
        {  
            get  
            {  
                return this.ENERGY_SWEEP_TYPEValue;  
            }  

          set { SetProperty(ref  ENERGY_SWEEP_TYPEValue, value); }
        } 
private  System.String ENERGY_TYPEValue; 
 public System.String ENERGY_TYPE
        {  
            get  
            {  
                return this.ENERGY_TYPEValue;  
            }  

          set { SetProperty(ref  ENERGY_TYPEValue, value); }
        } 
private  System.String ENVIRONMENTValue; 
 public System.String ENVIRONMENT
        {  
            get  
            {  
                return this.ENVIRONMENTValue;  
            }  

          set { SetProperty(ref  ENVIRONMENTValue, value); }
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
private  System.Decimal MONITOR_DEPTHValue; 
 public System.Decimal MONITOR_DEPTH
        {  
            get  
            {  
                return this.MONITOR_DEPTHValue;  
            }  

          set { SetProperty(ref  MONITOR_DEPTHValue, value); }
        } 
private  System.String MONITOR_DEPTH_OUOMValue; 
 public System.String MONITOR_DEPTH_OUOM
        {  
            get  
            {  
                return this.MONITOR_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  MONITOR_DEPTH_OUOMValue, value); }
        } 
private  System.String NOMINAL_INDValue; 
 public System.String NOMINAL_IND
        {  
            get  
            {  
                return this.NOMINAL_INDValue;  
            }  

          set { SetProperty(ref  NOMINAL_INDValue, value); }
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
private  System.Decimal RCRD_CHANNEL_COUNTValue; 
 public System.Decimal RCRD_CHANNEL_COUNT
        {  
            get  
            {  
                return this.RCRD_CHANNEL_COUNTValue;  
            }  

          set { SetProperty(ref  RCRD_CHANNEL_COUNTValue, value); }
        } 
private  System.String RCRD_FORMAT_TYPEValue; 
 public System.String RCRD_FORMAT_TYPE
        {  
            get  
            {  
                return this.RCRD_FORMAT_TYPEValue;  
            }  

          set { SetProperty(ref  RCRD_FORMAT_TYPEValue, value); }
        } 
private  System.String RCRD_GAIN_MODEValue; 
 public System.String RCRD_GAIN_MODE
        {  
            get  
            {  
                return this.RCRD_GAIN_MODEValue;  
            }  

          set { SetProperty(ref  RCRD_GAIN_MODEValue, value); }
        } 
private  System.Decimal RCRD_HF_FREQValue; 
 public System.Decimal RCRD_HF_FREQ
        {  
            get  
            {  
                return this.RCRD_HF_FREQValue;  
            }  

          set { SetProperty(ref  RCRD_HF_FREQValue, value); }
        } 
private  System.Decimal RCRD_HF_SLOPEValue; 
 public System.Decimal RCRD_HF_SLOPE
        {  
            get  
            {  
                return this.RCRD_HF_SLOPEValue;  
            }  

          set { SetProperty(ref  RCRD_HF_SLOPEValue, value); }
        } 
private  System.Decimal RCRD_LF_FREQValue; 
 public System.Decimal RCRD_LF_FREQ
        {  
            get  
            {  
                return this.RCRD_LF_FREQValue;  
            }  

          set { SetProperty(ref  RCRD_LF_FREQValue, value); }
        } 
private  System.Decimal RCRD_LF_SLOPEValue; 
 public System.Decimal RCRD_LF_SLOPE
        {  
            get  
            {  
                return this.RCRD_LF_SLOPEValue;  
            }  

          set { SetProperty(ref  RCRD_LF_SLOPEValue, value); }
        } 
private  System.String RCRD_MAKEValue; 
 public System.String RCRD_MAKE
        {  
            get  
            {  
                return this.RCRD_MAKEValue;  
            }  

          set { SetProperty(ref  RCRD_MAKEValue, value); }
        } 
private  System.String RCRD_NEAR_SURF_CORRValue; 
 public System.String RCRD_NEAR_SURF_CORR
        {  
            get  
            {  
                return this.RCRD_NEAR_SURF_CORRValue;  
            }  

          set { SetProperty(ref  RCRD_NEAR_SURF_CORRValue, value); }
        } 
private  System.String RCRD_NEAR_SURF_CORR_OUOMValue; 
 public System.String RCRD_NEAR_SURF_CORR_OUOM
        {  
            get  
            {  
                return this.RCRD_NEAR_SURF_CORR_OUOMValue;  
            }  

          set { SetProperty(ref  RCRD_NEAR_SURF_CORR_OUOMValue, value); }
        } 
private  System.Decimal RCRD_NF_FREQValue; 
 public System.Decimal RCRD_NF_FREQ
        {  
            get  
            {  
                return this.RCRD_NF_FREQValue;  
            }  

          set { SetProperty(ref  RCRD_NF_FREQValue, value); }
        } 
private  System.String RCRD_NF_INDValue; 
 public System.String RCRD_NF_IND
        {  
            get  
            {  
                return this.RCRD_NF_INDValue;  
            }  

          set { SetProperty(ref  RCRD_NF_INDValue, value); }
        } 
private  System.String RCRD_POLARITYValue; 
 public System.String RCRD_POLARITY
        {  
            get  
            {  
                return this.RCRD_POLARITYValue;  
            }  

          set { SetProperty(ref  RCRD_POLARITYValue, value); }
        } 
private  System.Decimal RCRD_REC_LENGTHValue; 
 public System.Decimal RCRD_REC_LENGTH
        {  
            get  
            {  
                return this.RCRD_REC_LENGTHValue;  
            }  

          set { SetProperty(ref  RCRD_REC_LENGTHValue, value); }
        } 
private  System.String RCRD_REC_LENGTH_OUOMValue; 
 public System.String RCRD_REC_LENGTH_OUOM
        {  
            get  
            {  
                return this.RCRD_REC_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  RCRD_REC_LENGTH_OUOMValue, value); }
        } 
private  System.Decimal RCRD_SAMPLE_RATEValue; 
 public System.Decimal RCRD_SAMPLE_RATE
        {  
            get  
            {  
                return this.RCRD_SAMPLE_RATEValue;  
            }  

          set { SetProperty(ref  RCRD_SAMPLE_RATEValue, value); }
        } 
private  System.String RCRD_SAMPLE_RATE_OUOMValue; 
 public System.String RCRD_SAMPLE_RATE_OUOM
        {  
            get  
            {  
                return this.RCRD_SAMPLE_RATE_OUOMValue;  
            }  

          set { SetProperty(ref  RCRD_SAMPLE_RATE_OUOMValue, value); }
        } 
private  System.Decimal RCVR_LINE_SPACINGValue; 
 public System.Decimal RCVR_LINE_SPACING
        {  
            get  
            {  
                return this.RCVR_LINE_SPACINGValue;  
            }  

          set { SetProperty(ref  RCVR_LINE_SPACINGValue, value); }
        } 
private  System.String RCVR_LINE_SPACING_OUOMValue; 
 public System.String RCVR_LINE_SPACING_OUOM
        {  
            get  
            {  
                return this.RCVR_LINE_SPACING_OUOMValue;  
            }  

          set { SetProperty(ref  RCVR_LINE_SPACING_OUOMValue, value); }
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
private  System.Decimal REP_WATER_ACOUSTIC_VELValue; 
 public System.Decimal REP_WATER_ACOUSTIC_VEL
        {  
            get  
            {  
                return this.REP_WATER_ACOUSTIC_VELValue;  
            }  

          set { SetProperty(ref  REP_WATER_ACOUSTIC_VELValue, value); }
        } 
private  System.String REP_WATER_ACOUSTIC_VEL_OUOMValue; 
 public System.String REP_WATER_ACOUSTIC_VEL_OUOM
        {  
            get  
            {  
                return this.REP_WATER_ACOUSTIC_VEL_OUOMValue;  
            }  

          set { SetProperty(ref  REP_WATER_ACOUSTIC_VEL_OUOMValue, value); }
        } 
private  System.String SHOT_BYValue; 
 public System.String SHOT_BY
        {  
            get  
            {  
                return this.SHOT_BYValue;  
            }  

          set { SetProperty(ref  SHOT_BYValue, value); }
        } 
private  System.String SHOT_FORValue; 
 public System.String SHOT_FOR
        {  
            get  
            {  
                return this.SHOT_FORValue;  
            }  

          set { SetProperty(ref  SHOT_FORValue, value); }
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
private  System.Decimal WELL_SRC_AZIMUTHValue; 
 public System.Decimal WELL_SRC_AZIMUTH
        {  
            get  
            {  
                return this.WELL_SRC_AZIMUTHValue;  
            }  

          set { SetProperty(ref  WELL_SRC_AZIMUTHValue, value); }
        } 
private  System.String WELL_SRC_AZIMUTH_NORTHValue; 
 public System.String WELL_SRC_AZIMUTH_NORTH
        {  
            get  
            {  
                return this.WELL_SRC_AZIMUTH_NORTHValue;  
            }  

          set { SetProperty(ref  WELL_SRC_AZIMUTH_NORTHValue, value); }
        } 
private  System.Decimal WELL_SRC_OFFSETValue; 
 public System.Decimal WELL_SRC_OFFSET
        {  
            get  
            {  
                return this.WELL_SRC_OFFSETValue;  
            }  

          set { SetProperty(ref  WELL_SRC_OFFSETValue, value); }
        } 
private  System.String WELL_SRC_OFFSET_OUOMValue; 
 public System.String WELL_SRC_OFFSET_OUOM
        {  
            get  
            {  
                return this.WELL_SRC_OFFSET_OUOMValue;  
            }  

          set { SetProperty(ref  WELL_SRC_OFFSET_OUOMValue, value); }
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


    public SEIS_ACQTN_DESIGN () { }

  }
}

