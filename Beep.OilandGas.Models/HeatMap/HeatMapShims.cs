//using System;

//namespace Beep.OilandGas.Models.HeatMap
//{
//    // Compatibility shim types to satisfy HeatMap project's references to Beep.OilandGas.Models.HeatMap
//    // These inherit from the Models.Data or HeatMap.Configuration types so code can compile without major refactors.

//    public class HEAT_MAP_DATA_POINT : Beep.OilandGas.Models.Data.HeatMap.HEAT_MAP_DATA_POINT
//    {
//        public HEAT_MAP_DATA_POINT() : base() { }
//        public HEAT_MAP_DATA_POINT(double originalX, double originalY, double value, string? label = null)
//            : base(originalX, originalY, value, label) { }
//    }

//    public class HeatMapResult : Beep.OilandGas.Models.Data.HeatMap.HeatMapResult { }
//    public class HeatMapConfigurationRecord : Beep.OilandGas.Models.Data.HeatMap.HeatMapConfigurationRecord { }

//    // Map HEAT_MAP_CONFIGURATION used by HeatMap service to the concrete configuration in HeatMap.Configuration namespace.
//    public class HEAT_MAP_CONFIGURATION : Beep.OilandGas.HeatMap.Configuration.HEAT_MAP_CONFIGURATION
//    {
//        public HEAT_MAP_CONFIGURATION() : base() { }
//    }
//}
