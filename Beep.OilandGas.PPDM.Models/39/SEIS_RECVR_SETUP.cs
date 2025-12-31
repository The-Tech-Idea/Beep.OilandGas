using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_RECVR_SETUP: Entity,IPPDMEntity

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
private  System.String RCVR_SETUP_IDValue; 
 public System.String RCVR_SETUP_ID
        {  
            get  
            {  
                return this.RCVR_SETUP_IDValue;  
            }  

          set { SetProperty(ref  RCVR_SETUP_IDValue, value); }
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
private  System.Decimal AVG_FEATHERING_ANGLEValue; 
 public System.Decimal AVG_FEATHERING_ANGLE
        {  
            get  
            {  
                return this.AVG_FEATHERING_ANGLEValue;  
            }  

          set { SetProperty(ref  AVG_FEATHERING_ANGLEValue, value); }
        } 
private  System.Decimal AVG_STREAMER_DEPTHValue; 
 public System.Decimal AVG_STREAMER_DEPTH
        {  
            get  
            {  
                return this.AVG_STREAMER_DEPTHValue;  
            }  

          set { SetProperty(ref  AVG_STREAMER_DEPTHValue, value); }
        } 
private  System.String AVG_STREAMER_DEPTH_OUOMValue; 
 public System.String AVG_STREAMER_DEPTH_OUOM
        {  
            get  
            {  
                return this.AVG_STREAMER_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  AVG_STREAMER_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal BASE_FREQValue; 
 public System.Decimal BASE_FREQ
        {  
            get  
            {  
                return this.BASE_FREQValue;  
            }  

          set { SetProperty(ref  BASE_FREQValue, value); }
        } 
private  System.String DEPTH_CONTROLLERValue; 
 public System.String DEPTH_CONTROLLER
        {  
            get  
            {  
                return this.DEPTH_CONTROLLERValue;  
            }  

          set { SetProperty(ref  DEPTH_CONTROLLERValue, value); }
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
private  System.String FIXED_INDValue; 
 public System.String FIXED_IND
        {  
            get  
            {  
                return this.FIXED_INDValue;  
            }  

          set { SetProperty(ref  FIXED_INDValue, value); }
        } 
private  System.Decimal GROUP_SPACINGValue; 
 public System.Decimal GROUP_SPACING
        {  
            get  
            {  
                return this.GROUP_SPACINGValue;  
            }  

          set { SetProperty(ref  GROUP_SPACINGValue, value); }
        } 
private  System.String GROUP_SPACING_OUOMValue; 
 public System.String GROUP_SPACING_OUOM
        {  
            get  
            {  
                return this.GROUP_SPACING_OUOMValue;  
            }  

          set { SetProperty(ref  GROUP_SPACING_OUOMValue, value); }
        } 
private  System.Decimal INLINE_OFFSETValue; 
 public System.Decimal INLINE_OFFSET
        {  
            get  
            {  
                return this.INLINE_OFFSETValue;  
            }  

          set { SetProperty(ref  INLINE_OFFSETValue, value); }
        } 
private  System.String INLINE_OFFSET_DIRECTIONValue; 
 public System.String INLINE_OFFSET_DIRECTION
        {  
            get  
            {  
                return this.INLINE_OFFSET_DIRECTIONValue;  
            }  

          set { SetProperty(ref  INLINE_OFFSET_DIRECTIONValue, value); }
        } 
private  System.Decimal OFFLINE_OFFSETValue; 
 public System.Decimal OFFLINE_OFFSET
        {  
            get  
            {  
                return this.OFFLINE_OFFSETValue;  
            }  

          set { SetProperty(ref  OFFLINE_OFFSETValue, value); }
        } 
private  System.String OFFLINE_OFFSET_DIRECTIONValue; 
 public System.String OFFLINE_OFFSET_DIRECTION
        {  
            get  
            {  
                return this.OFFLINE_OFFSET_DIRECTIONValue;  
            }  

          set { SetProperty(ref  OFFLINE_OFFSET_DIRECTIONValue, value); }
        } 
private  System.String OFFSET_OUOMValue; 
 public System.String OFFSET_OUOM
        {  
            get  
            {  
                return this.OFFSET_OUOMValue;  
            }  

          set { SetProperty(ref  OFFSET_OUOMValue, value); }
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
private  System.String RCVR_ARRAY_TYPEValue; 
 public System.String RCVR_ARRAY_TYPE
        {  
            get  
            {  
                return this.RCVR_ARRAY_TYPEValue;  
            }  

          set { SetProperty(ref  RCVR_ARRAY_TYPEValue, value); }
        } 
private  System.String RCVR_MAKEValue; 
 public System.String RCVR_MAKE
        {  
            get  
            {  
                return this.RCVR_MAKEValue;  
            }  

          set { SetProperty(ref  RCVR_MAKEValue, value); }
        } 
private  System.Decimal RCVR_PHONE_COUNTValue; 
 public System.Decimal RCVR_PHONE_COUNT
        {  
            get  
            {  
                return this.RCVR_PHONE_COUNTValue;  
            }  

          set { SetProperty(ref  RCVR_PHONE_COUNTValue, value); }
        } 
private  System.Decimal RCVR_SPACINGValue; 
 public System.Decimal RCVR_SPACING
        {  
            get  
            {  
                return this.RCVR_SPACINGValue;  
            }  

          set { SetProperty(ref  RCVR_SPACINGValue, value); }
        } 
private  System.String RCVR_SPACING_OUOMValue; 
 public System.String RCVR_SPACING_OUOM
        {  
            get  
            {  
                return this.RCVR_SPACING_OUOMValue;  
            }  

          set { SetProperty(ref  RCVR_SPACING_OUOMValue, value); }
        } 
private  System.String RCVR_TYPEValue; 
 public System.String RCVR_TYPE
        {  
            get  
            {  
                return this.RCVR_TYPEValue;  
            }  

          set { SetProperty(ref  RCVR_TYPEValue, value); }
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
private  System.String SPREAD_DESCRIPTIONValue; 
 public System.String SPREAD_DESCRIPTION
        {  
            get  
            {  
                return this.SPREAD_DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  SPREAD_DESCRIPTIONValue, value); }
        } 
private  System.String SPREAD_DESCRIPTION_OUOMValue; 
 public System.String SPREAD_DESCRIPTION_OUOM
        {  
            get  
            {  
                return this.SPREAD_DESCRIPTION_OUOMValue;  
            }  

          set { SetProperty(ref  SPREAD_DESCRIPTION_OUOMValue, value); }
        } 
private  System.Decimal STREAMER_COUNTValue; 
 public System.Decimal STREAMER_COUNT
        {  
            get  
            {  
                return this.STREAMER_COUNTValue;  
            }  

          set { SetProperty(ref  STREAMER_COUNTValue, value); }
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


    public SEIS_RECVR_SETUP () { }

  }
}

