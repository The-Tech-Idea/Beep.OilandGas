using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beep.WellSchematics
{
    public static class DefaultMapGenerator
    {
        public static List<DefaultMap> GenerateDefaultMaps()
        {
            return new List<DefaultMap>
        {
            new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "tubing anchor" } },
                SymbolFile = "tubing anchor.svg"
            },
            new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "FISH" } },
                SymbolFile = "tubing red.svg"
            },
            new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "STATOR_PCP", "stator" } },
                SymbolFile = "stator.svg"
            },
            // ... Continue adding more DefaultMap objects based on the XML structure
            // Example for "hole":
            new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "hole" } },
                SymbolFile = "hole.svg"
            },
              new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "blast joint", "BLAST_JOINT" } },
                SymbolFile = "blast joint.svg"
            },
            new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "cement" } },
                SymbolFile = "cement.svg"
            },
            new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "downhole choke" } },
                SymbolFile = "choke.svg"
            },
            new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "drill bit" } },
                SymbolFile = "drill bit.svg"
            },
            new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "drill collar" } },
                SymbolFile = "drill collar.svg"
            },
            // Add more entries as needed
            // ...
            new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "gas lift mandrel", "gas_lift_mandrel" } },
                SymbolFile = "glm.svg"
            },
            new DefaultMap
            {
                ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "gravel pack", "FISH_AND_GRAVEL_PACK" } },
                SymbolFile = "gravel pack.svg"
            },
            new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "gas lift valve", "Valve - Downhole", "valve" } },
    SymbolFile = "valve.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "entry guide" } },
    SymbolFile = "guide.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "gun", "hole in tubing string" } },
    SymbolFile = "hole_tub.svg"
},
// ... Continue for other components
// Example for "mud motor":
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "mud motor" } },
    SymbolFile = "mud motor.svg"
},
// Continue adding entries for other components
// ...
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "pup_joint" } },
    SymbolFile = "pup joint.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "landing_nipple" } },
    SymbolFile = "landing_nipple.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "DRAIN_NIPPLE_PCP" } },
    SymbolFile = "profile nipple.svg"
},
// Continue for the rest of the components
// ...
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "tubing half cut" } },
    SymbolFile = "tubing HC.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "on-off tool" } },
    SymbolFile = "on-off tool.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "reamer" } },
    SymbolFile = "reamer.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "lwd" } },
    SymbolFile = "lwd.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "PBR_SEAL" } },
    SymbolFile = "PBR_Seal.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "SNAP_LATCH" } },
    SymbolFile = "seal assembly 2.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "pup_joint" } },
    SymbolFile = "pup joint.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "landing_nipple" } },
    SymbolFile = "landing_nipple.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "DRAIN_NIPPLE_PCP" } },
    SymbolFile = "profile nipple.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "RT TO TOP OF TUBING HANGER" } },
    SymbolFile = "tubing green.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "DUAL SPLIT TUBING HANGER", "tubing hanger", "tubing_hanger" } },
    SymbolFile = "tubing hanger.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "dual tubing hanger", "dual_tubing_hanger" } },
    SymbolFile = "dual tubing hanger.svg"
},
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "bridge_plug", "bridge plug" } },
    SymbolFile = "bridge plug.svg"
},
// ... Continue for the remaining components

// Example for the last entry
new DefaultMap
{
    ComponentSpecification = new ComponentSpecification { ComponentNames = new List<string> { "GAS_SEPARATOR_PCP" } },
    SymbolFile = "gas anchor.svg"
}

            // Continue for other components...
        };
        }
    }

}
