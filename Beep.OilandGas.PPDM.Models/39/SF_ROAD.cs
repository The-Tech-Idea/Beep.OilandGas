using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SF_ROAD: Entity,IPPDMEntity

{

private  System.String SF_SUBTYPEValue; 
 public System.String SF_SUBTYPE
        {  
            get  
            {  
                return this.SF_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SF_SUBTYPEValue, value); }
        } 
private  System.String ROAD_IDValue; 
 public System.String ROAD_ID
        {  
            get  
            {  
                return this.ROAD_IDValue;  
            }  

          set { SetProperty(ref  ROAD_IDValue, value); }
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
private  System.Decimal CAPACITYValue; 
 public System.Decimal CAPACITY
        {  
            get  
            {  
                return this.CAPACITYValue;  
            }  

          set { SetProperty(ref  CAPACITYValue, value); }
        } 
private  System.String CAPACITY_OUOMValue; 
 public System.String CAPACITY_OUOM
        {  
            get  
            {  
                return this.CAPACITY_OUOMValue;  
            }  

          set { SetProperty(ref  CAPACITY_OUOMValue, value); }
        } 
private  System.Decimal COMMUNICATION_FREQValue; 
 public System.Decimal COMMUNICATION_FREQ
        {  
            get  
            {  
                return this.COMMUNICATION_FREQValue;  
            }  

          set { SetProperty(ref  COMMUNICATION_FREQValue, value); }
        } 
private  System.String COMMUNICATION_FREQ_DESCValue; 
 public System.String COMMUNICATION_FREQ_DESC
        {  
            get  
            {  
                return this.COMMUNICATION_FREQ_DESCValue;  
            }  

          set { SetProperty(ref  COMMUNICATION_FREQ_DESCValue, value); }
        } 
private  System.String CONTROL_TYPEValue; 
 public System.String CONTROL_TYPE
        {  
            get  
            {  
                return this.CONTROL_TYPEValue;  
            }  

          set { SetProperty(ref  CONTROL_TYPEValue, value); }
        } 
private  System.String CURRENT_OPERATOR_BA_IDValue; 
 public System.String CURRENT_OPERATOR_BA_ID
        {  
            get  
            {  
                return this.CURRENT_OPERATOR_BA_IDValue;  
            }  

          set { SetProperty(ref  CURRENT_OPERATOR_BA_IDValue, value); }
        } 
private  System.String DIRECTIONValue; 
 public System.String DIRECTION
        {  
            get  
            {  
                return this.DIRECTIONValue;  
            }  

          set { SetProperty(ref  DIRECTIONValue, value); }
        } 
private  System.String DRIVING_SIDEValue; 
 public System.String DRIVING_SIDE
        {  
            get  
            {  
                return this.DRIVING_SIDEValue;  
            }  

          set { SetProperty(ref  DRIVING_SIDEValue, value); }
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
private  System.Decimal ROAD_LENGTHValue; 
 public System.Decimal ROAD_LENGTH
        {  
            get  
            {  
                return this.ROAD_LENGTHValue;  
            }  

          set { SetProperty(ref  ROAD_LENGTHValue, value); }
        } 
private  System.String ROAD_LENGTH_OUOMValue; 
 public System.String ROAD_LENGTH_OUOM
        {  
            get  
            {  
                return this.ROAD_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  ROAD_LENGTH_OUOMValue, value); }
        } 
private  System.String ROAD_TYPEValue; 
 public System.String ROAD_TYPE
        {  
            get  
            {  
                return this.ROAD_TYPEValue;  
            }  

          set { SetProperty(ref  ROAD_TYPEValue, value); }
        } 
private  System.Decimal ROAD_WIDTHValue; 
 public System.Decimal ROAD_WIDTH
        {  
            get  
            {  
                return this.ROAD_WIDTHValue;  
            }  

          set { SetProperty(ref  ROAD_WIDTHValue, value); }
        } 
private  System.String ROAD_WIDTH_OUOMValue; 
 public System.String ROAD_WIDTH_OUOM
        {  
            get  
            {  
                return this.ROAD_WIDTH_OUOMValue;  
            }  

          set { SetProperty(ref  ROAD_WIDTH_OUOMValue, value); }
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
private  System.String SURFACE_TYPEValue; 
 public System.String SURFACE_TYPE
        {  
            get  
            {  
                return this.SURFACE_TYPEValue;  
            }  

          set { SetProperty(ref  SURFACE_TYPEValue, value); }
        } 
private  System.String TRAFFIC_FLOW_TYPEValue; 
 public System.String TRAFFIC_FLOW_TYPE
        {  
            get  
            {  
                return this.TRAFFIC_FLOW_TYPEValue;  
            }  

          set { SetProperty(ref  TRAFFIC_FLOW_TYPEValue, value); }
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


    public SF_ROAD () { }

  }
}

