using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;


 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class STRAT_WELL_SECTION: Entity,IPPDMEntity

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
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
        } 
private  System.String AREA_IDValue; 
 public System.String AREA_ID
        {  
            get  
            {  
                return this.AREA_IDValue;  
            }  

          set { SetProperty(ref  AREA_IDValue, value); }
        } 
private  System.String AREA_TYPEValue; 
 public System.String AREA_TYPE
        {  
            get  
            {  
                return this.AREA_TYPEValue;  
            }  

          set { SetProperty(ref  AREA_TYPEValue, value); }
        } 
private  System.String AZIMUTH_NORTH_TYPEValue; 
 public System.String AZIMUTH_NORTH_TYPE
        {  
            get  
            {  
                return this.AZIMUTH_NORTH_TYPEValue;  
            }  

          set { SetProperty(ref  AZIMUTH_NORTH_TYPEValue, value); }
        } 
private  System.String CERTIFIED_INDValue; 
 public System.String CERTIFIED_IND
        {  
            get  
            {  
                return this.CERTIFIED_INDValue;  
            }  

          set { SetProperty(ref  CERTIFIED_INDValue, value); }
        } 
private  System.String CONFORMITY_RELATIONSHIPValue; 
 public System.String CONFORMITY_RELATIONSHIP
        {  
            get  
            {  
                return this.CONFORMITY_RELATIONSHIPValue;  
            }  

          set { SetProperty(ref  CONFORMITY_RELATIONSHIPValue, value); }
        } 
private  System.Decimal DIP_ANGLEValue; 
 public System.Decimal DIP_ANGLE
        {  
            get  
            {  
                return this.DIP_ANGLEValue;  
            }  

          set { SetProperty(ref  DIP_ANGLEValue, value); }
        } 
private  System.String DIP_DIRECTIONValue; 
 public System.String DIP_DIRECTION
        {  
            get  
            {  
                return this.DIP_DIRECTIONValue;  
            }  

          set { SetProperty(ref  DIP_DIRECTIONValue, value); }
        } 
private  System.String DOMINANT_LITHOLOGYValue; 
 public System.String DOMINANT_LITHOLOGY
        {  
            get  
            {  
                return this.DOMINANT_LITHOLOGYValue;  
            }  

          set { SetProperty(ref  DOMINANT_LITHOLOGYValue, value); }
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
private  System.Decimal FAULT_HEAVEValue; 
 public System.Decimal FAULT_HEAVE
        {  
            get  
            {  
                return this.FAULT_HEAVEValue;  
            }  

          set { SetProperty(ref  FAULT_HEAVEValue, value); }
        } 
private  System.Decimal FAULT_THROWValue; 
 public System.Decimal FAULT_THROW
        {  
            get  
            {  
                return this.FAULT_THROWValue;  
            }  

          set { SetProperty(ref  FAULT_THROWValue, value); }
        } 
private  System.String INTERPRETERValue; 
 public System.String INTERPRETER
        {  
            get  
            {  
                return this.INTERPRETERValue;  
            }  

          set { SetProperty(ref  INTERPRETERValue, value); }
        } 
private  System.Decimal MISSING_SECTIONValue; 
 public System.Decimal MISSING_SECTION
        {  
            get  
            {  
                return this.MISSING_SECTIONValue;  
            }  

          set { SetProperty(ref  MISSING_SECTIONValue, value); }
        } 
private  System.String MISSING_STRAT_TYPEValue; 
 public System.String MISSING_STRAT_TYPE
        {  
            get  
            {  
                return this.MISSING_STRAT_TYPEValue;  
            }  

          set { SetProperty(ref  MISSING_STRAT_TYPEValue, value); }
        } 
private  System.Decimal ORDINAL_SEQ_NOValue; 
 public System.Decimal ORDINAL_SEQ_NO
        {  
            get  
            {  
                return this.ORDINAL_SEQ_NOValue;  
            }  

          set { SetProperty(ref  ORDINAL_SEQ_NOValue, value); }
        } 
private  System.String OVERTURNED_INDValue; 
 public System.String OVERTURNED_IND
        {  
            get  
            {  
                return this.OVERTURNED_INDValue;  
            }  

          set { SetProperty(ref  OVERTURNED_INDValue, value); }
        } 
private  System.DateTime? PICK_DATEValue; 
 public System.DateTime? PICK_DATE
        {  
            get  
            {  
                return this.PICK_DATEValue;  
            }  

          set { SetProperty(ref  PICK_DATEValue, value); }
        } 
private  System.Decimal PICK_DEPTHValue; 
 public System.Decimal PICK_DEPTH
        {  
            get  
            {  
                return this.PICK_DEPTHValue;  
            }  

          set { SetProperty(ref  PICK_DEPTHValue, value); }
        } 
private  System.String PICK_DEPTH_OUOMValue; 
 public System.String PICK_DEPTH_OUOM
        {  
            get  
            {  
                return this.PICK_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  PICK_DEPTH_OUOMValue, value); }
        } 
private  System.String PICK_LOCATIONValue; 
 public System.String PICK_LOCATION
        {  
            get  
            {  
                return this.PICK_LOCATIONValue;  
            }  

          set { SetProperty(ref  PICK_LOCATIONValue, value); }
        } 
private  System.String PICK_QUALIFIERValue; 
 public System.String PICK_QUALIFIER
        {  
            get  
            {  
                return this.PICK_QUALIFIERValue;  
            }  

          set { SetProperty(ref  PICK_QUALIFIERValue, value); }
        } 
private  System.String PICK_QUALIF_REASONValue; 
 public System.String PICK_QUALIF_REASON
        {  
            get  
            {  
                return this.PICK_QUALIF_REASONValue;  
            }  

          set { SetProperty(ref  PICK_QUALIF_REASONValue, value); }
        } 
private  System.String PICK_QUALITYValue; 
 public System.String PICK_QUALITY
        {  
            get  
            {  
                return this.PICK_QUALITYValue;  
            }  

          set { SetProperty(ref  PICK_QUALITYValue, value); }
        } 
private  System.Decimal PICK_TVDValue; 
 public System.Decimal PICK_TVD
        {  
            get  
            {  
                return this.PICK_TVDValue;  
            }  

          set { SetProperty(ref  PICK_TVDValue, value); }
        } 
private  System.String PICK_VERSION_TYPEValue; 
 public System.String PICK_VERSION_TYPE
        {  
            get  
            {  
                return this.PICK_VERSION_TYPEValue;  
            }  

          set { SetProperty(ref  PICK_VERSION_TYPEValue, value); }
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
private  System.String PREFERRED_PICK_INDValue; 
 public System.String PREFERRED_PICK_IND
        {  
            get  
            {  
                return this.PREFERRED_PICK_INDValue;  
            }  

          set { SetProperty(ref  PREFERRED_PICK_INDValue, value); }
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
private  System.Decimal REPEAT_SECTIONValue; 
 public System.Decimal REPEAT_SECTION
        {  
            get  
            {  
                return this.REPEAT_SECTIONValue;  
            }  

          set { SetProperty(ref  REPEAT_SECTIONValue, value); }
        } 
private  System.Decimal REPEAT_STRAT_OCCUR_NOValue; 
 public System.Decimal REPEAT_STRAT_OCCUR_NO
        {  
            get  
            {  
                return this.REPEAT_STRAT_OCCUR_NOValue;  
            }  

          set { SetProperty(ref  REPEAT_STRAT_OCCUR_NOValue, value); }
        } 
private  System.String REPEAT_STRAT_TYPEValue; 
 public System.String REPEAT_STRAT_TYPE
        {  
            get  
            {  
                return this.REPEAT_STRAT_TYPEValue;  
            }  

          set { SetProperty(ref  REPEAT_STRAT_TYPEValue, value); }
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
private  System.String SOURCE_DOCUMENT_IDValue; 
 public System.String SOURCE_DOCUMENT_ID
        {  
            get  
            {  
                return this.SOURCE_DOCUMENT_IDValue;  
            }  

          set { SetProperty(ref  SOURCE_DOCUMENT_IDValue, value); }
        } 
private  System.String STRAT_INTERPRET_METHODValue; 
 public System.String STRAT_INTERPRET_METHOD
        {  
            get  
            {  
                return this.STRAT_INTERPRET_METHODValue;  
            }  

          set { SetProperty(ref  STRAT_INTERPRET_METHODValue, value); }
        } 
private  System.Decimal STRIKEValue; 
 public System.Decimal STRIKE
        {  
            get  
            {  
                return this.STRIKEValue;  
            }  

          set { SetProperty(ref  STRIKEValue, value); }
        } 
private  System.String SW_APPLICATION_IDValue; 
 public System.String SW_APPLICATION_ID
        {  
            get  
            {  
                return this.SW_APPLICATION_IDValue;  
            }  

          set { SetProperty(ref  SW_APPLICATION_IDValue, value); }
        } 
private  System.String TVD_METHODValue; 
 public System.String TVD_METHOD
        {  
            get  
            {  
                return this.TVD_METHODValue;  
            }  

          set { SetProperty(ref  TVD_METHODValue, value); }
        } 
private  System.Decimal VERSION_OBS_NOValue; 
 public System.Decimal VERSION_OBS_NO
        {  
            get  
            {  
                return this.VERSION_OBS_NOValue;  
            }  

          set { SetProperty(ref  VERSION_OBS_NOValue, value); }
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
private  System.String BUSINESS_ASSOCIATE_IDValue; 
 public System.String BUSINESS_ASSOCIATE_ID
        {  
            get  
            {  
                return this.BUSINESS_ASSOCIATE_IDValue;  
            }  

          set { SetProperty(ref  BUSINESS_ASSOCIATE_IDValue, value); }
        } 


    public STRAT_WELL_SECTION () { }

  }
}

